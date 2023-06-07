using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStore.Migrations
{
    public partial class AddcategoryToTelegramAlert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AlertCategoryId",
                table: "TelegramAlerts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TelegramAlerts_AlertCategoryId",
                table: "TelegramAlerts",
                column: "AlertCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_TelegramAlerts_TelegramAlertCategories_AlertCategoryId",
                table: "TelegramAlerts",
                column: "AlertCategoryId",
                principalTable: "TelegramAlertCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TelegramAlerts_TelegramAlertCategories_AlertCategoryId",
                table: "TelegramAlerts");

            migrationBuilder.DropIndex(
                name: "IX_TelegramAlerts_AlertCategoryId",
                table: "TelegramAlerts");

            migrationBuilder.DropColumn(
                name: "AlertCategoryId",
                table: "TelegramAlerts");
        }
    }
}
