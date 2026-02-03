using HW2HOD;

Console.WriteLine("Loading HOD file...");
var hod = new HOD();
try 
{
    using var fs = File.OpenRead("/home/stella/Projects/CFHodEd-master/test/meg_starjumper.hod");
    hod.Read(fs);
}
catch (Exception ex) 
{
    Console.WriteLine($"ERROR: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

Console.WriteLine($"Meshes: {hod.Meshes.Count}");
foreach (var m in hod.Meshes)
{
    Console.WriteLine($"  Mesh: {m.Name}, Parent: {m.ParentJoint}, LODs: {m.LODs.Count}");
    foreach (var lod in m.LODs)
    {
        Console.WriteLine($"    LOD: {lod.Name}, Verts={lod.Vertices.Count}, Indices={lod.Indices.Count}");
    }
}

Console.WriteLine($"Materials: {hod.Materials.Count}");
foreach (var mat in hod.Materials)
{
    Console.WriteLine($"  Material: {mat.Name}, Shader: {mat.ShaderName}");
}
