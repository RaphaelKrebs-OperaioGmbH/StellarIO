using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StellarIO.Models
{
    public class Planet
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SystemId { get; set; }
        [JsonIgnore]
        public GalaxySystem System { get; set; } = default!;
        public string? UserId { get; set; } // Make UserId nullable
        [JsonIgnore]
        public User? User { get; set; } = default!;

        // Properties related to resource generation
        public int RelativeSpeed { get; set; }
        public int RelativeIronOutput { get; set; }
        public int RelativeSilverOutput { get; set; }
        public int RelativeAluminiumOutput { get; set; }
        public int RelativeH2Output { get; set; }
        public int RelativeEnergyOutput { get; set; }

        public int Iron { get; set; } = 500;
        public int Silver { get; set; } = 500;
        public int Aluminium { get; set; } = 500;
        public int H2 { get; set; } = 500;
        public int Energy { get; set; } = 500;
        public List<Building> Buildings { get; set; } = new List<Building>();

        // Coordinates property
        public string Coordinates
        {
            get { return $"{System.GalaxyId}:{System.Id}:{Id}"; }
        }
    }
}
