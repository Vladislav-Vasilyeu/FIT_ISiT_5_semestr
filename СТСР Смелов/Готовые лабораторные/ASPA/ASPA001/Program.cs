//класс program содержащий точку входа
internal class Program
{
    //точка входа приложения
    private static void Main(string[] args)
    {
        //создаем объект для насттройки хостинга и сервисов
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
        });
        // создаем приложение на основе настроек
        var app = builder.Build();
        app.UseHttpLogging();
        // Настраивает маршрут: GET-запрос к "/" возвращает строку
        app.MapGet("/", () => "Мое первое ASPA");
        // Запускает приложение, начинает слушать HTTP-запросы
        app.Run();
    }
}