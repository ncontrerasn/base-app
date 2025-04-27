namespace baseApp.Middleware;

public class RefreshTokenLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _logFilePath = "Logs/refresh_logs.txt";

    public RefreshTokenLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/auth/refresh"))
        {
            var refreshToken = context.Request.Cookies["refreshToken"];
            Directory.CreateDirectory("Logs"); // Asegura que exista el folder

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await File.AppendAllTextAsync(_logFilePath, $"[{DateTime.UtcNow}] REFRESH usado: {refreshToken.Substring(0, 10)}...\n");
            }
            else
            {
                await File.AppendAllTextAsync(_logFilePath, $"[{DateTime.UtcNow}] No se encontró refresh token en cookies.\n");
            }
        }

        await _next(context);
    }
}


