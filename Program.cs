using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OneC.Codex.Mcp.Configuration;
using OneC.Codex.Mcp.OData;

var builder = Host.CreateApplicationBuilder(args);

// STDOUT занят протоколом MCP.
// Логи должны идти в STDERR.
builder.Logging.ClearProviders();

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddOptions<OneCOptions>()
    .Bind(builder.Configuration.GetSection(
        OneCOptions.SectionName))
    .Validate(
        options => Uri.TryCreate(
            options.BaseUrl,
            UriKind.Absolute,
            out _),
        "OneC:BaseUrl должен быть абсолютным URL.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.UserName),
        "Не указан пользователь 1С.")
    .Validate(
        options => options.TimeoutSeconds is >= 1 and <= 300,
        "Недопустимый тайм-аут.")
    .Validate(
        options => options.MaximumRows is >= 1 and <= 500,
        "Недопустимое максимальное количество строк.")
    .ValidateOnStart();

builder.Services.AddHttpClient<OneCODataClient>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();