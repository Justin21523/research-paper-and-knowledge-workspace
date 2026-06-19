namespace ResearchPaperKnowledgeWorkspace.Application.Common.Exceptions;

public sealed class RequestValidationException : Exception
{
    public RequestValidationException(string message)
        : base(message)
    {
    }
}