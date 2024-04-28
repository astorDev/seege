using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

[SimpleJob(RunStrategy.Monitoring, iterationCount: 200, id: "WriteBatches")]
public class WriteBatches
{
    private int iteration = 0;

    [Benchmark]
    public async Task Mongo() {
        var collection = MongoFactory.GetCollection();
        var actualData = TestData.ForIteration(iteration++);
        await collection.InsertManyAsync(actualData);
    }

    [Benchmark]
    public async Task Postgres() {
        using var db = new Db();
        db.Expenses.AddRange(TestData.ForIteration(iteration++).Select(ExpenseRecord.FromExpense));
        await db.SaveChangesAsync();
    }
}

[SimpleJob(RunStrategy.Monitoring, iterationCount: 10, id: "WriteOneByOne")]
public class WriteOneByOne
{
    private int iteration = 0;

    [Benchmark]
    public async Task Mongo() {
        var collection = MongoFactory.GetCollection();
        foreach (var item in TestData.ForIteration(iteration++, shift: 5_000, take: 1_000)) {
            await collection.InsertOneAsync(item);
        }
    }

    [Benchmark]
    public async Task Postgres() {
        await using var db = new Db();
        foreach (var item in TestData.ForIteration(iteration++, shift: 5_000, take: 1_000)) {
            db.Expenses.Add(ExpenseRecord.FromExpense(item));
            await db.SaveChangesAsync();
        }
    }
}

[SimpleJob(RunStrategy.Monitoring, iterationCount: 100, id: "ReadByUserId")]
public class ReadByUserId
{
    int iteration = 0;
    
    [Benchmark]
    public async Task Mongo() {
        var userId = TestData.UserIds[iteration++];
        var collection = MongoFactory.GetCollection();
        await collection
            .Find(e => e.Labels["userId"] == userId)
            .ToListAsync();
    }

    [Benchmark]
    public async Task Postgres() {
        var userId = TestData.UserIds[iteration++];
        using var db = new Db();
        await db.Expenses
            .AsNoTracking()
            .Where(e => e.Labels.RootElement.GetProperty("userId").GetString() == userId)
            .ToArrayAsync();
    }
}

[SimpleJob(RunStrategy.Monitoring, iterationCount: 100, id: "SumByCategory")]
public class SumByCategory
{
    [Benchmark]
    public async Task<int> Mongo() {
        var collection = MongoFactory.GetCollection();
        var aggregateRecord = await collection.Aggregate()
            .Match(e => e.Labels["category"] == TestData.RandomCategory())
            .Group(e => 1, g => new { Sum = g.Sum(e => e.Amount) })
            .FirstAsync();

        return aggregateRecord.Sum;
    }

    [Benchmark]
    public async Task<int> Postgres() {
        using var db = new Db();
        return await db.Expenses
            .Where(e => e.Labels.RootElement.GetProperty("category").GetString() == TestData.RandomCategory())
            .SumAsync(e => e.Amount);
    }
}

[SimpleJob(RunStrategy.Monitoring, iterationCount: 1)]
public class CreateUserIdIndex
{
    [Benchmark]
    public async Task Mongo() {
        var collection = MongoFactory.GetCollection();
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Expense>("""{ "Labels.userId": 1 }"""));
    }

    [Benchmark]
    public async Task Postgres() {
        using var db = new Db();
        await db.Database.ExecuteSqlRawAsync("""CREATE INDEX IF NOT EXISTS idx_expenses_label_userId ON "Expenses" USING BTREE (("Labels" -> 'userId'));""");
    }
}

[SimpleJob(RunStrategy.Monitoring, iterationCount: 1)]
public class CreateCategoryIndex {
    [Benchmark]
    public async Task Mongo() {
        var collection = MongoFactory.GetCollection();
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<Expense>("""{ "Labels.category": 1 }"""));
    }

    [Benchmark]
    public async Task Postgres() {
        using var db = new Db();
        await db.Database.ExecuteSqlRawAsync("""CREATE INDEX IF NOT EXISTS idx_expenses_label_category ON "Expenses" USING BTREE (("Labels" -> 'category'));""");
    }
}