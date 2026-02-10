
using DAL_Celebrity_MSSQL;

using Microsoft.Extensions.Options;
using ASPA007_1.Services;
using ASPA007_1.API;
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.AddCelebritiesConfiguration();

        builder.AddCelebritiesDatabase();

        builder.Services.AddCelebritiesRouting();

        var app = builder.Build();

        app.UseMiddleware<CelebritiesError>();
        app.UseCelebritiesMiddleware();
        app.MapRazorPages();

        app.MapCelebrities();
        app.MapLifeevents();
        app.MapPhotoCelebrities();

        app.Run();
    }
}
public class CelebritiesConfig
{
    public const string SectionName = "Celebrities";
    public string PhotosFolder { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}
public static class AppConfig
{
    public static string ConnectionString { get; set; }
}