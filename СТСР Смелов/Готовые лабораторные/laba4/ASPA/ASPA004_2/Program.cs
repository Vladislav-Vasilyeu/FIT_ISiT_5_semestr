
using DAL004;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

Repository.JSONFileName = "Celebrities.json";

using (IRepository repository = Repository.Create("Celebrities"))
{


    app.UseExceptionHandler("/Celebrities/Error");

    app.MapGet("/Celebrities", () => repository.getAllCelebrities());

    app.MapGet("/Celebrities/{id::int}", (int id) => {
        Celebrity? celebrity = repository.getCelebrityById(id);
        if (celebrity == null) throw new FoundByIdException($"Celebrity Id = {id}");
        return celebrity;
    });
    app.MapDelete("/Celebrities/{id::int}", (int id) => {
        Celebrity? celebrity = repository.getCelebrityById(id);
        bool isDelete = repository.delCelebrityById(id);
        if (!isDelete)
        {
            throw new NotFoundCelebritiesToDelete($"Celebrity with Id = {id} don`t delete");
        }
        if (repository.SaveChanges() <= 0) throw new SaveException("/Celebrities error, SaveChanges() <= 0");
        return Results.Ok($"Celebrity with Id = {id} deleted");
    });

    app.MapPost("/Celebrities", (Celebrity celebrity) => {
        int? id = repository.addCelebrity(celebrity);
        if (id == null) throw new AddCelebrityException("/Celebrities error, id == null");
        if (repository.SaveChanges() <= 0) throw new SaveException("/Celebrities error, SaveChanges() <= 0");
        return new Celebrity((int)id, celebrity.Firstname, celebrity.Surname, celebrity.PhotoPath);
    });

    app.MapFallback((HttpContext ctx) => Results.NotFound(new { error = $"path {ctx.Request.Path} not supported" }));

    app.Map("/Celebrities/Error", (HttpContext ctx) => {
        Exception? ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
        IResult rc = Results.Problem(detail: "Panic", instance: app.Environment.EnvironmentName, title: "ASPA004", statusCode: 500);

        if (ex != null)
        {
            if (ex is NotFoundCelebritiesToDelete) rc = Results.NotFound(ex.Message);
            else if (ex is FoundByIdException) rc = Results.NotFound(ex.Message);
            else if (ex is BadHttpRequestException) rc = Results.BadRequest(ex.Message);
            else if (ex is SaveException) rc = Results.Problem(
                title: "ASPA004/SaveChanges",
                detail: ex.Message,
                instance: app.Environment.EnvironmentName,
                statusCode: 500
            );
            else if (ex is AddCelebrityException) rc = Results.Problem(
                title: "ASPA004/addCelebrity",
                detail: ex.Message,
                instance: app.Environment.EnvironmentName,
                statusCode: 500
            );
            else
            {
                rc = Results.Problem(
                    title: "ASPA004/UnknownError",
                    detail: ex.Message,
                    instance: app.Environment.EnvironmentName,
                    statusCode: 500
                );
            }
        }
        return rc;
    });

    app.Run();
}
public class NotFoundCelebritiesToDelete : Exception
{
    public NotFoundCelebritiesToDelete(string message) : base($"Not found to delete: {message}") { }
}
public class FoundByIdException : Exception
{
    public FoundByIdException(string message) : base($"Found by Id: {message}") { }
}

public class SaveException : Exception
{
    public SaveException(string message) : base($"SaveChanges error: {message}") { }
}

public class AddCelebrityException : Exception
{
    public AddCelebrityException(string message) : base($"AddCelebrityException error: {message}") { }
}