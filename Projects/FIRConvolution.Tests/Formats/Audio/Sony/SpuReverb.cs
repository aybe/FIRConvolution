using System.Diagnostics.CodeAnalysis;

namespace FIRConvolution.Tests.Formats.Audio.Sony;

public sealed class SpuReverb
{
    public SpuReverb(SpuReverbPreset preset, int sampleRate)
    {
        Settings = new SpuReverbSettings(preset, sampleRate);
    }

    private SpuReverbBuffer<float> Buffer { get; } = new(131072);

    private SpuReverbSettings Settings { get; }

    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToCompoundAssignment")]
    public void Process(in float sourceL, in float sourceR, out float targetL, out float targetR)
    {
        var dAPF1   = Settings.dAPF1;
        var dAPF2   = Settings.dAPF2;
        var vIIR    = Settings.vIIR;
        var vCOMB1  = Settings.vCOMB1;
        var vCOMB2  = Settings.vCOMB2;
        var vCOMB3  = Settings.vCOMB3;
        var vCOMB4  = Settings.vCOMB4;
        var vWALL   = Settings.vWALL;
        var vAPF1   = Settings.vAPF1;
        var vAPF2   = Settings.vAPF2;
        var mLSAME  = Settings.mLSAME;
        var mRSAME  = Settings.mRSAME;
        var mLCOMB1 = Settings.mLCOMB1;
        var mRCOMB1 = Settings.mRCOMB1;
        var mLCOMB2 = Settings.mLCOMB2;
        var mRCOMB2 = Settings.mRCOMB2;
        var dLSAME  = Settings.dLSAME;
        var dRSAME  = Settings.dRSAME;
        var mLDIFF  = Settings.mLDIFF;
        var mRDIFF  = Settings.mRDIFF;
        var mLCOMB3 = Settings.mLCOMB3;
        var mRCOMB3 = Settings.mRCOMB3;
        var mLCOMB4 = Settings.mLCOMB4;
        var mRCOMB4 = Settings.mRCOMB4;
        var dLDIFF  = Settings.dLDIFF;
        var dRDIFF  = Settings.dRDIFF;
        var mLAPF1  = Settings.mLAPF1;
        var mRAPF1  = Settings.mRAPF1;
        var mLAPF2  = Settings.mLAPF2;
        var mRAPF2  = Settings.mRAPF2;
        var vLIN    = Settings.vLIN;
        var vRIN    = Settings.vRIN;

        var Lin = sourceL * vLIN;
        var Rin = sourceR * vRIN;

        var l1 = Buffer[mLSAME - 2];
        var r1 = Buffer[mRSAME - 2];

        Buffer[mLSAME] = Clamp((Lin + Buffer[dLSAME] * vWALL - l1) * vIIR + l1);
        Buffer[mRSAME] = Clamp((Rin + Buffer[dRSAME] * vWALL - r1) * vIIR + r1);

        var l2 = Buffer[mLDIFF - 2];
        var r2 = Buffer[mRDIFF - 2];

        Buffer[mLDIFF] = Clamp((Lin + Buffer[dRDIFF] * vWALL - l2) * vIIR + l2);
        Buffer[mRDIFF] = Clamp((Rin + Buffer[dLDIFF] * vWALL - r2) * vIIR + r2);

        var Lout = vCOMB1 * Buffer[mLCOMB1] +
                   vCOMB2 * Buffer[mLCOMB2] +
                   vCOMB3 * Buffer[mLCOMB3] +
                   vCOMB4 * Buffer[mLCOMB4];

        var Rout = vCOMB1 * Buffer[mRCOMB1] +
                   vCOMB2 * Buffer[mRCOMB2] +
                   vCOMB3 * Buffer[mRCOMB3] +
                   vCOMB4 * Buffer[mRCOMB4];

        Lout = Lout - vAPF1 * Buffer[mLAPF1 - dAPF1];
        Rout = Rout - vAPF1 * Buffer[mRAPF1 - dAPF1];

        Buffer[mLAPF1] = Clamp(Lout);
        Buffer[mRAPF1] = Clamp(Rout);

        Lout = Lout * vAPF1 + Buffer[mLAPF1 - dAPF1];
        Rout = Rout * vAPF1 + Buffer[mRAPF1 - dAPF1];

        Lout = Lout - vAPF2 * Buffer[mLAPF2 - dAPF2];
        Rout = Rout - vAPF2 * Buffer[mRAPF2 - dAPF2];

        Buffer[mLAPF2] = Clamp(Lout);
        Buffer[mRAPF2] = Clamp(Rout);

        Lout = Lout * vAPF2 + Buffer[mLAPF2 - dAPF2];
        Rout = Rout * vAPF2 + Buffer[mRAPF2 - dAPF2];

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