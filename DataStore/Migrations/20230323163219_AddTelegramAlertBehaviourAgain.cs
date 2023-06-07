using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DataStore.Migrations
{
    public partial class AddTelegramAlertBehaviourAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TelegramAlertBehaviourTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnumId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramAlertBehaviourTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TelegramAlertBehaviours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TelegramAlertId = table.Column<int>(type: "integer", nullable: false),
                    TelegramAlertBehaviourTypeId = table.Column<int>(type: "integer", nullable: false),
                    TimeSpan = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramAlertBehaviours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TelegramAlertBehaviours_TelegramAlertBehaviourTypes_Telegra~",
                        column: x => x.TelegramAlertBehaviourTypeId,
                        principalTable: "TelegramAlertBehaviourTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TelegramAlertBehaviours_TelegramAlerts_TelegramAlertId",
                        column: x => x.TelegramAlertId,
                        principalTable: "TelegramAlerts",
                        principalColumn: "TelegramAlertId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TelegramAlertBehaviours_TelegramAlertBehaviourTypeId",
                table: "TelegramAlertBehaviours",
                column: "TelegramAlertBehaviourTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TelegramAlertBehaviours_TelegramAlertId",
                table: "TelegramAlertBehaviours",
                column: "TelegramAlertId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TelegramAlertBehaviours");

            migrationBuilder.DropTable(
                name: "TelegramAlertBehaviourTypes");
        }
    }
}
