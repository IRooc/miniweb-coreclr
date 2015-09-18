using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using MiniWeb.Storage.EFStorage;

namespace MiniWeb.Storage.EFStorage.Migrations
{
    [DbContext(typeof(MiniWebEFDbContext))]
    [Migration("20150918220154_First")]
    partial class First
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Annotation("ProductVersion", "7.0.0-beta8-15723")
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerIdentityStrategy.IdentityColumn);

            modelBuilder.Entity("MiniWeb.Storage.EFStorage.DbContentItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DbPageSectionKey");

                    b.Property<string>("Template");

                    b.Property<string>("Values");

                    b.Key("Id");
                });

            modelBuilder.Entity("MiniWeb.Storage.EFStorage.DbPageSection", b =>
                {
                    b.Property<string>("Key");

                    b.Property<string>("DbPageUrl");

                    b.Key("Key");
                });

            modelBuilder.Entity("MiniWeb.Storage.EFStorage.DbSitePage", b =>
                {
                    b.Property<string>("Url");

                    b.Property<DateTime>("Created");

                    b.Property<DateTime>("LastModified");

                    b.Property<string>("Layout");

                    b.Property<string>("MetaDescription");

                    b.Property<string>("MetaTitle");

                    b.Property<bool>("ShowInMenu");

                    b.Property<int>("SortOrder");

                    b.Property<string>("Template");

                    b.Property<string>("Title");

                    b.Property<bool>("Visible");

                    b.Key("Url");
                });

            modelBuilder.Entity("MiniWeb.Storage.EFStorage.DbContentItem", b =>
                {
                    b.Reference("MiniWeb.Storage.EFStorage.DbPageSection")
                        .InverseCollection()
                        .ForeignKey("DbPageSectionKey");
                });

            modelBuilder.Entity("MiniWeb.Storage.EFStorage.DbPageSection", b =>
                {
                    b.Reference("MiniWeb.Storage.EFStorage.DbSitePage")
                        .InverseCollection()
                        .ForeignKey("DbPageUrl");
                });
        }
    }
}
