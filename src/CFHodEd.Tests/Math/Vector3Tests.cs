using CFHodEd.Math;
using Xunit;

namespace CFHodEd.Tests.Math;

public class Vector3Tests
{
    [Fact]
    public void Zero_HasAllZeroComponents()
    {
        var v = Vector3.Zero;
        Assert.Equal(0f, v.X);
        Assert.Equal(0f, v.Y);
        Assert.Equal(0f, v.Z);
    }

    [Fact]
    public void Constructor_SetsComponents()
    {
        var v = new Vector3(1f, 2f, 3f);
        Assert.Equal(1f, v.X);
        Assert.Equal(2f, v.Y);
        Assert.Equal(3f, v.Z);
    }

    [Fact]
    public void Length_OfUnitX_IsOne()
    {
        Assert.Equal(1f, Vector3.UnitX.Length(), 5);
    }

    [Fact]
    public void Length_PythagoreanTriple()
    {
        var v = new Vector3(3f, 4f, 0f);
        Assert.Equal(5f, v.Length(), 5);
    }

    [Fact]
    public void Normalize_ReturnsUnitVector()
    {
        var v = new Vector3(3f, 0f, 0f);
        var n = Vector3.Normalize(v);
        Assert.Equal(1f, n.Length(), 5);
        Assert.Equal(1f, n.X, 5);
    }

    [Fact]
    public void Normalize_ZeroVector_ReturnsZero()
    {
        var n = Vector3.Normalize(Vector3.Zero);
        Assert.Equal(0f, n.X);
        Assert.Equal(0f, n.Y);
        Assert.Equal(0f, n.Z);
    }

    [Fact]
    public void Dot_OrthogonalVectors_IsZero()
    {
        Assert.Equal(0f, Vector3.Dot(Vector3.UnitX, Vector3.UnitY), 6);
    }

    [Fact]
    public void Dot_ParallelVectors_IsProduct()
    {
        var a = new Vector3(2f, 0f, 0f);
        var b = new Vector3(3f, 0f, 0f);
        Assert.Equal(6f, Vector3.Dot(a, b), 6);
    }

    [Fact]
    public void Cross_UnitXByUnitY_IsUnitZ()
    {
        var result = Vector3.Cross(Vector3.UnitX, Vector3.UnitY);
        Assert.Equal(0f, result.X, 5);
        Assert.Equal(0f, result.Y, 5);
        Assert.Equal(1f, result.Z, 5);
    }

    [Fact]
    public void Cross_IsAnticommutative()
    {
        var a = new Vector3(1f, 2f, 3f);
        var b = new Vector3(4f, 5f, 6f);
        var ab = Vector3.Cross(a, b);
        var ba = Vector3.Cross(b, a);
        Assert.Equal(-ab.X, ba.X, 5);
        Assert.Equal(-ab.Y, ba.Y, 5);
        Assert.Equal(-ab.Z, ba.Z, 5);
    }

    [Fact]
    public void Addition_Operator()
    {
        var result = new Vector3(1f, 2f, 3f) + new Vector3(4f, 5f, 6f);
        Assert.Equal(5f, result.X);
        Assert.Equal(7f, result.Y);
        Assert.Equal(9f, result.Z);
    }

    [Fact]
    public void Subtraction_Operator()
    {
        var result = new Vector3(4f, 5f, 6f) - new Vector3(1f, 2f, 3f);
        Assert.Equal(3f, result.X);
        Assert.Equal(3f, result.Y);
        Assert.Equal(3f, result.Z);
    }

    [Fact]
    public void Multiply_ScalarOperator()
    {
        var result = new Vector3(1f, 2f, 3f) * 2f;
        Assert.Equal(2f, result.X);
        Assert.Equal(4f, result.Y);
        Assert.Equal(6f, result.Z);
    }

    [Fact]
    public void Negate_Operator()
    {
        var v = new Vector3(1f, -2f, 3f);
        var neg = -v;
        Assert.Equal(-1f, neg.X);
        Assert.Equal(2f, neg.Y);
        Assert.Equal(-3f, neg.Z);
    }

    [Fact]
    public void Lerp_AtZero_ReturnsStart()
    {
        var result = Vector3.Lerp(new Vector3(0f, 0f, 0f), new Vector3(1f, 2f, 3f), 0f);
        Assert.Equal(0f, result.X, 6);
        Assert.Equal(0f, result.Y, 6);
        Assert.Equal(0f, result.Z, 6);
    }

    [Fact]
    public void Lerp_AtOne_ReturnsEnd()
    {
        var result = Vector3.Lerp(new Vector3(0f, 0f, 0f), new Vector3(1f, 2f, 3f), 1f);
        Assert.Equal(1f, result.X, 6);
        Assert.Equal(2f, result.Y, 6);
        Assert.Equal(3f, result.Z, 6);
    }

    [Fact]
    public void Lerp_AtHalf_ReturnsMidpoint()
    {
        var result = Vector3.Lerp(new Vector3(0f, 0f, 0f), new Vector3(2f, 4f, 6f), 0.5f);
        Assert.Equal(1f, result.X, 6);
        Assert.Equal(2f, result.Y, 6);
        Assert.Equal(3f, result.Z, 6);
    }

    [Fact]
    public void TransformCoordinate_ByTranslation()
    {
        var result = Vector3.TransformCoordinate(Vector3.Zero, Matrix.Translation(1f, 2f, 3f));
        Assert.Equal(1f, result.X, 5);
        Assert.Equal(2f, result.Y, 5);
        Assert.Equal(3f, result.Z, 5);
    }

    [Fact]
    public void TransformNormal_IgnoresTranslation()
    {
        var n = Vector3.UnitX;
        var result = Vector3.TransformNormal(n, Matrix.Translation(100f, 100f, 100f));
        Assert.Equal(1f, result.X, 5);
        Assert.Equal(0f, result.Y, 5);
        Assert.Equal(0f, result.Z, 5);
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        Assert.Equal(new Vector3(1f, 2f, 3f), new Vector3(1f, 2f, 3f));
    }

    [Fact]
    public void SystemNumerics_Roundtrip()
    {
        var v = new Vector3(1f, 2f, 3f);
        System.Numerics.Vector3 sysV = v;
        Vector3 back = sysV;
        Assert.Equal(v, back);
    }
}
