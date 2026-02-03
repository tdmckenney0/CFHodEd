using System.Runtime.InteropServices;
using CFHodEd.Math;
using GenericMesh.VertexFields;

namespace GenericMesh.Standard;

/// <summary>
/// Untransformed and unlit vertex type containing position (XYZ only), normal, 
/// diffuse color and 1 set of texture coordinates.
/// </summary>
/// <remarks>
/// The default vertex type associated with the mesh class.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
public struct Vertex : IVertex, IVertexPosition3, IVertexNormal3, IVertexTex2, IVertexDiffuse, IVertexTransformable
{
    /// <summary>Position.</summary>
    public Vector3 Position;

    /// <summary>Normal.</summary>
    public Vector3 Normal;

    /// <summary>Diffuse color (ARGB packed).</summary>
    public int DiffuseARGB;

    /// <summary>Texture coordinates.</summary>
    public Vector2 TexCoords;

    /// <summary>
    /// Constructs a new vertex with given data.
    /// </summary>
    public Vertex(Vector3 position, Vector3 normal, Vector2 texCoords, int color = -1)
    {
        Position = position;
        Normal = normal;
        TexCoords = texCoords;
        DiffuseARGB = color;
    }

    /// <summary>
    /// Constructs a new vertex with given data.
    /// </summary>
    public Vertex(Vector3 position, Vector3 normal, Vector2 texCoords, ColorValue color)
    {
        Position = position;
        Normal = normal;
        TexCoords = texCoords;
        DiffuseARGB = color.ToArgb();
    }

    /// <summary>
    /// Returns/Sets the diffuse color.
    /// </summary>
    public ColorValue Diffuse
    {
        get => ColorValue.FromArgb(DiffuseARGB);
        set => DiffuseARGB = value.ToArgb();
    }

    /// <summary>
    /// FVF (Flexible Vertex Format) of the vertex.
    /// </summary>
    public VertexFormats Format =>
        VertexFormats.Position |
        VertexFormats.Normal |
        VertexFormats.Diffuse |
        VertexFormats.Texture1;

    /// <summary>
    /// Size of vertex in bytes.
    /// </summary>
    public int VertexSize => Marshal.SizeOf<Vertex>();

    /// <summary>
    /// Sets the default values for non-zero fields.
    /// </summary>
    public void Initialize()
    {
        Position = new Vector3(0, 0, 0);
        Normal = new Vector3(0, 0, 1);
        TexCoords = new Vector2(0, 0);
        DiffuseARGB = -1;
    }

    // IVertexPosition3 implementation
    Vector3 IVertexPosition3.GetPosition3() => Position;
    IVertex IVertexPosition3.SetPosition3(Vector3 v) { Position = v; return this; }

    // IVertexNormal3 implementation
    Vector3 IVertexNormal3.GetNormal3() => Normal;
    IVertex IVertexNormal3.SetNormal3(Vector3 v) { Normal = v; return this; }

    // IVertexTex2 implementation
    Vector2 IVertexTex2.GetTexCoords2(int index) => TexCoords;
    IVertex IVertexTex2.SetTexCoords2(Vector2 v, int index) { TexCoords = v; return this; }

    // IVertexDiffuse implementation
    ColorValue IVertexDiffuse.GetDiffuse() => Diffuse;
    IVertex IVertexDiffuse.SetDiffuse(ColorValue v) { Diffuse = v; return this; }

    // IVertexTransformable implementation
    IVertex IVertexTransformable.Transform(Matrix m)
    {
        Position = Vector3.TransformCoordinate(Position, m);
        Normal = Vector3.TransformNormal(Normal, m);
        return this;
    }

    // Operators
    public static bool operator ==(Vertex left, Vertex right) =>
        left.Position == right.Position &&
        left.Normal == right.Normal &&
        left.DiffuseARGB == right.DiffuseARGB &&
        left.TexCoords == right.TexCoords;

    public static bool operator !=(Vertex left, Vertex right) => !(left == right);

    // IEquatable<IVertex> implementation
    public bool Equals(IVertex? other)
    {
        if (other is not Vertex v)
            return false;
        return this == v;
    }

    public override bool Equals(object? obj) => obj is Vertex v && this == v;
    public override int GetHashCode() => HashCode.Combine(Position, Normal, DiffuseARGB, TexCoords);
}
