namespace DnDAgency.Domain.Exceptions;

public class PastSlotBookingException : DomainException
{
    public PastSlotBookingException() : base("Cannot book slot in the past") { }
}