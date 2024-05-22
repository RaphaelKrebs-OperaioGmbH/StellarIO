using System.Numerics;

namespace StellarIO.Models
{
    public class GalaxySystem
    {
        public int Id { get; set; }
        public int GalaxyId { get; set; }
        public Galaxy Galaxy { get; set; }
        public List<Planet> Planets { get; set; } = new List<Planet>();
    }
}