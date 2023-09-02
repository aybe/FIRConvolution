namespace FIRConvolution
{
    public unsafe delegate void FilterMethod(
        in float* source, in float* target, in int length, in int stride, in int offset, ref Filter filter);
}