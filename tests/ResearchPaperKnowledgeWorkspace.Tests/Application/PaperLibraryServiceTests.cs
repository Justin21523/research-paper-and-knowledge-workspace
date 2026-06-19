using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ResearchPaperKnowledgeWorkspace.Application.Common.Exceptions;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Models;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Services;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Repositories;

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