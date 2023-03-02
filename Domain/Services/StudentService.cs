using Domain.Abstracts;
using Domain.Db;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Domain.Services;

public interface IStudentService : IManagementService<Student>
{
    Task<List<Student>> GetStudents(List<string> studentsId, CancellationToken cancellationToken);

    Task<List<Student>> StudentsByGuardian(Guid guardianId, CancellationToken cancellationToken);
}

public class StudentService : ManagementService<Student>, IStudentService
{
    private readonly AwqafDb dataContext;

    public StudentService(AwqafDb dataContext) : base(dataContext) => this.dataContext = dataContext;

    public async Task<List<Student>> GetStudents(List<string> studentsId, CancellationToken cancellationToken)
    {
        var students = new List<Student>();
        foreach (var studentId in studentsId)
        {
            var student = await Get(Guid.Parse(studentId), cancellationToken);
            if (student is not null) students.Add(student);
        }

        return students;
    }

    public async Task<List<Student>> StudentsByGuardian(Guid guardianId, CancellationToken cancellationToken)
        => await dataContext.Students.Where(x => x.GuardianId == guardianId).ToListAsync(cancellationToken);
}