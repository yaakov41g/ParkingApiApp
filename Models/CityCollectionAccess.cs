using MongoDB.Driver;
using ParkingApiApp.Models;

namespace ParkingApiApp.Data
{
    public class CityCollectionAccess
    {
        private readonly IMongoDatabase _database;

        public CityCollectionAccess(IConfiguration configuration)
        {
            // Read connection string and database name from appsettings.json
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            _database = client.GetDatabase(configuration["MongoDatabase"]);
        }

        // Expose the Cities collection
        public IMongoCollection<City> Cities => _database.GetCollection<City>("Cities");
    }
}
