using Microsoft.EntityFrameworkCore;
using StellarIO.Models;

namespace StellarIO.Services
{
    public class BuildService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PlanetService> _logger;

        // Consider moving this data into config file
        private static readonly Dictionary<string, List<(string RequiredBuilding, int RequiredLevel)>> _buildingRequirements = new Dictionary<string, List<(string, int)>>()
        {
            { "Iron Mine", new List<(string, int)>
                {
                    ("HQ", 1)
                }
            },
            { "Silver Mine", new List<(string, int)>
                {
                    ("HQ", 2)
                }
            },
            { "Aluminum Mill", new List<(string, int)>
                {
                    ("HQ", 2)
                }
            },
            { "Fusion Reactor", new List<(string, int)>
                {
                    ("HQ", 3),
                    ("Iron Mine", 2),
                    ("Silver Mine", 2),
                    ("Aluminum Mill", 2),
                    ("H2 Condenser", 1)
                }
            },
            { "Shipyard", new List<(string, int)>
                {
                    ("HQ", 10),
                    ("Iron Mine", 5),
                    ("Silver Mine", 5),
                    ("Aluminum Mill", 3),
                    ("H2 Condenser", 4),
                    ("Fusion Reactor", 2)
                }
            },
            { "Research Center", new List<(string, int)>
                {
                    ("HQ", 3)
                }
            }
        };

        public BuildService(ApplicationDbContext context, ILogger<PlanetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public virtual async Task<Building?> GetBuildingAsync(int buildingId)
        {
            return await _context.Buildings.Include(b => b.Planet).FirstOrDefaultAsync(b => b.Id == buildingId);
        }

        public virtual async Task CancelBuildingAsync(int buildingId)
        {
            var building = await GetBuildingAsync(buildingId);
            if(building == null)
            {
                throw new KeyNotFoundException($"Building with Id {buildingId} was not found");
            }

            // Check if the building is currently under construction
            if (building.ConstructionEndTime > DateTime.UtcNow)
            {
                if (building.Level == 1)
                {
                    // If the building is new and has not completed its first level, remove it
                    _context.Buildings.Remove(building);
                }
                else
                {
                    // If the building is being upgraded, just cancel the upgrade
                    building.Level--;
                    building.ConstructionEndTime = null;
                }
            }
            else
            {
                throw new BadHttpRequestException($"Building with Id {buildingId} is not under construction");
            }
            await _context.SaveChangesAsync();
        }

        public static Building RecalculateCosts(Building buildingType, int level)
        {
            return new Building
            {
                IronCost = buildingType.IronCost * level,
                SilverCost = buildingType.SilverCost * level,
                AluminiumCost = buildingType.AluminiumCost * level,
                H2Cost = buildingType.H2Cost * level,
                EnergyCost = buildingType.EnergyCost * level,
                Duration = buildingType.Duration * level
            };
        }

        public static bool CheckBuildingRequirements(Planet planet, string buildingName)
        {
            if (_buildingRequirements.ContainsKey(buildingName))
            {
                foreach (var requirement in _buildingRequirements[buildingName])
                {
                    var existingBuilding = planet.Buildings.FirstOrDefault(b => b.Name == requirement.RequiredBuilding);
                    if (existingBuilding == null || existingBuilding.Level < requirement.RequiredLevel)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // Consider moving the info of this function into a config file, e.g. a JSON
        public static List<Building> GetBuildingTypes()
        {
            return new List<Building>
            {
                new Building
                {
                    Name = "HQ",
                    IronCost = 300,
                    SilverCost = 100,
                    AluminiumCost = 150,
                    H2Cost = 90,
                    EnergyCost = 100,
                    Duration = 10,
                    Description = "The Headquarters is the central building of your planet. It serves as the administrative hub and the nerve center of all operations. Upgrading the HQ enhances your overall efficiency and unlocks new technologies and buildings.",
                    Points = 20
                },
                new Building
                {
                    Name = "Iron Mine",
                    IronCost = 100,
                    SilverCost = 500,
                    AluminiumCost = 150,
                    H2Cost = 140,
                    EnergyCost = 140,
                    Duration = 60,
                    Description = "The Iron Mine extracts iron from the planet's crust. This essential resource is used for constructing various buildings and crafting items. Continuous upgrades increase extraction efficiency and output.",
                    Points = 10
                },
                new Building
                {
                    Name = "Silver Mine",
                    IronCost = 100,
                    SilverCost = 200,
                    AluminiumCost = 75,
                    H2Cost = 30,
                    EnergyCost = 30,
                    Duration = 60,
                    Description = "The Silver Mine extracts silver, a valuable resource used in advanced construction and technology development. Increasing the mine's capacity and efficiency ensures a steady supply of this precious metal.",
                    Points = 10
                },
                new Building
                {
                    Name = "Aluminum Mill",
                    IronCost = 300,
                    SilverCost = 100,
                    AluminiumCost = 50,
                    H2Cost = 500,
                    EnergyCost = 500,
                    Duration = 60,
                    Description = "The Aluminum Mill processes raw aluminum ore into usable metal, which is crucial for building lightweight structures and advanced technology. Upgrading the mill enhances processing speed and output.",
                    Points = 20
                },
                new Building
                {
                    Name = "H2 Condenser",
                    IronCost = 350,
                    SilverCost = 600,
                    AluminiumCost = 50,
                    H2Cost = 400,
                    EnergyCost = 400,
                    Duration = 60,
                    Description = "The H2 Condenser produces hydrogen fuel from water through a process of condensation and separation. Hydrogen is a key resource for energy production and advanced propulsion systems. Improvements to the condenser increase fuel yield and efficiency.",
                    Points = 30
                },
                new Building
                {
                    Name = "Fusion Reactor",
                    IronCost = 800,
                    SilverCost = 900,
                    AluminiumCost = 750,
                    H2Cost = 50,
                    EnergyCost = 50,
                    Duration = 60,
                    Description = "The Fusion Reactor generates vast amounts of energy by fusing hydrogen atoms. This advanced power source is essential for maintaining your colony's energy needs and supporting high-tech operations. Upgrading the reactor boosts energy output and stability.",
                    Points = 50
                },
                new Building
                {
                    Name = "Research Center",
                    IronCost = 300,
                    SilverCost = 100,
                    AluminiumCost = 150,
                    H2Cost = 90,
                    EnergyCost = 90,
                    Duration = 60,
                    Description = "The Research Center is where new technologies are developed and existing ones are enhanced. Scientists and engineers work tirelessly here to unlock advancements that drive your colony forward. Enhancements to the center accelerate research speed and unlock higher-tier technologies.",
                    Points = 30
                },
                new Building
                {
                    Name = "Shipyard",
                    IronCost = 18000,
                    SilverCost = 10000,
                    AluminiumCost = 15000,
                    H2Cost = 9000,
                    EnergyCost = 9000,
                    Duration = 60,
                    Description = "The Shipyard constructs and repairs spacecraft, enabling exploration and defense capabilities. This massive facility is crucial for expanding your reach across the stars and ensuring the safety of your fleet. Upgrading the shipyard reduces construction times and increases the size and complexity of ships that can be built.",
                    Points = 200
                }
            };
        }
    }
}
