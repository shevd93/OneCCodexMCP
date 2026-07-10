using System.ComponentModel;
using System.Text.Json;
using System.Xml.Linq;
using ModelContextProtocol.Server;
using OneC.Codex.Mcp.OData;

namespace OneC.Codex.Mcp.Tools;

[McpServerToolType]
public sealed class MetadataTools
{
    private readonly OneCODataClient _client;

    public MetadataTools(OneCODataClient client)
    {
        _client = client;
    }

    [McpServerTool(
        Name = "onec_find_metadata"),
     Description(
        "Ищет сущности и поля в метаданных стандартного OData-интерфейса 1С.")]
    public async Task<string> FindMetadata(
        [Description(
            "Часть имени сущности или поля, например Контрагенты, " +
            "Номенклатура или ЗаказКлиента.")]
        string search,

        [Description("Максимальное количество найденных сущностей.")]
        int limit = 20,

        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            throw new ArgumentException(
                "Необходимо передать строку поиска.",
                nameof(search));
        }

        limit = Math.Clamp(limit, 1, 50);

        var xml = await _client.GetTextAsync(
            "$metadata",
            cancellationToken);

        var document = XDocument.Parse(xml);

        var entities = document
            .Descendants()
            .Where(element =>
                element.Name.LocalName == "EntityType")
            .Select(entity =>
            {
                var entityName =
                    entity.Attribute("Name")?.Value ?? string.Empty;

                var properties = entity
                    .Elements()
                    .Where(element =>
                        element.Name.LocalName == "Property")
                    .Select(property => new
                    {
                        Name = property.Attribute("Name")?.Value,
                        Type = property.Attribute("Type")?.Value,
                        Nullable = property.Attribute("Nullable")?.Value
                    })
                    .Where(property =>
                        property.Name is not null)
                    .ToArray();

                return new
                {
                    Entity = entityName,
                    Properties = properties
                };
            })
            .Where(entity =>
                entity.Entity.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase) ||
                entity.Properties.Any(property =>
                    property.Name?.Contains(
                        search,
                        StringComparison.OrdinalIgnoreCase) == true))
            .Take(limit)
            .ToArray();

        return JsonSerializer.Serialize(
            entities,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });
    }
}