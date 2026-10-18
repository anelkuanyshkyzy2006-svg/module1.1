namespace UniversityApi.Models;

public class Enrollment : IEntity
{
    public int Id { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime EnrollmentDate { get; set; }

    /// <summary>Оценка (0–100). null — оценка ещё не выставлена.</summary>
    public decimal? Grade { get; set; }
}
