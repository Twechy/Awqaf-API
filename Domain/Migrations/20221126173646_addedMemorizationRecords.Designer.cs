﻿// <auto-generated />
using System;
using Domain.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Domain.Migrations
{
    [DbContext(typeof(AwqafDb))]
    [Migration("20221126173646_addedMemorizationRecords")]
    partial class addedMemorizationRecords
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Domain.Models.City", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Active")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("RegionId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("RegionId");

                    b.ToTable("Cities");
                });

            modelBuilder.Entity("Domain.Models.Guardian", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Active")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("MosqueId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Pin")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("MosqueId");

                    b.ToTable("Guardians");
                });

            modelBuilder.Entity("Domain.Models.Memorization", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Active")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("CurrentSurah")
                        .HasColumnType("int");

                    b.Property<Guid>("StudentId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("SurahMemorized")
                        .HasColumnType("int");

                    b.Property<double>("TotalGrade")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("StudentId");

                    b.ToTable("Memorizations");
                });

            modelBuilder.Entity("Domain.Models.MemorizationRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Active")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("From")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Grade")
                        .HasColumnType("int");

                    b.Property<Guid>("MemorizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SurahId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("To")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("MemorizationId");

                    b.ToTable("MemorizationRecords");
                });

            modelBuilder.Entity("Domain.Models.Mosque", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Active")
                        .HasColumnType("int");

                    b.Property<Guid>("CityId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Location")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CityId");

                    b.ToTable("Mosques");
                });

            modelBuilder.Entity("Domain.Models.Region", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Active")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Regions");
                });

            modelBuilder.Entity("Domain.Models.Student", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Active")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("GuardianId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("MosqueId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Pin")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GuardianId");

                    b.HasIndex("MosqueId");

                    b.ToTable("GuardianMembers");
                });

            modelBuilder.Entity("Domain.Models.Worker", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Active")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("MosqueId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Pin")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("MosqueId");

                    b.ToTable("Workers");
                });

            modelBuilder.Entity("Domain.Models.City", b =>
                {
                    b.HasOne("Domain.Models.Region", null)
                        .WithMany("Cities")
                        .HasForeignKey("RegionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Models.Guardian", b =>
                {
                    b.HasOne("Domain.Models.Mosque", null)
                        .WithMany("Guardians")
                        .HasForeignKey("MosqueId");
                });

            modelBuilder.Entity("Domain.Models.Memorization", b =>
                {
                    b.HasOne("Domain.Models.Student", "Student")
                        .WithMany()
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Student");
                });

            modelBuilder.Entity("Domain.Models.MemorizationRecord", b =>
                {
                    b.HasOne("Domain.Models.Memorization", "Memorization")
                        .WithMany("MemorizationRecords")
                        .HasForeignKey("MemorizationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Memorization");
                });

            modelBuilder.Entity("Domain.Models.Mosque", b =>
                {
                    b.HasOne("Domain.Models.City", null)
                        .WithMany("Mosques")
                        .HasForeignKey("CityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Models.Student", b =>
                {
                    b.HasOne("Domain.Models.Guardian", null)
                        .WithMany("GuardianMembers")
                        .HasForeignKey("GuardianId");

                    b.HasOne("Domain.Models.Mosque", null)
                        .WithMany("GuardianMembers")
                        .HasForeignKey("MosqueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Domain.Models.Worker", b =>
                {
                    b.HasOne("Domain.Models.Mosque", "Mosque")
                        .WithMany("Workers")
                        .HasForeignKey("MosqueId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Mosque");
                });

            modelBuilder.Entity("Domain.Models.City", b =>
                {
                    b.Navigation("Mosques");
                });

            modelBuilder.Entity("Domain.Models.Guardian", b =>
                {
                    b.Navigation("GuardianMembers");
                });

            modelBuilder.Entity("Domain.Models.Memorization", b =>
                {
                    b.Navigation("MemorizationRecords");
                });

            modelBuilder.Entity("Domain.Models.Mosque", b =>
                {
                    b.Navigation("Guardians");

                    b.Navigation("GuardianMembers");

                    b.Navigation("Workers");
                });

            modelBuilder.Entity("Domain.Models.Region", b =>
                {
                    b.Navigation("Cities");
                });
#pragma warning restore 612, 618
        }
    }
}
