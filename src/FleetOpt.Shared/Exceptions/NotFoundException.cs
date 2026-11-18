namespace FleetOpt.Shared.Exceptions;

/// <summary>A resource the command depends on does not exist (HTTP 404).</summary>
public abstract class NotFoundException(string message) : Exception(message);
