using System.Runtime.CompilerServices;

namespace CFHodEd.Math;

/// <summary>
/// A 4-component vector compatible with DirectX Vector4.
/// </summary>
public struct Vector4 : IEquatable<Vector4>
{
    public float X, Y, Z, W;

    public static readonly Vector4 Zero = new(0, 0, 0, 0);
    public static readonly Vector4 One = new(1, 1, 1, 1);
    public static readonly Vector4 UnitX = new(1, 0, 0, 0);
    public static readonly Vector4 UnitY = new(0, 1, 0, 0);
    public static readonly Vector4 UnitZ = new(0, 0, 1, 0);
    public static readonly Vector4 UnitW = new(0, 0, 0, 1);

    public Vector4(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }
    public Vector4(float value) { X = Y = Z = W = value; }
    public Vector4(Vector3 xyz, float w) { X = xyz.X; Y = xyz.Y; Z = xyz.Z; W = w; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float LengthSquared() => X * X + Y * Y + Z * Z + W * W;

    public void Normalize()
    {
        float len = Length();
        if (len > 0) { X /= len; Y /= len; Z /= len; W /= len; }
    }

    public static Vector4 Normalize(Vector4 value)
    {
        float len = value.Length();
        return len > 0 ? new Vector4(value.X / len, value.Y / len, value.Z / len, value.W / len) : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Vector4 left, Vector4 right)
        => left.X * right.X + left.Y * right.Y + left.Z * right.Z + left.W * right.W;

    public static Vector4 Lerp(Vector4 start, Vector4 end, float amount)
        => new(
            start.X + (end.X - start.X) * amount,
            start.Y + (end.Y - start.Y) * amount,
            start.Z + (end.Z - start.Z) * amount,
            start.W + (end.W - start.W) * amount);

    /// <summary>
    /// Transforms a vector by a matrix.
    /// </summary>
    public static Vector4 Transform(Vector4 vector, Matrix matrix)
        => new(
            vector.X * matrix.M11 + vector.Y * matrix.M21 + vector.Z * matrix.M31 + vector.W * matrix.M41,
            vector.X * matrix.M12 + vector.Y * matrix.M22 + vector.Z * matrix.M32 + vector.W * matrix.M42,
            vector.X * matrix.M13 + vector.Y * matrix.M23 + vector.Z * matrix.M33 + vector.W * matrix.M43,
            vector.X * matrix.M14 + vector.Y * matrix.M24 + vector.Z * matrix.M34 + vector.W * matrix.M44);

    // Operators
    public static Vector4 operator +(Vector4 left, Vector4 right) => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
    public static Vector4 operator -(Vector4 left, Vector4 right) => new(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
    public static Vector4 operator *(Vector4 left, float right) => new(left.X * right, left.Y * right, left.Z * right, left.W * right);
    public static Vector4 operator *(float left, Vector4 right) => new(left * right.X, left * right.Y, left * right.Z, left * right.W);
    public static Vector4 operator /(Vector4 left, float right) => new(left.X / right, left.Y / right, left.Z / right, left.W / right);
    public static Vector4 operator -(Vector4 value) => new(-value.X, -value.Y, -value.Z, -value.W);
    public static bool operator ==(Vector4 left, Vector4 right) => left.Equals(right);
    public static bool operator !=(Vector4 left, Vector4 right) => !left.Equals(right);

    // Conversions
    public static implicit operator System.Numerics.Vector4(Vector4 v) => new(v.X, v.Y, v.Z, v.W);
    public static implicit operator Vector4(System.Numerics.Vector4 v) => new(v.X, v.Y, v.Z, v.W);

    public bool Equals(Vector4 other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
    public override bool Equals(object? obj) => obj is Vector4 v && Equals(v);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
    public override string ToString() => $"({X}, {Y}, {Z}, {W})";
}
