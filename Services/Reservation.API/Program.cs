using Reservation.API.Data;
using Reservation.API.ExternalServices;
using Reservation.API.Mapping;
using Reservation.API.Settings;
using Reservation.API.Repositories;
using Reservation.API.Services;
using Reservation.API.Services.Pricing;
using Microsoft.EntityFrameworkCore;
using Screening.API.Grpc;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.Configure<ReservationOptions>(builder.Configuration.GetSection("ReservationOptions"));
builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHttpClient<ICinemaApiClient, CinemaApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CinemaApi:BaseUrl"]
        ?? throw new InvalidOperationException("CinemaApi:BaseUrl is not configured"));
});
builder.Services.AddGrpcClient<ScreeningGrpc.ScreeningGrpcClient>(o =>
{
    o.Address = new Uri(builder.Configuration["ScreeningApi:BaseUrl"]
        ?? throw new InvalidOperationException("ScreeningApi:BaseUrl is not configured"));
});
builder.Services.AddScoped<IScreeningApiClient, ScreeningApiClient>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddSingleton<ITicketPricingService, TicketPricingService>();
builder.Services.AddSingleton<IReservationFactory, ReservationFactory>();
builder.Services.AddAutoMapper(typeof(ReservationMappingProfile));
builder.Services.AddHostedService<ReservationCleanupService>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<DataSeeder>();
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReservationDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
