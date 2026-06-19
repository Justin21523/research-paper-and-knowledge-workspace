using Microsoft.EntityFrameworkCore;
using ResearchPaperKnowledgeWorkspace.Application.Abstractions.Development;
using ResearchPaperKnowledgeWorkspace.Core.Entities;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Seeding;

public sealed class DatabaseDemoDataSeeder
    : IDemoDataSeeder
{
    private readonly IDbContextFactory<
        ResearchWorkspaceDbContext> _dbContextFactory;

    public DatabaseDemoDataSeeder(
        IDbContextFactory<
            ResearchWorkspaceDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<DemoSeedResult> SeedAsync(
        int paperCount = 150,
        CancellationToken cancellationToken = default)
    {
        paperCount = Math.Clamp(
            paperCount,
            10,
            500);

        await using var dbContext =
            await _dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var demoDataExists =
            await dbContext.Papers.AnyAsync(
                paper =>
                    paper.CitationKey != null &&
                    paper.CitationKey.StartsWith(
                        "demo-"),
                cancellationToken);

        if (demoDataExists)
        {
            return new DemoSeedResult(
                false,
                await dbContext.Papers.CountAsync(
                    paper =>
                        paper.CitationKey != null &&
                        paper.CitationKey.StartsWith(
                            "demo-"),
                    cancellationToken),
                0,
                0,
                0,
                0,
                0);
        }

        var random = new Random(20260619);

        var authors = CreateAuthors();
        var tags = CreateTags();
        var projects = CreateProjects();

        var topics = new[]
        {
            "Knowledge Organization",
            "Digital Archives",
            "Artificial Intelligence",
            "Information Retrieval",
            "Academic Libraries",
            "Metadata Quality",
            "Knowledge Graphs",
            "Digital Humanities",
            "Research Data Management",
            "Machine Learning",
            "Human-Computer Interaction",
            "Open Science",
            "Semantic Search",
            "Digital Preservation",
            "Scholarly Communication"
        };

        var approaches = new[]
        {
            "A Systematic Review",
            "A Comparative Study",
            "An Empirical Evaluation",
            "A Mixed-Methods Analysis",
            "A User-Centered Investigation",
            "A Longitudinal Study",
            "A Framework for Practice",
            "An Experimental Approach",
            "A Case Study",
            "A Bibliometric Analysis"
        };

        var journals = new[]
        {
            "Journal of Knowledge Systems",
            "Digital Library Research",
            "Information Science Review",
            "Archives and Records Quarterly",
            "International Journal of Metadata",
            "Research Data Journal",
            "Journal of Digital Humanities",
            "Information Retrieval Studies",
            "Library Technology Reports",
            "Knowledge Management Review"
        };

        var languages = new[]
        {
            "en",
            "zh-TW",
            "ja",
            "ko",
            "de",
            "fr"
        };

        var readingStatuses = new[]
        {
            ReadingStatus.Unread,
            ReadingStatus.Skimmed,
            ReadingStatus.Reading,
            ReadingStatus.Read,
            ReadingStatus.NotesCompleted,
            ReadingStatus.ReadyToCite,
            ReadingStatus.Cited
        };

        var papers = new List<Paper>(
            paperCount);

        var noteCount = 0;

        for (var index = 1;
             index <= paperCount;
             index++)
        {
            var topic =
                topics[random.Next(topics.Length)];

            var approach =
                approaches[random.Next(
                    approaches.Length)];

            var year = random.Next(1998, 2027);

            var isArchived = index % 17 == 0;

            var paper = new Paper
            {
                Title =
                    $"{topic}: {approach}",
                Subtitle =
                    index % 4 == 0
                        ? "Implications for Research and Practice"
                        : null,
                AbstractText =
                    $"This demonstration paper examines {topic.ToLowerInvariant()} using {approach.ToLowerInvariant()}. " +
                    "It discusses research design, information behavior, metadata, evaluation, and future directions.",
                PublicationYear = year,
                JournalTitle =
                    journals[random.Next(
                        journals.Length)],
                Publisher =
                    "Demo Academic Press",
                Volume =
                    random.Next(1, 30).ToString(),
                Issue =
                    random.Next(1, 5).ToString(),
                PageRange =
                    $"{random.Next(1, 80)}-{random.Next(81, 180)}",
                Doi =
                    $"10.9999/demo.{index:0000}",
                Url =
                    $"https://example.org/demo-paper/{index}",
                LanguageCode =
                    languages[random.Next(
                        languages.Length)],
                CitationKey =
                    $"demo-{index:0000}",
                ReadingStatus =
                    isArchived
                        ? ReadingStatus.Archived
                        : readingStatuses[
                            random.Next(
                                readingStatuses.Length)],
                Rating = random.Next(0, 6),
                Priority = random.Next(0, 6),
                IsFavorite = index % 7 == 0,
                IsArchived = isArchived
            };

            var selectedAuthors = PickDistinct(
                authors,
                random.Next(1, 5),
                random);

            for (var authorIndex = 0;
                 authorIndex <
                 selectedAuthors.Count;
                 authorIndex++)
            {
                var author =
                    selectedAuthors[authorIndex];

                paper.PaperAuthors.Add(
                    new PaperAuthor
                    {
                        PaperId = paper.Id,
                        Paper = paper,
                        AuthorId = author.Id,
                        Author = author,
                        AuthorOrder =
                            authorIndex + 1,
                        IsCorrespondingAuthor =
                            authorIndex == 0
                    });
            }

            foreach (var tag in PickDistinct(
                         tags,
                         random.Next(2, 6),
                         random))
            {
                paper.PaperTags.Add(
                    new PaperTag
                    {
                        PaperId = paper.Id,
                        Paper = paper,
                        TagId = tag.Id,
                        Tag = tag,
                        AssignmentSource =
                            TagAssignmentSource.Imported,
                        Confidence = 1.0
                    });
            }

            foreach (var project in PickDistinct(
                         projects,
                         random.Next(1, 3),
                         random))
            {
                paper.ProjectPapers.Add(
                    new ProjectPaper
                    {
                        PaperId = paper.Id,
                        Paper = paper,
                        ResearchProjectId =
                            project.Id,
                        ResearchProject =
                            project,
                        SortOrder = index
                    });
            }

            if (index % 2 == 0)
            {
                paper.Notes.Add(
                    new Note
                    {
                        PaperId = paper.Id,
                        Paper = paper,
                        Title =
                            "Demo reading note",
                        ContentMarkdown =
                            $"# Reading Note\n\n## Topic\n\n{topic}\n\n## Key Finding\n\nThis is generated demonstration content for paper {index}.",
                        NoteType =
                            NoteType.Summary
                    });

                noteCount++;
            }

            papers.Add(paper);
        }

        var relations =
            new List<PaperRelation>();

        var relationTypes = new[]
        {
            PaperRelationType.Related,
            PaperRelationType.SimilarTopic,
            PaperRelationType.SameMethodology,
            PaperRelationType.Supports,
            PaperRelationType.Contradicts
        };

        for (var index = 1;
             index < papers.Count;
             index++)
        {
            if (index % 3 != 0)
            {
                continue;
            }

            var targetIndex =
                random.Next(0, index);

            relations.Add(
                new PaperRelation
                {
                    SourcePaperId =
                        papers[index].Id,
                    SourcePaper =
                        papers[index],
                    TargetPaperId =
                        papers[targetIndex].Id,
                    TargetPaper =
                        papers[targetIndex],
                    RelationType =
                        relationTypes[
                            random.Next(
                                relationTypes.Length)],
                    Description =
                        "Automatically generated demo relationship.",
                    Confidence = 0.85,
                    IsUserConfirmed = true
                });
        }

        dbContext.Authors.AddRange(
            authors);

        dbContext.Tags.AddRange(
            tags);

        dbContext.ResearchProjects.AddRange(
            projects);

        dbContext.Papers.AddRange(
            papers);

        dbContext.PaperRelations.AddRange(
            relations);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return new DemoSeedResult(
            true,
            papers.Count,
            authors.Count,
            tags.Count,
            projects.Count,
            noteCount,
            relations.Count);
    }

    private static List<Author> CreateAuthors()
    {
        var names = new[]
        {
            "Emma Chen",
            "Daniel Lin",
            "Sophia Wang",
            "Noah Huang",
            "Olivia Chang",
            "Ethan Liu",
            "Mia Wu",
            "Liam Yang",
            "Ava Lee",
            "Lucas Tsai",
            "Haruto Sato",
            "Yuki Tanaka",
            "Min-jun Kim",
            "Ji-woo Park",
            "Emily Johnson",
            "Michael Smith",
            "Sarah Williams",
            "David Brown",
            "Anna Müller",
            "Lukas Schneider",
            "Marie Dubois",
            "Thomas Bernard",
            "Lucía García",
            "Mateo Rodríguez",
            "Aisha Rahman",
            "Omar Hassan",
            "Priya Sharma",
            "Arjun Patel",
            "Grace Mensah",
            "Kwame Boateng",
            "Isabella Rossi",
            "Lorenzo Romano",
            "Sofia Silva",
            "Gabriel Santos",
            "Nora Andersen",
            "Erik Nielsen",
            "Mei Li",
            "Wei Zhang",
            "Xiao Yu",
            "Jiahao Chen",
            "Rina Nakamura",
            "Kenji Ito",
            "Seo-yeon Lee",
            "Hyun-woo Choi",
            "Chloe Martin",
            "Nathan Wilson",
            "Amelia Taylor",
            "James Anderson"
        };

        return names
            .Select(
                (name, index) =>
                    new Author
                    {
                        FullName = name,
                        SortName = name,
                        Orcid =
                            $"0000-0002-{index:0000}-{(index + 1):0000}",
                        Affiliation =
                            $"Demo Research Institute {index % 8 + 1}"
                    })
            .ToList();
    }

    private static List<Tag> CreateTags()
    {
        var names = new[]
        {
            "artificial-intelligence",
            "digital-archives",
            "metadata",
            "knowledge-graph",
            "information-retrieval",
            "digital-humanities",
            "open-science",
            "libraries",
            "machine-learning",
            "research-data",
            "semantic-search",
            "user-experience",
            "digital-preservation",
            "bibliometrics",
            "scholarly-communication",
            "ontology",
            "classification",
            "data-visualization"
        };

        return names
            .Select(
                name =>
                    new Tag
                    {
                        Name = name,
                        NormalizedName = name,
                        Description =
                            $"Demo tag for {name}."
                    })
            .ToList();
    }

    private static List<ResearchProject>
        CreateProjects()
    {
        var names = new[]
        {
            "AI Knowledge Workspace",
            "Digital Archive Research",
            "Library Innovation",
            "Research Data Management",
            "Knowledge Graph Study",
            "Information Retrieval Review",
            "Academic Writing Project",
            "Digital Humanities Lab"
        };

        return names
            .Select(
                (name, index) =>
                    new ResearchProject
                    {
                        Name = name,
                        Description =
                            $"Demo research project: {name}.",
                        Status =
                            index % 4 == 0
                                ? ProjectStatus.Completed
                                : ProjectStatus.Active
                    })
            .ToList();
    }

    private static List<T> PickDistinct<T>(
        IReadOnlyList<T> source,
        int count,
        Random random)
    {
        return source
            .OrderBy(_ => random.Next())
            .Take(Math.Min(count, source.Count))
            .ToList();
    }
}