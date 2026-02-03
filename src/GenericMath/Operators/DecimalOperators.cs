namespace GenericMath.Operators;

/// <summary>
/// Provides operators for Decimal data-type.
/// </summary>
internal static class DecimalOperators
{
    /// <summary>Performs integral division.</summary>
    private static decimal IntegerDivide(decimal left, decimal right)
    {
        return (decimal)((long)left / (long)right);
    }

    /// <summary>
    /// Initializes the functions for Decimal datatype.
    /// </summary>
    internal static void Initialize()
    {
        Arithmetic<decimal>.UnaryPlus = v => v;
        Arithmetic<decimal>.UnaryMinus = v => -v;
        Arithmetic<decimal>.Add = (l, r) => l + r;
        Arithmetic<decimal>.Subtract = (l, r) => l - r;
        Arithmetic<decimal>.Multiply = (l, r) => l * r;
        Arithmetic<decimal>.Divide = (l, r) => l / r;
        Arithmetic<decimal>.Power = (l, r) => (decimal)System.Math.Pow((double)l, (double)r);
        Arithmetic<decimal>.IntegerDivide = IntegerDivide;
        Arithmetic<decimal>.Modulus = (l, r) => l % r;
        Arithmetic<decimal>.Equal = (l, r) => l == r;
        Arithmetic<decimal>.NotEqual = (l, r) => l != r;
        Arithmetic<decimal>.LessThan = (l, r) => l < r;
        Arithmetic<decimal>.LessThanEqual = (l, r) => l <= r;
        Arithmetic<decimal>.MoreThan = (l, r) => l > r;
        Arithmetic<decimal>.MoreThanEqual = (l, r) => l >= r;
    }
}
