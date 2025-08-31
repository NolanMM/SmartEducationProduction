using System.ComponentModel.DataAnnotations;

namespace SmartEducation.Entities
{
    public class Organization
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        // Add other properties such as Address, Phone

        public virtual ICollection<User> Users { get; set; }
    }
}
