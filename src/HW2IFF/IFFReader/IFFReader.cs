using System.Diagnostics;
using System.Text;

namespace HW2IFF;

/// <summary>
/// Class that can read data from Homeworld2 IFF files.
/// </summary>
public sealed class IFFReader : BinaryReader
{
    private readonly List<Chunk> _chunkList;
    private readonly HandlerList _handlers;

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <param name="input">Input stream to use for reading.</param>
    public IFFReader(Stream input) : base(input)
    {
        _chunkList = new List<Chunk>();
        _handlers = new HandlerList(this);
        _handlers.DefaultHandler = DefaultChunkHandler;
    }

    /// <summary>
    /// Returns/Sets the default handler.
    /// </summary>
    public ChunkHandler? DefaultHandler
    {
        get => _handlers.DefaultHandler;
        set => _handlers.DefaultHandler = value;
    }

    /// <summary>
    /// Opens a file for reading.
    /// </summary>
    /// <param name="filename">File to open.</param>
    /// <returns>The object if successful, null otherwise.</returns>
    public static IFFReader? Open(string filename)
    {
        try
        {
            return new IFFReader(File.OpenRead(filename));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Adds a handler for the specified chunk.
    /// </summary>
    /// <param name="id">ID of the chunk.</param>
    /// <param name="type">Type of chunk.</param>
    /// <param name="handler">Handler for the chunk.</param>
    /// <param name="version">Version of the chunk.</param>
    public void AddHandler(string id, ChunkType type, ChunkHandler handler, uint version = 0)
    {
        _handlers.AddHandler(id, type, handler, version);
    }

    /// <summary>
    /// Finds a handler for the specified type of chunk.
    /// </summary>
    /// <param name="id">ID of the chunk.</param>
    /// <param name="type">Type of chunk.</param>
    /// <param name="version">Version of the chunk.</param>
    /// <returns>The handler if set, or null.</returns>
    internal ChunkHandler? FindHandler(string id, ChunkType type, uint version = 0)
    {
        return _handlers.FindHandler(id, type, version);
    }

    /// <summary>
    /// Parses (i.e. reads) the file.
    /// </summary>
    /// <param name="fromBeginning">Set to true to reset position of stream to 0.</param>
    public void Parse(bool fromBeginning = false)
    {
        if (_chunkList.Count != 0)
            _chunkList.Clear();

        if (fromBeginning && BaseStream.Position != 0)
            BaseStream.Position = 0;

        while (BaseStream.Position < BaseStream.Length)
        {
            var chunk = new Chunk(this)
            {
                StartPosition = BaseStream.Position,
                ID = ReadString(4),
                Size = Swap(ReadUInt32())
            };

            switch (chunk.ID)
            {
                case "NRML":
                    chunk.Type = ChunkType.Normal;
                    chunk.ID = ReadString(4);
                    chunk.Version = Swap(ReadUInt32());
                    chunk.Size -= 8;
                    break;

                case "FORM":
                    chunk.Type = ChunkType.Form;
                    chunk.ID = ReadString(4);
                    chunk.Size -= 4;
                    break;
            }

            if (BaseStream.Position + chunk.Size > BaseStream.Length)
            {
                Trace.TraceError($"Input IFF file has an invalid chunk! (\"{chunk.ID}\")");
                break;
            }

            _chunkList.Add(chunk);
            chunk.Read();
        }
    }

    /// <summary>
    /// Reads a fixed length string from the stream.
    /// </summary>
    /// <param name="length">Length of string.</param>
    /// <returns>A string read from the stream.</returns>
    public string ReadString(int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (length == 0)
            return string.Empty;

        byte[] bytes = ReadBytes(length);
        return Encoding.ASCII.GetString(bytes);
    }

    /// <summary>
    /// Reads a string from the stream.
    /// </summary>
    /// <returns>A string read from the stream.</returns>
    public override string ReadString()
    {
        int size = ReadInt32();

        if (size > 256)
            Trace.TraceWarning("Input string may be invalid.");

        return ReadString(size);
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

    /// <summary>
    /// Default chunk handler.
    /// </summary>
    private static void DefaultChunkHandler(IFFReader reader, ChunkAttributes chunkAttributes)
    {
        string versionInfo = chunkAttributes.Type == ChunkType.Normal
            ? $" Version: {chunkAttributes.Version}"
            : "";

        Trace.TraceInformation(
            $"Skipping chunk:\n" +
            $" ID: \"{chunkAttributes.ID}\"\n" +
            $" Type: '{chunkAttributes.Type}'" +
            versionInfo);
    }
}
