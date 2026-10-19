# Architecture

Cork has two assemblies and one test project.

```
tests/Cork.Core.Tests ──> src/Cork.Core <── src/Cork ──> AutoCAD.NET (compile only)
```

## Cork.Core

Plain .NET 10. It holds every decision the commands make and knows nothing about AutoCAD, so it is
tested on any machine.

- `FloorAlignment.ZOffset` - the Z move that puts a picked point on the UCS XY plane.
- `MaterialLayerPlanner.Plan` - given each solid's material and lock state, the drawing's existing
  layers, and a layer-name validator, returns a `MaterialLayerPlan`: the layers to create (each with
  the index of the solid that supplies its colour and material) and one `SolidChange` per solid.
- `MaterialLayerPlan.Summary` - the command-line report.

Names are compared case-insensitively, as AutoCAD compares symbol names.

## Cork (the add-in)

`CorkCommands` holds both commands. Each one:

1. Prompts or selects through the `Editor`.
2. Reads what Core needs into plain records (`SolidMaterial`, `ExistingLayer`).
3. Calls Core.
4. Applies the plan in one transaction and commits once at the end.

An AutoCAD error inside `MaterialsToLayers` leaves the transaction uncommitted, so the drawing is
unchanged, and the user sees the error. Name validation is delegated to AutoCAD's own
`SymbolUtilityServices.ValidateSymbolName` and passed to Core as a function, so Core follows
AutoCAD's rules without copying them.

The AutoCAD packages are referenced with `ExcludeAssets="runtime"`: they are compiled against but not
copied to the output, because AutoCAD loads its own copies.

## What the tests cannot cover

The host code needs a running AutoCAD. It is kept to reading, calling Core and writing, so a defect
there shows up on the first run of a command rather than hiding in logic.

## Build settings

- `Directory.Build.props` - nullable, implicit usings, the `latest-recommended` analyzer set,
  code style enforced on build, warnings as errors, version and copyright.
- `Directory.Packages.props` - every package version (central package management).
- `global.json` - .NET 10 SDK and the Microsoft.Testing.Platform test runner.
- `.editorconfig` - formatting and style, checked by `dotnet format --verify-no-changes` in CI.

## Browser demo

`src/Cork.Demo` is a thin .NET WebAssembly adapter and static browser UI. `Planning` validates a
bounded JSON sample and calls the existing `MaterialLayerPlanner.Plan` and `FloorAlignment.ZOffset`.
It serializes results through a generated JSON context; JavaScript only handles input and rendering.
The test project compiles the same adapter source to exercise boundary validation without a browser.
`Browser` exports those calls to JavaScript. The browser provides explicit validity flags in place of
AutoCAD's symbol-name validation; matching names share validity, with any invalid flag taking precedence.

No AutoCAD reference ships in the browser. The add-in and its transactions are unchanged.
The demo's CI role can deploy only `opensource/cork/` in the existing website bucket.
