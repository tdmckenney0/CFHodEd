namespace GenericMath;

/// <summary>
/// Class acting as an interface for math functions on different data types.
/// </summary>
/// <typeparam name="T">Data type on which to operate upon.</typeparam>
public sealed class Math<T>
{
    // POWER FUNCTIONS
    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Sqrt;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Cubrt;

    // SPECIAL FUNCTIONS
    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Abs;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Sign;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Ceiling;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Floor;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Truncate;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? UnitStep;

    // TRIGONOMETRIC FUNCTIONS
    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Sin;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Cos;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Tan;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Cosec;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Sec;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Cot;

    // HYPERBOLIC FUNCTIONS
    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Sinh;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Cosh;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Tanh;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Cosech;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Sech;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Coth;

    // INVERSE TRIGONOMETRIC FUNCTIONS
    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Asin;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Acos;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Atan;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Acosec;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Asec;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Acot;

    // INVERSE HYPERBOLIC FUNCTIONS
    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Asinh;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Acosh;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Atanh;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Acosech;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Asech;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Acoth;

    // EXPONENTIAL/LOGARITHMIC FUNCTIONS
    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Exp;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Exp10;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Log;

    /// <summary>Math function for custom data type.</summary>
    public static Func<T, T>? Log10;
}
