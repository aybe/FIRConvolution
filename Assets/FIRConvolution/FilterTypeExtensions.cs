using System;

namespace FIRConvolution
{
    public static unsafe class FilterTypeExtensions
    {
        public static void GetHandlers(this FilterType filterType, out FilterCreateHandler create, out FilterMethodHandler method)
        {
            create = filterType switch
            {
                FilterType.ScalarFullBand                   => Filter.CreateScalarFullBand,
                FilterType.ScalarHalfBandFullLoop           => Filter.CreateScalarHalfBandFullLoop,
                FilterType.ScalarHalfBandHalfLoop           => Filter.CreateScalarHalfBandHalfLoop,
                FilterType.VectorFullBandInner              => Filter.CreateVectorFullBandInner,
                FilterType.VectorFullBandOuter              => Filter.CreateVectorFullBandOuter,
                FilterType.VectorFullBandOuterInner         => Filter.CreateVectorFullBandOuterInner,
                FilterType.VectorHalfBandFullLoopInner      => Filter.CreateVectorHalfBandFullLoopInner,
                FilterType.VectorHalfBandFullLoopOuter      => Filter.CreateVectorHalfBandFullLoopOuter,
                FilterType.VectorHalfBandFullLoopOuterInner => Filter.CreateVectorHalfBandFullLoopOuterInner,
                FilterType.VectorHalfBandHalfLoopInner      => Filter.CreateVectorHalfBandHalfLoopInner,
                FilterType.VectorHalfBandHalfLoopOuter      => Filter.CreateVectorHalfBandHalfLoopOuter,
                FilterType.VectorHalfBandHalfLoopOuterInner => Filter.CreateVectorHalfBandHalfLoopOuterInner,
                _                                           => throw new ArgumentOutOfRangeException(nameof(filterType))
            };

            method = filterType switch
            {
                FilterType.ScalarFullBand                   => Filter.ProcessScalarFullBand,
                FilterType.ScalarHalfBandFullLoop           => Filter.ProcessScalarHalfBandFullLoop,
                FilterType.ScalarHalfBandHalfLoop           => Filter.ProcessScalarHalfBandHalfLoop,
                FilterType.VectorFullBandInner              => Filter.ProcessVectorFullBandInner,
                FilterType.VectorFullBandOuter              => Filter.ProcessVectorFullBandOuter,
                FilterType.VectorFullBandOuterInner         => Filter.ProcessVectorFullBandOuterInner,
                FilterType.VectorHalfBandFullLoopInner      => Filter.ProcessVectorHalfBandFullLoopInner,
                FilterType.VectorHalfBandFullLoopOuter      => Filter.ProcessVectorHalfBandFullLoopOuter,
                FilterType.VectorHalfBandFullLoopOuterInner => Filter.ProcessVectorHalfBandFullLoopOuterInner,
                FilterType.VectorHalfBandHalfLoopInner      => Filter.ProcessVectorHalfBandHalfLoopInner,
                FilterType.VectorHalfBandHalfLoopOuter      => Filter.ProcessVectorHalfBandHalfLoopOuter,
                FilterType.VectorHalfBandHalfLoopOuterInner => Filter.ProcessVectorHalfBandHalfLoopOuterInner,
                _                                           => throw new ArgumentOutOfRangeException(nameof(filterType))
            };
        }
    }
}