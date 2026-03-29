using Godot;

namespace CFHodEd.Godot;

/// <summary>
/// Orbit/pan/zoom camera controller attached to the Camera3D inside SubViewport.
///
/// Controls:
///   Left mouse drag   → Orbit (yaw / pitch)
///   Middle/Right drag → Pan
///   Scroll wheel      → Zoom
///   R                 → Reset camera
///   F                 → Cycle render modes (handled in Main)
/// </summary>
public partial class CameraController : Camera3D
{
    private float _distance = 10f;
    private float _yaw     = 0f;
    private float _pitch   = 0.4f;
    private Vector3 _target = Vector3.Zero;

    private bool _orbiting = false;
    private bool _panning  = false;

    public override void _Ready()
    {
        UpdateTransform();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton btn)
            HandleMouseButton(btn);
        else if (@event is InputEventMouseMotion motion)
            HandleMouseMotion(motion);
        else if (@event is InputEventKey key && key.Pressed && !key.Echo)
            HandleKey(key);
    }

    private void HandleMouseButton(InputEventMouseButton btn)
    {
        switch (btn.ButtonIndex)
        {
            case MouseButton.Left:
                _orbiting = btn.Pressed;
                break;
            case MouseButton.Middle:
            case MouseButton.Right:
                _panning = btn.Pressed;
                break;
            case MouseButton.WheelUp:
                _distance = Mathf.Max(0.5f, _distance * 0.9f);
                UpdateTransform();
                break;
            case MouseButton.WheelDown:
                _distance *= 1.1f;
                UpdateTransform();
                break;
        }
    }

    private void HandleMouseMotion(InputEventMouseMotion motion)
    {
        if (_orbiting)
        {
            _yaw   -= motion.Relative.X * 0.01f;
            _pitch -= motion.Relative.Y * 0.01f;
            _pitch  = Mathf.Clamp(_pitch, -1.5f, 1.5f);
            UpdateTransform();
        }
        else if (_panning)
        {
            var right = GlobalTransform.Basis.X;
            var up    = GlobalTransform.Basis.Y;
            float panSpeed = _distance * 0.001f;
            _target -= right * motion.Relative.X * panSpeed;
            _target += up   * motion.Relative.Y * panSpeed;
            UpdateTransform();
        }
    }

    private void HandleKey(InputEventKey key)
    {
        if (key.Keycode == Key.R)
            ResetCamera();
    }

    private void UpdateTransform()
    {
        float cosP = Mathf.Cos(_pitch);
        float sinP = Mathf.Sin(_pitch);
        float cosY = Mathf.Cos(_yaw);
        float sinY = Mathf.Sin(_yaw);

        var offset = new Vector3(
            _distance * cosP * sinY,
            _distance * sinP,
            _distance * cosP * cosY
        );

        Position = _target + offset;
        LookAt(_target, Vector3.Up);
    }

    /// <summary>Resets to default orbit position.</summary>
    public void ResetCamera()
    {
        _distance = 10f;
        _yaw      = 0f;
        _pitch    = 0.4f;
        _target   = Vector3.Zero;
        UpdateTransform();
    }

    /// <summary>Frames the camera on the given world-space bounding box.</summary>
    public void FocusOnBounds(Aabb bounds)
    {
        _target   = bounds.GetCenter();
        _distance = Mathf.Max(bounds.Size.Length() * 1.5f, 1f);
        UpdateTransform();
    }
}
