using CityInfo.API.Services;
using CityInfo.API.DbContexts;
using Microsoft.AspNetCore.StaticFiles;
using Serilog;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            
            var builder = WebApplication.CreateBuilder(args);
            
            // Logging with serilog.
            builder.Host.UseSerilog();

            // Add services to the container.

            builder.Services
                .AddControllers(options =>
                {
                    options.ReturnHttpNotAcceptable = true;
                })
                .AddNewtonsoftJson()
                .AddXmlDataContractSerializerFormatters();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

            // Custom services
#if DEBUG
            builder.Services.AddTransient<IMailService, LocalMailService>();
#else
            builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

            builder.Services.AddSingleton<CitiesDataStore>();
            // For this to work (when you are configuring all) use the command add-migration <migrationName> in Package Manager Console.
            // Then you apply the migration to the database using the update-database command, this should produce the CityInfo.db file in this particular scenario.
            builder.Services.AddDbContext<CityInfoContext>(options => options.UseSqlite
            (
                builder.Configuration["ConnectionStrings:CityInfoDBConnectionString"]
            ));
            builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Run();
        }
    }
}