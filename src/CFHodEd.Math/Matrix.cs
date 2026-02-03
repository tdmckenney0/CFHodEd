using System.Runtime.CompilerServices;

namespace CFHodEd.Math;

/// <summary>
/// A 4x4 matrix compatible with DirectX Matrix.
/// Uses row-major layout matching DirectX conventions.
/// </summary>
public struct Matrix : IEquatable<Matrix>
{
    public float M11, M12, M13, M14;
    public float M21, M22, M23, M24;
    public float M31, M32, M33, M34;
    public float M41, M42, M43, M44;

    /// <summary>The identity matrix.</summary>
    public static readonly Matrix Identity = new(
        1, 0, 0, 0,
        0, 1, 0, 0,
        0, 0, 1, 0,
        0, 0, 0, 1);

    public Matrix(
        float m11, float m12, float m13, float m14,
        float m21, float m22, float m23, float m24,
        float m31, float m32, float m33, float m34,
        float m41, float m42, float m43, float m44)
    {
        M11 = m11; M12 = m12; M13 = m13; M14 = m14;
        M21 = m21; M22 = m22; M23 = m23; M24 = m24;
        M31 = m31; M32 = m32; M33 = m33; M34 = m34;
        M41 = m41; M42 = m42; M43 = m43; M44 = m44;
    }

    /// <summary>Creates a translation matrix.</summary>
    public static Matrix Translation(float x, float y, float z)
        => new(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, x, y, z, 1);

    /// <summary>Creates a translation matrix.</summary>
    public static Matrix Translation(Vector3 position)
        => Translation(position.X, position.Y, position.Z);

    /// <summary>Creates a scaling matrix.</summary>
    public static Matrix Scaling(float x, float y, float z)
        => new(x, 0, 0, 0, 0, y, 0, 0, 0, 0, z, 0, 0, 0, 0, 1);

    /// <summary>Creates a scaling matrix.</summary>
    public static Matrix Scaling(Vector3 scale)
        => Scaling(scale.X, scale.Y, scale.Z);

    /// <summary>Creates a uniform scaling matrix.</summary>
    public static Matrix Scaling(float scale)
        => Scaling(scale, scale, scale);

    /// <summary>Creates a rotation matrix around the X axis.</summary>
    public static Matrix RotationX(float angle)
    {
        float cos = MathF.Cos(angle), sin = MathF.Sin(angle);
        return new(1, 0, 0, 0, 0, cos, sin, 0, 0, -sin, cos, 0, 0, 0, 0, 1);
    }

    /// <summary>Creates a rotation matrix around the Y axis.</summary>
    public static Matrix RotationY(float angle)
    {
        float cos = MathF.Cos(angle), sin = MathF.Sin(angle);
        return new(cos, 0, -sin, 0, 0, 1, 0, 0, sin, 0, cos, 0, 0, 0, 0, 1);
    }

    /// <summary>Creates a rotation matrix around the Z axis.</summary>
    public static Matrix RotationZ(float angle)
    {
        float cos = MathF.Cos(angle), sin = MathF.Sin(angle);
        return new(cos, sin, 0, 0, -sin, cos, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1);
    }

    /// <summary>Creates a rotation matrix from a quaternion.</summary>
    public static Matrix RotationQuaternion(Quaternion q)
    {
        float xx = q.X * q.X, yy = q.Y * q.Y, zz = q.Z * q.Z;
        float xy = q.X * q.Y, xz = q.X * q.Z, yz = q.Y * q.Z;
        float wx = q.W * q.X, wy = q.W * q.Y, wz = q.W * q.Z;

        return new(
            1 - 2 * (yy + zz), 2 * (xy + wz), 2 * (xz - wy), 0,
            2 * (xy - wz), 1 - 2 * (xx + zz), 2 * (yz + wx), 0,
            2 * (xz + wy), 2 * (yz - wx), 1 - 2 * (xx + yy), 0,
            0, 0, 0, 1);
    }

    /// <summary>Creates a rotation matrix from yaw, pitch, and roll angles.</summary>
    public static Matrix RotationYawPitchRoll(float yaw, float pitch, float roll)
        => RotationZ(roll) * RotationX(pitch) * RotationY(yaw);

    /// <summary>Creates a left-handed look-at matrix.</summary>
    public static Matrix LookAtLH(Vector3 eye, Vector3 target, Vector3 up)
    {
        var zAxis = Vector3.Normalize(target - eye);
        var xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis));
        var yAxis = Vector3.Cross(zAxis, xAxis);

        return new(
            xAxis.X, yAxis.X, zAxis.X, 0,
            xAxis.Y, yAxis.Y, zAxis.Y, 0,
            xAxis.Z, yAxis.Z, zAxis.Z, 0,
            -Vector3.Dot(xAxis, eye), -Vector3.Dot(yAxis, eye), -Vector3.Dot(zAxis, eye), 1);
    }

    /// <summary>Creates a left-handed perspective projection matrix.</summary>
    public static Matrix PerspectiveFovLH(float fov, float aspect, float znear, float zfar)
    {
        float h = 1.0f / MathF.Tan(fov * 0.5f);
        float w = h / aspect;
        float q = zfar / (zfar - znear);

        return new(
            w, 0, 0, 0,
            0, h, 0, 0,
            0, 0, q, 1,
            0, 0, -znear * q, 0);
    }

    /// <summary>Creates an orthographic projection matrix.</summary>
    public static Matrix OrthoLH(float width, float height, float znear, float zfar)
    {
        float q = 1.0f / (zfar - znear);
        return new(
            2.0f / width, 0, 0, 0,
            0, 2.0f / height, 0, 0,
            0, 0, q, 0,
            0, 0, -znear * q, 1);
    }

    /// <summary>Transposes the matrix.</summary>
    public static Matrix Transpose(Matrix m)
        => new(
            m.M11, m.M21, m.M31, m.M41,
            m.M12, m.M22, m.M32, m.M42,
            m.M13, m.M23, m.M33, m.M43,
            m.M14, m.M24, m.M34, m.M44);

    /// <summary>Computes the inverse of a matrix.</summary>
    public static Matrix Invert(Matrix m)
    {
        // Convert to System.Numerics, invert, convert back
        var sysMatrix = (System.Numerics.Matrix4x4)m;
        if (System.Numerics.Matrix4x4.Invert(sysMatrix, out var inverted))
            return inverted;
        return Identity;
    }

    /// <summary>Gets the determinant of the matrix.</summary>
    public float Determinant()
    {
        var sysMatrix = (System.Numerics.Matrix4x4)this;
        return sysMatrix.GetDeterminant();
    }

    // Matrix multiplication
    public static Matrix operator *(Matrix left, Matrix right)
    {
        return new Matrix(
            left.M11 * right.M11 + left.M12 * right.M21 + left.M13 * right.M31 + left.M14 * right.M41,
            left.M11 * right.M12 + left.M12 * right.M22 + left.M13 * right.M32 + left.M14 * right.M42,
            left.M11 * right.M13 + left.M12 * right.M23 + left.M13 * right.M33 + left.M14 * right.M43,
            left.M11 * right.M14 + left.M12 * right.M24 + left.M13 * right.M34 + left.M14 * right.M44,

            left.M21 * right.M11 + left.M22 * right.M21 + left.M23 * right.M31 + left.M24 * right.M41,
            left.M21 * right.M12 + left.M22 * right.M22 + left.M23 * right.M32 + left.M24 * right.M42,
            left.M21 * right.M13 + left.M22 * right.M23 + left.M23 * right.M33 + left.M24 * right.M43,
            left.M21 * right.M14 + left.M22 * right.M24 + left.M23 * right.M34 + left.M24 * right.M44,

            left.M31 * right.M11 + left.M32 * right.M21 + left.M33 * right.M31 + left.M34 * right.M41,
            left.M31 * right.M12 + left.M32 * right.M22 + left.M33 * right.M32 + left.M34 * right.M42,
            left.M31 * right.M13 + left.M32 * right.M23 + left.M33 * right.M33 + left.M34 * right.M43,
            left.M31 * right.M14 + left.M32 * right.M24 + left.M33 * right.M34 + left.M34 * right.M44,

            left.M41 * right.M11 + left.M42 * right.M21 + left.M43 * right.M31 + left.M44 * right.M41,
            left.M41 * right.M12 + left.M42 * right.M22 + left.M43 * right.M32 + left.M44 * right.M42,
            left.M41 * right.M13 + left.M42 * right.M23 + left.M43 * right.M33 + left.M44 * right.M43,
            left.M41 * right.M14 + left.M42 * right.M24 + left.M43 * right.M34 + left.M44 * right.M44);
    }

    public static Matrix operator +(Matrix left, Matrix right)
        => new(
            left.M11 + right.M11, left.M12 + right.M12, left.M13 + right.M13, left.M14 + right.M14,
            left.M21 + right.M21, left.M22 + right.M22, left.M23 + right.M23, left.M24 + right.M24,
            left.M31 + right.M31, left.M32 + right.M32, left.M33 + right.M33, left.M34 + right.M34,
            left.M41 + right.M41, left.M42 + right.M42, left.M43 + right.M43, left.M44 + right.M44);

    public static Matrix operator -(Matrix left, Matrix right)
        => new(
            left.M11 - right.M11, left.M12 - right.M12, left.M13 - right.M13, left.M14 - right.M14,
            left.M21 - right.M21, left.M22 - right.M22, left.M23 - right.M23, left.M24 - right.M24,
            left.M31 - right.M31, left.M32 - right.M32, left.M33 - right.M33, left.M34 - right.M34,
            left.M41 - right.M41, left.M42 - right.M42, left.M43 - right.M43, left.M44 - right.M44);

    public static Matrix operator *(Matrix m, float s) => new(
        m.M11 * s, m.M12 * s, m.M13 * s, m.M14 * s,
        m.M21 * s, m.M22 * s, m.M23 * s, m.M24 * s,
        m.M31 * s, m.M32 * s, m.M33 * s, m.M34 * s,
        m.M41 * s, m.M42 * s, m.M43 * s, m.M44 * s);

    public static bool operator ==(Matrix left, Matrix right) => left.Equals(right);
    public static bool operator !=(Matrix left, Matrix right) => !left.Equals(right);

    // Conversions to/from System.Numerics.Matrix4x4
    public static implicit operator System.Numerics.Matrix4x4(Matrix m)
        => new(m.M11, m.M12, m.M13, m.M14,
               m.M21, m.M22, m.M23, m.M24,
               m.M31, m.M32, m.M33, m.M34,
               m.M41, m.M42, m.M43, m.M44);

    public static implicit operator Matrix(System.Numerics.Matrix4x4 m)
        => new(m.M11, m.M12, m.M13, m.M14,
               m.M21, m.M22, m.M23, m.M24,
               m.M31, m.M32, m.M33, m.M34,
               m.M41, m.M42, m.M43, m.M44);

    public bool Equals(Matrix other) =>
        M11 == other.M11 && M12 == other.M12 && M13 == other.M13 && M14 == other.M14 &&
        M21 == other.M21 && M22 == other.M22 && M23 == other.M23 && M24 == other.M24 &&
        M31 == other.M31 && M32 == other.M32 && M33 == other.M33 && M34 == other.M34 &&
        M41 == other.M41 && M42 == other.M42 && M43 == other.M43 && M44 == other.M44;

    public override bool Equals(object? obj) => obj is Matrix m && Equals(m);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(M11); hash.Add(M12); hash.Add(M13); hash.Add(M14);
        hash.Add(M21); hash.Add(M22); hash.Add(M23); hash.Add(M24);
        hash.Add(M31); hash.Add(M32); hash.Add(M33); hash.Add(M34);
        hash.Add(M41); hash.Add(M42); hash.Add(M43); hash.Add(M44);
        return hash.ToHashCode();
    }

    public override string ToString() =>
        $"[{M11}, {M12}, {M13}, {M14}]\n[{M21}, {M22}, {M23}, {M24}]\n[{M31}, {M32}, {M33}, {M34}]\n[{M41}, {M42}, {M43}, {M44}]";
}
