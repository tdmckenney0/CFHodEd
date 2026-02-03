using System.Runtime.CompilerServices;

namespace CFHodEd.Math;

/// <summary>
/// A quaternion compatible with DirectX Quaternion.
/// </summary>
public struct Quaternion : IEquatable<Quaternion>
{
    public float X, Y, Z, W;

    /// <summary>The identity quaternion (no rotation).</summary>
    public static readonly Quaternion Identity = new(0, 0, 0, 1);

    public Quaternion(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }

    /// <summary>Gets the length of the quaternion.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);

    /// <summary>Gets the squared length of the quaternion.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float LengthSquared() => X * X + Y * Y + Z * Z + W * W;

    /// <summary>Normalizes this quaternion in place.</summary>
    public void Normalize()
    {
        float len = Length();
        if (len > 0) { X /= len; Y /= len; Z /= len; W /= len; }
    }

    /// <summary>Returns a normalized copy of the quaternion.</summary>
    public static Quaternion Normalize(Quaternion q)
    {
        float len = q.Length();
        return len > 0 ? new Quaternion(q.X / len, q.Y / len, q.Z / len, q.W / len) : q;
    }

    /// <summary>Returns the conjugate of the quaternion.</summary>
    public static Quaternion Conjugate(Quaternion q) => new(-q.X, -q.Y, -q.Z, q.W);

    /// <summary>Returns the inverse of the quaternion.</summary>
    public static Quaternion Invert(Quaternion q)
    {
        float lenSq = q.LengthSquared();
        if (lenSq > 0)
        {
            float invLen = 1.0f / lenSq;
            return new Quaternion(-q.X * invLen, -q.Y * invLen, -q.Z * invLen, q.W * invLen);
        }
        return q;
    }

    /// <summary>Creates a quaternion from rotation around an axis.</summary>
    public static Quaternion RotationAxis(Vector3 axis, float angle)
    {
        axis = Vector3.Normalize(axis);
        float halfAngle = angle * 0.5f;
        float sin = MathF.Sin(halfAngle);
        return new Quaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, MathF.Cos(halfAngle));
    }

    /// <summary>Creates a quaternion from yaw, pitch, and roll angles.</summary>
    public static Quaternion RotationYawPitchRoll(float yaw, float pitch, float roll)
    {
        float halfYaw = yaw * 0.5f, halfPitch = pitch * 0.5f, halfRoll = roll * 0.5f;
        float cy = MathF.Cos(halfYaw), sy = MathF.Sin(halfYaw);
        float cp = MathF.Cos(halfPitch), sp = MathF.Sin(halfPitch);
        float cr = MathF.Cos(halfRoll), sr = MathF.Sin(halfRoll);

        return new Quaternion(
            cy * sp * cr + sy * cp * sr,
            sy * cp * cr - cy * sp * sr,
            cy * cp * sr - sy * sp * cr,
            cy * cp * cr + sy * sp * sr);
    }

    /// <summary>Creates a quaternion from a rotation matrix.</summary>
    public static Quaternion RotationMatrix(Matrix m)
    {
        float trace = m.M11 + m.M22 + m.M33;

        if (trace > 0)
        {
            float s = MathF.Sqrt(trace + 1.0f) * 2;
            return new Quaternion(
                (m.M23 - m.M32) / s,
                (m.M31 - m.M13) / s,
                (m.M12 - m.M21) / s,
                0.25f * s);
        }
        else if (m.M11 > m.M22 && m.M11 > m.M33)
        {
            float s = MathF.Sqrt(1.0f + m.M11 - m.M22 - m.M33) * 2;
            return new Quaternion(
                0.25f * s,
                (m.M12 + m.M21) / s,
                (m.M31 + m.M13) / s,
                (m.M23 - m.M32) / s);
        }
        else if (m.M22 > m.M33)
        {
            float s = MathF.Sqrt(1.0f + m.M22 - m.M11 - m.M33) * 2;
            return new Quaternion(
                (m.M12 + m.M21) / s,
                0.25f * s,
                (m.M23 + m.M32) / s,
                (m.M31 - m.M13) / s);
        }
        else
        {
            float s = MathF.Sqrt(1.0f + m.M33 - m.M11 - m.M22) * 2;
            return new Quaternion(
                (m.M31 + m.M13) / s,
                (m.M23 + m.M32) / s,
                0.25f * s,
                (m.M12 - m.M21) / s);
        }
    }

    /// <summary>Multiplies two quaternions.</summary>
    public static Quaternion Multiply(Quaternion left, Quaternion right)
        => new(
            left.W * right.X + left.X * right.W + left.Y * right.Z - left.Z * right.Y,
            left.W * right.Y - left.X * right.Z + left.Y * right.W + left.Z * right.X,
            left.W * right.Z + left.X * right.Y - left.Y * right.X + left.Z * right.W,
            left.W * right.W - left.X * right.X - left.Y * right.Y - left.Z * right.Z);

    /// <summary>Spherical linear interpolation between two quaternions.</summary>
    public static Quaternion Slerp(Quaternion start, Quaternion end, float amount)
    {
        float dot = start.X * end.X + start.Y * end.Y + start.Z * end.Z + start.W * end.W;

        if (dot < 0)
        {
            dot = -dot;
            end = new Quaternion(-end.X, -end.Y, -end.Z, -end.W);
        }

        if (dot > 0.9995f)
        {
            // Linear interpolation for very close quaternions
            return Normalize(new Quaternion(
                start.X + (end.X - start.X) * amount,
                start.Y + (end.Y - start.Y) * amount,
                start.Z + (end.Z - start.Z) * amount,
                start.W + (end.W - start.W) * amount));
        }

        float theta = MathF.Acos(dot);
        float sinTheta = MathF.Sin(theta);
        float a = MathF.Sin((1 - amount) * theta) / sinTheta;
        float b = MathF.Sin(amount * theta) / sinTheta;

        return new Quaternion(
            a * start.X + b * end.X,
            a * start.Y + b * end.Y,
            a * start.Z + b * end.Z,
            a * start.W + b * end.W);
    }

    // Operators
    public static Quaternion operator *(Quaternion left, Quaternion right) => Multiply(left, right);
    public static bool operator ==(Quaternion left, Quaternion right) => left.Equals(right);
    public static bool operator !=(Quaternion left, Quaternion right) => !left.Equals(right);

    // Conversions
    public static implicit operator System.Numerics.Quaternion(Quaternion q) => new(q.X, q.Y, q.Z, q.W);
    public static implicit operator Quaternion(System.Numerics.Quaternion q) => new(q.X, q.Y, q.Z, q.W);

    public bool Equals(Quaternion other) => X == other.X && Y == other.Y && Z == other.Z && W == other.W;
    public override bool Equals(object? obj) => obj is Quaternion q && Equals(q);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
    public override string ToString() => $"({X}, {Y}, {Z}, {W})";
}
