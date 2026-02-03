namespace GenericMath.Operators;

/// <summary>
/// Provides operators for Double data-type.
/// </summary>
internal static class DoubleOperators
{
    /// <summary>Performs integral division.</summary>
    private static double IntegerDivide(double left, double right)
    {
        return (double)((long)left / (long)right);
    }

    /// <summary>
    /// Initializes the functions for Double datatype.
    /// </summary>
    internal static void Initialize()
    {
        Arithmetic<double>.UnaryPlus = v => v;
        Arithmetic<double>.UnaryMinus = v => -v;
        Arithmetic<double>.Add = (l, r) => l + r;
        Arithmetic<double>.Subtract = (l, r) => l - r;
        Arithmetic<double>.Multiply = (l, r) => l * r;
        Arithmetic<double>.Divide = (l, r) => l / r;
        Arithmetic<double>.Power = System.Math.Pow;
        Arithmetic<double>.IntegerDivide = IntegerDivide;
        Arithmetic<double>.Modulus = (l, r) => l % r;
        Arithmetic<double>.Equal = (l, r) => l == r;
        Arithmetic<double>.NotEqual = (l, r) => l != r;
        Arithmetic<double>.LessThan = (l, r) => l < r;
        Arithmetic<double>.LessThanEqual = (l, r) => l <= r;
        Arithmetic<double>.MoreThan = (l, r) => l > r;
        Arithmetic<double>.MoreThanEqual = (l, r) => l >= r;
    }
}
