using System.Numerics;
using FIRConvolution.Extensions;
using JetBrains.Annotations;

namespace FIRConvolution.Tests.Unsorted;

[NoReorder]
public partial class HalfBandFirFilter
{
    private readonly float[] H;

    private readonly float[] Z;

    private readonly int Center;

    private readonly int Length;

    private readonly int[][] Tables;

    private int Offset;

    public HalfBandFirFilter(IReadOnlyList<float> h)
    {
        if (h.Count % 2 != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(h), "Taps count must be odd.");
        }

        var length = h.Count;
        var center = length / 2;

        for (var i = 0; i < center; i++)
        {
            var i1 = i;
            var i2 = length - 1 - i;

            var f1 = h[i1];
            var f2 = h[i2];

            if (f1.Equals(f2) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(h), $"Taps {i1} and {i2} are not symmetrical.");
            }
        }

        H      = h.ToArray();
        Z      = new float[length * 2];
        Length = length;
        Center = center;
        Offset = 0;
        Tables = GetTables(length, 4);
    }

    private int GetOffset(in int index)
    {
#if DEBUG
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, null);
        }
#endif

        var value = Offset - index;

        while (value < 0)
        {
            value += Length;
        }

#if DEBUG
        Assert.IsTrue(value < Length, $"{value} < {Length}");
#endif
        return value;
    }

    public static int[][] GetTables(int coefficients, int vectorization)
    {
        var tables = new int[coefficients][];

        var length = coefficients + (vectorization * 2 - 2);

        for (var i = 0; i < coefficients; i++)
        {
            tables[i] = new int[length];
        }

        for (var i = 0; i < coefficients; i++) // offset
        {
            for (var j = 0; j < length; j++) // tap
            {
                var k = i - j; // delay

                while (k < 0)
                {
                    k += coefficients;
                }

                tables[i][j] = k;
            }
        }

        return tables;
    }

    private void CheckOffset()
    {
        Assert.IsTrue(Offset >= 0, Offset.ToString());
    }

    private void UpdateOffset()
    {
        Offset--;

        if (Offset < 0)
        {
            Offset += Length;
        }

        CheckOffset();
    }

    private void UpdateOffset(int count)
    {
        Offset -= count;

        if (Offset < 0)
        {
            Offset += Length;
        }

        CheckOffset();
    }

    public override string ToString()
    {
        return $"{nameof(Center)}: {Center}, {nameof(Length)}: {Length}, {nameof(Offset)}: {Offset}";
    }
}

public partial class HalfBandFirFilter
{
    public float SymmetricalNormal(float input)
    {
        Z[Offset] = Z[Offset + Length] = input;

        var output = 0.0f;

        for (var i = 0; i < Length; i++)
        {
            output += H[i] * Z[i + Offset];
        }

        UpdateOffset();

        return output;
    }

    public float SymmetricalHalf(float input)
    {
        var offset = Offset + Length;

        Z[Offset] = Z[offset] = input;

        var output = 0.0f;

        for (var t = 1; t < Length; t += 2)
        {
            var l = offset - t;

            output += H[t] * Z[l];
        }

        if ((Center & 1) == 0)
        {
            output += H[Center] * Z[offset - Center];
        }

        UpdateOffset();

        return output;
    }

    public float SymmetricalQuarter(float input)
    {
        var offset = Offset + Length;

        Z[Offset] = Z[offset] = input;

        var output = 0.0f;

        for (var t = 1; t < Center; t += 2)
        {
            var l = offset - t;
            var r = offset - (Center + (Center - t));

            var a = Z[l];
            var b = Z[r];

            output += H[t] * (a + b);
        }

        output += H[Center] * Z[offset - Center];

        UpdateOffset();

        return output;
    }

    public float SymmetricalFinal(float input)
    {
        var offset = Offset + Length;

        Z[Offset] = Z[offset] = input;

        var output = 0.0f;

        var length = Length - 1;

        var tap = 1;

        int end;

        for (end = Center - 8; tap < end; tap += 8)
        {
            var tapL1 = tap + 0;
            var tapR1 = length - tapL1;

            var tapL2 = tap + 2;
            var tapR2 = length - tapL2;

            var tapL3 = tap + 4;
            var tapR3 = length - tapL3;

            var tapL4 = tap + 6;
            var tapR4 = length - tapL4;

            output += H[tapL1] * (Z[offset - tapL1] + Z[offset - tapR1]) +
                      H[tapL2] * (Z[offset - tapL2] + Z[offset - tapR2]) +
                      H[tapL3] * (Z[offset - tapL3] + Z[offset - tapR3]) +
                      H[tapL4] * (Z[offset - tapL4] + Z[offset - tapR4]);
        }

        for (end = Center - 4; tap < end; tap += 4)
        {
            var tapL1 = tap + 0;
            var tapR1 = length - tapL1;

            var tapL2 = tap + 2;
            var tapR2 = length - tapL2;

            output += H[tapL1] * (Z[offset - tapL1] + Z[offset - tapR1]) +
                      H[tapL2] * (Z[offset - tapL2] + Z[offset - tapR2]);
        }

        for (end = Center; tap < end; tap += 2)
        {
            var tapL1 = tap + 0;
            var tapR1 = length - tapL1;

            output += H[tapL1] * (Z[offset - tapL1] + Z[offset - tapR1]);
        }

        output += H[Center] * Z[offset - Center];

        UpdateOffset();

        return output;
    }
}

public partial class HalfBandFirFilter
{
    private static void VectorizedCheck(float[] source, float[] target, int length)
    {
        Assert.AreEqual(0, length % 4);
        Assert.AreEqual(source.Length, target.Length);
        Assert.That.IsGreaterThanOrEqual(length, source.Length);
        Assert.That.IsGreaterThanOrEqual(length, target.Length);
    }

    public void VectorizedNormal(float[] source, float[] target, int length)
    {
        VectorizedCheck(source, target, length);

        for (var n = 0; n < length; n++)
        {
            Z[Offset] = Z[Offset + Length] = source[n];

            ref var output = ref target[n];

            output = 0.0f;

            for (var i = 0; i < Length; i++)
            {
                output += H[i] * Z[i + Offset];
            }

            UpdateOffset();
        }
    }

    public void VectorizedInner(float[] source, float[] target, int length)
    {
        VectorizedCheck(source, target, length);

        for (var n = 0; n < length; n++)
        {
            Z[Offset] = Z[Offset + Length] = source[n];

            ref var output = ref target[n];

            output = 0.0f;

            var i = 0;

            for (; i < Length - 4; i += 4)
            {
                var h0 = H[i + 0];
                var h1 = H[i + 1];
                var h2 = H[i + 2];
                var h3 = H[i + 3];
                var z0 = Z[i + 0 + Offset];
                var z1 = Z[i + 1 + Offset];
                var z2 = Z[i + 2 + Offset];
                var z3 = Z[i + 3 + Offset];
                output += h0 * z0 + h1 * z1 + h2 * z2 + h3 * z3;
            }

            for (; i < Length - 2; i += 2)
            {
                var h0 = H[i + 0];
                var h1 = H[i + 1];
                var z0 = Z[i + 0 + Offset];
                var z1 = Z[i + 1 + Offset];
                output += h0 * z0 + h1 * z1;
            }

            for (; i < Length; i += 1)
            {
                var i0 = i;
                var h0 = H[i0];
                var z0 = Z[i0 + Offset];
                output += h0 * z0;
            }

            UpdateOffset();
        }
    }

    public void VectorizedOuter(float[] source, float[] target, int length)
    {
        VectorizedCheck(source, target, length);

        for (var n = 0; n < length; n += 4)
        {
            var i0 = GetOffset(0);
            var i1 = GetOffset(1);
            var i2 = GetOffset(2);
            var i3 = GetOffset(3);

            var n0 = n + 0;
            var n1 = n + 1;
            var n2 = n + 2;
            var n3 = n + 3;

            Z[i0] = Z[i0 + Length] = source[n0];
            Z[i1] = Z[i1 + Length] = source[n1];
            Z[i2] = Z[i2 + Length] = source[n2];
            Z[i3] = Z[i3 + Length] = source[n3];

            ref var y0 = ref target[n0];
            ref var y1 = ref target[n1];
            ref var y2 = ref target[n2];
            ref var y3 = ref target[n3];

            y0 = y1 = y2 = y3 = 0.0f;

            for (var j = 0; j < Length; j += 1)
            {
                var h = H[j];

                y0 += h * Z[j + i0];
                y1 += h * Z[j + i1];
                y2 += h * Z[j + i2];
                y3 += h * Z[j + i3];
            }

            UpdateOffset(4);

            continue;
            Console.WriteLine(
                $"{n,4}, " +
                $"{n0,4}, " +
                $"{n1,4}, " +
                $"{n2,4}, " +
                $"{n3,4}, " +
                $"{i0,4}, " +
                $"{i1,4}, " +
                $"{i2,4}, " +
                $"{i3,4}, " +
                $"");
            continue;

            Console.WriteLine(
                $"{nameof(n)}: {n,4}, " +
                $"{nameof(n0)}: {n0,4}, " +
                $"{nameof(n1)}: {n1,4}, " +
                $"{nameof(n2)}: {n2,4}, " +
                $"{nameof(n3)}: {n3,4}, " +
                $"{nameof(i0)}: {i0,4}, " +
                $"{nameof(i1)}: {i1,4}, " +
                $"{nameof(i2)}: {i2,4}, " +
                $"{nameof(i3)}: {i3,4}, " +
                $"");
        }
    }

    public void VectorizedOuterInner(float[] source, float[] target, int length)
    {
        VectorizedCheck(source, target, length);

        var h = H;
        var z = Z;

        for (var s = 0; s < length; s += 4)
        {
            var dt = Tables[Offset];

            var d0 = dt[0];
            var d1 = dt[1];
            var d2 = dt[2];
            var d3 = dt[3];

            var s0 = s + 0;
            var s1 = s + 1;
            var s2 = s + 2;
            var s3 = s + 3;

            var x0 = source[s0];
            var x1 = source[s1];
            var x2 = source[s2];
            var x3 = source[s3];

            z[d0] = z[d0 + Length] = x0;
            z[d1] = z[d1 + Length] = x1;
            z[d2] = z[d2 + Length] = x2;
            z[d3] = z[d3 + Length] = x3;

            ref var y0 = ref target[s0];
            ref var y1 = ref target[s1];
            ref var y2 = ref target[s2];
            ref var y3 = ref target[s3];

            y0 = y1 = y2 = y3 = 0.0f;

            var t = 0;

            for (; t < Length - 4; t += 4)
            {
                var t0 = t + 0;
                var t1 = t + 1;
                var t2 = t + 2;
                var t3 = t + 3;
                var t4 = t + 4;
                var t5 = t + 5;
                var t6 = t + 6;

                var h0 = h[t0];
                var h1 = h[t1];
                var h2 = h[t2];
                var h3 = h[t3];

                var k0 = dt[t0];
                var k1 = dt[t1];
                var k2 = dt[t2];
                var k3 = dt[t3];
                var k4 = dt[t4];
                var k5 = dt[t5];
                var k6 = dt[t6];

                var z0 = z[k0];
                var z1 = z[k1];
                var z2 = z[k2];
                var z3 = z[k3];
                var z4 = z[k4];
                var z5 = z[k5];
                var z6 = z[k6];

                y0 += h0 * z0 + h1 * z1 + h2 * z2 + h3 * z3;
                y1 += h0 * z1 + h1 * z2 + h2 * z3 + h3 * z4;
                y2 += h0 * z2 + h1 * z3 + h2 * z4 + h3 * z5;
                y3 += h0 * z3 + h1 * z4 + h2 * z5 + h3 * z6;
            }

            for (; t < Length; t += 1)
            {
                var h0 = h[t];
                var z0 = z[t + d0];
                var z1 = z[t + d1];
                var z2 = z[t + d2];
                var z3 = z[t + d3];

                y0 += h0 * z0;
                y1 += h0 * z1;
                y2 += h0 * z2;
                y3 += h0 * z3;
            }

            Console.WriteLine(
                $"{y0,14}, {y1,14}, {y2,14}, {y3,14}, " +
                $"{d0,2}, {d1,2}, {d2,2}, {d3,2}, " +
                $"| {Offset,2}");

            UpdateOffset(4);
        }
    }

    [Obsolete]
    public void VectorizedInnerSimple(float[] source, float[] target, int length)
    {
        VectorizedCheck(source, target, length);

        for (var i = 0; i < source.Length; i++)
        {
            Z[Offset] = Z[Offset + Length] = source[i];

            var sum = 0.0f;

            var t = 0;

            for (; t < Length - 4; t += 4)
            {
                var t0 = t + 0;
                var t1 = t + 1;
                var t2 = t + 2;
                var t3 = t + 3;
                var h0 = H[t0];
                var h1 = H[t1];
                var h2 = H[t2];
                var h3 = H[t3];
                var z0 = Z[Offset + t0];
                var z1 = Z[Offset + t1];
                var z2 = Z[Offset + t2];
                var z3 = Z[Offset + t3];
                sum += h0 * z0 + h1 * z1 + h2 * z2 + h3 * z3;
            }

            for (; t < Length; t += 1)
            {
                sum += H[t] * Z[Offset + t];
            }

            target[i] = sum;

            UpdateOffset(1);
        }
    }

    public void VectorizedInnerVector4(float[] source, float[] target, int length)
    {
        VectorizedCheck(source, target, length);

        for (var i = 0; i < length; i++)
        {
            Z[Offset] = Z[Offset + Length] = source[i];

            var sum = 0.0f;

            var t = 0;

            for (; t < Length - 4; t += 4)
            {
                var hSpan = H.AsSpan(t);
                var zSpan = Z.AsSpan(t + Offset);

                var h = new Vector4(hSpan);
                var z = new Vector4(zSpan);

                sum += Vector4.Dot(h, z);
            }

            for (; t < Length; t += 1)
            {
                sum += H[t] * Z[Offset + t];
            }

            target[i] = sum;

            UpdateOffset(1);
        }
    }

    public void VectorizedOuterInner2(float[] source, float[] target, int length)
    {
        VectorizedCheck(source, target, length);

        var print = true;

        var i = 0;

        for (; i < length - 4; i += 4)
        {
            var i0 = Offset - 0;
            var i1 = Offset - 1;
            var i2 = Offset - 2;
            var i3 = Offset - 3;

            if (i0 < 0) i0 += Length;
            if (i1 < 0) i1 += Length;
            if (i2 < 0) i2 += Length;
            if (i3 < 0) i3 += Length;

            var samples = new Vector4(source.AsSpan(i));

            Z[i0] = Z[i0 + Length] = samples.W;
            Z[i1] = Z[i1 + Length] = samples.Z;
            Z[i2] = Z[i2 + Length] = samples.Y;
            Z[i3] = Z[i3 + Length] = samples.X;

            var sample0 = 0.0f;
            var sample1 = 0.0f;
            var sample2 = 0.0f;
            var sample3 = 0.0f;

            var t = 0;

            for (; t < Length; t++)
            {
                var zi0 = Offset - 3 + t;
                var zi1 = Offset - 2 + t;
                var zi2 = Offset - 1 + t;
                var zi3 = Offset - 0 + t;

                if (zi0 < 0) zi0 += Length;
                if (zi1 < 0) zi1 += Length;
                if (zi2 < 0) zi2 += Length;
                if (zi3 < 0) zi3 += Length;

                var hi0 = t + 0;
                var hi1 = t + 1;
                var hi2 = t + 2;
                var hi3 = t + 3;

                if (hi0 >= Length) hi0 -= Length;
                if (hi1 >= Length) hi1 -= Length;
                if (hi2 >= Length) hi2 -= Length;
                if (hi3 >= Length) hi3 -= Length;

                if (print)
                {
                    Console.WriteLine($"Tap {t,2}:\t\tH:\t{hi0,3}, {hi1,3}, {hi2,3}, {hi3,3}\t\tZ:\t{zi0,3}, {zi1,3}, {zi2,3}, {zi3,3}");
                }

                var h0 = H[hi0];
                var h1 = H[hi1];
                var h2 = H[hi2];
                var h3 = H[hi3];

                var z0 = Z[zi0];
                var z1 = Z[zi1];
                var z2 = Z[zi2];
                var z3 = Z[zi3];

                sample0 += h0 * z0;
                sample1 += h1 * z1;
                sample2 += h2 * z2;
                sample3 += h3 * z3;
            }

            print = false;

            target[i + 0] = sample0;
            target[i + 1] = sample1;
            target[i + 2] = sample2;
            target[i + 3] = sample3;

            UpdateOffset(4);
        }
    }

    public void VectorizedOuterInner234(float[] source, float[] target, int length)
    {
        VectorizedCheck(source, target, length);

        var i = 0;

        for (; i < length - 4; i += 4)
        {
            var i0 = Offset - 0 < 0 ? Offset - 0 + Length : Offset - 0;
            var i1 = Offset - 1 < 0 ? Offset - 1 + Length : Offset - 1;
            var i2 = Offset - 2 < 0 ? Offset - 2 + Length : Offset - 2;
            var i3 = Offset - 3 < 0 ? Offset - 3 + Length : Offset - 3;

            var samples = new Vector4(source.AsSpan(i));

            Z[i0] = Z[i0 + Length] = samples.X;
            Z[i1] = Z[i1 + Length] = samples.Y;
            Z[i2] = Z[i2 + Length] = samples.Z;
            Z[i3] = Z[i3 + Length] = samples.W;

            var sum = Vector4.Zero;

            for (var t = 0; t < Length - 4; t++)
            {
                var asSpan = Z.AsSpan(Offset + t);

                var t0 = t + i0;
                var t1 = t + i1;
                var t2 = t + i2;
                var t3 = t + i3;

                t0 = i0 - t;
                t1 = i1 - t;
                t2 = i2 - t;
                t3 = i3 - t;

                var z0 = Z[t0];
                var z1 = Z[t1];
                var z2 = Z[t2];
                var z3 = Z[t3];

                var zzzz = new Vector4(z0, z1, z2, z3);
                //Console.WriteLine(zzzz);
                //sum += H[t] * new Vector4(asSpan);
                sum += H[t] * zzzz;
            }

            target[i + 0] = sum.X;
            target[i + 1] = sum.Y;
            target[i + 2] = sum.Z;
            target[i + 3] = sum.W;

            UpdateOffset(1);
            UpdateOffset(1);
            UpdateOffset(1);
            UpdateOffset(1);
        }

        for (; i < length; i += 1)
        {
            var sample = source[i];
        }

        Assert.AreEqual(length, i);
    }
}