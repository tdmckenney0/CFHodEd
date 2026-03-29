# HW2HOD — Agent Guidance

This library reads and writes Homeworld 2 `.hod` model files. It is the core data layer for the entire editor.

See [docs/file-formats.md](../../docs/file-formats.md) for the complete format specification.

## Key Files

| File | Role |
|------|------|
| `HOD.cs` | Top-level container, `Read(stream)` / `Write(stream)` |
| `HVMD/Mesh.cs` | `Mesh`, `MeshLOD`, `HODVertex` |
| `HVMD/Material.cs` | `Material`, `ShaderParameters`, texture bindings |
| `HVMD/Texture.cs` | `Texture`, DXT decompression, `GetRGBAData()` |
| `DTRM/Joint.cs` | `Joint`, recursive tree, transform math |
| `DTRM/Marker.cs` | Hardpoints and attachment points |
| `DTRM/EngineEffects.cs` | `EngineGlow`, `EngineBurn`, `EngineShape`, `NavLight` |

## Adding a New Chunk Handler

All chunk parsing happens via the `IFFReader` handler registry in `HOD.cs`'s `Read()` method:

```csharp
reader.RegisterHandler("MYCHUNK", (reader, size) =>
{
    var version = reader.ReadInt32();
    var name = reader.ReadString();
    // ... read exactly 'size' bytes total
    // store on the hod/context being built
});
```

For nested Form chunks, register the Form's type and use a child `IFFReader` on the payload bytes.

**Do not** inline new chunk parsing directly in the main read loop. Always use the handler registry pattern.

## Known Bugs — Already Fixed, Do Not Revert

### DTRM byte-vs-int32 (joint count)

In `DTRM/Joint.cs` (JNTS chunk parsing), the joint count field is an **int32**. A previous version incorrectly read it as a `byte`, which silently truncated models with more than 255 joints. This is fixed. Do not change this read to `ReadByte()`.

## Vertex Bitmask

In BMSH chunks, vertex components are optional. A bitmask at the start of the chunk indicates which components are present. Always check bits before reading:

```
Bit  0 → Position (4 floats: X, Y, Z, W)
Bit  1 → Normal   (4 floats: X, Y, Z, W)
Bit  2 → Colour   (1 int32: ARGB)
Bit  3 → TexCoord0 (2 floats: U, V)
Bit  4 → TexCoord1 (2 floats) — only in version 1401+
Bit  5 → TexCoord2 (2 floats) — only in version 1401+
Bit 13 → Tangent  (3 floats)
Bit 14 → Binormal (3 floats)
```

HODVertex stores: `Position` (Vector3), `Normal` (Vector3), `Tangent` (Vector3), `TexCoords` (Vector2). The W component of Position and Normal, plus Colour, Binormal, and UV1/2 are read but not stored in HODVertex.

## BMSH Versioning

Two BMSH sub-versions exist:

| Version | UV sets | Notes |
|---------|---------|-------|
| 1400 | UV0 only | Most common |
| 1401 | UV0, UV1, UV2 | Rare; some special materials |

Read the version int32 at the start of each BMSH chunk before reading vertices.

## Texture Decompression

`Texture.GetRGBAData(mipLevel)` decompresses DXT1/3/5 blocks or returns raw 8888 data. Returns `byte[]` as RGBA pixels, row-major, top-left origin.

DXT decompression handles:
- **DXT1**: 8 bytes per 4×4 block; 1-bit alpha for transparent DXT1
- **DXT3**: 16 bytes; explicit 4-bit alpha per texel
- **DXT5**: 16 bytes; interpolated alpha from two endpoints

## Material Types

- `Simple` — single material with one set of shader parameters
- `MultiMesh` — material with per-LOD parameter overrides (uncommon)

Shader name maps to GLSL shader: `"ship"`, `"matte"`, `"background"`, `"thruster"`.

## Reference Implementation

The original VB.NET source at [refs/cfhoded/](../../refs/cfhoded/) is the ground truth for format edge cases. When the C# behavior is uncertain, compare against the VB.NET code. It still compiles in modern Visual Studio.
