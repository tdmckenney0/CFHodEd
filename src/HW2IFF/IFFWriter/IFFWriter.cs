using System.Text;

namespace HW2IFF;

/// <summary>
/// Class that can write data in Homeworld2 IFF files.
/// </summary>
public sealed class IFFWriter : BinaryWriter
{
    private readonly ChunkStack _chunkStack;

    /// <summary>
    /// Class constructor.
    /// </summary>
    /// <param name="output">Output stream to use for writing.</param>
    public IFFWriter(Stream output) : base(output)
    {
        _chunkStack = new ChunkStack(this);
    }

    /// <summary>
    /// Opens a file for writing.
    /// </summary>
    /// <param name="filename">File to open.</param>
    /// <returns>The object if successful, null otherwise.</returns>
    public static IFFWriter? Open(string filename)
    {
        try
        {
            return new IFFWriter(File.OpenWrite(filename));
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Pushes a default chunk to the stack.
    /// </summary>
    /// <param name="id">ID of the chunk.</param>
    public void Push(string id)
    {
        _chunkStack.Push(id, ChunkType.Default);
    }

    /// <summary>
    /// Pushes a chunk to the stack.
    /// </summary>
    /// <param name="id">ID of the chunk.</param>
    /// <param name="type">Type of chunk.</param>
    /// <param name="version">Version of the chunk.</param>
    public void Push(string id, ChunkType type, uint version = 0)
    {
        _chunkStack.Push(id, type, version);
    }

    /// <summary>
    /// Pops a chunk off the stack and returns it.
    /// </summary>
    public string Pop()
    {
        return _chunkStack.Pop().ID;
    }

    /// <summary>
    /// Writes a fixed length string to the stream.
    /// </summary>
    /// <param name="value">String to write.</param>
    /// <param name="length">Length of string.</param>
    public void Write(string value, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        if (length == 0)
            return;

        byte[] bytes = new byte[length];

        if (value != null)
        {
            byte[] strBytes = Encoding.ASCII.GetBytes(value);
            int copyLength = Math.Min(strBytes.Length, length);
            Array.Copy(strBytes, bytes, copyLength);
        }

        Write(bytes);
    }

    /// <summary>
    /// Writes a string to the stream.
    /// </summary>
    /// <param name="value">String to write.</param>
    public override void Write(string value)
    {
        WriteInt32(value.Length);

        byte[] bytes = Encoding.ASCII.GetBytes(value);
        Write(bytes);
    }

    /// <summary>
    /// Writes a 16-bit integer to the stream.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="value">Value to write.</param>
    public void WriteInt16<T>(T value) where T : struct
    {
        Write(Convert.ToInt16(value));
    }

    /// <summary>
    /// Writes a 16-bit unsigned integer to the stream.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="value">Value to write.</param>
    public void WriteUInt16<T>(T value) where T : struct
    {
        Write(Convert.ToUInt16(value));
    }

    /// <summary>
    /// Writes a 32-bit integer to the stream.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="value">Value to write.</param>
    public void WriteInt32<T>(T value) where T : struct
    {
        Write(Convert.ToInt32(value));
    }

    /// <summary>
    /// Writes a 32-bit unsigned integer to the stream.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    /// <param name="value">Value to write.</param>
    public void WriteUInt32<T>(T value) where T : struct
    {
        Write(Convert.ToUInt32(value));
    }
}
