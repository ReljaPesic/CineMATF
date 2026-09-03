using Reservation.API.Data;
using Reservation.API.ExternalServices;
using Reservation.API.Mapping;
using Reservation.API.Settings;
using Reservation.API.Repositories;
using Reservation.API.Services;
using Reservation.API.Services.Email;
using Reservation.API.Services.Pricing;
using Reservation.API.Services.Tickets;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Screening.API.Grpc;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
// CORS: let the Angular dev server (http://localhost:4200) call this API from the
// browser
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition"));
});
builder.Services.Configure<ReservationOptions>(builder.Configuration.GetSection("ReservationOptions"));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

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
builder.Services.AddHttpClient<IIdentityApiClient, IdentityApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["IdentityApi:BaseUrl"]
        ?? throw new InvalidOperationException("IdentityApi:BaseUrl is not configured"));
});
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddSingleton<ITicketPdfGenerator, TicketPdfGenerator>();
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
    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
    }
}

app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
