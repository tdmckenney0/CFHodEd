namespace HW2IFF;

/// <summary>
/// Type of IFF chunk.
/// </summary>
public enum ChunkType
{
    /// <summary>No special header.</summary>
    Default,

    /// <summary>Chunk in a 'NRML' header.</summary>
    Normal,

    /// <summary>Chunk in a 'FORM' header.</summary>
    Form
}

/// <summary>
/// Function to handle reading a chunk.
/// </summary>
/// <param name="reader">IFF Reader handling the chunk.</param>
/// <param name="chunkAttributes">Attributes of the chunk.</param>
public delegate void ChunkHandler(IFFReader reader, ChunkAttributes chunkAttributes);
