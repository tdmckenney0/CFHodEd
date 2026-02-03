using CFHodEd.Math;

namespace CFHodEd.Rendering;

/// <summary>
/// Primitive types for drawing.
/// </summary>
public enum RenderPrimitiveType
{
    PointList,
    LineList,
    LineStrip,
    TriangleList,
    TriangleStrip,
    TriangleFan
}

/// <summary>
/// Blend mode for rendering.
/// </summary>
public enum BlendMode
{
    Opaque,
    Alpha,
    Additive,
    Multiply
}

/// <summary>
/// Cull mode for back-face culling.
/// </summary>
public enum CullMode
{
    None,
    Clockwise,
    CounterClockwise
}

/// <summary>
/// Fill mode for polygon rendering.
/// </summary>
public enum FillMode
{
    Solid,
    Wireframe,
    Point
}

/// <summary>
/// Texture filtering mode.
/// </summary>
public enum TextureFilter
{
    Point,
    Linear,
    Anisotropic
}

/// <summary>
/// Texture addressing mode.
/// </summary>
public enum TextureAddress
{
    Wrap,
    Clamp,
    Mirror
}

/// <summary>
/// Vertex declaration element.
/// </summary>
public record struct VertexElement(
    int Stream,
    int Offset,
    VertexElementFormat Format,
    VertexElementUsage Usage,
    int UsageIndex = 0);

/// <summary>
/// Format of a vertex element.
/// </summary>
public enum VertexElementFormat
{
    Float1,
    Float2,
    Float3,
    Float4,
    Color,
    UByte4,
    Short2,
    Short4
}

/// <summary>
/// Usage of a vertex element.
/// </summary>
public enum VertexElementUsage
{
    Position,
    Normal,
    Tangent,
    Binormal,
    TextureCoordinate,
    Color,
    BlendWeight,
    BlendIndices
}

/// <summary>
/// Render device interface abstracting graphics API operations.
/// </summary>
public interface IRenderDevice : IDisposable
{
    /// <summary>Gets the viewport width.</summary>
    int ViewportWidth { get; }
    
    /// <summary>Gets the viewport height.</summary>
    int ViewportHeight { get; }
    
    /// <summary>Sets the viewport.</summary>
    void SetViewport(int x, int y, int width, int height);
    
    /// <summary>Clears the render target.</summary>
    void Clear(ColorValue color, float depth = 1.0f, bool clearColor = true, bool clearDepth = true);
    
    /// <summary>Begins a rendering frame.</summary>
    void BeginScene();
    
    /// <summary>Ends a rendering frame.</summary>
    void EndScene();
    
    /// <summary>Creates a vertex buffer.</summary>
    IVertexBuffer CreateVertexBuffer(int vertexCount, int vertexSize, bool dynamic = false);
    
    /// <summary>Creates an index buffer.</summary>
    IIndexBuffer CreateIndexBuffer(int indexCount, bool is32Bit = false, bool dynamic = false);
    
    /// <summary>Creates a texture from file.</summary>
    ITexture? CreateTextureFromFile(string path);
    
    /// <summary>Creates a texture from memory.</summary>
    ITexture CreateTexture(int width, int height, byte[] data);
    
    /// <summary>Creates a shader program.</summary>
    IShaderProgram? CreateShaderProgram(string vertexSource, string fragmentSource);
    
    /// <summary>Sets the active shader program.</summary>
    void SetShaderProgram(IShaderProgram? program);
    
    /// <summary>Sets a shader uniform value.</summary>
    void SetUniform(string name, float value);
    void SetUniform(string name, int value);
    void SetUniform(string name, Vector2 value);
    void SetUniform(string name, Vector3 value);
    void SetUniform(string name, Vector4 value);
    void SetUniform(string name, Matrix value);
    void SetUniform(string name, ColorValue value);
    
    /// <summary>Binds a texture to a slot.</summary>
    void SetTexture(int slot, ITexture? texture);
    
    /// <summary>Sets the vertex buffer for rendering.</summary>
    void SetVertexBuffer(IVertexBuffer? buffer, VertexElement[] elements);
    
    /// <summary>Sets the index buffer for rendering.</summary>
    void SetIndexBuffer(IIndexBuffer? buffer);
    
    /// <summary>Sets the blend mode.</summary>
    void SetBlendMode(BlendMode mode);
    
    /// <summary>Sets the cull mode.</summary>
    void SetCullMode(CullMode mode);
    
    /// <summary>Sets the fill mode.</summary>
    void SetFillMode(FillMode mode);
    
    /// <summary>Enables or disables depth testing.</summary>
    void SetDepthTest(bool enabled, bool writeEnabled = true);
    
    /// <summary>Draws primitives.</summary>
    void DrawPrimitives(RenderPrimitiveType type, int startVertex, int primitiveCount);
    
    /// <summary>Draws indexed primitives.</summary>
    void DrawIndexedPrimitives(RenderPrimitiveType type, int startIndex, int primitiveCount, int baseVertex = 0);
    
    /// <summary>Sets the world matrix.</summary>
    Matrix WorldMatrix { get; set; }
    
    /// <summary>Sets the view matrix.</summary>
    Matrix ViewMatrix { get; set; }
    
    /// <summary>Sets the projection matrix.</summary>
    Matrix ProjectionMatrix { get; set; }
}

/// <summary>
/// Vertex buffer interface.
/// </summary>
public interface IVertexBuffer : IDisposable
{
    int VertexCount { get; }
    int VertexSize { get; }
    void SetData<T>(T[] data) where T : unmanaged;
    void SetData<T>(ReadOnlySpan<T> data) where T : unmanaged;
}

/// <summary>
/// Index buffer interface.
/// </summary>
public interface IIndexBuffer : IDisposable
{
    int IndexCount { get; }
    bool Is32Bit { get; }
    void SetData(ushort[] data);
    void SetData(uint[] data);
}

/// <summary>
/// Texture interface.
/// </summary>
public interface ITexture : IDisposable
{
    int Width { get; }
    int Height { get; }
}

/// <summary>
/// Shader program interface.
/// </summary>
public interface IShaderProgram : IDisposable
{
    bool IsValid { get; }
    int GetUniformLocation(string name);
}
