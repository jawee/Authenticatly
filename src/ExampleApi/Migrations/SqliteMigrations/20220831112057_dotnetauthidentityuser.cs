using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExampleApi.Migrations.SqliteMigrations;

public partial class dotnetauthidentityuser : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Discriminator",
            table: "AspNetUsers",
            type: "TEXT",
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "RefreshToken",
            table: "AspNetUsers",
            type: "TEXT",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Discriminator",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "RefreshToken",
            table: "AspNetUsers");
    }
}
