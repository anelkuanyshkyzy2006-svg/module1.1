using Microsoft.EntityFrameworkCore;
using UniversityApi.Data;
using UniversityApi.DTO.Courses;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;

namespace UniversityApi.Repositories.Implementations;

public class CourseRepository : Repository<Course>, ICourseRepository
{
    public CourseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<List<Course>> GetAllAsync()
        => await DbSet.AsNoTracking().Include(c => c.Teacher).OrderBy(c => c.Id).ToListAsync();

    public override async Task<Course?> GetByIdAsync(int id)
        => await DbSet.Include(c => c.Teacher).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<Course>> GetFilteredAsync(CourseFilterDto filter)
    {
        IQueryable<Course> query = DbSet.AsNoTracking().Include(c => c.Teacher);

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(name));
        }

        if (filter.TeacherId.HasValue)
            query = query.Where(c => c.TeacherId == filter.TeacherId.Value);

        if (filter.MinCredits.HasValue)
            query = query.Where(c => c.Credits >= filter.MinCredits.Value);

        if (filter.MaxCredits.HasValue)
            query = query.Where(c => c.Credits <= filter.MaxCredits.Value);

        return await query.OrderBy(c => c.Id).ToListAsync();
    }

    public override async Task<Course> AddAsync(Course entity)
    {
        await base.AddAsync(entity);
        await Context.Entry(entity).Reference(c => c.Teacher).LoadAsync();
        return entity;
    }

    public override async Task UpdateAsync(Course entity)
    {
        await base.UpdateAsync(entity);
        await Context.Entry(entity).Reference(c => c.Teacher).LoadAsync();
    }

    public async Task<bool> HasEnrollmentsAsync(int courseId)
        => await Context.Enrollments.AnyAsync(e => e.CourseId == courseId);
}
