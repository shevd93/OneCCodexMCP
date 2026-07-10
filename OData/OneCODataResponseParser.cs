using System.Text.Json;

namespace OneC.Codex.Mcp.OData;

public static class OneCODataResponseParser
{
    public static JsonElement ExtractRows(JsonElement root)
    {
        // Некоторые варианты JSON-представления OData.
        if (root.TryGetProperty("value", out var value) &&
            value.ValueKind == JsonValueKind.Array)
        {
            return value.Clone();
        }

        if (root.TryGetProperty("d", out var d))
        {
            if (d.ValueKind == JsonValueKind.Array)
            {
                return d.Clone();
            }

            if (d.ValueKind == JsonValueKind.Object &&
                d.TryGetProperty("results", out var results) &&
                results.ValueKind == JsonValueKind.Array)
            {
                return results.Clone();
            }
        }

        if (root.ValueKind == JsonValueKind.Array)
        {
            return root.Clone();
        }

        throw new InvalidOperationException(
            "Не удалось определить массив записей в ответе OData 1С.");
    }
}