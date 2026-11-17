namespace FleetOpt.Modules.Inventory.Domain;

internal sealed class Product
{
    public Guid Id { get; private set; }
    public string Sku { get; private set; }
    public string Name { get; private set; }
    public decimal UnitWeightKg { get; private set; }
    public decimal UnitVolumeM3 { get; private set; }

    private Product(Guid id, string sku, string name, decimal unitWeightKg, decimal unitVolumeM3)
    {
        Id = id;
        Sku = sku;
        Name = name;
        UnitWeightKg = unitWeightKg;
        UnitVolumeM3 = unitVolumeM3;
    }

    public static Product Create(string? sku, string? name, decimal unitWeightKg, decimal unitVolumeM3)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(unitWeightKg);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(unitVolumeM3);

        return new Product(Guid.CreateVersion7(), sku.Trim().ToUpperInvariant(), name.Trim(), unitWeightKg, unitVolumeM3);
    }
}
