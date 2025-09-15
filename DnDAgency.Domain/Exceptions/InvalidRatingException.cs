namespace DnDAgency.Domain.Exceptions;

public class InvalidRatingException : DomainException
{
    public InvalidRatingException(int rating) : base($"Rating must be between 1 and 5, but was {rating}") { }
}