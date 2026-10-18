using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly IStudentRepository _students;
    private readonly ICourseRepository _courses;
    private readonly IMapper _mapper;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(
        IEnrollmentRepository enrollments,
        IStudentRepository students,
        ICourseRepository courses,
        IMapper mapper,
        ILogger<EnrollmentService> logger)
    {
        _enrollments = enrollments;
        _students = students;
        _courses = courses;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReturnResult<IEnumerable<EnrollmentDto>>> GetAllAsync(int? studentId, int? courseId)
    {
        var list = await _enrollments.GetFilteredAsync(studentId, courseId);
        return ReturnResult<IEnumerable<EnrollmentDto>>.Success(_mapper.Map<IEnumerable<EnrollmentDto>>(list));
    }

    public async Task<ReturnResult<EnrollmentDto>> GetByIdAsync(int id)
    {
        var enrollment = await _enrollments.GetByIdAsync(id);
        if (enrollment is null)
            return NotFound<EnrollmentDto>(id);

        return ReturnResult<EnrollmentDto>.Success(_mapper.Map<EnrollmentDto>(enrollment));
    }

    public async Task<ReturnResult<EnrollmentDto>> EnrollAsync(EnrollmentCreateDto dto)
    {
        // Правило №2: нельзя записать несуществующего студента
        if (!await _students.ExistsAsync(dto.StudentId))
        {
            _logger.LogWarning("Enrollment rejected: student Id={StudentId} not found", dto.StudentId);
            return ReturnResult<EnrollmentDto>.Fail(404, ErrorCodes.StudentNotFound, "Student not found");
        }

        // Правило №1: нельзя записать студента на несуществующий курс
        if (!await _courses.ExistsAsync(dto.CourseId))
        {
            _logger.LogWarning("Enrollment rejected: course Id={CourseId} not found", dto.CourseId);
            return ReturnResult<EnrollmentDto>.Fail(404, ErrorCodes.CourseNotFound, "Course not found");
        }

        // Правило №3: нельзя записать дважды на один курс
        if (await _enrollments.IsEnrolledAsync(dto.StudentId, dto.CourseId))
            return AlreadyEnrolled(dto.StudentId, dto.CourseId);

        var enrollment = _mapper.Map<Enrollment>(dto);
        enrollment.EnrollmentDate = DateTime.UtcNow;

        try
        {
            await _enrollments.AddAsync(enrollment);
        }
        catch (DbUpdateException ex)
        {
            // Гонка запросов: уникальный индекс (StudentId, CourseId) сработал в БД
            _logger.LogWarning(ex, "Unique index violation on enrollment StudentId={StudentId}, CourseId={CourseId}",
                dto.StudentId, dto.CourseId);
            return AlreadyEnrolled(dto.StudentId, dto.CourseId);
        }

        _logger.LogInformation("Student {StudentId} enrolled to course {CourseId}: EnrollmentId={EnrollmentId}",
            dto.StudentId, dto.CourseId, enrollment.Id);

        return ReturnResult<EnrollmentDto>.Success(_mapper.Map<EnrollmentDto>(enrollment), 201);
    }

    public async Task<ReturnResult<EnrollmentDto>> UpdateGradeAsync(int id, EnrollmentGradeUpdateDto dto)
    {
        var enrollment = await _enrollments.GetByIdAsync(id);
        if (enrollment is null)
            return NotFound<EnrollmentDto>(id);

        enrollment.Grade = dto.Grade;
        await _enrollments.UpdateAsync(enrollment);
        _logger.LogInformation("Grade updated: EnrollmentId={EnrollmentId}, Grade={Grade}", id, dto.Grade);

        return ReturnResult<EnrollmentDto>.Success(_mapper.Map<EnrollmentDto>(enrollment));
    }

    public async Task<ReturnResult<bool>> DeleteAsync(int id)
    {
        var enrollment = await _enrollments.GetByIdAsync(id);
        if (enrollment is null)
            return NotFound<bool>(id);

        await _enrollments.DeleteAsync(enrollment);
        _logger.LogInformation("Enrollment deleted: Id={EnrollmentId}", id);

        return ReturnResult<bool>.Success(true);
    }

    private ReturnResult<EnrollmentDto> AlreadyEnrolled(int studentId, int courseId)
    {
        _logger.LogWarning("Enrollment rejected: student {StudentId} is already enrolled to course {CourseId}",
            studentId, courseId);
        return ReturnResult<EnrollmentDto>.Fail(409, ErrorCodes.AlreadyEnrolled, "Student is already enrolled in this course");
    }

    private ReturnResult<T> NotFound<T>(int id)
    {
        _logger.LogWarning("Enrollment with Id={EnrollmentId} not found", id);
        return ReturnResult<T>.Fail(404, ErrorCodes.EnrollmentNotFound, "Enrollment not found");
    }
}
