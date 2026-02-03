using CFHodEd.Math;
using GenericMesh.Exceptions;
using GenericMesh.VertexFields;

namespace GenericMesh;

/// <summary>
/// The mesh class which can store 2D/3D objects.
/// </summary>
/// <typeparam name="TVertex">Vertex format of the mesh.</typeparam>
/// <typeparam name="TIndex">Index format of the mesh.</typeparam>
/// <typeparam name="TMaterial">Type of material used by the mesh.</typeparam>
public class GBasicMesh<TVertex, TIndex, TMaterial> : ICloneable
    where TVertex : struct, IVertex
    where TIndex : struct, IConvertible
    where TMaterial : struct, IMaterial
{
    private readonly List<GMeshPart<TVertex, TIndex, TMaterial>> _parts;
    private bool _locked;
    private int _baseVertexOffset;
    private int _baseIndiceOffset;

    /// <summary>Class constructor.</summary>
    public GBasicMesh()
    {
        _parts = new List<GMeshPart<TVertex, TIndex, TMaterial>>();
    }

    /// <summary>Copy constructor (deep copy).</summary>
    public GBasicMesh(GBasicMesh<TVertex, TIndex, TMaterial>? other) : this()
    {
        if (other != null)
            Append(other);
    }

    /// <summary>Gets or sets the number of mesh parts.</summary>
    public int PartCount
    {
        get => _parts.Count;
        set
        {
            if (_locked)
                throw new MeshLockedException();

            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            int oldSize = _parts.Count;
            if (value < oldSize)
            {
                _parts.RemoveRange(value, oldSize - value);
            }
            else
            {
                for (int i = oldSize; i < value; i++)
                    _parts.Add(new GMeshPart<TVertex, TIndex, TMaterial>());
            }
        }
    }

    /// <summary>Gets the mesh part at the specified index.</summary>
    public GMeshPart<TVertex, TIndex, TMaterial>? Part(int index)
    {
        if (_locked)
            return null;

        if (index < 0 || index >= _parts.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return _parts[index];
    }

    /// <summary>Gets or sets the material at the specified part index.</summary>
    public TMaterial Material(int index)
    {
        if (index < 0 || index >= _parts.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _parts[index].Material;
    }

    /// <summary>Sets the material at the specified part index.</summary>
    public void SetMaterial(int index, TMaterial material)
    {
        if (index < 0 || index >= _parts.Count)
            throw new ArgumentOutOfRangeException(nameof(index));
        _parts[index].Material = material;
    }

    /// <summary>Gets the total number of vertices across all parts.</summary>
    public int TotalVertexCount => _parts.Sum(p => p.Vertices.Count);

    /// <summary>Gets the total number of indices across all parts.</summary>
    public int TotalIndiceCount => _parts.Sum(p => p.TotalIndiceCount);

    /// <summary>Returns whether the mesh is locked.</summary>
    public bool IsLocked => _locked;

    /// <summary>Gets or sets the base vertex offset for external buffers.</summary>
    public int BaseVertexOffset
    {
        get => _baseVertexOffset;
        set => _baseVertexOffset = value;
    }

    /// <summary>Gets or sets the base index offset for external buffers.</summary>
    public int BaseIndiceOffset
    {
        get => _baseIndiceOffset;
        set => _baseIndiceOffset = value;
    }

    /// <summary>Access to internal parts list.</summary>
    internal List<GMeshPart<TVertex, TIndex, TMaterial>> PartsList => _parts;

    /// <summary>Adds a copy of a mesh part to this mesh.</summary>
    public bool Add(GMeshPart<TVertex, TIndex, TMaterial>? part)
    {
        if (_locked)
            throw new MeshLockedException();

        if (part == null)
            return false;

        var copy = new GMeshPart<TVertex, TIndex, TMaterial>(part);
        _parts.Add(copy);
        return true;
    }

    /// <summary>Adds empty mesh parts.</summary>
    public bool Add(int count)
    {
        if (_locked)
            throw new MeshLockedException();

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count == 0)
            return false;

        for (int i = 0; i < count; i++)
            _parts.Add(new GMeshPart<TVertex, TIndex, TMaterial>());

        return true;
    }

    /// <summary>Appends another mesh to this one.</summary>
    public bool Append(GBasicMesh<TVertex, TIndex, TMaterial>? other)
    {
        if (_locked)
            throw new MeshLockedException();

        if (other == null)
            return false;

        if (ReferenceEquals(other, this))
            throw new InvalidOperationException("Cannot append to self.");

        foreach (var part in other._parts)
            _parts.Add(new GMeshPart<TVertex, TIndex, TMaterial>(part));

        return true;
    }

    /// <summary>Copies this mesh to another.</summary>
    public bool CopyTo(GBasicMesh<TVertex, TIndex, TMaterial>? other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (other._locked)
            throw new MeshLockedException("Target mesh is locked.");

        if (ReferenceEquals(other, this))
            throw new InvalidOperationException("Cannot copy to self.");

        other._parts.Clear();
        foreach (var part in _parts)
            other._parts.Add(new GMeshPart<TVertex, TIndex, TMaterial>(part));

        return true;
    }

    /// <summary>Removes mesh parts starting at index.</summary>
    public bool Remove(int index, int removeCount = 1)
    {
        if (_locked)
            throw new MeshLockedException();

        if (_parts.Count == 0)
            return false;

        if (index < 0 || index >= _parts.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (removeCount <= 0 || removeCount > _parts.Count || index + removeCount > _parts.Count)
            throw new ArgumentOutOfRangeException(nameof(removeCount));

        _parts.RemoveRange(index, removeCount);
        return true;
    }

    /// <summary>Removes all mesh parts.</summary>
    public bool RemoveAll()
    {
        if (_locked)
            throw new MeshLockedException();

        _parts.Clear();
        return true;
    }

    /// <summary>Clones this mesh (deep copy).</summary>
    public object Clone() => new GBasicMesh<TVertex, TIndex, TMaterial>(this);

    /// <summary>Locks the mesh (prevents modifications).</summary>
    public void Lock()
    {
        _locked = true;
    }

    /// <summary>Unlocks the mesh (allows modifications).</summary>
    public void Unlock()
    {
        _locked = false;
    }

    /// <summary>Transforms all vertices in all parts.</summary>
    public bool Transform(Matrix m, Func<TVertex, Matrix, TVertex>? transformer = null)
    {
        if (_locked)
            throw new MeshLockedException();

        foreach (var part in _parts)
            part.Transform(m, transformer);

        return true;
    }

    /// <summary>Gets the bounding box extents of the entire mesh.</summary>
    public bool GetMeshExtents(out Vector3 minExtents, out Vector3 maxExtents)
    {
        if (_parts.Count == 0)
        {
            minExtents = new Vector3(1, 1, 1);
            maxExtents = new Vector3(-1, -1, -1);
            return false;
        }

        // Get first part's extents
        if (!_parts[0].GetMeshExtents(out minExtents, out maxExtents))
        {
            minExtents = new Vector3(1, 1, 1);
            maxExtents = new Vector3(-1, -1, -1);
            return false;
        }

        // Merge with other parts
        for (int i = 1; i < _parts.Count; i++)
        {
            if (_parts[i].GetMeshExtents(out var partMin, out var partMax))
            {
                minExtents = Vector3.Min(minExtents, partMin);
                maxExtents = Vector3.Max(maxExtents, partMax);
            }
        }

        return true;
    }

    /// <summary>Merges all parts into a single part.</summary>
    public bool MergeAllParts()
    {
        if (_locked)
            throw new MeshLockedException();

        if (_parts.Count <= 1)
            return true;

        var merged = new GMeshPart<TVertex, TIndex, TMaterial>();
        foreach (var part in _parts)
            merged.Append(part);

        _parts.Clear();
        _parts.Add(merged);
        return true;
    }
}
