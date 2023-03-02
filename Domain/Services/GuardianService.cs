using Domain.Abstracts;
using Domain.Db;
using Domain.Models;

namespace Domain.Services;

public interface IGuardianService : IManagementService<Guardian>
{
    Task LinkGuardianWithFamily(string guardianId, List<string> familyMembersId, CancellationToken cancellationToken = default);

    Task<List<Student>> GuardianMembers(string guardianId, CancellationToken cancellationToken = default);
}

public class GuardianService : ManagementService<Guardian>, IGuardianService
{
    private readonly IStudentService _studentService;

    public GuardianService(AwqafDb dataContext, IStudentService studentService) : base(dataContext) => _studentService = studentService;

    public async Task LinkGuardianWithFamily(string guardianId, List<string> familyMembersId, CancellationToken cancellationToken = default)
    {
        var guardian = await Get(Guid.Parse(guardianId), cancellationToken);
        var students = await _studentService.GetStudents(familyMembersId, cancellationToken);

        guardian.SetGuardianFamilyMembers(students);

        await Save(cancellationToken);
    }

    public async Task<List<Student>> GuardianMembers(string guardianId, CancellationToken cancellationToken = default)
    {
        var guardian = await Get(Guid.Parse(guardianId), cancellationToken);

        return await _studentService.StudentsByGuardian(guardian.Id, cancellationToken);
    }
}