
namespace FleetOpt.Modules.Fleet.Domain;

internal sealed class DuplicatePlateNumberException(string plateNumber)
    : Exception($"A truck with plate number '{plateNumber}' already exists.")
{
    public string PlateNumber { get; } = plateNumber;
}
