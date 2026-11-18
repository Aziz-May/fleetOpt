using FleetOpt.Modules.Optimization.Engine;

namespace FleetOpt.Modules.Optimization.UnitTests.Engine;

public class GeoDistanceTests
{
    [Fact]
    public void Kilometers_BetweenSamePoint_IsZero()
    {
        var tunis = new GeoPoint(36.8065, 10.1815);

        Assert.Equal(0, GeoDistance.Kilometers(tunis, tunis), precision: 6);
    }

    [Fact]
    public void Kilometers_FromParisToLondon_IsAbout344Km()
    {
        // Well-known great-circle distance: ~343.5 km
        var paris = new GeoPoint(48.8566, 2.3522);
        var london = new GeoPoint(51.5074, -0.1278);

        Assert.InRange(GeoDistance.Kilometers(paris, london), 342.5, 344.5);
    }

    [Fact]
    public void Kilometers_IsSymmetric()
    {
        var a = new GeoPoint(36.8065, 10.1815);
        var b = new GeoPoint(34.7406, 10.7603);

        Assert.Equal(GeoDistance.Kilometers(a, b), GeoDistance.Kilometers(b, a), precision: 9);
    }

    [Fact]
    public void Kilometers_ForOneHundredthOfDegreeAtEquator_IsAbout1Point11Km()
    {
        // The rule of thumb the other engine tests rely on.
        var distance = GeoDistance.Kilometers(new GeoPoint(0, 0), new GeoPoint(0, 0.01));

        Assert.InRange(distance, 1.10, 1.12);
    }
}
