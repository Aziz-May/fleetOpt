using FleetOpt.Modules.Optimization.Engine;

namespace FleetOpt.Modules.Optimization.UnitTests.Engine;

public class DeliveryPlannerTests
{
    // ───────────────────────────── basic behaviour ─────────────────────────────

    [Fact]
    public void Plan_WithNoDeliveries_ReturnsAnEmptyPlan()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck();

        var plan = DeliveryPlanner.Plan(scenario.Build());

        Assert.Empty(plan.Routes);
        Assert.Empty(plan.Unassigned);
        Assert.Equal(0, plan.TotalDistanceKm);
    }

    [Fact]
    public void Plan_WithOneDeliveryThatFits_CreatesARouteFromDepotToStopAndBack()
    {
        var scenario = new PlanningScenario();
        var truckId = scenario.AddTruck();
        var water = scenario.AddProduct(unitWeightKg: 2m, unitVolumeM3: 0.01m);
        var deliveryId = scenario.AddDelivery(water, quantity: 50, longitude: 0.01);   // ~1.11 km east

        var plan = DeliveryPlanner.Plan(scenario.Build());

        var route = Assert.Single(plan.Routes);
        Assert.Equal(truckId, route.TruckId);
        var stop = Assert.Single(route.Stops);
        Assert.Equal(1, stop.Sequence);
        Assert.Equal(deliveryId, stop.DeliveryId);
        Assert.Equal(100m, route.LoadWeightKg);             // 50 × 2 kg
        Assert.Equal(0.5m, route.LoadVolumeM3);             // 50 × 0.01 m³
        Assert.InRange(route.DistanceKm, 2.20, 2.24);       // there AND back
        Assert.Empty(plan.Unassigned);
    }

    [Fact]
    public void Plan_ComputesFuelFromDistanceAndConsumption_AndCostFromFuelPrice()
    {
        var scenario = new PlanningScenario().WithFuelPrice(2.5m);
        scenario.AddTruck(fuelLitersPer100Km: 30m);
        var product = scenario.AddProduct();
        scenario.AddDelivery(product, quantity: 1, longitude: 0.1);   // ~11.1 km each way

        var route = Assert.Single(DeliveryPlanner.Plan(scenario.Build()).Routes);

        Assert.Equal(route.DistanceKm * 30 / 100, route.FuelLiters, precision: 1);   // liters = km × L/100km ÷ 100
        Assert.InRange(route.FuelCost, (decimal)route.FuelLiters * 2.5m - 0.02m, (decimal)route.FuelLiters * 2.5m + 0.02m);
    }

    // ───────────────────────────── stock allocation ─────────────────────────────

    [Fact]
    public void Plan_WhenStockIsInsufficient_LeavesTheDeliveryUnassignedWithAnExplanation()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck();
        var product = scenario.AddProduct(stock: 10);
        var deliveryId = scenario.AddDelivery(product, quantity: 25);

        var plan = DeliveryPlanner.Plan(scenario.Build());

        Assert.Empty(plan.Routes);
        var unassigned = Assert.Single(plan.Unassigned);
        Assert.Equal(deliveryId, unassigned.DeliveryId);
        Assert.Equal(UnassignedReason.InsufficientStock, unassigned.Reason);
        Assert.Contains("25", unassigned.Explanation);   // what was needed…
        Assert.Contains("10", unassigned.Explanation);   // …and what was available
    }

    [Fact]
    public void Plan_WhenStockIsContested_TheHigherPriorityDeliveryGetsIt()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck();
        var product = scenario.AddProduct(stock: 100);
        var normal = scenario.AddDelivery(product, quantity: 60, priority: PlanningPriority.Normal);   // added FIRST
        var urgent = scenario.AddDelivery(product, quantity: 60, priority: PlanningPriority.Urgent);

        var plan = DeliveryPlanner.Plan(scenario.Build());

        Assert.Equal(urgent, Assert.Single(Assert.Single(plan.Routes).Stops).DeliveryId);
        var unassigned = Assert.Single(plan.Unassigned);
        Assert.Equal(normal, unassigned.DeliveryId);
        Assert.Equal(UnassignedReason.InsufficientStock, unassigned.Reason);
    }

    [Fact]
    public void Plan_WhenPrioritiesAreEqual_TheEarlierDeadlineWins()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck();
        var product = scenario.AddProduct(stock: 100);
        var later = scenario.AddDelivery(product, quantity: 60, hoursUntilDeadline: 48);
        var sooner = scenario.AddDelivery(product, quantity: 60, hoursUntilDeadline: 12);

        var plan = DeliveryPlanner.Plan(scenario.Build());

        Assert.Equal(sooner, Assert.Single(Assert.Single(plan.Routes).Stops).DeliveryId);
        Assert.Equal(later, Assert.Single(plan.Unassigned).DeliveryId);
    }

    [Fact]
    public void Plan_WhenADeliveryCannotBeLoaded_ItDoesNotConsumeStock()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck(maxWeightKg: 1_000m);
        var heavy = scenario.AddProduct(unitWeightKg: 20m, stock: 100);
        var tooHeavy = scenario.AddDelivery(heavy, quantity: 100, priority: PlanningPriority.Urgent);   // 2 000 kg
        var fits = scenario.AddDelivery(heavy, quantity: 50, priority: PlanningPriority.Low);           // 1 000 kg

        var plan = DeliveryPlanner.Plan(scenario.Build());

        // The urgent one can never fit, so its 100 units must stay available for the low-priority one.
        Assert.Equal(tooHeavy, Assert.Single(plan.Unassigned).DeliveryId);
        Assert.Equal(fits, Assert.Single(Assert.Single(plan.Routes).Stops).DeliveryId);
    }

    [Fact]
    public void Plan_WhenAProductIsUnknown_LeavesTheDeliveryUnassigned()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck();
        var deliveryId = scenario.AddDelivery(productId: Guid.NewGuid(), quantity: 1);   // product never added

        var unassigned = Assert.Single(DeliveryPlanner.Plan(scenario.Build()).Unassigned);

        Assert.Equal(deliveryId, unassigned.DeliveryId);
        Assert.Equal(UnassignedReason.UnknownProduct, unassigned.Reason);
    }

    // ───────────────────────────── truck capacity ─────────────────────────────

    [Fact]
    public void Plan_WhenADeliveryIsHeavierThanTheLargestTruck_ItIsUnassignedAsTooBig()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck(maxWeightKg: 1_000m);
        var product = scenario.AddProduct(unitWeightKg: 10m);
        scenario.AddDelivery(product, quantity: 150);   // 1 500 kg

        var unassigned = Assert.Single(DeliveryPlanner.Plan(scenario.Build()).Unassigned);

        Assert.Equal(UnassignedReason.ExceedsLargestTruckCapacity, unassigned.Reason);
    }

    [Fact]
    public void Plan_RespectsVolume_EvenWhenWeightWouldFit()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck(maxWeightKg: 1_000m, maxVolumeM3: 10m);
        var bulky = scenario.AddProduct(unitWeightKg: 1m, unitVolumeM3: 0.5m);
        scenario.AddDelivery(bulky, quantity: 30);   // only 30 kg, but 15 m³

        var unassigned = Assert.Single(DeliveryPlanner.Plan(scenario.Build()).Unassigned);

        Assert.Equal(UnassignedReason.ExceedsLargestTruckCapacity, unassigned.Reason);
    }

    [Fact]
    public void Plan_WhenTheFirstTruckIsFull_UsesTheNextTruck()
    {
        var scenario = new PlanningScenario();
        var truckA = scenario.AddTruck(maxWeightKg: 1_000m, fuelLitersPer100Km: 20m, plateNumber: "A");
        var truckB = scenario.AddTruck(maxWeightKg: 1_000m, fuelLitersPer100Km: 30m, plateNumber: "B");
        var product = scenario.AddProduct(unitWeightKg: 10m);
        var first = scenario.AddDelivery(product, quantity: 60, hoursUntilDeadline: 10);    // 600 kg
        var second = scenario.AddDelivery(product, quantity: 60, hoursUntilDeadline: 20);   // 600 kg: no room left in A

        var plan = DeliveryPlanner.Plan(scenario.Build());

        Assert.Equal(2, plan.Routes.Count);
        Assert.Equal(first, Assert.Single(plan.Routes.Single(r => r.TruckId == truckA).Stops).DeliveryId);
        Assert.Equal(second, Assert.Single(plan.Routes.Single(r => r.TruckId == truckB).Stops).DeliveryId);
        Assert.Empty(plan.Unassigned);
    }

    [Fact]
    public void Plan_WhenEveryTruckIsFull_LeavesTheDeliveryUnassignedForLackOfCapacity()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck(maxWeightKg: 1_000m);
        var product = scenario.AddProduct(unitWeightKg: 10m);
        scenario.AddDelivery(product, quantity: 60, hoursUntilDeadline: 10);
        var second = scenario.AddDelivery(product, quantity: 60, hoursUntilDeadline: 20);

        var unassigned = Assert.Single(DeliveryPlanner.Plan(scenario.Build()).Unassigned);

        Assert.Equal(second, unassigned.DeliveryId);
        Assert.Equal(UnassignedReason.NoRemainingTruckCapacity, unassigned.Reason);
    }

    [Fact]
    public void Plan_WithoutAnyTruck_LeavesEveryDeliveryUnassigned()
    {
        var scenario = new PlanningScenario();
        var product = scenario.AddProduct();
        scenario.AddDelivery(product, quantity: 1);

        var unassigned = Assert.Single(DeliveryPlanner.Plan(scenario.Build()).Unassigned);

        Assert.Equal(UnassignedReason.NoRemainingTruckCapacity, unassigned.Reason);
    }

    [Fact]
    public void Plan_FillsTheMostFuelEfficientTruckFirst()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck(fuelLitersPer100Km: 40m, plateNumber: "THIRSTY");     // added first on purpose
        var efficient = scenario.AddTruck(fuelLitersPer100Km: 20m, plateNumber: "EFFICIENT");
        var product = scenario.AddProduct();
        scenario.AddDelivery(product, quantity: 1);

        var route = Assert.Single(DeliveryPlanner.Plan(scenario.Build()).Routes);   // unused trucks get no route

        Assert.Equal(efficient, route.TruckId);
    }

    // ───────────────────────────── routing & totals ─────────────────────────────

    [Fact]
    public void Plan_OrdersTheStopsOfARouteToReduceDistance()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck();
        var product = scenario.AddProduct();
        var far = scenario.AddDelivery(product, 1, longitude: 0.05, hoursUntilDeadline: 1);   // most urgent, but farthest
        var near = scenario.AddDelivery(product, 1, longitude: 0.01, hoursUntilDeadline: 2);
        var middle = scenario.AddDelivery(product, 1, longitude: 0.03, hoursUntilDeadline: 3);

        var stops = Assert.Single(DeliveryPlanner.Plan(scenario.Build()).Routes).Stops;

        // Priority decides WHO gets a truck; geography decides the ORDER of stops.
        Assert.Equal([near, middle, far], stops.Select(s => s.DeliveryId));
        Assert.Equal([1, 2, 3], stops.Select(s => s.Sequence));
        Assert.InRange(stops[0].DistanceFromPreviousKm, 1.10, 1.12);
    }

    [Fact]
    public void Plan_Totals_AreTheSumOfAllRoutes()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck(maxWeightKg: 500m);
        scenario.AddTruck(maxWeightKg: 500m);
        var product = scenario.AddProduct(unitWeightKg: 10m);
        scenario.AddDelivery(product, quantity: 40, longitude: 0.02, hoursUntilDeadline: 1);
        scenario.AddDelivery(product, quantity: 40, latitude: 0.03, longitude: 0, hoursUntilDeadline: 2);

        var plan = DeliveryPlanner.Plan(scenario.Build());

        Assert.Equal(2, plan.Routes.Count);
        Assert.Equal(Math.Round(plan.Routes.Sum(r => r.DistanceKm), 2), plan.TotalDistanceKm);
        Assert.Equal(plan.Routes.Sum(r => r.FuelCost), plan.TotalFuelCost);
    }

    [Fact]
    public void Plan_IsDeterministic_SameInputGivesSamePlan()
    {
        var scenario = new PlanningScenario();
        scenario.AddTruck(maxWeightKg: 300m);
        scenario.AddTruck(maxWeightKg: 300m);
        var product = scenario.AddProduct(unitWeightKg: 5m);
        for (var i = 0; i < 6; i++)
            scenario.AddDelivery(product, quantity: 20, latitude: i * 0.01, longitude: (5 - i) * 0.01, hoursUntilDeadline: i + 1);
        var request = scenario.Build();

        var first = DeliveryPlanner.Plan(request);
        var second = DeliveryPlanner.Plan(request);

        Assert.Equal(
            first.Routes.SelectMany(r => r.Stops.Select(s => (r.TruckId, s.DeliveryId))),
            second.Routes.SelectMany(r => r.Stops.Select(s => (r.TruckId, s.DeliveryId))));
        Assert.Equal(first.TotalDistanceKm, second.TotalDistanceKm);
    }
}
