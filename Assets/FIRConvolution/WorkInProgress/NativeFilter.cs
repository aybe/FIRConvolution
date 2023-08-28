namespace FIRConvolution.WorkInProgress
{
    public unsafe struct NativeFilter
    {
        public int CVector;

        public float* H;

        public int HLength;

        public float* Z;

        public int ZLength;

        public int ZOffset;

        public int ZOffsetGet;

        public int ZOffsetSet;

        public void ZOffsetUpdate()
        {
            ZOffset -= CVector;

            if (ZOffset < 0)
            {
                ZOffset += ZOffsetSet;
            }
        }

        public readonly override string ToString()
        {
            return $"{nameof(HLength)}: {HLength}, {nameof(ZLength)}: {ZLength}, {nameof(ZOffset)}: {ZOffset}";
        }
    }
}