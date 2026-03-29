# Rendering Pipeline

## Current Renderer: Software Rasterizer

The viewport currently uses a **CPU-based software rasterizer** written in `src/CFHodEd.UI/OpenGLViewport.cs`. It renders into an Avalonia `WriteableBitmap` which is then displayed as an image control inside the Avalonia window.

This was chosen for **cross-platform compatibility** — Avalonia's OpenGL interop has platform-specific complexity, and a software renderer works identically on Windows, Linux, and macOS.

`OpenGLRenderDevice` (in `src/CFHodEd.Rendering/`) is a complete OpenGL 3.3 backend but is **not yet wired to the viewport**. Connecting it is the intended next step for hardware-accelerated rendering.

## Render Pipeline Steps

Each frame, `OpenGLViewport.RenderToBuffer()` executes in order:

```
1. Clear backbuffer        → fill pixels with clear color (30, 30, 35)
2. Draw grid               → 20×20 world-space grid lines (60, 60, 70)
3. Draw axes               → origin markers: Red=X, Green=Y, Blue=Z
4. Collect all triangles   → iterate HOD meshes, transform to view space,
                             compute average depth per triangle
5. Sort back-to-front      → painter's algorithm (depth sort)
6. Rasterize triangles     → per-triangle: cull → shade → draw pixels
```

## Render Modes

Three modes are toggled from the toolbar or View menu:

### Wireframe
Draws triangle edges only using Bresenham's line algorithm. No face fill. Good for inspecting topology.

### Solid
Flat shading: each triangle gets a single color computed from the dot product of its face normal and the camera direction vector (diffuse + ambient). Backface culling is applied. Good for inspecting geometry without texture noise.

### Textured
Fills triangles with samples from the diffuse texture using **perspective-correct barycentric UV interpolation**. Falls back to solid shading if no texture is available for a material. Backface culling is applied.

## Camera

The camera is a **spherical orbit** (trackball-style) centered on a target point.

| Control | Action |
|---------|--------|
| Left mouse button + drag | Rotate (yaw + pitch) |
| Middle or right mouse button + drag | Pan (translate target) |
| Mouse scroll wheel | Zoom (adjust orbit radius) |
| R key | Reset camera to default (pos: 0,2,5; target: origin) |

Camera parameters used in matrix construction:
- **Yaw / Pitch**: Euler angles
- **Distance**: orbit radius from target
- **View matrix**: `Matrix.LookAtLH(eye, target, up)`
- **Projection matrix**: `Matrix.PerspectiveFovLH(45°, aspect, 0.1f, 1000f)`

`FocusOnModel()` computes the bounding box of all mesh vertices and positions the camera to fit the whole model in view.

## Rasterization Details

- **Depth buffer**: None. Triangles are sorted by average view-space Z (painter's algorithm). This means overlapping triangles can have artifacts, but it is sufficient for the current use case.
- **Backface culling**: Cross product of edge vectors compared to camera direction. Triangles facing away are skipped in Solid and Textured modes.
- **UV interpolation**: Barycentric coordinates with perspective correction (`w` divide).
- **Lighting model**: Single directional light aligned with the camera direction. `diffuse = max(dot(normal, lightDir), 0)`. Ambient term prevents fully black back-lit surfaces.

## Shader System (For Future GPU Renderer)

The GLSL shaders in `src/CFHodEd.Rendering/Shaders/` are for the `OpenGLRenderDevice` GPU path that is not yet active. They are compiled as **embedded resources** in the assembly.

| File | Type | Purpose |
|------|------|---------|
| `standard.vert` | Vertex | Transforms position, normal, UV; outputs WorldPos, ViewDir |
| `ship.frag` | Fragment | Full Homeworld 2 ship shader: team color, stripe, glow, specular |
| `matte.frag` | Fragment | Simple diffuse-only, ambient lighting |
| `background.frag` | Fragment | Unlit skybox / background surface |
| `thruster.frag` | Fragment | Additive engine exhaust glow |

### ship.frag: Team Color System

Homeworld 2 uses a multi-texture color masking scheme for per-player coloring:

- **Diffuse texture**: Base color map
- **Team color mask** (R channel): Blends base color with the player's team color
- **Stripe color mask** (G channel): Blends with the player's stripe color
- **Glow texture** (B channel): Controls specular highlight weight and self-illumination

The formula darkens/lightens the base texture toward the selected team or stripe color, allowing a single model to represent any player's color scheme.

### Uniforms (standard.vert)

```glsl
uniform mat4 uWorld;
uniform mat4 uView;
uniform mat4 uProjection;
```

Normal matrix is derived from the inverse-transpose of the world matrix inside the shader.

### Uniforms (ship.frag)

```glsl
uniform sampler2D uDiffuse;
uniform sampler2D uGlow;
uniform sampler2D uTeamColor;
uniform vec4 uTeamColorValue;
uniform vec4 uStripeColorValue;
uniform vec3 uLightDir[8];
uniform vec4 uLightColor[8];
uniform int uLightCount;
```

## Adding a New Render Mode

1. Add a value to the `RenderMode` enum in `OpenGLViewport.cs`
2. Add a case to the triangle rasterization switch in `DrawMeshes()`
3. Add a toolbar button or menu item in `MainWindow.axaml`
4. Bind the button to a command in `MainWindowViewModel` that sets `OpenGLViewport.CurrentRenderMode`
