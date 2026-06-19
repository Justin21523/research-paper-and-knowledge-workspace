using ResearchPaperKnowledgeWorkspace.Core.Entities;
using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Tests.Core;

public sealed class DomainModelTests
{
    [Fact]
    public void NewPaper_ShouldHaveExpectedDefaults()
    {
        var paper = new Paper
        {
            Title = "Test Research Paper"
        };

        Assert.NotEqual(Guid.Empty, paper.Id);
        Assert.Equal("Test Research Paper", paper.Title);
        Assert.Equal(ReadingStatus.Unread, paper.ReadingStatus);
        Assert.False(paper.IsFavorite);
        Assert.False(paper.IsArchived);
        Assert.Empty(paper.PaperAuthors);
        Assert.Empty(paper.PaperTags);
        Assert.Empty(paper.Notes);
        Assert.Empty(paper.Attachments);
    }

    [Fact]
    public void PaperAuthor_ShouldStoreAuthorOrder()
    {
        var paper = new Paper
        {
            Title = "Metadata Research"
        };

        var author = new Author
        {
            FullName = "Example Author"
        };

        var paperAuthor = new PaperAuthor
        {
            PaperId = paper.Id,
            Paper = paper,
            AuthorId = author.Id,
            Author = author,
            AuthorOrder = 1,
            IsCorrespondingAuthor = true
        };

        Assert.Equal(1, paperAuthor.AuthorOrder);
        Assert.True(paperAuthor.IsCorrespondingAuthor);
        Assert.Equal(paper.Id, paperAuthor.PaperId);
        Assert.Equal(author.Id, paperAuthor.AuthorId);
    }

    [Fact]
    public void PaperTag_ShouldDefaultToManualAssignment()
    {
        var paperTag = new PaperTag();

        Assert.Equal(
            TagAssignmentSource.Manual,
            paperTag.AssignmentSource);

        Assert.Equal(1.0, paperTag.Confidence);
    }

    [Fact]
    public void EntityBase_MarkUpdated_ShouldChangeUpdatedTime()
    {
        var paper = new Paper
        {
            Title = "Updated Paper"
        };

        var originalUpdatedAt = paper.UpdatedAtUtc;

        Thread.Sleep(5);
        paper.MarkUpdated();

        Assert.True(paper.UpdatedAtUtc > originalUpdatedAt);
    }
}