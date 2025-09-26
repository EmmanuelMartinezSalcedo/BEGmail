using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GmailOrganizer.Infrastructure.Data.Migrations;
/// <inheritdoc />
public partial class InitialCreation : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "Contributors",
        columns: table => new
        {
          Id = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
          Status = table.Column<int>(type: "integer", nullable: false),
          PhoneNumber_CountryCode = table.Column<string>(type: "text", nullable: true),
          PhoneNumber_Number = table.Column<string>(type: "text", nullable: true),
          PhoneNumber_Extension = table.Column<string>(type: "text", nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Contributors", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "Users",
        columns: table => new
        {
          Id = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          GoogleUserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
          Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
          AccessToken = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
          RefreshToken = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
          TokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Users", x => x.Id);
        });
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "Contributors");

    migrationBuilder.DropTable(
        name: "Users");
  }
}
