namespace HW2IFF;

/// <summary>
/// Chunk marker for using in an IFF writer chunk stack.
/// </summary>
internal struct ChunkMarker
{
    /// <summary>ID of chunk.</summary>
    public string ID;

    /// <summary>Type of chunk.</summary>
    public ChunkType Type;

    /// <summary>Position at which the chunk starts in file.</summary>
    public long StartPosition;
}
