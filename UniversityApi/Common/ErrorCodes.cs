namespace UniversityApi.Common;

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";

    public const string StudentNotFound = "STUDENT_NOT_FOUND";
    public const string TeacherNotFound = "TEACHER_NOT_FOUND";
    public const string CourseNotFound = "COURSE_NOT_FOUND";
    public const string EnrollmentNotFound = "ENROLLMENT_NOT_FOUND";

    public const string StudentEmailExists = "STUDENT_EMAIL_EXISTS";
    public const string TeacherEmailExists = "TEACHER_EMAIL_EXISTS";
    public const string AlreadyEnrolled = "ALREADY_ENROLLED";
    public const string TeacherHasCourses = "TEACHER_HAS_COURSES";
    public const string CourseHasEnrollments = "COURSE_HAS_ENROLLMENTS";
}
