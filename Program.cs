using ExtractCodeAPI.Factory;
using ExtractCodeAPI.Services.Abstractions;
using ExtractCodeAPI.Services.Facade;
using ExtractCodeAPI.Services.Implementations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Adăugăm suport pentru controlere
builder.Services.AddControllers();

// ✅ Configurare Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ExtractCodeAPI",
        Version = "v1",
        Description = "API pentru extracția și procesarea fișierelor cod sursă din arhive"
    });

    // ✅ Elimină opțiunea "Send empty value" prin specificarea tipului explicit
    options.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary",
        Nullable = false  // ✅ Obligă utilizatorul să încarce un fișier
    });
});

// ✅ Activăm suportul pentru WebSockets
builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromMinutes(2); // 🔹 Menține conexiunea activă
});

// ✅ Înregistrare servicii și factory
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileExtractionService, FileExtractionService>();
builder.Services.AddScoped<ICodeGenerationService, CodeGenerationService>();
builder.Services.AddScoped<IFileDownloadService, FileDownloadService>();
builder.Services.AddSingleton<ILogService, LogService>();
builder.Services.AddSingleton<FileValidatorService>();
builder.Services.AddScoped<ICodeExtractorService, CodeExtractorService>();


// ✅ Înregistrăm `ExtractFacade` cu interfața sa (ACUM ESTE CORECT)
builder.Services.AddScoped<IExtractFacade, ExtractFacade>();

builder.Services.AddScoped<IServiceFactory, ServiceFactory>();
builder.Services.AddScoped<IArchiveExtractor, ZipExtractor>();

// ✅ Înregistrăm serviciul WebSocket injectabil
builder.Services.AddScoped<IWebSocketHandler, WebSocketHandler>();

// ✅ Configurare pentru upload de fișiere mari (5GB)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 5L * 1024 * 1024 * 1024; // 5GB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5L * 1024 * 1024 * 1024; // 5GB
});

var app = builder.Build();

app.UseWebSockets(); // ✅ Activează WebSockets

// ✅ Middleware pentru WebSockets
app.Use(async (context, next) =>
{
    var webSocketHandler = context.RequestServices.GetRequiredService<IWebSocketHandler>();

    if (context.Request.Path == "/api/upload/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await webSocketHandler.HandleWebSocket(webSocket);
            return;
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }

    await next();
});

// ✅ Mapăm controloarele API
app.MapControllers();

// ✅ Activăm Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ExtractCodeAPI v1");
        options.RoutePrefix = ""; // ✅ Face ca Swagger să fie pagina implicită
    });
}

app.Run();
