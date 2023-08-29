namespace FIRConvolution
{
    public readonly struct FilterWorkspace
    {
        public readonly Filter State;

        public readonly FilterMethod Method;

        public FilterWorkspace(Filter state, FilterMethod method)
        {
            State  = state;
            Method = method;
        }
    }
}