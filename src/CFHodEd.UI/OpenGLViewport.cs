using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using HW2HOD;
using System.Runtime.InteropServices;
using Matrix = CFHodEd.Math.Matrix;
using Vector3 = CFHodEd.Math.Vector3;
using Vector4 = CFHodEd.Math.Vector4;
using BoundingBox = CFHodEd.Math.BoundingBox;

namespace CFHodEd.UI;

/// <summary>
/// 3D viewport control with software rendering fallback.
/// Uses WriteableBitmap for cross-platform compatibility.
/// </summary>
public class OpenGLViewport : Control
{
    private WriteableBitmap? _backBuffer;
    private bool _needsRedraw = true;
    
    // Camera
    private Vector3 _cameraPosition = new(0, 2, 5);
    private Vector3 _cameraTarget = Vector3.Zero;
    private float _cameraYaw;
    private float _cameraPitch = 0.3f;
    private float _cameraDistance = 5f;
    
    // Mouse state
    private Point _lastMousePos;
    private bool _isRotating;
    private bool _isPanning;
    
    // Render state
    private RenderMode _renderMode = RenderMode.Solid;
    private Color _clearColor = Color.FromRgb(30, 30, 35);
    private Color _gridColor = Color.FromRgb(60, 60, 70);
    private Color _meshColor = Color.FromRgb(100, 180, 255);
    
    // Model
    private HOD? _model;

    public event Action? RenderFrame;

    public HOD? Model
    {
        get => _model;
        set
        {
            _model = value;
            if (value != null)
                FocusOnModel();
            InvalidateVisual();
        }
    }

    public RenderMode Mode
    {
        get => _renderMode;
        set
        {
            _renderMode = value;
            InvalidateVisual();
        }
    }

    public Vector3 CameraPosition => _cameraPosition;
    
    public Matrix ViewMatrix => Matrix.LookAtLH(_cameraPosition, _cameraTarget, Vector3.UnitY);
    
    public Matrix ProjectionMatrix { get; private set; } = Matrix.Identity;

    public OpenGLViewport()
    {
        Focusable = true;
        ClipToBounds = true;
        UpdateCameraPosition();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        
        int width = System.Math.Max(1, (int)e.NewSize.Width);
        int height = System.Math.Max(1, (int)e.NewSize.Height);
        
        _backBuffer = new WriteableBitmap(
            new PixelSize(width, height),
            new Vector(96, 96),
            Avalonia.Platform.PixelFormat.Bgra8888,
            Avalonia.Platform.AlphaFormat.Premul);
        
        UpdateProjection();
        _needsRedraw = true;
        InvalidateVisual();
    }

    private void UpdateProjection()
    {
        if (Bounds.Width <= 0 || Bounds.Height <= 0) return;
        
        float aspect = (float)(Bounds.Width / Bounds.Height);
        ProjectionMatrix = Matrix.PerspectiveFovLH(
            MathF.PI / 4f,  // 45 degrees FOV
            aspect,
            0.1f,
            1000f);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (_backBuffer == null) return;

        // Software render to back buffer
        RenderToBuffer();

        // Draw the buffer
        context.DrawImage(_backBuffer, new Rect(0, 0, Bounds.Width, Bounds.Height));
        
        // Draw viewport info overlay
        DrawOverlay(context);
    }

    private unsafe void RenderToBuffer()
    {
        if (_backBuffer == null) return;

        using var fb = _backBuffer.Lock();
        var ptr = (uint*)fb.Address;
        int width = _backBuffer.PixelSize.Width;
        int height = _backBuffer.PixelSize.Height;
        int stride = fb.RowBytes / 4;

        // Clear with background color
        uint clearColorBgra = (uint)(_clearColor.A << 24 | _clearColor.R << 16 | _clearColor.G << 8 | _clearColor.B);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                ptr[y * stride + x] = clearColorBgra;
            }
        }

        // Draw grid
        DrawGrid(ptr, width, height, stride);

        // Draw coordinate axes
        DrawAxes(ptr, width, height, stride);

        // Draw model meshes
        if (_model != null)
            DrawMeshes(ptr, width, height, stride);

        _needsRedraw = false;
    }

    private unsafe void DrawGrid(uint* ptr, int width, int height, int stride)
    {
        uint gridColorBgra = (uint)(_gridColor.A << 24 | _gridColor.R << 16 | _gridColor.G << 8 | _gridColor.B);
        
        // Project grid lines in world space to screen space
        for (int i = -10; i <= 10; i++)
        {
            // Lines parallel to Z axis
            var p1 = ProjectPoint(new Vector3(i, 0, -10), width, height);
            var p2 = ProjectPoint(new Vector3(i, 0, 10), width, height);
            if (p1.HasValue && p2.HasValue)
                DrawLine(ptr, width, height, stride, p1.Value, p2.Value, gridColorBgra);

            // Lines parallel to X axis
            p1 = ProjectPoint(new Vector3(-10, 0, i), width, height);
            p2 = ProjectPoint(new Vector3(10, 0, i), width, height);
            if (p1.HasValue && p2.HasValue)
                DrawLine(ptr, width, height, stride, p1.Value, p2.Value, gridColorBgra);
        }
    }

    private unsafe void DrawAxes(uint* ptr, int width, int height, int stride)
    {
        var origin = ProjectPoint(Vector3.Zero, width, height);
        if (!origin.HasValue) return;

        // X axis - Red
        var xEnd = ProjectPoint(new Vector3(2, 0, 0), width, height);
        if (xEnd.HasValue)
            DrawLine(ptr, width, height, stride, origin.Value, xEnd.Value, 0xFF0000FF); // Red

        // Y axis - Green
        var yEnd = ProjectPoint(new Vector3(0, 2, 0), width, height);
        if (yEnd.HasValue)
            DrawLine(ptr, width, height, stride, origin.Value, yEnd.Value, 0xFF00FF00); // Green

        // Z axis - Blue
        var zEnd = ProjectPoint(new Vector3(0, 0, 2), width, height);
        if (zEnd.HasValue)
            DrawLine(ptr, width, height, stride, origin.Value, zEnd.Value, 0xFFFF0000); // Blue
    }

    private (int x, int y)? ProjectPoint(Vector3 worldPos, int width, int height)
    {
        var viewProj = ViewMatrix * ProjectionMatrix;
        var clipPos = Vector4.Transform(new Vector4(worldPos.X, worldPos.Y, worldPos.Z, 1), viewProj);
        
        if (clipPos.W <= 0) return null; // Behind camera
        
        float ndcX = clipPos.X / clipPos.W;
        float ndcY = clipPos.Y / clipPos.W;
        float ndcZ = clipPos.Z / clipPos.W;
        
        if (ndcZ < 0 || ndcZ > 1) return null; // Outside frustum
        
        int screenX = (int)((ndcX + 1) * 0.5f * width);
        int screenY = (int)((1 - ndcY) * 0.5f * height); // Flip Y
        
        return (screenX, screenY);
    }

    private unsafe void DrawLine(uint* ptr, int width, int height, int stride, 
        (int x, int y) p1, (int x, int y) p2, uint color)
    {
        // Bresenham's line algorithm
        int x0 = p1.x, y0 = p1.y, x1 = p2.x, y1 = p2.y;
        int dx = System.Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -System.Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
                ptr[y0 * stride + x0] = color;

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    private void DrawOverlay(DrawingContext context)
    {
        var brush = new SolidColorBrush(Colors.White, 0.7);
        var typeface = new Typeface("Segoe UI", FontStyle.Normal, FontWeight.Normal);
        
        // Camera info
        var text = new FormattedText(
            $"Camera: ({_cameraPosition.X:F1}, {_cameraPosition.Y:F1}, {_cameraPosition.Z:F1})\n" +
            $"Yaw: {_cameraYaw * 180 / MathF.PI:F0}° Pitch: {_cameraPitch * 180 / MathF.PI:F0}°\n" +
            $"Mode: {_renderMode}",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            12,
            brush);
        
        context.DrawText(text, new Point(8, 8));

        // Controls hint
        var hintText = new FormattedText(
            "LMB: Rotate | MMB/RMB: Pan | Scroll: Zoom | R: Reset",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            11,
            new SolidColorBrush(Colors.White, 0.5));
        
        context.DrawText(hintText, new Point(8, Bounds.Height - 20));
    }

    public void RequestRedraw()
    {
        _needsRedraw = true;
        InvalidateVisual();
    }

    public void ResetCamera()
    {
        _cameraPosition = new Vector3(0, 2, 5);
        _cameraTarget = Vector3.Zero;
        _cameraYaw = 0;
        _cameraPitch = 0.3f;
        _cameraDistance = 5f;
        UpdateCameraPosition();
        RequestRedraw();
    }

    public void FocusOnBounds(BoundingBox bounds)
    {
        _cameraTarget = bounds.Center;
        _cameraDistance = bounds.Size.Length() * 1.5f;
        UpdateCameraPosition();
        RequestRedraw();
    }

    private void FocusOnModel()
    {
        if (_model == null || _model.Meshes.Count == 0)
        {
            ResetCamera();
            return;
        }

        // Calculate bounding box across all meshes
        var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (var mesh in _model.Meshes)
        {
            foreach (var lod in mesh.LODs)
            {
                var bb = lod.Bounds;
                min = new Vector3(
                    System.Math.Min(min.X, bb.Min.X),
                    System.Math.Min(min.Y, bb.Min.Y),
                    System.Math.Min(min.Z, bb.Min.Z));
                max = new Vector3(
                    System.Math.Max(max.X, bb.Max.X),
                    System.Math.Max(max.Y, bb.Max.Y),
                    System.Math.Max(max.Z, bb.Max.Z));
            }
        }

        if (min.X < float.MaxValue)
        {
            var bounds = new BoundingBox(min, max);
            _cameraTarget = bounds.Center;
            _cameraDistance = System.Math.Max(2f, bounds.Size.Length() * 1.5f);
            _cameraYaw = 0.5f;
            _cameraPitch = 0.4f;
            UpdateCameraPosition();
        }
    }

    private unsafe void DrawMeshes(uint* ptr, int width, int height, int stride)
    {
        if (_model == null) return;

        // Light direction (from camera)
        var lightDir = Vector3.Normalize(_cameraPosition - _cameraTarget);
        
        // Collect triangles for depth sorting (for solid mode)
        var triangles = new List<(float depth, int i0, int i1, int i2, MeshLOD lod)>();

        foreach (var mesh in _model.Meshes)
        {
            if (mesh.LODs.Count == 0) continue;
            var lod = mesh.LODs[0];
            var vertices = lod.Vertices;
            var indices = lod.Indices;

            for (int i = 0; i + 2 < indices.Count; i += 3)
            {
                int i0 = indices[i];
                int i1 = indices[i + 1];
                int i2 = indices[i + 2];

                if (i0 >= vertices.Count || i1 >= vertices.Count || i2 >= vertices.Count)
                    continue;

                var v0 = vertices[i0].Position;
                var v1 = vertices[i1].Position;
                var v2 = vertices[i2].Position;

                // Calculate depth for sorting (average Z in view space)
                var viewMatrix = ViewMatrix;
                var tv0 = Vector3.TransformCoordinate(new Vector3(v0.X, v0.Y, v0.Z), viewMatrix);
                var tv1 = Vector3.TransformCoordinate(new Vector3(v1.X, v1.Y, v1.Z), viewMatrix);
                var tv2 = Vector3.TransformCoordinate(new Vector3(v2.X, v2.Y, v2.Z), viewMatrix);
                float avgDepth = (tv0.Z + tv1.Z + tv2.Z) / 3f;

                triangles.Add((avgDepth, i0, i1, i2, lod));
            }
        }

        // Sort back-to-front for painter's algorithm
        triangles.Sort((a, b) => b.depth.CompareTo(a.depth));

        foreach (var (depth, i0, i1, i2, lod) in triangles)
        {
            var vertices = lod.Vertices;
            var v0 = vertices[i0].Position;
            var v1 = vertices[i1].Position;
            var v2 = vertices[i2].Position;

            var p0Pos = new Vector3(v0.X, v0.Y, v0.Z);
            var p1Pos = new Vector3(v1.X, v1.Y, v1.Z);
            var p2Pos = new Vector3(v2.X, v2.Y, v2.Z);

            // Calculate face normal for backface culling and lighting
            var edge1 = p1Pos - p0Pos;
            var edge2 = p2Pos - p0Pos;
            var faceNormal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
            
            // Backface culling - skip if facing away from camera
            var toCamera = Vector3.Normalize(_cameraPosition - (p0Pos + p1Pos + p2Pos) / 3f);
            float dotCamera = Vector3.Dot(faceNormal, toCamera);
            if (dotCamera < 0 && _renderMode == RenderMode.Solid) continue;

            var p0 = ProjectPoint(p0Pos, width, height);
            var p1 = ProjectPoint(p1Pos, width, height);
            var p2 = ProjectPoint(p2Pos, width, height);

            if (!p0.HasValue || !p1.HasValue || !p2.HasValue) continue;

            if (_renderMode == RenderMode.Solid)
            {
                // Flat shading: calculate diffuse lighting
                float diffuse = System.Math.Max(0.2f, Vector3.Dot(faceNormal, lightDir));
                byte r = (byte)(_meshColor.R * diffuse);
                byte g = (byte)(_meshColor.G * diffuse);
                byte b = (byte)(_meshColor.B * diffuse);
                uint color = (uint)(255 << 24 | r << 16 | g << 8 | b);

                DrawFilledTriangle(ptr, width, height, stride, p0.Value, p1.Value, p2.Value, color);
            }
            else
            {
                // Wireframe mode
                uint meshColorBgra = (uint)(_meshColor.A << 24 | _meshColor.R << 16 | _meshColor.G << 8 | _meshColor.B);
                DrawLine(ptr, width, height, stride, p0.Value, p1.Value, meshColorBgra);
                DrawLine(ptr, width, height, stride, p1.Value, p2.Value, meshColorBgra);
                DrawLine(ptr, width, height, stride, p2.Value, p0.Value, meshColorBgra);
            }
        }
    }

    private unsafe void DrawFilledTriangle(uint* ptr, int width, int height, int stride, 
                                            (int x, int y) p0, (int x, int y) p1, (int x, int y) p2, uint color)
    {
        // Sort vertices by Y coordinate
        if (p0.y > p1.y) (p0, p1) = (p1, p0);
        if (p1.y > p2.y) (p1, p2) = (p2, p1);
        if (p0.y > p1.y) (p0, p1) = (p1, p0);

        int y0 = p0.y, y1 = p1.y, y2 = p2.y;
        double x0 = p0.x, x1 = p1.x, x2 = p2.x;

        // Scanline fill
        for (int y = System.Math.Max(0, y0); y <= System.Math.Min(height - 1, y2); y++)
        {
            double xa, xb;
            if (y < y1)
            {
                // Upper half
                if (y1 == y0) xa = x0; else xa = x0 + (x1 - x0) * (y - y0) / (y1 - y0);
                if (y2 == y0) xb = x0; else xb = x0 + (x2 - x0) * (y - y0) / (y2 - y0);
            }
            else
            {
                // Lower half
                if (y2 == y1) xa = x1; else xa = x1 + (x2 - x1) * (y - y1) / (y2 - y1);
                if (y2 == y0) xb = x0; else xb = x0 + (x2 - x0) * (y - y0) / (y2 - y0);
            }

            if (xa > xb) (xa, xb) = (xb, xa);
            int startX = System.Math.Max(0, (int)xa);
            int endX = System.Math.Min(width - 1, (int)xb);

            for (int x = startX; x <= endX; x++)
            {
                ptr[y * stride + x] = color;
            }
        }
    }

    private void UpdateCameraPosition()
    {
        float x = _cameraDistance * MathF.Cos(_cameraPitch) * MathF.Sin(_cameraYaw);
        float y = _cameraDistance * MathF.Sin(_cameraPitch);
        float z = _cameraDistance * MathF.Cos(_cameraPitch) * MathF.Cos(_cameraYaw);
        _cameraPosition = _cameraTarget + new Vector3(x, y, z);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        var point = e.GetCurrentPoint(this);
        _lastMousePos = point.Position;
        
        if (point.Properties.IsLeftButtonPressed)
            _isRotating = true;
        else if (point.Properties.IsMiddleButtonPressed || point.Properties.IsRightButtonPressed)
            _isPanning = true;
        
        Focus();
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isRotating = false;
        _isPanning = false;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        
        var pos = e.GetPosition(this);
        var delta = pos - _lastMousePos;
        _lastMousePos = pos;

        if (_isRotating)
        {
            _cameraYaw += (float)delta.X * 0.01f;
            _cameraPitch += (float)delta.Y * 0.01f;
            _cameraPitch = System.Math.Clamp(_cameraPitch, -MathF.PI / 2 + 0.1f, MathF.PI / 2 - 0.1f);
            UpdateCameraPosition();
            RequestRedraw();
        }
        else if (_isPanning)
        {
            var right = Vector3.Cross(Vector3.UnitY, Vector3.Normalize(_cameraPosition - _cameraTarget));
            var up = Vector3.UnitY;
            float panSpeed = _cameraDistance * 0.002f;
            _cameraTarget -= right * (float)delta.X * panSpeed;
            _cameraTarget += up * (float)delta.Y * panSpeed;
            UpdateCameraPosition();
            RequestRedraw();
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);
        
        _cameraDistance *= (float)(1 - e.Delta.Y * 0.1);
        _cameraDistance = System.Math.Clamp(_cameraDistance, 0.5f, 500f);
        UpdateCameraPosition();
        RequestRedraw();
        
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        float moveSpeed = 0.5f;
        var forward = Vector3.Normalize(_cameraTarget - _cameraPosition);
        var right = Vector3.Cross(Vector3.UnitY, forward);

        switch (e.Key)
        {
            case Key.W:
                _cameraTarget += forward * moveSpeed;
                break;
            case Key.S:
                _cameraTarget -= forward * moveSpeed;
                break;
            case Key.A:
                _cameraTarget += right * moveSpeed;
                break;
            case Key.D:
                _cameraTarget -= right * moveSpeed;
                break;
            case Key.R:
                ResetCamera();
                break;
        }

        UpdateCameraPosition();
        RequestRedraw();
    }
}

public enum RenderMode
{
    Wireframe,
    Solid,
    Textured
}
