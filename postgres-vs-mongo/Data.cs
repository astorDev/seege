using System.Text.Json;

public record Expense(
    string Id,
    int Amount,
    Dictionary<string, string> Labels
);

public static class TestData
{
    static readonly Expense[] raw = GetRaw();

    public static Expense[] GetRaw() {
        var dataJson = File.ReadAllText("data.json");
        return JsonSerializer.Deserialize<Expense[]>(dataJson, new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true
        })!;
    }

    public static Expense[] ForIteration(int iteration, int? shift = 0, int? take = null) {
        var shifted = raw.Select(d => d with { Id = d.Id + iteration + shift });
        var result = take.HasValue ? shifted.Take(take.Value) : shifted;
        return result.ToArray();
    }

    static Random random = new(8899);
    public static readonly string[] UserIds = RandomUserIds(20_000);
    static readonly string[] categories = [ "food", "transport", "fun", "home" ];

    public static string RandomCategory() {
        return categories[random.Next(0, categories.Length)];
    }

    public static string[] RandomUserIds(int count) {
        return Enumerable.Range(0, count).Select(i => random.Next(0, 1000).ToString()).ToArray();
    }
}