using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Enrollments;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/enrollments")]
[Produces("application/json")]
public class EnrollmentsController : BaseApiController
{
    private readonly IEnrollmentService _service;

    public EnrollmentsController(IEnrollmentService service) => _service = service;

    /// <summary>Получить список записей (опционально по studentId / courseId).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IEnumerable<EnrollmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int? studentId, [FromQuery] int? courseId)
        => Respond(await _service.GetAllAsync(studentId, courseId));

    /// <summary>Получить запись по ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) => Respond(await _service.GetByIdAsync(id));

    /// <summary>Записать студента на курс.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll([FromBody] EnrollmentCreateDto dto)
    {
        var result = await _service.EnrollAsync(dto);
        return Respond(result, nameof(GetById), new { id = result.Result?.Id });
    }

    /// <summary>Изменить оценку студента.</summary>
    [HttpPut("{id}/grade")]
    [ProducesResponseType(typeof(ReturnResult<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGrade(int id, [FromBody] EnrollmentGradeUpdateDto dto)
        => Respond(await _service.UpdateGradeAsync(id, dto));

    /// <summary>Удалить студента с курса.</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ReturnResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id) => Respond(await _service.DeleteAsync(id));
}
