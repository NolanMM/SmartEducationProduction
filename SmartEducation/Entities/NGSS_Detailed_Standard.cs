using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartEducation.Entities
{
    public class NGSS_Detailed_Standard
    {
        [JsonIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [JsonPropertyName("Title_NGSS_Standard")]
        public string Title_NGSS_Standard { get; set; }

        [JsonPropertyName("Matter_Interactions")]
        public string Matter_Interactions { get; set; }

        [JsonPropertyName("Science_Engineering_Practices")]
        public string Science_Engineering_Practices { get; set; }

        [JsonPropertyName("Disciplinary_Core_Ideas")]
        public string Disciplinary_Core_Ideas { get; set; }

        [JsonPropertyName("Crosscutting_Concepts")]
        public string Crosscutting_Concepts { get; set; }

        [JsonPropertyName("Connections_To_Other_DCI")]
        public string Connections_To_Other_DCI { get; set; }

        [JsonPropertyName("Common_Core_State_Standards_Connections")]
        public string Common_Core_State_Standards_Connections { get; set; }

        [JsonPropertyName("Articulation of DCIs across grade-levels")]
        public string Articulation_of_DCIs_across_grade_levels { get; set; }
    }
}
