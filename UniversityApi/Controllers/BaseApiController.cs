using Microsoft.AspNetCore.Mvc;
using UniversityApi.Common;

namespace UniversityApi.Controllers;

/// <summary>Общая логика: превращает ReturnResult в HTTP-ответ с нужным статус-кодом и TraceId.</summary>
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult Respond<T>(ReturnResult<T> result, string? createdAtAction = null, object? routeValues = null)
    {
        result.TraceId = HttpContext.TraceIdentifier;

        if (result.StatusCode == StatusCodes.Status201Created && createdAtAction is not null)
            return CreatedAtAction(createdAtAction, routeValues, result);

        return new ObjectResult(result) { StatusCode = result.StatusCode };
    }
}
