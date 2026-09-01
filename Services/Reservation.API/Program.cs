using Reservation.API.Data;
using Reservation.API.ExternalServices;
using Reservation.API.Mapping;
using Reservation.API.Settings;
using Reservation.API.Repositories;
using Reservation.API.Services;
using Reservation.API.Services.Pricing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Screening.API.Grpc;
using System.Text;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.Configure<ReservationOptions>(builder.Configuration.GetSection("ReservationOptions"));

<<<<<<< ours
// JWT bearer
var jwt = builder.Configuration.GetSection("JwtSettings");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["SecretKey"]
                    ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured"))),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
=======
// CORS: let the Angular dev server (http://localhost:4200) call this API from
// the browser. Mirrors the policy in Cinema.API / Screening.API / Movie.API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader());
});
>>>>>>> theirs
builder.Services.AddDbContext<ReservationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHttpClient<ICinemaApiClient, CinemaApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CinemaApi:BaseUrl"]
        ?? throw new InvalidOperationException("CinemaApi:BaseUrl is not configured"));
});
builder.Services.AddHttpClient<IMovieApiClient, MovieApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["MovieApi:BaseUrl"]
        ?? throw new InvalidOperationException("MovieApi:BaseUrl is not configured"));
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

<<<<<<< ours
app.UseAuthentication();
app.UseAuthorization();
=======
app.UseCors("CorsPolicy");
>>>>>>> theirs

app.MapControllers();
app.Run();
