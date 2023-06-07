using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStore.Migrations
{
    public partial class modelopeningbalagain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpeningExchangeBalances_ExchangeDetails_ExchangeDetailsId",
                table: "OpeningExchangeBalances");

            migrationBuilder.DropIndex(
                name: "IX_OpeningExchangeBalances_ExchangeDetailsId",
                table: "OpeningExchangeBalances");

            migrationBuilder.DropColumn(
                name: "ExchangeDetailsId",
                table: "OpeningExchangeBalances");

            migrationBuilder.RenameColumn(
                name: "OpeningBalance",
                table: "OpeningExchangeBalances",
                newName: "LiquidatingToOpeningBalance");

            migrationBuilder.RenameColumn(
                name: "Currency",
                table: "OpeningExchangeBalances",
                newName: "LiquidatingToCurrency");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "OpeningExchangeBalances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LiquidatingFromCurrency",
                table: "OpeningExchangeBalances",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LiquidatingFromOpeningBalance",
                table: "OpeningExchangeBalances",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "OpeningExchangeBalanceId",
                table: "ExchangeDetails",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeDetails_OpeningExchangeBalanceId",
                table: "ExchangeDetails",
                column: "OpeningExchangeBalanceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeDetails_OpeningExchangeBalances_OpeningExchangeBala~",
                table: "ExchangeDetails",
                column: "OpeningExchangeBalanceId",
                principalTable: "OpeningExchangeBalances",
                principalColumn: "OpeningExchangeBalanceId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeDetails_OpeningExchangeBalances_OpeningExchangeBala~",
                table: "ExchangeDetails");

            migrationBuilder.DropIndex(
                name: "IX_ExchangeDetails_OpeningExchangeBalanceId",
                table: "ExchangeDetails");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "OpeningExchangeBalances");

            migrationBuilder.DropColumn(
                name: "LiquidatingFromCurrency",
                table: "OpeningExchangeBalances");

            migrationBuilder.DropColumn(
                name: "LiquidatingFromOpeningBalance",
                table: "OpeningExchangeBalances");

            migrationBuilder.DropColumn(
                name: "OpeningExchangeBalanceId",
                table: "ExchangeDetails");

            migrationBuilder.RenameColumn(
                name: "LiquidatingToOpeningBalance",
                table: "OpeningExchangeBalances",
                newName: "OpeningBalance");

            migrationBuilder.RenameColumn(
                name: "LiquidatingToCurrency",
                table: "OpeningExchangeBalances",
                newName: "Currency");

            migrationBuilder.AddColumn<int>(
                name: "ExchangeDetailsId",
                table: "OpeningExchangeBalances",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpeningExchangeBalances_ExchangeDetailsId",
                table: "OpeningExchangeBalances",
                column: "ExchangeDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningExchangeBalances_ExchangeDetails_ExchangeDetailsId",
                table: "OpeningExchangeBalances",
                column: "ExchangeDetailsId",
                principalTable: "ExchangeDetails",
                principalColumn: "ExchangeDetailsId");
        }
    }
}
