using Microsoft.EntityFrameworkCore.Migrations;

namespace GoApi.Migrations.AppDb
{
    public partial class orgrename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Organisations");

            migrationBuilder.AddColumn<string>(
                name: "OrganisationName",
                table: "Organisations",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganisationName",
                table: "Organisations");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Organisations",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");
        }
    }
}
