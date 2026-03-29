# SharpHodEditor — Agent Guidance

## What This Is

SharpHodEditor (internally named CFHodEd) is a cross-platform 3D model editor for **Homeworld 2 HOD files** — the proprietary binary format used for ships, structures, and effects in the game. It is a modern C# .NET 8 / Avalonia port of the original VB.NET CFHodEd editor.

The original VB.NET source is preserved at [refs/cfhoded/](refs/cfhoded/) and still compiles in Visual Studio. Consult it when porting behavior or resolving format ambiguities.

## Build & Run

```bash
dotnet build                           # Build all projects
dotnet run --project src/CFHodEd.UI   # Launch the editor
dotnet run --project src/TestHOD      # Console HOD loader (useful for format debugging)
```

Requires: .NET 8 SDK.

## Project State

See [TASKS.md](TASKS.md) for the canonical task list.

**Complete:**
- All 9 libraries ported from VB.NET to C#
- Avalonia UI with full MVVM bindings
- HOD file reading and writing
- Joint/skeleton hierarchy (DTRM) parsing
- Mesh rendering: wireframe, solid (flat lighting), textured (software rasterizer)
- DXT1/3/5 and raw RGBA texture decompression

**In progress / incomplete:**
- OBJ import/export (GMWavObjT library exists; not wired up)
- Settings dialog (stub only)
- Animation system (HW2MAD library is stubbed — see `src/HW2MAD/MADFormat.cs`)

## Architecture at a Glance

```
CFHodEd.UI          ← Avalonia desktop app, 3D viewport, MVVM
    ↓
HW2HOD / HW2IFF / HW2MAD  ← File format libraries
    ↓
CFHodEd.Rendering   ← IRenderDevice abstraction + OpenGL backend
    ↓
CFHodEd.Math        ← DirectX-compatible math (Vector, Matrix, Quaternion)
```

Supporting libraries: `GenericMesh`, `GenericMath`, `GMWavObjT`.

See [docs/architecture.md](docs/architecture.md) for the full map.

## Critical Gotchas

### The viewport uses a SOFTWARE rasterizer, not hardware OpenGL
`OpenGLViewport.cs` renders via `WriteableBitmap` (CPU scanline rasterizer) for cross-platform Avalonia compatibility. `OpenGLRenderDevice` in `CFHodEd.Rendering` exists and is complete but is **not yet wired to the viewport**. Do not assume GPU rendering is active.

### Matrices are DirectX row-major
`CFHodEd.Math.Matrix` uses **row-major, left-handed** layout (DirectX convention), not the column-major layout of OpenGL or `System.Numerics`. Transformation order: Scale → Rotate → Translate. `LookAtLH` and `PerspectiveFovLH` are used throughout.

### DTRM byte-vs-int32 bug is already fixed
An earlier bug in joint-count parsing read a `byte` where an `int32` was needed, causing truncation on models with >255 joints. This has been fixed — do not revert the DTRM parsing code.

### BMSH chunk versioning
Vertex format differs between BMSH version 1400 and 1401. Version 1401 adds UV2 and UV3 texture coordinate sets. Always check the version before reading UV data.

## Code Conventions

- **MVVM throughout the UI layer.** All state lives in ViewModels. Code-behind in `.axaml.cs` handles only event wiring and file dialogs. Never put business logic in code-behind.
- **IRenderDevice for all GPU operations.** Do not call Silk.NET/OpenGL directly from outside `CFHodEd.Rendering`.
- **IFF chunk handlers.** To parse a new HOD chunk, register a handler in `HOD.cs`'s `Read()` method via the IFFReader handler registry. Don't inline parsing logic in the main read loop.
- **Nullable reference types are enabled.** Honor `?` annotations; don't suppress warnings with `!` without a comment explaining why.

## Docs Index

| File | Contents |
|------|----------|
| [docs/architecture.md](docs/architecture.md) | Full project map, all 10 libraries, dependency graph |
| [docs/rendering-pipeline.md](docs/rendering-pipeline.md) | Rasterizer, render modes, camera, shader inventory |
| [docs/file-formats.md](docs/file-formats.md) | HOD, IFF, MAD binary format documentation |
| [docs/development.md](docs/development.md) | Onboarding, how-to guides, extension patterns |
