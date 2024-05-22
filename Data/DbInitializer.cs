using Microsoft.AspNetCore.Identity;
using StellarIO.Models;

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

            var userPlanets = context.Planets.OrderBy(p => rng.Next()).Take(3).ToList();
            foreach (var planet in userPlanets)
            {
                planet.UserId = testUser.Id;
                planet.User = testUser;
            }
            await context.SaveChangesAsync();

            logger.LogInformation("Assigned 3 planets to test user.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                logger.LogError("Error creating test user: {Error}", error.Description);
            }
        }
    }
}
