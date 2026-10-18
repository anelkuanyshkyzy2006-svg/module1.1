using UniversityApi.DTO.Courses;
using UniversityApi.Models;

namespace UniversityApi.Repositories.Interfaces;

public interface ICourseRepository : IRepository<Course>
{
    Task<List<Course>> GetFilteredAsync(CourseFilterDto filter);
    Task<bool> HasEnrollmentsAsync(int courseId);
}
