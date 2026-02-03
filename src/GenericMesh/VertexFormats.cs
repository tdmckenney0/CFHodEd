namespace GenericMesh;

/// <summary>
/// Flexible Vertex Format flags - replaces Direct3D VertexFormats enum.
/// Defines which vertex components are present in a vertex structure.
/// </summary>
[Flags]
public enum VertexFormats
{
    /// <summary>No format specified.</summary>
    None = 0,

    /// <summary>Vertex has X, Y, Z position (untransformed).</summary>
    Position = 0x002,

    /// <summary>Vertex has X, Y, Z, W position (transformed).</summary>
    PositionRhw = 0x004,

    /// <summary>Vertex has a normal vector.</summary>
    Normal = 0x010,

    /// <summary>Vertex has diffuse color.</summary>
    Diffuse = 0x040,

    /// <summary>Vertex has specular color.</summary>
    Specular = 0x080,

    /// <summary>Vertex has one set of texture coordinates.</summary>
    Texture1 = 0x100,

    /// <summary>Vertex has two sets of texture coordinates.</summary>
    Texture2 = 0x200,

    /// <summary>Vertex has three sets of texture coordinates.</summary>
    Texture3 = 0x300,

    /// <summary>Vertex has four sets of texture coordinates.</summary>
    Texture4 = 0x400,

    /// <summary>Vertex has point size.</summary>
    PointSize = 0x020,

    /// <summary>Mask for texture count.</summary>
    TextureCountMask = 0xF00,

    /// <summary>Shift for texture count bits.</summary>
    TextureCountShift = 8,
}
