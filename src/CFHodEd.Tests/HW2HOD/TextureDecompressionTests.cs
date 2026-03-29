using HW2HOD;
using HW2IFF;
using Xunit;

namespace CFHodEd.Tests.HW2HOD;

/// <summary>
/// Tests for Texture DXT decompression and raw RGBA passthrough.
/// Uses synthetic pixel blocks crafted to produce known output colors.
/// </summary>
public class TextureDecompressionTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Builds a minimal LMIP chunk and reads it into a Texture via the same
    /// path used in production (Texture.ReadIFF).
    /// </summary>
    private static Texture BuildTexture(string fourCC, int width, int height, byte[] pixelData)
    {
        using var ms = new MemoryStream();
        var writer = new IFFWriter(ms);

        writer.Push("LMIP");

        // Path string (length-prefixed)
        writer.Write("test/texture.tga");

        // FourCC (fixed 4-byte string)
        writer.Write(fourCC, 4);

        // Mip count
        writer.WriteInt32(1);

        // Width and height of first (only) mip
        writer.WriteInt32(width);
        writer.WriteInt32(height);

        // Pixel data
        writer.Write(pixelData);

        writer.Pop();

        ms.Position = 0;
        var texture = new Texture();
        var reader = new IFFReader(ms);
        reader.AddHandler("LMIP", ChunkType.Default, (r, a) => texture.ReadIFF(r, a));
        reader.Parse();

        return texture;
    }

    // -------------------------------------------------------------------------
    // Raw 8888 passthrough
    // -------------------------------------------------------------------------

    [Fact]
    public void GetRGBAData_Raw8888_ReturnsPixelsUnmodified()
    {
        // 1×1 red pixel: RGBA = (255, 0, 0, 255)
        var pixels = new byte[] { 255, 0, 0, 255 };
        var tex = BuildTexture("8888", 1, 1, pixels);

        var result = tex.GetRGBAData();

        Assert.NotNull(result);
        Assert.Equal(4, result!.Length);
        Assert.Equal(255, result[0]); // R
        Assert.Equal(0,   result[1]); // G
        Assert.Equal(0,   result[2]); // B
        Assert.Equal(255, result[3]); // A
    }

    [Fact]
    public void GetRGBAData_Raw8888_4x1_ReturnsCorrectSize()
    {
        var pixels = new byte[4 * 4]; // 4 pixels × 4 bytes
        var tex = BuildTexture("8888", 4, 1, pixels);

        var result = tex.GetRGBAData();

        Assert.NotNull(result);
        Assert.Equal(16, result!.Length);
    }

    // -------------------------------------------------------------------------
    // DXT1 — 4×4 block, 8 bytes, opaque color interpolation
    // -------------------------------------------------------------------------

    [Fact]
    public void GetRGBAData_DXT1_OutputSizeIsCorrect()
    {
        // DXT1: 8 bytes per 4×4 block
        var block = BuildDXT1Block(0xFFFF, 0x0000); // white and black endpoints
        var tex = BuildTexture("DXT1", 4, 4, block);

        var result = tex.GetRGBAData();

        Assert.NotNull(result);
        Assert.Equal(4 * 4 * 4, result!.Length); // 64 bytes = 16 pixels × 4 channels
    }

    [Fact]
    public void GetRGBAData_DXT1_SolidWhite_AllPixelsAreWhite()
    {
        // c0 = 0xFFFF (white in RGB565), c1 = 0x0000, all 16 indices = 0 → all color0
        // Build indices selecting color0 (index 0) for all 16 texels
        var block = BuildDXT1Block(
            c0: 0xFFFF,   // white
            c1: 0x0000,   // black, but c0 > c1 so 4-color mode
            indices: 0x00000000u); // all texels → color0

        var tex = BuildTexture("DXT1", 4, 4, block);
        var result = tex.GetRGBAData()!;

        // Every pixel should be white (R≈255, G≈255, B≈255)
        for (int i = 0; i < 16; i++)
        {
            Assert.True(result[i * 4 + 0] > 200, $"pixel {i} R = {result[i * 4 + 0]}");
            Assert.True(result[i * 4 + 1] > 200, $"pixel {i} G = {result[i * 4 + 1]}");
            Assert.True(result[i * 4 + 2] > 200, $"pixel {i} B = {result[i * 4 + 2]}");
            Assert.Equal((byte)255, result[i * 4 + 3]); // alpha opaque
        }
    }

    [Fact]
    public void GetRGBAData_DXT1_SolidBlack_AllPixelsAreBlack()
    {
        // c0 < c1 → 3-color+transparent mode; index 1 → color1 (black)
        var block = BuildDXT1Block(
            c0: 0x0000,   // black
            c1: 0xFFFF,   // white — c0 < c1, so 3-color + transparent
            indices: 0x55555555u); // all texels → index 1 (color1 = white, but…)

        // In 3-color mode: c0=0 (black), c1=0xFFFF (white)
        // index=0 → black, index=1 → white. Use index=0 for all-black.
        var blockBlack = BuildDXT1Block(0x0000, 0xFFFF, 0x00000000u);
        var tex = BuildTexture("DXT1", 4, 4, blockBlack);
        var result = tex.GetRGBAData()!;

        for (int i = 0; i < 16; i++)
        {
            Assert.Equal(0, result[i * 4 + 0]); // R
            Assert.Equal(0, result[i * 4 + 1]); // G
            Assert.Equal(0, result[i * 4 + 2]); // B
        }
    }

    // -------------------------------------------------------------------------
    // DXT3 — 4×4 block, 16 bytes (8 explicit alpha + 8 DXT1 color)
    // -------------------------------------------------------------------------

    [Fact]
    public void GetRGBAData_DXT3_OutputSizeIsCorrect()
    {
        var block = new byte[16];
        var tex = BuildTexture("DXT3", 4, 4, block);

        var result = tex.GetRGBAData();

        Assert.NotNull(result);
        Assert.Equal(4 * 4 * 4, result!.Length);
    }

    [Fact]
    public void GetRGBAData_DXT3_FullAlpha_AllPixelsAreOpaque()
    {
        // First 8 bytes = explicit alpha nibbles; all 0xF = full alpha
        var block = new byte[16];
        for (int i = 0; i < 8; i++) block[i] = 0xFF; // two nibbles = 0xF each = 15 → 255
        // bytes 8-15 = DXT1 color block (all zeros = all black, all-black with indices 0)
        var tex = BuildTexture("DXT3", 4, 4, block);
        var result = tex.GetRGBAData()!;

        for (int i = 0; i < 16; i++)
            Assert.Equal((byte)255, result[i * 4 + 3]); // 0xF * 17 = 255
    }

    [Fact]
    public void GetRGBAData_DXT3_ZeroAlpha_AllPixelsAreTransparent()
    {
        // First 8 bytes = 0x00 → alpha nibbles all 0 → alpha = 0
        var block = new byte[16];
        var tex = BuildTexture("DXT3", 4, 4, block);
        var result = tex.GetRGBAData()!;

        for (int i = 0; i < 16; i++)
            Assert.Equal((byte)0, result[i * 4 + 3]);
    }

    // -------------------------------------------------------------------------
    // DXT5 — 4×4 block, 16 bytes (8 interpolated alpha + 8 DXT1 color)
    // -------------------------------------------------------------------------

    [Fact]
    public void GetRGBAData_DXT5_OutputSizeIsCorrect()
    {
        var block = new byte[16];
        var tex = BuildTexture("DXT5", 4, 4, block);

        var result = tex.GetRGBAData();

        Assert.NotNull(result);
        Assert.Equal(4 * 4 * 4, result!.Length);
    }

    [Fact]
    public void GetRGBAData_DXT5_FullAlphaEndpoints_AllOpaque()
    {
        // a0 = 255, a1 = 0 → a0 > a1 → 8-alpha interpolation; index=0 → alpha=255
        var block = new byte[16];
        block[0] = 255; // a0
        block[1] = 0;   // a1
        // bytes 2-7: all zero = all indices 0 → alpha = a0 = 255
        // bytes 8-15: DXT1 color block (zeros)
        var tex = BuildTexture("DXT5", 4, 4, block);
        var result = tex.GetRGBAData()!;

        for (int i = 0; i < 16; i++)
            Assert.Equal((byte)255, result[i * 4 + 3]);
    }

    // -------------------------------------------------------------------------
    // Texture metadata
    // -------------------------------------------------------------------------

    [Fact]
    public void Texture_Dimensions_CorrectAfterRead()
    {
        var pixels = new byte[8 * 8 * 4];
        var tex = BuildTexture("8888", 8, 8, pixels);

        Assert.Equal(8, tex.Width);
        Assert.Equal(8, tex.Height);
        Assert.Equal(1, tex.NumMips);
    }

    [Fact]
    public void Texture_EmptyTexture_GetRGBAData_ReturnsNull()
    {
        var tex = new Texture();
        Assert.Null(tex.GetRGBAData());
    }

    // -------------------------------------------------------------------------
    // Block builder helpers
    // -------------------------------------------------------------------------

    private static byte[] BuildDXT1Block(ushort c0, ushort c1, uint indices = 0)
    {
        return new byte[]
        {
            (byte)(c0 & 0xFF), (byte)(c0 >> 8),
            (byte)(c1 & 0xFF), (byte)(c1 >> 8),
            (byte)(indices & 0xFF),
            (byte)((indices >> 8) & 0xFF),
            (byte)((indices >> 16) & 0xFF),
            (byte)((indices >> 24) & 0xFF),
        };
    }
}
