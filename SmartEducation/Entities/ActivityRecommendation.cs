using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartEducation.Entities
{
    public class ActivityRecommendation
    {
        [JsonIgnore]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        // --- Request Information ---
        [JsonIgnore]
        public string UserPrompt { get; set; }

        [JsonIgnore]
        public DateTime DateTimeRequest { get; set; } = DateTime.UtcNow;

        // --- Core Content (mirrors the DTO) ---
        [JsonPropertyName("Summary")]
        public string Summary { get; set; }

        [JsonPropertyName("Engineer_Connection")]
        public string EngineerConnection { get; set; }

        [JsonPropertyName("Learning_Objectives")]
        public List<string> LearningObjectives { get; set; }

        [JsonPropertyName("Education_Standards")]
        public List<string> EducationStandards { get; set; }

        [JsonPropertyName("Material_Lists")]
        public List<string> MaterialLists { get; set; }

        [JsonPropertyName("Worksheets_and_Attachments")]
        public List<string> WorksheetsAndAttachments { get; set; }

        [JsonPropertyName("Introduction_Motivation")]
        public string IntroductionMotivation { get; set; }

        [JsonPropertyName("Procedure")]
        public List<string> Procedure { get; set; }

        [JsonPropertyName("Assessments")]
        public List<string> Assessments { get; set; }

        [JsonPropertyName("Safety_Issues")]
        public string SafetyIssues { get; set; }

        [JsonPropertyName("Troubleshooting_Tips")]
        public List<string> TroubleshootingTips { get; set; }

        [JsonPropertyName("Activity_Extensions")]
        public List<string> ActivityExtensions { get; set; }

        [JsonPropertyName("Activity_Scaling")]
        public List<string> ActivityScaling { get; set; }

        // --- Token usage ---
        [JsonIgnore]
        public int? PromptTokens { get; set; }

        [JsonIgnore]
        public int? CompletionTokens { get; set; }

        [JsonIgnore]
        public int? TotalTokens { get; set; }

        // --- Book-keeping / Timestamps ---
        [JsonIgnore]
        public DateTime CreatedAt { get; set; }

        [JsonIgnore]
        public DateTime UpdatedAt { get; set; }

        [JsonIgnore]
        public string UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; }
    }
}
