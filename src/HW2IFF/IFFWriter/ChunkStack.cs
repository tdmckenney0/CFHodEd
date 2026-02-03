namespace HW2IFF;

/// <summary>
/// Class which acts as a chunk stack for writing IFF files.
/// </summary>
internal class ChunkStack
{
    private readonly Stack<ChunkMarker> _stack;
    private readonly IFFWriter _writer;

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <param name="writer">The writer which will be associated with this instance.</param>
    public ChunkStack(IFFWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        _stack = new Stack<ChunkMarker>();
        _writer = writer;
    }

    /// <summary>
    /// Pushes a chunk to the stack.
    /// </summary>
    /// <param name="id">ID of the chunk.</param>
    /// <param name="type">Type of chunk.</param>
    /// <param name="version">Version of the chunk.</param>
    public void Push(string id, ChunkType type, uint version = 0)
    {
        // Validate by creating ChunkAttributes (will throw on invalid input)
        _ = new ChunkAttributes(id, 0, type, version);

        _stack.Push(new ChunkMarker
        {
            ID = id,
            Type = type,
            StartPosition = _writer.BaseStream.Position + 8
        });

        switch (type)
        {
            case ChunkType.Normal:
                _writer.Write("NRML", 4);
                _writer.WriteUInt32(0u);
                _writer.Write(id, 4);
                _writer.WriteUInt32(Swap(version));
                break;

            case ChunkType.Form:
                _writer.Write("FORM", 4);
                _writer.WriteUInt32(0u);
                _writer.Write(id, 4);
                break;

            default:
                _writer.Write(id, 4);
                _writer.WriteUInt32(0u);
                break;
        }
    }

    /// <summary>
    /// Pops a chunk off the stack and returns it.
    /// </summary>
    public ChunkMarker Pop()
    {
        if (_stack.Count == 0)
            throw new Exception("No chunk to pop.");

        var chunk = _stack.Pop();
        long currentPos = _writer.BaseStream.Position;
        uint size = (uint)(currentPos - chunk.StartPosition);

        // Seek to appropriate position to write size.
        _writer.BaseStream.Position = chunk.StartPosition - 4;

        // Now write the size.
        _writer.WriteUInt32(Swap(size));

        // Return to original position.
        _writer.BaseStream.Position = currentPos;

        return chunk;
    }

    /// <summary>
    /// Endian converter.
    /// </summary>
    private static uint Swap(uint v)
    {
        return ((v & 0xFF000000) >> 24) |
               ((v & 0x00FF0000) >> 8) |
               ((v & 0x0000FF00) << 8) |
               ((v & 0x000000FF) << 24);
    }
}
