namespace Domain.Models;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public Active Active { get; protected set; }

    public void SetState()
    {
        if (Active == Active.Active) Active = Active.Inactive;
        else Active = Active.Active;
    }
}

public enum Active
{
    Inactive,
    Active
}

public class BaseInfoEntity : BaseEntity
{
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Pin { get; set; }
    public string Token { get; set; }
}