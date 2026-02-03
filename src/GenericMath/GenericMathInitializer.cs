namespace GenericMath;

/// <summary>
/// Static class containing initialization procedure for generic math module.
/// </summary>
public static class GenericMathInitializer
{
    /// <summary>
    /// Performs initialization of the generic math module.
    /// </summary>
    /// <remarks>
    /// Remember to initialize by calling this method before using
    /// any arithmetic of the number class, otherwise a
    /// 'NullReferenceException' will result.
    /// </remarks>
    public static void Initialize()
    {
        // Initialize the operators.
        Operators.ByteOperators.Initialize();
        Operators.DecimalOperators.Initialize();
        Operators.DoubleOperators.Initialize();
        Operators.IntegerOperators.Initialize();
        Operators.LongOperators.Initialize();
        Operators.SByteOperators.Initialize();
        Operators.ShortOperators.Initialize();
        Operators.SingleOperators.Initialize();
        Operators.UIntegerOperators.Initialize();
        Operators.ULongOperators.Initialize();
        Operators.UShortOperators.Initialize();

        // Initialize the math functions.
        MathFunctions.DecimalMathFunctions.Initialize();
        MathFunctions.DoubleMathFunctions.Initialize();
        MathFunctions.SingleMathFunctions.Initialize();
    }
}
