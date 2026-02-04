using HW2IFF;

namespace HW2HOD;

/// <summary>
/// Represents a texture mip level.
/// </summary>
public class TextureMip
{
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Class representing a Homeworld2 texture stored in HOD files.
/// </summary>
public sealed class Texture
{
    private string _path = "no path";
    private string _fourCC = "8888";
    private readonly List<TextureMip> _mips = new();

    public string Path
    {
        get => _path;
        set => _path = value ?? "no path";
    }

    public string FourCC => _fourCC;
    public int NumMips => _mips.Count;
    public int Width => _mips.Count > 0 ? _mips[0].Width : 0;
    public int Height => _mips.Count > 0 ? _mips[0].Height : 0;
    public IReadOnlyList<TextureMip> Mips => _mips;

    /// <summary>
    /// Gets the RGBA pixel data (decompressed if needed).
    /// </summary>
    public byte[]? GetRGBAData()
    {
        if (_mips.Count == 0) return null;
        var mip = _mips[0];
        
        if (_fourCC == "8888")
            return mip.Data;
        
        // Decompress DXT
        return DecompressDXT(mip.Data, mip.Width, mip.Height, _fourCC);
    }

    internal void ReadIFF(IFFReader iff, ChunkAttributes attrs)
    {
        _mips.Clear();
        
        // Read texture path
        _path = iff.ReadString();
        
        // Read FourCC (4 chars)
        _fourCC = iff.ReadString(4);
        
        // Read mip count
        int mipCount = iff.ReadInt32();
        
        int width = 0, height = 0;
        for (int i = 0; i < mipCount; i++)
        {
            var mip = new TextureMip();
            
            // First mip has explicit dimensions
            if (i == 0)
            {
                width = iff.ReadInt32();
                height = iff.ReadInt32();
            }
            else
            {
                width = System.Math.Max(1, width / 2);
                height = System.Math.Max(1, height / 2);
            }
            
            mip.Width = width;
            mip.Height = height;
            
            // Calculate data size
            int dataSize = CalculateDataSize(width, height, _fourCC);
            mip.Data = iff.ReadBytes(dataSize);
            
            _mips.Add(mip);
        }
    }

    private static int CalculateDataSize(int width, int height, string fourCC)
    {
        switch (fourCC)
        {
            case "8888":
                return width * height * 4;
            case "DXT1":
                return System.Math.Max(1, (width + 3) / 4) * System.Math.Max(1, (height + 3) / 4) * 8;
            case "DXT3":
            case "DXT5":
                return System.Math.Max(1, (width + 3) / 4) * System.Math.Max(1, (height + 3) / 4) * 16;
            default:
                return width * height * 4;
        }
    }

    private static byte[]? DecompressDXT(byte[] data, int width, int height, string fourCC)
    {
        byte[] output = new byte[width * height * 4];
        int blockCountX = (width + 3) / 4;
        int blockCountY = (height + 3) / 4;
        int offset = 0;

        for (int by = 0; by < blockCountY; by++)
        {
            for (int bx = 0; bx < blockCountX; bx++)
            {
                byte[] blockPixels;
                if (fourCC == "DXT1")
                {
                    blockPixels = DecompressDXT1Block(data, offset);
                    offset += 8;
                }
                else if (fourCC == "DXT3")
                {
                    blockPixels = DecompressDXT3Block(data, offset);
                    offset += 16;
                }
                else // DXT5
                {
                    blockPixels = DecompressDXT5Block(data, offset);
                    offset += 16;
                }

                // Copy block pixels to output
                for (int py = 0; py < 4 && (by * 4 + py) < height; py++)
                {
                    for (int px = 0; px < 4 && (bx * 4 + px) < width; px++)
                    {
                        int srcIdx = (py * 4 + px) * 4;
                        int dstIdx = ((by * 4 + py) * width + (bx * 4 + px)) * 4;
                        output[dstIdx] = blockPixels[srcIdx];     // R
                        output[dstIdx + 1] = blockPixels[srcIdx + 1]; // G
                        output[dstIdx + 2] = blockPixels[srcIdx + 2]; // B
                        output[dstIdx + 3] = blockPixels[srcIdx + 3]; // A
                    }
                }
            }
        }

        return output;
    }

    private static byte[] DecompressDXT1Block(byte[] data, int offset)
    {
        byte[] pixels = new byte[16 * 4];
        
        ushort c0 = (ushort)(data[offset] | (data[offset + 1] << 8));
        ushort c1 = (ushort)(data[offset + 2] | (data[offset + 3] << 8));
        uint indices = (uint)(data[offset + 4] | (data[offset + 5] << 8) | 
                              (data[offset + 6] << 16) | (data[offset + 7] << 24));

        byte[] colors = new byte[4 * 4]; // 4 colors, RGBA each
        DecodeColor565(c0, colors, 0);
        DecodeColor565(c1, colors, 4);

        if (c0 > c1)
        {
            colors[8] = (byte)((2 * colors[0] + colors[4]) / 3);
            colors[9] = (byte)((2 * colors[1] + colors[5]) / 3);
            colors[10] = (byte)((2 * colors[2] + colors[6]) / 3);
            colors[11] = 255;
            colors[12] = (byte)((colors[0] + 2 * colors[4]) / 3);
            colors[13] = (byte)((colors[1] + 2 * colors[5]) / 3);
            colors[14] = (byte)((colors[2] + 2 * colors[6]) / 3);
            colors[15] = 255;
        }
        else
        {
            colors[8] = (byte)((colors[0] + colors[4]) / 2);
            colors[9] = (byte)((colors[1] + colors[5]) / 2);
            colors[10] = (byte)((colors[2] + colors[6]) / 2);
            colors[11] = 255;
            colors[12] = 0;
            colors[13] = 0;
            colors[14] = 0;
            colors[15] = 0; // Transparent
        }

        for (int i = 0; i < 16; i++)
        {
            int idx = (int)((indices >> (i * 2)) & 3);
            pixels[i * 4] = colors[idx * 4];
            pixels[i * 4 + 1] = colors[idx * 4 + 1];
            pixels[i * 4 + 2] = colors[idx * 4 + 2];
            pixels[i * 4 + 3] = colors[idx * 4 + 3];
        }

        return pixels;
    }

    private static byte[] DecompressDXT3Block(byte[] data, int offset)
    {
        byte[] pixels = DecompressDXT1Block(data, offset + 8);
        
        // Apply explicit alpha
        for (int i = 0; i < 16; i++)
        {
            int alphaIdx = i / 2;
            int alpha = (i % 2 == 0) ? (data[offset + alphaIdx] & 0xF) : ((data[offset + alphaIdx] >> 4) & 0xF);
            pixels[i * 4 + 3] = (byte)(alpha * 17); // Scale 0-15 to 0-255
        }

        return pixels;
    }

    private static byte[] DecompressDXT5Block(byte[] data, int offset)
    {
        byte[] pixels = DecompressDXT1Block(data, offset + 8);
        
        // Interpolated alpha
        byte a0 = data[offset];
        byte a1 = data[offset + 1];
        byte[] alphas = new byte[8];
        alphas[0] = a0;
        alphas[1] = a1;
        
        if (a0 > a1)
        {
            for (int i = 0; i < 6; i++)
                alphas[2 + i] = (byte)(((6 - i) * a0 + (i + 1) * a1) / 7);
        }
        else
        {
            for (int i = 0; i < 4; i++)
                alphas[2 + i] = (byte)(((4 - i) * a0 + (i + 1) * a1) / 5);
            alphas[6] = 0;
            alphas[7] = 255;
        }

        // Read 48-bit alpha indices
        ulong alphaIndices = 0;
        for (int i = 0; i < 6; i++)
            alphaIndices |= (ulong)data[offset + 2 + i] << (i * 8);

        for (int i = 0; i < 16; i++)
        {
            int idx = (int)((alphaIndices >> (i * 3)) & 7);
            pixels[i * 4 + 3] = alphas[idx];
        }

        return pixels;
    }

    private static void DecodeColor565(ushort color, byte[] output, int offset)
    {
        output[offset] = (byte)(((color >> 11) & 0x1F) * 255 / 31);     // R
        output[offset + 1] = (byte)(((color >> 5) & 0x3F) * 255 / 63);  // G
        output[offset + 2] = (byte)((color & 0x1F) * 255 / 31);         // B
        output[offset + 3] = 255;                                        // A
    }

    public override string ToString() => System.IO.Path.GetFileName(_path);
}
