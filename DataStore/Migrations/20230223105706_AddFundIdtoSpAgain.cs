using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataStore.Migrations
{
    public partial class AddFundIdtoSpAgain : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FundId",
                table: "SPs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SPs_FundId",
                table: "SPs",
                column: "FundId");

            migrationBuilder.AddForeignKey(
                name: "FK_SPs_Funds_FundId",
                table: "SPs",
                column: "FundId",
                principalTable: "Funds",
                principalColumn: "FundId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SPs_Funds_FundId",
                table: "SPs");

            migrationBuilder.DropIndex(
                name: "IX_SPs_FundId",
                table: "SPs");

            migrationBuilder.DropColumn(
                name: "FundId",
                table: "SPs");
        }
    }
}
