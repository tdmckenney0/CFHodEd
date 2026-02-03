namespace GenericMath.Operators;

/// <summary>
/// Provides operators for Int64 (Long) data-type.
/// </summary>
internal static class LongOperators
{
    internal static void Initialize()
    {
        Arithmetic<long>.UnaryPlus = v => v;
        Arithmetic<long>.UnaryMinus = v => -v;
        Arithmetic<long>.Add = (l, r) => l + r;
        Arithmetic<long>.Subtract = (l, r) => l - r;
        Arithmetic<long>.Multiply = (l, r) => l * r;
        Arithmetic<long>.Divide = (l, r) => l / r;
        Arithmetic<long>.Power = (l, r) => (long)System.Math.Pow(l, r);
        Arithmetic<long>.IntegerDivide = (l, r) => l / r;
        Arithmetic<long>.Modulus = (l, r) => l % r;
        Arithmetic<long>.Equal = (l, r) => l == r;
        Arithmetic<long>.NotEqual = (l, r) => l != r;
        Arithmetic<long>.LessThan = (l, r) => l < r;
        Arithmetic<long>.LessThanEqual = (l, r) => l <= r;
        Arithmetic<long>.MoreThan = (l, r) => l > r;
        Arithmetic<long>.MoreThanEqual = (l, r) => l >= r;
    }
}
