using CFHodEd.Math;
using GenericMesh.VertexFields;

namespace GenericMesh;

/// <summary>
/// The vertex group of generic mesh is represented using this class.
/// </summary>
/// <typeparam name="TVertex">The type of vertices this vertex group uses.</typeparam>
public sealed class GVertexGroup<TVertex> : ICloneable
    where TVertex : struct, IVertex
{
    private List<TVertex> _vertices;

    /// <summary>Class constructor.</summary>
    public GVertexGroup()
    {
        _vertices = new List<TVertex>();
    }

    /// <summary>Class constructor with initial vertex data.</summary>
    public GVertexGroup(TVertex[] vertexData) : this()
    {
        Append(vertexData);
    }

    /// <summary>Copy constructor (deep copy).</summary>
    public GVertexGroup(GVertexGroup<TVertex>? other) : this()
    {
        if (other != null)
            other.CopyTo(this);
    }

    /// <summary>Returns the vertex format.</summary>
    public static VertexFormats VertexFormat
    {
        get
        {
            TVertex v = default;
            return v.Format;
        }
    }

    /// <summary>Returns the size of vertex in bytes.</summary>
    public static int VertexSize
    {
        get
        {
            TVertex v = default;
            return v.VertexSize;
        }
    }

    /// <summary>Gets or sets the number of vertices in the group.</summary>
    public int Count
    {
        get => _vertices.Count;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            int oldSize = _vertices.Count;
            if (value < oldSize)
            {
                _vertices.RemoveRange(value, oldSize - value);
            }
            else
            {
                for (int i = oldSize; i < value; i++)
                    _vertices.Add(new TVertex());
            }
        }
    }

    /// <summary>The internal list of vertices.</summary>
    internal List<TVertex> Vertices => _vertices;

    /// <summary>Gets or sets vertex data at the specified index.</summary>
    public TVertex this[int index]
    {
        get
        {
            if (index < 0 || index >= _vertices.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _vertices[index];
        }
        set
        {
            if (index < 0 || index >= _vertices.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            _vertices[index] = value;
        }
    }

    /// <summary>Appends vertices to the group.</summary>
    public bool Append(TVertex[]? vertexData)
    {
        if (vertexData == null || vertexData.Length == 0)
            return false;

        _vertices.AddRange(vertexData);
        return true;
    }

    /// <summary>Appends another vertex group to this one.</summary>
    public bool Append(GVertexGroup<TVertex>? other)
    {
        if (other == null || other._vertices.Count == 0)
            return false;

        if (ReferenceEquals(other, this))
            throw new InvalidOperationException("Cannot append to self.");

        _vertices.AddRange(other._vertices);
        return true;
    }

    /// <summary>Copies vertices to an array.</summary>
    public bool CopyTo(TVertex[]? destination, int sourceIndex = 0, int destinationIndex = 0, int length = -1)
    {
        if (_vertices.Count == 0)
            return false;

        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        if (sourceIndex < 0 || sourceIndex >= _vertices.Count)
            throw new ArgumentOutOfRangeException(nameof(sourceIndex));

        if (destinationIndex < 0 || destinationIndex >= destination.Length)
            throw new ArgumentOutOfRangeException(nameof(destinationIndex));

        if (length == -1)
            length = Math.Min(_vertices.Count - sourceIndex, destination.Length - destinationIndex);

        if (length <= 0 || length > _vertices.Count - sourceIndex || length > destination.Length - destinationIndex)
            throw new ArgumentOutOfRangeException(nameof(length));

        _vertices.CopyTo(sourceIndex, destination, destinationIndex, length);
        return true;
    }

    /// <summary>Copies this vertex group to another.</summary>
    public bool CopyTo(GVertexGroup<TVertex>? other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (ReferenceEquals(other, this))
            throw new InvalidOperationException("Cannot copy to self.");

        other._vertices.Clear();
        other._vertices.AddRange(_vertices);
        return true;
    }

    /// <summary>Removes vertices starting at index.</summary>
    public bool Remove(int index, int removeCount = 1)
    {
        if (_vertices.Count == 0)
            return false;

        if (index < 0 || index >= _vertices.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (removeCount <= 0 || removeCount > _vertices.Count || index + removeCount > _vertices.Count)
            throw new ArgumentOutOfRangeException(nameof(removeCount));

        _vertices.RemoveRange(index, removeCount);
        return true;
    }

    /// <summary>Removes vertices in the specified range (inclusive).</summary>
    public bool RemoveRange(int startIndex, int endIndex)
    {
        if (_vertices.Count == 0)
            return false;

        if (startIndex < 0 || startIndex >= _vertices.Count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        if (endIndex < 0 || endIndex >= _vertices.Count)
            throw new ArgumentOutOfRangeException(nameof(endIndex));

        if (startIndex > endIndex)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        _vertices.RemoveRange(startIndex, endIndex - startIndex + 1);
        return true;
    }

    /// <summary>Removes all vertices from this group.</summary>
    public bool RemoveAll()
    {
        _vertices.Clear();
        return true;
    }

    /// <summary>Clones this vertex group (deep copy).</summary>
    public object Clone() => new GVertexGroup<TVertex>(this);

    /// <summary>Transforms vertices using a matrix.</summary>
    public bool Transform(Matrix m, Func<TVertex, Matrix, TVertex>? transformer = null, int startIndex = 0, int count = -1)
    {
        if (startIndex < 0 || startIndex >= _vertices.Count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        if (count == -1)
            count = _vertices.Count - startIndex;

        if (count <= 0 || count > _vertices.Count || startIndex + count > _vertices.Count)
            throw new ArgumentOutOfRangeException(nameof(count));

        transformer ??= DefaultTransformVertex;

        for (int i = startIndex; i < startIndex + count; i++)
            _vertices[i] = transformer(_vertices[i], m);

        return true;
    }

    /// <summary>Transforms a range of vertices.</summary>
    public bool TransformRange(Matrix m, Func<TVertex, Matrix, TVertex> transformer, int startIndex, int endIndex)
    {
        if (startIndex < 0 || startIndex >= _vertices.Count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        if (endIndex < 0 || endIndex >= _vertices.Count)
            throw new ArgumentOutOfRangeException(nameof(endIndex));

        if (startIndex > endIndex)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        return Transform(m, transformer, startIndex, endIndex - startIndex + 1);
    }

    /// <summary>Gets the bounding box extents of the mesh.</summary>
    public bool GetMeshExtents(out Vector3 minExtents, out Vector3 maxExtents)
    {
        if (_vertices.Count == 0)
        {
            minExtents = new Vector3(1, 1, 1);
            maxExtents = new Vector3(-1, -1, -1);
            return false;
        }

        var first = _vertices[0];
        if (first is not IVertexPosition3 posVertex)
            throw new InvalidOperationException("Vertex type does not implement IVertexPosition3.");

        minExtents = posVertex.GetPosition3();
        maxExtents = minExtents;

        for (int i = 1; i < _vertices.Count; i++)
        {
            if (_vertices[i] is IVertexPosition3 v)
            {
                var pos = v.GetPosition3();
                minExtents = Vector3.Min(minExtents, pos);
                maxExtents = Vector3.Max(maxExtents, pos);
            }
        }

        return true;
    }

    private static TVertex DefaultTransformVertex(TVertex vertex, Matrix m)
    {
        if (vertex is IVertexTransformable transformable)
        {
            transformable.Transform(m);
            return vertex;
        }
        return vertex;
    }
}
