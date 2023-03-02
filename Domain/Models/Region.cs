namespace Domain.Models;

public class Region : BaseEntity
{
    public Region(string name)
    {
        Name = name;
    }

    public string Name { get; private set; }
    public List<City> Cities { get; set; } = new List<City>();
}

public class City : BaseEntity
{
    public City(string name, Guid regionId)
    {
        Name = name;
        RegionId = regionId;
    }

    public Guid RegionId { get; private set; }
    public string Name { get; private set; }
    public List<Mosque> Mosques { get; private set; } = new List<Mosque>();
}
