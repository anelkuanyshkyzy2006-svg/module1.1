using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class TeacherRepository : Repository<Teacher>, ITeacherRepository
{
    public TeacherRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        => await DbSet.AnyAsync(t => t.Email == email && (excludeId == null || t.Id != excludeId));

    public async Task<bool> HasCoursesAsync(int teacherId)
        => await Context.Courses.AnyAsync(c => c.TeacherId == teacherId);
}
