using System.ComponentModel.DataAnnotations;

namespace SmartEducation.Entities
{
    public class Kid
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        // Foreign key to the parent User
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
