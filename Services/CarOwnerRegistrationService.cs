using MongoDB.Driver;
using Newtonsoft.Json;
using ParkingApiApp.Models;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

public class CarOwnerRegistrationService
{
    private readonly IMongoCollection<CarOwner> _carOwners;
    private readonly IDatabase _cache;
    private readonly ILogger<CarOwnerRegistrationService> _logger;

    public CarOwnerRegistrationService(IMongoClient mongoClient, IConnectionMultiplexer redis, ILogger<CarOwnerRegistrationService> logger)
    {
        var db = mongoClient.GetDatabase("ParkingDB");
        _carOwners = db.GetCollection<CarOwner>("car_owners");
        _cache = redis.GetDatabase();
        _logger = logger;
    }

    public async Task RegisterAsync(CarOwner owner)
    {
        await _carOwners.InsertOneAsync(owner);

        var cacheKey = $"carowner:{owner.CarNumber}";
        var json = JsonConvert.SerializeObject(owner);
        await _cache.StringSetAsync(cacheKey, json, TimeSpan.FromHours(1));

        _logger.LogInformation($"Car owner {owner.CarNumber} registered and cached.");
    }

    public async Task<CarOwner?> GetByCarNumberAsync(string carNumber)
    {
        var cacheKey = $"carowner:{carNumber}";
        var cached = await _cache.StringGetAsync(cacheKey);

        if (!cached.IsNullOrEmpty)
        {
            _logger.LogInformation($"Car owner {carNumber} retrieved from cache.");
            return JsonConvert.DeserializeObject<CarOwner>(cached);
        }

        var owner = await _carOwners.Find(o => o.CarNumber == carNumber).FirstOrDefaultAsync();

        if (owner != null)
        {
            await _cache.StringSetAsync(cacheKey, JsonConvert.SerializeObject(owner), TimeSpan.FromHours(1));
            _logger.LogInformation($"Car owner {carNumber} loaded from DB and cached.");
        }

        return owner;
    }
}
