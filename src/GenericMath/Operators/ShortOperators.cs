namespace GenericMath.Operators;

/// <summary>
/// Provides operators for Int16 (Short) data-type.
/// </summary>
internal static class ShortOperators
{
    internal static void Initialize()
    {
        Arithmetic<short>.UnaryPlus = v => v;
        Arithmetic<short>.UnaryMinus = v => (short)-v;
        Arithmetic<short>.Add = (l, r) => (short)(l + r);
        Arithmetic<short>.Subtract = (l, r) => (short)(l - r);
        Arithmetic<short>.Multiply = (l, r) => (short)(l * r);
        Arithmetic<short>.Divide = (l, r) => (short)(l / r);
        Arithmetic<short>.Power = (l, r) => (short)System.Math.Pow(l, r);
        Arithmetic<short>.IntegerDivide = (l, r) => (short)(l / r);
        Arithmetic<short>.Modulus = (l, r) => (short)(l % r);
        Arithmetic<short>.Equal = (l, r) => l == r;
        Arithmetic<short>.NotEqual = (l, r) => l != r;
        Arithmetic<short>.LessThan = (l, r) => l < r;
        Arithmetic<short>.LessThanEqual = (l, r) => l <= r;
        Arithmetic<short>.MoreThan = (l, r) => l > r;
        Arithmetic<short>.MoreThanEqual = (l, r) => l >= r;
    }
}
