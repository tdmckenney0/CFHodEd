# HOD Test Fixtures

Place any Homeworld 2 `.hod` files here to test them with the editor and test suites.

**These files are gitignored — do not commit them.**

## How files are used

| Consumer | Behaviour |
|----------|-----------|
| `dotnet test src/CFHodEd.Tests` | Copies all `*.hod` files to `TestData/` in the test output directory. `HodReadTests` looks for `meg_starjumper.hod` specifically and skips if absent. `HodDiscoveryTests` runs basic parse checks on every file found. |
| `dotnet test src/CFHodEd.Tests.Godot` | Same copy; `HODLoaderTests` and `HierarchyPanelTests` look for `meg_starjumper.hod` and silently pass if it is absent. |
| Godot screenshot mode (`--screenshot`) | `meg_starjumper.hod` is the default model; any other file can be passed with `--hod`. |

## Suggested starting file

`meg_starjumper.hod` — the Hiigaran Carrier from Homeworld 2. This is the fixture the existing tests were written against, so it produces the most useful output. Extract it from your Homeworld 2 installation with the HW2 HOD extraction tools.
