using Google.Api;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using ParkingApiApp.Configuration;
using ParkingApiApp.Models;
using ParkingApiApp.Services;
using ParkingApiApp.Services.Email;
using ParkingApiApp.Services.Hosted;
using ParkingApiApp.Utilities;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
//*** The line was changed to set the environment variable for Google credentials ***
//***Not tested fully yet
var credentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSession();
var mongoConnection = builder.Configuration.GetConnectionString("MongoDb");
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnection));

builder.Services.AddSingleton<SpeechToTextService>();
builder.Services.AddSingleton<TexToSpeechService>();
builder.Services.AddSingleton<CityService>();
builder.Services.AddSingleton<CitySeederService>();
builder.Services.AddSingleton<TranslationService>();
builder.Services.AddScoped<AudioConversionService>();
builder.Services.AddSingleton<CarOwnerRegistrationService>();
builder.Services.AddScoped<ParkingSessionService>();
builder.Services.AddHostedService<InvoiceScheduler>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ReportService>();
builder.Configuration.AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true);
builder.Services.AddSingleton(
    provider => builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>()!
);

// רישום EmailService
builder.Services.AddScoped<IEmailService, EmailService>();
// Redis
var redis = ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:8081")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Mongo collections
builder.Services.AddSingleton<IMongoCollection<City>>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var db = client.GetDatabase("ParkingDB");
    return db.GetCollection<City>("cities");
});
builder.Services.AddSingleton<IMongoCollection<ParkingSessionAux>>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var db = client.GetDatabase("ParkingDB");
    return db.GetCollection<ParkingSessionAux>("parking_sessions_aux");
});

var app = builder.Build();

// Seed cities
using (var scope = app.Services.CreateScope())
{
    var mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
    var db = mongoClient.GetDatabase("ParkingDB");
    await Seeder.SeedCitiesAsync(db.GetCollection<City>("cities"));
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSession();
app.UseCors("AllowLocalhost");
app.UseStaticFiles();
app.UseAuthorization();

// ✅ Top-level route registration
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.MapFallbackToController("Index", "Admin");

app.Run();
