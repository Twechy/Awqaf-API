using System.Linq.Expressions;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Abstracts;

public interface IManagementService<R> where R : BaseEntity
{
    Task AddOrUpdate(R request, CancellationToken cancellationToken);

    Task ChangeState(Guid requestId, CancellationToken cancellationToken);

    Task<List<R>> List(Expression<Func<R, bool>> predicate = null, CancellationToken cancellationToken = default);

    Task<R> Get(Guid id, CancellationToken cancellationToken = default);
}

public class ManagementService<TR> where TR : BaseEntity
{
    protected readonly DbContext DbContext;

    protected ManagementService(DbContext dataContext) => this.DbContext = dataContext;

    public async Task AddOrUpdate(TR request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            await Add(request, cancellationToken);
        }
        else
        {
            var model = await Get(request.Id, cancellationToken);
            if (model == null) throw new ArgumentNullException(nameof(request));

            await Update(request, cancellationToken);
        }
    }

    private async Task Add(TR request, CancellationToken cancellationToken)
    {
        await DbContext.Set<TR>().AddAsync(request, cancellationToken);

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task Update(TR request, CancellationToken cancellationToken)
    {
        DbContext.Set<TR>().Update(request);

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeState(Guid requestId, CancellationToken cancellationToken)
    {
        var request = await DbContext.Set<TR>().FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);

        request.SetState();

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TR>> List(Expression<Func<TR, bool>> predicate = null, CancellationToken cancellationToken = default) =>
        predicate != null
            ? await DbContext.Set<TR>().Where(predicate).ToListAsync(cancellationToken)
            : await DbContext.Set<TR>().ToListAsync(cancellationToken);

    public async Task<TR> Get(Guid id, CancellationToken cancellationToken = default)
        => await DbContext.Set<TR>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    protected async Task Save(CancellationToken cancellationToken = default) => await DbContext.SaveChangesAsync(cancellationToken);
}