using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Students;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/students")]
[Produces("application/json")]
public class StudentsController : BaseApiController
{
    private readonly IStudentService _service;

    public StudentsController(IStudentService service) => _service = service;

    /// <summary>Получить всех студентов.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IEnumerable<StudentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Respond(await _service.GetAllAsync());

    /// <summary>Получить студента по ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) => Respond(await _service.GetByIdAsync(id));

    /// <summary>Получить студента вместе с его курсами и оценками.</summary>
    [HttpGet("{id}/courses")]
    [ProducesResponseType(typeof(ReturnResult<StudentWithCoursesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWithCourses(int id) => Respond(await _service.GetWithCoursesAsync(id));

    /// <summary>Создать студента.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] StudentCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return Respond(result, nameof(GetById), new { id = result.Result?.Id });
    }

    /// <summary>Изменить данные студента.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ReturnResult<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] StudentUpdateDto dto)
        => Respond(await _service.UpdateAsync(id, dto));

    /// <summary>Удалить студента (его записи на курсы удаляются каскадно).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ReturnResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id) => Respond(await _service.DeleteAsync(id));
}
