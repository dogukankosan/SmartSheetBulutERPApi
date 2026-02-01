using Serilog;
using SmartSheetLicenceApi.Middleware;
using SmartSheetLicenceApi.Services;

namespace SmartSheetLicenceApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Serilog yapýlandýrmasý
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("logs/api-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    fileSizeLimitBytes: 10_000_000,
                    rollOnFileSizeLimit: true)
                .CreateLogger();
            try
            {
                Log.Information("Starting SmartSheet License API");
                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
                // URL'i ayarla - BUNU EKLE
                builder.WebHost.UseUrls("http://0.0.0.0:1020");
                // Serilog'u kullan
                builder.Host.UseSerilog();
                // Windows Service desteði ekle
                builder.Services.AddWindowsService();
                // Services
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                // Swagger
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new()
                    {
                        Title = "SmartSheet License API",
                        Version = "v1",
                        Description = "SmartSheet lisans yönetim servisi"
                    });
                });
                // Memory Cache ekle
                builder.Services.AddMemoryCache();
                // DatabaseService'i Singleton olarak ekle
                builder.Services.AddSingleton<DatabaseService>();
                // CORS - Tüm kaynaklardan eriþime izin ver
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowAll", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                });
                // Response Compression (performans için)
                builder.Services.AddResponseCompression(options =>
                {
                    options.EnableForHttps = true;
                });
                // Health Checks
                builder.Services.AddHealthChecks();
                WebApplication app = builder.Build();
                // Veritabanýný initialize et
                using (var scope = app.Services.CreateScope())
                {
                    DatabaseService dbService = scope.ServiceProvider.GetRequiredService<DatabaseService>();
                    await dbService.InitializeDatabaseAsync();
                    Log.Information("Database initialized successfully");
                }
                // Middleware'ler
                app.UseResponseCompression();
                // Rate Limiting Middleware
                app.UseMiddleware<RateLimitMiddleware>();
                // Swagger - sadece Development'ta
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartSheet License API v1");
                    });
                }
                // Global Exception Handler
                app.UseExceptionHandler("/error");
                // HTTPS Redirection - Production'da aktif
                if (app.Environment.IsProduction())
                    app.UseHttpsRedirection();
                app.UseCors("AllowAll");
                app.UseAuthorization();
                // Health Check endpoint
                app.MapHealthChecks("/health");
                app.MapControllers();
                Log.Information("API started successfully on {Environment} - Port 1020", app.Environment.EnvironmentName);
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}