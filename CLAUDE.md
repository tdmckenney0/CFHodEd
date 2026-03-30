# SharpHodEditor — Agent Instructions

## Project Overview

SharpHodEditor is a VB.NET desktop application for editing Homeworld 2 HOD (3D mesh) files. It is currently Windows-only and is being ported to run natively on **Windows, macOS, and Linux**.

- **Language:** Visual Basic .NET
- **Runtime target:** .NET 8 (migrating from .NET Framework 3.5)
- **UI framework:** Avalonia UI 11 (migrating from Windows Forms)
- **Graphics:** OpenTK 4 / OpenGL 3.3 (migrating from DirectX 9)
- **Config storage:** JSON via `System.Text.Json` (migrating from Windows Registry)

See [`docs/codebase-analysis.md`](docs/codebase-analysis.md) for full architecture detail.
See [`docs/cross-platform-plan.md`](docs/cross-platform-plan.md) for the full migration plan and phase breakdown.

---

## Build & Run

```bash
# Build the solution
dotnet build src/CFHodEd/CFHodEd.sln

# Build a single project
dotnet build src/CFHodEd/CFHodEd.vbproj

# Run the application
dotnet run --project src/CFHodEd/CFHodEd.vbproj

# Run all tests
dotnet test

# Run tests for a specific project
dotnet test tests/HW2HOD.Tests/HW2HOD.Tests.vbproj

# Run visual snapshot tests only
dotnet test --filter Category=Visual

# Run E2E tests only
dotnet test --filter Category=E2E
```

---

## Project Structure

```
src/
├── CFHodEd/          # Main executable — Avalonia UI
├── D3DHelper/        # OpenGL rendering (OpenTK) + JSON config
├── GenericMesh/      # Mesh rendering abstractions
├── GenericMath/      # Vector/matrix math
├── GMWavObjT/        # Wavefront OBJ import/export
├── HW2HOD/           # HOD file format parser + GLSL shaders
├── HW2IFF/           # IFF chunk container format
└── HW2MAD/           # Animation file format

tests/
├── HW2IFF.Tests/     # Unit tests for IFF parsing
├── HW2MAD.Tests/     # Unit tests for animation parsing
├── HW2HOD.Tests/     # Unit tests for HOD parsing
├── GenericMath.Tests/# Unit tests for math utilities
├── GMWavObjT.Tests/  # Unit tests for OBJ import/export
├── D3DHelper.Tests/  # Unit tests for GL config/JSON config
├── CFHodEd.Tests/    # Avalonia headless UI tests
├── E2E/              # End-to-end tests (load → render → save round-trips)
└── Visual/
    ├── reference/    # Reference screenshots (committed to git)
    └── output/       # Test-generated screenshots (gitignored)
```

---

## MCP Server Usage

MCP servers are available and **must** be used proactively. Do not rely on training data for library APIs.

### recallium — Project Memory

**Load context at the start of every session:**

```
recallium(project_name="sharp-hod-editor")
```

**Store memories after every substantive task** — completing a feature, making an architectural decision, fixing a bug, discovering something non-obvious. This is mandatory, not optional.

Key triggers:
- After completing a phase of the migration → `memory_type: "progress"`
- After choosing a specific API approach → `memory_type: "decision"`
- After translating a shader or fixing a rendering bug → `memory_type: "debug"` with `related_files`
- After any code change → `memory_type: "code-snippet"` or `"feature"` with `related_files`

### context7 — Library Documentation

**Use before implementing anything involving a library:**

```
resolve-library-id("avalonia ui")   → then query-docs(...)
resolve-library-id("opentk")        → then query-docs(...)
resolve-library-id("system.text.json") → then query-docs(...)
```

Always use context7 for: OpenTK API (buffers, shaders, textures, context), Avalonia (controls, AXAML, headless testing, NativeControlHost), .NET 8 APIs.
Never rely on training data alone for these — the APIs have changed since training cutoffs.

### augment_code_search — Semantic Code Search

Use `mcp__auggie__augment_code_search` for semantic searches across the codebase when you need to find where a concept is implemented (e.g., "where is the vertex buffer uploaded", "how is the camera transform applied"). Prefer this over Grep for conceptual searches; use Grep for exact symbol lookups.

---

## Acceptance Criteria for All New Work

Every PR or completed task **must** include:

### 1. Unit Tests
- One test project per source project (e.g., `tests/HW2HOD.Tests/`)
- Cover all public methods in new or modified code
- Use `xUnit` as the test framework
- Tests must be platform-agnostic — no Windows-only assertions
- Minimum: happy path + one edge case + one error case per method
- File format tests must use checked-in test fixtures from `tests/fixtures/`

### 2. E2E Tests (for user-facing changes)
- Located in `tests/E2E/`
- Must cover the full round-trip: open file → modify → save → reload → verify
- Run headlessly (no display required) using Avalonia headless mode
- Each E2E test must clean up any files it writes

### 3. Visual Snapshot Tests (for rendering changes)
See the [Visual Testing](#visual-testing--agent-feedback-loop) section below.

### 4. No Regressions
- All existing tests must continue to pass
- `dotnet build` must succeed with zero errors and zero new warnings

---

## Visual Testing — Agent Feedback Loop

Visual tests let agents verify that 3D ship rendering and Avalonia UI look correct **without requiring a display**. Output images are readable by Claude directly (multimodal).

### How It Works

**Avalonia UI tests** use `Avalonia.Headless.XUnit` to render windows to bitmaps in-process:

```vb
' Example: render a window headlessly and capture a screenshot
<AvaloniaFact>
Public Async Function MainWindow_LoadHod_ShowsMesh() As Task
    Dim window = New HODEditorWindow()
    window.LoadHod("tests/fixtures/sample_ship.hod")
    Await Task.Delay(100) ' allow render frame
    Dim bitmap = window.CaptureRenderedFrame()
    bitmap.Save("tests/Visual/output/main_window_mesh.png")
    ' Claude reads this PNG to visually verify
End Function
```

**3D ship rendering tests** use an OpenGL offscreen framebuffer (FBO) to render without a window:

```vb
' Example: render a ship to offscreen buffer and save as PNG
<Fact, Category("Visual")>
Public Sub ShipRenderer_MattShader_MatchesReference()
    Using ctx = New OffscreenGLContext(800, 600)
        Dim renderer = New ShipRenderer(ctx)
        renderer.LoadHod("tests/fixtures/sample_ship.hod")
        renderer.SetShader("matte")
        Dim png = renderer.RenderToPng()
        png.Save("tests/Visual/output/ship_matte_shader.png")
        ' Assert pixel similarity against reference (tolerance: 2%)
        AssertImageSimilarity(
            reference:="tests/Visual/reference/ship_matte_shader.png",
            actual:="tests/Visual/output/ship_matte_shader.png",
            tolerancePercent:=2.0
        )
    End Using
End Sub
```

### Visual Test Workflow for Agents

1. **Run visual tests:**
   ```bash
   dotnet test --filter Category=Visual
   ```

2. **Read output screenshots** using the `Read` tool — Claude can view PNG files directly:
   ```
   Read("tests/Visual/output/ship_matte_shader.png")
   ```

3. **Compare against reference** — read both reference and output, visually assess:
   ```
   Read("tests/Visual/reference/ship_matte_shader.png")
   Read("tests/Visual/output/ship_matte_shader.png")
   ```

4. **Iterate** — if the output doesn't match expectations, adjust the shader or renderer and re-run.

5. **Update references** when a visual change is intentional:
   ```bash
   dotnet run --project tools/UpdateVisualReferences
   # or manually copy output/ → reference/
   ```

### Reference Image Management

- Reference images live in `tests/Visual/reference/` and are **committed to git**
- Output images go to `tests/Visual/output/` which is **gitignored**
- When a rendering change is intentional, update the reference and commit it with the code change
- Reference images are 800×600 PNG, rendered at 1x DPI

### Test Fixtures

Checked-in test HOD files in `tests/fixtures/`:

| File | Purpose |
|------|---------|
| `sample_ship.hod` | Basic ship mesh, all standard shaders |
| `sample_wireframe.hod` | Wireframe mesh variant |
| `sample_animated.hod` | Ship with MAD animation |
| `sample_badge.hod` | Ship with team badge geometry |
| `minimal.hod` | Smallest valid HOD (parser edge case) |

---

## Code Conventions

- **VB.NET only** — do not introduce C# files
- **No platform-specific APIs** — all new code must compile targeting `net8.0` (not `net8.0-windows`)
- **Path handling** — always use `IO.Path.Combine()`, never `& "\"` or `& "/"`
- **Line endings** — use `Environment.NewLine`, never `vbCrLf` or `vbLf` directly
- **Config access** — use `JsonConfig` module (never `Microsoft.Win32.Registry`)
- **Shader loading** — use `ShaderLibrary.LoadGlsl()`, never `Direct3D.ShaderLoader`
- **Startup path** — use `AppContext.BaseDirectory`, never `Application.StartupPath`

---

## Testing Quick Reference

```bash
# All tests
dotnet test

# Unit tests only (fast, no display)
dotnet test --filter "Category!=Visual&Category!=E2E"

# Visual rendering tests (produces PNGs in tests/Visual/output/)
dotnet test --filter Category=Visual

# E2E tests (full file round-trips)
dotnet test --filter Category=E2E

# Single test project
dotnet test tests/HW2HOD.Tests/

# Verbose output
dotnet test --logger "console;verbosity=detailed"

# Update visual references (run after intentional rendering changes)
dotnet run --project tools/UpdateVisualReferences
```

---

## Migration Status

See [`docs/cross-platform-plan.md`](docs/cross-platform-plan.md) for the full phase plan.

| Phase | Description | Status |
|-------|-------------|--------|
| 0 | Documentation & audit | ✅ Done |
| 1 | Project files → .NET 8 SDK-style | ⬜ Pending |
| 2 | Registry → JSON config | ⬜ Pending |
| 3 | Path separator fixes | ⬜ Pending |
| 4 | HLSL → GLSL shader translation | ⬜ Pending |
| 5 | DirectX 9 → OpenTK graphics rewrite | ⬜ Pending |
| 6 | Windows Forms → Avalonia UI | ⬜ Pending |
| 7 | Final target framework switch (net8.0) | ⬜ Pending |
