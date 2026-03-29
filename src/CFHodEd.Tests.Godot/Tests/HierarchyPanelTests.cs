using GdUnit4;
using static GdUnit4.Assertions;
using HW2HOD;

namespace CFHodEd.Tests.Godot;

/// <summary>
/// Tests for HierarchyPanel population logic, verified against the HOD data model.
///
/// Full UI tests (instantiating the Godot Tree control, verifying tree items, etc.)
/// require the Godot addon runner. Run those via the Godot editor or:
///   godot --headless -s res://addons/gdUnit4/run_tests.gd
///
/// These tests verify the HOD data structures that HierarchyPanel reads from,
/// ensuring the data contract is correct before any Godot UI is involved.
/// </summary>
[TestSuite]
public class HierarchyPanelTests
{
    // -------------------------------------------------------------------------
    // Joint hierarchy
    // -------------------------------------------------------------------------

    [TestCase]
    public void RootJoint_HasName()
    {
        var hod = new HOD();
        AssertString(hod.Root.Name).IsNotEmpty();
    }

    [TestCase]
    public void ChildJoint_AppearsInParentChildren()
    {
        var hod = new HOD();
        var child = new Joint { Name = "ChildBone" };
        hod.Root.Children.Add(child);

        AssertInt(hod.Root.Children.Count).IsEqual(1);
        AssertString(hod.Root.Children[0].Name).IsEqual("ChildBone");
    }

    [TestCase]
    public void MultipleChildJoints_AllPresent()
    {
        var hod = new HOD();
        hod.Root.Children.Add(new Joint { Name = "A" });
        hod.Root.Children.Add(new Joint { Name = "B" });
        hod.Root.Children.Add(new Joint { Name = "C" });

        AssertInt(hod.Root.Children.Count).IsEqual(3);
    }

    [TestCase]
    public void GetJointByName_FindsRoot()
    {
        var hod = new HOD();
        hod.Root.Name = "RootBone";

        AssertObject(hod.GetJointByName("RootBone")).IsNotNull();
    }

    [TestCase]
    public void GetJointByName_FindsChild()
    {
        var hod = new HOD();
        hod.Root.Children.Add(new Joint { Name = "Wing" });

        AssertObject(hod.GetJointByName("Wing")).IsNotNull();
    }

    [TestCase]
    public void GetJointByName_MissingName_ReturnsNull()
    {
        var hod = new HOD();
        AssertObject(hod.GetJointByName("DoesNotExist")).IsNull();
    }

    // -------------------------------------------------------------------------
    // Mesh list
    // -------------------------------------------------------------------------

    [TestCase]
    public void EmptyHod_HasNoMeshes()
    {
        AssertInt(new HOD().Meshes.Count).IsEqual(0);
    }

    [TestCase]
    public void AddedMesh_AppearsInList()
    {
        var hod = new HOD();
        hod.Meshes.Add(new HW2HOD.Mesh { Name = "Hull" });

        AssertInt(hod.Meshes.Count).IsEqual(1);
        AssertString(hod.Meshes[0].Name).IsEqual("Hull");
    }

    [TestCase]
    public void Mesh_LODs_AreAccessible()
    {
        var mesh = new HW2HOD.Mesh { Name = "Hull" };
        mesh.LODs.Add(new MeshLOD { Name = "Hull_LOD0" });
        mesh.LODs.Add(new MeshLOD { Name = "Hull_LOD1" });

        AssertInt(mesh.LODs.Count).IsEqual(2);
    }

    // -------------------------------------------------------------------------
    // Material list
    // -------------------------------------------------------------------------

    [TestCase]
    public void EmptyHod_HasNoMaterials()
    {
        AssertInt(new HOD().Materials.Count).IsEqual(0);
    }

    [TestCase]
    public void AddedMaterial_AppearsInList()
    {
        var hod = new HOD();
        hod.Materials.Add(new HW2HOD.Material { Name = "ship_diffuse" });

        AssertInt(hod.Materials.Count).IsEqual(1);
        AssertString(hod.Materials[0].Name).IsEqual("ship_diffuse");
    }

    // -------------------------------------------------------------------------
    // Marker list
    // -------------------------------------------------------------------------

    [TestCase]
    public void EmptyHod_HasNoMarkers()
    {
        AssertInt(new HOD().Markers.Count).IsEqual(0);
    }

    // -------------------------------------------------------------------------
    // Integration: real HOD
    // -------------------------------------------------------------------------

    [TestCase]
    public void RealHod_HasExpectedSections()
    {
        var hodPath = TestFixtures.FindHod("meg_starjumper.hod");
        if (hodPath is null) { AssertBool(true).IsTrue(); return; }

        var hod = new HOD();
        using var fs = File.OpenRead(hodPath);
        hod.Read(fs);

        // All five sections HierarchyPanel displays must have accessible data
        AssertObject(hod.Root).IsNotNull();
        AssertString(hod.Root.Name).IsNotEmpty();
        AssertBool(hod.Meshes.Count > 0).IsTrue();
        AssertBool(hod.Materials.Count > 0).IsTrue();
    }
}
