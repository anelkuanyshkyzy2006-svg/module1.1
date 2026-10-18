using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniversityApi.Common;
using UniversityApi.Data;
using UniversityApi.Mapping;
using UniversityApi.Middleware;
using UniversityApi.Repositories.Implementations;
using UniversityApi.Repositories.Interfaces;
using UniversityApi.Services.Implementations;
using UniversityApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ---- Controllers + единый формат ошибок валидации (400) ----
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value is { Errors.Count: > 0 })
                .ToDictionary(
                    e => e.Key,
                    e => e.Value!.Errors
                        .Select(err => string.IsNullOrWhiteSpace(err.ErrorMessage) ? "Invalid value" : err.ErrorMessage)
                        .ToArray());

            var result = ReturnResult<object>.Fail(
                StatusCodes.Status400BadRequest,
                ErrorCodes.ValidationError,
                "One or more validation errors occurred");
            result.Errors = errors;
            result.TraceId = context.HttpContext.TraceIdentifier;

            context.HttpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("Validation")
                .LogWarning("Validation failed for {Method} {Path}: {Errors}",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path,
                    string.Join("; ", errors.Select(kv => $"{kv.Key}: {string.Join(", ", kv.Value)}")));

            return new ObjectResult(result) { StatusCode = StatusCodes.Status400BadRequest };
        };
    });

// ---- Entity Framework Core + PostgreSQL ----
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- AutoMapper ----
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ---- Dependency Injection ----
// Scoped: DbContext живёт один HTTP-запрос, поэтому репозитории и сервисы,
// которые его используют, тоже должны жить один запрос (иначе captive dependency).
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ITeacherRepository, TeacherRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

// ---- Swagger / OpenAPI ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "University Course Management API",
        Version = "v1",
        Description = "REST Web API: students, teachers, courses and enrollments."
    });

    var xmlPath = Path.Combine(AppContext.BaseDirectory, "UniversityApi.xml");
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ---- Создание БД и тестовых данных при старте ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(db);
}

// ---- Pipeline ----
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "University API v1"));

app.MapControllers();

app.Run();
