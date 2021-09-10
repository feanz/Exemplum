using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApi
{
    using Infrastructure.Persistence;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using Serilog.Events;

    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var logConfiguration = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Debug()
                .WriteTo.Debug()
                .WriteTo.Seq("http://localhost:5341")
#else
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
#endif
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Async(c =>
                    c.File($"App_Data/Logs/Exemplum-Logs-.txt", rollingInterval: RollingInterval.Day));
            
            Log.Logger = logConfiguration.CreateLogger();

            try
            {
                Log.Information("Starting web host");
                
                var host = CreateHostBuilder(args).Build();
                
                await SeedDatabase(host);

                await host.RunAsync();
                
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
            
            
        }

        private static async Task SeedDatabase(IHost host)
        {
            using var scope = host.Services.CreateScope();
            
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<ApplicationDbContext>();

                await ApplicationDbContextSeed.SeedSampleDataAsync(context);
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

                logger.LogError(ex, "An error occurred while migrating or seeding the database");

                throw;
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
