using Silk.NET.OpenGL;

namespace CFHodEd.Rendering;

/// <summary>
/// OpenGL vertex buffer implementation.
/// </summary>
public class OpenGLVertexBuffer : IVertexBuffer
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly int _vertexCount;
    private readonly int _vertexSize;
    private readonly bool _dynamic;
    private bool _disposed;

    internal uint Handle => _handle;

    public int VertexCount => _vertexCount;
    public int VertexSize => _vertexSize;

    public OpenGLVertexBuffer(GL gl, int vertexCount, int vertexSize, bool dynamic)
    {
        _gl = gl;
        _vertexCount = vertexCount;
        _vertexSize = vertexSize;
        _dynamic = dynamic;
        _handle = _gl.GenBuffer();
    }

    public void SetData<T>(T[] data) where T : unmanaged
    {
        SetData<T>(data.AsSpan());
    }

    public unsafe void SetData<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _handle);
        fixed (T* ptr = data)
        {
            _gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(data.Length * sizeof(T)),
                ptr,
                _dynamic ? BufferUsageARB.DynamicDraw : BufferUsageARB.StaticDraw);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _gl.DeleteBuffer(_handle);
    }
}

/// <summary>
/// OpenGL index buffer implementation.
/// </summary>
public class OpenGLIndexBuffer : IIndexBuffer
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly int _indexCount;
    private readonly bool _is32Bit;
    private bool _disposed;

    internal uint Handle => _handle;

    public int IndexCount => _indexCount;
    public bool Is32Bit => _is32Bit;

    public OpenGLIndexBuffer(GL gl, int indexCount, bool is32Bit, bool dynamic)
    {
        _gl = gl;
        _indexCount = indexCount;
        _is32Bit = is32Bit;
        _handle = _gl.GenBuffer();
    }

    public unsafe void SetData(ushort[] data)
    {
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _handle);
        fixed (ushort* ptr = data)
        {
            _gl.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(data.Length * sizeof(ushort)),
                ptr,
                BufferUsageARB.StaticDraw);
        }
    }

    public unsafe void SetData(uint[] data)
    {
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _handle);
        fixed (uint* ptr = data)
        {
            _gl.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(data.Length * sizeof(uint)),
                ptr,
                BufferUsageARB.StaticDraw);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _gl.DeleteBuffer(_handle);
    }
}

/// <summary>
/// OpenGL texture implementation.
/// </summary>
public class OpenGLTexture : ITexture
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly int _width;
    private readonly int _height;
    private bool _disposed;

    internal uint Handle => _handle;

    public int Width => _width;
    public int Height => _height;

    public OpenGLTexture(GL gl, int width, int height, byte[] data)
    {
        _gl = gl;
        _width = width;
        _height = height;
        _handle = _gl.GenTexture();

        _gl.BindTexture(TextureTarget.Texture2D, _handle);
        
        unsafe
        {
            fixed (byte* ptr = data)
            {
                _gl.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    InternalFormat.Rgba,
                    (uint)width,
                    (uint)height,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    ptr);
            }
        }

        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        _gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _gl.DeleteTexture(_handle);
    }
}

/// <summary>
/// OpenGL shader program implementation.
/// </summary>
public class OpenGLShaderProgram : IShaderProgram
{
    private readonly GL _gl;
    private readonly uint _handle;
    private readonly Dictionary<string, int> _uniformCache = new();
    private bool _disposed;

    internal uint Handle => _handle;
    
    public bool IsValid { get; }

    public OpenGLShaderProgram(GL gl, string vertexSource, string fragmentSource)
    {
        _gl = gl;

        uint vs = CompileShader(ShaderType.VertexShader, vertexSource);
        uint fs = CompileShader(ShaderType.FragmentShader, fragmentSource);

        if (vs == 0 || fs == 0)
        {
            IsValid = false;
            _handle = 0;
            return;
        }

        _handle = _gl.CreateProgram();
        _gl.AttachShader(_handle, vs);
        _gl.AttachShader(_handle, fs);
        _gl.LinkProgram(_handle);

        _gl.GetProgram(_handle, ProgramPropertyARB.LinkStatus, out int status);
        if (status == 0)
        {
            string log = _gl.GetProgramInfoLog(_handle);
            Console.Error.WriteLine($"Shader link error: {log}");
            _gl.DeleteProgram(_handle);
            _handle = 0;
            IsValid = false;
        }
        else
        {
            IsValid = true;
        }

        _gl.DeleteShader(vs);
        _gl.DeleteShader(fs);
    }

    private uint CompileShader(ShaderType type, string source)
    {
        uint shader = _gl.CreateShader(type);
        _gl.ShaderSource(shader, source);
        _gl.CompileShader(shader);

        _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
        if (status == 0)
        {
            string log = _gl.GetShaderInfoLog(shader);
            Console.Error.WriteLine($"Shader compile error ({type}): {log}");
            _gl.DeleteShader(shader);
            return 0;
        }

        return shader;
    }

    public int GetUniformLocation(string name)
    {
        if (_uniformCache.TryGetValue(name, out int loc))
            return loc;

        loc = _gl.GetUniformLocation(_handle, name);
        _uniformCache[name] = loc;
        return loc;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        if (_handle != 0)
            _gl.DeleteProgram(_handle);
    }
}
