namespace ASPA007_1.Services
{
    public static class CelebritiesConfigurationService
    {
        public static void AddCelebritiesConfiguration(this WebApplicationBuilder builder)
        {
            builder.Configuration.AddJsonFile("Celebrities.config.json", optional: false, reloadOnChange: true);
            builder.Services.Configure<CelebritiesConfig>(
                builder.Configuration.GetSection(CelebritiesConfig.SectionName));
            AppConfig.ConnectionString = builder.Configuration.GetSection("Celebrities:ConnectionString").Value;
        }
    }
}
