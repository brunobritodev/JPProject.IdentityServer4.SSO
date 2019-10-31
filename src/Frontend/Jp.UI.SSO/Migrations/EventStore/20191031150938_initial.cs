using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Jp.UI.SSO.Migrations.EventStore
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoredEvent",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Action = table.Column<string>(type: "varchar(100)", nullable: true),
                    AggregateId = table.Column<string>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    EventType = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    LocalIpAddress = table.Column<string>(nullable: true),
                    RemoteIpAddress = table.Column<string>(nullable: true),
                    User = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredEvent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoredEventDetails",
                columns: table => new
                {
                    EventId = table.Column<Guid>(nullable: false),
                    Metadata = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredEventDetails", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_StoredEventDetails_StoredEvent_EventId",
                        column: x => x.EventId,
                        principalTable: "StoredEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoredEventDetails");

            migrationBuilder.DropTable(
                name: "StoredEvent");
        }
    }
}
