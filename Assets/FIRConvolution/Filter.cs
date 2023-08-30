using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FIRConvolution
{
    // TODO make UpdateZ return zGet, delete field
// TODO make UpdateZ update offset, delete method

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public struct Filter
    {
        public Filter(IReadOnlyCollection<float> h, int vLength, int hOffset = 0)
        {
            if ((h.Count & 1) == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(h), "Expected an odd number of taps.");
            }

            if (vLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(vLength));
            }

            if (hOffset < 0 || hOffset >= h.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(hOffset));
            }

            VLength    = vLength;
            H          = h.ToArray();
            HLength    = H.Length;
            HCenter    = H.Length / 2;
            Z          = new float[(H.Length + (vLength - 1)) * 2];
            ZLength    = Z.Length;
            ZOffset    = VLength; // check UpdateZ
            ZOffsetGet = 0;
            ZOffsetSet = HLength + VLength - 1;
            HOffset    = hOffset;
            TCenter    = HCenter % 2 == 1 || HOffset == 1;
        }

        /// <summary>
        ///     The taps.
        /// </summary>
        public float[] H { get; }

        /// <summary>
        ///     The taps center index.
        /// </summary>
        public int HCenter { get; }

        /// <summary>
        ///     The taps count.
        /// </summary>
        public int HLength { get; }

        /// <summary>
        ///     The first tap offset (for half-band filtering).
        /// </summary>
        public int HOffset { get; }

        /// <summary>
        ///     The center tap process flag;
        /// </summary>
        public bool TCenter { get; } // TODO rename TCenter // TODO one could avoid branching by doing `xyz * PCenter`

        /// <summary>
        ///     The doubled delay line.
        /// </summary>
        public float[] Z { get; }

        /// <summary>
        ///     The delay line length.
        /// </summary>
        internal int ZLength { get; }

        /// <summary>
        ///     The doubled delay line state.
        /// </summary>
        internal int ZOffset { get; set; }

        /// <summary>
        ///     The delay line index to get samples.
        /// </summary>
        internal int ZOffsetGet { get; set; }

        /// <summary>
        ///     The delay line index to set samples.
        /// </summary>
        internal int ZOffsetSet { get; }

        /// <summary>
        ///     The vectorization count.
        /// </summary>
        public int VLength { get; }
    }
}