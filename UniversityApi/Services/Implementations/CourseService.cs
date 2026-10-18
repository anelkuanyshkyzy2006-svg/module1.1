using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courses;
    private readonly ITeacherRepository _teachers;
    private readonly IMapper _mapper;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ICourseRepository courses,
        ITeacherRepository teachers,
        IMapper mapper,
        ILogger<CourseService> logger)
    {
        _courses = courses;
        _teachers = teachers;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReturnResult<IEnumerable<CourseDto>>> GetAllAsync(CourseFilterDto filter)
    {
        var courses = await _courses.GetFilteredAsync(filter);
        return ReturnResult<IEnumerable<CourseDto>>.Success(_mapper.Map<IEnumerable<CourseDto>>(courses));
    }

    public async Task<ReturnResult<CourseDto>> GetByIdAsync(int id)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course is null)
            return NotFound<CourseDto>(id);

        return ReturnResult<CourseDto>.Success(_mapper.Map<CourseDto>(course));
    }

    public async Task<ReturnResult<CourseDto>> CreateAsync(CourseCreateDto dto)
    {
        // Правило №4: курс не может ссылаться на несуществующего преподавателя
        if (!await _teachers.ExistsAsync(dto.TeacherId))
            return TeacherNotFound<CourseDto>(dto.TeacherId);

        var course = _mapper.Map<Course>(dto);
        course.CreatedAt = DateTime.UtcNow;

        await _courses.AddAsync(course);
        _logger.LogInformation("Course created: Id={CourseId}, Name={CourseName}, TeacherId={TeacherId}",
            course.Id, course.Name, course.TeacherId);

        return ReturnResult<CourseDto>.Success(_mapper.Map<CourseDto>(course), 201);
    }

    public async Task<ReturnResult<CourseDto>> UpdateAsync(int id, CourseUpdateDto dto)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course is null)
            return NotFound<CourseDto>(id);

        if (!await _teachers.ExistsAsync(dto.TeacherId))
            return TeacherNotFound<CourseDto>(dto.TeacherId);

        _mapper.Map(dto, course);

        await _courses.UpdateAsync(course);
        _logger.LogInformation("Course updated: Id={CourseId}", id);

        return ReturnResult<CourseDto>.Success(_mapper.Map<CourseDto>(course));
    }

    public async Task<ReturnResult<bool>> DeleteAsync(int id)
    {
        var course = await _courses.GetByIdAsync(id);
        if (course is null)
            return NotFound<bool>(id);

        if (await _courses.HasEnrollmentsAsync(id))
        {
            _logger.LogWarning("Course deletion rejected: course Id={CourseId} has enrollments", id);
            return ReturnResult<bool>.Fail(409, ErrorCodes.CourseHasEnrollments, "Course has enrolled students and cannot be deleted");
        }

        await _courses.DeleteAsync(course);
        _logger.LogInformation("Course deleted: Id={CourseId}", id);

        return ReturnResult<bool>.Success(true);
    }

    private ReturnResult<T> NotFound<T>(int id)
    {
        _logger.LogWarning("Course with Id={CourseId} not found", id);
        return ReturnResult<T>.Fail(404, ErrorCodes.CourseNotFound, "Course not found");
    }

    private ReturnResult<T> TeacherNotFound<T>(int teacherId)
    {
        _logger.LogWarning("Course operation rejected: teacher Id={TeacherId} not found", teacherId);
        return ReturnResult<T>.Fail(404, ErrorCodes.TeacherNotFound, "Teacher not found");
    }
}
