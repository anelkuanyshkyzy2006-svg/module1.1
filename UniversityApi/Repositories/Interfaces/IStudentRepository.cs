using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface IStudentRepository : IRepository<Student>
{
    Task<Student?> GetByIdWithCoursesAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeId = null);
}
