using Microsoft.OpenApi.Models;
using ExtractCodeAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ Adăugăm serviciile necesare
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Extract Code API",
        Version = "v1",
        Description = "API pentru extragerea codului sursă din arhive ZIP",
        Contact = new OpenApiContact
        {
            Name = "Suport",
            Email = "suport@example.com",
            Url = new Uri("https://github.com")
        }
    });
});

// ✅ Adăugăm serviciile noastre
builder.Services.AddSingleton<FileService>();
builder.Services.AddSingleton<ExtractService>();
builder.Services.AddSingleton<LogService>();

var app = builder.Build();

// ✅ Activăm Swagger doar în Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Extract Code API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// ✅ Log după pornirea aplicației
Console.WriteLine("✅ API-ul rulează la: https://localhost:7033");
Console.WriteLine("✅ Poți accesa Swagger la: https://localhost:7033/swagger");

app.Run();
