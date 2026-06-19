using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ResearchPaperKnowledgeWorkspace.Application.Organization.Models;
using ResearchPaperKnowledgeWorkspace.Application.Organization.Services;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Models;
using ResearchPaperKnowledgeWorkspace.Application.Papers.Services;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Repositories;

namespace ResearchPaperKnowledgeWorkspace.Tests.Application;

public sealed class PaperOrganizationServiceTests
{
    [Fact]
    public async Task OrganizationService_ShouldCreateAndAssignCatalogItems()
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

        var paperRepository =
            new EfPaperRepository(factory);

        var catalogRepository =
            new EfResearchCatalogRepository(factory);

        var paperService =
            new PaperLibraryService(
                paperRepository);

        var organizationService =
            new PaperOrganizationService(
                catalogRepository,
                paperRepository);

        var paperId =
            await paperService.CreatePaperAsync(
                new CreatePaperRequest(
                    "Organized Research Paper"));

        var authorId =
            await organizationService.CreateAuthorAsync(
                new CreateAuthorRequest(
                    "Example Author"));

        var tagId =
            await organizationService.CreateTagAsync(
                new CreateTagRequest(
                    "knowledge-graph"));

        var projectId =
            await organizationService.CreateProjectAsync(
                new CreateResearchProjectRequest(
                    "Knowledge Graph Study"));

        await organizationService
            .UpdatePaperOrganizationAsync(
                new UpdatePaperOrganizationRequest(
                    paperId,
                    new[] { authorId },
                    new[] { tagId },
                    new[] { projectId }));

        var organization =
            await organizationService
                .GetPaperOrganizationAsync(
                    paperId);

        Assert.Equal(
            new[] { authorId },
            organization.AuthorIds);

        Assert.Equal(
            new[] { tagId },
            organization.TagIds);

        Assert.Equal(
            new[] { projectId },
            organization.ProjectIds);
    }

    [Fact]
    public async Task BatchOperation_ShouldUpdateMultiplePapers()
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

        var paperRepository =
            new EfPaperRepository(factory);

        var catalogRepository =
            new EfResearchCatalogRepository(factory);

        var paperService =
            new PaperLibraryService(
                paperRepository);

        var organizationService =
            new PaperOrganizationService(
                catalogRepository,
                paperRepository);

        var firstPaperId =
            await paperService.CreatePaperAsync(
                new CreatePaperRequest(
                    "First Batch Paper"));

        var secondPaperId =
            await paperService.CreatePaperAsync(
                new CreatePaperRequest(
                    "Second Batch Paper"));

        var result =
            await organizationService
                .SetBatchFavoriteAsync(
                    new[]
                    {
                        firstPaperId,
                        secondPaperId
                    },
                    true);

        Assert.Equal(2, result.RequestedCount);
        Assert.Equal(2, result.AffectedCount);

        var firstPaper =
            await paperService.GetPaperDetailsAsync(
                firstPaperId);

        var secondPaper =
            await paperService.GetPaperDetailsAsync(
                secondPaperId);

        Assert.True(firstPaper.IsFavorite);
        Assert.True(secondPaper.IsFavorite);
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