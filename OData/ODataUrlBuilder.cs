namespace OneC.Codex.Mcp.OData;

public static class ODataUrlBuilder
{
    public static string EscapeStringLiteral(string value)
    {
        return value.Replace("'", "''");
    }

    public static string BuildCatalogSearch(
        string entity,
        string descriptionField,
        string search,
        IReadOnlyCollection<string> selectFields,
        int top)
    {
        var safeSearch = EscapeStringLiteral(search);

        var parameters = new[]
        {
            "$format=json",
            $"$top={top}",
            $"$select={string.Join(",", selectFields)}",
            $"$filter=substringof('{safeSearch}',{descriptionField}) eq true"
        };

        return entity + "?" + string.Join("&", parameters);
    }
}