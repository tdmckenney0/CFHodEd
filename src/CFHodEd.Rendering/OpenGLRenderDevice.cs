using CFHodEd.Math;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace CFHodEd.Rendering;

/// <summary>
/// OpenGL implementation of IRenderDevice.
/// </summary>
public class OpenGLRenderDevice : IRenderDevice
{
    private readonly GL _gl;
    private uint _vao;
    private OpenGLShaderProgram? _currentProgram;
    private OpenGLVertexBuffer? _currentVB;
    private OpenGLIndexBuffer? _currentIB;
    
    private int _viewportWidth;
    private int _viewportHeight;
    
    private Matrix _worldMatrix = Matrix.Identity;
    private Matrix _viewMatrix = Matrix.Identity;
    private Matrix _projMatrix = Matrix.Identity;
    
    private bool _disposed;

    public int ViewportWidth => _viewportWidth;
    public int ViewportHeight => _viewportHeight;

    public Matrix WorldMatrix
    {
        get => _worldMatrix;
        set => _worldMatrix = value;
    }

    public Matrix ViewMatrix
    {
        get => _viewMatrix;
        set => _viewMatrix = value;
    }

    public Matrix ProjectionMatrix
    {
        get => _projMatrix;
        set => _projMatrix = value;
    }

    public OpenGLRenderDevice(GL gl)
    {
        _gl = gl;
        _vao = _gl.GenVertexArray();
        _gl.BindVertexArray(_vao);
        
        // Enable standard states
        _gl.Enable(EnableCap.DepthTest);
        _gl.Enable(EnableCap.CullFace);
        _gl.CullFace(TriangleFace.Back);
    }

    public void SetViewport(int x, int y, int width, int height)
    {
        _viewportWidth = width;
        _viewportHeight = height;
        _gl.Viewport(x, y, (uint)width, (uint)height);
    }

    public void Clear(ColorValue color, float depth = 1.0f, bool clearColor = true, bool clearDepth = true)
    {
        ClearBufferMask mask = 0;
        
        if (clearColor)
        {
            _gl.ClearColor(color.R, color.G, color.B, color.A);
            mask |= ClearBufferMask.ColorBufferBit;
        }
        
        if (clearDepth)
        {
            _gl.ClearDepth(depth);
            mask |= ClearBufferMask.DepthBufferBit;
        }
        
        if (mask != 0)
            _gl.Clear((uint)mask);
    }

    public void BeginScene() { }
    public void EndScene() { }

    public IVertexBuffer CreateVertexBuffer(int vertexCount, int vertexSize, bool dynamic = false)
    {
        return new OpenGLVertexBuffer(_gl, vertexCount, vertexSize, dynamic);
    }

    public IIndexBuffer CreateIndexBuffer(int indexCount, bool is32Bit = false, bool dynamic = false)
    {
        return new OpenGLIndexBuffer(_gl, indexCount, is32Bit, dynamic);
    }

    public ITexture? CreateTextureFromFile(string path)
    {
        if (!File.Exists(path))
            return null;

        try
        {
            using var stream = File.OpenRead(path);
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            return new OpenGLTexture(_gl, image.Width, image.Height, image.Data);
        }
        catch
        {
            return null;
        }
    }

    public ITexture CreateTexture(int width, int height, byte[] data)
    {
        return new OpenGLTexture(_gl, width, height, data);
    }

    public IShaderProgram? CreateShaderProgram(string vertexSource, string fragmentSource)
    {
        var program = new OpenGLShaderProgram(_gl, vertexSource, fragmentSource);
        return program.IsValid ? program : null;
    }

    public void SetShaderProgram(IShaderProgram? program)
    {
        _currentProgram = program as OpenGLShaderProgram;
        if (_currentProgram != null)
            _gl.UseProgram(_currentProgram.Handle);
        else
            _gl.UseProgram(0);
    }

    public void SetUniform(string name, float value)
    {
        if (_currentProgram == null) return;
        int loc = _currentProgram.GetUniformLocation(name);
        if (loc >= 0) _gl.Uniform1(loc, value);
    }

    public void SetUniform(string name, int value)
    {
        if (_currentProgram == null) return;
        int loc = _currentProgram.GetUniformLocation(name);
        if (loc >= 0) _gl.Uniform1(loc, value);
    }

    public void SetUniform(string name, Vector2 value)
    {
        if (_currentProgram == null) return;
        int loc = _currentProgram.GetUniformLocation(name);
        if (loc >= 0) _gl.Uniform2(loc, value.X, value.Y);
    }

    public void SetUniform(string name, Vector3 value)
    {
        if (_currentProgram == null) return;
        int loc = _currentProgram.GetUniformLocation(name);
        if (loc >= 0) _gl.Uniform3(loc, value.X, value.Y, value.Z);
    }

    public void SetUniform(string name, Vector4 value)
    {
        if (_currentProgram == null) return;
        int loc = _currentProgram.GetUniformLocation(name);
        if (loc >= 0) _gl.Uniform4(loc, value.X, value.Y, value.Z, value.W);
    }

    public unsafe void SetUniform(string name, Matrix value)
    {
        if (_currentProgram == null) return;
        int loc = _currentProgram.GetUniformLocation(name);
        if (loc >= 0)
        {
            float* data = stackalloc float[16];
            // Row-major layout
            data[0] = value.M11; data[1] = value.M12; data[2] = value.M13; data[3] = value.M14;
            data[4] = value.M21; data[5] = value.M22; data[6] = value.M23; data[7] = value.M24;
            data[8] = value.M31; data[9] = value.M32; data[10] = value.M33; data[11] = value.M34;
            data[12] = value.M41; data[13] = value.M42; data[14] = value.M43; data[15] = value.M44;
            _gl.UniformMatrix4(loc, 1, true, data); // true = transpose for OpenGL column-major
        }
    }

    public void SetUniform(string name, ColorValue value)
    {
        SetUniform(name, new Vector4(value.R, value.G, value.B, value.A));
    }

    public void SetTexture(int slot, ITexture? texture)
    {
        _gl.ActiveTexture(TextureUnit.Texture0 + slot);
        if (texture is OpenGLTexture glTex)
            _gl.BindTexture(TextureTarget.Texture2D, glTex.Handle);
        else
            _gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void SetVertexBuffer(IVertexBuffer? buffer, VertexElement[] elements)
    {
        _currentVB = buffer as OpenGLVertexBuffer;
        if (_currentVB == null)
        {
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            return;
        }

        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _currentVB.Handle);

        uint attribIndex = 0;
        foreach (var element in elements)
        {
            _gl.EnableVertexAttribArray(attribIndex);
            
            int size = element.Format switch
            {
                VertexElementFormat.Float1 => 1,
                VertexElementFormat.Float2 => 2,
                VertexElementFormat.Float3 => 3,
                VertexElementFormat.Float4 => 4,
                VertexElementFormat.Color => 4,
                VertexElementFormat.UByte4 => 4,
                VertexElementFormat.Short2 => 2,
                VertexElementFormat.Short4 => 4,
                _ => 4
            };

            VertexAttribPointerType type = element.Format switch
            {
                VertexElementFormat.Color => VertexAttribPointerType.UnsignedByte,
                VertexElementFormat.UByte4 => VertexAttribPointerType.UnsignedByte,
                VertexElementFormat.Short2 => VertexAttribPointerType.Short,
                VertexElementFormat.Short4 => VertexAttribPointerType.Short,
                _ => VertexAttribPointerType.Float
            };

            bool normalized = element.Format == VertexElementFormat.Color;

            unsafe
            {
                _gl.VertexAttribPointer(
                    attribIndex,
                    size,
                    type,
                    normalized,
                    (uint)_currentVB.VertexSize,
                    (void*)element.Offset);
            }

            attribIndex++;
        }
    }

    public void SetIndexBuffer(IIndexBuffer? buffer)
    {
        _currentIB = buffer as OpenGLIndexBuffer;
        if (_currentIB != null)
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _currentIB.Handle);
        else
            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
    }

    public void SetBlendMode(BlendMode mode)
    {
        switch (mode)
        {
            case BlendMode.Opaque:
                _gl.Disable(EnableCap.Blend);
                break;
            case BlendMode.Alpha:
                _gl.Enable(EnableCap.Blend);
                _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                break;
            case BlendMode.Additive:
                _gl.Enable(EnableCap.Blend);
                _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
                break;
            case BlendMode.Multiply:
                _gl.Enable(EnableCap.Blend);
                _gl.BlendFunc(BlendingFactor.DstColor, BlendingFactor.Zero);
                break;
        }
    }

    public void SetCullMode(CullMode mode)
    {
        if (mode == CullMode.None)
        {
            _gl.Disable(EnableCap.CullFace);
        }
        else
        {
            _gl.Enable(EnableCap.CullFace);
            _gl.FrontFace(mode == CullMode.CounterClockwise ? FrontFaceDirection.Ccw : FrontFaceDirection.CW);
        }
    }

    public void SetFillMode(FillMode mode)
    {
        PolygonMode glMode = mode switch
        {
            FillMode.Wireframe => PolygonMode.Line,
            FillMode.Point => PolygonMode.Point,
            _ => PolygonMode.Fill
        };
        _gl.PolygonMode(TriangleFace.FrontAndBack, glMode);
    }

    public void SetDepthTest(bool enabled, bool writeEnabled = true)
    {
        if (enabled)
            _gl.Enable(EnableCap.DepthTest);
        else
            _gl.Disable(EnableCap.DepthTest);
        
        _gl.DepthMask(writeEnabled);
    }

    public void DrawPrimitives(RenderPrimitiveType type, int startVertex, int primitiveCount)
    {
        int vertexCount = GetVertexCount(type, primitiveCount);
        _gl.DrawArrays(ToGLPrimitive(type), startVertex, (uint)vertexCount);
    }

    public void DrawIndexedPrimitives(RenderPrimitiveType type, int startIndex, int primitiveCount, int baseVertex = 0)
    {
        if (_currentIB == null) return;
        
        int indexCount = GetVertexCount(type, primitiveCount);
        DrawElementsType indexType = _currentIB.Is32Bit ? DrawElementsType.UnsignedInt : DrawElementsType.UnsignedShort;
        int indexSize = _currentIB.Is32Bit ? 4 : 2;
        
        unsafe
        {
            _gl.DrawElements(ToGLPrimitive(type), (uint)indexCount, indexType, (void*)(startIndex * indexSize));
        }
    }

    private static PrimitiveType ToGLPrimitive(RenderPrimitiveType type) => type switch
    {
        RenderPrimitiveType.PointList => PrimitiveType.Points,
        RenderPrimitiveType.LineList => PrimitiveType.Lines,
        RenderPrimitiveType.LineStrip => PrimitiveType.LineStrip,
        RenderPrimitiveType.TriangleList => PrimitiveType.Triangles,
        RenderPrimitiveType.TriangleStrip => PrimitiveType.TriangleStrip,
        RenderPrimitiveType.TriangleFan => PrimitiveType.TriangleFan,
        _ => PrimitiveType.Triangles
    };

    private static int GetVertexCount(RenderPrimitiveType type, int primitiveCount) => type switch
    {
        RenderPrimitiveType.PointList => primitiveCount,
        RenderPrimitiveType.LineList => primitiveCount * 2,
        RenderPrimitiveType.LineStrip => primitiveCount + 1,
        RenderPrimitiveType.TriangleList => primitiveCount * 3,
        RenderPrimitiveType.TriangleStrip => primitiveCount + 2,
        RenderPrimitiveType.TriangleFan => primitiveCount + 2,
        _ => primitiveCount * 3
    };

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _gl.DeleteVertexArray(_vao);
    }
}
