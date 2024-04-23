using MongoDB.Driver;

public static class MongoExtensions 
{
    public static Task Drop(this IMongoCollection<Expense> collection) => collection.DeleteManyAsync(FilterDefinition<Expense>.Empty);
    public static Task Load(this IMongoCollection<Expense> collection, Expense[] data) => collection.InsertManyAsync(data);
}