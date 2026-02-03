using System.Diagnostics;

namespace HW2IFF;

/// <summary>
/// Class representing a single IFF chunk.
/// </summary>
internal struct Chunk
{
    /// <summary>Type of chunk.</summary>
    public ChunkType Type;

    /// <summary>ID of Chunk.</summary>
    public string ID;

    /// <summary>Version of chunk.</summary>
    public uint Version;

    /// <summary>Position where the chunk starts in file.</summary>
    public long StartPosition;

    /// <summary>Size of chunk.</summary>
    public long Size;

    private readonly IFFReader _iffReader;

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <param name="iffReader">IFF Reader this instance will be associated with.</param>
    public Chunk(IFFReader iffReader)
    {
        _iffReader = iffReader;
        Type = ChunkType.Default;
        ID = "    ";
        StartPosition = -1;
        Size = -1;
        Version = 0;
    }

    /// <summary>
    /// Reads the chunk.
    /// </summary>
    public void Read()
    {
        if (Size < 0)
            throw new Exception("Size < 0");

        var handler = _iffReader.FindHandler(ID, Type, Version);

        // If handler does not exist then skip this chunk.
        if (handler == null)
        {
            _iffReader.BaseStream.Position += Size;
            return;
        }

        // Read the bytes.
        byte[] data = _iffReader.ReadBytes((int)Size);

        // Create the backing store for the chunk IFF reader.
        using var chunkStream = new MemoryStream(data, 0, data.Length, false, false);

        // Create a chunk IFF reader which would be used for reading the chunk.
        using var chunkReader = new IFFReader(chunkStream);

        // Read the chunk.
        handler(chunkReader, new ChunkAttributes(ID, (int)Size, Type, Version));
    }
}
