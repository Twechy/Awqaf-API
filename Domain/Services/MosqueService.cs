using Domain.Abstracts;
using Domain.Db;
using Domain.Models;

namespace Domain.Services;

public interface IMosqueService : IManagementService<Mosque>
{
}

public class MosqueService : ManagementService<Mosque>, IMosqueService
{
    public MosqueService(AwqafDb dataContext) : base(dataContext)
    {
    }
}