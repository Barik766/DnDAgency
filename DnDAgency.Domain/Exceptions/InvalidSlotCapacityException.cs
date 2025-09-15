namespace DnDAgency.Domain.Exceptions;

public class InvalidSlotCapacityException : DomainException
{
    public InvalidSlotCapacityException(int maxPlayers)
        : base($"Slot capacity must be between 1 and 8, but was {maxPlayers}") { }
}