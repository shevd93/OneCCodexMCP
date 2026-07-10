using System.ComponentModel;
using ModelContextProtocol.Server;
using OneC.Codex.Mcp.OData;

namespace OneC.Codex.Mcp.Tools;

[McpServerToolType]
public sealed class ConnectionTools
{
    private readonly OneCODataClient _client;

    public ConnectionTools(OneCODataClient client)
    {
        _client = client;
    }

    [McpServerTool(
        Name = "onec_test_connection"),
     Description(
        "Проверяет доступность стандартного OData-интерфейса 1С.")]
    public async Task<string> TestConnection(
        CancellationToken cancellationToken = default)
    {
        var metadata = await _client.GetTextAsync(
            "$metadata",
            cancellationToken);

        return metadata.Contains("EntityType", StringComparison.Ordinal)
            ? "Подключение к OData 1С успешно."
            : "Сервис ответил, но метаданные OData не распознаны.";
    }
}