using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SmartEducation.Entities
{
    public class Grade_Standards
    {
        [JsonIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [JsonPropertyName("Title_Grade_Standard")]
        public string Title_Grade_Standard { get; set; }

        [JsonPropertyName("Description")]
        public string Description { get; set; }
    }
}
