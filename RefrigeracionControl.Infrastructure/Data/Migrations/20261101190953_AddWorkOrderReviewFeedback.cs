using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RefrigeracionControl.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkOrderReviewFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewFeedback",
                table: "WorkOrders",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewFeedback",
                table: "WorkOrders");
        }
    }
}
