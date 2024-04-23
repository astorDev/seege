using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

BenchmarkRunner.Run<LoadData>();

public static class Setup 
{
    public static IMongoCollection<Expense> GetFreshMongoCollection() {
        var mongoClient = new MongoClient("mongodb://localhost:27017");
        var mongoDb = mongoClient.GetDatabase("performance-battle");
        mongoDb.DropCollection("expenses");
        return mongoDb.GetCollection<Expense>("expenses");
    }

    public static Db GetFreshPostgresContext() {
        var db = new Db();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        return db;
    }

    public static Expense[] GetData() {
        var dataJson = File.ReadAllText("data.json");
        return JsonSerializer.Deserialize<Expense[]>(dataJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
}

[SimpleJob(RunStrategy.Monitoring, iterationCount: 100, id: "Load 3000 records")]
public class LoadData
{
    private int counter = 0;
    IMongoCollection<Expense> mongoCollection = Setup.GetFreshMongoCollection();
    Db postgresContext = Setup.GetFreshPostgresContext();
    Expense[] data = Setup.GetData();

    [Benchmark]
    public async Task InMongo() {
        counter++;
        var actualData = data.Select(d => d with { Id = d.Id + counter }).ToArray();

        await mongoCollection.Load(actualData);
    }

    [Benchmark]
    public async Task InPostgres() {
        counter++;
        var actualData = data.Select(d => d with { Id = d.Id + counter }).ToArray();

        postgresContext.Expenses.AddRange(actualData);
        await postgresContext.SaveChangesAsync();
    }
}

public record Expense(
    string Id,
    DateTime CreationTime,
    int Amount,
    Dictionary<string, string> Labels
);

public class Db : DbContext {
    public DbSet<Expense> Expenses { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=54321;Database=performance-battle;Username=postgres;Password=postgres;IncludeErrorDetail=true");
    }
}