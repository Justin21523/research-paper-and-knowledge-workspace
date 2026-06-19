using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Application.Organization.Models;

public sealed record AuthorCatalogItem(
    Guid Id,
    string FullName,
    string? Orcid,
    string? Affiliation);

public sealed record TagCatalogItem(
    Guid Id,
    string Name,
    string? Description,
    string? ColorHex);

public sealed record ResearchProjectCatalogItem(
    Guid Id,
    string Name,
    string? Description,
    ProjectStatus Status,
    string? ColorHex);

public sealed record ResearchCatalog(
    IReadOnlyList<AuthorCatalogItem> Authors,
    IReadOnlyList<TagCatalogItem> Tags,
    IReadOnlyList<ResearchProjectCatalogItem> Projects);

public sealed record PaperOrganizationDetails(
    Guid PaperId,
    IReadOnlyList<Guid> AuthorIds,
    IReadOnlyList<Guid> TagIds,
    IReadOnlyList<Guid> ProjectIds);