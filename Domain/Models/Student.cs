namespace Domain.Models;

public class Student : BaseInfoEntity
{
    public Student(string name, string phone, string pin, Guid mosqueId)
    {
        Name = name;
        Phone = phone;
        Pin = pin;
        MosqueId = mosqueId;
    }

    public void SetGuardianId(Guid guardianId) => GuardianId = guardianId;

    public Guid MosqueId { get; private set; }
    public Guid? GuardianId { get; private set; }
    public Guid MemorizationId { get; private set; }
    public Memorization Memorization { get; private set; }
}