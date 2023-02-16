record Greeting(Hello Hello, Bye Bye) : IScenarioBuilder {
    public Scenario Build() => BuildersScenarioBuilder
            .Build("greeting", this.Hello, this.Bye)
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: 200_000, TimeSpan.FromSeconds(8))
            );
}

record Bye(Random Random) : IStepBuilder {
    public IStep Build() {
        return Step.Create("buy",
            timeout: TimeSpan.FromMilliseconds(1500),
            execute: async context => {
                var name = (string)context.Data["Name"] + Random.Next(0, 1_000);
                if (name == "George7788") Console.WriteLine("Bingo!!!");
                return Response.Ok();
            }
        );
    }
}

record Hello(Random Random) : IStepBuilder {
    static readonly string[] names = { "Alex", "George", "Angela", "Basil", "Bill" };
    static readonly IFeed<string> namesFeed = Feed.CreateRandom("names", names);

    public IStep Build() {
        return Step.Create("hello",
            timeout: TimeSpan.FromMilliseconds(2000),
            feed: namesFeed,
            execute: async context => {
                context.Data["Name"] = context.FeedItem + Random.Next(0, 1_000);
                return Response.Ok(context.FeedItem);
            }
        );
    }
}