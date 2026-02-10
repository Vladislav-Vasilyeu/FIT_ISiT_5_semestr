using DAL_Celebrity_MSSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ASPA007_1.Services
{
    public static class DatabaseService
    {
        public static void AddCelebritiesDatabase(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<Context>(options =>
                options.UseSqlServer(builder.Configuration.GetSection("Celebrities:ConnectionString").Value));

            builder.Services.AddScoped<IRepository, Repository>(p =>
            {
                var config = p.GetRequiredService<IOptions<CelebritiesConfig>>().Value;
                return new Repository(config.ConnectionString);
            });
        }
    }
}
