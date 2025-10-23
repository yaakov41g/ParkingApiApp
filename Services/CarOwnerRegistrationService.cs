using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using ParkingApiApp.Models;

public class CarOwnerRegistrationService
{
    private readonly IMongoCollection<CarOwner> _carOwners;
    private readonly ILogger<CarOwnerRegistrationService> _logger;

    public CarOwnerRegistrationService(IMongoClient mongoClient, ILogger<CarOwnerRegistrationService> logger)
    {
        var db = mongoClient.GetDatabase("ParkingDB");
        _carOwners = db.GetCollection<CarOwner>("car_owners");
        _logger = logger;
    }

    public async Task RegisterAsync(CarOwner owner)
    {
        await _carOwners.InsertOneAsync(owner);
        _logger.LogInformation($"Car owner {owner.CarNumber} registered.");
    }

    public async Task<CarOwner?> GetByCarNumberAsync(string carNumber)
    {
        var owner = await _carOwners.Find(o => o.CarNumber == carNumber).FirstOrDefaultAsync();

        if (owner != null)
        {
            _logger.LogInformation($"Car owner {carNumber} loaded from DB.");
        }
        else
        {
            _logger.LogWarning($"Car owner {carNumber} not found in DB.");
        }

        return owner;
    }
    public async Task<CarOwner?> GetByPhoneNumberAsync(string phoneNumber)
    {
        var owner = await _carOwners.Find(o => o.PhoneNumber == phoneNumber).FirstOrDefaultAsync();

        if (owner != null)
        {
            _logger.LogInformation($"Car owner with phone {phoneNumber} loaded from DB.");
        }
        else
        {
            _logger.LogWarning($"Car owner with phone {phoneNumber} not found in DB.");
        }

        return owner;
    }

}
