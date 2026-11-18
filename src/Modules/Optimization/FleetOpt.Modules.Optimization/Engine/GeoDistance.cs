namespace FleetOpt.Modules.Optimization.Engine;

/// <summary>
/// Straight-line ("as the crow flies") distance between two GPS positions.
/// </summary>
/// <remarks>
/// <para>
/// Uses the <b>Haversine formula</b>: the shortest path between two points on a sphere
/// (the "great circle"). The Earth is treated as a perfect sphere of radius 6 371 km;
/// the error versus the real (slightly flattened) Earth is below 0.5 %.
/// </para>
/// <para>
/// <b>Known limitation:</b> trucks drive on roads, not in straight lines. Real road distance is
/// typically 20–40 % longer. Straight-line distance is still a good <i>relative</i> measure for
/// comparing routes ("is A→B→C shorter than A→C→B?"), which is what the optimizer needs.
/// A road-network service (OSRM, Google Distance Matrix…) could replace this class later
/// without changing the algorithms that use it.
/// </para>
/// <para>
/// Uses <see cref="double"/>, not <see cref="decimal"/>: trigonometry (sin, cos, asin) only exists
/// for floating-point numbers, and a few millimetres of rounding error are irrelevant here.
/// Money, on the other hand, is computed in decimal (see <see cref="DeliveryPlanner"/>).
/// </para>
/// </remarks>
internal static class GeoDistance
{
    /// <summary>Mean radius of the Earth.</summary>
    public const double EarthRadiusKm = 6_371.0;

    public static double Kilometers(GeoPoint from, GeoPoint to)
    {
        var latitude1 = ToRadians(from.Latitude);
        var latitude2 = ToRadians(to.Latitude);
        var deltaLatitude = ToRadians(to.Latitude - from.Latitude);
        var deltaLongitude = ToRadians(to.Longitude - from.Longitude);

        // h = haversine of the central angle between the two points
        var h = Math.Pow(Math.Sin(deltaLatitude / 2), 2)
              + Math.Cos(latitude1) * Math.Cos(latitude2) * Math.Pow(Math.Sin(deltaLongitude / 2), 2);

        // Math.Min guards against floating-point rounding pushing sqrt(h) slightly above 1,
        // which would make Asin return NaN for (almost) antipodal points.
        var centralAngle = 2 * Math.Asin(Math.Min(1.0, Math.Sqrt(h)));

        return EarthRadiusKm * centralAngle;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
