using Domain.Configuration;
using Domain.Interfaces;
using Domain.Interfaces.Services;
using Infrastructure;
using Infrastructure.Task;
using Serilog;

namespace Template.Scheduler.Worker
{
    public class Program
    {
        const string DEFAULT_LOG_DIRECTORY = "C:/Logs/Template.Scheduler.Worker";

        public static void Main(string[] args)
        {
            var hostBuilder = CreateHostBuilder(args);

            hostBuilder.Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>

            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;
                    var filePath = configuration.GetSection("Log:Path") is null ? DEFAULT_LOG_DIRECTORY : configuration.GetSection("Log:Path").Value;

                    services.AddTransient<IWorkerNextTime, WorkerNextTime>();
                    services.AddTransient<IScheduleTask, ScheduleTask>();
                    services.AddTransient<IWorkerService, WorkerService>();
                    services.Configure<WorkerOptions>(options =>
                    configuration.GetSection("WorkerOptions").Bind(options));
                    services.AddHostedService<Worker>();

                    services.AddSingleton<Serilog.ILogger, Serilog.Core.Logger>(x =>
                    {
                        var logger = new LoggerConfiguration()
                        .WriteTo.File(filePath, rollingInterval: RollingInterval.Day)
                        .CreateLogger();

                        return logger;
                    });

                    services.AddInfrastructure(configuration);
                });
    }
}