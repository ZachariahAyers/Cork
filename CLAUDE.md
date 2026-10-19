# Cork

AutoCAD 2027 add-in with two commands: `Floor` and `MaterialsToLayers`. See `README.md` for what
they do and `docs/ARCHITECTURE.md` for how the code is split.

## Commands

```sh
dotnet restore Cork.slnx
dotnet format Cork.slnx --verify-no-changes --no-restore   # style gate; run `dotnet format Cork.slnx` to fix
dotnet build Cork.slnx --configuration Release --no-restore
dotnet test --solution Cork.slnx --configuration Release --no-build
```

`dotnet test` runs on Microsoft.Testing.Platform (set in `global.json`), so pass the solution with
`--solution`. Everything builds on Linux and macOS; only running the add-in needs Windows and AutoCAD.

## Project map

- `src/Cork.Core` - all decisions, plain .NET 10, no AutoCAD reference. Unit tested.
- `src/Cork` - the AutoCAD host: prompts, selection, transactions. `CorkCommands.cs` only.
- `tests/Cork.Core.Tests` - xUnit v3 tests for `Cork.Core`.

## Conventions

- Logic goes in `Cork.Core` with a test. The host only reads the drawing, calls Core, and writes back.
- Never reference AutoCAD types from `Cork.Core` or the tests.
- One transaction per command; commit only after every change is made.
- AutoCAD packages stay `ExcludeAssets="runtime"`: AutoCAD ships those DLLs.
- Package versions live in `Directory.Packages.props`; shared build settings in `Directory.Build.props`.
- Warnings are errors. Fix the cause; do not add `NoWarn` or suppressions.
- Test names describe behaviour in PascalCase, no underscores (CA1707 is on).
- Do not claim an AutoCAD API exists from memory: check the XML docs in the AutoCAD.NET packages
  (`~/.nuget/packages/autocad.net.model/<version>/lib/net10.0/AcDbMgd.xml`).

## Definition of done

Format check, build and tests all pass with the commands above, `CHANGELOG.md` has an entry for any
behaviour change, and a change to a command has been tried in AutoCAD or is reported as untried.
