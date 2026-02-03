namespace GenericMath.Operators;

/// <summary>
/// Provides operators for Byte data-type.
/// </summary>
internal static class ByteOperators
{
    internal static void Initialize()
    {
        Arithmetic<byte>.UnaryPlus = v => v;
        Arithmetic<byte>.UnaryMinus = v => throw new OverflowException("Cannot negate unsigned type");
        Arithmetic<byte>.Add = (l, r) => (byte)(l + r);
        Arithmetic<byte>.Subtract = (l, r) => (byte)(l - r);
        Arithmetic<byte>.Multiply = (l, r) => (byte)(l * r);
        Arithmetic<byte>.Divide = (l, r) => (byte)(l / r);
        Arithmetic<byte>.Power = (l, r) => (byte)System.Math.Pow(l, r);
        Arithmetic<byte>.IntegerDivide = (l, r) => (byte)(l / r);
        Arithmetic<byte>.Modulus = (l, r) => (byte)(l % r);
        Arithmetic<byte>.Equal = (l, r) => l == r;
        Arithmetic<byte>.NotEqual = (l, r) => l != r;
        Arithmetic<byte>.LessThan = (l, r) => l < r;
        Arithmetic<byte>.LessThanEqual = (l, r) => l <= r;
        Arithmetic<byte>.MoreThan = (l, r) => l > r;
        Arithmetic<byte>.MoreThanEqual = (l, r) => l >= r;
    }
}
