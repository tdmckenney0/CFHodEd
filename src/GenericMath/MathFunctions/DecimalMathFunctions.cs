namespace GenericMath.MathFunctions;

/// <summary>
/// Module containing code for arithmetic/math functions (on Decimal data type).
/// </summary>
internal static class DecimalMathFunctions
{
    private static decimal UnitStep(decimal value) => value <= 0 ? 0 : 1;
    private static decimal Asinh(decimal value) => (decimal)System.Math.Log((double)value + System.Math.Sqrt((double)(value * value + 1)));
    private static decimal Acosh(decimal value) => (decimal)System.Math.Log((double)value + System.Math.Sqrt((double)(value * value - 1)));
    private static decimal Atanh(decimal value) => (decimal)(0.5 * System.Math.Log((double)((1 + value) / (1 - value))));
    private static decimal Acosech(decimal value)
    {
        value = 1 / value;
        return (decimal)System.Math.Log((double)value + System.Math.Sqrt((double)(value * value + 1)));
    }
    private static decimal Asech(decimal value)
    {
        value = 1 / value;
        return (decimal)System.Math.Log((double)value + System.Math.Sqrt((double)(value * value - 1)));
    }
    private static decimal Acoth(decimal value) => (decimal)(0.5 * System.Math.Log((double)((value + 1) / (value - 1))));

    internal static void Initialize()
    {
        Math<decimal>.Sqrt = v => (decimal)System.Math.Sqrt((double)v);
        Math<decimal>.Cubrt = v => (decimal)System.Math.Pow((double)v, 1.0 / 3.0);
        Math<decimal>.Abs = System.Math.Abs;
        Math<decimal>.Sign = v => System.Math.Sign(v);
        Math<decimal>.Ceiling = System.Math.Ceiling;
        Math<decimal>.Floor = System.Math.Floor;
        Math<decimal>.Truncate = System.Math.Truncate;
        Math<decimal>.UnitStep = UnitStep;
        Math<decimal>.Sin = v => (decimal)System.Math.Sin((double)v);
        Math<decimal>.Cos = v => (decimal)System.Math.Cos((double)v);
        Math<decimal>.Tan = v => (decimal)System.Math.Tan((double)v);
        Math<decimal>.Cosec = v => (decimal)(1 / System.Math.Sin((double)v));
        Math<decimal>.Sec = v => (decimal)(1 / System.Math.Cos((double)v));
        Math<decimal>.Cot = v => (decimal)(1 / System.Math.Tan((double)v));
        Math<decimal>.Sinh = v => (decimal)System.Math.Sinh((double)v);
        Math<decimal>.Cosh = v => (decimal)System.Math.Cosh((double)v);
        Math<decimal>.Tanh = v => (decimal)System.Math.Tanh((double)v);
        Math<decimal>.Cosech = v => (decimal)(1 / System.Math.Sinh((double)v));
        Math<decimal>.Sech = v => (decimal)(1 / System.Math.Cosh((double)v));
        Math<decimal>.Coth = v => (decimal)(1 / System.Math.Tanh((double)v));
        Math<decimal>.Asin = v => (decimal)System.Math.Asin((double)v);
        Math<decimal>.Acos = v => (decimal)System.Math.Acos((double)v);
        Math<decimal>.Atan = v => (decimal)System.Math.Atan((double)v);
        Math<decimal>.Acosec = v => (decimal)System.Math.Asin(1 / (double)v);
        Math<decimal>.Asec = v => (decimal)System.Math.Acos(1 / (double)v);
        Math<decimal>.Acot = v => (decimal)System.Math.Atan(1 / (double)v);
        Math<decimal>.Asinh = Asinh;
        Math<decimal>.Acosh = Acosh;
        Math<decimal>.Atanh = Atanh;
        Math<decimal>.Acosech = Acosech;
        Math<decimal>.Asech = Asech;
        Math<decimal>.Acoth = Acoth;
        Math<decimal>.Exp = v => (decimal)System.Math.Exp((double)v);
        Math<decimal>.Exp10 = v => (decimal)System.Math.Pow(10, (double)v);
        Math<decimal>.Log = v => (decimal)System.Math.Log((double)v);
        Math<decimal>.Log10 = v => (decimal)System.Math.Log10((double)v);
    }
}
