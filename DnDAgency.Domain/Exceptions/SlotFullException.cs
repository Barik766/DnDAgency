namespace DnDAgency.Domain.Exceptions;

public class SlotFullException : DomainException
{
    public SlotFullException() : base("Cannot book slot - maximum capacity reached") { }
}