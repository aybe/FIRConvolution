namespace FIRConvolution
{
    public unsafe delegate void ZFilterMethod(
        in float* source,
        in float* target,
        in int length,
        in int stride,
        in int offset,
        ref ZFilter filter);
}