record Wooding(CreateCitizen CreateCitizen, Craft Craft, Burn Burn) : IScenarioBuilder {
    public Scenario Build() => BuildersScenarioBuilder
            .Build("wooding", this.CreateCitizen, this.Craft, this.Burn, this.Craft, this.Burn)
            .WithoutWarmUp()
            .WithLoadSimulations(
                Simulation.InjectPerSec(rate: 100, TimeSpan.FromSeconds(26))
            );
}

record Populating(CreateCitizen CreateCitizen) : IScenarioBuilder {
    public Scenario Build() => BuildersScenarioBuilder
            .Build("populating", this.CreateCitizen)
            .WithoutWarmUp()
            .WithLoadSimulations(Simulation.InjectPerSec(rate: 100, TimeSpan.FromSeconds(10)));
}

record Burn(Castle.Client Client, Random Random) : IStepBuilder {
    public IStep Build() => Step.Create("burn",
            timeout: TimeSpan.FromMilliseconds(500),
            execute: async context =>
            {
                var amount = this.Random.Next(0, 300);
                var citizenId = (string)context.Data["citizenId"];
                await this.Client.PatchCitizenWood(citizenId, new(Burn: amount));
                return Response.Ok();
            }
        );
}

record Craft(Castle.Client Client, Random Random) : IStepBuilder {
    public IStep Build() {
        return Step.Create("craft",
            timeout: TimeSpan.FromMilliseconds(1500),
            execute: async context => {
                var citizenId = (string)context.Data["citizenId"];
                var amount = this.Random.Next(0, 1000);
                await this.Client.PatchCitizenWood(citizenId, new(Craft: amount));
                return Response.Ok();
            }
        );
    }
}

record CreateCitizen(Castle.Client Client) : IStepBuilder {
    static readonly string[] names = new [] { "Alex", "George", "Angela", "Basil", "Bill" };
    static readonly IFeed<string> namesFeed = Feed.CreateRandom("names", names);

    public IStep Build() {
        return Step.Create("createCitizen",
            timeout: TimeSpan.FromMilliseconds(20000),
            feed: namesFeed,
            execute: async context => {
                var citizen = await this.Client.PostCitizen(new (context.FeedItem));
                context.Data["citizenId"] = citizen.Id;
                return Response.Ok(citizen.Id);
            }
        );
    }
}