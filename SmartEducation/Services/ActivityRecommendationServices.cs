using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using SmartEducation.Entities;

namespace SmartEducation.Services
{
    public class ActivityRecommendationService
    {
        internal class ListActivityRecommendation
        {
            [JsonPropertyName("ActivityRecommendation")]
            public List<ActivityRecommendation> List_Activities_Recommendation { get; set; }
        }

        internal class OpenAiResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; }

            [JsonPropertyName("usage")]
            public Usage Usage { get; set; }
        }

        internal class Choice
        {
            [JsonPropertyName("message")]
            public Message Message { get; set; }
        }

        internal class Message
        {
            [JsonPropertyName("content")]
            public string Content { get; set; }
        }

        internal class Usage
        {
            [JsonPropertyName("prompt_tokens")]
            public int PromptTokens { get; set; }

            [JsonPropertyName("completion_tokens")]
            public int CompletionTokens { get; set; }

            [JsonPropertyName("total_tokens")]
            public int TotalTokens { get; set; }
        }

        private readonly HttpClient _httpClient;
        private const string OpenAiEndpoint = "https://api.openai.com/v1/chat/completions";

        public ActivityRecommendationService()
        {
            _httpClient = new HttpClient();
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException(
                    "ERROR: The 'OPENAI_API_KEY' environment variable is not set."
                );
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<List<ActivityRecommendation>> GetActivityRecommendationsAsync(string userPrompt, User user_requested, string userOriginalPrompt)
        {
            //Console.WriteLine("\nGenerating activity recommendations...");

            var systemMessage = await File.ReadAllTextAsync("Prompt.txt");

            var requestPayload = new
            {
                model = Environment.GetEnvironmentVariable("OPEN_AI_MODEL"),//"gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = systemMessage },
                    new { role = "user", content = userPrompt }
                },
                response_format = new { type = "json_object" }
            };

            var jsonContent = JsonSerializer.Serialize(requestPayload);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(OpenAiEndpoint, httpContent);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            // Deserialize the top-level response to access message content and usage tokens
            var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseBody);
            var content = openAiResponse?.Choices?[0]?.Message?.Content;

            if (string.IsNullOrEmpty(content))
            {
                return new List<ActivityRecommendation>();
            }
            // Check if content contains "ActivityRecommendation" convert to Json
            List<ActivityRecommendation> return_results = new List<ActivityRecommendation>();

            if (content.Contains("ActivityRecommendation"))
            {
                var list_recommendations = JsonSerializer.Deserialize<ListActivityRecommendation>(content);
                if (list_recommendations != null && list_recommendations.List_Activities_Recommendation != null)
                {
                    foreach (var recommendation_item in list_recommendations.List_Activities_Recommendation)
                    {
                        // Attach request context
                        recommendation_item.UserPrompt = userOriginalPrompt;
                        // Stamp timestamps
                        var now_item = DateTime.UtcNow;
                        recommendation_item.CreatedAt = now_item;
                        recommendation_item.UpdatedAt = now_item;
                        // Attach usage tokens if present
                        var usage_item = openAiResponse?.Usage;
                        if (usage_item != null)
                        {
                            recommendation_item.PromptTokens = usage_item.PromptTokens;
                            recommendation_item.CompletionTokens = usage_item.CompletionTokens;
                            recommendation_item.TotalTokens = usage_item.TotalTokens;
                        }
                        recommendation_item.UserId = user_requested.Id;
                        recommendation_item.User = user_requested;
                        return_results.Add(recommendation_item);
                    }
                    //Console.WriteLine($"\nMultiple Recommendations: {return_results.Count}\n");
                }
                else
                {
                    return new List<ActivityRecommendation>();
                }
            }
            else
            {
                var recommendation = JsonSerializer.Deserialize<ActivityRecommendation>(content);

                recommendation.UserPrompt = userOriginalPrompt;

                // Stamp timestamps
                var now_single = DateTime.UtcNow;
                recommendation.CreatedAt = now_single;
                recommendation.UpdatedAt = now_single;

                // Attach usage tokens if present
                var usage_single = openAiResponse?.Usage;
                if (usage_single != null)
                {
                    recommendation.PromptTokens = usage_single.PromptTokens;
                    recommendation.CompletionTokens = usage_single.CompletionTokens;
                    recommendation.TotalTokens = usage_single.TotalTokens;
                }
                recommendation.UserId = user_requested.Id;
                recommendation.User = user_requested;
                return_results.Add(recommendation);
            }

            if (return_results == null)
            {
                return new List<ActivityRecommendation>();
            }
            return return_results;
        }
    }
}
