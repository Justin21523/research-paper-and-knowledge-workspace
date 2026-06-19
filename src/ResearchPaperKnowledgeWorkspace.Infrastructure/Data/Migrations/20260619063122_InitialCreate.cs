using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    GivenName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    FamilyName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    SortName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Orcid = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Affiliation = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Biography = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Papers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Subtitle = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    AbstractText = table.Column<string>(type: "TEXT", nullable: true),
                    PublicationYear = table.Column<int>(type: "INTEGER", nullable: true),
                    PublicationDate = table.Column<long>(type: "INTEGER", nullable: true),
                    JournalTitle = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ConferenceName = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Publisher = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Volume = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Issue = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PageRange = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Doi = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Isbn = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Issn = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Url = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CitationKey = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    ReadingStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastOpenedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Papers", x => x.Id);
                    table.CheckConstraint("CK_Papers_Priority", "\"Priority\" BETWEEN 0 AND 5");
                    table.CheckConstraint("CK_Papers_PublicationYear", "\"PublicationYear\" IS NULL OR (\"PublicationYear\" BETWEEN 1000 AND 9999)");
                    table.CheckConstraint("CK_Papers_Rating", "\"Rating\" BETWEEN 0 AND 5");
                });

            migrationBuilder.CreateTable(
                name: "ResearchProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ColorHex = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    StartedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    TargetCompletionAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ColorHex = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    ParentTagId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_Tags_ParentTagId",
                        column: x => x.ParentTagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PaperId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    FileExtension = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    Sha256Hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    AttachmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFileAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    ImportedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.CheckConstraint("CK_Attachments_FileSizeBytes", "\"FileSizeBytes\" >= 0");
                    table.ForeignKey(
                        name: "FK_Attachments_Papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "Papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PaperId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ContentMarkdown = table.Column<string>(type: "TEXT", nullable: false),
                    NoteType = table.Column<int>(type: "INTEGER", nullable: false),
                    IsPinned = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastEditedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "Papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaperAuthors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PaperId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuthorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuthorOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCorrespondingAuthor = table.Column<bool>(type: "INTEGER", nullable: false),
                    ContributionRole = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperAuthors", x => x.Id);
                    table.CheckConstraint("CK_PaperAuthors_AuthorOrder", "\"AuthorOrder\" >= 1");
                    table.ForeignKey(
                        name: "FK_PaperAuthors_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaperAuthors_Papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "Papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaperRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourcePaperId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TargetPaperId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RelationType = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    Confidence = table.Column<double>(type: "REAL", nullable: false),
                    IsUserConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperRelations", x => x.Id);
                    table.CheckConstraint("CK_PaperRelations_Confidence", "\"Confidence\" BETWEEN 0.0 AND 1.0");
                    table.CheckConstraint("CK_PaperRelations_DifferentPapers", "\"SourcePaperId\" <> \"TargetPaperId\"");
                    table.ForeignKey(
                        name: "FK_PaperRelations_Papers_SourcePaperId",
                        column: x => x.SourcePaperId,
                        principalTable: "Papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaperRelations_Papers_TargetPaperId",
                        column: x => x.TargetPaperId,
                        principalTable: "Papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectPapers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ResearchProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PaperId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AddedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectSpecificNote = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectPapers", x => x.Id);
                    table.CheckConstraint("CK_ProjectPapers_SortOrder", "\"SortOrder\" >= 0");
                    table.ForeignKey(
                        name: "FK_ProjectPapers_Papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "Papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectPapers_ResearchProjects_ResearchProjectId",
                        column: x => x.ResearchProjectId,
                        principalTable: "ResearchProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaperTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PaperId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TagId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AssignmentSource = table.Column<int>(type: "INTEGER", nullable: false),
                    Confidence = table.Column<double>(type: "REAL", nullable: false),
                    AssignedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperTags", x => x.Id);
                    table.CheckConstraint("CK_PaperTags_Confidence", "\"Confidence\" BETWEEN 0.0 AND 1.0");
                    table.ForeignKey(
                        name: "FK_PaperTags_Papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "Papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaperTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_AttachmentType",
                table: "Attachments",
                column: "AttachmentType");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_IsPrimary",
                table: "Attachments",
                column: "IsPrimary");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_PaperId",
                table: "Attachments",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_Sha256Hash",
                table: "Attachments",
                column: "Sha256Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_FullName",
                table: "Authors",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_Orcid",
                table: "Authors",
                column: "Orcid");

            migrationBuilder.CreateIndex(
                name: "IX_Authors_SortName",
                table: "Authors",
                column: "SortName");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_IsPinned",
                table: "Notes",
                column: "IsPinned");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_LastEditedAtUtc",
                table: "Notes",
                column: "LastEditedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_NoteType",
                table: "Notes",
                column: "NoteType");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_PaperId",
                table: "Notes",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperAuthors_AuthorId",
                table: "PaperAuthors",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperAuthors_PaperId_AuthorId",
                table: "PaperAuthors",
                columns: new[] { "PaperId", "AuthorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaperAuthors_PaperId_AuthorOrder",
                table: "PaperAuthors",
                columns: new[] { "PaperId", "AuthorOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PaperRelations_RelationType",
                table: "PaperRelations",
                column: "RelationType");

            migrationBuilder.CreateIndex(
                name: "IX_PaperRelations_SourcePaperId",
                table: "PaperRelations",
                column: "SourcePaperId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperRelations_SourcePaperId_TargetPaperId_RelationType",
                table: "PaperRelations",
                columns: new[] { "SourcePaperId", "TargetPaperId", "RelationType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaperRelations_TargetPaperId",
                table: "PaperRelations",
                column: "TargetPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_Papers_CitationKey",
                table: "Papers",
                column: "CitationKey");

            migrationBuilder.CreateIndex(
                name: "IX_Papers_Doi",
                table: "Papers",
                column: "Doi");

            migrationBuilder.CreateIndex(
                name: "IX_Papers_IsArchived",
                table: "Papers",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_Papers_IsFavorite",
                table: "Papers",
                column: "IsFavorite");

            migrationBuilder.CreateIndex(
                name: "IX_Papers_PublicationYear",
                table: "Papers",
                column: "PublicationYear");

            migrationBuilder.CreateIndex(
                name: "IX_Papers_ReadingStatus",
                table: "Papers",
                column: "ReadingStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Papers_Title",
                table: "Papers",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_PaperTags_AssignmentSource",
                table: "PaperTags",
                column: "AssignmentSource");

            migrationBuilder.CreateIndex(
                name: "IX_PaperTags_PaperId_TagId",
                table: "PaperTags",
                columns: new[] { "PaperId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaperTags_TagId",
                table: "PaperTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPapers_PaperId",
                table: "ProjectPapers",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPapers_ResearchProjectId_PaperId",
                table: "ProjectPapers",
                columns: new[] { "ResearchProjectId", "PaperId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPapers_ResearchProjectId_SortOrder",
                table: "ProjectPapers",
                columns: new[] { "ResearchProjectId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ResearchProjects_IsArchived",
                table: "ResearchProjects",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchProjects_Name",
                table: "ResearchProjects",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchProjects_Status",
                table: "ResearchProjects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_NormalizedName",
                table: "Tags",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_ParentTagId",
                table: "Tags",
                column: "ParentTagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "PaperAuthors");

            migrationBuilder.DropTable(
                name: "PaperRelations");

            migrationBuilder.DropTable(
                name: "PaperTags");

            migrationBuilder.DropTable(
                name: "ProjectPapers");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Papers");

            migrationBuilder.DropTable(
                name: "ResearchProjects");
        }
    }
}
