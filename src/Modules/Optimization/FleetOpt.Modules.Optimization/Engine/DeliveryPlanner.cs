namespace FleetOpt.Modules.Optimization.Engine;

/// <summary>
/// Turns a <see cref="PlanningRequest"/> (trucks, deliveries, stock) into a <see cref="DeliveryPlan"/>
/// (which truck carries which deliveries, in which order, at which estimated cost).
/// </summary>
/// <remarks>
/// <para><b>The problem.</b> This is a simplified <i>Capacitated Vehicle Routing Problem</i> (CVRP):
/// several trucks with weight AND volume limits, one depot, many destinations, limited stock.
/// Solving it optimally is NP-hard, so the planner uses a <b>deterministic greedy heuristic</b>
/// in two phases:</para>
///
/// <para><b>Phase 1, assignment (who goes on which truck?).</b> Deliveries are processed one by one,
/// most important first (priority desc, then deadline asc). For each delivery:</para>
/// <list type="number">
///   <item>every product must be known (we need its weight and volume) → else <see cref="UnassignedReason.UnknownProduct"/></item>
///   <item>enough stock must remain for every line → else <see cref="UnassignedReason.InsufficientStock"/></item>
///   <item>it must fit in at least one EMPTY truck → else <see cref="UnassignedReason.ExceedsLargestTruckCapacity"/></item>
///   <item><b>First Fit</b>: put it in the first truck (most fuel-efficient first) that still has room
///         → else <see cref="UnassignedReason.NoRemainingTruckCapacity"/></item>
///   <item>only once a truck accepted it is the stock <b>reserved</b> (decremented). A delivery that cannot
///         be loaded never takes stock away from the deliveries after it.</item>
/// </list>
///
/// <para><b>Phase 2, routing (in which order does each truck drive?).</b> For each truck that received
/// deliveries: <see cref="RouteOptimizer"/> (Nearest Neighbor + 2-opt), then distance, fuel and cost.</para>
///
/// <para><b>Why priority decides assignment but not the stop order:</b> priority answers
/// "who gets the scarce stock and truck space?". Once a delivery is on a truck, it WILL be delivered
/// today, so the driving order should minimise distance. (Respecting exact delivery time windows is a
/// harder problem, VRPTW, deliberately out of scope for the MVP.)</para>
///
/// <para><b>Properties:</b> pure function (no I/O, no clock, no randomness, input never modified) and
/// deterministic (same input → same plan). That is what makes it easy to test and to explain.</para>
///
/// <para><b>Complexity:</b> assignment O(D × T) for D deliveries and T trucks; routing O(n²) per truck
/// pass. Fine for hundreds of deliveries per warehouse.</para>
/// </remarks>
internal static class DeliveryPlanner
{
    public static DeliveryPlan Plan(PlanningRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Mutable working state lives in LOCAL variables: the request itself is never modified.
        var remainingStock = new Dictionary<Guid, int>(request.AvailableStock);
        var trucks = request.Trucks
            .OrderBy(t => t.FuelLitersPer100Km)   // cheapest-to-run trucks are filled first
            .Select(t => new TruckLoad(t))         // OrderBy is stable: equal consumption keeps input order
            .ToList();
        var unassigned = new List<UnassignedDelivery>();

        // ── Phase 1: assignment ────────────────────────────────────────────────
        foreach (var delivery in OrderByImportance(request.Deliveries))
        {
            var rejection = TryAssign(delivery, request.Products, remainingStock, trucks);
            if (rejection is not null)
                unassigned.Add(rejection);
        }

        // ── Phase 2: routing ───────────────────────────────────────────────────
        var routes = trucks
            .Where(t => t.Deliveries.Count > 0)   // an unused truck gets no route
            .Select(t => BuildRoute(t, request.Depot, request.FuelPricePerLiter))
            .ToList();

        return new DeliveryPlan(routes, unassigned);
    }

    /// <summary>Most important first. The final tie-breaker (id) guarantees a stable, deterministic order.</summary>
    private static IEnumerable<PlanningDelivery> OrderByImportance(IEnumerable<PlanningDelivery> deliveries)
        => deliveries
            .OrderByDescending(d => d.Priority)
            .ThenBy(d => d.Deadline)
            .ThenBy(d => d.DeliveryId);

    /// <summary>
    /// Tries to place one delivery on a truck and reserve its stock.
    /// Returns <c>null</c> on success, or the reason it could not be planned.
    /// </summary>
    private static UnassignedDelivery? TryAssign(
        PlanningDelivery delivery,
        IReadOnlyDictionary<Guid, PlanningProduct> products,
        Dictionary<Guid, int> remainingStock,
        List<TruckLoad> trucks)
    {
        // Merge lines per product (robustness: the same product could appear twice).
        var demand = delivery.Lines
            .GroupBy(l => l.ProductId)
            .Select(g => (ProductId: g.Key, Quantity: g.Sum(l => l.Quantity)))
            .ToList();

        // 1. Every product must be known, otherwise weight/volume cannot be computed.
        var unknownProductIds = demand.Where(d => !products.ContainsKey(d.ProductId)).Select(d => d.ProductId).ToList();
        if (unknownProductIds.Count > 0)
            return Reject(delivery, UnassignedReason.UnknownProduct,
                $"Unknown product(s): {string.Join(", ", unknownProductIds)}.");

        // 2. Enough stock must remain (higher-priority deliveries have already reserved theirs).
        foreach (var (productId, quantity) in demand)
        {
            var available = remainingStock.GetValueOrDefault(productId);
            if (available < quantity)
                return Reject(delivery, UnassignedReason.InsufficientStock,
                    $"Needs {quantity} units of product {productId}, but only {available} remain in the warehouse.");
        }

        // 3. Physical size of the delivery.
        var weightKg = demand.Sum(d => d.Quantity * products[d.ProductId].UnitWeightKg);
        var volumeM3 = demand.Sum(d => d.Quantity * products[d.ProductId].UnitVolumeM3);

        if (trucks.Count == 0)
            return Reject(delivery, UnassignedReason.NoRemainingTruckCapacity, "No trucks are available.");

        // 4. Could it fit in ANY truck if that truck were empty? If not, it never will: say so clearly.
        if (!trucks.Any(t => t.FitsWhenEmpty(weightKg, volumeM3)))
            return Reject(delivery, UnassignedReason.ExceedsLargestTruckCapacity,
                $"Load of {weightKg} kg / {volumeM3} m³ exceeds the capacity of every truck.");

        // 5. First Fit: the first truck (most fuel-efficient first) that still has room.
        var truck = trucks.FirstOrDefault(t => t.HasRoomFor(weightKg, volumeM3));
        if (truck is null)
            return Reject(delivery, UnassignedReason.NoRemainingTruckCapacity,
                $"Load of {weightKg} kg / {volumeM3} m³ does not fit in the remaining space of any truck.");

        // 6. Commit: only now do we load the truck AND reserve the stock.
        truck.Load(delivery, weightKg, volumeM3);
        foreach (var (productId, quantity) in demand)
            remainingStock[productId] -= quantity;

        return null;
    }

    private static UnassignedDelivery Reject(PlanningDelivery delivery, UnassignedReason reason, string explanation)
        => new(delivery.DeliveryId, reason, explanation);

    /// <summary>Orders the truck's stops, then computes distance, fuel and cost.</summary>
    private static PlannedRoute BuildRoute(TruckLoad truckLoad, GeoPoint depot, decimal fuelPricePerLiter)
    {
        var deliveries = truckLoad.Deliveries;
        var destinations = deliveries.Select(d => d.Destination).ToList();
        var order = RouteOptimizer.BuildRoute(depot, destinations);

        var stops = new List<PlannedStop>(order.Count);
        var previous = depot;
        for (var position = 0; position < order.Count; position++)
        {
            var delivery = deliveries[order[position]];
            var legKm = GeoDistance.Kilometers(previous, delivery.Destination);
            stops.Add(new PlannedStop(position + 1, delivery.DeliveryId, Math.Round(legKm, 2)));
            previous = delivery.Destination;
        }

        var distanceKm = RouteOptimizer.RouteLengthKm(depot, destinations, order);   // includes the return trip
        var fuelLiters = distanceKm * (double)truckLoad.Truck.FuelLitersPer100Km / 100.0;
        var fuelCost = Math.Round((decimal)fuelLiters * fuelPricePerLiter, 2);         // money → decimal

        return new PlannedRoute(
            truckLoad.Truck.TruckId,
            truckLoad.Truck.PlateNumber,
            stops,
            truckLoad.WeightKg,
            truckLoad.VolumeM3,
            Math.Round(distanceKm, 2),
            Math.Round(fuelLiters, 2),
            fuelCost);
    }

    /// <summary>
    /// Working state for one truck during planning: what it carries so far.
    /// Private to the planner: it is a calculation helper, not part of the result.
    /// </summary>
    private sealed class TruckLoad(PlanningTruck truck)
    {
        private readonly List<PlanningDelivery> _deliveries = [];

        public PlanningTruck Truck { get; } = truck;
        public decimal WeightKg { get; private set; }
        public decimal VolumeM3 { get; private set; }
        public IReadOnlyList<PlanningDelivery> Deliveries => _deliveries;

        public bool FitsWhenEmpty(decimal weightKg, decimal volumeM3)
            => weightKg <= Truck.MaxWeightKg && volumeM3 <= Truck.MaxVolumeM3;

        public bool HasRoomFor(decimal weightKg, decimal volumeM3)
            => WeightKg + weightKg <= Truck.MaxWeightKg && VolumeM3 + volumeM3 <= Truck.MaxVolumeM3;

        public void Load(PlanningDelivery delivery, decimal weightKg, decimal volumeM3)
        {
            _deliveries.Add(delivery);
            WeightKg += weightKg;
            VolumeM3 += volumeM3;
        }
    }
}
