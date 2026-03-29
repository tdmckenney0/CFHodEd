using CFHodEd.Math;
using Xunit;

namespace CFHodEd.Tests.Math;

public class MatrixTests
{
    private const float Tol = 1e-5f;

    [Fact]
    public void Identity_HasCorrectDiagonal()
    {
        var m = Matrix.Identity;
        Assert.Equal(1f, m.M11); Assert.Equal(0f, m.M12); Assert.Equal(0f, m.M13); Assert.Equal(0f, m.M14);
        Assert.Equal(0f, m.M21); Assert.Equal(1f, m.M22); Assert.Equal(0f, m.M23); Assert.Equal(0f, m.M24);
        Assert.Equal(0f, m.M31); Assert.Equal(0f, m.M32); Assert.Equal(1f, m.M33); Assert.Equal(0f, m.M34);
        Assert.Equal(0f, m.M41); Assert.Equal(0f, m.M42); Assert.Equal(0f, m.M43); Assert.Equal(1f, m.M44);
    }

    [Fact]
    public void Multiply_ByIdentity_IsUnchanged()
    {
        var m = Matrix.Translation(1f, 2f, 3f);
        Assert.Equal(m, m * Matrix.Identity);
        Assert.Equal(m, Matrix.Identity * m);
    }

    [Fact]
    public void Translation_SetsRow4()
    {
        var m = Matrix.Translation(1f, 2f, 3f);
        Assert.Equal(1f, m.M41);
        Assert.Equal(2f, m.M42);
        Assert.Equal(3f, m.M43);
        Assert.Equal(1f, m.M44);
    }

    [Fact]
    public void Scaling_SetsDiagonal()
    {
        var m = Matrix.Scaling(2f, 3f, 4f);
        Assert.Equal(2f, m.M11);
        Assert.Equal(3f, m.M22);
        Assert.Equal(4f, m.M33);
        Assert.Equal(1f, m.M44);
    }

    [Fact]
    public void Scaling_Uniform()
    {
        var m = Matrix.Scaling(5f);
        Assert.Equal(5f, m.M11);
        Assert.Equal(5f, m.M22);
        Assert.Equal(5f, m.M33);
    }

    [Fact]
    public void RotationX_ZeroAngle_IsIdentity()
    {
        AssertApprox(Matrix.Identity, Matrix.RotationX(0f));
    }

    [Fact]
    public void RotationY_ZeroAngle_IsIdentity()
    {
        AssertApprox(Matrix.Identity, Matrix.RotationY(0f));
    }

    [Fact]
    public void RotationZ_ZeroAngle_IsIdentity()
    {
        AssertApprox(Matrix.Identity, Matrix.RotationZ(0f));
    }

    [Fact]
    public void RotationX_90Deg_RotatesY_To_NegZ()
    {
        // In LH row-major convention, RotationX(90°) maps +Y toward +Z
        var result = Vector3.TransformNormal(Vector3.UnitY, Matrix.RotationX(MathF.PI / 2));
        Assert.Equal(0f, result.X, 5);
        Assert.Equal(0f, result.Y, 5);
        Assert.Equal(1f, result.Z, 5);
    }

    [Fact]
    public void Transpose_SwapsOffDiagonal()
    {
        var m = new Matrix(
            1, 2, 3, 4,
            5, 6, 7, 8,
            9, 10, 11, 12,
            13, 14, 15, 16);
        var t = Matrix.Transpose(m);
        Assert.Equal(m.M12, t.M21);
        Assert.Equal(m.M13, t.M31);
        Assert.Equal(m.M21, t.M12);
        Assert.Equal(m.M34, t.M43);
    }

    [Fact]
    public void Transpose_DoubleTranspose_IsOriginal()
    {
        var m = Matrix.Translation(1f, 2f, 3f);
        Assert.Equal(m, Matrix.Transpose(Matrix.Transpose(m)));
    }

    [Fact]
    public void Invert_OfIdentity_IsIdentity()
    {
        AssertApprox(Matrix.Identity, Matrix.Invert(Matrix.Identity));
    }

    [Fact]
    public void Invert_TranslationMatrix_CancelsOut()
    {
        var m = Matrix.Translation(1f, 2f, 3f);
        AssertApprox(Matrix.Identity, m * Matrix.Invert(m));
    }

    [Fact]
    public void Translation_Accumulates_Via_Multiply()
    {
        // In row-major (DirectX) convention, transforms chain left-to-right
        var a = Matrix.Translation(1f, 0f, 0f);
        var b = Matrix.Translation(2f, 0f, 0f);
        var result = Vector3.TransformCoordinate(Vector3.Zero, a * b);
        Assert.Equal(3f, result.X, 5);
    }

    [Fact]
    public void RotationQuaternion_Identity_IsIdentityMatrix()
    {
        var m = Matrix.RotationQuaternion(Quaternion.Identity);
        AssertApprox(Matrix.Identity, m);
    }

    [Fact]
    public void SystemNumerics_Roundtrip()
    {
        var m = Matrix.Translation(1f, 2f, 3f);
        System.Numerics.Matrix4x4 sysM = m;
        Matrix back = sysM;
        Assert.Equal(m, back);
    }

    private static void AssertApprox(Matrix expected, Matrix actual)
    {
        Assert.True(MathF.Abs(expected.M11 - actual.M11) < Tol, $"M11: {expected.M11} != {actual.M11}");
        Assert.True(MathF.Abs(expected.M12 - actual.M12) < Tol, $"M12: {expected.M12} != {actual.M12}");
        Assert.True(MathF.Abs(expected.M13 - actual.M13) < Tol);
        Assert.True(MathF.Abs(expected.M14 - actual.M14) < Tol);
        Assert.True(MathF.Abs(expected.M21 - actual.M21) < Tol);
        Assert.True(MathF.Abs(expected.M22 - actual.M22) < Tol);
        Assert.True(MathF.Abs(expected.M23 - actual.M23) < Tol);
        Assert.True(MathF.Abs(expected.M24 - actual.M24) < Tol);
        Assert.True(MathF.Abs(expected.M31 - actual.M31) < Tol);
        Assert.True(MathF.Abs(expected.M32 - actual.M32) < Tol);
        Assert.True(MathF.Abs(expected.M33 - actual.M33) < Tol);
        Assert.True(MathF.Abs(expected.M34 - actual.M34) < Tol);
        Assert.True(MathF.Abs(expected.M41 - actual.M41) < Tol, $"M41: {expected.M41} != {actual.M41}");
        Assert.True(MathF.Abs(expected.M42 - actual.M42) < Tol, $"M42: {expected.M42} != {actual.M42}");
        Assert.True(MathF.Abs(expected.M43 - actual.M43) < Tol, $"M43: {expected.M43} != {actual.M43}");
        Assert.True(MathF.Abs(expected.M44 - actual.M44) < Tol);
    }
}
