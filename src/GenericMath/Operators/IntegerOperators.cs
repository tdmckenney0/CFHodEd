namespace GenericMath.Operators;

/// <summary>
/// Provides operators for Int32 (Integer) data-type.
/// </summary>
internal static class IntegerOperators
{
    /// <summary>
    /// Initializes the functions for Int32 datatype.
    /// </summary>
    internal static void Initialize()
    {
        Arithmetic<int>.UnaryPlus = v => v;
        Arithmetic<int>.UnaryMinus = v => -v;
        Arithmetic<int>.Add = (l, r) => l + r;
        Arithmetic<int>.Subtract = (l, r) => l - r;
        Arithmetic<int>.Multiply = (l, r) => l * r;
        Arithmetic<int>.Divide = (l, r) => l / r;
        Arithmetic<int>.Power = (l, r) => (int)System.Math.Pow(l, r);
        Arithmetic<int>.IntegerDivide = (l, r) => l / r;
        Arithmetic<int>.Modulus = (l, r) => l % r;
        Arithmetic<int>.Equal = (l, r) => l == r;
        Arithmetic<int>.NotEqual = (l, r) => l != r;
        Arithmetic<int>.LessThan = (l, r) => l < r;
        Arithmetic<int>.LessThanEqual = (l, r) => l <= r;
        Arithmetic<int>.MoreThan = (l, r) => l > r;
        Arithmetic<int>.MoreThanEqual = (l, r) => l >= r;
    }
}
