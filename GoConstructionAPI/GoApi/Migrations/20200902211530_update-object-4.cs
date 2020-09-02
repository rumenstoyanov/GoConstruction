using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GoApi.Migrations
{
    public partial class updateobject4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Oid",
                table: "Updates",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Updates_Oid",
                table: "Updates",
                column: "Oid");

            migrationBuilder.AddForeignKey(
                name: "FK_Updates_Organisations_Oid",
                table: "Updates",
                column: "Oid",
                principalTable: "Organisations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Updates_Organisations_Oid",
                table: "Updates");

            migrationBuilder.DropIndex(
                name: "IX_Updates_Oid",
                table: "Updates");

            migrationBuilder.DropColumn(
                name: "Oid",
                table: "Updates");
        }
    }
}
