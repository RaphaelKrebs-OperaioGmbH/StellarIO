using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StellarIO.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DbInitializer
{
    public static async Task Initialize(ApplicationDbContext context, UserManager<User> userManager, ILogger<DbInitializer> logger)
    {
        context.Database.EnsureCreated();

        if (context.Galaxies.Any())
        {
            logger.LogInformation("Database already seeded.");
            return; // DB has been seeded
        }

        var rng = new Random();

        // Seed Galaxies, Systems, and Planets
        for (int g = 1; g <= 10; g++)
        {
            var galaxy = new Galaxy { Name = $"Galaxy {g}" };
            context.Galaxies.Add(galaxy);
            context.SaveChanges();

            for (int s = 1; s <= 150; s++)
            {
                var system = new GalaxySystem { Galaxy = galaxy };
                context.GalaxySystems.Add(system);
                context.SaveChanges();

                int planetCount = rng.Next(6, 13); // 6 to 12 planets
                for (int p = 1; p <= planetCount; p++)
                {
                    var planet = new Planet
                    {
                        Name = $"Planet {g}-{s}-{p}",
                        System = system,
                        RelativeSpeed = rng.Next(90, 111),
                        RelativeIronOutput = rng.Next(90, 111),
                        RelativeSilverOutput = rng.Next(90, 111),
                        RelativeAluminiumOutput = rng.Next(90, 111),
                        RelativeH2Output = rng.Next(90, 111),
                        RelativeEnergyOutput = rng.Next(90, 111),
                        Iron = 500,
                        Silver = 500,
                        Aluminium = 500,
                        H2 = 500,
                        Energy = 500,
                        UserId = null // Ensure planets are unassigned
                    };
                    context.Planets.Add(planet);
                }
                context.SaveChanges();
            }
        }

        logger.LogInformation("Galaxy and planets seeded successfully.");

        // Create test user and assign planets
        var testUser = new User { UserName = "testuser", Email = "testuser@example.com" };
        var result = await userManager.CreateAsync(testUser, "Test@2018!");

        if (result.Succeeded)
        {
            logger.LogInformation("Test user created successfully.");

            // Assign three planets to test user
            var availablePlanets = context.Planets.Where(p => p.UserId == null).Take(3).ToList();
            foreach (var planet in availablePlanets)
            {
                planet.UserId = testUser.Id;
                planet.User = testUser;
                context.Planets.Update(planet);

                // Add HQ building to each planet
                var hq = new Building
                {
                    Name = "HQ",
                    Level = 1,
                    PlanetId = planet.Id,
                    IronCost = 300,
                    SilverCost = 100,
                    AluminiumCost = 150,
                    H2Cost = 90,
                    EnergyCost = 90,
                    Duration = 10,
                    Description = "The Headquarters is the central building of your planet. It serves as the administrative hub and the nerve center of all operations. Upgrading the HQ enhances your overall efficiency and unlocks new technologies and buildings.",
                    Points = 20
                };
                context.Buildings.Add(hq);
            }
            context.SaveChanges();
            logger.LogInformation("Three planets assigned to test user.");
        }
        else
        {
            logger.LogError("Failed to create test user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
