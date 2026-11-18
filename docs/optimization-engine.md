# FleetOpt optimization engine

> Code: `src/Modules/Optimization/FleetOpt.Modules.Optimization/Engine/`
> Tests: `tests/FleetOpt.Modules.Optimization.UnitTests/Engine/` (25 tests)

## 1. The problem in plain words

A distribution company has **one warehouse** (the *depot*), **some trucks** and a list of **deliveries waiting**.
Every morning it must decide:

1. **Which deliveries can be served today?** Stock is limited, and trucks are limited.
2. **Which truck carries which deliveries?** A truck has a maximum **weight** (kg) and a maximum **volume** (m³).
3. **In which order does each truck drive?** Less distance means less fuel and lower cost.
4. **What will it cost?** Distance → fuel → money.
5. **What could not be planned, and why?** A planner must give an explanation for every refusal.

In computer science this is the **Capacitated Vehicle Routing Problem (CVRP)**, combined with
**stock allocation**. Finding the *perfect* answer is **NP-hard**: the number of possibilities explodes
(10 stops alone can be ordered in 3,628,800 ways). Real planners therefore use **heuristics**, which are
fast methods that give *good* answers and are explainable and deterministic.

## 2. Inputs and outputs

| Input (`PlanningRequest`) | Meaning |
|---|---|
| `Depot` | GPS position of the warehouse. Every route starts and ends here. |
| `Trucks` | id, plate, `MaxWeightKg`, `MaxVolumeM3`, `FuelLitersPer100Km` |
| `Deliveries` | id, destination, `Priority` (Low…Urgent), `Deadline`, lines (product + quantity) |
| `Products` | unit weight and unit volume per product |
| `AvailableStock` | units available in the depot per product |
| `FuelPricePerLiter` | to convert liters into money |

| Output (`DeliveryPlan`) | Meaning |
|---|---|
| `Routes` | one per **used** truck: ordered stops, load, distance, fuel, cost |
| `Unassigned` | every delivery not planned, with a `Reason` and a human `Explanation` |
| `TotalDistanceKm`, `TotalFuelLiters`, `TotalFuelCost` | sums over all routes |

The engine has **its own input types**. It does not use `Truck`, `Delivery` or `Product` from the other
modules. The Optimization module (next step) translates the other modules' contracts into these records.
The engine therefore depends on **nothing**: no EF Core, no HTTP, no other module.

## 3. The algorithm

```
                    ┌─────────────────────────── PHASE 1: ASSIGNMENT ───────────────────────────┐
deliveries ──sort──►│ for each delivery (most important first):                                │
 (priority desc,    │   1. all products known?            no → UnknownProduct                  │
  deadline asc,     │   2. enough stock left?             no → InsufficientStock               │
  id asc)           │   3. fits in an EMPTY truck?        no → ExceedsLargestTruckCapacity     │
                    │   4. first truck with room left     none → NoRemainingTruckCapacity      │
                    │      (trucks sorted: lowest fuel consumption first)                      │
                    │   5. load truck + reserve stock                                          │
                    └──────────────────────────────────────────────────────────────────────────┘
                    ┌─────────────────────────── PHASE 2: ROUTING (per used truck) ─────────────┐
                    │   Nearest Neighbor  → initial order                                       │
                    │   2-opt             → remove crossings until no improvement               │
                    │   distance (Haversine) → fuel = km × L/100km ÷ 100 → cost = fuel × price  │
                    └──────────────────────────────────────────────────────────────────────────┘
```

### 3.1 Why sort by priority, then deadline?
Stock and truck space are **scarce**. Whoever is processed first gets them. Sorting decides **who wins
when there isn't enough for everyone**: urgent before normal, and between equals, the earliest deadline.
The last tie-breaker (delivery id) makes the order **total**, so the result is identical every run.

### 3.2 Why check "fits in an empty truck" separately?
Two very different situations both look like "no truck":

* the delivery is **too big for any truck, ever** → it needs splitting or a bigger vehicle (`ExceedsLargestTruckCapacity`)
* the delivery would fit, but **trucks are already full** today → plan it tomorrow or add a truck (`NoRemainingTruckCapacity`)

A dispatcher reacts differently to each, so the explanation must be different.

### 3.3 Why reserve stock only after a truck accepted the delivery?
If stock were reserved first, an urgent delivery that can **never** be loaded would "eat" units that a
lower-priority delivery could have used. Test: `Plan_WhenADeliveryCannotBeLoaded_ItDoesNotConsumeStock`.

### 3.4 First Fit, and why not something "smarter"?
**First Fit** = put the item in the first bin with enough room. It is the classic **bin-packing** heuristic.

| Option | Idea | Trade-off |
|---|---|---|
| **First Fit in priority order** *(chosen)* | serve the most important first | simple, explainable, priorities respected; may pack trucks less tightly |
| First Fit **Decreasing** | sort by size, biggest first | packs tighter (fewer trucks), but **ignores priority**: a big low-priority delivery could take the space of an urgent one |
| Best Fit | the truck that leaves the least free space | slightly tighter packing, harder to explain |
| Exact (knapsack / ILP solver) | optimal | slow, needs a solver library, overkill for the MVP |

Trucks are tried **most fuel-efficient first**, so the cheapest trucks to run are used before the thirsty ones.

### 3.5 Routing: Nearest Neighbor + 2-opt
Once a truck's deliveries are known, the question is the **order** of stops. That is the
**Travelling Salesman Problem**.

* **Nearest Neighbor**: "from where I am, drive to the closest stop I haven't visited." O(n²), simple,
  usually within about 25% of optimal, but it can produce routes that **cross themselves**.
* **2-opt**: take two edges `a→b` and `c→d`; if `a→c` + `b→d` is shorter, reverse the segment between
  them, which removes the crossing. Repeat until no swap helps. Only 4 distances are needed per check:

```
before:  a → b … c → d        delta = dist(a,c) + dist(b,d) − dist(a,b) − dist(c,d)
after:   a → c … b → d        if delta < 0 → the route got shorter → keep it
```

Test `BuildRoute_VisitsEveryStopExactlyOnce_AndIsNeverLongerThanNearestNeighbor` checks on 50 random
instances that 2-opt **never makes a route worse** and never loses or duplicates a stop.

### 3.6 Priority decides *who*, geography decides *the order*
An urgent delivery gets its place on a truck first, but it is **not** necessarily the first stop: once it
is on the truck it will be delivered today, so the stops are ordered to minimise distance.
Guaranteeing arrival **before a precise time** is the *VRP with Time Windows* (VRPTW), a harder problem
that is deliberately out of scope for the MVP (see section 6).

### 3.7 Distance, fuel and cost
* **Distance:** Haversine (great-circle) formula, straight line on a sphere of radius 6,371 km.
* **Fuel:** `liters = km × FuelLitersPer100Km ÷ 100`
* **Cost:** `liters × FuelPricePerLiter`, computed in `decimal` (money), rounded to 2 decimals.
  Geometry uses `double` because trigonometry only exists for floating-point numbers.

## 4. A worked example

Depot in Tunis. Two trucks: **A** (1,000 kg, 20 L/100 km) and **B** (1,000 kg, 30 L/100 km). Product: water, 10 kg per pack, 100 packs in stock.

| Delivery | Priority | Deadline | Packs | Weight |
|---|---|---|---|---|
| D1 | Urgent | today 12:00 | 60 | 600 kg |
| D2 | Normal | today 10:00 | 30 | 300 kg |
| D3 | Normal | today 16:00 | 30 | 300 kg |

Sorted: **D1** (urgent), **D2** (normal, earlier), **D3**.

1. **D1**: stock 100 ≥ 60 ✓ → truck **A** (most efficient) has room ✓ → A: 600 kg, stock left 40.
2. **D2**: stock 40 ≥ 30 ✓ → A has 400 kg free ✓ → A: 900 kg, stock left 10.
3. **D3**: stock 10 < 30 ✗ → **Unassigned, InsufficientStock**: "Needs 30 units…, but only 10 remain."

Truck B gets nothing, so it gets **no route**. Truck A's two stops are then ordered by Nearest Neighbor + 2-opt.

## 5. Design properties (and why they matter)

| Property | How | Why |
|---|---|---|
| **Pure** | no database, no HTTP, no clock, no `Random`; the input is never modified | testable with plain objects, in milliseconds |
| **Deterministic** | total sort order, stable tie-breaking (lowest index / id) | same input → same plan; bugs are reproducible; users trust it |
| **Explainable** | every unassigned delivery has a reason and a sentence | dispatchers must understand *why*, not just *what* |
| **Replaceable parts** | distance, routing and assignment are separate | road distances or a better algorithm can be swapped in later |

## 6. Known limitations (honest list)

| Limitation | Impact | Possible improvement |
|---|---|---|
| Straight-line distance | real roads are about 20–40% longer | road-distance service (OSRM, Google Distance Matrix) |
| No time windows | no guarantee to arrive *before* the deadline | VRPTW heuristics (insertion with time windows) |
| Fuel ignores load and traffic | estimates are approximate | regression / ML model on historical trips |
| One depot per run | multi-warehouse needs one run per warehouse | multi-depot VRP |
| Greedy assignment, no backtracking | packing can be sub-optimal | Clarke-Wright savings, local search between trucks, or an ILP solver |
| No partial deliveries | a delivery is all-or-nothing | allow splitting with a minimum quantity |

## 7. Why not Machine Learning here?
The rules are **known and exact**: capacity limits, stock counts, distances. That is a job for
**mathematical optimization**, which gives answers that respect the constraints and can be explained.
ML predicts **uncertain quantities from historical data**: *how long will this delivery take?*,
*how much fuel will this trip really use?*, *will it be late?* Those predictions could later become
**inputs** to this engine (for example, a better fuel estimate), but ML should not replace it.
