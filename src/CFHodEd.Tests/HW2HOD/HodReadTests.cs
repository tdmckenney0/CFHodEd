using HW2HOD;
using Xunit;

namespace CFHodEd.Tests.HW2HOD;

/// <summary>
/// Tests against meg_starjumper.hod specifically.
/// Place the file in test/hod-files/meg_starjumper.hod — fixture tests return early (pass trivially) when absent.
/// The .csproj copies all files from test/hod-files/ to TestData/ in the output directory.
/// </summary>
public class HodReadTests
{
    private static readonly string? TestHodPath = TestFixtures.FindHod("meg_starjumper.hod");

    /// <summary>Returns null when the fixture file is absent; callers return early in that case.</summary>
    private HOD? TryLoadTestHod()
    {
        if (TestHodPath is null)
            return null;
        var hod = new HOD();
        using var fs = File.OpenRead(TestHodPath);
        hod.Read(fs);
        return hod;
    }

    [Fact]
    public void Read_TestFile_DoesNotThrow()
    {
        if (TestHodPath is null) return;
        var ex = Record.Exception(() => TryLoadTestHod());
        Assert.Null(ex);
    }

    [Fact]
    public void Read_TestFile_HasExpectedVersion()
    {
        var hod = TryLoadTestHod();
        if (hod is null) return;
        Assert.Equal(0x200, hod.Version);
    }

    [Fact]
    public void Read_TestFile_HasExpectedName()
    {
        var hod = TryLoadTestHod();
        if (hod is null) return;
        Assert.Equal("Homeworld2 Multi Mesh File", hod.Name);
    }

    [Fact]
    public void Read_TestFile_HasMeshes()
    {
        var hod = TryLoadTestHod();
        if (hod is null) return;
        Assert.NotEmpty(hod.Meshes);
    }

    [Fact]
    public void Read_TestFile_HasMaterials()
    {
        var hod = TryLoadTestHod();
        if (hod is null) return;
        Assert.NotEmpty(hod.Materials);
    }

    [Fact]
    public void Read_TestFile_RootJoint_HasName()
    {
        var hod = TryLoadTestHod();
        if (hod is null) return;
        Assert.False(string.IsNullOrEmpty(hod.Root.Name));
    }

    [Fact]
    public void Read_TestFile_Meshes_HaveVertices()
    {
        var hod = TryLoadTestHod();
        if (hod is null) return;
        foreach (var mesh in hod.Meshes)
        {
            Assert.NotEmpty(mesh.LODs);
            // At least LOD 0 should have vertices
            Assert.True(mesh.LODs[0].VertexCount > 0, $"Mesh '{mesh.Name}' LOD0 has no vertices");
        }
    }

    [Fact]
    public void Read_TestFile_Meshes_HaveIndices()
    {
        var hod = TryLoadTestHod();
        if (hod is null) return;
        foreach (var mesh in hod.Meshes)
        {
            var lod0 = mesh.LODs[0];
            Assert.True(lod0.Indices.Count > 0, $"Mesh '{mesh.Name}' LOD0 has no indices");
            // Triangle list: index count must be a multiple of 3
            Assert.Equal(0, lod0.Indices.Count % 3);
        }
    }

    [Fact]
    public void Read_TestFile_Materials_HaveNames()
    {
        var hod = TryLoadTestHod();
        if (hod is null) return;
        foreach (var mat in hod.Materials)
            Assert.False(string.IsNullOrEmpty(mat.Name));
    }

    [Fact]
    public void HOD_Initialize_ResetsToDefaults()
    {
        var hod = new HOD();
        hod.Initialize();

        Assert.Equal(0x200, hod.Version);
        Assert.Equal("Homeworld2 Multi Mesh File", hod.Name);
        Assert.Empty(hod.Meshes);
        Assert.Empty(hod.Materials);
        Assert.Empty(hod.Textures);
        Assert.Empty(hod.Markers);
    }

    [Fact]
    public void HOD_Version_RejectsInvalidValue()
    {
        var hod = new HOD();
        Assert.Throws<ArgumentException>(() => hod.Version = 999);
    }

    [Fact]
    public void HOD_ThrusterPower_ClampedToZeroOne()
    {
        var hod = new HOD();
        hod.ThrusterPower = 2f;
        Assert.Equal(1f, hod.ThrusterPower);
        hod.ThrusterPower = -1f;
        Assert.Equal(0f, hod.ThrusterPower);
    }

    [Fact]
    public void HOD_Read_NullStream_Throws()
    {
        var hod = new HOD();
        Assert.Throws<ArgumentNullException>(() => hod.Read(null!));
    }

    [Fact]
    public void HOD_Write_NullStream_Throws()
    {
        var hod = new HOD();
        Assert.Throws<ArgumentNullException>(() => hod.Write(null!));
    }

    [Fact]
    public void HOD_Write_DoesNotThrow()
    {
        var hod = new HOD();
        using var ms = new MemoryStream();
        var ex = Record.Exception(() => hod.Write(ms));
        Assert.Null(ex);
    }
}
