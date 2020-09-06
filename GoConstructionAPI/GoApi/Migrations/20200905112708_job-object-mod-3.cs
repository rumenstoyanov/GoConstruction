using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GoApi.Migrations
{
    public partial class jobobjectmod3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Jobs_ParentJobId",
                table: "Jobs");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentJobId",
                table: "Jobs",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Jobs_ParentJobId",
                table: "Jobs",
                column: "ParentJobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Jobs_ParentJobId",
                table: "Jobs");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentJobId",
                table: "Jobs",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Jobs_ParentJobId",
                table: "Jobs",
                column: "ParentJobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
