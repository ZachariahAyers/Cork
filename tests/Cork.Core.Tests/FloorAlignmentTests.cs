using Cork.Core;

namespace Cork.Core.Tests;

public sealed class FloorAlignmentTests
{
    [Theory]
    [InlineData(12.5, -12.5)]
    [InlineData(-3.0, 3.0)]
    [InlineData(1e-9, -1e-9)]
    public void MovesThePickedPointToTheXYPlane(double pickedZ, double expected)
    {
        Assert.Equal(expected, FloorAlignment.ZOffset(pickedZ));
    }

    [Fact]
    public void PointOnThePlaneNeedsNoMove()
    {
        var offset = FloorAlignment.ZOffset(0);

        Assert.Equal(0, offset);
        Assert.False(double.IsNegative(offset));
    }
}
