using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Converters;

/// <summary>
/// Stores DateTimeOffset values as UTC Unix milliseconds.
/// </summary>
public sealed class DateTimeOffsetToUnixMillisecondsConverter
    : ValueConverter<DateTimeOffset, long>
{
    public DateTimeOffsetToUnixMillisecondsConverter()
        : base(
            value => value.ToUniversalTime().ToUnixTimeMilliseconds(),
            value => DateTimeOffset.FromUnixTimeMilliseconds(value))
    {
    }
}