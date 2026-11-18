namespace FleetOpt.Modules.Optimization.Engine;

/// <summary>
/// Decides in which ORDER one truck visits its stops, to keep the driven distance short.
/// </summary>
/// <remarks>
/// <para>
/// This is the <b>Travelling Salesman Problem</b> (TSP): visit every stop once, start and end at the depot,
/// minimise total distance. Finding the true optimum is NP-hard: the number of possible orders is n!
/// (10 stops → 3 628 800 orders, 15 stops → 1.3 trillion). So we use two classic <b>heuristics</b>:
/// fast methods that give a good (not guaranteed optimal) answer.
/// </para>
/// <list type="number">
///   <item><b>Nearest Neighbor</b> (construction): from where you are, drive to the closest unvisited stop.
///   Simple, O(n²), usually within ~25 % of optimal, but it can leave "crossing" edges.</item>
///   <item><b>2-opt</b> (improvement): repeatedly look for two edges that cross and "uncross" them by
///   reversing the segment between them, as long as that makes the route shorter.</item>
/// </list>
/// <para>
/// Methods work with <b>indexes</b> into the <c>stops</c> list (0, 1, 2…), not with deliveries:
/// the class knows nothing about trucks or products. It only knows geometry. That makes it reusable
/// and easy to test.
/// </para>
/// </remarks>
internal static class RouteOptimizer
{
    /// <summary>Tiny tolerance so that floating-point noise is never mistaken for an improvement.</summary>
    private const double Epsilon = 1e-9;

    /// <summary>Nearest Neighbor to build a route, then 2-opt to improve it.</summary>
    /// <returns>The order in which to visit the stops, as indexes into <paramref name="stops"/>.</returns>
    public static IReadOnlyList<int> BuildRoute(GeoPoint depot, IReadOnlyList<GeoPoint> stops)
        => ImproveWithTwoOpt(depot, stops, NearestNeighbor(depot, stops));

    /// <summary>
    /// Greedy construction: start at the depot, always go to the closest stop not visited yet.
    /// </summary>
    /// <remarks>
    /// Ties (two stops at exactly the same distance) go to the stop with the lowest index,
    /// so the result is deterministic: same input → same route, every time.
    /// </remarks>
    public static IReadOnlyList<int> NearestNeighbor(GeoPoint depot, IReadOnlyList<GeoPoint> stops)
    {
        var order = new List<int>(stops.Count);
        var visited = new bool[stops.Count];
        var current = depot;

        for (var step = 0; step < stops.Count; step++)
        {
            var closest = -1;
            var closestDistance = double.MaxValue;

            for (var candidate = 0; candidate < stops.Count; candidate++)
            {
                if (visited[candidate])
                    continue;

                var distance = GeoDistance.Kilometers(current, stops[candidate]);
                if (distance < closestDistance)   // strict "<": on a tie the earlier index wins
                {
                    closestDistance = distance;
                    closest = candidate;
                }
            }

            visited[closest] = true;
            order.Add(closest);
            current = stops[closest];
        }

        return order;
    }

    /// <summary>
    /// 2-opt local search: removes "crossings" from a route until no single 2-opt move can shorten it.
    /// </summary>
    /// <remarks>
    /// <para>The full tour is: depot → order[0] → order[1] → … → order[n-1] → depot.</para>
    /// <para>
    /// For every pair of positions (i, k), consider the two edges around the segment order[i..k]:
    /// <code>
    ///   before:  a → [b … c] → d          a = point before position i, b = order[i]
    ///   after:   a → [c … b] → d          c = order[k],                d = point after position k
    /// </code>
    /// Reversing the segment replaces edges (a,b) + (c,d) with (a,c) + (b,d); everything inside the
    /// segment keeps the same length (distances are symmetric). So the gain can be computed from
    /// just 4 distances, without re-measuring the whole route:
    /// <code>  delta = dist(a,c) + dist(b,d) − dist(a,b) − dist(c,d)  </code>
    /// If delta &lt; 0 the route gets shorter → apply the reversal and keep searching.
    /// </para>
    /// <para>
    /// Termination: every accepted move makes the route strictly shorter, and there is a finite number
    /// of routes, so the loop must end. Cost: O(n²) per pass; a handful of passes in practice.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<int> ImproveWithTwoOpt(GeoPoint depot, IReadOnlyList<GeoPoint> stops, IReadOnlyList<int> initialOrder)
    {
        var order = initialOrder.ToArray();   // work on a copy: never modify the caller's list
        var improved = true;

        while (improved)
        {
            improved = false;

            for (var i = 0; i < order.Length - 1; i++)
            {
                for (var k = i + 1; k < order.Length; k++)
                {
                    var a = i == 0 ? depot : stops[order[i - 1]];
                    var b = stops[order[i]];
                    var c = stops[order[k]];
                    var d = k == order.Length - 1 ? depot : stops[order[k + 1]];

                    var delta = GeoDistance.Kilometers(a, c) + GeoDistance.Kilometers(b, d)
                              - GeoDistance.Kilometers(a, b) - GeoDistance.Kilometers(c, d);

                    if (delta < -Epsilon)
                    {
                        Array.Reverse(order, i, k - i + 1);
                        improved = true;
                    }
                }
            }
        }

        return order;
    }

    /// <summary>Total length of the tour depot → stops (in the given order) → depot.</summary>
    public static double RouteLengthKm(GeoPoint depot, IReadOnlyList<GeoPoint> stops, IReadOnlyList<int> order)
    {
        var total = 0.0;
        var current = depot;

        foreach (var index in order)
        {
            total += GeoDistance.Kilometers(current, stops[index]);
            current = stops[index];
        }

        return total + GeoDistance.Kilometers(current, depot);   // drive back to the warehouse
    }
}
