using MongoDB.Driver;
using ParkingApiApp.Models;
using System.Text.Json;
namespace ParkingApiApp.Utilities
{
    public class Seeder
    {
        public static async Task SeedCitiesAsync(IMongoCollection<City> cityCollection)
        {
            string json = await File.ReadAllTextAsync("cities.json");
            //var cities = JsonSerializer.Deserialize<List<City>>(json);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var cities = JsonSerializer.Deserialize<List<City>>(json, options);


            if (cities != null && cities.Count > 0)
            {
                var existing = await cityCollection.CountDocumentsAsync(_ => true);// Check if there are existing documents in the collection
                if (existing == 0)
                {
                    await cityCollection.InsertManyAsync(cities);
                }
            }
        }
    }
}
