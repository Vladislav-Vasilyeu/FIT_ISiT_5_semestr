using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "Neumann.html" }
});
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/static",
    FileProvider = new PhysicalFileProvider(builder.Environment.WebRootPath)
});
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/picture",
    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Picture"))
});
app.UseWelcomePage("/aspnetcore");

app.Run();
