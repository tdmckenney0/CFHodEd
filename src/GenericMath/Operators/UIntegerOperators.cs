namespace GenericMath.Operators;

/// <summary>
/// Provides operators for UInt32 (UInteger) data-type.
/// </summary>
internal static class UIntegerOperators
{
    internal static void Initialize()
    {
        Arithmetic<uint>.UnaryPlus = v => v;
        Arithmetic<uint>.UnaryMinus = v => throw new OverflowException("Cannot negate unsigned type");
        Arithmetic<uint>.Add = (l, r) => l + r;
        Arithmetic<uint>.Subtract = (l, r) => l - r;
        Arithmetic<uint>.Multiply = (l, r) => l * r;
        Arithmetic<uint>.Divide = (l, r) => l / r;
        Arithmetic<uint>.Power = (l, r) => (uint)System.Math.Pow(l, r);
        Arithmetic<uint>.IntegerDivide = (l, r) => l / r;
        Arithmetic<uint>.Modulus = (l, r) => l % r;
        Arithmetic<uint>.Equal = (l, r) => l == r;
        Arithmetic<uint>.NotEqual = (l, r) => l != r;
        Arithmetic<uint>.LessThan = (l, r) => l < r;
        Arithmetic<uint>.LessThanEqual = (l, r) => l <= r;
        Arithmetic<uint>.MoreThan = (l, r) => l > r;
        Arithmetic<uint>.MoreThanEqual = (l, r) => l >= r;
    }
}
