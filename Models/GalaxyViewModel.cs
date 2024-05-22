public class GalaxyViewModel
{
    public string Name { get; set; }
    public List<GalaxySystemViewModel> Systems { get; set; }
}

public class GalaxySystemViewModel
{
    public int Id { get; set; }
    public List<PlanetViewModel> Planets { get; set; }
}

public class PlanetViewModel
{
    public string Name { get; set; }
    public string Owner { get; set; }
    public string Coordinates { get; set; }
}
