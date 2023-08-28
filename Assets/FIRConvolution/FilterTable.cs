using System;
using UnityEngine;

namespace FIRConvolution
{
    public abstract class FilterTable
    {
        private static void GetTableCheckArgs(int taps, int vectors)
        {
            if (taps <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(taps));
            }

            if (vectors <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(vectors));
            }
        }

        public static int[][][] GetTableInnerInput(int taps, int vectors)
        {
            Debug.Log(nameof(GetTableInnerInput));

            GetTableCheckArgs(taps, vectors);

            var table = new int[taps + vectors][][];

            for (var i = 0; i < taps + vectors; i++)
            {
                table[i] = new int[taps][];

                for (var j = 0; j < taps; j++)
                {
                    var index = i + taps - j - 1;

                    var i0 = index - 0;
                    var i1 = index - 1;
                    var i2 = index - 2;
                    var i3 = index - 3;

                    table[i][j] = new[] { i0, i1, i2, i3 };

                    Debug.Log($"dly: {i,2}, tap: {j,2}, idx 0/1/2/3: {i0,2}, {i1,2}, {i2,2}, {i3,2}");
                }

                Debug.Log(string.Empty);
            }

            return table;
        }

        public static int[][] GetTableOuterInput(int taps, int vectors)
        {
            Debug.Log(nameof(GetTableOuterInput));

            GetTableCheckArgs(taps, vectors);

            var count = taps + vectors;

            var table = new int[count][];

            var index = 0;

            for (var i = 0; i < count; i++)
            {
                var i0 = index + 3;
                var i1 = index + 2;
                var i2 = index + 1;
                var i3 = index + 0;
                var i4 = (i0 + count) % (count * 2);
                var i5 = (i1 + count) % (count * 2);
                var i6 = (i2 + count) % (count * 2);
                var i7 = (i3 + count) % (count * 2);

                table[index] = new[] { i0, i1, i2, i3, i4, i5, i6, i7 };

                Debug.Log($"dly: {i,2}, tap: {index,2}, idx 0/1/2/3: {i0,2}, {i1,2}, {i2,2}, {i3,2}, idx 4/5/6/7: {i4,2}, {i5,2}, {i6,2}, {i7,2}");

                index -= vectors;

                if (index < 0)
                {
                    index += count;
                }
            }

            return table;
        }

        public static int[][][] GetTableOuterDelay(int taps, int vectors)
        {
            Debug.Log(nameof(GetTableOuterDelay));

            GetTableCheckArgs(taps, vectors);

            var count = taps + vectors;

            var table = new int[count][][];

            for (var i = 0; i < count; i++)
            {
                table[i] = new int[taps][];

                for (var j = 0; j < taps; j++)
                {
                    var index = i + taps - j - 1;

                    var i0 = index + 3;
                    var i1 = index + 2;
                    var i2 = index + 1;
                    var i3 = index + 0;
                    var i4 = index - 1;
                    var i5 = index - 2;
                    var i6 = index - 3;
                    var i7 = index - 4;

                    table[i][j] = new[] { i0, i1, i2, i3, i4, i5, i6, i7 };

                    Debug.Log($"dly: {i,2}, tap: {j,2}, idx 0/1/2/3: {i0,2}, {i1,2}, {i2,2}, {i3,2}");
                }
            }

            return table;
        }
    }
}