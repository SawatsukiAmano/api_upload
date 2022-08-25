using log4net;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.HttpLogging;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.Features;

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


// 200 MB
const int maxRequestLimit = 209715200;
// If using Kestrel
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = maxRequestLimit;//default max upload file size is 30 MB
});


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

app.MapPost("/pos", async (IHttpContextAccessor _httpContextAccessor, HttpRequest request) =>
{
    log.Info($"upload file,ip:{_httpContextAccessor.HttpContext.Connection.RemoteIpAddress}{_httpContextAccessor.HttpContext.Connection.RemotePort}");
    var form = await request.ReadFormAsync();
    if (!form.Keys.Contains("key")) return Results.BadRequest();
    string tempKey = BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(form["key"]))).Replace("-", "");
    if (tempKey != key) return Results.BadRequest();
    foreach (var file in form.Files)
    {
        string filePath = Path.Combine(path, file.FileName);
        using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }
    }
    return Results.Ok(form.Files.First().FileName);
}).Accepts<IFormFile>("multipart/form-data");



app.MapPost("/set", (IHttpContextAccessor _httpContextAccessor, string old_key, string new_key) =>
{
    if (BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(old_key))).Replace("-", "") == key)
    {
        key = BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(new_key))).Replace("-", "");
        return Results.Ok(key);
    }
    return Results.BadRequest();
});

app.Run();


