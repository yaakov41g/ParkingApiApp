namespace ParkingApiApp.Services
{
    using MongoDB.Driver;
    using Microsoft.Extensions.Logging;
    using ParkingApiApp.Models;

    public class ParkingSessionService
    {
        private readonly IMongoCollection<CarOwner> _carOwners;
        private readonly IMongoCollection<ParkingSession> _sessions;
        private readonly IMongoCollection<ParkingSessionAux> _auxCollection;
        private readonly ILogger<ParkingSessionService> _logger;

        public ParkingSessionService(IMongoClient mongoClient, ILogger<ParkingSessionService> logger, IMongoCollection<ParkingSessionAux> auxCollection )
        {
            var db = mongoClient.GetDatabase("ParkingDB");
            _carOwners = db.GetCollection<CarOwner>("car_owners");
            _sessions = db.GetCollection<ParkingSession>("parking_sessions");
            _logger = logger;
            _auxCollection = auxCollection;
        }

        public async Task<bool> RegisterSessionAsync(ParkingSessionRequest request)
        {
            // Step 1: Find car owner by phone number
            var owner = await _carOwners.Find(o => o.PhoneNumber == request.PhoneNumber).FirstOrDefaultAsync();

            if (owner == null)
            {
                _logger.LogWarning($"No car owner found with phone number {request.PhoneNumber}");
                return false;
            }

            // Step 2: Create session object
            var session = new ParkingSession
            {
                CarNumber = owner.CarNumber,
                City = request.City,
                Zone = request.Zone,
                StartParkingTime = request.StartTime,
                EndParkingTime = null,
            };
            var auxSession = new ParkingSessionAux
            {
                CarNumber = owner.CarNumber,
                StartParkingTime = request.StartTime,
                EndParkingTime = null,
            };

            // Step 3: Insert into MongoDB
            await _sessions.InsertOneAsync(session);       // Full session with context
            await _auxCollection.InsertOneAsync(auxSession); // Minimal session for history

            //_logger.LogInformation($"Parking session for {session.CarNumber} registered in {session.City}/{session.Zone} from {session.StartParkingTime} to {session.EndParkingTime}");

            return true;
        }
        public async Task<bool> RegisterEndTimeAsync(string carNumber, DateTime startTime, DateTime endTime)
        {
            var filter = Builders<ParkingSessionAux>.Filter.And(
                Builders<ParkingSessionAux>.Filter.Eq(x => x.CarNumber, carNumber),
                Builders<ParkingSessionAux>.Filter.Eq(x => x.StartParkingTime, startTime)
            );

            var update = Builders<ParkingSessionAux>.Update.Set(x => x.EndParkingTime, endTime);

            var result = await _auxCollection.UpdateOneAsync(filter, update);

            return result.ModifiedCount > 0;
        }

    }
}
