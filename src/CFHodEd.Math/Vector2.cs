using System.Runtime.CompilerServices;

namespace CFHodEd.Math;

/// <summary>
/// A 2-component vector compatible with DirectX Vector2.
/// </summary>
public struct Vector2 : IEquatable<Vector2>
{
    public float X, Y;

    public static readonly Vector2 Zero = new(0, 0);
    public static readonly Vector2 One = new(1, 1);
    public static readonly Vector2 UnitX = new(1, 0);
    public static readonly Vector2 UnitY = new(0, 1);

    public Vector2(float x, float y) { X = x; Y = y; }
    public Vector2(float value) { X = Y = value; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Length() => MathF.Sqrt(X * X + Y * Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float LengthSquared() => X * X + Y * Y;

    public void Normalize()
    {
        float len = Length();
        if (len > 0) { X /= len; Y /= len; }
    }

    public static Vector2 Normalize(Vector2 value)
    {
        float len = value.Length();
        return len > 0 ? new Vector2(value.X / len, value.Y / len) : value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Dot(Vector2 left, Vector2 right) => left.X * right.X + left.Y * right.Y;

    public static float Distance(Vector2 a, Vector2 b) => (a - b).Length();

    public static Vector2 Lerp(Vector2 start, Vector2 end, float amount)
        => new(start.X + (end.X - start.X) * amount, start.Y + (end.Y - start.Y) * amount);

    public static Vector2 Min(Vector2 a, Vector2 b) => new(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y));
    public static Vector2 Max(Vector2 a, Vector2 b) => new(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y));

    // Operators
    public static Vector2 operator +(Vector2 left, Vector2 right) => new(left.X + right.X, left.Y + right.Y);
    public static Vector2 operator -(Vector2 left, Vector2 right) => new(left.X - right.X, left.Y - right.Y);
    public static Vector2 operator *(Vector2 left, float right) => new(left.X * right, left.Y * right);
    public static Vector2 operator *(float left, Vector2 right) => new(left * right.X, left * right.Y);
    public static Vector2 operator /(Vector2 left, float right) => new(left.X / right, left.Y / right);
    public static Vector2 operator -(Vector2 value) => new(-value.X, -value.Y);
    public static bool operator ==(Vector2 left, Vector2 right) => left.Equals(right);
    public static bool operator !=(Vector2 left, Vector2 right) => !left.Equals(right);

    // Conversions
    public static implicit operator System.Numerics.Vector2(Vector2 v) => new(v.X, v.Y);
    public static implicit operator Vector2(System.Numerics.Vector2 v) => new(v.X, v.Y);

    public bool Equals(Vector2 other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is Vector2 v && Equals(v);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"({X}, {Y})";
}
