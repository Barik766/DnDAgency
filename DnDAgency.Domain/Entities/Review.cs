using DnDAgency.Domain.Exceptions;

namespace DnDAgency.Domain.Entities
{
    public class Review
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public Guid UserId { get; private set; }
        public User User { get; private set; }
        public Guid MasterId { get; private set; }
        public Master Master { get; private set; }
        public int Rating { get; private set; }
        public string Comment { get; private set; }
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public Review(Guid userId, Guid masterId, int rating, string comment)
        {
            ValidateRating(rating);
            ValidateComment(comment);

            UserId = userId;
            MasterId = masterId;
            Rating = rating;
            Comment = comment;
        }

        public void UpdateRating(int rating)
        {
            ValidateRating(rating);
            Rating = rating;
        }

        public void UpdateComment(string comment)
        {
            ValidateComment(comment);
            Comment = comment;
        }

        private static void ValidateRating(int rating)
        {
            if (rating < 1 || rating > 5)
                throw new InvalidRatingException(rating);
        }

        private static void ValidateComment(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Comment cannot be empty");
            if (comment.Length > 1000)
                throw new ArgumentException("Comment cannot exceed 1000 characters");
        }
    }
}
