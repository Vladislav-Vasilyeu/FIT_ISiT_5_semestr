namespace ASPA007_1.Services
{
    public static class RoutingService
    {
        public static void AddCelebritiesRouting(this IServiceCollection services)
        {
            services.AddRazorPages(options =>
            {
                options.Conventions.AddPageRoute("/Pages/Index", "/");
                options.Conventions.AddPageRoute("/Pages/0", "/0");
                options.Conventions.AddPageRoute("/Pages/PhotoPage", "/Celebrities/{id:int}");
            });
        }
    }
}
