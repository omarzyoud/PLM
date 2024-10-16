using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PLM.api.Migrations.PLMAuthDb
{
    /// <inheritdoc />
    public partial class dbauthmig2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e2150bc3-1789-4ad0-bf5b-5b84242089e7",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "User", "USER" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e2150bc3-1789-4ad0-bf5b-5b84242089e7",
                columns: new[] { "Name", "NormalizedName" },
                values: new object[] { "Customer", "CUSTOMER" });
        }
    }
}
