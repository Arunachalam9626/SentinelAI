using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SentinelAI.Models.ViewModels;

namespace SentinelAI.Services
{
    public class GeminiAnalysisResult
    {
        public string IncidentType { get; set; } = "Unknown";
        public string RiskLevel { get; set; } = "Low";
        public string MitigationSteps { get; set; } = "Report to your unit CERT. Do not click suspicious links. Change credentials immediately.";
    }

    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = (configuration["GeminiApiKey"] ?? string.Empty).Trim();
            Console.WriteLine($"[GeminiService] Constructor API Key: '{_apiKey}' (Length: {_apiKey.Length})");
        }

        public async Task<GeminiAnalysisResult> AnalyzeComplaintAsync(string description)
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY_HERE" || _apiKey == "PASTE_YOUR_KEY_HERE")
            {
                Console.WriteLine("[GeminiService] Gemini API key not configured. Using keyword fallback.");
                return KeywordFallback(description);
            }

            try
            {
                var prompt = "You are a defence cybersecurity analyst. Analyze the following cyber complaint and respond ONLY with a valid JSON object — no markdown, no explanation.\n\n" +
                    $"Complaint: {description}\n\n" +
                    "JSON format:\n" +
                    "{\n" +
                    "  \"IncidentType\": \"Phishing|Fraud|Malware|Espionage|Social Engineering|Unknown\",\n" +
                    "  \"RiskLevel\": \"Low|Medium|High|Critical\",\n" +
                    "  \"MitigationSteps\": \"Brief actionable steps as a single string\"\n" +
                    "}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[GeminiService] AnalyzeComplaintAsync failed. Status: {response.StatusCode}");
                    return KeywordFallback(description);
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);

                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? string.Empty;

                // Strip markdown code fences if present
                text = text.Trim();
                if (text.StartsWith("```"))
                {
                    var lines = text.Split('\n');
                    text = string.Join('\n', lines.Skip(1).TakeWhile(l => !l.StartsWith("```")));
                }

                var result = JsonSerializer.Deserialize<GeminiAnalysisResult>(text, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result == null)
                {
                    Console.WriteLine("[GeminiService] Failed to deserialize response. Using fallback.");
                    return KeywordFallback(description);
                }

                // Validate values
                var validTypes = new[] { "Phishing", "Fraud", "Malware", "Espionage", "Social Engineering", "Unknown" };
                var validLevels = new[] { "Low", "Medium", "High", "Critical" };

                if (!validTypes.Contains(result.IncidentType)) result.IncidentType = "Unknown";
                if (!validLevels.Contains(result.RiskLevel)) result.RiskLevel = "Low";
                if (string.IsNullOrWhiteSpace(result.MitigationSteps))
                    result.MitigationSteps = "Report to your unit CERT. Do not click suspicious links. Change credentials immediately.";

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeminiService] AnalyzeComplaintAsync exception: {ex.Message}");
                return KeywordFallback(description);
            }
        }

        public async Task<string> AskCyberAssistantAsync(List<ChatMessageViewModel> history, string userMessage)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                return "API key not configured. Please add GeminiApiKey to appsettings.json.";
            }

            var fallbackMessage = "I'm currently offline. For urgent cyber threats, call 1930 (National Cyber Crime Helpline) or visit cybercrime.gov.in. Your unit CERT is also available 24/7.";
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

            try
            {
                var systemInstruction = "You are SentinelAI Cyber Assistant, an expert cybersecurity advisor for Indian defence\n" +
                    "personnel, veterans, and their families. You ONLY answer questions related to:\n" +
                    "- Cybersecurity threats (phishing, malware, fraud, espionage, social engineering)\n" +
                    "- Safe digital practices for defence personnel\n" +
                    "- Steps to take after a cyber incident\n" +
                    "- Securing personal accounts (WhatsApp, email, UPI, Aadhaar)\n" +
                    "- Reporting channels (CERT-In, cybercrime.gov.in, unit CERT)\n\n" +
                    "If asked anything unrelated to cybersecurity or defence safety, politely refuse and\n" +
                    "redirect to cybersecurity topics. Keep answers concise (under 200 words), practical,\n" +
                    "and use numbered steps where applicable. Always end sensitive answers with:\n" +
                    "\"Report this immediately to your unit CERT or call 1930 (Cyber Crime Helpline).\"";

                var contentsList = new List<object>
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = systemInstruction }
                        }
                    },
                    new
                    {
                        role = "model",
                        parts = new[]
                        {
                            new { text = "Understood. I am SentinelAI Cyber Assistant, ready to help with cybersecurity guidance for defence personnel." }
                        }
                    }
                };

                if (history != null)
                {
                    foreach (var turn in history)
                    {
                        contentsList.Add(new
                        {
                            role = turn.Role,
                            parts = new[]
                            {
                                new { text = turn.Content }
                            }
                        });
                    }
                }

                contentsList.Add(new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = userMessage }
                    }
                });

                var requestBody = new
                {
                    contents = contentsList.ToArray()
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody);
                Console.WriteLine($"[GeminiService] Request Body: {jsonRequest}");
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);

                // --- DEBUGGING ---
                Console.WriteLine($"[GeminiService] API Key present: {!string.IsNullOrEmpty(_apiKey)}");
                Console.WriteLine($"[GeminiService] Default Headers: {string.Join(", ", _httpClient.DefaultRequestHeaders.Select(h => $"{h.Key}={string.Join(";", h.Value)}"))}");
                Console.WriteLine($"[GeminiService] Calling: {url}");
                Console.WriteLine($"[GeminiService] Response status: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[GeminiService] Error body: {error}");
                    return fallbackMessage;
                }
                // -----------------

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var text = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return text ?? fallbackMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeminiService] AskCyberAssistantAsync exception: {ex.Message}");
                return fallbackMessage;
            }
        }

        private static GeminiAnalysisResult KeywordFallback(string description)
        {
            var lower = description.ToLowerInvariant();
            const string defaultMitigation = "Report to your unit CERT. Do not click suspicious links. Change credentials immediately.";

            if (lower.Contains("otp") || lower.Contains("phish"))
                return new GeminiAnalysisResult { IncidentType = "Phishing", RiskLevel = "High", MitigationSteps = defaultMitigation };

            if (lower.Contains("bank") || lower.Contains("fraud"))
                return new GeminiAnalysisResult { IncidentType = "Fraud", RiskLevel = "Medium", MitigationSteps = defaultMitigation };

            if (lower.Contains("virus") || lower.Contains("malware"))
                return new GeminiAnalysisResult { IncidentType = "Malware", RiskLevel = "Critical", MitigationSteps = defaultMitigation };

            if (lower.Contains("army") || lower.Contains("military") || lower.Contains("posting") || lower.Contains("espionage"))
                return new GeminiAnalysisResult { IncidentType = "Espionage", RiskLevel = "Critical", MitigationSteps = defaultMitigation };

            return new GeminiAnalysisResult { IncidentType = "Unknown", RiskLevel = "Low", MitigationSteps = defaultMitigation };
        }
    }
}
