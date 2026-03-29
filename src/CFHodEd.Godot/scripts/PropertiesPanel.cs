using Godot;
using HW2HOD;
using CfMath = CFHodEd.Math;
using HodMaterial = HW2HOD.Material;

namespace CFHodEd.Godot;

/// <summary>
/// Shows editable properties for the currently selected HOD object.
///
/// Expected scene layout (attached to the VBoxContainer named "PropertiesPanel"):
///   PropertiesPanel (VBoxContainer)   ← this script
///     TransformSection (VBoxContainer, unique name)
///       TransformGrid (GridContainer)
///         PosX … ScaleZ (SpinBox × 9, unique names)
///     MaterialSection (VBoxContainer, unique name)
///       ShaderOption (OptionButton, unique name)
///       DiffuseEdit, GlowEdit, NormalEdit (LineEdit × 3, unique names)
///     TeamColorsSection (VBoxContainer, unique name)
///       TeamColorPicker, StripeColorPicker (ColorPickerButton × 2, unique names)
/// </summary>
public partial class PropertiesPanel : VBoxContainer
{
    // Sections
    private Control _transformSection = null!;
    private Control _materialSection  = null!;
    private Control _teamColorsSection = null!;

    // Transform spinboxes
    private SpinBox _posX = null!, _posY = null!, _posZ = null!;
    private SpinBox _rotX = null!, _rotY = null!, _rotZ = null!;
    private SpinBox _scaleX = null!, _scaleY = null!, _scaleZ = null!;

    // Material controls
    private OptionButton _shaderOption = null!;
    private LineEdit _diffuseEdit = null!, _glowEdit = null!, _normalEdit = null!;

    // Team color pickers
    private ColorPickerButton _teamColorPicker   = null!;
    private ColorPickerButton _stripeColorPicker = null!;

    // State
    private Joint?    _selectedJoint;
    private HodMaterial? _selectedMaterial;
    private HOD?      _currentHod;
    private bool      _updating;

    public override void _Ready()
    {
        _transformSection  = GetNode<Control>("%TransformSection");
        _materialSection   = GetNode<Control>("%MaterialSection");
        _teamColorsSection = GetNode<Control>("%TeamColorsSection");

        _posX   = GetNode<SpinBox>("%PosX");
        _posY   = GetNode<SpinBox>("%PosY");
        _posZ   = GetNode<SpinBox>("%PosZ");
        _rotX   = GetNode<SpinBox>("%RotX");
        _rotY   = GetNode<SpinBox>("%RotY");
        _rotZ   = GetNode<SpinBox>("%RotZ");
        _scaleX = GetNode<SpinBox>("%ScaleX");
        _scaleY = GetNode<SpinBox>("%ScaleY");
        _scaleZ = GetNode<SpinBox>("%ScaleZ");

        _shaderOption  = GetNode<OptionButton>("%ShaderOption");
        _diffuseEdit   = GetNode<LineEdit>("%DiffuseEdit");
        _glowEdit      = GetNode<LineEdit>("%GlowEdit");
        _normalEdit    = GetNode<LineEdit>("%NormalEdit");

        _teamColorPicker   = GetNode<ColorPickerButton>("%TeamColorPicker");
        _stripeColorPicker = GetNode<ColorPickerButton>("%StripeColorPicker");

        // Populate shader dropdown options
        foreach (var shader in new[] { "ship", "matte", "background", "thruster" })
            _shaderOption.AddItem(shader);

        // Wire transform spinboxes
        _posX.ValueChanged   += _ => OnTransformChanged();
        _posY.ValueChanged   += _ => OnTransformChanged();
        _posZ.ValueChanged   += _ => OnTransformChanged();
        _rotX.ValueChanged   += _ => OnTransformChanged();
        _rotY.ValueChanged   += _ => OnTransformChanged();
        _rotZ.ValueChanged   += _ => OnTransformChanged();
        _scaleX.ValueChanged += _ => OnTransformChanged();
        _scaleY.ValueChanged += _ => OnTransformChanged();
        _scaleZ.ValueChanged += _ => OnTransformChanged();

        // Wire team color pickers
        _teamColorPicker.ColorChanged += color =>
        {
            if (_currentHod != null && !_updating)
                _currentHod.TeamColor = new CfMath.ColorValue(color.R, color.G, color.B, color.A);
        };
        _stripeColorPicker.ColorChanged += color =>
        {
            if (_currentHod != null && !_updating)
                _currentHod.StripeColor = new CfMath.ColorValue(color.R, color.G, color.B, color.A);
        };

        HideAll();
    }

    /// <summary>
    /// Updates the panel to show properties for the given HOD object.
    /// Pass null to clear the panel.
    /// </summary>
    public void ShowProperties(object? item, HOD? hod)
    {
        _currentHod      = hod;
        _selectedJoint   = null;
        _selectedMaterial = null;

        HideAll();

        if (item is Joint joint)
        {
            _selectedJoint = joint;
            _transformSection.Visible = true;

            _updating = true;
            _posX.Value   = joint.Position.X;
            _posY.Value   = joint.Position.Y;
            _posZ.Value   = joint.Position.Z;
            _rotX.Value   = joint.Rotation.X;
            _rotY.Value   = joint.Rotation.Y;
            _rotZ.Value   = joint.Rotation.Z;
            _scaleX.Value = joint.Scale.X;
            _scaleY.Value = joint.Scale.Y;
            _scaleZ.Value = joint.Scale.Z;
            _updating = false;
        }
        else if (item is HodMaterial mat)
        {
            _selectedMaterial = mat;
            _materialSection.Visible = true;

            _updating = true;
            // OptionButton.GetItemIndex takes an item ID (int), not text.
            // Search by text instead.
            int shaderIdx = -1;
            for (int i = 0; i < _shaderOption.ItemCount; i++)
            {
                if (_shaderOption.GetItemText(i) == mat.ShaderName)
                {
                    shaderIdx = i;
                    break;
                }
            }
            if (shaderIdx >= 0)
                _shaderOption.Selected = shaderIdx;
            _diffuseEdit.Text = mat.ShaderParameters.Diffuse.Name;
            _glowEdit.Text    = mat.ShaderParameters.Glow.Name;
            _normalEdit.Text  = mat.ShaderParameters.Normal.Name;
            _updating = false;
        }

        // Always show team colors when any HOD is loaded
        if (hod != null)
        {
            _teamColorsSection.Visible = true;
            _updating = true;
            _teamColorPicker.Color   = new Color(hod.TeamColor.R,   hod.TeamColor.G,   hod.TeamColor.B,   hod.TeamColor.A);
            _stripeColorPicker.Color = new Color(hod.StripeColor.R, hod.StripeColor.G, hod.StripeColor.B, hod.StripeColor.A);
            _updating = false;
        }
    }

    private void OnTransformChanged()
    {
        if (_updating || _selectedJoint == null)
            return;

        _selectedJoint.Position = new CfMath.Vector3(
            (float)_posX.Value, (float)_posY.Value, (float)_posZ.Value);
        _selectedJoint.Rotation = new CfMath.Vector3(
            (float)_rotX.Value, (float)_rotY.Value, (float)_rotZ.Value);
        _selectedJoint.Scale = new CfMath.Vector3(
            (float)_scaleX.Value, (float)_scaleY.Value, (float)_scaleZ.Value);
    }

    private void HideAll()
    {
        _transformSection.Visible  = false;
        _materialSection.Visible   = false;
        _teamColorsSection.Visible = false;
    }
}
