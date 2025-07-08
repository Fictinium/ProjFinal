using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjFinal.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Users_UserId",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_UserId",
                table: "Purchases");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Purchases",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Purchases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_UserId1",
                table: "Purchases",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Users_UserId1",
                table: "Purchases",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Users_UserId1",
                table: "Purchases");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_UserId1",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Purchases");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Purchases",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_UserId",
                table: "Purchases",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Users_UserId",
                table: "Purchases",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
