using log4net;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.HttpLogging;
using System.Security.Cryptography;

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

string path = "/data/upload/";
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !Directory.Exists("/data/upload/"))
{
    path = "/data/upload/";
    Directory.CreateDirectory(path);//linux os upload path is /data/upload
}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !Directory.Exists(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "upload")))
{
    path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "upload");
    Directory.CreateDirectory(path);//windows os upload folder is upload in the current directory
}


var md5 = MD5.Create();
string key = BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes("default"))).Replace("-", "");
ILog log = LogManager.GetLogger(typeof(Program));


//viewed the files
app.MapGet("/ls", (IHttpContextAccessor _httpContextAccessor) =>
{
    log.Info($"viewed upload files,ip:{_httpContextAccessor.HttpContext.Connection.RemoteIpAddress}{_httpContextAccessor.HttpContext.Connection.RemotePort}");
    return "1";
});


app.MapPost("/pos", (IHttpContextAccessor _httpContextAccessor) =>
{
    log.Info($"viewed upload files,ip:{_httpContextAccessor.HttpContext.Connection.RemoteIpAddress}{_httpContextAccessor.HttpContext.Connection.RemotePort}");
    return "1";
});


app.MapPost("/set", (IHttpContextAccessor _httpContextAccessor) =>
{

});

app.Run();

