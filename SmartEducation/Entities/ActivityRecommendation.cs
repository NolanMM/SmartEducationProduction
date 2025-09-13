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
        public string UserPrompt { get; set; } = new string("");

        [JsonIgnore]
        public DateTime DateTimeRequest { get; set; } = DateTime.UtcNow;

        // --- Core Content (mirrors the DTO) ---
        [JsonPropertyName("Name_of_Activity")]
        public string? NameOfActivity { get; set; } = new string("");

        [JsonPropertyName("Summary")]
        public string Summary { get; set; } = new string("");

        [JsonPropertyName("Engineer_Connection")]
        public string EngineerConnection { get; set; } = new string("");

        [JsonPropertyName("Learning_Objectives")]
        public List<string> LearningObjectives { get; set; } = new List<string>();

        [JsonPropertyName("Education_Standards")]
        public List<string> EducationStandards { get; set; } = new List<string>();

        [JsonPropertyName("Material_Lists")]
        public List<string> MaterialLists { get; set; } = new List<string>();

        [JsonPropertyName("Worksheets_and_Attachments")]
        public List<string> WorksheetsAndAttachments { get; set; } = new List<string>();

        [JsonPropertyName("Introduction_Motivation")]
        public string IntroductionMotivation { get; set; } = new string("");

        [JsonPropertyName("Procedure")]
        public List<string> Procedure { get; set; } = new List<string>();

        [JsonPropertyName("Assessments")]
        public List<string> Assessments { get; set; } = new List<string>();

        [JsonPropertyName("Safety_Issues")]
        public string SafetyIssues { get; set; } = new string("");

        [JsonPropertyName("Troubleshooting_Tips")]
        public List<string> TroubleshootingTips { get; set; } = new List<string>();

        [JsonPropertyName("Activity_Extensions")]
        public List<string> ActivityExtensions { get; set; } = new List<string>();

        [JsonPropertyName("Activity_Scaling")]
        public List<string> ActivityScaling { get; set; } = new List<string>();

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
