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
        public static async Task SeedCityZoneRatesAsync(IMongoCollection<City> cityCollection,
            IMongoCollection<CityZoneRate> rateCollection)
        {
            var cities = await cityCollection.Find(_ => true).ToListAsync();

            var existingRates = await rateCollection.CountDocumentsAsync(_ => true);
            if (existingRates > 0) return; // Already seeded

            var rates = new List<CityZoneRate>();
            var random = new Random();
            var possibleRates = new[] { 5m, 7m, 10m, 12m };

            foreach (var city in cities)
            {
                foreach (var zone in city.Zones)
                {
                    var selectedRate = possibleRates[random.Next(possibleRates.Length)];

                    rates.Add(new CityZoneRate
                    {
                        CityName = city.Name,
                        ZoneName = zone,
                        HourlyRate = selectedRate
                    });
                }
            }

            if (rates.Count > 0)
            {
                await rateCollection.InsertManyAsync(rates);
            }
        }

    }
}
