using BenchmarkDotNet.Running;

MongoFactory.GetCollection().Database.DropCollection("expenses");
new Db().Database.EnsureDeleted();
new Db().Database.EnsureCreated();

BenchmarkRunner.Run<CreateUserIdIndex>();
BenchmarkRunner.Run<WriteBatches>();
BenchmarkRunner.Run<WriteOneByOne>();
BenchmarkRunner.Run<ReadByUserId>();
BenchmarkRunner.Run<CreateCategoryIndex>();
BenchmarkRunner.Run<SumByCategory>();