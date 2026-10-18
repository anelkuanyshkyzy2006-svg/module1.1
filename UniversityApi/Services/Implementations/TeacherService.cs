using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Teachers;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<TeacherService> _logger;

    public TeacherService(ITeacherRepository repository, IMapper mapper, ILogger<TeacherService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReturnResult<IEnumerable<TeacherDto>>> GetAllAsync()
    {
        var teachers = await _repository.GetAllAsync();
        return ReturnResult<IEnumerable<TeacherDto>>.Success(_mapper.Map<IEnumerable<TeacherDto>>(teachers));
    }

    public async Task<ReturnResult<TeacherDto>> GetByIdAsync(int id)
    {
        var teacher = await _repository.GetByIdAsync(id);
        if (teacher is null)
            return NotFound<TeacherDto>(id);

        return ReturnResult<TeacherDto>.Success(_mapper.Map<TeacherDto>(teacher));
    }

    public async Task<ReturnResult<TeacherDto>> CreateAsync(TeacherCreateDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _repository.EmailExistsAsync(email))
        {
            _logger.LogWarning("Teacher creation rejected: email {Email} already exists", email);
            return ReturnResult<TeacherDto>.Fail(409, ErrorCodes.TeacherEmailExists, "Teacher with this email already exists");
        }

        var teacher = _mapper.Map<Teacher>(dto);
        teacher.Email = email;

        await _repository.AddAsync(teacher);
        _logger.LogInformation("Teacher created: Id={TeacherId}, Email={Email}", teacher.Id, teacher.Email);

        return ReturnResult<TeacherDto>.Success(_mapper.Map<TeacherDto>(teacher), 201);
    }

    public async Task<ReturnResult<TeacherDto>> UpdateAsync(int id, TeacherUpdateDto dto)
    {
        var teacher = await _repository.GetByIdAsync(id);
        if (teacher is null)
            return NotFound<TeacherDto>(id);

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _repository.EmailExistsAsync(email, id))
        {
            _logger.LogWarning("Teacher update rejected: email {Email} already used by another teacher", email);
            return ReturnResult<TeacherDto>.Fail(409, ErrorCodes.TeacherEmailExists, "Teacher with this email already exists");
        }

        _mapper.Map(dto, teacher);
        teacher.Email = email;

        await _repository.UpdateAsync(teacher);
        _logger.LogInformation("Teacher updated: Id={TeacherId}", id);

        return ReturnResult<TeacherDto>.Success(_mapper.Map<TeacherDto>(teacher));
    }

    public async Task<ReturnResult<bool>> DeleteAsync(int id)
    {
        var teacher = await _repository.GetByIdAsync(id);
        if (teacher is null)
            return NotFound<bool>(id);

        if (await _repository.HasCoursesAsync(id))
        {
            _logger.LogWarning("Teacher deletion rejected: teacher Id={TeacherId} still has courses", id);
            return ReturnResult<bool>.Fail(409, ErrorCodes.TeacherHasCourses, "Teacher has assigned courses and cannot be deleted");
        }

        await _repository.DeleteAsync(teacher);
        _logger.LogInformation("Teacher deleted: Id={TeacherId}", id);

        return ReturnResult<bool>.Success(true);
    }

    private ReturnResult<T> NotFound<T>(int id)
    {
        _logger.LogWarning("Teacher with Id={TeacherId} not found", id);
        return ReturnResult<T>.Fail(404, ErrorCodes.TeacherNotFound, "Teacher not found");
    }
}
