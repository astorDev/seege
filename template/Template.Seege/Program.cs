var services = new ServiceCollection();
var configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .AddInMemoryCollection(new KeyValuePair<string, string?>[] {
        new ("Scenarios", ".")
    })
    .Build();

services.AddSingleton<IConfiguration>(configuration);

var assemblies = AssemblyCollection.EntryAndReferenced();
var stepBuilderTypes = assemblies.Items.SelectMany(a => a.DefinedTypes.Where(t => t.IsImplementorOf(typeof(IStepBuilder))));
var scenarioBuilderTypes = assemblies.Items.SelectMany(a => a.DefinedTypes.Where(t => t.IsImplementorOf(typeof(IScenarioBuilder))));

foreach (Type type in stepBuilderTypes)  { services.AddSingleton(typeof(IStepBuilder), type); services.AddSingleton(type);}
foreach (Type type in scenarioBuilderTypes) { services.AddSingleton(typeof(IScenarioBuilder), type); services.AddSingleton(type); }

services.AddSingleton<Random>();

var provider = services.BuildServiceProvider();
var builders = provider.GetServices<IScenarioBuilder>();
var allScenarios = builders.Select(b => b.Build()).ToArray();
var scenariosToRun = allScenarios.Where(s => Regex.IsMatch(s.ScenarioName, configuration["Scenarios"]!)).ToArray();

NBomberRunner.RegisterScenarios(scenariosToRun).Run();