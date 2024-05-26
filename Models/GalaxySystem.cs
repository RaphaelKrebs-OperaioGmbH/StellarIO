using System.Numerics;
using System.Text.Json.Serialization;

namespace StellarIO.Models
{
    public class GalaxySystem
    {
        public int Id { get; set; }
        public int GalaxyId { get; set; }
        [JsonIgnore]
        public Galaxy Galaxy { get; set; }
        public List<Planet> Planets { get; set; } = new List<Planet>();
    }
}