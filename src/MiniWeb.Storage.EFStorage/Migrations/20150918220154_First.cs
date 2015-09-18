using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace MiniWeb.Storage.EFStorage.Migrations
{
    public partial class First : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DbSitePage",
                columns: table => new
                {
                    Url = table.Column<string>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    LastModified = table.Column<DateTime>(nullable: false),
                    Layout = table.Column<string>(nullable: true),
                    MetaDescription = table.Column<string>(nullable: true),
                    MetaTitle = table.Column<string>(nullable: true),
                    ShowInMenu = table.Column<bool>(nullable: false),
                    SortOrder = table.Column<int>(nullable: false),
                    Template = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    Visible = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbSitePage", x => x.Url);
                });
            migrationBuilder.CreateTable(
                name: "DbPageSection",
                columns: table => new
                {
                    Key = table.Column<string>(nullable: false),
                    DbPageUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbPageSection", x => x.Key);
                    table.ForeignKey(
                        name: "FK_DbPageSection_DbSitePage_DbPageUrl",
                        column: x => x.DbPageUrl,
                        principalTable: "DbSitePage",
                        principalColumn: "Url");
                });
            migrationBuilder.CreateTable(
                name: "DbContentItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DbPageSectionKey = table.Column<string>(nullable: true),
                    Template = table.Column<string>(nullable: true),
                    Values = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbContentItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DbContentItem_DbPageSection_DbPageSectionKey",
                        column: x => x.DbPageSectionKey,
                        principalTable: "DbPageSection",
                        principalColumn: "Key");
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("DbContentItem");
            migrationBuilder.DropTable("DbPageSection");
            migrationBuilder.DropTable("DbSitePage");
        }
    }
}
