namespace CFHodEd.Math;

/// <summary>
/// An axis-aligned bounding box.
/// </summary>
public struct BoundingBox : IEquatable<BoundingBox>
{
    /// <summary>The minimum corner of the bounding box.</summary>
    public Vector3 Min;

    /// <summary>The maximum corner of the bounding box.</summary>
    public Vector3 Max;

    public BoundingBox(Vector3 min, Vector3 max) { Min = min; Max = max; }

    /// <summary>Gets the center of the bounding box.</summary>
    public Vector3 Center => (Min + Max) * 0.5f;

    /// <summary>Gets the size (extents) of the bounding box.</summary>
    public Vector3 Size => Max - Min;

    /// <summary>Gets half the size of the bounding box.</summary>
    public Vector3 HalfSize => (Max - Min) * 0.5f;

    /// <summary>Creates a bounding box from a collection of points.</summary>
    public static BoundingBox FromPoints(IEnumerable<Vector3> points)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var point in points)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }

        return new BoundingBox(min, max);
    }

    /// <summary>Creates the smallest bounding box that contains both input boxes.</summary>
    public static BoundingBox Merge(BoundingBox a, BoundingBox b)
        => new(Vector3.Min(a.Min, b.Min), Vector3.Max(a.Max, b.Max));

    /// <summary>Checks if a point is inside the bounding box.</summary>
    public bool Contains(Vector3 point)
        => point.X >= Min.X && point.X <= Max.X &&
           point.Y >= Min.Y && point.Y <= Max.Y &&
           point.Z >= Min.Z && point.Z <= Max.Z;

    /// <summary>Checks if another bounding box intersects with this one.</summary>
    public bool Intersects(BoundingBox other)
        => Min.X <= other.Max.X && Max.X >= other.Min.X &&
           Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
           Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;

    /// <summary>Expands the bounding box to include a point.</summary>
    public void ExpandToInclude(Vector3 point)
    {
        Min = Vector3.Min(Min, point);
        Max = Vector3.Max(Max, point);
    }

    /// <summary>Transforms the bounding box by a matrix.</summary>
    public static BoundingBox Transform(BoundingBox box, Matrix matrix)
    {
        // Transform all 8 corners and create a new AABB
        Vector3[] corners = new Vector3[8];
        corners[0] = new Vector3(box.Min.X, box.Min.Y, box.Min.Z);
        corners[1] = new Vector3(box.Max.X, box.Min.Y, box.Min.Z);
        corners[2] = new Vector3(box.Min.X, box.Max.Y, box.Min.Z);
        corners[3] = new Vector3(box.Max.X, box.Max.Y, box.Min.Z);
        corners[4] = new Vector3(box.Min.X, box.Min.Y, box.Max.Z);
        corners[5] = new Vector3(box.Max.X, box.Min.Y, box.Max.Z);
        corners[6] = new Vector3(box.Min.X, box.Max.Y, box.Max.Z);
        corners[7] = new Vector3(box.Max.X, box.Max.Y, box.Max.Z);

        for (int i = 0; i < 8; i++)
            corners[i] = Vector3.TransformCoordinate(corners[i], matrix);

        return FromPoints(corners);
    }

    public static bool operator ==(BoundingBox left, BoundingBox right) => left.Equals(right);
    public static bool operator !=(BoundingBox left, BoundingBox right) => !left.Equals(right);

    public bool Equals(BoundingBox other) => Min == other.Min && Max == other.Max;
    public override bool Equals(object? obj) => obj is BoundingBox b && Equals(b);
    public override int GetHashCode() => HashCode.Combine(Min, Max);
    public override string ToString() => $"BoundingBox(Min: {Min}, Max: {Max})";
}
