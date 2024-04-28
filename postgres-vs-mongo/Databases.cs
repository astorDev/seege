using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

public static class MongoFactory {
    public static IMongoCollection<Expense> GetCollection() {
        var mongoClient = new MongoClient("mongodb://localhost:27017");
        var db = mongoClient.GetDatabase("performance-battle");
        return db.GetCollection<Expense>("expenses");
    }
}

public class Db : DbContext {
    public DbSet<ExpenseRecord> Expenses { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=performance-battle;Username=postgres;Password=postgres;IncludeErrorDetail=true");
    }
}

public class ExpenseRecord {
    public string Id { get; set; } = null!;
    public int Amount { get; set; }
    public JsonDocument Labels { get; set; } = null!;

    public static ExpenseRecord FromExpense(Expense expense) => new ExpenseRecord {
        Id = expense.Id,
        Amount = expense.Amount,
        Labels = JsonDocument.Parse(JsonSerializer.Serialize(expense.Labels))
    };
}