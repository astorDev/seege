using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration["Logging:LogLevel:Default"] = "Warning";
builder.Configuration["Logging:LogLevel:Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware"] = "None";
builder.Configuration["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Information";
builder.Configuration["Logging:StateJsonConsole:LogLevel:Default"] = "None";
builder.Configuration["Logging:StateJsonConsole:LogLevel:Nist.Logs"] = "Information";

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(c => c.SingleLine = true);
builder.Logging.AddStateJsonConsole();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpIOLogging();
app.UseErrorBody(ex => ex switch {
    _ => Errors.Unknown
});
app.MapControllers();

app.MapGet($"/{Uris.About}", (IHostEnvironment env) => new About(
    Description: "Castle webapi",
    Version: Assembly.GetEntryAssembly()!.GetName().Version!.ToString(),
    Environment: env.EnvironmentName
));

BlockingCollection<Citizen> citizens = new ();
int indexer = 0;

app.MapPost($"/{Uris.Citizens}", (CitizenCandidate candidate) => {
    var index = Interlocked.Increment(ref indexer);
    var citizen = new Citizen(index.ToString(), candidate.Name);
    citizens.Add(citizen);
    return citizen;
});

ConcurrentDictionary<string, Wood> citizedWoods = new();

app.MapPatch($"/{Uris.Citizens}/{{citizenId}}/{Uris.Wood}", async (string citizenId, WoodModification modification) => {
    await Task.Delay(modification.Burn + modification.Craft);
    var wood = citizedWoods.GetOrAdd(citizenId, (c) => new Wood(0));
    if (modification.Burn > wood.Amount) throw new InvalidOperationException("Not enough wood");
    var resutlWood = new Wood(wood.Amount - modification.Burn + modification.Craft);
    citizedWoods[citizenId] = resutlWood;
    return resutlWood;
});

app.Run();