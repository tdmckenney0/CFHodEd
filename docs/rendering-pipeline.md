# Rendering Pipeline

## Renderer: Godot 4 GPU

The viewport uses **Godot 4's built-in Forward Plus renderer** running inside a `SubViewport` node. Geometry is uploaded to the GPU as `ArrayMesh` surfaces; lighting, materials, and draw calls are all managed by Godot. There is no software rasterizer.

## Coordinate System Conversion

HOD/DirectX uses **left-handed** coordinates (camera looks toward +Z, Y is up).
Godot uses **right-handed** coordinates (camera looks toward −Z, Y is up).

`HODLoader.cs` performs the conversion on all geometry before uploading to Godot:

| HOD data | Conversion | Godot data |
|----------|-----------|------------|
| Vertex position `(x, y, z)` | Negate Z | `(x, y, −z)` |
| Vertex normal `(nx, ny, nz)` | Negate Z | `(nx, ny, −nz)` |
| Triangle indices `[i0, i1, i2]` | Swap i1 ↔ i2 | `[i0, i2, i1]` |
| Joint Euler rotation `(rx, ry, rz)` | Negate Z component | `Basis.FromEuler(rx, ry, −rz)` |
| Joint position | Same as vertex | Negate Z |

The winding swap is necessary because negating Z mirrors the geometry; flipping two indices restores the outward-facing normal direction.

## Mesh Construction (HODLoader)

For each `MeshLOD` (LOD 0 is used), `BuildMesh()` fills a Godot `ArrayMesh`:

```
Mesh.ARRAY_VERTEX  ← HODVertex.Position  (Z negated)
Mesh.ARRAY_NORMAL  ← HODVertex.Normal    (Z negated)
Mesh.ARRAY_TEX_UV  ← HODVertex.TexCoords (unchanged)
Mesh.ARRAY_INDEX   ← MeshLOD.Indices     (winding flipped)
```

Each surface is added with `ArrayMesh.AddSurfaceFromArrays()` and a `StandardMaterial3D` is set on the `MeshInstance3D`.

## Materials

HOD materials map to Godot `StandardMaterial3D`:

| HOD parameter | Godot property |
|---------------|---------------|
| `ShaderParameters.Diffuse` texture | `AlbedoTexture` |
| `ShaderParameters.Normal` texture | `NormalTexture` (NormalEnabled = true) |
| `ShaderParameters.Glow` texture | `EmissionTexture` (EmissionEnabled = true) |

Textures are decompressed by `Texture.GetRGBAData()` (DXT1/3/5 or raw RGBA) then uploaded as `Image.Format.Rgba8` via `ImageTexture.CreateFromImage()`.

## Render Modes

Switched from the toolbar in `Main.cs`:

| Mode | Implementation |
|------|---------------|
| **Wireframe** | `SubViewport.DebugDraw = Viewport.DebugDrawEnum.Wireframe` |
| **Solid** | DebugDraw disabled; `AlbedoColor = grey`, texture hidden |
| **Textured** | DebugDraw disabled; `AlbedoColor = white`, full material active |

## Skeleton

Joint hierarchy becomes a Godot `Skeleton3D` node (sibling of the `MeshInstance3D` nodes under `HodModelRoot`). Each joint maps to one bone:

- `Skeleton3D.AddBone(joint.Name)`
- `SetBoneParent(boneIdx, parentIdx)` — DFS traversal ensures parent is always added first
- `SetBoneRest(boneIdx, Transform3D)` — rest pose from converted position + rotation + scale

`MeshInstance3D.Skeleton` is set to the `Skeleton3D`'s node path, associating the mesh with the rig.

## Camera

`CameraController` (extends `Camera3D`) uses spherical orbit coordinates:

| Field | Meaning |
|-------|---------|
| `_distance` | Orbit radius from target |
| `_yaw` | Horizontal rotation angle (radians) |
| `_pitch` | Vertical rotation angle (radians, clamped ±1.5) |
| `_target` | World-space point being orbited |

Each frame after input: `Position = target + spherical_offset(distance, yaw, pitch)`, then `LookAt(target)`.

| Input | Action |
|-------|--------|
| Left drag | Orbit (yaw / pitch) |
| Middle / Right drag | Pan (translate target) |
| Scroll wheel | Zoom (scale distance) |
| R | Reset to default |

`FocusOnBounds(Aabb)` sets target to the bounding box center and scales distance to fit.

## Team Color Shader (Future Work)

Homeworld 2 uses a multi-texture masking scheme for per-player colors:

- **Diffuse** texture: base color
- **Team mask** (R channel): blends base toward the player's team color
- **Stripe mask** (G channel): blends base toward the stripe color
- **Glow** (B channel): drives specular weight and self-illumination

This is not yet implemented in the Godot material. `StandardMaterial3D` currently uses the diffuse texture directly. A custom `ShaderMaterial` using Godot's shader language will be needed to replicate the full HW2 look.

## Adding a New Render Mode

1. Add a toolbar button in `Main.tscn` with a unique name
2. In `Main.cs._Ready()`, wire its `Pressed` signal to a handler
3. In the handler, set `_viewport.DebugDraw` and call `SetAllMaterials()` as needed
