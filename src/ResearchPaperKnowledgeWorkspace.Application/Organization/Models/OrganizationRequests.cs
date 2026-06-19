using ResearchPaperKnowledgeWorkspace.Core.Enums;

namespace ResearchPaperKnowledgeWorkspace.Application.Organization.Models;

public sealed record CreateAuthorRequest(
    string FullName,
    string? GivenName = null,
    string? FamilyName = null,
    string? Orcid = null,
    string? Affiliation = null);

public sealed record CreateTagRequest(
    string Name,
    string? Description = null,
    string? ColorHex = null);

public sealed record CreateResearchProjectRequest(
    string Name,
    string? Description = null,
    ProjectStatus Status = ProjectStatus.Active,
    string? ColorHex = null);

public sealed record UpdatePaperOrganizationRequest(
    Guid PaperId,
    IReadOnlyList<Guid> AuthorIds,
    IReadOnlyList<Guid> TagIds,
    IReadOnlyList<Guid> ProjectIds);