namespace Cork.Core;

/// <summary>
/// The move behind the Floor command: drop a selection so a picked point lands on the XY plane
/// of the current user coordinate system.
/// </summary>
public static class FloorAlignment
{
    /// <summary>
    /// Returns the Z displacement, in UCS units, that moves a point at <paramref name="pickedZ"/> to Z = 0.
    /// </summary>
    /// <param name="pickedZ">Z coordinate of the picked point, in UCS units.</param>
    /// <returns>The displacement along the UCS Z axis; zero when the point is already on the plane.</returns>
    public static double ZOffset(double pickedZ) => pickedZ == 0 ? 0 : -pickedZ;
}
