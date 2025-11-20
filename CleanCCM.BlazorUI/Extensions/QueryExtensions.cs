namespace CleanCCM.BlazorUI.Extensions;


    public static class QueryBuilder
    {
        public static string Build(string baseUrl, Dictionary<string, object?>? query)
        {
            if (query == null || query.Count == 0)
                return baseUrl;

            var q = string.Join("&",
                query
                    .Where(x => x.Value != null)
                    .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!.ToString()!)}"));

            return $"{baseUrl}?{q}";
        }
    }

