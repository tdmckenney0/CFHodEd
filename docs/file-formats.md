# File Formats

## Overview

Homeworld 2 uses a hierarchy of binary formats:

```
IFF  ← container format (chunked binary, like RIFF)
└── HOD  ← model format built on IFF (meshes, materials, skeleton, effects)
└── MAD  ← animation format built on IFF (stubbed, not yet implemented)
```

The original VB.NET implementations are in [refs/cfhoded/](../refs/cfhoded/) and remain the authoritative reference for format edge cases.

---

## IFF — Interchange File Format

**Library:** `src/HW2IFF/`

IFF is a generic chunked binary container format (similar to RIFF). All Homeworld 2 asset files use it as the outer wrapper.

### Structure

Every IFF file is a sequence of chunks. Each chunk has:

```
[4 bytes] Type ID     — FourCC string (e.g., "VERS", "HVMD", "JNTS")
[4 bytes] Data size   — size of the payload in bytes (big-endian in some versions)
[N bytes] Payload     — chunk-specific data, may contain nested chunks
```

### Chunk Types

| Type | Meaning |
|------|---------|
| `Normal` | Leaf chunk — contains raw data, no sub-chunks |
| `Form` | Container chunk — payload is more chunks |
| `Default` | Fallback for unrecognized types |

`Form` chunks are the nesting mechanism. A `HVMD` Form contains `STAT`, `MULT`, `BMSH`, and `LMIP` chunks.

### Reading (IFFReader)

```csharp
// Register a handler for chunk type "BMSH"
reader.RegisterHandler("BMSH", (reader, size) => {
    // read size bytes from reader
});
reader.Read(stream);
```

The handler registry lives in `IFFReader`. Handlers are registered before `Read()` is called. Unregistered chunk types are skipped.

### Writing (IFFWriter)

```csharp
writer.BeginChunk("HVMD");   // opens a Form chunk
    writer.BeginChunk("STAT"); // opens a Normal chunk
        writer.WriteInt32(version);
        // ... write fields
    writer.EndChunk();
writer.EndChunk();
```

`EndChunk()` backtracks to write the size field once the payload is known.

---

## HOD — Homeworld 2 Model Format

**Library:** `src/HW2HOD/`

HOD files describe a complete 3D model: geometry, materials, textures, skeleton, markers, and engine effects.

### Top-Level Chunk Structure

```
VERS   — version number (0x200 for standard ships, 1000 for backgrounds)
NAME   — format name string
HVMD   (Form) — mesh and material data
    STAT   — material definitions (one per material)
    MULT   — multi-mesh descriptor
    BMSH   — binary mesh geometry (one per LOD)
    LMIP   — texture/mipmap data (one per texture)
DTRM   (Form) — hierarchical joint structure
    JNTS   — joint definitions
    MARK   — markers (hardpoints, attachment points)
    ENGM   — engine glow effects
    ENGS   — engine shape/burn effects
    NAVI   — navigation lights
INFO   (Form) — metadata
    team color, stripe color, thruster power, badge name
```

### Mesh Geometry (BMSH)

**Versions:** 1400 (base), 1401 (adds UV2/UV3)

Vertex components are present or absent based on a **bitmask** stored at the start of the chunk:

| Bit | Component | Size |
|-----|-----------|------|
| 0 | Position | 4 floats (XYZ + W padding) |
| 1 | Normal | 4 floats (XYZ + W padding) |
| 2 | Colour | 1 int (ARGB) |
| 3 | TexCoord0 | 2 floats (UV) |
| 4 | TexCoord1 (v1401) | 2 floats |
| 5 | TexCoord2 (v1401) | 2 floats |
| 13 | Tangent | 3 floats |
| 14 | Binormal | 3 floats |

Always read the bitmask and check bits before reading each component. The HODVertex struct stores: `Position`, `Normal`, `Tangent`, `TexCoords` (Vector2).

Indices are `ushort` (16-bit), stored as triangle lists.

### Materials (STAT)

Each material has:
- **Name** — material identifier
- **ShaderName** — which GLSL shader to use: `"ship"`, `"matte"`, `"background"`, `"thruster"`
- **ShaderParameters** — up to 8 named texture bindings

Texture parameter types: `4` = Color value, `5` = Texture reference.

Named texture slots used by the ship shader:

| Slot name | Purpose |
|-----------|---------|
| `Diffuse` | Base color |
| `Glow` | Self-illumination + specular weight |
| `Specular` | Shininess map |
| `Reflection` | Environment map |
| `Normal` | Normal map |
| `Team` | Team/stripe color mask |
| `Pain` | Damage decal |
| `Stripe` | Stripe decal |

### Textures (LMIP)

Each texture record:
- **Format** (FourCC): `"8888"` = raw RGBA, `"DXT1"`, `"DXT3"`, `"DXT5"`
- **Width / Height** (top mip level)
- **Mip levels** — each subsequent level is half the dimensions
- **Pixel data** — compressed blocks or raw RGBA bytes

DXT decompression is implemented in `src/HW2HOD/HVMD/Texture.cs`. Call `GetRGBAData(mipLevel)` to get decompressed RGBA pixel bytes.

### Skeleton / Joints (DTRM/JNTS)

Each joint:
- **Name** — string identifier
- **Position** — Vector3 translation
- **Rotation** — Euler angles in radians (Vector3), applied as Rx * Ry * Rz
- **Scale** — Vector3
- **Visible** — bool display flag
- **Children** — List<Joint> (tree structure)

> **Bug note:** The joint count field is an `int32`. An earlier version of the parser read it as a `byte`, which silently truncated models with more than 255 joints. This is fixed — do not change the JNTS parsing code to use byte reads.

### Markers (DTRM/MARK)

Markers are named attachment points (hardpoints for weapons, engines, shield generators):
- **Name** — identifier (e.g., `"HardPoint_Gun1"`)
- **Position** — Vector3
- **Rotation** — Euler angles
- **ParentJoint** — joint name

### Engine Effects (DTRM/ENGM, ENGS, NAVI)

Three types:
- **EngineGlow** (ENGM) — subtle ambient engine illumination
- **EngineBurn** / EngineShape (ENGS) — bright directional exhaust
- **NavLight** (NAVI) — navigation beacon

All store position, rotation, and animation parameters relative to a parent joint.

---

## MAD — Homeworld 2 Animation Format

**Library:** `src/HW2MAD/`
**Status:** Stubbed. `MADFormat.cs` contains TODO comments. Not yet implemented.

When implemented, the format will provide:
- Named animation sequences
- Per-joint keyframe curves (position, rotation, scale)
- Frame range and playback rate

Consult [refs/cfhoded/](../refs/cfhoded/) for the VB.NET reference implementation.

---

## Wavefront OBJ

**Library:** `src/GMWavObjT/`
**Status:** Parser exists, not yet wired to the import/export UI.

Standard `.obj` + `.mtl` format. Supports: vertices, normals, UV coordinates, faces, material groups. The `WavefrontObject` class can read and produce a `GBasicMesh`.
