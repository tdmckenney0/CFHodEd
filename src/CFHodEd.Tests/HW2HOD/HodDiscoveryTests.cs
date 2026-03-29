using HW2HOD;
using Xunit;

namespace CFHodEd.Tests.HW2HOD;

/// <summary>
/// Runs basic parse checks against every HOD file found in TestData/.
/// Place any .hod files in test/hod-files/ — they are copied here at build time.
/// Tests pass trivially when no files are present.
/// </summary>
public class HodDiscoveryTests
{
    private static IEnumerable<string> HodFiles() => TestFixtures.HodFiles();

    [Fact]
    public void Read_DoesNotThrow()
    {
        foreach (var hodPath in HodFiles())
        {
            var hod = new HOD();
            using var fs = File.OpenRead(hodPath);
            var ex = Record.Exception(() => hod.Read(fs));
            Assert.Null(ex);
        }
    }

    [Fact]
    public void Read_HasExpectedVersion()
    {
        foreach (var hodPath in HodFiles())
        {
            var hod = new HOD();
            using var fs = File.OpenRead(hodPath);
            hod.Read(fs);
            Assert.Equal(0x200, hod.Version);
        }
    }

    [Fact]
    public void Read_HasMeshes()
    {
        foreach (var hodPath in HodFiles())
        {
            var hod = new HOD();
            using var fs = File.OpenRead(hodPath);
            hod.Read(fs);
            Assert.NotEmpty(hod.Meshes);
        }
    }

    [Fact]
    public void Read_HasMaterials()
    {
        foreach (var hodPath in HodFiles())
        {
            var hod = new HOD();
            using var fs = File.OpenRead(hodPath);
            hod.Read(fs);
            Assert.NotEmpty(hod.Materials);
        }
    }

    [Fact]
    public void Read_AllMeshes_HaveVertices()
    {
        foreach (var hodPath in HodFiles())
        {
            var hod = new HOD();
            using var fs = File.OpenRead(hodPath);
            hod.Read(fs);
            foreach (var mesh in hod.Meshes)
                Assert.True(mesh.LODs[0].VertexCount > 0,
                    $"{Path.GetFileName(hodPath)}: mesh '{mesh.Name}' LOD0 has no vertices");
        }
    }

    [Fact]
    public void Read_AllMeshes_HaveTriangleAlignedIndices()
    {
        foreach (var hodPath in HodFiles())
        {
            var hod = new HOD();
            using var fs = File.OpenRead(hodPath);
            hod.Read(fs);
            foreach (var mesh in hod.Meshes)
                Assert.Equal(0, mesh.LODs[0].Indices.Count % 3);
        }
    }

    [Fact]
    public void Read_RootJoint_HasName()
    {
        foreach (var hodPath in HodFiles())
        {
            var hod = new HOD();
            using var fs = File.OpenRead(hodPath);
            hod.Read(fs);
            Assert.False(string.IsNullOrEmpty(hod.Root.Name));
        }
    }

    [Fact]
    public void RoundTrip_WriteAndReadBack_DoesNotThrow()
    {
        foreach (var hodPath in HodFiles())
        {
            var hod = new HOD();
            using var fs = File.OpenRead(hodPath);
            hod.Read(fs);

            using var ms = new MemoryStream();
            hod.Write(ms);
            ms.Position = 0;

            var hod2 = new HOD();
            var ex = Record.Exception(() => hod2.Read(ms));
            Assert.Null(ex);
        }
    }
}
