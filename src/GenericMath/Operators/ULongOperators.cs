namespace GenericMath.Operators;

/// <summary>
/// Provides operators for UInt64 (ULong) data-type.
/// </summary>
internal static class ULongOperators
{
    internal static void Initialize()
    {
        Arithmetic<ulong>.UnaryPlus = v => v;
        Arithmetic<ulong>.UnaryMinus = v => throw new OverflowException("Cannot negate unsigned type");
        Arithmetic<ulong>.Add = (l, r) => l + r;
        Arithmetic<ulong>.Subtract = (l, r) => l - r;
        Arithmetic<ulong>.Multiply = (l, r) => l * r;
        Arithmetic<ulong>.Divide = (l, r) => l / r;
        Arithmetic<ulong>.Power = (l, r) => (ulong)System.Math.Pow(l, r);
        Arithmetic<ulong>.IntegerDivide = (l, r) => l / r;
        Arithmetic<ulong>.Modulus = (l, r) => l % r;
        Arithmetic<ulong>.Equal = (l, r) => l == r;
        Arithmetic<ulong>.NotEqual = (l, r) => l != r;
        Arithmetic<ulong>.LessThan = (l, r) => l < r;
        Arithmetic<ulong>.LessThanEqual = (l, r) => l <= r;
        Arithmetic<ulong>.MoreThan = (l, r) => l > r;
        Arithmetic<ulong>.MoreThanEqual = (l, r) => l >= r;
    }
}
