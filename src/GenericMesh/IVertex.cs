namespace GenericMesh;

/// <summary>
/// Defines the generalized properties that a structure implements to
/// create a suitable vertex type for the generic mesh class.
/// </summary>
public interface IVertex : IEquatable<IVertex>
{
    /// <summary>
    /// FVF (Flexible Vertex Format) of the vertex.
    /// </summary>
    VertexFormats Format { get; }

    /// <summary>
    /// Size of vertex in bytes.
    /// </summary>
    int VertexSize { get; }

    /// <summary>
    /// Initializes the vertex with default values.
    /// </summary>
    void Initialize();
}
