using HW2IFF;
using Xunit;

namespace CFHodEd.Tests.HW2IFF;

/// <summary>
/// Round-trip tests: write a chunk via IFFWriter then read it back via IFFReader.
/// </summary>
public class IFFReaderWriterTests
{
    [Fact]
    public void WriteAndRead_Int32_RoundTrip()
    {
        using var ms = new MemoryStream();
        var writer = new IFFWriter(ms);
        writer.Push("TEST");
        writer.WriteInt32(42);
        writer.Pop();

        ms.Position = 0;
        int? read = null;
        var reader = new IFFReader(ms);
        reader.AddHandler("TEST", ChunkType.Default, (r, _) => { read = r.ReadInt32(); });
        reader.Parse();

        Assert.Equal(42, read);
    }

    [Fact]
    public void WriteAndRead_Int16_RoundTrip()
    {
        using var ms = new MemoryStream();
        var writer = new IFFWriter(ms);
        writer.Push("I16C");
        writer.WriteInt16((short)1234);
        writer.Pop();

        ms.Position = 0;
        short? read = null;
        var reader = new IFFReader(ms);
        reader.AddHandler("I16C", ChunkType.Default, (r, _) => { read = r.ReadInt16(); });
        reader.Parse();

        Assert.Equal((short)1234, read);
    }

    [Fact]
    public void WriteAndRead_Float_RoundTrip()
    {
        const float testValue = 3.14159f;
        using var ms = new MemoryStream();
        var writer = new IFFWriter(ms);
        writer.Push("FVAL");
        writer.Write(testValue);   // BinaryWriter.Write(float)
        writer.Pop();

        ms.Position = 0;
        float? read = null;
        var reader = new IFFReader(ms);
        reader.AddHandler("FVAL", ChunkType.Default, (r, _) => { read = r.ReadSingle(); });
        reader.Parse();

        Assert.Equal(testValue, read!.Value, 5);
    }

    [Fact]
    public void WriteAndRead_LengthPrefixedString_RoundTrip()
    {
        const string testStr = "Hello HOD";
        using var ms = new MemoryStream();
        var writer = new IFFWriter(ms);
        writer.Push("STRG");
        writer.Write(testStr);
        writer.Pop();

        ms.Position = 0;
        string? read = null;
        var reader = new IFFReader(ms);
        reader.AddHandler("STRG", ChunkType.Default, (r, _) => { read = r.ReadString(); });
        reader.Parse();

        Assert.Equal(testStr, read);
    }

    [Fact]
    public void WriteAndRead_FixedLengthString_RoundTrip()
    {
        using var ms = new MemoryStream();
        var writer = new IFFWriter(ms);
        writer.Push("FXST");
        writer.Write("ABCD", 4);
        writer.Pop();

        ms.Position = 0;
        string? read = null;
        var reader = new IFFReader(ms);
        reader.AddHandler("FXST", ChunkType.Default, (r, _) => { read = r.ReadString(4); });
        reader.Parse();

        Assert.Equal("ABCD", read);
    }

    [Fact]
    public void WriteAndRead_FormChunk_RoundTrip()
    {
        using var ms = new MemoryStream();
        var writer = new IFFWriter(ms);
        writer.Push("FORM", ChunkType.Form);
        writer.Push("INNR");
        writer.WriteInt32(99);
        writer.Pop();
        writer.Pop();

        ms.Position = 0;
        int? read = null;
        var reader = new IFFReader(ms);
        reader.AddHandler("FORM", ChunkType.Form, (r, _) =>
        {
            r.AddHandler("INNR", ChunkType.Default, (ir, _) => { read = ir.ReadInt32(); });
            r.Parse();
        });
        reader.Parse();

        Assert.Equal(99, read);
    }

    [Fact]
    public void WriteAndRead_NormalChunk_RoundTrip()
    {
        using var ms = new MemoryStream();
        var writer = new IFFWriter(ms);
        writer.Push("DATA", ChunkType.Normal, 1400u);
        writer.WriteInt32(7);
        writer.Pop();

        ms.Position = 0;
        int? read = null;
        uint? readVersion = null;
        var reader = new IFFReader(ms);
        reader.AddHandler("DATA", ChunkType.Normal, (r, a) =>
        {
            read = r.ReadInt32();
            readVersion = a.Version;
        }, 1400u);
        reader.Parse();

        Assert.Equal(7, read);
        Assert.Equal(1400u, readVersion);
    }

    [Fact]
    public void WriteAndRead_MultipleChunks_AllRead()
    {
        using var ms = new MemoryStream();
        var writer = new IFFWriter(ms);
        writer.Push("AAA ");
        writer.WriteInt32(1);
        writer.Pop();
        writer.Push("BBB ");
        writer.WriteInt32(2);
        writer.Pop();
        writer.Push("CCC ");
        writer.WriteInt32(3);
        writer.Pop();

        ms.Position = 0;
        var values = new List<int>();
        var reader = new IFFReader(ms);
        reader.AddHandler("AAA ", ChunkType.Default, (r, _) => values.Add(r.ReadInt32()));
        reader.AddHandler("BBB ", ChunkType.Default, (r, _) => values.Add(r.ReadInt32()));
        reader.AddHandler("CCC ", ChunkType.Default, (r, _) => values.Add(r.ReadInt32()));
        reader.Parse();

        Assert.Equal(new[] { 1, 2, 3 }, values);
    }

    [Fact]
    public void WriteAndRead_EmptyChunk_DoesNotThrow()
    {
        using var ms = new MemoryStream();
        var writer = new IFFWriter(ms);
        writer.Push("EMPT");
        writer.Pop();

        ms.Position = 0;
        bool called = false;
        var reader = new IFFReader(ms);
        reader.AddHandler("EMPT", ChunkType.Default, (_, _) => { called = true; });
        reader.Parse();

        Assert.True(called);
    }
}
