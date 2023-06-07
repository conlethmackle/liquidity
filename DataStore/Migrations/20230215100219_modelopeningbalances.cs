using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStore.Migrations
{
    public partial class modelopeningbalances : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpeningExchangeBalances_ExchangeDetails_ExchangeDetailsId",
                table: "OpeningExchangeBalances");

            migrationBuilder.AlterColumn<int>(
                name: "ExchangeDetailsId",
                table: "OpeningExchangeBalances",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningExchangeBalances_ExchangeDetails_ExchangeDetailsId",
                table: "OpeningExchangeBalances",
                column: "ExchangeDetailsId",
                principalTable: "ExchangeDetails",
                principalColumn: "ExchangeDetailsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OpeningExchangeBalances_ExchangeDetails_ExchangeDetailsId",
                table: "OpeningExchangeBalances");

            migrationBuilder.AlterColumn<int>(
                name: "ExchangeDetailsId",
                table: "OpeningExchangeBalances",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningExchangeBalances_ExchangeDetails_ExchangeDetailsId",
                table: "OpeningExchangeBalances",
                column: "ExchangeDetailsId",
                principalTable: "ExchangeDetails",
                principalColumn: "ExchangeDetailsId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
