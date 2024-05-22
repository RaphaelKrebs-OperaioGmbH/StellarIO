using System.ComponentModel.DataAnnotations.Schema;

namespace StellarIO.Models
{
    public class Fleet
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("OriginPlanet")]
        public int OriginPlanetId { get; set; }
        public Planet OriginPlanet { get; set; }

        [ForeignKey("DestinationPlanet")]
        public int DestinationPlanetId { get; set; }
        public Planet DestinationPlanet { get; set; }

        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
    }
}