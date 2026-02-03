using System.Collections.ObjectModel;
using HW2HOD;
using Vector3 = CFHodEd.Math.Vector3;

namespace CFHodEd.UI.ViewModels;

/// <summary>
/// Types of items in the hierarchy tree.
/// </summary>
public enum HierarchyItemType
{
    Root,
    Joint,
    Mesh,
    MeshLOD,
    Material,
    Marker,
    EngineGlow,
    EngineBurn,
    NavLight,
    Category
}

/// <summary>
/// ViewModel for a single item in the hierarchy tree.
/// </summary>
public class HierarchyItemViewModel : ViewModelBase
{
    private string _name = "";
    private bool _isExpanded;
    private bool _isSelected;
    private HierarchyItemType _itemType;
    private object? _data;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public HierarchyItemType ItemType
    {
        get => _itemType;
        set => SetProperty(ref _itemType, value);
    }

    /// <summary>Underlying data object (Joint, Mesh, Material, etc.)</summary>
    public object? Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }

    public ObservableCollection<HierarchyItemViewModel> Children { get; } = new();

    public HierarchyItemViewModel() { }

    public HierarchyItemViewModel(string name, HierarchyItemType type, object? data = null)
    {
        _name = name;
        _itemType = type;
        _data = data;
        _isExpanded = type is HierarchyItemType.Root or HierarchyItemType.Category;
    }

    /// <summary>Creates a hierarchy tree from a HOD file.</summary>
    public static HierarchyItemViewModel FromHOD(HOD hod)
    {
        var root = new HierarchyItemViewModel("Model", HierarchyItemType.Root, hod);
        root.IsExpanded = true;

        // Add joint hierarchy
        if (hod.Root != null)
        {
            var jointsCategory = new HierarchyItemViewModel("Joints", HierarchyItemType.Category);
            AddJointRecursive(jointsCategory, hod.Root);
            root.Children.Add(jointsCategory);
        }

        // Add meshes
        if (hod.Meshes.Count > 0)
        {
            var meshesCategory = new HierarchyItemViewModel($"Meshes ({hod.Meshes.Count})", HierarchyItemType.Category);
            foreach (var mesh in hod.Meshes)
            {
                var meshItem = new HierarchyItemViewModel(mesh.Name, HierarchyItemType.Mesh, mesh);
                foreach (var lod in mesh.LODs)
                {
                    meshItem.Children.Add(new HierarchyItemViewModel(
                        $"LOD ({lod.VertexCount} verts, {lod.TriangleCount} tris)",
                        HierarchyItemType.MeshLOD, lod));
                }
                meshesCategory.Children.Add(meshItem);
            }
            root.Children.Add(meshesCategory);
        }

        // Add materials
        if (hod.Materials.Count > 0)
        {
            var materialsCategory = new HierarchyItemViewModel($"Materials ({hod.Materials.Count})", HierarchyItemType.Category);
            foreach (var mat in hod.Materials)
            {
                materialsCategory.Children.Add(new HierarchyItemViewModel(mat.Name, HierarchyItemType.Material, mat));
            }
            root.Children.Add(materialsCategory);
        }

        // Add markers
        if (hod.Markers.Count > 0)
        {
            var markersCategory = new HierarchyItemViewModel($"Markers ({hod.Markers.Count})", HierarchyItemType.Category);
            foreach (var marker in hod.Markers)
            {
                markersCategory.Children.Add(new HierarchyItemViewModel(marker.Name, HierarchyItemType.Marker, marker));
            }
            root.Children.Add(markersCategory);
        }

        // Add engine effects
        int effectCount = hod.EngineGlows.Count + hod.EngineBurns.Count + hod.NavLights.Count;
        if (effectCount > 0)
        {
            var effectsCategory = new HierarchyItemViewModel($"Effects ({effectCount})", HierarchyItemType.Category);
            
            foreach (var glow in hod.EngineGlows)
                effectsCategory.Children.Add(new HierarchyItemViewModel($"Glow: {glow.ParentName}", HierarchyItemType.EngineGlow, glow));
            
            foreach (var burn in hod.EngineBurns)
                effectsCategory.Children.Add(new HierarchyItemViewModel($"Burn: {burn.ParentName}", HierarchyItemType.EngineBurn, burn));
            
            foreach (var nav in hod.NavLights)
                effectsCategory.Children.Add(new HierarchyItemViewModel($"NavLight: {nav.Name}", HierarchyItemType.NavLight, nav));
            
            root.Children.Add(effectsCategory);
        }

        return root;
    }

    private static void AddJointRecursive(HierarchyItemViewModel parent, Joint joint)
    {
        var jointItem = new HierarchyItemViewModel(joint.Name, HierarchyItemType.Joint, joint);
        jointItem.IsExpanded = true;
        
        foreach (var child in joint.Children)
            AddJointRecursive(jointItem, child);
        
        parent.Children.Add(jointItem);
    }
}
