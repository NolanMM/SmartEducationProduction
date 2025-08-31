using System.ComponentModel.DataAnnotations;

namespace SmartEducation.ViewModels
{
    public class CreateKidViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }
    }
}
