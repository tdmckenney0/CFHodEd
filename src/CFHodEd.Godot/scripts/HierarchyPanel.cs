using Godot;
using HW2HOD;
using System.Collections.Generic;

namespace CFHodEd.Godot;

/// <summary>
/// Populates a Godot Tree control from a loaded HOD and emits SelectionChanged
/// when the user clicks a node.
///
/// Expected scene layout (attached to the VBoxContainer named "HierarchyPanel"):
///   HierarchyPanel (VBoxContainer)  ← this script
///     Tree                          ← direct child named "Tree"
/// </summary>
public partial class HierarchyPanel : VBoxContainer
{
    /// <summary>Fired when a tree item is selected. Payload is the HOD object (Joint, Mesh, etc.).</summary>
    public event Action<object?>? SelectionChanged;

    private Tree _tree = null!;
    private readonly Dictionary<TreeItem, object> _itemToObject = new();

    public override void _Ready()
    {
        _tree = GetNode<Tree>("Tree");
        _tree.ItemSelected += OnItemSelected;
    }

    /// <summary>Clears the tree and rebuilds it from hod.</summary>
    public void Populate(HOD hod)
    {
        _tree.Clear();
        _itemToObject.Clear();

        var root = _tree.CreateItem();
        root.SetText(0, "Model");

        // Joints
        var jointsItem = _tree.CreateItem(root);
        jointsItem.SetText(0, "Joints");
        AddJoint(jointsItem, hod.Root);

        // Meshes
        var meshesItem = _tree.CreateItem(root);
        meshesItem.SetText(0, "Meshes");
        foreach (var mesh in hod.Meshes)
        {
            var meshItem = _tree.CreateItem(meshesItem);
            meshItem.SetText(0, mesh.Name);
            _itemToObject[meshItem] = mesh;

            for (int i = 0; i < mesh.LODs.Count; i++)
            {
                var lodItem = _tree.CreateItem(meshItem);
                lodItem.SetText(0, $"LOD {i}");
                _itemToObject[lodItem] = mesh.LODs[i];
            }
        }

        // Materials
        var matsItem = _tree.CreateItem(root);
        matsItem.SetText(0, "Materials");
        foreach (var mat in hod.Materials)
        {
            var matItem = _tree.CreateItem(matsItem);
            matItem.SetText(0, mat.Name);
            _itemToObject[matItem] = mat;
        }

        // Markers
        var markersItem = _tree.CreateItem(root);
        markersItem.SetText(0, "Markers");
        foreach (var marker in hod.Markers)
        {
            var item = _tree.CreateItem(markersItem);
            item.SetText(0, marker.Name);
            _itemToObject[item] = marker;
        }

        // Effects
        var effectsItem = _tree.CreateItem(root);
        effectsItem.SetText(0, "Effects");

        foreach (var glow in hod.EngineGlows)
        {
            var item = _tree.CreateItem(effectsItem);
            item.SetText(0, $"Glow ({glow.ParentName})");
            _itemToObject[item] = glow;
        }
        foreach (var burn in hod.EngineBurns)
        {
            var item = _tree.CreateItem(effectsItem);
            item.SetText(0, $"Burn ({burn.ParentName})");
            _itemToObject[item] = burn;
        }
        foreach (var light in hod.NavLights)
        {
            var item = _tree.CreateItem(effectsItem);
            item.SetText(0, light.Name);
            _itemToObject[item] = light;
        }

        root.Collapsed = false;
        jointsItem.Collapsed = false;
    }

    private void AddJoint(TreeItem parent, Joint joint)
    {
        var item = _tree.CreateItem(parent);
        item.SetText(0, joint.Name);
        _itemToObject[item] = joint;

        foreach (var child in joint.Children)
            AddJoint(item, child);
    }

    private void OnItemSelected()
    {
        var selected = _tree.GetSelected();
        if (selected != null && _itemToObject.TryGetValue(selected, out var obj))
            SelectionChanged?.Invoke(obj);
        else
            SelectionChanged?.Invoke(null);
    }
}
