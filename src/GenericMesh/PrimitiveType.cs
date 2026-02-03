namespace GenericMesh;

/// <summary>
/// Primitive type for rendering - replaces Direct3D PrimitiveType.
/// </summary>
public enum PrimitiveType
{
    /// <summary>Render as a point list.</summary>
    PointList = 1,

    /// <summary>Render as a line list.</summary>
    LineList = 2,

    /// <summary>Render as a line strip.</summary>
    LineStrip = 3,

    /// <summary>Render as a triangle list.</summary>
    TriangleList = 4,

    /// <summary>Render as a triangle strip.</summary>
    TriangleStrip = 5,

    /// <summary>Render as a triangle fan.</summary>
    TriangleFan = 6,
}
