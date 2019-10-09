using CityInfo.API.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API
{
    public static class CityInfoContextExtensions
    {
        public static void EnsureSeedDataForContext(this CityInfoContext context)
        {
            if (context.Cities.Any())
            {

            }

            var cities = new List<City>()
            {
                new City()
                {
                    Name = "New York",
                    Description = "The big one",
                    PointsOfInterest = new List<PointOfInterest>()
                    {
                        new PointOfInterest()
                        {
                            Name = "Central Park",
                            Description = "Teste 123"
                        },
                       new PointOfInterest()
                        {
                            Name = "Empire State Building",
                            Description = "Teste"
                        }
                    }
                },
                 new City()
                {
                    Name = "Braga",
                    Description = "The big one",
                    PointsOfInterest = new List<PointOfInterest>()
                    {
                        new PointOfInterest()
                        {
                            Name = "Sameiro",
                            Description = "Teste 123"
                        },
                       new PointOfInterest()
                        {
                            Name = "Good Jesus",
                            Description = "Teste"
                        }
                    }
                }
            };

            context.Cities.AddRange(cities);
            context.SaveChanges();
        }
    }
}
