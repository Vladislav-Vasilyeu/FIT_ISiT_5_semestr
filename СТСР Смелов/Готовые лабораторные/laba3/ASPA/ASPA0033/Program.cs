using DAL003;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System.Text.Encodings.Web;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/Photo",
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Celebrities", "Photo"))

});

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    RequestPath = "/Celebrities/download",
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Celebrities", "Photo")),
    Formatter = new HtmlDirectoryFormatter(HtmlEncoder.Default)
});

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/Celebrities/download",
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Celebrities", "Photo")),
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.ContentType = "image/jpg";
        var fileName = ctx.Context.Request.Path.Value?.Split('/').Last() ?? "photo.jpg";
        ctx.Context.Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");

    }
});

Repository.JSONFileName = "Celebrities.json";
using (IRepository repository = Repository.Create("Celebrities"))
{
    app.MapGet("/Celebrities", () => repository.getAllCelebrities());
    app.MapGet("/Celebrities/{id:int}", (int id) => repository.getCelebrityById(id));
    app.MapGet("/Celebrities/BySurname/{surname}", (string surname) => repository.getCelebritiesBySurname(surname));
    app.MapGet("/Celebrities/PhotoPathById/{id:int}", (int id) => repository.getPhotoPathById(id));

    app.MapGet("/", () => "Hello World!");
    app.Run();
}