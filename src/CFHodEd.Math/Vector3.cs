using System.Numerics;
using System.Runtime.CompilerServices;

namespace CFHodEd.Math;

/// <summary>
/// A 3-component vector compatible with DirectX Vector3.
/// Wraps System.Numerics.Vector3 with DirectX-compatible API.
/// </summary>
public struct Vector3 : IEquatable<Vector3>
{
    /// <summary>The X component of the vector.</summary>
    public float X;

    /// <summary>The Y component of the vector.</summary>
    public float Y;

    /// <summary>The Z component of the vector.</summary>
    public float Z;

    /// <summary>A vector with all components set to zero.</summary>
    public static readonly Vector3 Zero = new(0, 0, 0);

    /// <summary>A unit vector pointing in the X direction.</summary>
    public static readonly Vector3 UnitX = new(1, 0, 0);

    /// <summary>A unit vector pointing in the Y direction.</summary>
    public static readonly Vector3 UnitY = new(0, 1, 0);

    /// <summary>A unit vector pointing in the Z direction.</summary>
    public static readonly Vector3 UnitZ = new(0, 0, 1);

    /// <summary>A vector with all components set to one.</summary>
    public static readonly Vector3 One = new(1, 1, 1);

    /// <summary>
    /// Initializes a new Vector3.
    /// </summary>
    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    /// <summary>
    /// Initializes a new Vector3 with all components set to the same value.
    /// </summary>
    public Vector3(float value)
    {
        X = Y = Z = value;
    }

    /// <summary>
    /// Gets the length (magnitude) of the vector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z);

    /// <summary>
    /// Gets the squared length of the vector.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float LengthSquared() => X * X + Y * Y + Z * Z;

    /// <summary>
    /// Normalizes this vector in place.
    /// </summary>
    public void Normalize()
    {
        float len = Length();
        if (len > 0)
        {
            X /= len;
            Y /= len;
            Z /= len;
        }
    }

    /// <summary>
    /// Returns a normalized copy of this vector.
    /// </summary>
    public static Vector3 Normalize(Vector3 value)
    {
        float len = value.Length();
        if (len > 0)
            return new Vector3(value.X / len, value.Y / len, value.Z / len);
        return value;
    }

    /// <summary>
    /// Computes the dot product of two vectors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Vector3 left, Vector3 right)
        => left.X * right.X + left.Y * right.Y + left.Z * right.Z;

    /// <summary>
    /// Computes the cross product of two vectors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Cross(Vector3 left, Vector3 right)
        => new(
            left.Y * right.Z - left.Z * right.Y,
            left.Z * right.X - left.X * right.Z,
            left.X * right.Y - left.Y * right.X);

    /// <summary>
    /// Returns the distance between two vectors.
    /// </summary>
    public static float Distance(Vector3 a, Vector3 b)
        => (a - b).Length();

    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    public static Vector3 Lerp(Vector3 start, Vector3 end, float amount)
        => new(
            start.X + (end.X - start.X) * amount,
            start.Y + (end.Y - start.Y) * amount,
            start.Z + (end.Z - start.Z) * amount);

    /// <summary>
    /// Returns the component-wise minimum of two vectors.
    /// </summary>
    public static Vector3 Min(Vector3 a, Vector3 b)
        => new(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y), MathF.Min(a.Z, b.Z));

    /// <summary>
    /// Returns the component-wise maximum of two vectors.
    /// </summary>
    public static Vector3 Max(Vector3 a, Vector3 b)
        => new(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y), MathF.Max(a.Z, b.Z));

    /// <summary>
    /// Transforms a vector by a matrix (DirectX compatible - row-vector convention).
    /// </summary>
    public static Vector3 TransformCoordinate(Vector3 coordinate, Matrix matrix)
    {
        float w = coordinate.X * matrix.M14 + coordinate.Y * matrix.M24 + coordinate.Z * matrix.M34 + matrix.M44;
        return new Vector3(
            (coordinate.X * matrix.M11 + coordinate.Y * matrix.M21 + coordinate.Z * matrix.M31 + matrix.M41) / w,
            (coordinate.X * matrix.M12 + coordinate.Y * matrix.M22 + coordinate.Z * matrix.M32 + matrix.M42) / w,
            (coordinate.X * matrix.M13 + coordinate.Y * matrix.M23 + coordinate.Z * matrix.M33 + matrix.M43) / w);
    }

    /// <summary>
    /// Transforms a normal vector by a matrix (ignores translation).
    /// </summary>
    public static Vector3 TransformNormal(Vector3 normal, Matrix matrix)
        => new(
            normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31,
            normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32,
            normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33);

    // Operators
    public static Vector3 operator +(Vector3 left, Vector3 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    public static Vector3 operator -(Vector3 left, Vector3 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
    public static Vector3 operator *(Vector3 left, float right) => new(left.X * right, left.Y * right, left.Z * right);
    public static Vector3 operator *(float left, Vector3 right) => new(left * right.X, left * right.Y, left * right.Z);
    public static Vector3 operator /(Vector3 left, float right) => new(left.X / right, left.Y / right, left.Z / right);
    public static Vector3 operator -(Vector3 value) => new(-value.X, -value.Y, -value.Z);
    public static bool operator ==(Vector3 left, Vector3 right) => left.Equals(right);
    public static bool operator !=(Vector3 left, Vector3 right) => !left.Equals(right);

    // Conversions to/from System.Numerics
    public static implicit operator System.Numerics.Vector3(Vector3 v) => new(v.X, v.Y, v.Z);
    public static implicit operator Vector3(System.Numerics.Vector3 v) => new(v.X, v.Y, v.Z);

    public bool Equals(Vector3 other) => X == other.X && Y == other.Y && Z == other.Z;
    public override bool Equals(object? obj) => obj is Vector3 v && Equals(v);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public override string ToString() => $"({X}, {Y}, {Z})";
}
