using System.Text.Json.Serialization;

namespace StellarIO.Models
{
    public class Science
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserId { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        public int Level { get; set; }
        public int IronCost { get; set; }
        public int SilverCost { get; set; }
        public int AluminiumCost { get; set; }
        public int H2Cost { get; set; }
        public int EnergyCost { get; set; }
        public int Duration { get; set; } // Duration in seconds
        public DateTime? ResearchStartTime { get; set; }
        public DateTime? ResearchEndTime { get; set; }
        public string Description { get; set; }
    }
}
