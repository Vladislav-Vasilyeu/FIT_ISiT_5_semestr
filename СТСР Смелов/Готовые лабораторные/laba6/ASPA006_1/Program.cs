using DAL_Celebrity_MSSQL;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;



var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("Celebrities.config.json", optional: false, reloadOnChange: true);

builder.Services.Configure<CelebritiesConfig>(
    builder.Configuration.GetSection(CelebritiesConfig.SectionName));

builder.Services.AddScoped<IRepository, Repository>((IServiceProvider p) =>
{
    CelebritiesConfig config = p.GetRequiredService<IOptions<CelebritiesConfig>>().Value;
    return new Repository(config.ConnectionString);
});

var app = builder.Build();
app.UseStaticFiles();
app.UseExceptionHandler("/Error");
app.UseExceptionHandler(appError => {
    appError.Run(async context => {
        var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (ex is DeleteError deleteEx)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = deleteEx.Message,
                instance = "ANC25"
            });
        }
        else if (ex is NullError nullError)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Not Found Info",
                status = 404,
                detail = nullError.Message
            });
        }
        else if (ex is FileError fileError)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Not Found file",
                statusCode = 404,
                detail = fileError.Message
            });
        }
        else if (ex is MinusIdError minusIdError)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Negative index",
                statusCode = 404,
                detail = minusIdError.Message
            });
        }
        else
        {
            await context.Response.WriteAsJsonAsync(new
            {
                title = "Not Found this error",
                status = 500
            });
        }
    });
});

// --- Группа эндпоинтов для работы со знаменитостями ---
var celebrities = app.MapGroup("/api/Celebrities");

// Получить всех знаменитостей
celebrities.MapGet("/", (IRepository repo) =>
{
    var celebrities = repo.GetAllCelebrities();
    if (celebrities == null)
    {
        throw new NullError("GetAllCelebrities вернул null");
    }
    return repo.GetAllCelebrities();
});

// Получить знаменитость по ID  
celebrities.MapGet("/{id:int:min(1)}", (IRepository repo, int id) =>
{
    if (id < 0)
    {
        throw new MinusIdError($"Negative id {id}");
    }
    // Реализация получения знаменитости по ID
    var celebrity = repo.GetCelebrityById(id);
    if (celebrity == null)
    {
        throw new NullError("GetCelebrityById вернул null");
    }
    return repo.GetCelebrityById(id);
});

// Получить знаменитость по ID события
celebrities.MapGet("/LifeEvents/{eventId:int:min(1)}", (IRepository repo, int eventId) =>
{
    // Реализация получения знаменитости по ID события
    if (eventId < 0)
    {
        throw new MinusIdError($"Negative id {eventId}");
    }
    var celebrity = repo.GetCelebrityByLifeeventId(eventId);
    if (celebrity == null)
    {
        throw new NullError("GetCelebrityById вернул null");
    }
    return repo.GetCelebrityByLifeeventId(eventId);
});

// Удалить знаменитость по ID
celebrities.MapDelete("/{id:int:min(1)}", (IRepository repo, int id) =>
{
    if (id < 0)
    {
        throw new MinusIdError($"Negative id {id}");
    }
    var celebrity = repo.GetCelebrityById(id);
    if (celebrity == null)
    {
        throw new DeleteError($"404002:Celebrity Id = {id}");
    }
    repo.DelCelebrity(id);
    return celebrity;
});

// Добавить новую знаменитость
celebrities.MapPost("/", (IRepository repo, Celebrity celebrity) =>
{
    // Реализация добавления новой знаменитости
    repo.AddCelebrity(celebrity);
    if (repo.GetCelebrityIdByName(celebrity.FullName) == -1)
    {
        throw new NullError("GetCelebrityIdByName вернул null");
    }
    return repo.GetCelebrityIdByName(celebrity.FullName);
});

// Обновить знаменитость по ID
celebrities.MapPut("/{id:int:min(1)}", (IRepository repo, int id, Celebrity celebrity) =>
{
    // Реализация обновления знаменитости
    if (id < 0)
    {
        throw new MinusIdError($"Negative id {id}");
    }
    repo.UpdCelebrity(id, celebrity);
    return repo.GetCelebrityById(id);
});

// Получить файл фотографии по имени
celebrities.MapGet("/photo/{fileName}", async (IOptions<CelebritiesConfig> config, IWebHostEnvironment env, string fileName) =>
{
    var webRootPath = env.WebRootPath;

    var filePath = Path.Combine(webRootPath, "photo", fileName);

    if (!File.Exists(filePath))
    {
        throw new FileError($"File - {fileName} not found");
    }

    return Results.File(filePath, "image/jpeg");
});

// --- Группа эндпоинтов для работы с событиями ---
var lifeEvents = app.MapGroup("/api/LifeEvents");

// Получить все события
lifeEvents.MapGet("/", (IRepository repo) =>
{
    // Реализация получения всех событий
    var lifeevents = repo.GetAllLifeevents();
    if (lifeevents == null)
    {
        throw new NullError("GetAllLifeevents вернул null");
    }
    return repo.GetAllLifeevents();
});

// Получить событие по ID
lifeEvents.MapGet("/{id:int:min(1)}", (IRepository repo, int id) =>
{
    // Реализация получения события по ID
    if (id < 0)
    {
        throw new MinusIdError($"Negative id {id}");
    }
    var lifeevents = repo.GetLifeeventById(id);
    if (lifeevents == null)
    {
        throw new NullError("GetLifeeventById вернул null");
    }
    return repo.GetLifeeventById(id);
});

// Получить все события по ID знаменитости
lifeEvents.MapGet("/Celebrities/{celebrityId:int:min(1)}", (IRepository repo, int celebrityId) =>
{
    // Реализация получения событий знаменитости
    if (celebrityId < 0)
    {
        throw new MinusIdError($"Negative id {celebrityId}");
    }
    var lifeevents = repo.GetLifeeventsByCelebrityId(celebrityId);
    if (lifeevents == null)
    {
        throw new NullError("GetLifeeventsByCelebrityId вернул null");
    }
    return repo.GetLifeeventsByCelebrityId(celebrityId);
});

// Удалить событие по ID
lifeEvents.MapDelete("/{id:int:min(1)}", (IRepository repo, int id) =>
{
    if (id < 0)
    {
        throw new MinusIdError($"Negative id {id}");
    }
    var celebrity = repo.GetLifeeventById(id);
    if (celebrity == null)
    {
        throw new DeleteError($"404002:Lifeevent Id = {id}");
    }
    repo.DelLifeevent(id);
    return celebrity;
});

// Добавить новое событие
lifeEvents.MapPost("/", (IRepository repo, Lifeevent lifeEvent) =>
{
    // Реализация добавления нового события
    repo.AddLifeevent(lifeEvent);
    if (repo.GetLifeeventsByCelebrityId(lifeEvent.CelebrityId) == null)
    {
        throw new NullError("GetLifeeventsByCelebrityId вернул null");
    }
    return repo.GetLifeeventsByCelebrityId(lifeEvent.CelebrityId);
});

// Обновить событие по ID
lifeEvents.MapPut("/{id:int:min(1)}", (IRepository repo, int id, Lifeevent lifeEvent) =>
{
    // Реализация обновления события
    if (id < 0)
    {
        throw new MinusIdError($"Negative id {id}");
    }
    repo.UpdLifeevent(id, lifeEvent);
    return repo.GetLifeeventById(id);
});

app.Map("/Error", (HttpContext ctx) =>
{
    var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    if (ex is DeleteError deleteError)
    {
        return Results.Problem(
            type: "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            title: "Not Found",
            statusCode: 404,
            detail: deleteError.Message,
            instance: "ANC25"
        );
    }
    else if (ex is NullError nullError)
    {
        return Results.Problem(
            title: "Not Found Info",
            statusCode: 404,
            detail: nullError.Message
        );
    }
    else if (ex is FileError fileError)
    {
        return Results.Problem(
            title: "Not Found file",
            statusCode: 404,
            detail: fileError.Message
        );
    }
    else if (ex is MinusIdError minusIdError)
    {
        return Results.Problem(
            title: "Negative index",
            statusCode: 404,
            detail: minusIdError.Message
        );
    }

    return Results.Problem(
        title: "Not Found this error",
        statusCode: StatusCodes.Status500InternalServerError
    );
});
app.Run();
public class DeleteError : Exception
{
    public DeleteError(string message) : base($"Delete by Id: {message}") { }
}
public class NullError : Exception
{
    public NullError(string message) : base($"Null info: {message}") { }
}
public class FileError : Exception
{
    public FileError(string message) : base($"File not found: {message}") { }
}
public class MinusIdError : Exception
{
    public MinusIdError(string message) : base($"Negative index: {message}") { }
}
public class CelebritiesConfig
{
    public const string SectionName = "Celebrities";

    public string PhotosFolder { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}


