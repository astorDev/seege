namespace Castle;

public class Uris{
    public const string About = "about";
    public const string Citizens = "citizens";
    public const string Wood = "wood";

    public static string Citizen(string id) => $"{Citizens}/{id}";
    public static string CitizenWood(string id) => $"{Citizen(id)}/{Wood}";
}

public record About(string Description, string Version, string Environment);
public record CitizenCandidate(string Name);
public record Citizen(string Id, string Name);
public record WoodModification(int Craft = 0, int Burn = 0);
public record Wood(int Amount);

public interface IClient {
    Task<About> GetAbout();
    Task<Citizen> PostCitizen(CitizenCandidate candidate);
    Task<Wood> PatchCitizenWood(string citizenId, WoodModification modification);
}

public class Client : IClient {
    public HttpClient Http { get; }
    public Client(HttpClient http) { this.Http = http; }

    public Task<About> GetAbout() => this.Http.GetAsync(Uris.About).Read<About>();
    public Task<Citizen> PostCitizen(CitizenCandidate candidate) => this.Http.PostAsJsonAsync(Uris.Citizens, candidate).Read<Citizen>();
    public Task<Wood> PatchCitizenWood(string citizenId, WoodModification modification) => this.Http.PatchAsJsonAsync(Uris.CitizenWood(citizenId), modification).Read<Wood>();
}

public class Errors {
    public static Error Unknown => new (HttpStatusCode.InternalServerError, "Unknown");
}