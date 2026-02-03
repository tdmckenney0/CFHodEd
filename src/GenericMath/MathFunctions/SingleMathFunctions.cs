namespace GenericMath.MathFunctions;

/// <summary>
/// Module containing code for arithmetic/math functions (on Single data type).
/// </summary>
internal static class SingleMathFunctions
{
    /// <summary>Math function for Single data type.</summary>
    private static float UnitStep(float value) => value <= 0 ? 0 : 1;

    /// <summary>Math function for Single data type.</summary>
    private static float Asinh(float value) => (float)System.Math.Log(value + System.Math.Sqrt(value * value + 1));

    /// <summary>Math function for Single data type.</summary>
    private static float Acosh(float value) => (float)System.Math.Log(value + System.Math.Sqrt(value * value - 1));

    /// <summary>Math function for Single data type.</summary>
    private static float Atanh(float value) => (float)(0.5 * System.Math.Log((1 + value) / (1 - value)));

    /// <summary>Math function for Single data type.</summary>
    private static float Acosech(float value)
    {
        value = 1 / value;
        return (float)System.Math.Log(value + System.Math.Sqrt(value * value + 1));
    }

    /// <summary>Math function for Single data type.</summary>
    private static float Asech(float value)
    {
        value = 1 / value;
        return (float)System.Math.Log(value + System.Math.Sqrt(value * value - 1));
    }

    /// <summary>Math function for Single data type.</summary>
    private static float Acoth(float value) => (float)(0.5 * System.Math.Log((value + 1) / (value - 1)));

    /// <summary>
    /// Initializes the single functions.
    /// </summary>
    internal static void Initialize()
    {
        Math<float>.Sqrt = v => (float)System.Math.Sqrt(v);
        Math<float>.Cubrt = v => (float)System.Math.Pow(v, 1.0 / 3.0);
        Math<float>.Abs = System.Math.Abs;
        Math<float>.Sign = v => System.Math.Sign(v);
        Math<float>.Ceiling = v => (float)System.Math.Ceiling(v);
        Math<float>.Floor = v => (float)System.Math.Floor(v);
        Math<float>.Truncate = v => (float)System.Math.Truncate(v);
        Math<float>.UnitStep = UnitStep;
        Math<float>.Sin = v => (float)System.Math.Sin(v);
        Math<float>.Cos = v => (float)System.Math.Cos(v);
        Math<float>.Tan = v => (float)System.Math.Tan(v);
        Math<float>.Cosec = v => (float)(1 / System.Math.Sin(v));
        Math<float>.Sec = v => (float)(1 / System.Math.Cos(v));
        Math<float>.Cot = v => (float)(1 / System.Math.Tan(v));
        Math<float>.Sinh = v => (float)System.Math.Sinh(v);
        Math<float>.Cosh = v => (float)System.Math.Cosh(v);
        Math<float>.Tanh = v => (float)System.Math.Tanh(v);
        Math<float>.Cosech = v => (float)(1 / System.Math.Sinh(v));
        Math<float>.Sech = v => (float)(1 / System.Math.Cosh(v));
        Math<float>.Coth = v => (float)(1 / System.Math.Tanh(v));
        Math<float>.Asin = v => (float)System.Math.Asin(v);
        Math<float>.Acos = v => (float)System.Math.Acos(v);
        Math<float>.Atan = v => (float)System.Math.Atan(v);
        Math<float>.Acosec = v => (float)System.Math.Asin(1 / v);
        Math<float>.Asec = v => (float)System.Math.Acos(1 / v);
        Math<float>.Acot = v => (float)System.Math.Atan(1 / v);
        Math<float>.Asinh = Asinh;
        Math<float>.Acosh = Acosh;
        Math<float>.Atanh = Atanh;
        Math<float>.Acosech = Acosech;
        Math<float>.Asech = Asech;
        Math<float>.Acoth = Acoth;
        Math<float>.Exp = v => (float)System.Math.Exp(v);
        Math<float>.Exp10 = v => (float)System.Math.Pow(10, v);
        Math<float>.Log = v => (float)System.Math.Log(v);
        Math<float>.Log10 = v => (float)System.Math.Log10(v);
    }
}
