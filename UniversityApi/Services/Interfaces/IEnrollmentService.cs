using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;

namespace UniversityApi.Services.Interfaces;

public interface IEnrollmentService
{
    Task<ReturnResult<IEnumerable<EnrollmentDto>>> GetAllAsync(int? studentId, int? courseId);
    Task<ReturnResult<EnrollmentDto>> GetByIdAsync(int id);
    Task<ReturnResult<EnrollmentDto>> EnrollAsync(EnrollmentCreateDto dto);
    Task<ReturnResult<EnrollmentDto>> UpdateGradeAsync(int id, EnrollmentGradeUpdateDto dto);
    Task<ReturnResult<bool>> DeleteAsync(int id);
}
