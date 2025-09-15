namespace DnDAgency.Domain.Entities
{
    public class CampaignTag
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; }

        public Guid CampaignId { get; private set; }
        public Campaign Campaign { get; private set; }

        private CampaignTag() { } // EF Core

        public CampaignTag(string name, Guid campaignId)
        {
            ValidateName(name);

            Name = name;
            CampaignId = campaignId;
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tag name cannot be empty");
            if (name.Length > 50)
                throw new ArgumentException("Tag name cannot exceed 50 characters");
        }
    }
}
