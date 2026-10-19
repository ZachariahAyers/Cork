namespace Cork.Core;

/// <summary>What the MaterialsToLayers command reads from one 3D solid.</summary>
/// <param name="MaterialName">Name of the material the solid uses.</param>
/// <param name="IsOnLockedLayer">True when the solid's current layer is locked.</param>
public readonly record struct SolidMaterial(string MaterialName, bool IsOnLockedLayer);

/// <summary>A layer that already exists in the drawing.</summary>
/// <param name="Name">Layer name.</param>
/// <param name="MaterialName">Name of the material assigned to the layer.</param>
public readonly record struct ExistingLayer(string Name, string MaterialName);

/// <summary>What happens to one solid.</summary>
public enum SolidOutcome
{
    /// <summary>The solid moves to the layer named after its material.</summary>
    Moved,

    /// <summary>The solid is on a locked layer and is left alone.</summary>
    SkippedLockedLayer,

    /// <summary>The solid takes its material from its layer or block, so there is no material to name a layer after.</summary>
    SkippedInheritedMaterial,

    /// <summary>The material name is not a valid layer name.</summary>
    SkippedInvalidName,
}

/// <summary>The change planned for one solid.</summary>
/// <param name="Outcome">What happens to the solid.</param>
/// <param name="Layer">The target layer when <paramref name="Outcome"/> is <see cref="SolidOutcome.Moved"/>; otherwise null.</param>
/// <param name="UseLayerMaterial">
/// True when the target layer carries the solid's own material, so the solid can switch to ByLayer
/// without changing how it renders.
/// </param>
public readonly record struct SolidChange(SolidOutcome Outcome, string? Layer, bool UseLayerMaterial);

/// <summary>A layer to create.</summary>
/// <param name="Name">Layer name, equal to the material name.</param>
/// <param name="SourceSolid">Index of the first solid with this material; the layer takes its colour and material.</param>
public readonly record struct NewLayer(string Name, int SourceSolid);
