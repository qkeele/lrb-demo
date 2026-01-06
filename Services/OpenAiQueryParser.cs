using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LrbDemo.Services;

public sealed record QueryFilters(
    List<string> Introducers,
    List<string> Statuses,
    List<string> Committees,
    List<string> CommitteeReports,
    List<string> HrsSections,
    List<string> Keywords
);

public sealed record ParseResult(QueryFilters Filters, string RawJson);

public sealed class OpenAiQueryParser
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public OpenAiQueryParser(HttpClient http, IConfiguration config)
    {
        _http = http;
        _apiKey = config["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("Missing OpenAI:ApiKey");
    }

    public async Task<ParseResult> ParseAsync(string userQuery, CancellationToken ct = default)
    {
        var system = """
Return ONLY valid JSON.
Keys:
- introducers
- statuses
- committees
- committeeReports
- hrsSections
- keywords

Each value MUST be an array of strings.
Use empty arrays when nothing applies.
No extra keys. Keep keywords to a minimum.
""";

        var payload = new
        {
            model = "gpt-4o-mini",
            temperature = 0,
            messages = new object[]
            {
                new { role = "system", content = system },
                new { role = "user", content = userQuery }
            }
        };

        using var req = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/chat/completions"
        );

        req.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        req.Content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException(body);

        using var doc = JsonDocument.Parse(body);
        var rawJson =
            doc.RootElement
               .GetProperty("choices")[0]
               .GetProperty("message")
               .GetProperty("content")
               .GetString()
            ?? "{}";

        QueryFilters filters;

        try
        {
            filters = JsonSerializer.Deserialize<QueryFilters>(
                rawJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? Empty();
        }
        catch
        {
            filters = Empty();
        }

        return new ParseResult(filters, rawJson);
    }

    private static QueryFilters Empty() =>
        new([], [], [], [], [], []);
}