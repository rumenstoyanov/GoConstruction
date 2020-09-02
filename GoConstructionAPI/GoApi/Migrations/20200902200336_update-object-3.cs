using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GoApi.Migrations
{
    public partial class updateobject3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedResourceId",
                table: "Updates",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "FriendlyId",
                table: "Sites",
                maxLength: 16,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedResourceId",
                table: "Updates");

            migrationBuilder.DropColumn(
                name: "FriendlyId",
                table: "Sites");
        }
    }
}
