using System.Text.Json;
using System.Text.Json.Serialization;
using Cork.Core;

namespace Cork.Demo;

internal static class Planning
{
    internal static double Floor(double pickedZ)
    {
        if (!double.IsFinite(pickedZ) || Math.Abs(pickedZ) > 1_000_000)
        {
            throw new ArgumentOutOfRangeException(nameof(pickedZ), "Enter a finite coordinate between -1000000 and 1000000.");
        }

        return FloorAlignment.ZOffset(pickedZ);
    }

    internal static string Run(string input)
    {
        try
        {
            if (input.Length > 16_384)
            {
                throw new ArgumentException("Keep the drawing sample under 16 KB.");
            }

            var request = JsonSerializer.Deserialize(input, DemoJson.Default.PlanRequest)
                ?? throw new ArgumentException("Enter a drawing sample.");
            if (request.Solids is not { Length: >= 1 and <= 24 } || request.ExistingLayers is not { Length: <= 24 })
            {
                throw new ArgumentException("Use 1–24 solids and at most 24 existing layers.");
            }

            foreach (var solid in request.Solids)
            {
                if (solid is null || !ValidText(solid.MaterialName))
                {
                    throw new ArgumentException("Every solid needs a material name of 1–80 characters.");
                }
            }

            foreach (var layer in request.ExistingLayers)
            {
                if (layer is null || !ValidText(layer.Name) || !ValidText(layer.MaterialName))
                {
                    throw new ArgumentException("Layer and material names must contain 1–80 characters.");
                }
            }

            // AutoCAD supplies its validator in the add-in; the browser supplies explicit sample flags.
            var rejectedNames = request.Solids.Where(s => !s.NameIsValid)
                .Select(s => s.MaterialName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var result = MaterialLayerPlanner.Plan(
                request.Solids.Select(s => new SolidMaterial(s.MaterialName, s.IsOnLockedLayer)).ToArray(),
                request.ExistingLayers.Select(l => new ExistingLayer(l.Name, l.MaterialName)),
                name => !rejectedNames.Contains(name));
            return JsonSerializer.Serialize(new PlanResult(
                result.NewLayers.Select(l => new LayerResult(l.Name, l.SourceSolid)).ToArray(),
                result.Changes.Select(c => new ChangeResult(c.Outcome.ToString(), c.Layer, c.UseLayerMaterial)).ToArray(),
                result.Summary()), DemoJson.Default.PlanResult);
        }
        catch (Exception error) when (error is JsonException or ArgumentException)
        {
            return JsonSerializer.Serialize(new PlanError(error is JsonException
                ? "The drawing sample is not valid JSON." : error.Message), DemoJson.Default.PlanError);
        }
    }

    private static bool ValidText(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 80;
}

internal sealed record PlanRequest(SolidInput[] Solids, LayerInput[] ExistingLayers);
internal sealed record SolidInput(string MaterialName, bool IsOnLockedLayer, bool NameIsValid);
internal sealed record LayerInput(string Name, string MaterialName);
internal sealed record LayerResult(string Name, int SourceSolid);
internal sealed record ChangeResult(string Outcome, string? Layer, bool UseLayerMaterial);
internal sealed record PlanResult(LayerResult[] NewLayers, ChangeResult[] Changes, string Summary);
internal sealed record PlanError(string Error);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PlanRequest))]
[JsonSerializable(typeof(PlanResult))]
[JsonSerializable(typeof(PlanError))]
internal sealed partial class DemoJson : JsonSerializerContext;
