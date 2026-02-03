namespace HW2IFF;

/// <summary>
/// Single node in a handler list.
/// </summary>
internal struct HandlerNode
{
    /// <summary>Function to handle the chunk.</summary>
    public ChunkHandler? Handler;

    /// <summary>Type of chunk.</summary>
    public ChunkType Type;

    /// <summary>ID of the chunk.</summary>
    public string ID;

    /// <summary>Version of the chunk.</summary>
    public uint Version;
}
