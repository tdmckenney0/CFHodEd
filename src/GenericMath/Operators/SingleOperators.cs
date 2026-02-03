namespace GenericMath.Operators;

/// <summary>
/// Provides operators for Single (float) data-type.
/// </summary>
internal static class SingleOperators
{
    /// <summary>Performs integral division.</summary>
    private static float IntegerDivide(float left, float right)
    {
        return (float)((long)left / (long)right);
    }

    /// <summary>
    /// Initializes the functions for Single datatype.
    /// </summary>
    internal static void Initialize()
    {
        Arithmetic<float>.UnaryPlus = v => v;
        Arithmetic<float>.UnaryMinus = v => -v;
        Arithmetic<float>.Add = (l, r) => l + r;
        Arithmetic<float>.Subtract = (l, r) => l - r;
        Arithmetic<float>.Multiply = (l, r) => l * r;
        Arithmetic<float>.Divide = (l, r) => l / r;
        Arithmetic<float>.Power = (l, r) => (float)System.Math.Pow(l, r);
        Arithmetic<float>.IntegerDivide = IntegerDivide;
        Arithmetic<float>.Modulus = (l, r) => l % r;
        Arithmetic<float>.Equal = (l, r) => l == r;
        Arithmetic<float>.NotEqual = (l, r) => l != r;
        Arithmetic<float>.LessThan = (l, r) => l < r;
        Arithmetic<float>.LessThanEqual = (l, r) => l <= r;
        Arithmetic<float>.MoreThan = (l, r) => l > r;
        Arithmetic<float>.MoreThanEqual = (l, r) => l >= r;
    }
}
