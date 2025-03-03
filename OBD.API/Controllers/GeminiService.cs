using Newtonsoft.Json.Linq;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GeminiApiKey"];
    }

    public async Task<string> GenerateMaintenanceSchedule(string year, string make, string model, string mileage)
    {
        var prompt = $@"
        Generate a detailed maintenance schedule for a {year} {make} {model} 
        with {mileage} miles. Only include the **Required Maintenance** for the given mileage, and format the response like this:

        For each item, only include it if it's required at this mileage. If not required, skip it.

        List the maintenance items like this:
        __________
        Oil Change: Required
        Tire Rotation: Required
        Brake Inspection: Required
        Air Filter Replacement: Required
        Battery Inspection: Required
        Fluid Checks (Coolant, Brake, Transmission): Required
        Timing Belt Replacement: Required
        Spark Plug Replacement: Required
        Suspension Check: Required
        Wheel Alignment: Required
        Engine Tune-Up: Required
        Transmission Service: Required
        Cabin Air Filter Replacement: Required
        Fuel System Cleaning: Required
        Clutch Inspection: Required
        Exhaust System Inspection: Required
        __________

        Ensure the list is formatted clearly and only contains the required maintenance items for the provided mileage. 
        Do not include anything that is not required.
    ";

        var requestBody = new
        {
            contents = new[]
            {
            new
            {
                parts = new[]
                { new { text = prompt }}
            }
        }
        };
        var content = JsonContent.Create(requestBody);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}")
        {
            Content = content
        };


        var response = await _httpClient.SendAsync(request);

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();

        var jsonResponse = JObject.Parse(responseContent);
        var result = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString().Trim();

        if (string.IsNullOrEmpty(result))
        {
            return null;
        }

        
        var filteredResult = string.Join("\n", result
            .Split('\n')
            .SkipWhile(line => !line.StartsWith("__________")) 
            .Skip(1) 
            .TakeWhile(line => !line.StartsWith("__________")) 
            .ToArray());

        return filteredResult;
    }

}
