namespace ResearchPaperKnowledgeWorkspace.Application.Abstractions.Development;

public interface IDemoDataSeeder
{
    Task<DemoSeedResult> SeedAsync(
        int paperCount = 150,
        CancellationToken cancellationToken = default);
}

public sealed record DemoSeedResult(
    bool WasCreated,
    int PaperCount,
    int AuthorCount,
    int TagCount,
    int ProjectCount,
    int NoteCount,
    int RelationCount);