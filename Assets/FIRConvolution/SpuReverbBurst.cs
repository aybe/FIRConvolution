using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace FIRConvolution
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public unsafe struct SpuReverbBurst
    {
        private readonly float* BufferArray;
        private readonly int BufferCount;
        private int BufferIndex;

        private readonly int dAPF1;
        private readonly int dAPF2;
        private readonly float vIIR;
        private readonly float vCOMB1;
        private readonly float vCOMB2;
        private readonly float vCOMB3;
        private readonly float vCOMB4;
        private readonly float vWALL;
        private readonly float vAPF1;
        private readonly float vAPF2;
        private readonly int mLSAME;
        private readonly int mRSAME;
        private readonly int mLCOMB1;
        private readonly int mRCOMB1;
        private readonly int mLCOMB2;
        private readonly int mRCOMB2;
        private readonly int dLSAME;
        private readonly int dRSAME;
        private readonly int mLDIFF;
        private readonly int mRDIFF;
        private readonly int mLCOMB3;
        private readonly int mRCOMB3;
        private readonly int mLCOMB4;
        private readonly int mRCOMB4;
        private readonly int dLDIFF;
        private readonly int dRDIFF;
        private readonly int mLAPF1;
        private readonly int mRAPF1;
        private readonly int mLAPF2;
        private readonly int mRAPF2;
        private readonly float vLIN;
        private readonly float vRIN;

        private readonly ref float this[int index] => ref BufferArray[BufferIndex + index & BufferCount - 1];

        public SpuReverbBurst(SpuReverbPreset preset, int sampleRate, MemoryAllocator allocator)
        {
            if (sampleRate <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, null);
            }

            const int bufferCount = 131072;

            Assert.AreEqual(0, bufferCount % 2); // for faster modulo

            BufferArray = (float*)allocator.AlignedAlloc(new float[bufferCount]);
            BufferCount = bufferCount;
            BufferIndex = 0;

            dAPF1   = GetOffset(preset.dAPF1);
            dAPF2   = GetOffset(preset.dAPF2);
            vIIR    = GetVolume(preset.vIIR);
            vCOMB1  = GetVolume(preset.vCOMB1);
            vCOMB2  = GetVolume(preset.vCOMB2);
            vCOMB3  = GetVolume(preset.vCOMB3);
            vCOMB4  = GetVolume(preset.vCOMB4);
            vWALL   = GetVolume(preset.vWALL);
            vAPF1   = GetVolume(preset.vAPF1);
            vAPF2   = GetVolume(preset.vAPF2);
            mLSAME  = GetOffset(preset.mLSAME);
            mRSAME  = GetOffset(preset.mRSAME);
            mLCOMB1 = GetOffset(preset.mLCOMB1);
            mRCOMB1 = GetOffset(preset.mRCOMB1);
            mLCOMB2 = GetOffset(preset.mLCOMB2);
            mRCOMB2 = GetOffset(preset.mRCOMB2);
            dLSAME  = GetOffset(preset.dLSAME);
            dRSAME  = GetOffset(preset.dRSAME);
            mLDIFF  = GetOffset(preset.mLDIFF);
            mRDIFF  = GetOffset(preset.mRDIFF);
            mLCOMB3 = GetOffset(preset.mLCOMB3);
            mRCOMB3 = GetOffset(preset.mRCOMB3);
            mLCOMB4 = GetOffset(preset.mLCOMB4);
            mRCOMB4 = GetOffset(preset.mRCOMB4);
            dLDIFF  = GetOffset(preset.dLDIFF);
            dRDIFF  = GetOffset(preset.dRDIFF);
            mLAPF1  = GetOffset(preset.mLAPF1);
            mRAPF1  = GetOffset(preset.mRAPF1);
            mLAPF2  = GetOffset(preset.mLAPF2);
            mRAPF2  = GetOffset(preset.mRAPF2);
            vLIN    = GetVolume(preset.vLIN);
            vRIN    = GetVolume(preset.vRIN);

            float GetCenter(short value)
            {
                const float div = 1.0f / 22050.0f;

                var dt1 = GetVolume(value);
                var rc1 = 1.0f / (2.0f * MathF.PI * (div / dt1 - div));

                var dt2 = 1.0f / sampleRate;
                var rc2 = 1.0f / (2.0f * MathF.PI * rc1);

                var ctr = dt2 / (rc2 + dt2);

                return ctr;
            }

            int GetOffset(short value)
            {
                return value * 8 * sampleRate / 22050;
            }

            static float GetVolume(short value)
            {
                return value / 32768.0f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Clamp(in float value)
        {
            return Math.Clamp(value, -1.0f, +1.0f); // todo
        }

        private void Shift(int count = 2)
        {
            BufferIndex = BufferIndex + count & BufferCount - 1;
        }

        public static void Free(ref SpuReverbBurst reverb, MemoryAllocator allocator)
        {
            allocator.AlignedFree(new IntPtr(reverb.BufferArray));

            reverb = default;
        }

        public static void Process(
            in void* pSource, in void* pFilter, in void* pOutput,
            in int length, in float dry, in float wet, ref SpuReverbBurst state)
        {
            var source = new Span<float2>(pSource, length);
            var filter = new Span<float2>(pFilter, length);
            var output = new Span<float2>(pOutput, length);
            
            var dAPF1   = state.dAPF1;
            var dAPF2   = state.dAPF2;
            var vIIR    = state.vIIR;
            var vCOMB1  = state.vCOMB1;
            var vCOMB2  = state.vCOMB2;
            var vCOMB3  = state.vCOMB3;
            var vCOMB4  = state.vCOMB4;
            var vWALL   = state.vWALL;
            var vAPF1   = state.vAPF1;
            var vAPF2   = state.vAPF2;
            var mLSAME  = state.mLSAME;
            var mRSAME  = state.mRSAME;
            var mLCOMB1 = state.mLCOMB1;
            var mRCOMB1 = state.mRCOMB1;
            var mLCOMB2 = state.mLCOMB2;
            var mRCOMB2 = state.mRCOMB2;
            var dLSAME  = state.dLSAME;
            var dRSAME  = state.dRSAME;
            var mLDIFF  = state.mLDIFF;
            var mRDIFF  = state.mRDIFF;
            var mLCOMB3 = state.mLCOMB3;
            var mRCOMB3 = state.mRCOMB3;
            var mLCOMB4 = state.mLCOMB4;
            var mRCOMB4 = state.mRCOMB4;
            var dLDIFF  = state.dLDIFF;
            var dRDIFF  = state.dRDIFF;
            var mLAPF1  = state.mLAPF1;
            var mRAPF1  = state.mRAPF1;
            var mLAPF2  = state.mLAPF2;
            var mRAPF2  = state.mRAPF2;
            var vLIN    = state.vLIN;
            var vRIN    = state.vRIN;

            for (var i = 0; i < length; i++)
            {
                var pcm = filter[i];

                var Lin = vLIN * pcm.x;
                var Rin = vRIN * pcm.y;

                state[mLSAME] = Clamp((Lin + state[dLSAME] * vWALL - state[mLSAME - 2]) * vIIR + state[mLSAME - 2]);
                state[mRSAME] = Clamp((Rin + state[dRSAME] * vWALL - state[mRSAME - 2]) * vIIR + state[mRSAME - 2]);

                state[mLDIFF] = Clamp((Lin + state[dRDIFF] * vWALL - state[mLDIFF - 2]) * vIIR + state[mLDIFF - 2]);
                state[mRDIFF] = Clamp((Rin + state[dLDIFF] * vWALL - state[mRDIFF - 2]) * vIIR + state[mRDIFF - 2]);

                var Lout = vCOMB1 * state[mLCOMB1] + vCOMB2 * state[mLCOMB2] + vCOMB3 * state[mLCOMB3] + vCOMB4 * state[mLCOMB4];
                var Rout = vCOMB1 * state[mRCOMB1] + vCOMB2 * state[mRCOMB2] + vCOMB3 * state[mRCOMB3] + vCOMB4 * state[mRCOMB4];

                Lout = Lout - vAPF1 * state[mLAPF1 - dAPF1];

                state[mLAPF1] = Clamp(Lout);

                Lout = Lout * vAPF1 + state[mLAPF1 - dAPF1];

                Rout = Rout - vAPF1 * state[mRAPF1 - dAPF1];

                state[mRAPF1] = Clamp(Rout);

                Rout = Rout * vAPF1 + state[mRAPF1 - dAPF1];

                Lout = Lout - vAPF2 * state[mLAPF2 - dAPF2];

                state[mLAPF2] = Clamp(Lout);

                Lout = Lout * vAPF2 + state[mLAPF2 - dAPF2];

                Rout = Rout - vAPF2 * state[mRAPF2 - dAPF2];

                state[mRAPF2] = Clamp(Rout);

                Rout = Rout * vAPF2 + state[mRAPF2 - dAPF2];

                var l = Clamp(Lout);
                var r = Clamp(Rout);

                output[i] = source[i] * dry + new float2(l, r) * wet;

                state.Shift();
            }
        }
    }
}