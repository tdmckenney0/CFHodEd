namespace CFHodEd.Math;

/// <summary>
/// A plane in 3D space, compatible with DirectX Plane.
/// Defined by the equation: Ax + By + Cz + D = 0
/// </summary>
public struct Plane : IEquatable<Plane>
{
    /// <summary>The normal vector of the plane (A, B, C).</summary>
    public float A, B, C;

    /// <summary>The distance from the origin along the normal (D).</summary>
    public float D;

    public Plane(float a, float b, float c, float d) { A = a; B = b; C = c; D = d; }

    public Plane(Vector3 normal, float d)
    {
        A = normal.X; B = normal.Y; C = normal.Z; D = d;
    }

    public Plane(Vector3 point, Vector3 normal)
    {
        normal = Vector3.Normalize(normal);
        A = normal.X; B = normal.Y; C = normal.Z;
        D = -Vector3.Dot(point, normal);
    }

    /// <summary>Creates a plane from three points.</summary>
    public static Plane FromPoints(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var v1 = p2 - p1;
        var v2 = p3 - p1;
        var normal = Vector3.Normalize(Vector3.Cross(v1, v2));
        return new Plane(p1, normal);
    }

    /// <summary>Gets the normal vector of the plane.</summary>
    public Vector3 Normal => new(A, B, C);

    /// <summary>Normalizes the plane coefficients.</summary>
    public void Normalize()
    {
        float len = MathF.Sqrt(A * A + B * B + C * C);
        if (len > 0)
        {
            A /= len; B /= len; C /= len; D /= len;
        }
    }

    /// <summary>Returns the signed distance from a point to the plane.</summary>
    public float DotCoordinate(Vector3 point)
        => A * point.X + B * point.Y + C * point.Z + D;

    /// <summary>Returns the dot product of the normal with a vector.</summary>
    public float DotNormal(Vector3 normal)
        => A * normal.X + B * normal.Y + C * normal.Z;

    /// <summary>Transforms a plane by a matrix.</summary>
    public static Plane Transform(Plane plane, Matrix matrix)
    {
        var invTranspose = Matrix.Transpose(Matrix.Invert(matrix));
        return new Plane(
            plane.A * invTranspose.M11 + plane.B * invTranspose.M21 + plane.C * invTranspose.M31 + plane.D * invTranspose.M41,
            plane.A * invTranspose.M12 + plane.B * invTranspose.M22 + plane.C * invTranspose.M32 + plane.D * invTranspose.M42,
            plane.A * invTranspose.M13 + plane.B * invTranspose.M23 + plane.C * invTranspose.M33 + plane.D * invTranspose.M43,
            plane.A * invTranspose.M14 + plane.B * invTranspose.M24 + plane.C * invTranspose.M34 + plane.D * invTranspose.M44);
    }

    public static bool operator ==(Plane left, Plane right) => left.Equals(right);
    public static bool operator !=(Plane left, Plane right) => !left.Equals(right);

    // Conversion to System.Numerics.Plane
    public static implicit operator System.Numerics.Plane(Plane p) => new(p.A, p.B, p.C, p.D);
    public static implicit operator Plane(System.Numerics.Plane p) => new(p.Normal.X, p.Normal.Y, p.Normal.Z, p.D);

    public bool Equals(Plane other) => A == other.A && B == other.B && C == other.C && D == other.D;
    public override bool Equals(object? obj) => obj is Plane p && Equals(p);
    public override int GetHashCode() => HashCode.Combine(A, B, C, D);
    public override string ToString() => $"Plane({A}x + {B}y + {C}z + {D} = 0)";
}
