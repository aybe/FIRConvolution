namespace FIRConvolution.Tests.Extensions
{
    public class Arrays
    {
        public static TBase[] Create<TBase>(int length, Func<TBase> create)
        {
            var array = new TBase[length];

            for (var i = 0; i < length; i++)
            {
                array[i] = create();
            }

            return array;
        }
    }
}