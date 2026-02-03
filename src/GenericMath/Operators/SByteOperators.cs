namespace GenericMath.Operators;

/// <summary>
/// Provides operators for SByte data-type.
/// </summary>
internal static class SByteOperators
{
    internal static void Initialize()
    {
        Arithmetic<sbyte>.UnaryPlus = v => v;
        Arithmetic<sbyte>.UnaryMinus = v => (sbyte)-v;
        Arithmetic<sbyte>.Add = (l, r) => (sbyte)(l + r);
        Arithmetic<sbyte>.Subtract = (l, r) => (sbyte)(l - r);
        Arithmetic<sbyte>.Multiply = (l, r) => (sbyte)(l * r);
        Arithmetic<sbyte>.Divide = (l, r) => (sbyte)(l / r);
        Arithmetic<sbyte>.Power = (l, r) => (sbyte)System.Math.Pow(l, r);
        Arithmetic<sbyte>.IntegerDivide = (l, r) => (sbyte)(l / r);
        Arithmetic<sbyte>.Modulus = (l, r) => (sbyte)(l % r);
        Arithmetic<sbyte>.Equal = (l, r) => l == r;
        Arithmetic<sbyte>.NotEqual = (l, r) => l != r;
        Arithmetic<sbyte>.LessThan = (l, r) => l < r;
        Arithmetic<sbyte>.LessThanEqual = (l, r) => l <= r;
        Arithmetic<sbyte>.MoreThan = (l, r) => l > r;
        Arithmetic<sbyte>.MoreThanEqual = (l, r) => l >= r;
    }
}
