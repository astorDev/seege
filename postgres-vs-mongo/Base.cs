using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

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

public static class MongoFactory
{
    public static IMongoCollection<Expense> GetCollection() {
        var mongoClient = new MongoClient("mongodb://localhost:27017");
        var db = mongoClient.GetDatabase("performance-battle");
        return db.GetCollection<Expense>("expenses");
    }
}

public class Db : DbContext {
    public DbSet<ExpenseRecord> Expenses { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseNpgsql("Host=localhost;Port=54321;Database=performance-battle;Username=postgres;Password=postgres;IncludeErrorDetail=true");
    }
}

public class ExpenseRecord
{
    public string Id { get; set; } = null!;
    public int Amount { get; set; }
    public JsonDocument Labels { get; set; } = null!;

    public static ExpenseRecord FromExpense(Expense expense) {
        return new ExpenseRecord {
            Id = expense.Id,
            Amount = expense.Amount,
            Labels = JsonDocument.Parse(JsonSerializer.Serialize(expense.Labels))
        };
    }
}