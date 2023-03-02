namespace Domain.Models;

public class Worker : BaseInfoEntity
{
    public Worker(string name, string phone, string pin, Guid mosqueId)
    {
        Name = name;
        Phone = phone;
        Pin = pin;
        MosqueId = mosqueId;
    }

    public Guid MosqueId { get; private set; }
    public Mosque Mosque { get; private set; }
    public DateTime? RegDate { get; set; } = null;

    public void SetMosque(Mosque mosque)
    {
        MosqueId = mosque.Id;
        Mosque = mosque;
    }
}