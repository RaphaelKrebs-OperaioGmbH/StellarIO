using StellarIO.Models;

public class Building
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int PlanetId { get; set; }
    public Planet Planet { get; set; }
    public int IronCost { get; set; }
    public int SilverCost { get; set; }
    public int AluminiumCost { get; set; }
    public int H2Cost { get; set; }
    public int EnergyCost { get; set; }
    public int Points { get; set; }
    public DateTime? ConstructionEndTime { get; set; }
    public DateTime? ConstructionStartTime { get; set; }
    public int Duration { get; set; } // Add Duration property
    public string Description { get; set; } // Add Description property

}
