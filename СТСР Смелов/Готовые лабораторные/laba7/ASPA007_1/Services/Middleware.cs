namespace ASPA007_1.Services
{
    public static class MiddlewareService
    {
        public static void UseCelebritiesMiddleware(this WebApplication app)
        {
            app.UseStaticFiles();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseRouting();
            app.UseAuthorization();
        }
    }
}
