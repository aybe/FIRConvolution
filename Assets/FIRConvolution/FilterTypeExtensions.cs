using System;

namespace FIRConvolution
{
    public static unsafe class FilterTypeExtensions
    {
        public static void GetHandlers(this FilterType filterType, out FilterCreateHandler create, out FilterMethodHandler method)
        {
            create = filterType switch
            {
                FilterType.ScalarFull               => Filter.CreateScalarFull,
                FilterType.ScalarHalfFull           => Filter.CreateScalarHalfFull,
                FilterType.ScalarHalfHalf           => Filter.CreateScalarHalfHalf,
                FilterType.VectorFullInner          => Filter.CreateVectorFullInner,
                FilterType.VectorFullOuter          => Filter.CreateVectorFullOuter,
                FilterType.VectorFullOuterInner     => Filter.CreateVectorFullOuterInner,
                FilterType.VectorHalfFullInner      => Filter.CreateVectorHalfFullInner,
                FilterType.VectorHalfFullOuter      => Filter.CreateVectorHalfFullOuter,
                FilterType.VectorHalfFullOuterInner => Filter.CreateVectorHalfFullOuterInner,
                FilterType.VectorHalfHalfInner      => Filter.CreateVectorHalfHalfInner,
                FilterType.VectorHalfHalfOuter      => Filter.CreateVectorHalfHalfOuter,
                FilterType.VectorHalfHalfOuterInner => Filter.CreateVectorHalfHalfOuterInner,
                _                                   => throw new ArgumentOutOfRangeException(nameof(filterType))
            };

            method = filterType switch
            {
                FilterType.ScalarFull               => Filter.ProcessScalarFull,
                FilterType.ScalarHalfFull           => Filter.ProcessScalarHalfFull,
                FilterType.ScalarHalfHalf           => Filter.ProcessScalarHalfHalf,
                FilterType.VectorFullInner          => Filter.ProcessVectorFullInner,
                FilterType.VectorFullOuter          => Filter.ProcessVectorFullOuter,
                FilterType.VectorFullOuterInner     => Filter.ProcessVectorFullOuterInner,
                FilterType.VectorHalfFullInner      => Filter.ProcessVectorHalfFullInner,
                FilterType.VectorHalfFullOuter      => Filter.ProcessVectorHalfFullOuter,
                FilterType.VectorHalfFullOuterInner => Filter.ProcessVectorHalfFullOuterInner,
                FilterType.VectorHalfHalfInner      => Filter.ProcessVectorHalfHalfInner,
                FilterType.VectorHalfHalfOuter      => Filter.ProcessVectorHalfHalfOuter,
                FilterType.VectorHalfHalfOuterInner => Filter.ProcessVectorHalfHalfOuterInner,
                _                                   => throw new ArgumentOutOfRangeException(nameof(filterType))
            };
        }
    }
}