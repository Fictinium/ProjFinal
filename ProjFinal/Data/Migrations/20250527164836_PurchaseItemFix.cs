using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjFinal.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseItemFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileUrl",
                table: "Books",
                newName: "BookFile");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "BookImages",
                newName: "Image");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "PurchaseItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "PurchaseItems");

            migrationBuilder.RenameColumn(
                name: "BookFile",
                table: "Books",
                newName: "FileUrl");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "BookImages",
                newName: "ImageUrl");
        }
    }
}
