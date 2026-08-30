using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Screening.API.Data;
using Screening.API.Grpc;
using Screening.API.Mapping;
using Screening.API.Repositories;
using Screening.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080, listenOptions => listenOptions.Protocols = HttpProtocols.Http1);
    options.ListenAnyIP(8081, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
});

builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddGrpc();
builder.Services.AddSingleton<IScreeningContext, ScreeningDbContext>();
builder.Services.AddScoped<IScreeningRepository, ScreeningRepository>();
builder.Services.AddScoped<IScreeningService, ScreeningService>();
builder.Services.AddAutoMapper(typeof(ScreeningMappingProfile));
builder.Services.AddHostedService<DataSeeder>();

var app = builder.Build();

await app.EnsureCreatedAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapGrpcService<ScreeningGrpcService>();
app.Run();
