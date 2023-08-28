using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using FIRConvolution.Tests.Unsorted;

namespace FIRConvolution.Tests.Formats.Audio.Sony;

public static class SpuReverbBurst
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToCompoundAssignment")]
    public static void Process(
        Span<float2> source, Span<float2> target, int length, ref SpuReverbSettings settings, ref NativeBufferNew<float> mem)
    {
        var dAPF1   = settings.dAPF1;
        var dAPF2   = settings.dAPF2;
        var vIIR    = settings.vIIR;
        var vCOMB1  = settings.vCOMB1;
        var vCOMB2  = settings.vCOMB2;
        var vCOMB3  = settings.vCOMB3;
        var vCOMB4  = settings.vCOMB4;
        var vWALL   = settings.vWALL;
        var vAPF1   = settings.vAPF1;
        var vAPF2   = settings.vAPF2;
        var mLSAME  = settings.mLSAME;
        var mRSAME  = settings.mRSAME;
        var mLCOMB1 = settings.mLCOMB1;
        var mRCOMB1 = settings.mRCOMB1;
        var mLCOMB2 = settings.mLCOMB2;
        var mRCOMB2 = settings.mRCOMB2;
        var dLSAME  = settings.dLSAME;
        var dRSAME  = settings.dRSAME;
        var mLDIFF  = settings.mLDIFF;
        var mRDIFF  = settings.mRDIFF;
        var mLCOMB3 = settings.mLCOMB3;
        var mRCOMB3 = settings.mRCOMB3;
        var mLCOMB4 = settings.mLCOMB4;
        var mRCOMB4 = settings.mRCOMB4;
        var dLDIFF  = settings.dLDIFF;
        var dRDIFF  = settings.dRDIFF;
        var mLAPF1  = settings.mLAPF1;
        var mRAPF1  = settings.mRAPF1;
        var mLAPF2  = settings.mLAPF2;
        var mRAPF2  = settings.mRAPF2;
        var vLIN    = settings.vLIN;
        var vRIN    = settings.vRIN;

        for (var i = 0; i < length; i++)
        {
            var pcm = source[i];

            var Lin = vLIN * pcm.x;
            var Rin = vRIN * pcm.y;

            mem[mLSAME] = Clamp((Lin + mem[dLSAME] * vWALL - mem[mLSAME - 2]) * vIIR + mem[mLSAME - 2]);
            mem[mRSAME] = Clamp((Rin + mem[dRSAME] * vWALL - mem[mRSAME - 2]) * vIIR + mem[mRSAME - 2]);
            
            mem[mLDIFF] = Clamp((Lin + mem[dRDIFF] * vWALL - mem[mLDIFF - 2]) * vIIR + mem[mLDIFF - 2]);
            mem[mRDIFF] = Clamp((Rin + mem[dLDIFF] * vWALL - mem[mRDIFF - 2]) * vIIR + mem[mRDIFF - 2]);

            var Lout = vCOMB1 * mem[mLCOMB1] + vCOMB2 * mem[mLCOMB2] + vCOMB3 * mem[mLCOMB3] + vCOMB4 * mem[mLCOMB4];
            var Rout = vCOMB1 * mem[mRCOMB1] + vCOMB2 * mem[mRCOMB2] + vCOMB3 * mem[mRCOMB3] + vCOMB4 * mem[mRCOMB4];

            Lout        = Lout - vAPF1 * mem[mLAPF1 - dAPF1];
            mem[mLAPF1] = Clamp(Lout);
            Lout        = Lout * vAPF1 + mem[mLAPF1 - dAPF1];
            Rout        = Rout - vAPF1 * mem[mRAPF1 - dAPF1];
            mem[mRAPF1] = Clamp(Rout);
            Rout        = Rout * vAPF1 + mem[mRAPF1 - dAPF1];

            Lout        = Lout - vAPF2 * mem[mLAPF2 - dAPF2];
            mem[mLAPF2] = Clamp(Lout);
            Lout        = Lout * vAPF2 + mem[mLAPF2 - dAPF2];
            Rout        = Rout - vAPF2 * mem[mRAPF2 - dAPF2];
            mem[mRAPF2] = Clamp(Rout);
            Rout        = Rout * vAPF2 + mem[mRAPF2 - dAPF2];

            var l = Clamp(Lout);
            var r = Clamp(Rout);

            target[i] = new float2(l, r);

            mem.Advance();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static float Clamp(in float value)
    {
        return Math.Clamp(value, -1.0f, +1.0f);
    }
}