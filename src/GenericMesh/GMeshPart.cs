using CFHodEd.Math;
using GenericMesh.Exceptions;
using GenericMesh.VertexFields;

namespace GenericMesh;

/// <summary>
/// The part of the mesh class is represented using this class.
/// </summary>
/// <typeparam name="TVertex">Vertex format of the mesh part.</typeparam>
/// <typeparam name="TIndex">Index format of the mesh part.</typeparam>
/// <typeparam name="TMaterial">Type of material used by the mesh part.</typeparam>
public sealed class GMeshPart<TVertex, TIndex, TMaterial> : ICloneable
    where TVertex : struct, IVertex
    where TIndex : struct, IConvertible
    where TMaterial : struct, IMaterial
{
    private readonly GVertexGroup<TVertex> _vertices;
    private readonly List<GPrimitiveGroup<TIndex>> _primitiveGroups;
    private TMaterial _material;

    /// <summary>Class constructor.</summary>
    public GMeshPart()
    {
        _vertices = new GVertexGroup<TVertex>();
        _primitiveGroups = new List<GPrimitiveGroup<TIndex>>();
        _material = new TMaterial();
        _material.Initialize();
    }

    /// <summary>Copy constructor (deep copy).</summary>
    public GMeshPart(GMeshPart<TVertex, TIndex, TMaterial>? other) : this()
    {
        if (other != null)
            Append(other);
    }

    /// <summary>Gets the vertex group for this part.</summary>
    public GVertexGroup<TVertex> Vertices => _vertices;

    /// <summary>Gets or sets the material for this part.</summary>
    public TMaterial Material
    {
        get => _material;
        set => _material = value;
    }

    /// <summary>Gets or sets the number of primitive groups.</summary>
    public int PrimitiveGroupCount
    {
        get => _primitiveGroups.Count;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            int oldSize = _primitiveGroups.Count;
            if (value < oldSize)
            {
                _primitiveGroups.RemoveRange(value, oldSize - value);
            }
            else
            {
                for (int i = oldSize; i < value; i++)
                    _primitiveGroups.Add(new GPrimitiveGroup<TIndex>());
            }
        }
    }

    /// <summary>Gets the primitive group at the specified index.</summary>
    public GPrimitiveGroup<TIndex> PrimitiveGroups(int index)
    {
        if (index < 0 || index >= _primitiveGroups.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _primitiveGroups[index];
    }

    /// <summary>Access to internal primitive groups list.</summary>
    internal List<GPrimitiveGroup<TIndex>> PrimitiveGroupsList => _primitiveGroups;

    /// <summary>Adds a copy of a primitive group to this part.</summary>
    public bool AddPrimitiveGroup(GPrimitiveGroup<TIndex>? primitiveGroup)
    {
        if (primitiveGroup == null)
            return false;

        var copy = new GPrimitiveGroup<TIndex>(primitiveGroup);
        _primitiveGroups.Add(copy);
        return true;
    }

    /// <summary>Adds empty primitive groups.</summary>
    public bool AddPrimitiveGroup(int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count == 0)
            return false;

        for (int i = 0; i < count; i++)
            _primitiveGroups.Add(new GPrimitiveGroup<TIndex>());

        return true;
    }

    /// <summary>Appends another mesh part to this one.</summary>
    public bool Append(GMeshPart<TVertex, TIndex, TMaterial>? other)
    {
        if (other == null)
            return false;

        if (ReferenceEquals(other, this))
            throw new InvalidOperationException("Cannot append to self.");

        // Append vertices
        int indiceOffset = _vertices.Count;
        _vertices.Append(other._vertices);

        // Append primitive groups with shifted indices
        int pgOffset = _primitiveGroups.Count;
        foreach (var pg in other._primitiveGroups)
        {
            var copy = new GPrimitiveGroup<TIndex>(pg);
            copy.Offset(indiceOffset);
            _primitiveGroups.Add(copy);
        }

        // Merge list primitives
        MergeListPrimitives();

        return true;
    }

    /// <summary>Copies this mesh part to another.</summary>
    public bool CopyTo(GMeshPart<TVertex, TIndex, TMaterial>? other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (ReferenceEquals(other, this))
            throw new InvalidOperationException("Cannot copy to self.");

        // Clear and copy vertices
        other._vertices.RemoveAll();
        _vertices.CopyTo(other._vertices);

        // Clear and copy primitive groups
        other._primitiveGroups.Clear();
        foreach (var pg in _primitiveGroups)
            other._primitiveGroups.Add(new GPrimitiveGroup<TIndex>(pg));

        // Copy material
        other._material = _material;

        return true;
    }

    /// <summary>Removes primitive groups starting at index.</summary>
    public bool RemovePrimitiveGroup(int index, int removeCount = 1)
    {
        if (_primitiveGroups.Count == 0)
            return false;

        if (index < 0 || index >= _primitiveGroups.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (removeCount <= 0 || removeCount > _primitiveGroups.Count || index + removeCount > _primitiveGroups.Count)
            throw new ArgumentOutOfRangeException(nameof(removeCount));

        _primitiveGroups.RemoveRange(index, removeCount);
        return true;
    }

    /// <summary>Removes all primitive groups.</summary>
    public bool RemoveAllPrimitiveGroups()
    {
        _primitiveGroups.Clear();
        return true;
    }

    /// <summary>Clones this mesh part (deep copy).</summary>
    public object Clone() => new GMeshPart<TVertex, TIndex, TMaterial>(this);

    /// <summary>Transforms all vertices in this part.</summary>
    public bool Transform(Matrix m, Func<TVertex, Matrix, TVertex>? transformer = null)
    {
        return _vertices.Transform(m, transformer);
    }

    /// <summary>Merges consecutive list-type primitive groups of the same type.</summary>
    public void MergeListPrimitives()
    {
        if (_primitiveGroups.Count <= 1)
            return;

        var merged = new List<GPrimitiveGroup<TIndex>>();

        foreach (var pg in _primitiveGroups)
        {
            // Convert to list if strip/fan
            if (pg.Type is PrimitiveType.LineStrip or PrimitiveType.TriangleStrip or PrimitiveType.TriangleFan)
                pg.ConvertToList();

            // Try to merge with last
            if (merged.Count > 0)
            {
                var last = merged[^1];
                if (last.Type == pg.Type && last.IndicesPerPrimitive == pg.IndicesPerPrimitive)
                {
                    // Merge indices
                    for (int i = 0; i < pg.IndiceCount; i++)
                        last.Append(new[] { pg[i] });
                    continue;
                }
            }

            merged.Add(new GPrimitiveGroup<TIndex>(pg));
        }

        _primitiveGroups.Clear();
        _primitiveGroups.AddRange(merged);
    }

    /// <summary>Returns the total number of indices across all primitive groups.</summary>
    public int TotalIndiceCount => _primitiveGroups.Sum(pg => pg.IndiceCount);

    /// <summary>Returns the total number of primitives across all primitive groups.</summary>
    public int TotalPrimitiveCount => _primitiveGroups.Sum(pg => pg.PrimitiveCount);

    /// <summary>Gets the bounding box extents of this mesh part.</summary>
    public bool GetMeshExtents(out Vector3 minExtents, out Vector3 maxExtents)
    {
        return _vertices.GetMeshExtents(out minExtents, out maxExtents);
    }
}
