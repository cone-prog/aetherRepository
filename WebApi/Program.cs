using UrbanLayoutGenerator.Configuration;
using UrbanLayoutGenerator.Core.Services;
using UrbanLayoutGenerator.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<LibraryPassport>(_ => LibraryPassport.CreateDefault());
builder.Services.AddScoped<PdfProcessor>();
builder.Services.AddScoped<AdvancedSvgParser>();
builder.Services.AddScoped<PdfToSvgConverter>();
builder.Services.AddScoped<GeoJsonExporter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();

    var maxSize = 50L * 1024 * 1024;
    context.Request.Body = new MemoryStream((int)Math.Min(maxSize, int.MaxValue));
    await next();
});

app.Run();