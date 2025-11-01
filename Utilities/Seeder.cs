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
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var cities = JsonSerializer.Deserialize<List<City>>(json, options);

            if (cities != null && cities.Count > 0)
            {
                var existing = await cityCollection.CountDocumentsAsync(_ => true);
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
            if (existingRates > 0) return;

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

        public static async Task SeedCarOwnersAsync(IMongoCollection<CarOwner> carOwnerCollection, ILogger _logger)
        {
            File.AppendAllText("seed_log.txt", $"SeedCarOwnersAsync called at {DateTime.Now}\n");
            var existingOwners = await carOwnerCollection.CountDocumentsAsync(_ => true);
            if (existingOwners > 0) return;

            var owners = new List<CarOwner>
    {
        new CarOwner {
            Name = "יעקב וויס",
            PhoneNumber = "0521234567",
            CarNumber = "98765432",
            Email = "yaakov41g@gmail.com",
            IdNumber = "301112222",
            BankNumber = "12",
            AccountNumber = "12345678",
            CreditNumber = "4580123456789012"
        },
        new CarOwner {
            Name = "משה כהן",
            PhoneNumber = "0529999999",
            CarNumber = "111111111",
            Email = "moshe@walla.com",
            IdNumber = "302223333",
            BankNumber = "10",
            AccountNumber = "87654321",
            CreditNumber = "4580123499990000"
        },
        new CarOwner {
            Name = "איילת השחר",
            PhoneNumber = "0528885555",
            CarNumber = "88855522",
            Email = "ayelet@outlook.co.il",
            IdNumber = "303334444",
            BankNumber = "11",
            AccountNumber = "11223344",
            CreditNumber = "4580123488887777"
        },
        new CarOwner {
            Name = "ישראל לוי",
            PhoneNumber = "05677722222",
            CarNumber = "13311331",
            Email = "israel@walla.com",
            IdNumber = "304445555",
            BankNumber = "13",
            AccountNumber = "55667788",
            CreditNumber = "4580123477776666"
        },
        new CarOwner {
            Name = "נטליה רוסקיה",
            PhoneNumber = "05311133322",
            CarNumber = "4422444",
            Email = "natalya@012.net.il",
            IdNumber = "305556666",
            BankNumber = "14",
            AccountNumber = "99887766",
            CreditNumber = "4580123466665555"
        }
    };

            await carOwnerCollection.InsertManyAsync(owners);
        }
    }
}
