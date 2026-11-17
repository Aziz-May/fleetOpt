using FleetOpt.Shared.Exceptions;
namespace FleetOpt.Modules.Fleet.Domain;

internal sealed class DuplicatePlateNumberException(string plateNumber)
    : ConflictException($"A truck with plate number '{plateNumber}' already exists.")
{
    public string PlateNumber { get; } = plateNumber;
}
