using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ResearchPaperKnowledgeWorkspace.Application.Common.Exceptions;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Models;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Services;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Repositories;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Tests.Application;

public sealed class PaperLibraryServiceTests
{
    [Fact]
    public async Task CreatePaperAsync_ShouldPersistPaper()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<ResearchWorkspaceDbContext>()
                .UseSqlite(connection)
                .Options;

        var factory = new TestDbContextFactory(options);

        await using (var dbContext = factory.CreateDbContext())
        {
            await dbContext.Database.EnsureCreatedAsync();
        }

        var repository = new EfPaperRepository(factory);
        var service = new PaperLibraryService(repository);

        var paperId = await service.CreatePaperAsync(
            new CreatePaperRequest(
                Title: "  Research Knowledge Systems  ",
                PublicationYear: 2026,
                JournalTitle: "Knowledge Management Review"));

        var papers = await service.GetPaperListAsync();

        var paper = Assert.Single(papers);

        Assert.Equal(paperId, paper.Id);
        Assert.Equal(
            "Research Knowledge Systems",
            paper.Title);

        Assert.Equal(2026, paper.PublicationYear);

        Assert.Equal(
            "Knowledge Management Review",
            paper.JournalTitle);
    }

    [Fact]
    public async Task CreatePaperAsync_ShouldRejectEmptyTitle()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<ResearchWorkspaceDbContext>()
                .UseSqlite(connection)
                .Options;

        var factory = new TestDbContextFactory(options);
        var repository = new EfPaperRepository(factory);
        var service = new PaperLibraryService(repository);

        await Assert.ThrowsAsync<RequestValidationException>(
            () => service.CreatePaperAsync(
                new CreatePaperRequest(
                    Title: "   ")));
    }
    [Fact]
    public async Task UpdatePaperAsync_ShouldPersistMetadataChanges()
    {
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

        var repository =
            new EfPaperRepository(factory);

        var service =
            new PaperLibraryService(repository);

        var paperId =
            await service.CreatePaperAsync(
                new CreatePaperRequest(
                    Title: "Original Title",
                    PublicationYear: 2024));

        var updated =
            await service.UpdatePaperAsync(
                new UpdatePaperRequest(
                    Id: paperId,
                    Title: "Updated Research Title",
                    Subtitle: "A Knowledge Workspace Study",
                    AbstractText: "Updated abstract.",
                    PublicationYear: 2026,
                    JournalTitle: "Knowledge Systems Journal",
                    ConferenceName: null,
                    Publisher: "Research Publisher",
                    Volume: "12",
                    Issue: "3",
                    PageRange: "10-32",
                    Doi: "10.1234/example",
                    Isbn: null,
                    Issn: "1234-5678",
                    Url: "https://example.org/paper",
                    LanguageCode: "en",
                    CitationKey: "example2026workspace",
                    ReadingStatus: ReadingStatus.Read,
                    Rating: 5,
                    Priority: 4,
                    IsFavorite: true));

        Assert.Equal(
            "Updated Research Title",
            updated.Title);

        Assert.Equal(
            ReadingStatus.Read,
            updated.ReadingStatus);

        Assert.Equal(5, updated.Rating);
        Assert.Equal(4, updated.Priority);
        Assert.True(updated.IsFavorite);

        var reloaded =
            await service.GetPaperDetailsAsync(
                paperId);

        Assert.Equal(
            "Knowledge Systems Journal",
            reloaded.JournalTitle);

        Assert.Equal(
            "Updated abstract.",
            reloaded.AbstractText);
    }
    private sealed class TestDbContextFactory
        : IDbContextFactory<ResearchWorkspaceDbContext>
    {
        private readonly DbContextOptions<
            ResearchWorkspaceDbContext> _options;

        public TestDbContextFactory(
            DbContextOptions<ResearchWorkspaceDbContext> options)
        {
            _options = options;
        }

        public ResearchWorkspaceDbContext CreateDbContext()
        {
            return new ResearchWorkspaceDbContext(_options);
        }

        public Task<ResearchWorkspaceDbContext>
            CreateDbContextAsync(
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateDbContext());
        }
    }
}