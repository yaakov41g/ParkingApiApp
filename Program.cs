using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MongoDB.Driver;
using ParkingApiApp.Data;
using ParkingApiApp.Models;
using ParkingApiApp.Services;
using ParkingApiApp.Utilities;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "Secrets/parkingapp-473913-84210e5927a9.json");

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();//Envoke
builder.Services.AddSwaggerGen();
var mongoConnection = builder.Configuration.GetConnectionString("MongoDb");
builder.Services.AddSingleton<IMongoClient>(new MongoClient(mongoConnection));
builder.Services.AddSingleton<CityCollectionAccess>();
builder.Services.AddSingleton<VoiceGeneratorService>();
builder.Services.AddSingleton<SpeechToTextService>(); 
builder.Services.AddSingleton<TexToSpeechService>();
builder.Services.AddSingleton<CityService>();
builder.Services.AddSingleton<TranslationService>();
builder.Services.AddScoped<AudioConversionService>();

// register redis connection
var redis = ConnectionMultiplexer.Connect("localhost:6379,abortConnect=false");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
/// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:8081") // Your Expo Web origin
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddSingleton<IMongoCollection<City>>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var db = client.GetDatabase("ParkingDB"); // Make sure this matches your actual DB name
    return db.GetCollection<City>("cities");  // Make sure this matches your actual collection name
});

var app = builder.Build();
// Use CORS
app.UseCors("AllowLocalhost");
using (var scope = app.Services.CreateScope())
{
    var mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
    var db = mongoClient.GetDatabase("ParkingDB");
    await Seeder.SeedCitiesAsync(db.GetCollection<City>("cities"));
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles(); // This enables access to /audio/welcome_message.wav

app.UseAuthorization();

app.MapControllers();

app.Run();
