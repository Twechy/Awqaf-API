namespace Domain.Models;

public class Mosque : BaseEntity
{
    public Mosque()
    {
    }

    public Mosque(string name, string description, Location location)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CreatedAt = DateTime.Now;
        Location = location;
    }

    public string Name { get; private set; }
    public string Description { get; private set; }
    public Location Location { get; private set; }
    public Guid CityId { get; private set; }
    public List<Worker> Workers { get; private set; } = new List<Worker>();
    public List<Guardian> Guardians { get; private set; } = new List<Guardian>();
    public List<Student> Students { get; private set; } = new List<Student>();
}