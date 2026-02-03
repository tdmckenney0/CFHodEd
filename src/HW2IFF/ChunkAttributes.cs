namespace HW2IFF;

/// <summary>
/// Structure which stores chunk attributes.
/// </summary>
public readonly struct ChunkAttributes
{
    /// <summary>ID of chunk.</summary>
    public string ID { get; }

    /// <summary>Size of chunk.</summary>
    public int Size { get; }

    /// <summary>Type of chunk.</summary>
    public ChunkType Type { get; }

    /// <summary>Version of chunk.</summary>
    public uint Version { get; }

    /// <summary>
    /// Structure Initializer.
    /// </summary>
    /// <param name="id">ID of chunk.</param>
    /// <param name="size">Size of chunk.</param>
    /// <param name="type">Type of chunk.</param>
    /// <param name="version">Version of chunk.</param>
    internal ChunkAttributes(string id, int size, ChunkType type, uint version = 0)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentNullException(nameof(id));

        if (id.Length != 4)
            throw new ArgumentException("ID.Length != 4", nameof(id));

        if (type != ChunkType.Default && type != ChunkType.Normal && type != ChunkType.Form)
            throw new ArgumentOutOfRangeException(nameof(type));

        ID = id;
        Size = size;
        Type = type;
        Version = version;
    }
}
