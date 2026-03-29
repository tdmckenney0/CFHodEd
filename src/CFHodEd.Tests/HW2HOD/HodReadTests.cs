using HW2HOD;
using Xunit;

namespace CFHodEd.Tests.HW2HOD;

/// <summary>
/// Tests for loading the real HOD test fixture (test/meg_starjumper.hod).
/// The .csproj copies the file to TestData/meg_starjumper.hod in the output directory.
/// </summary>
public class HodReadTests
{
    private static readonly string TestHodPath =
        Path.Combine(AppContext.BaseDirectory, "TestData", "meg_starjumper.hod");

    private HOD LoadTestHod()
    {
        Assert.True(File.Exists(TestHodPath), $"Test HOD not found: {TestHodPath}");
        var hod = new HOD();
        using var fs = File.OpenRead(TestHodPath);
        hod.Read(fs);
        return hod;
    }

    [Fact]
    public void Read_TestFile_DoesNotThrow()
    {
        var ex = Record.Exception(() => LoadTestHod());
        Assert.Null(ex);
    }

    [Fact]
    public void Read_TestFile_HasExpectedVersion()
    {
        var hod = LoadTestHod();
        Assert.Equal(0x200, hod.Version);
    }

    [Fact]
    public void Read_TestFile_HasExpectedName()
    {
        var hod = LoadTestHod();
        Assert.Equal("Homeworld2 Multi Mesh File", hod.Name);
    }

    [Fact]
    public void Read_TestFile_HasMeshes()
    {
        var hod = LoadTestHod();
        Assert.NotEmpty(hod.Meshes);
    }

    [Fact]
    public void Read_TestFile_HasMaterials()
    {
        var hod = LoadTestHod();
        Assert.NotEmpty(hod.Materials);
    }

    [Fact]
    public void Read_TestFile_RootJoint_HasName()
    {
        var hod = LoadTestHod();
        Assert.False(string.IsNullOrEmpty(hod.Root.Name));
    }

    [Fact]
    public void Read_TestFile_Meshes_HaveVertices()
    {
        var hod = LoadTestHod();
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
        var hod = LoadTestHod();
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
        var hod = LoadTestHod();
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
