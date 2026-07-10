namespace OneC.Codex.Mcp.Configuration;

public sealed class OneCOptions
{
    public const string SectionName = "OneC";
    public required string BaseUrl { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
    public int MaximumRows { get; init; } = 50;
}