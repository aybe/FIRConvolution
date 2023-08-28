using System.Diagnostics.CodeAnalysis;

namespace FIRConvolution.Tests.Formats.Audio.Sony
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public readonly struct SpuReverbSettings
    {
        public SpuReverbSettings(SpuReverbPreset preset, int sampleRate)
        {
            if (sampleRate <= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(sampleRate), sampleRate, null);
            }

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

        public int dAPF1 { get; }

        public int dAPF2 { get; }

        public float vIIR { get; }

        public float vCOMB1 { get; }

        public float vCOMB2 { get; }

        public float vCOMB3 { get; }

        public float vCOMB4 { get; }

        public float vWALL { get; }

        public float vAPF1 { get; }

        public float vAPF2 { get; }

        public int mLSAME { get; }

        public int mRSAME { get; }

        public int mLCOMB1 { get; }

        public int mRCOMB1 { get; }

        public int mLCOMB2 { get; }

        public int mRCOMB2 { get; }

        public int dLSAME { get; }

        public int dRSAME { get; }

        public int mLDIFF { get; }

        public int mRDIFF { get; }

        public int mLCOMB3 { get; }

        public int mRCOMB3 { get; }

        public int mLCOMB4 { get; }

        public int mRCOMB4 { get; }

        public int dLDIFF { get; }

        public int dRDIFF { get; }

        public int mLAPF1 { get; }

        public int mRAPF1 { get; }

        public int mLAPF2 { get; }

        public int mRAPF2 { get; }

        public float vLIN { get; }

        public float vRIN { get; }
    }
}