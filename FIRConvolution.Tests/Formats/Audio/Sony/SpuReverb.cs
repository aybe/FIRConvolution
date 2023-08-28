using System.Diagnostics.CodeAnalysis;

namespace FIRConvolution.Tests.Formats.Audio.Sony
{
    public sealed class SpuReverb
    {
        public SpuReverb(SpuReverbPreset preset, int sampleRate)
        {
            Settings = new SpuReverbSettings(preset, sampleRate);
        }

        private SpuReverbBuffer<float> Buffer { get; } = new(131072);

        private SpuReverbSettings Settings { get; }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public void Process(in float sourceL, in float sourceR, out float targetL, out float targetR)
        {
            var s = Settings;

            var Lin = sourceL * s.vLIN;
            var Rin = sourceR * s.vRIN;

            var l1 = Buffer[s.mLSAME - 2];
            var r1 = Buffer[s.mRSAME - 2];

            Buffer[s.mLSAME] = Clamp((Lin + Buffer[s.dLSAME] * s.vWALL - l1) * s.vIIR + l1);
            Buffer[s.mRSAME] = Clamp((Rin + Buffer[s.dRSAME] * s.vWALL - r1) * s.vIIR + r1);

            var l2 = Buffer[s.mLDIFF - 2];
            var r2 = Buffer[s.mRDIFF - 2];

            Buffer[s.mLDIFF] = Clamp((Lin + Buffer[s.dRDIFF] * s.vWALL - l2) * s.vIIR + l2);
            Buffer[s.mRDIFF] = Clamp((Rin + Buffer[s.dLDIFF] * s.vWALL - r2) * s.vIIR + r2);

            var Lout = s.vCOMB1 * Buffer[s.mLCOMB1] +
                       s.vCOMB2 * Buffer[s.mLCOMB2] +
                       s.vCOMB3 * Buffer[s.mLCOMB3] +
                       s.vCOMB4 * Buffer[s.mLCOMB4];

            var Rout = s.vCOMB1 * Buffer[s.mRCOMB1] +
                       s.vCOMB2 * Buffer[s.mRCOMB2] +
                       s.vCOMB3 * Buffer[s.mRCOMB3] +
                       s.vCOMB4 * Buffer[s.mRCOMB4];

            Lout = Lout - s.vAPF1 * Buffer[s.mLAPF1 - s.dAPF1];
            Rout = Rout - s.vAPF1 * Buffer[s.mRAPF1 - s.dAPF1];

            Buffer[s.mLAPF1] = Clamp(Lout);
            Buffer[s.mRAPF1] = Clamp(Rout);

            Lout = Lout * s.vAPF1 + Buffer[s.mLAPF1 - s.dAPF1];
            Rout = Rout * s.vAPF1 + Buffer[s.mRAPF1 - s.dAPF1];

            Lout = Lout - s.vAPF2 * Buffer[s.mLAPF2 - s.dAPF2];
            Rout = Rout - s.vAPF2 * Buffer[s.mRAPF2 - s.dAPF2];

            Buffer[s.mLAPF2] = Clamp(Lout);
            Buffer[s.mRAPF2] = Clamp(Rout);

            Lout = Lout * s.vAPF2 + Buffer[s.mLAPF2 - s.dAPF2];
            Rout = Rout * s.vAPF2 + Buffer[s.mRAPF2 - s.dAPF2];

            targetL = Clamp(Lout);
            targetR = Clamp(Rout);

            Buffer.Advance();
        }

        private static float Clamp(in float value)
        {
            var clamp = Math.Clamp(value, -1.0f, +1.0f);

            return clamp;
        }
    }
}