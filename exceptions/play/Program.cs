using System.Net;
using Nist;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSimpleConsole(c => c.SingleLine = true);
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseProblemForExceptions(AppExceptionMapper.ToError);

app.MapPost("/exceptional-vegetables", (Vegetable vegetable) =>
{
    HotPotatoException.Of(vegetable)?.Throw();
    return vegetable;
});

app.MapPost("/resulted-vegetables", (Vegetable vegetable) =>
{
    return 
        HotPotatoException.Of(vegetable)?.ToError().ToResult()
        ?? Results.Ok(vegetable);
});

app.Run();

public record Vegetable(string Name, bool IsHot);
public class HotPotatoException : Exception
{
    public static HotPotatoException? Of(Vegetable vegetable) => vegetable.Name == "Potato" && vegetable.IsHot ? new() : null;
}

public static class AppExceptionMapper
{
    public static Error ToError(this Exception exception) => exception is HotPotatoException
            ? new(HttpStatusCode.BadRequest, "HotPotato")
            : new Error(HttpStatusCode.InternalServerError, "Unknown");
}

// TO DO: Move to NIST package

public static class ExceptionExtensions
{
    public static void Throw(this Exception? exception)
    {
        if (exception is not null) throw exception;
    }
}

public static class ExceptionMapper
{
    public static IResult? ToResult(this Error? error) => error == null
        ? null 
        : Results.Problem(type: error.Reason, statusCode: (int)error.Code);
}