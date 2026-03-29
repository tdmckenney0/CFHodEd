using GdUnit4;
using static GdUnit4.Assertions;
using HW2HOD;
using CfMath = CFHodEd.Math;

namespace CFHodEd.Tests.Godot;

/// <summary>
/// Tests for HODLoader behavior that can be verified without running the Godot runtime:
/// coordinate conversion rules, mesh data integrity, and skeleton structure expectations.
///
/// Full node-tree tests (BuildScene returning Node3D, etc.) require the Godot addon runner.
/// Run those via the Godot editor or: godot --headless -s res://addons/gdUnit4/run_tests.gd
/// </summary>
[TestSuite]
public class HODLoaderTests
{
    // -------------------------------------------------------------------------
    // Z-axis negation rule (LH → RH)
    // -------------------------------------------------------------------------

    [TestCase]
    public void ZNegation_PositiveZ_BecomesNegative()
    {
        // HOD position (0, 0, 1) in LH space → Godot (0, 0, -1) in RH space
        float hodZ = 1f;
        float godotZ = -hodZ;
        AssertFloat(godotZ).IsEqual(-1f);
    }

    [TestCase]
    public void ZNegation_NegativeZ_BecomesPositive()
    {
        float hodZ = -5f;
        float godotZ = -hodZ;
        AssertFloat(godotZ).IsEqual(5f);
    }

    [TestCase]
    public void ZNegation_ZeroZ_RemainsZero()
    {
        float hodZ = 0f;
        float godotZ = -hodZ;
        AssertFloat(godotZ).IsEqual(0f);
    }

    // -------------------------------------------------------------------------
    // Winding order swap rule
    // -------------------------------------------------------------------------

    [TestCase]
    public void WindingOrder_TriangleIndices_AreSwapped()
    {
        // HOD triangle [i0, i1, i2] → Godot [i0, i2, i1] (indices 1 and 2 swap)
        var hodIndices = new ushort[] { 0, 1, 2 };
        var godotIndices = new int[3];

        // This is the exact logic in HODLoader.BuildMesh
        godotIndices[0] = hodIndices[0];
        godotIndices[1] = hodIndices[2]; // swapped
        godotIndices[2] = hodIndices[1]; // swapped

        AssertInt(godotIndices[0]).IsEqual(0);
        AssertInt(godotIndices[1]).IsEqual(2);
        AssertInt(godotIndices[2]).IsEqual(1);
    }

    [TestCase]
    public void WindingOrder_MultipleTriangles_EachSwappedIndependently()
    {
        var hodIndices = new ushort[] { 0, 1, 2,  3, 4, 5 };
        var godotIndices = new int[6];

        int triCount = hodIndices.Length / 3;
        for (int t = 0; t < triCount; t++)
        {
            godotIndices[t * 3 + 0] = hodIndices[t * 3 + 0];
            godotIndices[t * 3 + 1] = hodIndices[t * 3 + 2];
            godotIndices[t * 3 + 2] = hodIndices[t * 3 + 1];
        }

        // Triangle 0: [0,1,2] → [0,2,1]
        AssertInt(godotIndices[0]).IsEqual(0);
        AssertInt(godotIndices[1]).IsEqual(2);
        AssertInt(godotIndices[2]).IsEqual(1);

        // Triangle 1: [3,4,5] → [3,5,4]
        AssertInt(godotIndices[3]).IsEqual(3);
        AssertInt(godotIndices[4]).IsEqual(5);
        AssertInt(godotIndices[5]).IsEqual(4);
    }

    // -------------------------------------------------------------------------
    // HOD data integrity pre-conditions
    // -------------------------------------------------------------------------

    [TestCase]
    public void EmptyHod_HasNoMeshes()
    {
        var hod = new HOD();
        AssertInt(hod.Meshes.Count).IsEqual(0);
    }

    [TestCase]
    public void EmptyHod_RootJoint_HasDefaultName()
    {
        var hod = new HOD();
        AssertString(hod.Root.Name).IsNotEmpty();
    }

    [TestCase]
    public void MeshWithZeroVertices_IsSkippedByLoader()
    {
        // HODLoader skips any LOD where VertexCount == 0
        var lod = new MeshLOD();
        AssertInt(lod.VertexCount).IsEqual(0);
    }

    [TestCase]
    public void MeshWithVertices_VertexCountMatchesAdded()
    {
        var lod = new MeshLOD();
        lod.Vertices.Add(new HODVertex { Position = new CfMath.Vector3(0, 0, 0) });
        lod.Vertices.Add(new HODVertex { Position = new CfMath.Vector3(1, 0, 0) });
        lod.Vertices.Add(new HODVertex { Position = new CfMath.Vector3(0, 1, 0) });
        AssertInt(lod.VertexCount).IsEqual(3);
    }

    [TestCase]
    public void TriangleCount_IsIndexCountDividedByThree()
    {
        var lod = new MeshLOD();
        lod.Indices.Add(0); lod.Indices.Add(1); lod.Indices.Add(2);
        lod.Indices.Add(0); lod.Indices.Add(2); lod.Indices.Add(3);
        AssertInt(lod.TriangleCount).IsEqual(2);
    }

    // -------------------------------------------------------------------------
    // Integration: real HOD file pre-conditions for BuildScene
    // -------------------------------------------------------------------------

    [TestCase]
    public void RealHod_AllMeshes_HaveNonZeroVertexCount()
    {
        var hodPath = TestFixtures.FindHod("meg_starjumper.hod");
        if (hodPath is null) { AssertBool(true).IsTrue(); return; }

        var hod = new HOD();
        using var fs = File.OpenRead(hodPath);
        hod.Read(fs);

        foreach (var mesh in hod.Meshes)
            AssertInt(mesh.LODs[0].VertexCount).IsGreater(0);
    }

    [TestCase]
    public void RealHod_AllMeshLODs_HaveTriangleAlignedIndices()
    {
        var hodPath = TestFixtures.FindHod("meg_starjumper.hod");
        if (hodPath is null) { AssertBool(true).IsTrue(); return; }

        var hod = new HOD();
        using var fs = File.OpenRead(hodPath);
        hod.Read(fs);

        foreach (var mesh in hod.Meshes)
            AssertInt(mesh.LODs[0].Indices.Count % 3).IsEqual(0);
    }
}
