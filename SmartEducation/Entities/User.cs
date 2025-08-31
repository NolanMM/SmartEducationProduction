using Microsoft.AspNetCore.Identity;

namespace SmartEducation.Entities
{
    public class User : IdentityUser
    {
        public int? OrganizationId { get; set; }
        public virtual Organization? Organization { get; set; }

        public virtual ICollection<Kid>? Kids { get; set; }

        public virtual ICollection<ActivityRecommendation>? List_ActivityRecommendations { get; set; }
    }
}
