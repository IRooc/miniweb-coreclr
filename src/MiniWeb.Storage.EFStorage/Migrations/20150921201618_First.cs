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
                name: "DbContentItem",
                columns: table => new
                {
                    PageUrl = table.Column<string>(nullable: false),
                    Sortorder = table.Column<int>(nullable: false),
                    SectionKey = table.Column<string>(nullable: false),
                    Template = table.Column<string>(nullable: true),
                    Values = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbContentItem", x => new { x.PageUrl, x.Sortorder, x.SectionKey });
                    table.ForeignKey(
                        name: "FK_DbContentItem_DbSitePage_PageUrl",
                        column: x => x.PageUrl,
                        principalTable: "DbSitePage",
                        principalColumn: "Url");
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("DbContentItem");
            migrationBuilder.DropTable("DbSitePage");
        }
    }
}
