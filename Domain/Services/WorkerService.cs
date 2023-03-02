using Domain.Abstracts;
using Domain.Db;
using Domain.Models;

namespace Domain.Services;

public interface IWorkerService : IManagementService<Worker>
{
    Task Register(string workerId, string mosqueId, CancellationToken cancellationToken = default);
}

public class WorkerService : ManagementService<Worker>, IWorkerService
{
    private readonly IMosqueService mosqueService;

    public WorkerService(AwqafDb dataContext, IMosqueService mosqueService) : base(dataContext)
    {
        this.mosqueService = mosqueService;
    }

    public async Task Register(string workerId, string mosqueId, CancellationToken cancellationToken = default)
    {
        var mosque = await mosqueService.Get(Guid.Parse(mosqueId), cancellationToken);

        var worker = await Get(Guid.Parse(workerId), cancellationToken);

        worker.SetMosque(mosque);

        await Save(cancellationToken);
    }
}