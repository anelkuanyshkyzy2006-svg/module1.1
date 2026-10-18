using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class EnrollmentRepository : Repository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<List<Enrollment>> GetAllAsync()
        => await GetFilteredAsync(null, null);

    public override async Task<Enrollment?> GetByIdAsync(int id)
        => await DbSet
            .Include(e => e.Student)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<List<Enrollment>> GetFilteredAsync(int? studentId, int? courseId)
    {
        IQueryable<Enrollment> query = DbSet
            .AsNoTracking()
            .Include(e => e.Student)
            .Include(e => e.Course);

        if (studentId.HasValue)
            query = query.Where(e => e.StudentId == studentId.Value);

        if (courseId.HasValue)
            query = query.Where(e => e.CourseId == courseId.Value);

        return await query.OrderBy(e => e.Id).ToListAsync();
    }

    public async Task<bool> IsEnrolledAsync(int studentId, int courseId)
        => await DbSet.AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);

    public override async Task<Enrollment> AddAsync(Enrollment entity)
    {
        await base.AddAsync(entity);
        await Context.Entry(entity).Reference(e => e.Student).LoadAsync();
        await Context.Entry(entity).Reference(e => e.Course).LoadAsync();
        return entity;
    }
}
