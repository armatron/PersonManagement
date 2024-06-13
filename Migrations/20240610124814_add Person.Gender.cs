using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonManagement.Migrations
{
    /// <inheritdoc />
    public partial class addPersonGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Person",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Person");
        }
    }
}
