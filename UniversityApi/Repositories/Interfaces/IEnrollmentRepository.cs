using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    Task<List<Enrollment>> GetFilteredAsync(int? studentId, int? courseId);
    Task<bool> IsEnrolledAsync(int studentId, int courseId);
}
