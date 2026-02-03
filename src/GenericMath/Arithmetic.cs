namespace GenericMath;

/// <summary>
/// Represents a generic structure which can perform arithmetic on
/// generic data types; this acts as a wrapper for numeric types.
/// </summary>
/// <typeparam name="T">The type of data type to perform arithmetic on.</typeparam>
/// <remarks>
/// The functions that are intended to be used with the data-type
/// must have been set; there is no exception handling provided.
/// Also, before using this structure, make sure to call
/// <c>GenericMathInitializer.Initialize()</c>
/// </remarks>
public struct Arithmetic<T>
{
    // Operator delegates
    /// <summary>Unary plus operator.</summary>
    public static Func<T, T>? UnaryPlus;

    /// <summary>Unary minus operator.</summary>
    public static Func<T, T>? UnaryMinus;

    /// <summary>Function to perform addition.</summary>
    public static Func<T, T, T>? Add;

    /// <summary>Function to perform subtraction.</summary>
    public static Func<T, T, T>? Subtract;

    /// <summary>Function to perform multiplication.</summary>
    public static Func<T, T, T>? Multiply;

    /// <summary>Function to perform division.</summary>
    public static Func<T, T, T>? Divide;

    /// <summary>Function to perform integer division (return quotient).</summary>
    public static Func<T, T, T>? IntegerDivide;

    /// <summary>Function to perform modulus arithmetic (return remainder).</summary>
    public static Func<T, T, T>? Modulus;

    /// <summary>Function to perform exponentiation.</summary>
    public static Func<T, T, T>? Power;

    /// <summary>'Equality' operator (==).</summary>
    public static Func<T, T, bool>? Equal;

    /// <summary>'Inequality' operator (!=).</summary>
    public static Func<T, T, bool>? NotEqual;

    /// <summary>'Less than' operator (&lt;)</summary>
    public static Func<T, T, bool>? LessThan;

    /// <summary>'Less than or equal to' operator (&lt;=).</summary>
    public static Func<T, T, bool>? LessThanEqual;

    /// <summary>'More than' operator (&gt;).</summary>
    public static Func<T, T, bool>? MoreThan;

    /// <summary>'More than or equal to' operator (&gt;=).</summary>
    public static Func<T, T, bool>? MoreThanEqual;

    /// <summary>
    /// The data stored in this instance.
    /// </summary>
    public T Value;

    /// <summary>
    /// Constructor for the <c>Arithmetic</c> structure.
    /// </summary>
    /// <param name="value">The data to store.</param>
    public Arithmetic(T value)
    {
        Value = value;
    }

    // Implicit conversion operators
    /// <summary>
    /// Generic data type (T) to Arithmetic conversion operator.
    /// </summary>
    public static implicit operator Arithmetic<T>(T value)
    {
        return new Arithmetic<T>(value);
    }

    /// <summary>
    /// Arithmetic to generic data type (T) conversion operator.
    /// </summary>
    public static implicit operator T(Arithmetic<T> obj)
    {
        return obj.Value;
    }

    /// <summary>Unary plus operator.</summary>
    public static Arithmetic<T> operator +(Arithmetic<T> v)
    {
        return UnaryPlus!(v.Value);
    }

    /// <summary>
    /// Unary minus operator.
    /// </summary>
    /// <exception cref="OverflowException">
    /// Thrown when the unary negation operator is used with unsigned generic
    /// data type (T). In other words this happens for the following
    /// data types: Byte, UShort, UInteger, and ULong.
    /// </exception>
    public static Arithmetic<T> operator -(Arithmetic<T> v)
    {
        return UnaryMinus!(v.Value);
    }

    /// <summary>Function to perform addition.</summary>
    public static Arithmetic<T> operator +(Arithmetic<T> left, Arithmetic<T> right)
    {
        return Add!(left.Value, right.Value);
    }

    /// <summary>Function to perform subtraction.</summary>
    public static Arithmetic<T> operator -(Arithmetic<T> left, Arithmetic<T> right)
    {
        return Subtract!(left.Value, right.Value);
    }

    /// <summary>Function to perform multiplication.</summary>
    public static Arithmetic<T> operator *(Arithmetic<T> left, Arithmetic<T> right)
    {
        return Multiply!(left.Value, right.Value);
    }

    /// <summary>Function to perform division.</summary>
    public static Arithmetic<T> operator /(Arithmetic<T> left, Arithmetic<T> right)
    {
        return Divide!(left.Value, right.Value);
    }

    /// <summary>Function to perform modular division (return remainder).</summary>
    public static Arithmetic<T> operator %(Arithmetic<T> left, Arithmetic<T> right)
    {
        return Modulus!(left.Value, right.Value);
    }

    /// <summary>'Equality' operator (==).</summary>
    public static bool operator ==(Arithmetic<T> left, Arithmetic<T> right)
    {
        return Equal!(left.Value, right.Value);
    }

    /// <summary>'Inequality' operator (!=).</summary>
    public static bool operator !=(Arithmetic<T> left, Arithmetic<T> right)
    {
        return NotEqual!(left.Value, right.Value);
    }

    /// <summary>'Less than' operator (&lt;).</summary>
    public static bool operator <(Arithmetic<T> left, Arithmetic<T> right)
    {
        return LessThan!(left.Value, right.Value);
    }

    /// <summary>'Less than or equal to' operator (&lt;=).</summary>
    public static bool operator <=(Arithmetic<T> left, Arithmetic<T> right)
    {
        return LessThanEqual!(left.Value, right.Value);
    }

    /// <summary>'More than' operator (&gt;).</summary>
    public static bool operator >(Arithmetic<T> left, Arithmetic<T> right)
    {
        return MoreThan!(left.Value, right.Value);
    }

    /// <summary>'More than or equal to' operator (&gt;=).</summary>
    public static bool operator >=(Arithmetic<T> left, Arithmetic<T> right)
    {
        return MoreThanEqual!(left.Value, right.Value);
    }

    /// <summary>
    /// Returns the value in the specified format.
    /// </summary>
    /// <typeparam name="TOut">The type of value to return.</typeparam>
    /// <returns>The converted value.</returns>
    /// <remarks>
    /// This involves boxing and unboxing (i.e. an
    /// object is used as an intermediate for conversion).
    /// </remarks>
    public TOut ValueGet<TOut>()
    {
        return (TOut)(object)Value!;
    }

    /// <summary>
    /// Sets the value in the specified format.
    /// </summary>
    /// <typeparam name="TIn">The type of value to set.</typeparam>
    /// <remarks>
    /// This involves boxing and unboxing (i.e. an
    /// object is used as an intermediate for conversion).
    /// </remarks>
    public void ValueSet<TIn>(TIn v)
    {
        Value = (T)(object)v!;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is Arithmetic<T> other)
            return Equal!(Value, other.Value);
        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }
}
