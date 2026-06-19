using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentImportSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Attachments_Sha256Hash",
                table: "Attachments");

            migrationBuilder.AddColumn<string>(
                name: "ExtractedText",
                table: "Attachments",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageCount",
                table: "Attachments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "SourceModifiedAtUtc",
                table: "Attachments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImportJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginalFilePath = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AttachmentType = table.Column<int>(type: "INTEGER", nullable: false),
                    Sha256Hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    DetectedTitle = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DetectedAuthorText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    PaperId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AttachmentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StartedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    CompletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportJobs_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ImportJobs_Papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "Papers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_Sha256Hash",
                table: "Attachments",
                column: "Sha256Hash",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Attachments_PageCount",
                table: "Attachments",
                sql: "\"PageCount\" IS NULL OR \"PageCount\" >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_AttachmentId",
                table: "ImportJobs",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_CreatedAtUtc",
                table: "ImportJobs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_PaperId",
                table: "ImportJobs",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_Sha256Hash",
                table: "ImportJobs",
                column: "Sha256Hash");

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_Status",
                table: "ImportJobs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportJobs");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_Sha256Hash",
                table: "Attachments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Attachments_PageCount",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "ExtractedText",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "PageCount",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "SourceModifiedAtUtc",
                table: "Attachments");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_Sha256Hash",
                table: "Attachments",
                column: "Sha256Hash");
        }
    }
}
