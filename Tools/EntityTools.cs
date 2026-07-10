using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OneC.Codex.Mcp.OData;

namespace OneC.Codex.Mcp.Tools;

[McpServerToolType]
public sealed class EntityTools
{
    private readonly OneCODataClient _client;

    public EntityTools(OneCODataClient client)
    {
        _client = client;
    }

    [McpServerTool(
        Name = "onec_find_counterparties"),
     Description(
        "Ищет контрагентов в 1С по части наименования. " +
        "Инструмент выполняет только чтение.")]
    public async Task<string> FindCounterparties(
        [Description("Часть наименования контрагента.")]
        string search,

        [Description("Количество результатов от 1 до 20.")]
        int limit = 10,

        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            throw new ArgumentException(
                "Необходимо указать строку поиска.",
                nameof(search));
        }

        limit = Math.Clamp(limit, 1, 20);

        var url = ODataUrlBuilder.BuildCatalogSearch(
            entity: "Catalog_Контрагенты",
            descriptionField: "Description",
            search: search.Trim(),
            selectFields:
            [
                "Ref_Key",
                "Code",
                "Description",
                "DeletionMark"
            ],
            top: limit);

        var response = await _client.GetJsonAsync(
            url,
            cancellationToken);

        var rows = OneCODataResponseParser.ExtractRows(response);

        return JsonSerializer.Serialize(
            rows,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }
}