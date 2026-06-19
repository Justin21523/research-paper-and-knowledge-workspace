using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ResearchPaperKnowledgeWorkspace.Core.Enums;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Imports;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Imports.Extractors;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Storage;

namespace ResearchPaperKnowledgeWorkspace.Tests.Infrastructure;

public sealed class DocumentImportServiceTests
    : IAsyncLifetime
{
    private readonly string _temporaryDirectory =
        Path.Combine(
            Path.GetTempPath(),
            $"research-workspace-tests-{Guid.NewGuid():N}");

    public Task InitializeAsync()
    {
        Directory.CreateDirectory(
            _temporaryDirectory);

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(
                _temporaryDirectory,
                true);
        }

        return Task.CompletedTask;
    }

    [Fact]
    public async Task ImportAsync_ShouldImportMarkdownDocument()
    {
        var markdownPath = Path.Combine(
            _temporaryDirectory,
            "knowledge-graph.md");

        await File.WriteAllTextAsync(
            markdownPath,
            """
            # Knowledge Graph Research

            This paper examines semantic relationships,
            metadata, and knowledge organization.
            """);

        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<
                    ResearchWorkspaceDbContext>()
                .UseSqlite(connection)
                .Options;

        var factory =
            new TestDbContextFactory(options);

        await using (var dbContext =
                     factory.CreateDbContext())
        {
            await dbContext.Database
                .EnsureCreatedAsync();
        }

        var paths = CreateWorkspacePaths();

        var service = new DocumentImportService(
            factory,
            paths,
            new IDocumentTextExtractor[]
            {
                new MarkdownDocumentExtractor()
            });

        var result = await service.ImportAsync(
            new[] { markdownPath });

        var item = Assert.Single(result.Items);

        Assert.Equal(
            ImportJobStatus.Succeeded,
            item.Status);

        await using var verificationContext =
            factory.CreateDbContext();

        var paper = await verificationContext.Papers
            .Include(entity => entity.Attachments)
            .SingleAsync();

        Assert.Equal(
            "Knowledge Graph Research",
            paper.Title);

        var attachment =
            Assert.Single(paper.Attachments);

        Assert.Contains(
            "semantic relationships",
            attachment.ExtractedText);

        var absoluteStoredPath = Path.Combine(
            paths.ApplicationDirectory,
            attachment.FilePath);

        Assert.True(
            File.Exists(absoluteStoredPath));
    }

    [Fact]
    public async Task ImportAsync_ShouldDetectDuplicateFile()
    {
        var markdownPath = Path.Combine(
            _temporaryDirectory,
            "duplicate.md");

        await File.WriteAllTextAsync(
            markdownPath,
            "# Duplicate Research");

        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<
                    ResearchWorkspaceDbContext>()
                .UseSqlite(connection)
                .Options;

        var factory =
            new TestDbContextFactory(options);

        await using (var dbContext =
                     factory.CreateDbContext())
        {
            await dbContext.Database
                .EnsureCreatedAsync();
        }

        var service = new DocumentImportService(
            factory,
            CreateWorkspacePaths(),
            new IDocumentTextExtractor[]
            {
                new MarkdownDocumentExtractor()
            });

        var firstResult =
            await service.ImportAsync(
                new[] { markdownPath });

        var secondResult =
            await service.ImportAsync(
                new[] { markdownPath });

        Assert.Equal(
            ImportJobStatus.Succeeded,
            firstResult.Items.Single().Status);

        Assert.Equal(
            ImportJobStatus.Duplicate,
            secondResult.Items.Single().Status);

        await using var verificationContext =
            factory.CreateDbContext();

        Assert.Equal(
            1,
            await verificationContext.Papers.CountAsync());

        Assert.Equal(
            1,
            await verificationContext.Attachments.CountAsync());
    }

    private WorkspacePaths CreateWorkspacePaths()
    {
        var applicationDirectory =
            Path.Combine(
                _temporaryDirectory,
                "workspace");

        var dataDirectory =
            Path.Combine(
                applicationDirectory,
                "data");

        var filesDirectory =
            Path.Combine(
                applicationDirectory,
                "files");

        var importDirectory =
            Path.Combine(
                applicationDirectory,
                "imports");

        Directory.CreateDirectory(dataDirectory);
        Directory.CreateDirectory(filesDirectory);
        Directory.CreateDirectory(importDirectory);

        return new WorkspacePaths(
            applicationDirectory,
            dataDirectory,
            Path.Combine(
                dataDirectory,
                "test.db"),
            filesDirectory,
            importDirectory);
    }

    private sealed class TestDbContextFactory
        : IDbContextFactory<ResearchWorkspaceDbContext>
    {
        private readonly DbContextOptions<
            ResearchWorkspaceDbContext> _options;

        public TestDbContextFactory(
            DbContextOptions<
                ResearchWorkspaceDbContext> options)
        {
            _options = options;
        }

        public ResearchWorkspaceDbContext
            CreateDbContext()
        {
            return new ResearchWorkspaceDbContext(
                _options);
        }

        public Task<ResearchWorkspaceDbContext>
            CreateDbContextAsync(
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                CreateDbContext());
        }
    }
}