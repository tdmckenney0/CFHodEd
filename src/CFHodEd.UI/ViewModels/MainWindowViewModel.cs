using System.Collections.ObjectModel;
using HW2HOD;
using Avalonia.Media;
using Vector3 = CFHodEd.Math.Vector3;

namespace CFHodEd.UI.ViewModels;

/// <summary>
/// Main window view model with property bindings.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private HOD? _currentHod;
    private string? _currentFilePath;
    private string _statusText = "Ready";
    private HierarchyItemViewModel? _selectedItem;
    private bool _hasModel;

    // Transform properties
    private float _positionX, _positionY, _positionZ;
    private float _rotationX, _rotationY, _rotationZ;
    private float _scaleX = 1, _scaleY = 1, _scaleZ = 1;

    // Material properties
    private string _shaderName = "ship";
    private string _diffuseTexture = "";
    private string _glowTexture = "";
    private string _normalTexture = "";

    // Team colors
    private Color _teamColor = Color.FromRgb(128, 128, 128);
    private Color _stripeColor = Color.FromRgb(128, 128, 128);

    // Hierarchy
    public ObservableCollection<HierarchyItemViewModel> HierarchyItems { get; } = new();

    public HOD? CurrentHod
    {
        get => _currentHod;
        set
        {
            if (SetProperty(ref _currentHod, value))
            {
                HasModel = value != null;
                RefreshHierarchy();
            }
        }
    }

    public string? CurrentFilePath
    {
        get => _currentFilePath;
        set
        {
            if (SetProperty(ref _currentFilePath, value))
                OnPropertyChanged(nameof(WindowTitle));
        }
    }

    public string WindowTitle
    {
        get
        {
            string fileName = _currentFilePath != null 
                ? System.IO.Path.GetFileName(_currentFilePath) 
                : "Untitled";
            return $"CFHodEd - {fileName}";
        }
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public bool HasModel
    {
        get => _hasModel;
        private set => SetProperty(ref _hasModel, value);
    }

    public HierarchyItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
                UpdatePropertiesFromSelection();
        }
    }

    // Transform properties
    public float PositionX
    {
        get => _positionX;
        set { if (SetProperty(ref _positionX, value)) ApplyTransformToSelection(); }
    }

    public float PositionY
    {
        get => _positionY;
        set { if (SetProperty(ref _positionY, value)) ApplyTransformToSelection(); }
    }

    public float PositionZ
    {
        get => _positionZ;
        set { if (SetProperty(ref _positionZ, value)) ApplyTransformToSelection(); }
    }

    public float RotationX
    {
        get => _rotationX;
        set { if (SetProperty(ref _rotationX, value)) ApplyTransformToSelection(); }
    }

    public float RotationY
    {
        get => _rotationY;
        set { if (SetProperty(ref _rotationY, value)) ApplyTransformToSelection(); }
    }

    public float RotationZ
    {
        get => _rotationZ;
        set { if (SetProperty(ref _rotationZ, value)) ApplyTransformToSelection(); }
    }

    public float ScaleX
    {
        get => _scaleX;
        set { if (SetProperty(ref _scaleX, value)) ApplyTransformToSelection(); }
    }

    public float ScaleY
    {
        get => _scaleY;
        set { if (SetProperty(ref _scaleY, value)) ApplyTransformToSelection(); }
    }

    public float ScaleZ
    {
        get => _scaleZ;
        set { if (SetProperty(ref _scaleZ, value)) ApplyTransformToSelection(); }
    }

    // Material properties
    public string ShaderName
    {
        get => _shaderName;
        set { if (SetProperty(ref _shaderName, value)) ApplyMaterialToSelection(); }
    }

    public string DiffuseTexture
    {
        get => _diffuseTexture;
        set { if (SetProperty(ref _diffuseTexture, value)) ApplyMaterialToSelection(); }
    }

    public string GlowTexture
    {
        get => _glowTexture;
        set { if (SetProperty(ref _glowTexture, value)) ApplyMaterialToSelection(); }
    }

    public string NormalTexture
    {
        get => _normalTexture;
        set { if (SetProperty(ref _normalTexture, value)) ApplyMaterialToSelection(); }
    }

    // Team colors
    public Color TeamColor
    {
        get => _teamColor;
        set
        {
            if (SetProperty(ref _teamColor, value) && _currentHod != null)
            {
                _currentHod.TeamColor = new CFHodEd.Math.ColorValue(
                    value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
            }
        }
    }

    public Color StripeColor
    {
        get => _stripeColor;
        set
        {
            if (SetProperty(ref _stripeColor, value) && _currentHod != null)
            {
                _currentHod.StripeColor = new CFHodEd.Math.ColorValue(
                    value.R / 255f, value.G / 255f, value.B / 255f, value.A / 255f);
            }
        }
    }

    public void NewFile()
    {
        CurrentHod = new HOD();
        CurrentHod.Initialize();
        CurrentFilePath = null;
        StatusText = "New file created";
    }

    public void RefreshHierarchy()
    {
        HierarchyItems.Clear();

        if (_currentHod == null)
        {
            HierarchyItems.Add(new HierarchyItemViewModel("(No model loaded)", HierarchyItemType.Root));
            return;
        }

        var root = HierarchyItemViewModel.FromHOD(_currentHod);
        HierarchyItems.Add(root);

        // Update team colors from HOD
        var tc = _currentHod.TeamColor;
        _teamColor = Color.FromArgb((byte)(tc.A * 255), (byte)(tc.R * 255), (byte)(tc.G * 255), (byte)(tc.B * 255));
        OnPropertyChanged(nameof(TeamColor));

        var sc = _currentHod.StripeColor;
        _stripeColor = Color.FromArgb((byte)(sc.A * 255), (byte)(sc.R * 255), (byte)(sc.G * 255), (byte)(sc.B * 255));
        OnPropertyChanged(nameof(StripeColor));
    }

    private void UpdatePropertiesFromSelection()
    {
        if (_selectedItem?.Data == null) return;

        switch (_selectedItem.Data)
        {
            case Joint joint:
                _positionX = joint.Position.X;
                _positionY = joint.Position.Y;
                _positionZ = joint.Position.Z;
                _rotationX = joint.Rotation.X;
                _rotationY = joint.Rotation.Y;
                _rotationZ = joint.Rotation.Z;
                _scaleX = joint.Scale.X;
                _scaleY = joint.Scale.Y;
                _scaleZ = joint.Scale.Z;
                break;

            case Marker marker:
                _positionX = marker.Position.X;
                _positionY = marker.Position.Y;
                _positionZ = marker.Position.Z;
                _rotationX = marker.Rotation.X;
                _rotationY = marker.Rotation.Y;
                _rotationZ = marker.Rotation.Z;
                _scaleX = 1; _scaleY = 1; _scaleZ = 1;
                break;

            case Material material:
                _shaderName = material.ShaderName;
                _diffuseTexture = material.ShaderParameters.Diffuse.Name;
                _glowTexture = material.ShaderParameters.Glow.Name;
                _normalTexture = material.ShaderParameters.Normal.Name;
                OnPropertyChanged(nameof(ShaderName));
                OnPropertyChanged(nameof(DiffuseTexture));
                OnPropertyChanged(nameof(GlowTexture));
                OnPropertyChanged(nameof(NormalTexture));
                break;
        }

        OnPropertyChanged(nameof(PositionX));
        OnPropertyChanged(nameof(PositionY));
        OnPropertyChanged(nameof(PositionZ));
        OnPropertyChanged(nameof(RotationX));
        OnPropertyChanged(nameof(RotationY));
        OnPropertyChanged(nameof(RotationZ));
        OnPropertyChanged(nameof(ScaleX));
        OnPropertyChanged(nameof(ScaleY));
        OnPropertyChanged(nameof(ScaleZ));
    }

    private void ApplyTransformToSelection()
    {
        if (_selectedItem?.Data == null) return;

        switch (_selectedItem.Data)
        {
            case Joint joint:
                joint.Position = new Vector3(_positionX, _positionY, _positionZ);
                joint.Rotation = new Vector3(_rotationX, _rotationY, _rotationZ);
                joint.Scale = new Vector3(_scaleX, _scaleY, _scaleZ);
                break;

            case Marker marker:
                marker.Position = new Vector3(_positionX, _positionY, _positionZ);
                marker.Rotation = new Vector3(_rotationX, _rotationY, _rotationZ);
                break;
        }
    }

    private void ApplyMaterialToSelection()
    {
        if (_selectedItem?.Data is Material material)
        {
            material.ShaderName = _shaderName;
            // Note: TextureParameters is a readonly struct, so we need to replace the whole thing
            // For now, just update the shader name. Full texture editing requires more complex handling.
        }
    }
}
