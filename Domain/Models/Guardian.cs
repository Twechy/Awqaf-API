namespace Domain.Models;

public class Guardian : BaseInfoEntity
{
    public Guardian(string name, string phone, string pin)
    {
        Name = name;
        Phone = phone;
        Pin = pin;
        Students = new List<Student>();
    }

    public List<Student> Students { get; set; } = new();

    public void SetGuardianFamilyMembers(IEnumerable<Student> students)
    {
        Students.Clear();
        Students.AddRange(students);
    }
}