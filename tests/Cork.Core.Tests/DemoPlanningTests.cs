using System.Text.Json;
using Cork.Demo;

namespace Cork.Core.Tests;

public sealed class DemoPlanningTests
{
    [Fact]
    public void RunsTheRealPlanAndPreservesSkipReasons()
    {
        const string input = """
            {"solids":[
              {"materialName":"Oak","nameIsValid":true},
              {"materialName":"oak","nameIsValid":true},
              {"materialName":"Steel","isOnLockedLayer":true,"nameIsValid":true},
              {"materialName":"ByLayer","nameIsValid":true},
              {"materialName":"Bad/name","nameIsValid":false}
            ],"existingLayers":[]}
            """;
        using var result = JsonDocument.Parse(Planning.Run(input));
        var root = result.RootElement;
        Assert.Single(root.GetProperty("newLayers").EnumerateArray());
        Assert.Equal("Oak", root.GetProperty("newLayers")[0].GetProperty("name").GetString());
        var changes = root.GetProperty("changes");
        Assert.Equal("Moved", changes[0].GetProperty("outcome").GetString());
        Assert.True(changes[1].GetProperty("useLayerMaterial").GetBoolean());
        Assert.Equal("SkippedLockedLayer", changes[2].GetProperty("outcome").GetString());
        Assert.Equal("SkippedInheritedMaterial", changes[3].GetProperty("outcome").GetString());
        Assert.Equal("SkippedInvalidName", changes[4].GetProperty("outcome").GetString());
    }

    [Theory]
    [InlineData("{")]
    [InlineData("null")]
    [InlineData("{}")]
    [InlineData("{\"solids\":[null],\"existingLayers\":[]}")]
    [InlineData("{\"solids\":[{\"materialName\":\" \"}],\"existingLayers\":[]}")]
    public void RejectsMalformedOrMissingInputs(string input)
    {
        using var result = JsonDocument.Parse(Planning.Run(input));
        Assert.False(string.IsNullOrWhiteSpace(result.RootElement.GetProperty("error").GetString()));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(1_000_001)]
    public void RejectsInvalidFloorCoordinates(double coordinate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Planning.Floor(coordinate));
    }

    [Fact]
    public void UsesTheFloorCalculation()
    {
        Assert.Equal(-42.5, Planning.Floor(42.5));
    }

    [Fact]
    public void RejectsOversizedPayloads()
    {
        using var result = JsonDocument.Parse(Planning.Run(new string(' ', 16_385)));
        Assert.Contains("16 KB", result.RootElement.GetProperty("error").GetString(), StringComparison.Ordinal);
    }
}
