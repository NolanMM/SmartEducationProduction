using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SmartEducation.ViewModels
{
    public class ConfigurationViewModel
    {
        [Display(Name = "Sender Email Address")]
        [EmailAddress]
        public string SENDER_EMAIL { get; set; }

        [Display(Name = "SMTP Password")]
        [DataType(DataType.Password)]
        public string SMTP_PASSWORD { get; set; }

        [Display(Name = "OpenAI API Key")]
        [DataType(DataType.Password)]
        public string OPENAI_API_KEY { get; set; }

        [Display(Name = "OpenAI Model")]
        public string OPEN_AI_MODEL { get; set; }

        public List<SelectListItem> AvailableModels { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "gpt-4o-mini", Text = "gpt-4o-mini" },
            new SelectListItem { Value = "gpt-5-mini", Text = "gpt-5-mini" },
            new SelectListItem { Value = "gpt-4o", Text = "gpt-4o" },
            new SelectListItem { Value = "gpt-5", Text = "gpt-5" }
        };
    }
}
