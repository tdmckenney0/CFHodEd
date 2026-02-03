namespace GenericMath.Operators;

/// <summary>
/// Provides operators for UInt16 (UShort) data-type.
/// </summary>
internal static class UShortOperators
{
    internal static void Initialize()
    {
        Arithmetic<ushort>.UnaryPlus = v => v;
        Arithmetic<ushort>.UnaryMinus = v => throw new OverflowException("Cannot negate unsigned type");
        Arithmetic<ushort>.Add = (l, r) => (ushort)(l + r);
        Arithmetic<ushort>.Subtract = (l, r) => (ushort)(l - r);
        Arithmetic<ushort>.Multiply = (l, r) => (ushort)(l * r);
        Arithmetic<ushort>.Divide = (l, r) => (ushort)(l / r);
        Arithmetic<ushort>.Power = (l, r) => (ushort)System.Math.Pow(l, r);
        Arithmetic<ushort>.IntegerDivide = (l, r) => (ushort)(l / r);
        Arithmetic<ushort>.Modulus = (l, r) => (ushort)(l % r);
        Arithmetic<ushort>.Equal = (l, r) => l == r;
        Arithmetic<ushort>.NotEqual = (l, r) => l != r;
        Arithmetic<ushort>.LessThan = (l, r) => l < r;
        Arithmetic<ushort>.LessThanEqual = (l, r) => l <= r;
        Arithmetic<ushort>.MoreThan = (l, r) => l > r;
        Arithmetic<ushort>.MoreThanEqual = (l, r) => l >= r;
    }
}
