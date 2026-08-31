using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudConfigurationHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateConfigurationHubSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    AccessKeyHash = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfigDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Group = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsSensitive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigDefinitions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigDraftValues",
                columns: table => new
                {
                    EnvironmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigDraftValues", x => new { x.ProjectId, x.EnvironmentId, x.ConfigurationId });
                    table.ForeignKey(
                        name: "FK_ConfigDraftValues_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigReleases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EnvironmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PublishedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigReleases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigReleases_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectEnvironments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectEnvironments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectEnvironments_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfigReleaseValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConfigurationKey = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    IsSensitive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ReleaseId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigReleaseValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigReleaseValues_ConfigReleases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "ConfigReleases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigDefinitions_ProjectId_Group_Key",
                table: "ConfigDefinitions",
                columns: new[] { "ProjectId", "Group", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfigReleases_ProjectId_EnvironmentId_Version",
                table: "ConfigReleases",
                columns: new[] { "ProjectId", "EnvironmentId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfigReleaseValues_ReleaseId",
                table: "ConfigReleaseValues",
                column: "ReleaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectEnvironments_ProjectId_Key",
                table: "ProjectEnvironments",
                columns: new[] { "ProjectId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Key",
                table: "Projects",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigDefinitions");

            migrationBuilder.DropTable(
                name: "ConfigDraftValues");

            migrationBuilder.DropTable(
                name: "ConfigReleaseValues");

            migrationBuilder.DropTable(
                name: "ProjectEnvironments");

            migrationBuilder.DropTable(
                name: "ConfigReleases");

            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
