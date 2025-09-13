using Microsoft.AspNetCore.Identity;

namespace SmartEducation.Entities
{
    public class User : IdentityUser
    {
        public int? OrganizationId { get; set; }
        public virtual Organization? Organization { get; set; }

        public virtual ICollection<Kid>? Kids { get; set; }

        public virtual ICollection<ActivityRecommendation>? List_ActivityRecommendations { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public virtual DateTime DateUpdated { get; set; } = DateTime.UtcNow;

        public virtual DateTime DateDeleted { get; set; } = DateTime.MinValue;

        public virtual DateTime LastLogin { get; set; } = DateTime.MinValue;

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
                {
                    return $"{FirstName} {LastName}";
                }
                else if (!string.IsNullOrEmpty(FirstName))
                {
                    return FirstName;
                }
                else if (!string.IsNullOrEmpty(LastName))
                {
                    return LastName;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
