using DAL004;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

Repository.JSONFileName = "Celebrities.json";

using (IRepository repository =Repository.Create("Celebrities"))
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
        repository.SaveChanges();
        return Results.Ok($"Celebrity with Id = {id} deleted");
    });

    app.MapPut("/Celebrities/{id::int}", (int id, Celebrity celebrity) => {
        int? index = repository.updCelebrityById(id, celebrity);
        if (index == null) throw new DontUpdateCelebrity($"Celebrity with Id = {id} don`t updated");
        repository.SaveChanges();
        return repository.getCelebrityById(id);
    });

    app.MapPost("/Celebrities", (Celebrity celebrity) => {
        int? id = repository.addCelebrity(celebrity);
        if (id == null) throw new AddCelebrityException("/Celebrities error, id == null");
        if (repository.SaveChanges() <= 0) throw new SaveException("/Celebrities error, SaveChanges() <= 0");
        return new Celebrity((int)id, celebrity.Firstname, celebrity.Surname, celebrity.PhotoPath);
    })
    .AddEndpointFilter(async (content, next) => {
        var celebrity = content.GetArgument<Celebrity>(0);
        if (celebrity == null)
        {
            return Results.Problem(
                title: "Internal Server Error",
                detail: "Параметр celebrity не может быть null.",
                statusCode: 500
            );
        }
        if (string.IsNullOrEmpty(celebrity.Surname) || celebrity.Surname.Length < 2)
        {
            return Results.Problem(
                title: "Conflict",
                detail: "Поле Surname обязательно и должно содержать минимум 2 символа.",
                statusCode: 409
            );
        }
        return await next(content);
    })
    .AddEndpointFilter(async (content, next) => {
        var celebrity = content.GetArgument<Celebrity>(0);
        if (celebrity == null)
        {
            return Results.Problem(
                title: "Internal Server Error",
                detail: "Параметр celebrity не может быть null.",
                statusCode: 500
            );
        }
        var allcelebrities = repository.getAllCelebrities();
        foreach (Celebrity item in allcelebrities)
        {
            if (celebrity.Surname == item.Surname)
            {
                return Results.Problem(
                    title: "Conflict",
                    detail: "Поле Surname c таким значением уже есть.",
                    statusCode: 409
                );
            }
        }
        return await next(content);
    })
    .AddEndpointFilter(async (context, next) => {
        var celebrity = context.GetArgument<Celebrity>(0);
        if (celebrity == null)
        {
            return Results.Problem(
                title: "Internal Server Error",
                detail: "Параметр celebrity не может быть null.",
                statusCode: 500
            );
        }
        var photoFileName = Path.GetFileName(celebrity.PhotoPath);
        var basePath = "D:\\Универ\\СТСР Смелов\\Готовые лабораторные\\laba5\\ASPA\\ASPA005_1\\Celebrities";
        var fullPath = Path.Combine(basePath, photoFileName);
        if (!File.Exists(fullPath))
        {
            context.HttpContext.Response.Headers.Append("X-Celebrity", $"NotFound = {photoFileName}");
            return Results.Problem(
                title: "Not Found",
                detail: $"Файл изображения {photoFileName} не найден.",
                statusCode: 404
            );
        }
        return await next(context);
    });

    app.MapFallback((HttpContext ctx) => Results.NotFound(new { error = $"path {ctx.Request.Path} not supported" }));

    app.Map("/Celebrities/Error", (HttpContext ctx) => {
        Exception? ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
        IResult rc = Results.Problem(detail: "Panic", instance: app.Environment.EnvironmentName, title: "ASPA004", statusCode: 500);

        if (ex != null)
        {
            if (ex is DontUpdateCelebrity) rc = Results.NotFound(ex.Message);
            else if (ex is NotFoundCelebritiesToDelete) rc = Results.NotFound(ex.Message);
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
public class DontUpdateCelebrity : Exception
{
    public DontUpdateCelebrity(string message) : base($"Not found to update: {message}") { }
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