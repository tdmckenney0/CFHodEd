using CFHodEd.Math;

namespace GenericMesh.Standard;

/// <summary>
/// Standard material implementation.
/// </summary>
public class Material : IMaterial
{
    /// <summary>Material name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Diffuse color.</summary>
    public ColorValue Diffuse { get; set; }

    /// <summary>Ambient color.</summary>
    public ColorValue Ambient { get; set; }

    /// <summary>Specular color.</summary>
    public ColorValue Specular { get; set; }

    /// <summary>Emissive color.</summary>
    public ColorValue Emissive { get; set; }

    /// <summary>Specular sharpness (power).</summary>
    public float SpecularPower { get; set; }

    /// <summary>Texture path or name.</summary>
    public string? TexturePath { get; set; }

    /// <summary>
    /// Sets the default properties.
    /// </summary>
    public void Initialize()
    {
        Name = string.Empty;
        Diffuse = ColorValue.White;
        Ambient = new ColorValue(0.2f, 0.2f, 0.2f, 1.0f);
        Specular = ColorValue.Black;
        Emissive = ColorValue.Black;
        SpecularPower = 0;
        TexturePath = null;
    }

    /// <summary>
    /// Applies the material to a render device.
    /// </summary>
    public void Apply(IRenderDevice device)
    {
        device.SetMaterial(new MaterialProperties
        {
            Diffuse = Diffuse,
            Ambient = Ambient,
            Specular = Specular,
            Emissive = Emissive,
            SpecularSharpness = SpecularPower
        });
    }

    /// <summary>
    /// Resets the device state.
    /// </summary>
    public void Reset(IRenderDevice device)
    {
        // Reset to default material
        device.SetMaterial(new MaterialProperties
        {
            Diffuse = ColorValue.White,
            Ambient = ColorValue.White,
            Specular = ColorValue.Black,
            Emissive = ColorValue.Black,
            SpecularSharpness = 0
        });
    }

    public bool Equals(IMaterial? other)
    {
        if (other is not Material m)
            return false;

        return Name == m.Name &&
               Diffuse == m.Diffuse &&
               Ambient == m.Ambient &&
               Specular == m.Specular &&
               Emissive == m.Emissive &&
               SpecularPower == m.SpecularPower &&
               TexturePath == m.TexturePath;
    }

    public override bool Equals(object? obj) => obj is Material m && Equals(m);
    public override int GetHashCode() => HashCode.Combine(Name, Diffuse, Specular, TexturePath);
}
