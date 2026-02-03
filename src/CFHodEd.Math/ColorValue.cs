namespace CFHodEd.Math;

/// <summary>
/// A color value with RGBA components, compatible with Direct3D ColorValue.
/// Components are stored as floats in the range [0, 1].
/// </summary>
public struct ColorValue : IEquatable<ColorValue>
{
    public float R, G, B, A;

    /// <summary>Transparent black (0, 0, 0, 0).</summary>
    public static readonly ColorValue Transparent = new(0, 0, 0, 0);

    /// <summary>Opaque black (0, 0, 0, 1).</summary>
    public static readonly ColorValue Black = new(0, 0, 0, 1);

    /// <summary>Opaque white (1, 1, 1, 1).</summary>
    public static readonly ColorValue White = new(1, 1, 1, 1);

    /// <summary>Opaque red (1, 0, 0, 1).</summary>
    public static readonly ColorValue Red = new(1, 0, 0, 1);

    /// <summary>Opaque green (0, 1, 0, 1).</summary>
    public static readonly ColorValue Green = new(0, 1, 0, 1);

    /// <summary>Opaque blue (0, 0, 1, 1).</summary>
    public static readonly ColorValue Blue = new(0, 0, 1, 1);

    /// <summary>
    /// Creates a color from RGBA float components.
    /// </summary>
    public ColorValue(float r, float g, float b, float a = 1.0f)
    {
        R = r; G = g; B = b; A = a;
    }

    /// <summary>
    /// Creates a color from a 32-bit ARGB integer (as used by System.Drawing.Color).
    /// </summary>
    public static ColorValue FromArgb(int argb)
    {
        return new ColorValue(
            ((argb >> 16) & 0xFF) / 255.0f,
            ((argb >> 8) & 0xFF) / 255.0f,
            (argb & 0xFF) / 255.0f,
            ((argb >> 24) & 0xFF) / 255.0f);
    }

    /// <summary>
    /// Creates a color from byte components (0-255).
    /// </summary>
    public static ColorValue FromRgba(byte r, byte g, byte b, byte a = 255)
    {
        return new ColorValue(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
    }

    /// <summary>
    /// Converts to a 32-bit ARGB integer.
    /// </summary>
    public int ToArgb()
    {
        int r = (int)(System.Math.Clamp(R, 0, 1) * 255);
        int g = (int)(System.Math.Clamp(G, 0, 1) * 255);
        int b = (int)(System.Math.Clamp(B, 0, 1) * 255);
        int a = (int)(System.Math.Clamp(A, 0, 1) * 255);
        return (a << 24) | (r << 16) | (g << 8) | b;
    }

    /// <summary>
    /// Converts to a 32-bit RGBA integer.
    /// </summary>
    public int ToRgba()
    {
        int r = (int)(System.Math.Clamp(R, 0, 1) * 255);
        int g = (int)(System.Math.Clamp(G, 0, 1) * 255);
        int b = (int)(System.Math.Clamp(B, 0, 1) * 255);
        int a = (int)(System.Math.Clamp(A, 0, 1) * 255);
        return (r << 24) | (g << 16) | (b << 8) | a;
    }

    /// <summary>
    /// Linearly interpolates between two colors.
    /// </summary>
    public static ColorValue Lerp(ColorValue start, ColorValue end, float amount)
    {
        return new ColorValue(
            start.R + (end.R - start.R) * amount,
            start.G + (end.G - start.G) * amount,
            start.B + (end.B - start.B) * amount,
            start.A + (end.A - start.A) * amount);
    }

    /// <summary>
    /// Modulates (multiplies) two colors component-wise.
    /// </summary>
    public static ColorValue Modulate(ColorValue a, ColorValue b)
    {
        return new ColorValue(a.R * b.R, a.G * b.G, a.B * b.B, a.A * b.A);
    }

    /// <summary>
    /// Adds two colors component-wise.
    /// </summary>
    public static ColorValue Add(ColorValue a, ColorValue b)
    {
        return new ColorValue(a.R + b.R, a.G + b.G, a.B + b.B, a.A + b.A);
    }

    /// <summary>
    /// Scales a color by a factor.
    /// </summary>
    public static ColorValue Scale(ColorValue color, float factor)
    {
        return new ColorValue(color.R * factor, color.G * factor, color.B * factor, color.A * factor);
    }

    // Operators
    public static ColorValue operator +(ColorValue left, ColorValue right) => Add(left, right);
    public static ColorValue operator *(ColorValue left, ColorValue right) => Modulate(left, right);
    public static ColorValue operator *(ColorValue color, float scale) => Scale(color, scale);
    public static ColorValue operator *(float scale, ColorValue color) => Scale(color, scale);
    public static bool operator ==(ColorValue left, ColorValue right) => left.Equals(right);
    public static bool operator !=(ColorValue left, ColorValue right) => !left.Equals(right);

    // Conversion to Vector4 (useful for shader uniforms)
    public static implicit operator Vector4(ColorValue c) => new(c.R, c.G, c.B, c.A);
    public static implicit operator ColorValue(Vector4 v) => new(v.X, v.Y, v.Z, v.W);

    public bool Equals(ColorValue other) => R == other.R && G == other.G && B == other.B && A == other.A;
    public override bool Equals(object? obj) => obj is ColorValue c && Equals(c);
    public override int GetHashCode() => HashCode.Combine(R, G, B, A);
    public override string ToString() => $"RGBA({R:F2}, {G:F2}, {B:F2}, {A:F2})";
}
