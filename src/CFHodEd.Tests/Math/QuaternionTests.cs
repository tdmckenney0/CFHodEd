using CFHodEd.Math;
using Xunit;

namespace CFHodEd.Tests.Math;

public class QuaternionTests
{
    private const float Tol = 1e-5f;

    [Fact]
    public void Identity_HasCorrectComponents()
    {
        var q = Quaternion.Identity;
        Assert.Equal(0f, q.X);
        Assert.Equal(0f, q.Y);
        Assert.Equal(0f, q.Z);
        Assert.Equal(1f, q.W);
    }

    [Fact]
    public void Identity_LengthIsOne()
    {
        Assert.Equal(1f, Quaternion.Identity.Length(), 5);
    }

    [Fact]
    public void Normalize_ProducesUnitLength()
    {
        var n = Quaternion.Normalize(new Quaternion(1f, 2f, 3f, 4f));
        Assert.Equal(1f, n.Length(), 5);
    }

    [Fact]
    public void Normalize_UnitQuaternion_IsUnchanged()
    {
        var q = Quaternion.Identity;
        var n = Quaternion.Normalize(q);
        Assert.Equal(q.X, n.X, 6);
        Assert.Equal(q.Y, n.Y, 6);
        Assert.Equal(q.Z, n.Z, 6);
        Assert.Equal(q.W, n.W, 6);
    }

    [Fact]
    public void Conjugate_NegatesXYZ_KeepsW()
    {
        var c = Quaternion.Conjugate(new Quaternion(1f, 2f, 3f, 4f));
        Assert.Equal(-1f, c.X);
        Assert.Equal(-2f, c.Y);
        Assert.Equal(-3f, c.Z);
        Assert.Equal(4f, c.W);
    }

    [Fact]
    public void Multiply_ByIdentity_IsUnchanged()
    {
        var q = Quaternion.Normalize(new Quaternion(1f, 0f, 0f, 0f));
        var result = q * Quaternion.Identity;
        Assert.Equal(q.X, result.X, 6);
        Assert.Equal(q.Y, result.Y, 6);
        Assert.Equal(q.Z, result.Z, 6);
        Assert.Equal(q.W, result.W, 6);
    }

    [Fact]
    public void RotationAxis_90Deg_AboutX_HasCorrectComponents()
    {
        var q = Quaternion.RotationAxis(Vector3.UnitX, MathF.PI / 2);
        float expected = MathF.Sin(MathF.PI / 4);
        Assert.Equal(1f, q.Length(), 5);
        Assert.Equal(expected, q.X, 5);
        Assert.Equal(0f, q.Y, 5);
        Assert.Equal(0f, q.Z, 5);
    }

    [Fact]
    public void RotationAxis_ZeroAngle_IsIdentity()
    {
        var q = Quaternion.RotationAxis(Vector3.UnitX, 0f);
        Assert.Equal(Quaternion.Identity.X, q.X, 5);
        Assert.Equal(Quaternion.Identity.Y, q.Y, 5);
        Assert.Equal(Quaternion.Identity.Z, q.Z, 5);
        Assert.Equal(Quaternion.Identity.W, q.W, 5);
    }

    [Fact]
    public void Slerp_AtZero_ReturnsStart()
    {
        var start = Quaternion.Identity;
        var end = Quaternion.RotationAxis(Vector3.UnitY, MathF.PI / 2);
        var result = Quaternion.Slerp(start, end, 0f);
        Assert.Equal(start.X, result.X, 5);
        Assert.Equal(start.Y, result.Y, 5);
        Assert.Equal(start.Z, result.Z, 5);
        Assert.Equal(start.W, result.W, 5);
    }

    [Fact]
    public void Slerp_AtOne_ReturnsEnd()
    {
        var start = Quaternion.Identity;
        var end = Quaternion.Normalize(Quaternion.RotationAxis(Vector3.UnitY, MathF.PI / 2));
        var result = Quaternion.Slerp(start, end, 1f);
        Assert.Equal(end.X, result.X, 5);
        Assert.Equal(end.Y, result.Y, 5);
        Assert.Equal(end.Z, result.Z, 5);
        Assert.Equal(end.W, result.W, 5);
    }

    [Fact]
    public void RotationMatrix_RoundTrip_SameRotation()
    {
        var original = Quaternion.Normalize(Quaternion.RotationAxis(new Vector3(1f, 1f, 0f), MathF.PI / 3));
        var matrix = Matrix.RotationQuaternion(original);
        var reconstructed = Quaternion.Normalize(Quaternion.RotationMatrix(matrix));
        // q and -q represent the same rotation; test |dot| ≈ 1
        float dot = original.X * reconstructed.X + original.Y * reconstructed.Y +
                    original.Z * reconstructed.Z + original.W * reconstructed.W;
        Assert.True(MathF.Abs(MathF.Abs(dot) - 1f) < Tol, $"|dot| = {MathF.Abs(dot)}");
    }

    [Fact]
    public void SystemNumerics_Roundtrip()
    {
        var q = new Quaternion(1f, 2f, 3f, 4f);
        System.Numerics.Quaternion sysQ = q;
        Quaternion back = sysQ;
        Assert.Equal(q.X, back.X);
        Assert.Equal(q.Y, back.Y);
        Assert.Equal(q.Z, back.Z);
        Assert.Equal(q.W, back.W);
    }
}
