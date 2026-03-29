# SharpHodEditor — Agent Guidance

## What This Is

SharpHodEditor (internally named CFHodEd) is a cross-platform 3D model editor for **Homeworld 2 HOD files** — the proprietary binary format used for ships, structures, and effects in the game. It is a C# .NET 8 / Godot 4 editor built on top of the original VB.NET CFHodEd libraries.

The original VB.NET source is preserved at [refs/cfhoded/](refs/cfhoded/) and still compiles in Visual Studio. Consult it when porting behavior or resolving format ambiguities.

## Build & Run

```bash
# Open src/CFHodEd.Godot/ in the Godot 4 editor to run the full application.
# The Godot editor manages the build for CFHodEd.Godot.

dotnet build src/HW2HOD             # Build core format library
```

Requires: .NET 8 SDK, Godot 4.6 with C# support.

## Project State

See [TASKS.md](TASKS.md) for the canonical task list.

**Complete:**
- All core libraries ported from VB.NET to C#
- Godot 4 UI: toolbar, HSplitContainer layout, SubViewport 3D view
- HOD file reading and writing
- Joint/skeleton hierarchy (DTRM) parsing
- Mesh rendering via Godot GPU renderer (wireframe, solid, textured)
- DXT1/3/5 and raw RGBA texture decompression
- Hierarchy panel (Tree control) populated from HOD
- Properties panel: Joint transforms, Material textures, Team/Stripe colors
- Orbit/pan/zoom camera controller

**In progress / incomplete:**
- OBJ import/export (GMWavObjT library exists; not wired up)
- Animation system (HW2MAD library is stubbed — see `src/HW2MAD/MADFormat.cs`)

## Architecture at a Glance

```
CFHodEd.Godot       ← Godot 4 desktop app (scenes + C# scripts)
    ↓
HW2HOD / HW2IFF / HW2MAD  ← File format libraries
    ↓
CFHodEd.Math        ← DirectX-compatible math (Vector, Matrix, Quaternion)
```

Supporting libraries: `GenericMesh`, `GenericMath`, `GMWavObjT`.

See [docs/architecture.md](docs/architecture.md) for the full map.

## Critical Gotchas

### Coordinate system conversion (LH → RH)
HOD/DirectX uses **left-handed** coordinates (camera toward +Z).
Godot uses **right-handed** coordinates (camera toward -Z).
`HODLoader.cs` negates Z on all vertex positions and normals, and swaps index[1]/index[2] on every triangle to flip winding order.

### Matrices are DirectX row-major
`CFHodEd.Math.Matrix` uses **row-major, left-handed** layout (DirectX convention), not the column-major layout of OpenGL or `System.Numerics`. Transformation order: Scale → Rotate → Translate. `LookAtLH` and `PerspectiveFovLH` are used throughout.

### DTRM byte-vs-int32 bug is already fixed
An earlier bug in joint-count parsing read a `byte` where an `int32` was needed, causing truncation on models with >255 joints. This has been fixed — do not revert the DTRM parsing code.

### BMSH chunk versioning
Vertex format differs between BMSH version 1400 and 1401. Version 1401 adds UV2 and UV3 texture coordinate sets. Always check the version before reading UV data.

### `Material` and `Mesh` name conflicts
`Godot.Material`, `HW2HOD.Material`, `Godot.Mesh`, and `HW2HOD.Mesh` are all in scope in the Godot scripts. Use the aliases defined at the top of each script file:
- `using HodMaterial = HW2HOD.Material;`
- Use `global::Godot.Mesh` for Godot's Mesh type where needed.

## Code Conventions

- **Godot scripts are `partial` classes** extending their node type (`Control`, `VBoxContainer`, `Camera3D`, etc.).
- **`%NodeName` unique-name paths** are used in `_Ready()` for all cross-scene node lookups. Nodes accessed this way must have `unique_name_in_owner = true` in `Main.tscn`.
- **HODLoader is a static class** — no state. `BuildScene(hod)` returns a `Node3D` subtree; the caller adds it to `HodModelRoot`.
- **IFF chunk handlers.** To parse a new HOD chunk, register a handler in `HOD.cs`'s `Read()` method via the IFFReader handler registry. Don't inline parsing logic in the main read loop.
- **Nullable reference types are enabled.** Honor `?` annotations; don't suppress warnings with `!` without a comment explaining why.

## Docs Index

| File | Contents |
|------|----------|
| [docs/architecture.md](docs/architecture.md) | Full project map, all libraries, dependency graph |
| [docs/godot-migration.md](docs/godot-migration.md) | Godot migration plan and scene/script design |
| [docs/file-formats.md](docs/file-formats.md) | HOD, IFF, MAD binary format documentation |
| [docs/development.md](docs/development.md) | Onboarding, how-to guides, extension patterns |
