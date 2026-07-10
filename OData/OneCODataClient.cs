using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OneC.Codex.Mcp.Configuration;

namespace OneC.Codex.Mcp.OData;

public sealed class OneCODataClient
{
    private readonly HttpClient _httpClient;

    public OneCODataClient(
        HttpClient httpClient,
        IOptions<OneCOptions> options)
    {
        _httpClient = httpClient;

        var settings = options.Value;

        _httpClient.BaseAddress = new Uri(
            settings.BaseUrl.TrimEnd('/') + "/");

        _httpClient.Timeout =
            TimeSpan.FromSeconds(settings.TimeoutSeconds);

        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(
                $"{settings.UserName}:{settings.Password}"));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", credentials);

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<JsonElement> GetJsonAsync(
        string relativeUrl,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            relativeUrl,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Ошибка 1С OData. HTTP {(int)response.StatusCode}: " +
                Limit(body, 2_000));
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.Clone();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "1С вернула ответ, который не является корректным JSON.",
                exception);
        }
    }

    public async Task<string> GetTextAsync(
        string relativeUrl,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            relativeUrl,
            cancellationToken);

        var body = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Ошибка 1С OData. HTTP {(int)response.StatusCode}: " +
                Limit(body, 2_000));
        }

        return body;
    }

    private static string Limit(string value, int maximumLength)
    {
        return value.Length <= maximumLength
            ? value
            : value[..maximumLength];
    }
}