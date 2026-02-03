using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
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

    public event Action? RenderFrame;

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
