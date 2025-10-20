//using MongoDB.Driver;

//namespace ParkingApiApp.Data
//{
//    public class ParkingCollectionAccess
//    {
//        private readonly IMongoDatabase _database;

//        public ParkingCollectionAccess(IConfiguration configuration)
//        {
//            // Read connection string and database name from appsettings.json
//            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
//            _database = client.GetDatabase(configuration["MongoDatabase"]);
//        }

        // Expose the Cities collection
        //public IMongoCollection<City> Cities => _database.GetCollection<City>("Cities");
   // }
//}
