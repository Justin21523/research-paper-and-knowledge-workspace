namespace ResearchPaperKnowledgeWorkspace.Core.Enums;

public enum PaperRelationType
{
    Related = 0,
    SimilarTopic = 1,
    SameMethodology = 2,
    Extends = 3,
    Contradicts = 4,
    Supports = 5,
    Cites = 6,
    CitedBy = 7,
    EarlierVersion = 8,
    LaterVersion = 9,
    Duplicate = 10,
    UserDefined = 11
}