using Godot;
using HW2HOD;
using CfMath = CFHodEd.Math;
using HodMaterial = HW2HOD.Material;

namespace CFHodEd.Godot;

/// <summary>
/// Converts a loaded HOD object into a Godot 3D scene subtree.
///
/// Coordinate system: HOD/DirectX is left-handed (camera toward +Z).
/// Godot is right-handed (camera toward -Z).
/// Conversion: negate Z on all positions and normals; flip winding order.
/// </summary>
public static class HODLoader
{
    /// <summary>
    /// Builds a Node3D subtree from a HOD. Returns the root node,
    /// ready to be added to HodModelRoot in the viewport.
    /// </summary>
    public static Node3D BuildScene(HOD hod)
    {
        var root = new Node3D { Name = "HodModel" };

        // Build skeleton from joint hierarchy
        var skeleton = new Skeleton3D { Name = "Skeleton" };
        root.AddChild(skeleton);
        skeleton.Owner = root;
        BuildSkeleton(skeleton, hod.Root, -1);

        // Build Godot materials indexed by HOD material index
        var materials = BuildMaterials(hod);

        // Build mesh instances (LOD 0 only)
        foreach (var mesh in hod.Meshes)
        {
            if (mesh.LODs.Count == 0)
                continue;

            var lod0 = mesh.LODs[0];
            if (lod0.VertexCount == 0)
                continue;

            var arrayMesh = BuildMesh(lod0);
            var instance = new MeshInstance3D { Name = mesh.Name, Mesh = arrayMesh };

            if (lod0.MaterialIndex >= 0 && lod0.MaterialIndex < materials.Length)
                instance.SetSurfaceOverrideMaterial(0, materials[lod0.MaterialIndex]);

            // Attach to the appropriate skeleton bone
            int boneIdx = skeleton.FindBone(mesh.ParentJoint);
            if (boneIdx >= 0)
                instance.Skeleton = instance.GetPathTo(skeleton);

            root.AddChild(instance);
            instance.Owner = root;
        }

        return root;
    }

    /// <summary>
    /// Replaces all MeshInstance3D children of modelRoot with a fresh build.
    /// Called when joint transforms are edited in the Properties panel.
    /// </summary>
    public static void RebuildSkeleton(Node3D modelRoot, HOD hod)
    {
        var skeleton = modelRoot.GetNodeOrNull<Skeleton3D>("Skeleton");
        if (skeleton == null)
            return;

        // Update bone rest poses from current joint data
        var joints = hod.Root.ToArray();
        for (int i = 0; i < skeleton.GetBoneCount() && i < joints.Length; i++)
        {
            var joint = joints[i];
            skeleton.SetBoneRest(i, BuildBoneTransform(joint));
        }
    }

    // -------------------------------------------------------------------------
    // Mesh construction
    // -------------------------------------------------------------------------

    private static ArrayMesh BuildMesh(MeshLOD lod)
    {
        int vCount = lod.VertexCount;
        var positions = new global::Godot.Vector3[vCount];
        var normals   = new global::Godot.Vector3[vCount];
        var uvs       = new global::Godot.Vector2[vCount];

        for (int i = 0; i < vCount; i++)
        {
            var v = lod.Vertices[i];
            positions[i] = FlipZ(v.Position);
            normals[i]   = FlipZ(v.Normal);
            uvs[i]       = new global::Godot.Vector2(v.TexCoords.X, v.TexCoords.Y);
        }

        // Flip winding order to compensate for Z-negation
        int triCount = lod.TriangleCount;
        var indices = new int[triCount * 3];
        for (int t = 0; t < triCount; t++)
        {
            indices[t * 3 + 0] = lod.Indices[t * 3 + 0];
            indices[t * 3 + 1] = lod.Indices[t * 3 + 2]; // swapped
            indices[t * 3 + 2] = lod.Indices[t * 3 + 1]; // swapped
        }

        var arrays = new global::Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = positions;
        arrays[(int)ArrayMesh.ArrayType.Normal]  = normals;
        arrays[(int)ArrayMesh.ArrayType.TexUV]   = uvs;
        arrays[(int)ArrayMesh.ArrayType.Index]   = indices;

        var mesh = new ArrayMesh();
        mesh.AddSurfaceFromArrays(global::Godot.Mesh.PrimitiveType.Triangles, arrays);
        return mesh;
    }

    // -------------------------------------------------------------------------
    // Material construction
    // -------------------------------------------------------------------------

    private static StandardMaterial3D[] BuildMaterials(HOD hod)
    {
        var result = new StandardMaterial3D[hod.Materials.Count];
        for (int i = 0; i < hod.Materials.Count; i++)
            result[i] = BuildMaterial(hod.Materials[i], hod);
        return result;
    }

    private static StandardMaterial3D BuildMaterial(HodMaterial mat, HOD hod)
    {
        var godotMat = new StandardMaterial3D
        {
            AlbedoColor = Colors.White,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.PerPixel,
        };

        // Load diffuse texture
        var diffuseParam = mat.ShaderParameters.Diffuse;
        if (diffuseParam.HasTexture && diffuseParam.TextureIndex < hod.Textures.Count)
        {
            var tex = LoadTexture(hod.Textures[diffuseParam.TextureIndex]);
            if (tex != null)
                godotMat.AlbedoTexture = tex;
        }

        // Load normal map
        var normalParam = mat.ShaderParameters.Normal;
        if (normalParam.HasTexture && normalParam.TextureIndex < hod.Textures.Count)
        {
            var tex = LoadTexture(hod.Textures[normalParam.TextureIndex]);
            if (tex != null)
            {
                godotMat.NormalEnabled = true;
                godotMat.NormalTexture = tex;
            }
        }

        // Emission from glow texture
        var glowParam = mat.ShaderParameters.Glow;
        if (glowParam.HasTexture && glowParam.TextureIndex < hod.Textures.Count)
        {
            var tex = LoadTexture(hod.Textures[glowParam.TextureIndex]);
            if (tex != null)
            {
                godotMat.EmissionEnabled = true;
                godotMat.EmissionTexture = tex;
            }
        }

        return godotMat;
    }

    private static ImageTexture? LoadTexture(HW2HOD.Texture hodTex)
    {
        if (hodTex.Width == 0 || hodTex.Height == 0)
            return null;

        var rgba = hodTex.GetRGBAData();
        if (rgba == null)
            return null;

        var image = Image.CreateFromData(hodTex.Width, hodTex.Height, false, Image.Format.Rgba8, rgba);
        return ImageTexture.CreateFromImage(image);
    }

    // -------------------------------------------------------------------------
    // Skeleton construction
    // -------------------------------------------------------------------------

    private static void BuildSkeleton(Skeleton3D skeleton, Joint joint, int parentIdx)
    {
        int boneIdx = skeleton.AddBone(joint.Name);

        if (parentIdx >= 0)
            skeleton.SetBoneParent(boneIdx, parentIdx);

        skeleton.SetBoneRest(boneIdx, BuildBoneTransform(joint));

        foreach (var child in joint.Children)
            BuildSkeleton(skeleton, child, boneIdx);
    }

    private static Transform3D BuildBoneTransform(Joint joint)
    {
        // Convert position: negate Z
        var pos = FlipZ(joint.Position);

        // Convert rotation: Euler angles, negate Z component for RH coordinate system
        var euler = new global::Godot.Vector3(joint.Rotation.X, joint.Rotation.Y, -joint.Rotation.Z);
        var basis = Basis.FromEuler(euler, EulerOrder.Xyz).Scaled(
            new global::Godot.Vector3(joint.Scale.X, joint.Scale.Y, joint.Scale.Z));

        return new Transform3D(basis, pos);
    }

    // -------------------------------------------------------------------------
    // Coordinate conversion helpers
    // -------------------------------------------------------------------------

    /// <summary>Converts a DirectX LH Vector3 to Godot RH by negating Z.</summary>
    private static global::Godot.Vector3 FlipZ(CfMath.Vector3 v)
        => new global::Godot.Vector3(v.X, v.Y, -v.Z);
}
