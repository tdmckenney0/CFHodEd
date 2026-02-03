using CFHodEd.Math;

namespace GenericMesh.MaterialFields;

/// <summary>
/// Interface for materials with a texture.
/// </summary>
public interface IMaterialTexture
{
    /// <summary>Gets or sets the texture name (filename or path).</summary>
    string GetTextureName();
    
    /// <summary>Sets the texture name.</summary>
    void SetTextureName(string name);
}

/// <summary>
/// Interface for materials with a name.
/// </summary>
public interface IMaterialName
{
    /// <summary>Gets the material name.</summary>
    string GetMaterialName();
    
    /// <summary>Sets the material name.</summary>
    void SetMaterialName(string name);
}

/// <summary>
/// Interface for materials with DirectX-style material attributes.
/// </summary>
public interface IMaterialAttributes
{
    /// <summary>Gets the material attributes.</summary>
    MaterialProperties GetAttributes();
    
    /// <summary>Sets the material attributes.</summary>
    void SetAttributes(MaterialProperties attributes);
}

/// <summary>
/// Provides static methods to read material field values.
/// </summary>
public static class MaterialFieldReader
{
    /// <summary>Reads the texture name from a material.</summary>
    public static string TextureName<TMaterial>(TMaterial material) where TMaterial : IMaterial
    {
        if (material is IMaterialTexture mt)
            return mt.GetTextureName();
        return string.Empty;
    }

    /// <summary>Reads the material name.</summary>
    public static string MaterialName<TMaterial>(TMaterial material) where TMaterial : IMaterial
    {
        if (material is IMaterialName mn)
            return mn.GetMaterialName();
        return string.Empty;
    }

    /// <summary>Reads the material attributes.</summary>
    public static MaterialProperties Attributes<TMaterial>(TMaterial material) where TMaterial : IMaterial
    {
        if (material is IMaterialAttributes ma)
            return ma.GetAttributes();
        return new MaterialProperties
        {
            Diffuse = new ColorValue(0.8f, 0.8f, 0.8f, 1.0f),
            Ambient = new ColorValue(0.2f, 0.2f, 0.2f, 1.0f)
        };
    }
}

/// <summary>
/// Provides static methods to write material field values.
/// </summary>
public static class MaterialFieldWriter
{
    /// <summary>Sets the texture name on a material.</summary>
    public static void TextureName<TMaterial>(ref TMaterial material, string name) where TMaterial : struct, IMaterial
    {
        if (material is IMaterialTexture mt)
            mt.SetTextureName(name);
    }

    /// <summary>Sets the material name.</summary>
    public static void MaterialName<TMaterial>(ref TMaterial material, string name) where TMaterial : struct, IMaterial
    {
        if (material is IMaterialName mn)
            mn.SetMaterialName(name);
    }

    /// <summary>Sets the material attributes.</summary>
    public static void Attributes<TMaterial>(ref TMaterial material, MaterialProperties attribs) where TMaterial : struct, IMaterial
    {
        if (material is IMaterialAttributes ma)
            ma.SetAttributes(attribs);
    }
}
