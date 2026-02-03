using System.Reflection;

namespace CFHodEd.Rendering;

/// <summary>
/// Utility class for loading shader sources.
/// </summary>
public static class ShaderLoader
{
    /// <summary>
    /// Loads a shader from an embedded resource.
    /// </summary>
    /// <param name="name">Resource name (e.g., "ship.vert")</param>
    /// <returns>Shader source or null if not found.</returns>
    public static string? LoadEmbedded(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = $"CFHodEd.Rendering.Shaders.{name}";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            return null;
            
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Loads a shader from a file path.
    /// </summary>
    public static string? LoadFromFile(string path)
    {
        if (!File.Exists(path))
            return null;
        return File.ReadAllText(path);
    }

    /// <summary>
    /// Creates a shader program using embedded resources.
    /// </summary>
    public static IShaderProgram? CreateFromEmbedded(IRenderDevice device, string baseName)
    {
        string? vertSource = LoadEmbedded($"{baseName}.vert");
        string? fragSource = LoadEmbedded($"{baseName}.frag");

        if (vertSource == null || fragSource == null)
            return null;

        return device.CreateShaderProgram(vertSource, fragSource);
    }
}
