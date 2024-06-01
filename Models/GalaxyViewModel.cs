public class GalaxyViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<GalaxySystemViewModel> Systems { get; set; }
}

public class GalaxySystemViewModel
{
    public int Id { get; set; }
    public IEnumerable<PlanetViewModel> Planets { get; set; }
}

public class PlanetViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Owner { get; set; }
    public string Coordinates { get; set; }
}
