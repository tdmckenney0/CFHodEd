using CFHodEd.Math;
using HW2IFF;

namespace HW2HOD;

/// <summary>
/// HOD texture parameters structure.
/// </summary>
public readonly struct TextureParameters
{
    public readonly string Name;
    public readonly int UVSet;
    public readonly int TextureIndex;

    public TextureParameters(string name = "", int uvSet = 0, int textureIndex = -1)
    {
        Name = name ?? "";
        UVSet = uvSet;
        TextureIndex = textureIndex;
    }

    public static TextureParameters Empty => new("", 0, -1);
    public bool HasTexture => TextureIndex >= 0;

    public override string ToString() => Name;
}

/// <summary>
/// Shader parameters for HOD materials.
/// </summary>
public sealed class ShaderParameters
{
    public TextureParameters Diffuse;
    public TextureParameters Glow;
    public TextureParameters Specular;
    public TextureParameters Reflection;
    public TextureParameters Normal;
    public TextureParameters Team;
    public TextureParameters Pain;
    public TextureParameters Stripe;

    public ShaderParameters()
    {
        Diffuse = TextureParameters.Empty;
        Glow = TextureParameters.Empty;
        Specular = TextureParameters.Empty;
        Reflection = TextureParameters.Empty;
        Normal = TextureParameters.Empty;
        Team = TextureParameters.Empty;
        Pain = TextureParameters.Empty;
        Stripe = TextureParameters.Empty;
    }

    public ShaderParameters(ShaderParameters other)
    {
        Diffuse = other.Diffuse;
        Glow = other.Glow;
        Specular = other.Specular;
        Reflection = other.Reflection;
        Normal = other.Normal;
        Team = other.Team;
        Pain = other.Pain;
        Stripe = other.Stripe;
    }
}

/// <summary>
/// Material type enumeration.
/// </summary>
public enum MaterialType
{
    Simple,
    MultiMesh
}

/// <summary>
/// Class representing a Homeworld2 material.
/// </summary>
public sealed class Material
{
    private string _name = "Material";
    private string _shaderName = "ship";
    private MaterialType _type = MaterialType.MultiMesh;
    private readonly ShaderParameters _shaderParams = new();

    public Material() { }

    public Material(Material m)
    {
        _name = m._name;
        _shaderName = m._shaderName;
        _type = m._type;
        _shaderParams = new ShaderParameters(m._shaderParams);
    }

    public string Name
    {
        get => _name;
        set => _name = value ?? throw new ArgumentNullException(nameof(value));
    }

    public string ShaderName
    {
        get => _shaderName;
        set => _shaderName = value ?? "";
    }

    public MaterialType Type
    {
        get => _type;
        set => _type = value;
    }

    public ShaderParameters ShaderParameters => _shaderParams;

    public override string ToString() => _name;

    internal void ReadIFF(IFFReader iff, int version)
    {
        // Read material name
        _name = iff.ReadString();
        
        // Read shader name
        _shaderName = iff.ReadString();
        
        // Read parameter count
        int paramCount = iff.ReadInt32();
        
        // Read parameters using VB.NET format: type, dataLength, data, name
        for (int i = 0; i < paramCount; i++)
        {
            // Parameter type: 4=Colour, 5=Texture
            int paramType = iff.ReadInt32();
            
            // Data length (int32)
            int dataLength = iff.ReadInt32();
            
            // Parse based on type
            int textureIndex = -1;
            if (paramType == 5 && dataLength == 4) // Texture
            {
                textureIndex = iff.ReadInt32();
            }
            else
            {
                // Skip the data bytes
                for (int b = 0; b < dataLength; b++)
                    iff.ReadByte();
            }
            
            // Name comes AFTER data (only in versioned chunks, version 1001+)
            string paramName = "";
            if (version > 0)
                paramName = iff.ReadString();
            
            // Assign texture index by parameter name
            if (paramType == 5 && textureIndex >= 0)
            {
                // Handle $ prefix in parameter names (e.g. "$diffuse" -> "diffuse")
                string normalizedName = paramName.TrimStart('$').ToLowerInvariant();
                var texParam = new TextureParameters(paramName, 0, textureIndex);
                switch (normalizedName)
                {
                    case "diffuse": _shaderParams.Diffuse = texParam; break;
                    case "glow": _shaderParams.Glow = texParam; break;
                    case "specular": _shaderParams.Specular = texParam; break;
                    case "reflection": _shaderParams.Reflection = texParam; break;
                    case "normal": _shaderParams.Normal = texParam; break;
                    case "team": _shaderParams.Team = texParam; break;
                    case "pain": _shaderParams.Pain = texParam; break;
                    case "stripe": _shaderParams.Stripe = texParam; break;
                }
            }
        }
    }

    internal void WriteIFF(IFFWriter iff, int version)
    {
        iff.Write(_name);
        iff.Write(_shaderName);

        WriteTextureParam(iff, _shaderParams.Diffuse);
        WriteTextureParam(iff, _shaderParams.Glow);
        WriteTextureParam(iff, _shaderParams.Specular);
        WriteTextureParam(iff, _shaderParams.Reflection);
        WriteTextureParam(iff, _shaderParams.Normal);
        WriteTextureParam(iff, _shaderParams.Team);
    }

    private static void ReadTextureParam(IFFReader iff, ref TextureParameters param)
    {
        string name = iff.ReadString();
        int uvSet = iff.ReadInt32();
        param = new TextureParameters(name, uvSet);
    }

    private static void WriteTextureParam(IFFWriter iff, TextureParameters param)
    {
        iff.Write(param.Name);
        iff.WriteInt32(param.UVSet);
    }
}
