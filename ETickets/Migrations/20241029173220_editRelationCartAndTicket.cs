using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETickets.Migrations
{
    /// <inheritdoc />
    public partial class editRelationCartAndTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Tickets_TicketId",
                table: "Carts");

            migrationBuilder.DropIndex(
                name: "IX_Carts_TicketId",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "Count",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "Carts");

            migrationBuilder.AddColumn<int>(
                name: "CartId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CartId",
                table: "Tickets",
                column: "CartId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_Carts_CartId",
                table: "Tickets",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_Carts_CartId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CartId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CartId",
                table: "Tickets");

            migrationBuilder.AddColumn<int>(
                name: "Count",
                table: "Carts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TicketId",
                table: "Carts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Carts_TicketId",
                table: "Carts",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Tickets_TicketId",
                table: "Carts",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
