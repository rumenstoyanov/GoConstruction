using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GoApi.Migrations
{
    public partial class jobobject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Oid = table.Column<Guid>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    SiteId = table.Column<Guid>(nullable: false),
                    OwnerId = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(maxLength: 250, nullable: false),
                    Description = table.Column<string>(maxLength: 4000, nullable: true),
                    FriendlyId = table.Column<string>(maxLength: 16, nullable: false),
                    JobStatusId = table.Column<int>(nullable: false),
                    DueDate = table.Column<DateTime>(nullable: false),
                    ParentJobId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_JobStatuses_JobStatusId",
                        column: x => x.JobStatusId,
                        principalTable: "JobStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jobs_Organisations_Oid",
                        column: x => x.Oid,
                        principalTable: "Organisations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jobs_AspNetUsers_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jobs_Jobs_ParentJobId",
                        column: x => x.ParentJobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Jobs_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_JobStatusId",
                table: "Jobs",
                column: "JobStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_Oid",
                table: "Jobs",
                column: "Oid");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_OwnerId",
                table: "Jobs",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_ParentJobId",
                table: "Jobs",
                column: "ParentJobId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_SiteId",
                table: "Jobs",
                column: "SiteId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobs");
        }
    }
}
