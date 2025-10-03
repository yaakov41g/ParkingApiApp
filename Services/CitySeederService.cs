using MongoDB.Driver;
using ParkingApiApp.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace ParkingApiApp.Services
{
    public class CitySeederService
    {
        private readonly IMongoCollection<City> _cities;
        private readonly IDatabase _cache;

        public CitySeederService(IMongoClient mongoClient, IConnectionMultiplexer redis)
        {
            var db = mongoClient.GetDatabase("ParkingDB");
            _cities = db.GetCollection<City>("cities");
            _cache = redis.GetDatabase();
        }

        public async Task<string> ResetAndSeedAsync(string source = "api")
        {
            // Clear MongoDB
            await _cities.DeleteManyAsync(_ => true);

            // Clear Redis
            await _cache.KeyDeleteAsync("cities");

            List<City> cities;

            if (source == "redis")
            {
                var cached = await _cache.StringGetAsync("cities");
                if (cached.HasValue)
                {
                    cities = JsonSerializer.Deserialize<List<City>>(cached);
                    await _cities.InsertManyAsync(cities);
                    return "✅ Loaded from Redis";
                }
                return "⚠️ Redis was empty";
            }

            if (source == "db")
            {
                cities = await _cities.Find(_ => true).ToListAsync();
                return cities.Count > 0 ? "✅ Already in DB" : "⚠️ DB was empty";
            }

            // Default: Load from mock API or file
            cities = await LoadFromJsonAsync(); // or FetchFromApi()
            await _cities.InsertManyAsync(cities);
            await _cache.StringSetAsync("cities", JsonSerializer.Serialize(cities));
            return "✅ Loaded from API Storage";
        }

        private async Task<List<City>> LoadFromJsonAsync()
        {
            string json = await File.ReadAllTextAsync("Data/cities.json");
            return JsonSerializer.Deserialize<List<City>>(json) ?? new List<City>();
        }
    }
}
