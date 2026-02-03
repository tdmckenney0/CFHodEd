namespace GenericMath.MathFunctions;

/// <summary>
/// Module containing code for arithmetic/math functions (on Double data type).
/// </summary>
internal static class DoubleMathFunctions
{
    private static double UnitStep(double value) => value <= 0 ? 0 : 1;
    private static double Asinh(double value) => System.Math.Log(value + System.Math.Sqrt(value * value + 1));
    private static double Acosh(double value) => System.Math.Log(value + System.Math.Sqrt(value * value - 1));
    private static double Atanh(double value) => 0.5 * System.Math.Log((1 + value) / (1 - value));
    private static double Acosech(double value)
    {
        value = 1 / value;
        return System.Math.Log(value + System.Math.Sqrt(value * value + 1));
    }
    private static double Asech(double value)
    {
        value = 1 / value;
        return System.Math.Log(value + System.Math.Sqrt(value * value - 1));
    }
    private static double Acoth(double value) => 0.5 * System.Math.Log((value + 1) / (value - 1));

    internal static void Initialize()
    {
        Math<double>.Sqrt = System.Math.Sqrt;
        Math<double>.Cubrt = v => System.Math.Pow(v, 1.0 / 3.0);
        Math<double>.Abs = System.Math.Abs;
        Math<double>.Sign = v => System.Math.Sign(v);
        Math<double>.Ceiling = System.Math.Ceiling;
        Math<double>.Floor = System.Math.Floor;
        Math<double>.Truncate = System.Math.Truncate;
        Math<double>.UnitStep = UnitStep;
        Math<double>.Sin = System.Math.Sin;
        Math<double>.Cos = System.Math.Cos;
        Math<double>.Tan = System.Math.Tan;
        Math<double>.Cosec = v => 1 / System.Math.Sin(v);
        Math<double>.Sec = v => 1 / System.Math.Cos(v);
        Math<double>.Cot = v => 1 / System.Math.Tan(v);
        Math<double>.Sinh = System.Math.Sinh;
        Math<double>.Cosh = System.Math.Cosh;
        Math<double>.Tanh = System.Math.Tanh;
        Math<double>.Cosech = v => 1 / System.Math.Sinh(v);
        Math<double>.Sech = v => 1 / System.Math.Cosh(v);
        Math<double>.Coth = v => 1 / System.Math.Tanh(v);
        Math<double>.Asin = System.Math.Asin;
        Math<double>.Acos = System.Math.Acos;
        Math<double>.Atan = System.Math.Atan;
        Math<double>.Acosec = v => System.Math.Asin(1 / v);
        Math<double>.Asec = v => System.Math.Acos(1 / v);
        Math<double>.Acot = v => System.Math.Atan(1 / v);
        Math<double>.Asinh = Asinh;
        Math<double>.Acosh = Acosh;
        Math<double>.Atanh = Atanh;
        Math<double>.Acosech = Acosech;
        Math<double>.Asech = Asech;
        Math<double>.Acoth = Acoth;
        Math<double>.Exp = System.Math.Exp;
        Math<double>.Exp10 = v => System.Math.Pow(10, v);
        Math<double>.Log = System.Math.Log;
        Math<double>.Log10 = System.Math.Log10;
    }
}
