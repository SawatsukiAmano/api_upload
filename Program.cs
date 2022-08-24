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
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    path = "/data/upload/";
    if (!Directory.Exists(path)) Directory.CreateDirectory(path);//linux os upload path is /data/upload

}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "upload");
    if (!Directory.Exists(path)) Directory.CreateDirectory(path);//windows os upload folder is upload in the current directory

}


var md5 = MD5.Create();
string key = BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes("default"))).Replace("-", "");
ILog log = LogManager.GetLogger(typeof(Program));


//viewed the files
app.MapGet("/ls", (IHttpContextAccessor _httpContextAccessor) =>
{
    log.Info($"viewed upload files,ip:{_httpContextAccessor.HttpContext.Connection.RemoteIpAddress}{_httpContextAccessor.HttpContext.Connection.RemotePort}");

    DirectoryInfo dic = new DirectoryInfo(path);

    FileInfo[] files = dic.GetFiles();
    string result = string.Empty;
    foreach (var item in files)
    {
        result += item.Name + " " + item.LastWriteTime + " " + item.Length + "\r\n";
    }

    DirectoryInfo[] dics = dic.GetDirectories();
    foreach (var item in dics)
    {
        result += item.Name + " " + item.LastWriteTime + " " + item.CreationTime + "\r\n";
    }
    return result;
});


app.MapPost("/pos",  (IHttpContextAccessor _httpContextAccessor, IFormFile file) =>
{
    log.Info($"upload file,ip:{_httpContextAccessor.HttpContext.Connection.RemoteIpAddress}{_httpContextAccessor.HttpContext.Connection.RemotePort}");
    string filePath = path;
    if (file.Length > 0)
    {
        var full_name = path + file.Name;
        using (var stream = System.IO.File.Create(filePath))
        {
             file.CopyToAsync(stream);
            return "1";
        }
    }

    return "0";
});


app.MapPost("/set", (IHttpContextAccessor _httpContextAccessor) =>
{

});

app.Run();

