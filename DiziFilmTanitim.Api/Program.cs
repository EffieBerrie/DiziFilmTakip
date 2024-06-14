using Microsoft.EntityFrameworkCore;
using DiziFilmTanitim.Api.Data;
using DiziFilmTanitim.Api.Services;
using DiziFilmTanitim.Core.Interfaces;
using DiziFilmTanitim.Api.Interfaces;
using DiziFilmTanitim.Api.Endpoints;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servislerin kaydedilmesi
builder.Services.AddScoped<IFilmService, FilmService>();
builder.Services.AddScoped<IDiziService, DiziService>();
builder.Services.AddScoped<ITurService, TurService>();
builder.Services.AddScoped<IKullaniciService, KullaniciService>();
builder.Services.AddScoped<IYonetmenService, YonetmenService>();
builder.Services.AddScoped<IOyuncuService, OyuncuService>();
builder.Services.AddScoped<IKullaniciListesiService, KullaniciListesiService>();
builder.Services.AddScoped<IUploadService, UploadService>();

// JSON Serialization ayarları - Circular reference handling
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

// Swagger/OpenAPI için servisleri ekle
builder.Services.AddEndpointsApiExplorer(); // Minimal API'ler için gerekli
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();

// Endpointlerin kaydedilmesi
app.MapKullaniciEndpoints();
app.MapFilmEndpoints();
app.MapDiziEndpoints();
app.MapTurEndpoints();
app.MapYonetmenEndpoints();
app.MapOyuncuEndpoints();
app.MapKullaniciListesiEndpoints();
app.MapUploadEndpoints();

app.Run();
