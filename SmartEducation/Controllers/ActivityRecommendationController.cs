using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartEducation.dbContext;
using SmartEducation.Entities;
using SmartEducation.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Metrics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartEducation.Controllers
{
    [Authorize(Roles = "Admin, User, OrganizationAdmin")]
    public class ActivityRecommendationController : Controller
    {
        private readonly ActivityRecommendationService _activityRecommendationService;
        private readonly SmartEduDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ChatGptService _chatGptService;
        private int numberOfActivities = 1;

        public ActivityRecommendationController(ActivityRecommendationService activityRecommendationService, SmartEduDbContext context, UserManager<User> userManager, ChatGptService chatGptService)
        {
            _activityRecommendationService = activityRecommendationService;
            _context = context;
            _chatGptService = chatGptService;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetRecommendation(string userPrompt)
        {
            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                ModelState.AddModelError("", "Prompt cannot be empty. Please enter a request.");
                return View("Index");
            }

            GptParseResponse userPromptParse = null;
            if (TempData["SuccessfulParse"] is string parsedJson)
            {
                userPromptParse = JsonSerializer.Deserialize<GptParseResponse>(parsedJson);
            }

            if (userPromptParse == null)
            {
                ModelState.AddModelError("", "A validation error occurred. Please use the chat interface to build your request.");
                return View("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Redirect to login if user is not found
                return RedirectToAction("LogIn", "Account");
            }

            List<ActivityRecommendation> recommendations = null;
            List <ActivityRecommendation> recommendations_sub_list = new List<ActivityRecommendation>();
            List<string> activities_avoid = new List<string>();

            if (userPromptParse != null) {
                numberOfActivities = userPromptParse.NumberOfActivities > 0 ? userPromptParse.NumberOfActivities : 1;
            }
            if (numberOfActivities == 1)
            {
                string userPrompt_rewrite = $"Give me an activity for teaching lesson about {userPromptParse.Field} specific for {userPromptParse.Topics}, for age {userPromptParse.StartAgeInteger}, that lasts {userPromptParse.LengthOfActivities}, and return in JSON format as an ActivityRecommendation object";
                if(userPromptParse.isGrade)
                {
                    userPrompt_rewrite = $"Give me an activity for teaching lesson about {userPromptParse.Field} specific for {userPromptParse.Topics}, for grade {userPromptParse.StartAgeInteger}, that lasts {userPromptParse.LengthOfActivities}, and return in JSON format as an ActivityRecommendation object";
                }

                const int maxRetries = 10;

                // Loop to retry the API call on failure
                for (int attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        // Call the service to get recommendations.
                        recommendations_sub_list = await _activityRecommendationService.GetActivityRecommendationsAsync(userPrompt, user, userPrompt);

                        // If the result is valid (not null and not empty), exit the loop.
                        if (recommendations_sub_list != null && recommendations_sub_list.Any())
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        if (attempt == maxRetries)
                        {
                            recommendations_sub_list = null;
                        }
                    }
                }

                if (recommendations_sub_list != null && recommendations_sub_list.Any())
                {
                    if (recommendations == null)
                    {
                        recommendations = new List<ActivityRecommendation>();
                    }
                    recommendations.AddRange(recommendations_sub_list);
                }
            }
            else
            {
                for (int counter = 0; counter < numberOfActivities; counter++)
                {
                    //userPrompt += $"\n\nActivity {counter + 1} of {userPromptParse.NumberOfActivities}:";
                    string userPrompt_rewrite = $"Give me an activity for teaching lesson about {userPromptParse.Field} specific for {userPromptParse.Topics}, for age {userPromptParse.StartAgeInteger + counter}, that lasts {userPromptParse.LengthOfActivities}, and return in JSON format as an ActivityRecommendation object.";
                    if (userPromptParse.isGrade)
                    {
                        userPrompt_rewrite = $"Give me an activity for teaching lesson about {userPromptParse.Field} specific for {userPromptParse.Topics}, for grade {userPromptParse.StartAgeInteger + counter}, that lasts {userPromptParse.LengthOfActivities}, and return in JSON format as an ActivityRecommendation object.";
                    }

                    activities_avoid = recommendations != null ? recommendations.Select(r => r.NameOfActivity).ToList() : new List<string>();
                    // Collect the last EducationStandards from recommendations if available
                    List<string> last_education_standards = recommendations != null && recommendations.Any() ?
                        recommendations.Last().EducationStandards : new List<string>();

                    if (activities_avoid.Any())
                    {
                        string avoid_activities_text = activities_avoid.Any() ? $" Also, avoid these activities: {string.Join(", ", activities_avoid)}." : "";
                        userPrompt_rewrite += avoid_activities_text;
                    }

                    if (last_education_standards.Any())
                    {
                        string last_education_standards_text = last_education_standards.Any() ? $" The activity must related, more advanced concept and build upon the principles of these education standards: {string.Join(", ", last_education_standards)}." : "";
                        userPrompt_rewrite += last_education_standards_text;
                    }

                    const int maxRetries = 10;

                    // Loop to retry the API call on failure
                    for (int attempt = 1; attempt <= maxRetries; attempt++)
                    {
                        try
                        {
                            // Call the service to get recommendations.
                            recommendations_sub_list = await _activityRecommendationService.GetActivityRecommendationsAsync(userPrompt_rewrite, user, userPrompt);

                            // If the result is valid (not null and not empty), exit the loop.
                            if (recommendations_sub_list != null && recommendations_sub_list.Any())
                            {
                                break;
                            }
                        }
                        catch (Exception)
                        {
                            if (attempt == maxRetries)
                            {
                                recommendations_sub_list = null;
                            }
                        }
                    }

                    if (recommendations_sub_list != null && recommendations_sub_list.Any())
                    {
                        if (recommendations == null)
                        {
                            recommendations = new List<ActivityRecommendation>();
                        }
                        recommendations.AddRange(recommendations_sub_list);
                    }
                }
            }

            // After all retries, check if recommendations are still missing.
            if (recommendations == null || !recommendations.Any())
            {
                // If all attempts failed, add the specified error message to the model state.
                ModelState.AddModelError("", "Please connect with the developer");
                return View("Index");
            }

            // On success, save the recommendations to the database.
            _context.ActivityRecommendations.AddRange(recommendations);
            await _context.SaveChangesAsync();

            // Pass the list of recommendations to the result view.
            return View("RecommendationResult", recommendations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAsyncChat([FromBody] SendRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new SendResponseDto
                {
                    Messages = new List<ChatMessageDto>
                    {
                        new ChatMessageDto { Role = "server", Text = "Invalid input." }
                    }
                });
            }

            var userMessage = new ChatMessageDto
            {
                Role = "user",
                Text = dto.Prompt.Trim(),
                Timestamp = DateTime.UtcNow
            };

            var gptJsonResponse = await _chatGptService.SendRequest(dto.Prompt);
            var serverText = "Sorry, I couldn't process your request. Please try again.";
            bool isComplete = false;

            try
            {
                var parsedResponse = JsonSerializer.Deserialize<GptParseResponse>(gptJsonResponse);

                if (parsedResponse != null)
                {
                    if (parsedResponse.Pass)
                    {
                        isComplete = true;
                        serverText = $"Great! I have everything I need. I am generating your recommendation now...\n" +
                                     $"- Subject: {parsedResponse.Topics}\n" +
                                     $"- Target Age: {parsedResponse.TargetAge}\n" +
                                     $"- Activity Length: {parsedResponse.LengthOfActivities}";

                        TempData["SuccessfulParse"] = JsonSerializer.Serialize(parsedResponse);
                    }
                    else if (!string.IsNullOrEmpty(parsedResponse.Reasons))
                    {
                        serverText = parsedResponse.Reasons;
                    }
                    else if (parsedResponse.Needs?.Any() == true)
                    {
                        serverText = $"I'm missing some information. Could you please provide details on the following: {string.Join(", ", parsedResponse.Needs)}?";
                    }
                    else
                    {
                        serverText = "I'm having trouble understanding your request. Could you please rephrase it with more details about the lesson plan you want?";
                    }
                }
            }
            catch (JsonException)
            {
                serverText = "I received an unexpected response. Could you please try rephrasing your request?";
            }

            var serverMessage = new ChatMessageDto
            {
                Role = "server",
                Text = serverText,
                Timestamp = DateTime.UtcNow
            };

            return Json(new SendResponseDto
            {
                Messages = new List<ChatMessageDto> { userMessage, serverMessage },
                IsComplete = isComplete
            });
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Retrieve all recommendations for the current user from the database.
            var userRecommendations = await _context.ActivityRecommendations
                                            .Where(r => r.UserId == user.Id)
                                            .OrderByDescending(r => r.CreatedAt)
                                            .ToListAsync();

            return View(userRecommendations);
        }

        [HttpGet]
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            // Find the specific recommendation by its ID.
            var recommendation = await _context.ActivityRecommendations
                .FirstOrDefaultAsync(m => m.Id == id);

            if (recommendation == null)
            {
                return NotFound();
            }

            // Security check: ensure the user owns the recommendation unless they are an Admin.
            if (recommendation.UserId != user.Id && !User.IsInRole("Admin"))
            {
                // Return a 403 Forbidden error.
                return Forbid(); 
            }

            return View(recommendation);
        }
        public class GptParseResponse
        {
            [JsonPropertyName("target_age")]
            public string TargetAge { get; set; }

            [JsonPropertyName("topics")]
            public string Topics { get; set; }

            [JsonPropertyName("lengh_of_activities")]
            public string LengthOfActivities { get; set; }

            [JsonPropertyName("field")]
            public string Field { get; set; }

            [JsonPropertyName("top_age")]
            public string TopAge { get; set; }

            [JsonPropertyName("number_of_activities")]
            public int NumberOfActivities { get; set; }

            [JsonPropertyName("start_age_integer")]
            public int StartAgeInteger { get; set; }

            [JsonPropertyName("pass")]
            public bool Pass { get; set; }

            [JsonPropertyName("isGrade")]
            public bool isGrade { get; set; }

            [JsonPropertyName("needs")]
            public List<string> Needs { get; set; }

            [JsonPropertyName("reasons")]
            public string Reasons { get; set; }
        }

        public class SendRequestDto
        {
            [Required, MinLength(1), MaxLength(4000)]
            public string Prompt { get; set; } = string.Empty;
        }

        public class SendResponseDto
        {
            public List<ChatMessageDto> Messages { get; set; } = new();
            public bool IsComplete { get; set; } = false;
        }

        public class ChatMessageDto
        {
            [Required] public string Role { get; set; } = "user";
            [Required] public string Text { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }

    }
}
