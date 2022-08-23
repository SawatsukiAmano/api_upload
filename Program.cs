using log4net;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.All;
    options.RequestHeaders.Add("Cache-Control");
    options.ResponseHeaders.Add("Server");
});
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpLogging();//start the http logs
log4net.Config.XmlConfigurator.Configure(new FileInfo("log4net.config")); //config log4net

if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !Directory.Exists("/data/upload/"))
{
    Directory.CreateDirectory("/data/upload/");//linux os upload path is /data/upload
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !Directory.Exists(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "upload")))
{
    Directory.CreateDirectory(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "upload"));//windows os upload folder is upload in the current directory
}



app.MapGet("/weatherforecast", (IHttpContextAccessor _httpContextAccessor) =>
{
    return "123";
}).WithName("GetWeatherForecast");




app.Run();

