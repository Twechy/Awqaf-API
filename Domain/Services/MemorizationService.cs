using Domain.Abstracts;
using Domain.Db;
using Domain.Models;

namespace Domain.Services;

public interface IMemorizationService : IManagementService<Memorization>
{
    Task AddSession(MemorizationRecordDto memorizationDto, CancellationToken cancellationToken = default);
}

public class MemorizationService : ManagementService<Memorization>, IMemorizationService
{
    public MemorizationService(AwqafDb dataContext) : base(dataContext)
    {
    }

    public async Task AddSession(MemorizationRecordDto memorizationDto, CancellationToken cancellationToken = default)
    {
        var memorization = await Get(memorizationDto.MemorizationId, cancellationToken);

        var memorizationRecord = MemorizationRecordDto.Create(memorizationDto);
        memorization.MemorizationRecords.Add(memorizationRecord);

        await Save(cancellationToken);
    }
}

public class MemorizationRecordDto
{
    public string From { get; set; }
    public string To { get; set; }
    public string SurahId { get; set; }
    public Grade Grade { get; set; }
    public Guid MemorizationId { get; set; }

    public static MemorizationRecord Create(MemorizationRecordDto memorization) => new()
    {
        From = memorization.From,
        To = memorization.To,
        Grade = memorization.Grade,
        MemorizationId = memorization.MemorizationId,
        SurahId = memorization.SurahId
    };
}