using System.Collections.Generic;

public class BuildViewModel
{
    public int PlanetId { get; set; }
    public string SelectedBuilding { get; set; }
    public List<BuildingOption> AvailableBuildings { get; set; }
    public int Duration { get; set; }
    public int IronCost { get; set; }
    public int SilverCost { get; set; }
    public int AluminiumCost { get; set; }
    public int H2Cost { get; set; }
    public int EnergyCost { get; set; }
}


public class BuildingOption
{
    public string Name { get; set; }
    public int Duration { get; set; }
    public int IronCost { get; set; }
    public int SilverCost { get; set; }
    public int AluminiumCost { get; set; }
    public int H2Cost { get; set; }
    public int EnergyCost { get; set; }
}
