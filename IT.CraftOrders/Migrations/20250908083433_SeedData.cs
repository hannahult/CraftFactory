using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IT.CraftOrders.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "CustomerId", "Address", "Email", "Name", "Phone" },
                values: new object[,]
                {
                    { 1, "Examplegatan 1", "hanna@example.com", "Hanna Hult", "0701234567" },
                    { 2, "Examplegatan 2", "kevin@example.com", "Kevin Selin", "0702345678" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "Description", "IsActive", "Name", "Price", "Sku" },
                values: new object[,]
                {
                    { 1, null, true, "Craft Glue Stick", 19.90m, "GLUE-STICK" },
                    { 2, null, true, "Glitter Kit 6-pack", 49.00m, "GLITTER-KIT" },
                    { 3, null, true, "Coloured A4 Paper Pack", 39.00m, "PAPER-A4-COLOR" },
                    { 4, null, true, "Mixed Beads 200g", 59.00m, "BEADS-MIX" },
                    { 5, null, true, "Acrylic Paint Set 12pcs", 129.00m, "PAINT-SET" },
                    { 6, null, true, "Fine Brush Set 6pcs", 79.00m, "BRUSH-SET" },
                    { 7, null, true, "Safety Scissors for Kids", 29.90m, "SCISSORS-KIDS" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Customers",
                keyColumn: "CustomerId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "ProductId",
                keyValue: 7);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
