using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace FIRConvolution
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [NoReorder]
    public sealed class SpuReverbFilter16
    {
        public SpuReverbFilter16(SpuReverbPreset reverb)
        {
            // note that this works "by accident" for 44100Hz
            // any other sample rate needs an adjusted preset
            // unless you're an expert in reverb, just forget

            const int step = 8 * 2 /* 44100Hz */;

            dAPF1   = step * reverb.dAPF1;
            dAPF2   = step * reverb.dAPF2;
            vIIR    = reverb.vIIR;
            vCOMB1  = reverb.vCOMB1;
            vCOMB2  = reverb.vCOMB2;
            vCOMB3  = reverb.vCOMB3;
            vCOMB4  = reverb.vCOMB4;
            vWALL   = reverb.vWALL;
            vAPF1   = reverb.vAPF1;
            vAPF2   = reverb.vAPF2;
            mLSAME  = step * reverb.mLSAME;
            mRSAME  = step * reverb.mRSAME;
            mLCOMB1 = step * reverb.mLCOMB1;
            mRCOMB1 = step * reverb.mRCOMB1;
            mLCOMB2 = step * reverb.mLCOMB2;
            mRCOMB2 = step * reverb.mRCOMB2;
            dLSAME  = step * reverb.dLSAME;
            dRSAME  = step * reverb.dRSAME;
            mLDIFF  = step * reverb.mLDIFF;
            mRDIFF  = step * reverb.mRDIFF;
            mLCOMB3 = step * reverb.mLCOMB3;
            mRCOMB3 = step * reverb.mRCOMB3;
            mLCOMB4 = step * reverb.mLCOMB4;
            mRCOMB4 = step * reverb.mRCOMB4;
            dLDIFF  = step * reverb.dLDIFF;
            dRDIFF  = step * reverb.dRDIFF;
            mLAPF1  = step * reverb.mLAPF1;
            mRAPF1  = step * reverb.mRAPF1;
            mLAPF2  = step * reverb.mLAPF2;
            mRAPF2  = step * reverb.mRAPF2;
            vLIN    = reverb.vLIN;
            vRIN    = reverb.vRIN;
        }

        private readonly int   dAPF1;
        private readonly int   dAPF2;
        private readonly short vIIR;
        private readonly short vCOMB1;
        private readonly short vCOMB2;
        private readonly short vCOMB3;
        private readonly short vCOMB4;
        private readonly short vWALL;
        private readonly short vAPF1;
        private readonly short vAPF2;
        private readonly int   mLSAME;
        private readonly int   mRSAME;
        private readonly int   mLCOMB1;
        private readonly int   mRCOMB1;
        private readonly int   mLCOMB2;
        private readonly int   mRCOMB2;
        private readonly int   dLSAME;
        private readonly int   dRSAME;
        private readonly int   mLDIFF;
        private readonly int   mRDIFF;
        private readonly int   mLCOMB3;
        private readonly int   mRCOMB3;
        private readonly int   mLCOMB4;
        private readonly int   mRCOMB4;
        private readonly int   dLDIFF;
        private readonly int   dRDIFF;
        private readonly int   mLAPF1;
        private readonly int   mRAPF1;
        private readonly int   mLAPF2;
        private readonly int   mRAPF2;
        private readonly short vLIN;
        private readonly short vRIN;
        private const    short vLOUT = short.MaxValue;
        private const    short vROUT = short.MaxValue;

        private readonly SpuReverbBuffer<int> Buffer = new(524288);

        [SuppressMessage("ReSharper", "ConvertToCompoundAssignment")]
        [SuppressMessage("Style", "IDE0054:Use compound assignment")]
        public void Process(in short sourceL, in short sourceR, out short targetL, out short targetR)
        {
            const int div = 0x8000;

            var LIn = vLIN * sourceL / div;
            var RIn = vRIN * sourceR / div;

            var L1 = Buffer[mLSAME - 1];
            var R1 = Buffer[mRSAME - 1];

            Buffer[mLSAME] = Clamp((LIn + Buffer[dLSAME] * vWALL / div - L1) * vIIR / div + L1);
            Buffer[mRSAME] = Clamp((RIn + Buffer[dRSAME] * vWALL / div - R1) * vIIR / div + R1);

            var L2 = Buffer[mLDIFF - 1];
            var R2 = Buffer[mRDIFF - 1];

            Buffer[mLDIFF] = Clamp((LIn + Buffer[dRDIFF] * vWALL / div - L2) * vIIR / div + L2);
            Buffer[mRDIFF] = Clamp((RIn + Buffer[dLDIFF] * vWALL / div - R2) * vIIR / div + R2);

            var LOut = vCOMB1 * Buffer[mLCOMB1] / div +
                       vCOMB2 * Buffer[mLCOMB2] / div +
                       vCOMB3 * Buffer[mLCOMB3] / div +
                       vCOMB4 * Buffer[mLCOMB4] / div;

            var ROut = vCOMB1 * Buffer[mRCOMB1] / div +
                       vCOMB2 * Buffer[mRCOMB2] / div +
                       vCOMB3 * Buffer[mRCOMB3] / div +
                       vCOMB4 * Buffer[mRCOMB4] / div;

            LOut = LOut - vAPF1 * Buffer[mLAPF1 - dAPF1] / div;
            ROut = ROut - vAPF1 * Buffer[mRAPF1 - dAPF1] / div;

            Buffer[mLAPF1] = Clamp(LOut);
            Buffer[mRAPF1] = Clamp(ROut);

            LOut = LOut * vAPF1 / div + Buffer[mLAPF1 - dAPF1];
            ROut = ROut * vAPF1 / div + Buffer[mRAPF1 - dAPF1];

            LOut = LOut - vAPF2 * Buffer[mLAPF2 - dAPF2] / div;
            ROut = ROut - vAPF2 * Buffer[mRAPF2 - dAPF2] / div;

            Buffer[mLAPF2] = Clamp(LOut);
            Buffer[mRAPF2] = Clamp(ROut);

            LOut = LOut * vAPF2 / div + Buffer[mLAPF2 - dAPF2];
            ROut = ROut * vAPF2 / div + Buffer[mRAPF2 - dAPF2];

            targetL = Clamp(LOut * vLOUT / div);
            targetR = Clamp(ROut * vROUT / div);

            Buffer.Advance();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short Clamp(in int value)
        {
            const short minValue = short.MinValue;
            const short maxValue = short.MaxValue;
            return value < minValue ? minValue : value > maxValue ? maxValue : (short)value;
        }
    }
}