using Microsoft.AspNetCore.Mvc;

namespace UniversityApi.Controllers;

/// <summary>Только для демонстрации на защите: вызывает непредвиденную ошибку (500).</summary>
[ApiController]
[Route("api/diagnostics")]
public class DiagnosticsController : ControllerBase
{
    [HttpGet("error")]
    public IActionResult ThrowError()
        => throw new InvalidOperationException("Demo exception: this stack trace must never reach the client.");
}
