using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RefrigeracionControl.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSupervisorTechnicianAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupervisorTechnicianAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SupervisorId = table.Column<string>(type: "text", nullable: false),
                    TechnicianId = table.Column<string>(type: "text", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupervisorTechnicianAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupervisorTechnicianAssignments_AspNetUsers_SupervisorId",
                        column: x => x.SupervisorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupervisorTechnicianAssignments_AspNetUsers_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorTechnicianAssignments_SupervisorId",
                table: "SupervisorTechnicianAssignments",
                column: "SupervisorId");

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorTechnicianAssignments_TechnicianId",
                table: "SupervisorTechnicianAssignments",
                column: "TechnicianId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupervisorTechnicianAssignments");
        }
    }
}
