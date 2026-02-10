namespace ASPA007_1.Services
{
    public class CelebritiesError
    {
        private readonly RequestDelegate _next;
        public CelebritiesError(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Celebrities Module Error!");
            }
        }
    }
}
