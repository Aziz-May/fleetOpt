using FleetOpt.Modules.Optimization.Engine;

namespace FleetOpt.Modules.Optimization.UnitTests.Engine;

public class RouteOptimizerTests
{
    private static readonly GeoPoint Depot = new(0, 0);

    [Fact]
    public void BuildRoute_WithoutStops_ReturnsEmptyOrder()
    {
        Assert.Empty(RouteOptimizer.BuildRoute(Depot, []));
    }

    [Fact]
    public void NearestNeighbor_AlwaysDrivesToTheClosestUnvisitedStop()
    {
        // All stops on one line east of the depot, given in a scrambled order.
        GeoPoint[] stops =
        [
            new(0, 0.05),   // index 0: far     (~5.6 km)
            new(0, 0.01),   // index 1: near    (~1.1 km)
            new(0, 0.03)    // index 2: middle  (~3.3 km)
        ];

        var order = RouteOptimizer.NearestNeighbor(Depot, stops);

        Assert.Equal([1, 2, 0], order);   // near → middle → far
    }

    [Fact]
    public void ImproveWithTwoOpt_OnACrossingRoute_UncrossesItAndShortensIt()
    {
        // A square: depot at (0,0), stops North, East and North-East (~1.11 km sides).
        GeoPoint[] stops =
        [
            new(0.01, 0),       // 0: North
            new(0, 0.01),       // 1: East
            new(0.01, 0.01)     // 2: North-East
        ];
        int[] crossing = [0, 1, 2];   // North → East → North-East → depot: two diagonals cross

        var improved = RouteOptimizer.ImproveWithTwoOpt(Depot, stops, crossing);

        var before = RouteOptimizer.RouteLengthKm(Depot, stops, crossing);
        var after = RouteOptimizer.RouteLengthKm(Depot, stops, improved);
        Assert.True(after < before, $"2-opt should shorten the route ({before:F2} km → {after:F2} km)");
        Assert.InRange(after, 4.40, 4.50);   // the perimeter of the square: 4 × ~1.11 km
    }

    [Fact]
    public void BuildRoute_VisitsEveryStopExactlyOnce_AndIsNeverLongerThanNearestNeighbor()
    {
        // A property checked on many random instances (fixed seed → the test is reproducible).
        var random = new Random(42);
        for (var instance = 0; instance < 50; instance++)
        {
            var stops = Enumerable.Range(0, 8)
                .Select(_ => new GeoPoint(random.NextDouble() * 0.2, random.NextDouble() * 0.2))
                .ToArray();

            var nearestNeighbor = RouteOptimizer.NearestNeighbor(Depot, stops);
            var route = RouteOptimizer.BuildRoute(Depot, stops);

            Assert.Equal(Enumerable.Range(0, stops.Length), route.Order());
            Assert.True(
                RouteOptimizer.RouteLengthKm(Depot, stops, route)
                <= RouteOptimizer.RouteLengthKm(Depot, stops, nearestNeighbor) + 1e-9);
        }
    }
}
