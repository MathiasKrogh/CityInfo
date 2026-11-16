using CityInfo.API.Services;
using CityInfo.API;
using Microsoft.AspNetCore.StaticFiles;
using Serilog;
using CityInfo.API.DbContexts;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 3 different ways to add your own services: Transient, Scoped, Singleton.
// Transient is used for lightweight stateless services.
// Scoped is used for services that are created once per request.
// Singleton is used for services that are created once and shared throughout the application.
#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif
builder.Services.AddSingleton<CitiesDataStore>();

// You can use an environment variable to set the connection string for the database. This is useful for production environments
// where you don't want to store sensitive information in your code or configuration files.
// You can set the environment variable in your hosting environment, and then retrieve it in your application using the Configuration API.

// Azure Key Vault is an even better way to store sensitive information like connection strings, as it provides additional security features such as encryption and access control.
// You can configure your application to retrieve the connection string from Azure Key Vault using the Azure SDK for .NET.
// This way, you can keep your connection strings secure and separate from your application code and configuration files.
builder.Services.AddDbContext<CityInfoContext>(dbContextOptions => dbContextOptions.UseSqlite(builder.Configuration["ConnectionStrings:CityInfoDBConnectionString"]));

// Adds exceptions for formats
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson()
  .AddXmlDataContractSerializerFormatters();
// Allows to see what type files are
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();


// Can add extra information for error codes
builder.Services.AddProblemDetails();
//builder.Services.AddProblemDetails(options =>
//{
//    options.CustomizeProblemDetails = ctx =>
//    {
//        ctx.ProblemDetails.Extensions.Add("AdditionalInfo", "This is additional information for the problem details.");
//        ctx.ProblemDetails.Extensions.Add("Server", Environment.MachineName);
//    };
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
} 
else // When in production
{
    app.UseExceptionHandler();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
