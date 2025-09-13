using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using SmartEducation.Entities;

namespace SmartEducation.Services
{
    public class ChatGptService
    {
        // Internal class to match the OpenAI API response structure
        internal class OpenAiResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; }
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

        const string NgssExtractorSystemPrompt = """
            You are an information-extraction engine for NGSS-aligned activity planning.
            If user input multiple topics or multiple target_ages or multiple top_age or multiple lengh_of_activities, use the last mentioned in the user prompt only and ignore the rest.

            Task:
            Given a single user request (free-form text), decide whether there is enough information to define ALL NINE attributes:
            1) target_age : string. Accept only explicit numeric ages or explicit numeric ranges written by the user, normalized to use “–” for ranges (e.g., "4", "4–6"). If the user provides grades (e.g., "Grade 2", "Middle School") without explicit numeric ages, treat target_age as missing.
            2) topics : lowercase string for content of activity required by the user. For Examples: "gravity using household items", "rocket", "wind using household items.", "energy"... Do not infer if not clearly stated.
            3) lengh_of_activities : string in minutes or minute ranges only (For Examples "15 minutes", "20–30 minutes", "60–90 minutes"). If the user provides only qualitative timing, treat as missing and using default of 60 minutes. Do not infer if not clearly stated. If missing, use default of "60 minutes".
            4) field : string for generic field follow NGSS Standard such as "physical science", "life science", "earth and space science", "engineering", "technology", "math".... If the user does not explicitly specify one of these fields, find the most relevant field based on the topics provided. If unclear, treat as needed.
            5) top_age : string. The numeric maximum age boundary derived directly from the user’s explicit numeric age range. If a single numeric age is given, top_age equals that same age. Never infer top_age from grades or typical ages. Only use numbers explicitly present in the request.
            6) number_of_activities : integer according to the target_age and top age following NGSS Standard. If target_age and top_age are missing request user else use default of 1 (never request user for this attribute under any other circumstance).
            7) start_age_integer : integer. The numeric corresponding to the starting age in target_age. If target_age is a single age, start_age_integer equals that same age. If target_age is numeric range, start_age_integer equals the minimum age in that range. If target_age is not numeric or a range of numbers, parse the starting age from the string and return as integer. If unable to parse, treat as missing but never request user for this attribute under any circumstance.
            8) pass : boolean. true only if all nine attributes are present and valid per these rules. Otherwise false.
            9) isGrade :boolean. true if target_age is a grade or level (e.g., "Grade 2", "Middle School", "High School"). Otherwise false.

            Important:
            • Consider NGSS suitability when interpreting the user’s intent, BUT DO NOT output any NGSS mapping.
            • Output MUST be valid JSON. No comments, explanations, markdown, or extra keys.
            • Never invent values. If unsure about any attribute, use the "needs" form.
            • Normalize all age ranges to use an en dash “–” with no spaces (For Examples "4–6").
            • For topics, return topics corresponding to user prompt lowercase string. If unclear, treat it as needed.
            • For lengh_of_activities, return minutes or minute ranges only, if user not specifies in minutes, using 60 minutes as default.
            • For target_age,top_age if user specifies a single age, return that age as both target_age and top_age, if a range is specified, return the maximum age as top_age.
            • For topics, take the entire relevant phrase from the user prompt that describes the topic or content they want to teach about.
            • For field, if the user does not explicitly specify the fields, find the most relevant field based on the topics provided follow NGSS Standard. if unclear find the most relevant field based on the user prompt.

            Patterns to recognize for ages:
            • "X–Y years old", "X - Y years old", "X to Y years old", "from X to Y", "between X and Y", "ages X through Y"
            • Single age: "X", "X years old"
            • Always extract ages as integers from the user text. if user input grade instead of ages using grade as string output as normal.
            • If user input non-numeric age or grade, isGrade is true.
            • "Grade X", "Grades X-Y", "Kindergarten", "Middle School", "High School" return "Grade X", "Grades X-Y", "Kindergarten", "Middle School", "High School" as string for target_age
            • top_age is the top grade corresponding to the level education for example
            If user input "Grade 7" return target_age is "Grade 7" and top_age is "8th Grade"
            If user input "Grade 3" return target_age is "Grade 3" and top_age is "5th Grade" 
            If user input "Middle School" return target_age is "Grade 6" and top_age is "8th Grade"
            If user input "High School" return target_age is "Grade 10" and top_age is "12th Grade"
            If user input "Grade 6 and 7" return target_age is "Grade 6–7" and top_age is "8th Grade"
            If user input "Grade 2 and above" return target_age is "Grade 2" and top_age is "5th Grade"
            If user input "Grade 6 and above" return target_age is "Grade 6" and top_age is "8th Grade"
            If user input "Grade 4 to 6" return target_age is "Grade 4–6" and top_age is "6th Grade"
            If user input "Grade 4 through 6" return target_age is "Grade 4–6" and top_age is "6th Grade"
            If user input "Grade 4-6" return target_age is "Grade 4–6" and top_age is "6th Grade"
            If user input "Grade 8 and 9" return target_age is "Grade 8–9" and top_age is "10th Grade"
            If user input "Grade 8 and below" return target_age is "Grade 5" and top_age is "8th Grade"
            If user input "Kindergarten" return target_age is "Kindergarten" and top_age is "1st Grade"
            If user input "Grade X to Grade Y" return target_age is "Grade X–Y" and top_age is "Grade Y"
            If user input "grade X to Y" return target_age is "Grade X–Y" and top_age is "Grade Y"

            Rules for number_of_activities and start_age_integer:
            • If target_age and top_age are present, use the following rules:
            - If target_age is a single numeric age, number_of_activities = 1 and start_age_integer = that age (integer).
            - If target_age is a numeric range X–Y, number_of_activities = Y - X + 1 and start_age_integer = X (integer).
            - If target_age is a grade or non-numeric, number_of_activities = 1 and start_age_integer = the starting age corresponding to that grade or level (integer).
            - If target_age is a non-numeric range (e.g., "Grades X-Y", "Grades X to Y",...), number_of_activities = Y - X + 1 and start_age_integer = the starting age corresponding to grade X (integer).
            

            If lengh_of_activities is missing, use default of "60 minutes".
            If field is missing from the user prompt, infer the most relevant field based on the topics provided.

            If ALL NINE attributes can be inferred and comply, output EXACTLY:
            {"target_age":"...","topics":"...","lengh_of_activities":"...","field":"..." ,"top_age":"...", "number_of_activities":int,"start_age_integer":int,"pass":true, "isGrade":boolean}

            Otherwise if target_age, topics is missing output EXACTLY:
            {"needs":["target_age","topics"],"reasons":"Send user friendly message to user to explain why you cannot extract the information from the user prompt or why the user prompt is insufficient and how can they input additional informaion to fill the missing attribute"}
        """;

        private readonly HttpClient _http;

        public ChatGptService()
        {
            _http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("ERROR: The 'OPENAI_API_KEY' environment variable is not set.");

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<string> SendRequest(string userPrompt, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userPrompt))
                throw new ArgumentException("Prompt cannot be empty.", nameof(userPrompt));

            var url = "https://api.openai.com/v1/chat/completions";
            var model = Environment.GetEnvironmentVariable("OPEN_AI_MODEL");
            if (string.IsNullOrEmpty(model))
                throw new InvalidOperationException("ERROR: The 'OPEN_AI_MODEL' environment variable is not set.");

            var body = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = NgssExtractorSystemPrompt },
                    new { role = "user", content = userPrompt }
                },
                response_format = new { type = "json_object" },
                temperature = 0.1
            };

            var jsonContent = JsonSerializer.Serialize(body);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(url, httpContent, ct);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseBody);

            return openAiResponse?.Choices?[0]?.Message?.Content;
        }
    }
}