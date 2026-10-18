using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Teachers;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/teachers")]
[Produces("application/json")]
public class TeachersController : BaseApiController
{
    private readonly ITeacherService _service;

    public TeachersController(ITeacherService service) => _service = service;

    /// <summary>Получить всех преподавателей.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IEnumerable<TeacherDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() => Respond(await _service.GetAllAsync());

    /// <summary>Получить преподавателя по ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) => Respond(await _service.GetByIdAsync(id));

    /// <summary>Создать преподавателя.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] TeacherCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return Respond(result, nameof(GetById), new { id = result.Result?.Id });
    }

    /// <summary>Изменить данные преподавателя.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ReturnResult<TeacherDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] TeacherUpdateDto dto)
        => Respond(await _service.UpdateAsync(id, dto));

    /// <summary>Удалить преподавателя (409, если за ним закреплены курсы).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ReturnResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id) => Respond(await _service.DeleteAsync(id));
}
