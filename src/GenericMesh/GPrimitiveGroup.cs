using GenericMesh.Exceptions;

namespace GenericMesh;

/// <summary>
/// The primitive group of generic mesh is represented using this class.
/// </summary>
/// <typeparam name="TIndex">The type of indices this primitive uses (must be IConvertible).</typeparam>
public sealed class GPrimitiveGroup<TIndex> : ICloneable
    where TIndex : struct, IConvertible
{
    private PrimitiveType _type;
    private List<TIndex> _indices;

    /// <summary>Class constructor.</summary>
    public GPrimitiveGroup()
    {
        _indices = new List<TIndex>();
    }

    /// <summary>Class constructor with initial index data.</summary>
    public GPrimitiveGroup(TIndex[] indexData) : this()
    {
        Append(indexData);
    }

    /// <summary>Copy constructor (deep copy).</summary>
    public GPrimitiveGroup(GPrimitiveGroup<TIndex>? other) : this()
    {
        if (other != null)
            other.CopyTo(this);
    }

    /// <summary>Gets or sets the primitive type.</summary>
    public PrimitiveType Type
    {
        get => _type;
        set
        {
            if (value < PrimitiveType.PointList || value > PrimitiveType.TriangleFan)
                throw new ArgumentException("Invalid primitive type.", nameof(value));
            _type = value;
        }
    }

    /// <summary>Gets or sets the number of indices.</summary>
    public int IndiceCount
    {
        get => _indices.Count;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            int oldSize = _indices.Count;
            if (value < oldSize)
            {
                _indices.RemoveRange(value, oldSize - value);
            }
            else
            {
                for (int i = oldSize; i < value; i++)
                    _indices.Add(default);
            }
        }
    }

    /// <summary>Gets or sets the primitive count.</summary>
    public int PrimitiveCount
    {
        get
        {
            if (_indices.Count == 0)
                return 0;

            return _type switch
            {
                PrimitiveType.PointList => _indices.Count,
                PrimitiveType.LineList => _indices.Count / 2,
                PrimitiveType.LineStrip => _indices.Count - 1,
                PrimitiveType.TriangleList => _indices.Count / 3,
                PrimitiveType.TriangleStrip => _indices.Count - 2,
                PrimitiveType.TriangleFan => _indices.Count - 2,
                _ => throw new MeshPrimitiveTypeException("Primitive type not defined.")
            };
        }
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));

            if (value == 0)
            {
                IndiceCount = 0;
                return;
            }

            IndiceCount = _type switch
            {
                PrimitiveType.PointList => value,
                PrimitiveType.LineList => value * 2,
                PrimitiveType.LineStrip => value + 1,
                PrimitiveType.TriangleList => value * 3,
                PrimitiveType.TriangleStrip => value + 2,
                PrimitiveType.TriangleFan => value + 2,
                _ => throw new MeshPrimitiveTypeException("Primitive type not defined.")
            };
        }
    }

    /// <summary>The internal list of indices.</summary>
    internal List<TIndex> Indices => _indices;

    /// <summary>Gets or sets index data at the specified position.</summary>
    public TIndex this[int index]
    {
        get
        {
            if (index < 0 || index >= _indices.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _indices[index];
        }
        set
        {
            if (index < 0 || index >= _indices.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            _indices[index] = value;
        }
    }

    /// <summary>Returns the number of indices per primitive for list types.</summary>
    public int IndicesPerPrimitive => _type switch
    {
        PrimitiveType.PointList => 1,
        PrimitiveType.LineList or PrimitiveType.LineStrip => 2,
        PrimitiveType.TriangleList or PrimitiveType.TriangleStrip or PrimitiveType.TriangleFan => 3,
        _ => 0
    };

    /// <summary>Appends indices to the group.</summary>
    public bool Append(TIndex[]? indexData)
    {
        if (indexData == null || indexData.Length == 0)
            return false;

        _indices.AddRange(indexData);
        return true;
    }

    /// <summary>Appends another primitive group.</summary>
    public bool Append(GPrimitiveGroup<TIndex>? other)
    {
        if (other == null || other._indices.Count == 0)
            return false;

        if (ReferenceEquals(other, this))
            throw new InvalidOperationException("Cannot append to self.");

        Validate();
        other.Validate();

        if (IndicesPerPrimitive != other.IndicesPerPrimitive)
            throw new MeshPrimitiveTypeException("IndicesPerPrimitive not same for primitive groups.");

        // Convert to list type if needed
        if (_type is PrimitiveType.LineStrip or PrimitiveType.TriangleStrip or PrimitiveType.TriangleFan)
            ConvertToList();

        var otherToAppend = other;
        if (other._type is PrimitiveType.LineStrip or PrimitiveType.TriangleStrip or PrimitiveType.TriangleFan)
        {
            otherToAppend = new GPrimitiveGroup<TIndex>(other);
            otherToAppend.ConvertToList();
        }

        _indices.AddRange(otherToAppend._indices);
        return true;
    }

    /// <summary>Copies indices to an array.</summary>
    public bool CopyTo(TIndex[]? destination, int sourceIndex = 0, int destinationIndex = 0, int length = -1)
    {
        if (_indices.Count == 0)
            return false;

        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        if (sourceIndex < 0 || sourceIndex >= _indices.Count)
            throw new ArgumentOutOfRangeException(nameof(sourceIndex));

        if (destinationIndex < 0 || destinationIndex >= destination.Length)
            throw new ArgumentOutOfRangeException(nameof(destinationIndex));

        if (length == -1)
            length = Math.Min(_indices.Count - sourceIndex, destination.Length - destinationIndex);

        if (length <= 0 || length > _indices.Count - sourceIndex || length > destination.Length - destinationIndex)
            throw new ArgumentOutOfRangeException(nameof(length));

        _indices.CopyTo(sourceIndex, destination, destinationIndex, length);
        return true;
    }

    /// <summary>Copies this primitive group to another.</summary>
    public bool CopyTo(GPrimitiveGroup<TIndex>? other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        if (ReferenceEquals(other, this))
            throw new InvalidOperationException("Cannot copy to self.");

        other._indices.Clear();
        other._type = _type;
        other._indices.AddRange(_indices);
        return true;
    }

    /// <summary>Removes indices starting at index.</summary>
    public bool Remove(int index, int removeCount = 1)
    {
        if (_indices.Count == 0)
            return false;

        if (index < 0 || index >= _indices.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (removeCount <= 0 || removeCount > _indices.Count || index + removeCount > _indices.Count)
            throw new ArgumentOutOfRangeException(nameof(removeCount));

        _indices.RemoveRange(index, removeCount);
        return true;
    }

    /// <summary>Removes indices in the specified range (inclusive).</summary>
    public bool RemoveRange(int startIndex, int endIndex)
    {
        if (_indices.Count == 0)
            return false;

        if (startIndex < 0 || startIndex >= _indices.Count)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        if (endIndex < 0 || endIndex >= _indices.Count)
            throw new ArgumentOutOfRangeException(nameof(endIndex));

        if (startIndex > endIndex)
            throw new ArgumentOutOfRangeException(nameof(startIndex));

        _indices.RemoveRange(startIndex, endIndex - startIndex + 1);
        return true;
    }

    /// <summary>Removes all indices.</summary>
    public bool RemoveAll()
    {
        _indices.Clear();
        return true;
    }

    /// <summary>Clones this primitive group (deep copy).</summary>
    public object Clone() => new GPrimitiveGroup<TIndex>(this);

    /// <summary>Converts strip/fan primitives to list format.</summary>
    public bool ConvertToList()
    {
        if (_indices.Count == 0)
            return true;

        if (_type is PrimitiveType.TriangleList or PrimitiveType.LineList or PrimitiveType.PointList)
            return true;

        Validate();

        switch (_type)
        {
            case PrimitiveType.LineStrip:
            {
                var newIndices = new TIndex[2 * (_indices.Count - 1)];
                int j = 0;
                for (int i = 0; i < _indices.Count - 1; i++)
                {
                    newIndices[j] = _indices[i];
                    newIndices[j + 1] = _indices[i + 1];
                    j += 2;
                }
                _indices = new List<TIndex>(newIndices);
                _type = PrimitiveType.LineList;
                break;
            }
            case PrimitiveType.TriangleStrip:
            {
                var newIndices = new TIndex[3 * (_indices.Count - 2)];
                bool reverse = false;
                int j = 0;
                for (int i = 0; i < _indices.Count - 2; i++)
                {
                    if (reverse)
                    {
                        newIndices[j + 2] = _indices[i];
                        newIndices[j + 1] = _indices[i + 1];
                        newIndices[j] = _indices[i + 2];
                    }
                    else
                    {
                        newIndices[j] = _indices[i];
                        newIndices[j + 1] = _indices[i + 1];
                        newIndices[j + 2] = _indices[i + 2];
                    }
                    j += 3;
                    reverse = !reverse;
                }
                _indices = new List<TIndex>(newIndices);
                _type = PrimitiveType.TriangleList;
                break;
            }
            case PrimitiveType.TriangleFan:
            {
                var newIndices = new TIndex[3 * (_indices.Count - 2)];
                int j = 0;
                for (int i = 1; i < _indices.Count - 1; i++)
                {
                    newIndices[j] = _indices[0];
                    newIndices[j + 1] = _indices[i];
                    newIndices[j + 2] = _indices[i + 1];
                    j += 3;
                }
                _indices = new List<TIndex>(newIndices);
                _type = PrimitiveType.TriangleList;
                break;
            }
        }

        return true;
    }

    /// <summary>Offsets all indices by the given amount.</summary>
    public bool Offset(int offsetValue)
    {
        if (_indices.Count == 0)
            return false;

        for (int i = 0; i < _indices.Count; i++)
        {
            int current = _indices[i].ToInt32(null);
            _indices[i] = (TIndex)Convert.ChangeType(current + offsetValue, typeof(TIndex));
        }

        return true;
    }

    /// <summary>Validates that indices match the primitive type requirements.</summary>
    public void Validate()
    {
        if (_indices.Count == 0)
            return;

        int required = _type switch
        {
            PrimitiveType.PointList => 1,
            PrimitiveType.LineList => 2,
            PrimitiveType.LineStrip => 2,
            PrimitiveType.TriangleList => 3,
            PrimitiveType.TriangleStrip => 3,
            PrimitiveType.TriangleFan => 3,
            _ => throw new MeshPrimitiveTypeException("Primitive type not defined.")
        };

        if (_indices.Count < required)
            throw new MeshPrimitiveTypeException($"Not enough indices for primitive type. Need at least {required}.");
    }
}
