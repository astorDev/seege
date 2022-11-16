using NBomber.Contracts;
using NBomber.CSharp;

namespace Seege;

public interface IScenarioBuilder {
    Scenario Build();
}

public interface IStepBuilder {
    IStep Build();
}

public static class BuildersScenarioBuilder {
    public static Scenario Build(string name, params IStepBuilder[] builders) {
        var duplicatedSteps = builders.Select(b => b.Build()).ToArray();
        var uniqueSteps = duplicatedSteps.DistinctBy(s => s.StepName).ToArray();
        var steps = duplicatedSteps.Select(s => uniqueSteps.Single(us => s.StepName == us.StepName)).ToArray();
        
        return ScenarioBuilder.CreateScenario(name, steps);
    }
}