using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace FIRConvolution.Tests;

[TestClass]
public class UnitTestTemp
{
    [UsedImplicitly]
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void TestMethod1()
    {
        PrintSizeOfAlignOf<Struct1>();
        PrintSizeOfAlignOf<Struct2>();
        PrintSizeOfAlignOf<Struct3>();
        PrintSizeOfAlignOf<Struct4>();
        PrintSizeOfAlignOf<Struct5>();
        PrintSizeOfAlignOf<Struct6>();
        PrintSizeOfAlignOf<Struct7>();
        PrintSizeOfAlignOf<Struct8>();
        PrintSizeOfAlignOf<Struct9>();
        PrintSizeOfAlignOf<Struct10>();
        PrintSizeOfAlignOf<Struct11>();
    }
    
    public void PrintSizeOfAlignOf<T>() where T : struct
    {
        var sizeOf1 = Marshal.SizeOf<T>();
        var sizeOf2 = UnsafeUnity.SizeOf<T>();

        var alignOf1 = UnsafeHelper.AlignOf<T>();
        var alignOf2 = UnsafeUnity.AlignOf<T>();

        TestContext.WriteLine(
            $"{typeof(T).Name,-12}: " +
            $"SizeOf: {sizeOf1,3}, {sizeOf2,3}, " +
            $"AlignOf: {alignOf1,3}, {alignOf2,3}, " +
            $"Match: {sizeOf1 == sizeOf2,6}, {alignOf1 == alignOf2,6}");
    }

    public struct Struct1
    {
        public byte B1;
    }

    public struct Struct2
    {
        public byte B1, B2;
    }

    public struct Struct3
    {
        public byte B1, B2, B3;
    }

    public struct Struct4
    {
        public byte B1, B2, B3, B4;
    }

    public struct Struct5
    {
        public int B1;
    }

    public struct Struct6
    {
        public int I1, I2;
    }

    public struct Struct7
    {
        public int I1, I2, I3;
    }

    public struct Struct8
    {
        public int I1, I2, I3, I4;
    }

    public struct Struct9
    {
        public byte B1;
        public Struct4 Struct4;
        public int I1;
        public Struct5 Struct5;
        public byte B2;
        public int I2;
        public Struct6 Struct6;
        public int I3;
        public byte B4;
        public Struct7 Struct7;
        public int I4;
        public byte B3;
    }

    public struct Struct10
    {
        public Struct9 Struct4;
        public byte B1;
        public Struct9 Struct5;
        public int I1;
        public byte B2;
        public int I2;
        public Struct9 Struct6;
        public int I3;
        public byte B4;
        public int I4;
        public Struct9 Struct7;
        public byte B3;
    }

    public struct Struct11
    {
        public Struct10 Struct4;
        public byte B1;
        public Struct10 Struct5;
        public int I1;
        public byte B2;
        public int I2;
        public Struct10 Struct6;
        public int I3;
        public byte B4;
        public int I4;
        public Struct10 Struct7;
        public byte B3;
        public string D;
    }
}

public static class UnsafeHelper
{
    public static int AlignOf<T>() where T : struct
    {
        var type = typeof(T);

        if (!IsBlittable<T>())
        {
            throw new InvalidOperationException("The type is not blittable.");
        }

        var result = 1;

        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            var fieldType = field.FieldType;

            if (fieldType.IsPrimitive)
            {
                result = Math.Max(result, Marshal.SizeOf(fieldType));
            }
        }

        return result;
    }

    public static bool IsBlittable<T>()
    {
        return IsBlittableCache<T>.Value;
    }

    private static bool IsBlittable(this Type type)
    {
        var handle = default(GCHandle);

        try
        {
            var instance = FormatterServices.GetUninitializedObject(type);

            handle = GCHandle.Alloc(instance, GCHandleType.Pinned);

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (handle.IsAllocated)
            {
                handle.Free();
            }
        }
    }

    private static class IsBlittableCache<T>
    {
        public static readonly bool Value = IsBlittable(typeof(T));
    }
}