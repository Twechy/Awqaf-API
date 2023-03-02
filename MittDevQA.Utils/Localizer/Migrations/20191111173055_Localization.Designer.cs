using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Utils.Localizer.DbLocalizer;

namespace  MittDevQA.Utils.Localizer.Migrations
{
    [DbContext(typeof(LocalizationModelContext))]
    [Migration("20191111173055_Localization")]
    partial class Localization
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

             
            modelBuilder.Entity("Localization.SqlLocalizer.DbStringLocalizer.LocalizationRecord", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Key")
                        .IsRequired();

                    b.Property<string>("LocalizationCulture")
                        .IsRequired();

                    b.Property<string>("ResourceKey")
                        .IsRequired();

                    b.Property<string>("Text");

                    b.Property<DateTime>("UpdatedTimestamp");

                    b.HasKey("Id");

                    b.HasAlternateKey("Key", "LocalizationCulture", "ResourceKey");

                    b.ToTable("LocalizationRecords");
                });
#pragma warning restore 612, 618
        }
    }
}