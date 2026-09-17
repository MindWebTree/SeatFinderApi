namespace CounsellingApp.Application.Exceptions;

/// <summary>
/// A known, "expected" application error (bad request, unauthorized, conflict, etc.)
/// that should be surfaced to the client with a clean message instead of a stack trace.
/// </summary>
public class AppException : Exception
{
    public int StatusCode { get; }

    public AppException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}
