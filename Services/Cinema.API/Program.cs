using Cinema.API.Converters;
using Cinema.API.Data;
using Cinema.API.Entities;
using Cinema.API.Mapping;
using Cinema.API.Repositories;
using Cinema.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new CityEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter<SeatType>());
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddDbContext<CinemaDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<ICinemaRepository, CinemaRepository>();
builder.Services.AddScoped<ICinemaService, CinemaService>();
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddAutoMapper(typeof(CinemaMappingProfile));
builder.Services.AddAutoMapper(typeof(HallMappingProfile));

var app = builder.Build();
app.UseCors("CorsPolicy");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    await app.SeedDataAsync();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
