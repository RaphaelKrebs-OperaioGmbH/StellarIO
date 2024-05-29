namespace StellarIO.Models
{
    public class Galaxy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<GalaxySystem> Systems { get; set; } = new List<GalaxySystem>();
    }

}