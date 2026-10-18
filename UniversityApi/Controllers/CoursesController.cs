using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;
using UniversityApi.DTO.Courses;
using UniversityApi.Services.Interfaces;

namespace UniversityApi.Controllers;

[ApiController]
[Route("api/courses")]
[Produces("application/json")]
public class CoursesController : BaseApiController
{
    private readonly ICourseService _service;

    public CoursesController(ICourseService service) => _service = service;

    /// <summary>Получить курсы. Фильтры: name, teacherId, minCredits, maxCredits.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ReturnResult<IEnumerable<CourseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] CourseFilterDto filter)
        => Respond(await _service.GetAllAsync(filter));

    /// <summary>Получить курс по ID.</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id) => Respond(await _service.GetByIdAsync(id));

    /// <summary>Создать курс и закрепить его за преподавателем.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CourseCreateDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return Respond(result, nameof(GetById), new { id = result.Result?.Id });
    }

    /// <summary>Изменить курс.</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ReturnResult<CourseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] CourseUpdateDto dto)
        => Respond(await _service.UpdateAsync(id, dto));

    /// <summary>Удалить курс (409, если на курс записаны студенты).</summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ReturnResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ReturnResult<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id) => Respond(await _service.DeleteAsync(id));
}
