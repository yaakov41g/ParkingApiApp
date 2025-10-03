using System.Text.Json;
using MongoDB.Driver;
using StackExchange.Redis;
using ParkingApiApp.Models;

public class CityService
{
    private readonly IMongoCollection<City> _cities;
    private readonly IDatabase _cache;

    public CityService(IMongoClient mongoClient, IConnectionMultiplexer redis)
    {
        var db = mongoClient.GetDatabase("ParkingDB");
        _cities = db.GetCollection<City>("cities");
        _cache = redis.GetDatabase();
    }

    /// <summary>
    /// Retrieves full city data from Redis if available.
    /// If Redis is empty, loads from MongoDB.
    /// If MongoDB is empty, loads from JSON and stores in both MongoDB and Redis.
    /// </summary>
    public async Task<List<City>> GetCitiesAsync()
    {
        // Step 1: Try to get cities from Redis
        var cached = await _cache.StringGetAsync("cities_full");
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<List<City>>(cached) ?? new List<City>();
        }

        // Step 2: Try to get cities from MongoDB
        var citiesInDb = await _cities.Find(_ => true).ToListAsync();
        if (citiesInDb.Count > 0)
        {
            // Cache the result in Redis
            await _cache.StringSetAsync("cities_full", JsonSerializer.Serialize(citiesInDb), TimeSpan.FromHours(1));
            return citiesInDb;
        }

        // Step 3: Load from external source (JSON file)
        var citiesFromJson = await LoadFromJsonAsync();

        // Save to MongoDB
        await _cities.InsertManyAsync(citiesFromJson);

        // Cache in Redis
        await _cache.StringSetAsync("cities_full", JsonSerializer.Serialize(citiesFromJson), TimeSpan.FromHours(1));

        return citiesFromJson;
    }

    /// <summary>
    /// Loads full city data from a local JSON file.
    /// </summary>
    private async Task<List<City>> LoadFromJsonAsync()
    {
        string json = await File.ReadAllTextAsync("Data/cities.json");
        return JsonSerializer.Deserialize<List<City>>(json) ?? new List<City>();
    }
}
