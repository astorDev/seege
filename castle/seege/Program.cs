using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NBomber.CSharp;
using Seege;
using Seege.Reflection;

var services = new ServiceCollection();
var configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .AddInMemoryCollection(new KeyValuePair<string, string?>[] {
        new ("CastleUrl", "http://localhost:5000")
    })
    .Build();

services.AddSingleton<IConfiguration>(configuration);

var assemblies = AssemblyCollection.EntryAndReferenced();
var stepBuilderTypes = assemblies.Items.SelectMany(a => a.DefinedTypes.Where(t => t.IsImplementorOf(typeof(IStepBuilder))));
var scenarioBuilderTypes = assemblies.Items.SelectMany(a => a.DefinedTypes.Where(t => t.IsImplementorOf(typeof(IScenarioBuilder))));

foreach (Type type in stepBuilderTypes)  { services.AddSingleton(typeof(IStepBuilder), type); services.AddSingleton(type);}
foreach (Type type in scenarioBuilderTypes) { services.AddSingleton(typeof(IScenarioBuilder), type); services.AddSingleton(type); }

services.AddSingleton<Random>();
services.AddHttpService<Castle.Client>("CastleUrl");

var provider = services.BuildServiceProvider();
var builders = provider.GetServices<IScenarioBuilder>();
var allScenarios = builders.Select(b => b.Build()).ToArray();
var scenariosPattern = configuration["Scenarios"];
var scenariosToRun = scenariosPattern == null ? allScenarios : allScenarios.Where(s => Regex.IsMatch(s.ScenarioName, scenariosPattern)).ToArray();

NBomberRunner.RegisterScenarios(scenariosToRun).Run();