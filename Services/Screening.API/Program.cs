using Microsoft.AspNetCore.Server.Kestrel.Core;
using Screening.API.Data;
using Screening.API.Grpc;
using Screening.API.Mapping;
using Screening.API.Repositories;
using Screening.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http1AndHttp2);
});

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddGrpc();
builder.Services.AddSingleton<IScreeningContext, ScreeningDbContext>();
builder.Services.AddScoped<IScreeningRepository, ScreeningRepository>();
builder.Services.AddScoped<IScreeningService, ScreeningService>();
builder.Services.AddAutoMapper(typeof(ScreeningMappingProfile));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapGrpcService<ScreeningGrpcService>();
app.Run();
