using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class StudentRepository : Repository<Student>, IStudentRepository
{
    public StudentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Student?> GetByIdWithCoursesAsync(int id)
        => await DbSet
            .AsNoTracking()
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(s => s.Id == id);

    public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        => await DbSet.AnyAsync(s => s.Email == email && (excludeId == null || s.Id != excludeId));
}
