using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExampleApi.Migrations.SqliteMigrations;

public partial class RemoveRefreshTokenFromAspNetUsers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "RefreshToken",
            table: "AspNetUsers");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "RefreshToken",
            table: "AspNetUsers",
            type: "TEXT",
            nullable: true);
    }
}
