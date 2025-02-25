using ElectronicDiary.Application.Interfaces;
using ElectronicDiary.Infrastructure.Repositories;
using ElectronicDiary.Infrastructure.Services;
using ElectronicDiary.WebApi.Extensions;
using ElectronicDiary.WebApi.Helpers.Mappings;
using Serilog;

namespace ElectronicDiary.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            Log.Logger = new LoggerConfiguration()
                     .Enrich.FromLogContext()
                     .WriteTo.Console()
                     .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
                     .CreateLogger();
            builder.Host.UseSerilog();


            builder.Services.ConfigureDatabase(builder.Configuration);
            builder.Services.ConfigureIdentity();
            builder.Services.ConfigureAuthentication(builder.Configuration);
            builder.Services.ConfigureSwagger();
            builder.Services.ConfigureApplicationServices();


            builder.Services.AddHealthChecks()
                .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();


            var app = builder.Build();

            app.ConfigureMiddlewares();

            app.Run();
        }
    }
}
