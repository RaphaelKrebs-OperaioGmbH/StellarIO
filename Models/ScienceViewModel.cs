using Microsoft.AspNetCore.Mvc.Rendering;
using StellarIO.Models;

public class ScienceOption
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Level { get; set; }
    public int IronCost { get; set; }
    public int SilverCost { get; set; }
    public int AluminiumCost { get; set; }
    public int H2Cost { get; set; }
    public int EnergyCost { get; set; }
    public int Duration { get; set; }
    public string UserId { get; set; }
    public DateTime? ResearchStartTime { get; set; }
    public DateTime? ResearchEndTime { get; set; }
}


public class ScienceViewModel
{
    public List<ScienceOption> AvailableSciences { get; set; } = new List<ScienceOption>();
    public Science ActiveScience { get; set; } 
    public List<Planet> Planets { get; set; }
    public int SelectedPlanetId { get; set; } // Property to hold the selected planet ID
    public SelectList PlanetSelectList { get; set; } // Property to hold the planet select list


    // Planet resources
    public int PlanetIron { get; set; }
    public int PlanetSilver { get; set; }
    public int PlanetAluminium { get; set; }
    public int PlanetH2 { get; set; }
    public int PlanetEnergy { get; set; }
    public string PlanetCoordinates { get; set; }

}
