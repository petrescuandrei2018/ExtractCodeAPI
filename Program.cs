using ExtractCodeAPI.Factory;
using ExtractCodeAPI.Services.Abstractions;
using ExtractCodeAPI.Services.Facade;
using ExtractCodeAPI.Services.Implementations;
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
});

// ✅ Înregistrare servicii și factory
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IFileExtractionService, FileExtractionService>();
builder.Services.AddScoped<ICodeGenerationService, CodeGenerationService>();
builder.Services.AddScoped<IFileDownloadService, FileDownloadService>();
builder.Services.AddSingleton<ILogService, LogService>();
builder.Services.AddSingleton<FileValidatorService>();

// ✅ Înregistrăm `ExtractFacade` cu interfața sa (ACUM ESTE CORECT)
builder.Services.AddScoped<IExtractFacade, ExtractFacade>();

builder.Services.AddScoped<IServiceFactory, ServiceFactory>();
builder.Services.AddScoped<IArchiveExtractor, ZipExtractor>();

var app = builder.Build();

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

// ✅ Middleware-uri corecte
app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.Run();
