# Changelog

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [2.0.0] - 2026-10-19

Built for AutoCAD 2027, with two commands: `Floor` and `MaterialsToLayers`.

### Changed

- Targets AutoCAD 2027 and .NET 10. Builds from Autodesk's AutoCAD.NET packages instead of DLLs
  from a local AutoCAD install.
- Four Visual Studio 2015 projects replaced by one add-in (`Cork`) and one logic library
  (`Cork.Core`), with unit tests.
- `MaterialsToLayers` assigns the material to each layer it creates, so solids switched to ByLayer
  keep rendering with their material.
- `MaterialsToLayers` reports what it moved and what it skipped, and why.

### Fixed

- `MaterialsToLayers` no longer aborts the whole run on a solid on a locked layer; it skips that
  solid and continues.
- `MaterialsToLayers` no longer tries to create a layer named ByLayer or ByBlock, or a layer from a
  material name that is not a valid layer name.
- `Floor` makes no change and says so when the picked point is already on the XY plane.

### Removed

- The empty `Cork` and `Cork_AnnoUtils` projects, wizard boilerplate, and an unused hard-coded
  material list.

[2.0.0]: https://github.com/ZachariahAyers/Cork/tree/v2.0.0
