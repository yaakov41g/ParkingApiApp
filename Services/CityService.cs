using MongoDB.Driver;
using ParkingApiApp.Controllers;
using ParkingApiApp.Models;
using ParkingApiApp.Services;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;
public class CityService
{
    private List<City>? _cachedCities;
    private readonly TranslationService _translationService;
    private readonly IMongoCollection<City> _cities;
    private readonly IDatabase _cache;
    private readonly ILogger<CityService> _logger;
    public CityService(IMongoClient mongoClient, IConnectionMultiplexer redis, TranslationService translationService,
                ILogger<CityService> logger)
    {
        var db = mongoClient.GetDatabase("ParkingDB");
        _cities = db.GetCollection<City>("cities");
        _cache = redis.GetDatabase();
        _translationService = translationService;   
        _logger = logger;   
    }

    /// <summary>
    /// Retrieves full city data from Redis if available.
    /// If Redis is empty, loads from MongoDB.
    /// If MongoDB is empty, loads from JSON and stores in both MongoDB and Redis.
    /// </summary>
    public async Task<List<City>> GetCitiesAsync()
    {
     // Step 1: Try to get cities from Redis
        if (_cachedCities != null)
            return _cachedCities;
        var cached = await _cache.StringGetAsync("cities_full");
        if (cached.HasValue)
        {
            _cachedCities = JsonSerializer.Deserialize<List<City>>(cached) ?? new List<City>();
            return _cachedCities;
        }

        // Step 2: Try to get cities from MongoDB
        var citiesInDb = await _cities.Find(_ => true).ToListAsync();
        if (citiesInDb.Count > 0)
        {
            // Cache the result in Redis
            await _cache.StringSetAsync("cities_full", JsonSerializer.Serialize(citiesInDb), null);
            return citiesInDb;
        }

        // Step 3: Load from external source (JSON file)
        var citiesFromJson = await LoadFromJsonAsync();

        // Save to MongoDB
        await _cities.InsertManyAsync(citiesFromJson);

        // Cache in Redis
        await _cache.StringSetAsync("cities_full", JsonSerializer.Serialize(citiesFromJson), null);

        return citiesFromJson;
    }
    public async Task<City?> FindCity(string inputName)
    {
        _cachedCities = await GetCitiesAsync();
        _logger.LogInformation($"###### CachedCities  "+ _cachedCities); 

        if (_cachedCities == null || !_cachedCities.Any())
            return null; // Or throw, or load from Redis if needed

        string englishName = await _translationService.TranslateHebrewToEnglishAsync(inputName);
        return _cachedCities.FirstOrDefault(c =>
            c.Name.Equals(englishName, StringComparison.OrdinalIgnoreCase));
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
