using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiProject.Migrations
{
    /// <inheritdoc />
    public partial class RestrictProductDeletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Productos_ProductId",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Productos_ProductId",
                table: "Orders",
                column: "ProductId",
                principalTable: "Productos",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Productos_ProductId",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Productos_ProductId",
                table: "Orders",
                column: "ProductId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
