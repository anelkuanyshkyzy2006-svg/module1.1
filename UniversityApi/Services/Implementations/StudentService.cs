using AutoMapper;
using UniversityApi.Common;
using UniversityApi.DTO.Students;
using UniversityApi.Models;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Services.Implementations;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<StudentService> _logger;

    public StudentService(IStudentRepository repository, IMapper mapper, ILogger<StudentService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ReturnResult<IEnumerable<StudentDto>>> GetAllAsync()
    {
        var students = await _repository.GetAllAsync();
        return ReturnResult<IEnumerable<StudentDto>>.Success(_mapper.Map<IEnumerable<StudentDto>>(students));
    }

    public async Task<ReturnResult<StudentDto>> GetByIdAsync(int id)
    {
        var student = await _repository.GetByIdAsync(id);
        if (student is null)
            return NotFound<StudentDto>(id);

        return ReturnResult<StudentDto>.Success(_mapper.Map<StudentDto>(student));
    }

    public async Task<ReturnResult<StudentWithCoursesDto>> GetWithCoursesAsync(int id)
    {
        var student = await _repository.GetByIdWithCoursesAsync(id);
        if (student is null)
            return NotFound<StudentWithCoursesDto>(id);

        return ReturnResult<StudentWithCoursesDto>.Success(_mapper.Map<StudentWithCoursesDto>(student));
    }

    public async Task<ReturnResult<StudentDto>> CreateAsync(StudentCreateDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _repository.EmailExistsAsync(email))
        {
            _logger.LogWarning("Student creation rejected: email {Email} already exists", email);
            return ReturnResult<StudentDto>.Fail(409, ErrorCodes.StudentEmailExists, "Student with this email already exists");
        }

        var student = _mapper.Map<Student>(dto);
        student.Email = email;
        student.CreatedAt = DateTime.UtcNow;

        await _repository.AddAsync(student);
        _logger.LogInformation("Student created: Id={StudentId}, Email={Email}", student.Id, student.Email);

        return ReturnResult<StudentDto>.Success(_mapper.Map<StudentDto>(student), 201);
    }

    public async Task<ReturnResult<StudentDto>> UpdateAsync(int id, StudentUpdateDto dto)
    {
        var student = await _repository.GetByIdAsync(id);
        if (student is null)
            return NotFound<StudentDto>(id);

        var email = dto.Email.Trim().ToLowerInvariant();
        if (await _repository.EmailExistsAsync(email, id))
        {
            _logger.LogWarning("Student update rejected: email {Email} already used by another student", email);
            return ReturnResult<StudentDto>.Fail(409, ErrorCodes.StudentEmailExists, "Student with this email already exists");
        }

        _mapper.Map(dto, student);
        student.Email = email;

        await _repository.UpdateAsync(student);
        _logger.LogInformation("Student updated: Id={StudentId}", id);

        return ReturnResult<StudentDto>.Success(_mapper.Map<StudentDto>(student));
    }

    public async Task<ReturnResult<bool>> DeleteAsync(int id)
    {
        var student = await _repository.GetByIdAsync(id);
        if (student is null)
            return NotFound<bool>(id);

        await _repository.DeleteAsync(student);
        _logger.LogInformation("Student deleted: Id={StudentId}", id);

        return ReturnResult<bool>.Success(true);
    }

    private ReturnResult<T> NotFound<T>(int id)
    {
        _logger.LogWarning("Student with Id={StudentId} not found", id);
        return ReturnResult<T>.Fail(404, ErrorCodes.StudentNotFound, "Student not found");
    }
}
