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
          TokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
          CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Users", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "Waitlists",
        columns: table => new
        {
          Id = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
          CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Waitlists", x => x.Id);
        });

    migrationBuilder.CreateTable(
        name: "EmailProcessingLogs",
        columns: table => new
        {
          Id = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          UserId = table.Column<int>(type: "integer", nullable: false),
          ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
          LabelAssigned = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_EmailProcessingLogs", x => x.Id);
          table.ForeignKey(
                      name: "FK_EmailProcessingLogs_Users_UserId",
                      column: x => x.UserId,
                      principalTable: "Users",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "LabelStats",
        columns: table => new
        {
          Id = table.Column<int>(type: "integer", nullable: false)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
          UserId = table.Column<int>(type: "integer", nullable: false),
          LabelName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
          EmailCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_LabelStats", x => x.Id);
          table.ForeignKey(
                      name: "FK_LabelStats_Users_UserId",
                      column: x => x.UserId,
                      principalTable: "Users",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateTable(
        name: "Subscriptions",
        columns: table => new
        {
          Id = table.Column<int>(type: "integer", nullable: false),
          UserId = table.Column<int>(type: "integer", nullable: false),
          Tier = table.Column<int>(type: "integer", nullable: false),
          Status = table.Column<int>(type: "integer", nullable: false),
          StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
          EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
          EmailsProcessed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
          EmailLimit = table.Column<int>(type: "integer", nullable: false, defaultValue: 100)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Subscriptions", x => x.Id);
          table.ForeignKey(
                      name: "FK_Subscriptions_Users_UserId",
                      column: x => x.UserId,
                      principalTable: "Users",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
        });

    migrationBuilder.CreateIndex(
        name: "IX_EmailProcessingLogs_UserId",
        table: "EmailProcessingLogs",
        column: "UserId");

    migrationBuilder.CreateIndex(
        name: "IX_LabelStats_UserId",
        table: "LabelStats",
        column: "UserId");

    migrationBuilder.CreateIndex(
        name: "IX_Subscriptions_UserId",
        table: "Subscriptions",
        column: "UserId",
        unique: true);
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "Contributors");

    migrationBuilder.DropTable(
        name: "EmailProcessingLogs");

    migrationBuilder.DropTable(
        name: "LabelStats");

    migrationBuilder.DropTable(
        name: "Subscriptions");

    migrationBuilder.DropTable(
        name: "Waitlists");

    migrationBuilder.DropTable(
        name: "Users");
  }
}
