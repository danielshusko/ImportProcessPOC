﻿// <auto-generated />
using System;
using ImportProcessPOC.Database.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ImportProcessPOC.Migrations
{
    [DbContext(typeof(ImportProcessDataContext))]
    [Migration("20241009122700_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ImportJobDataModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Importer")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsOrdered")
                        .HasColumnType("boolean");

                    b.Property<int>("ItemCount")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("ImportJob", "importProcessPoc");
                });

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ImportJobHeaderDataModel", b =>
                {
                    b.Property<int>("JobId")
                        .HasColumnType("integer");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<string>("Header")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("JobId", "Index");

                    b.ToTable("ImportJobHeader", "importProcessPoc");
                });

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ImportJobLineDataModel", b =>
                {
                    b.Property<int>("JobId")
                        .HasColumnType("integer");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<string>("Line")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer");

                    b.HasKey("JobId", "Index");

                    b.ToTable("ImportJobLine", "importProcessPoc");
                });

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ImportJobLineQueueDataModel", b =>
                {
                    b.Property<int>("JobId")
                        .HasColumnType("integer");

                    b.Property<int>("Index")
                        .HasColumnType("integer");

                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<bool>("IsProcessed")
                        .HasColumnType("boolean");

                    b.Property<string>("Line")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer");

                    b.HasKey("JobId", "Index");

                    b.HasIndex("ParentId", "IsProcessed");

                    b.ToTable("ImportJobLineQueue", "importProcessPoc");
                });

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ImportJobSpanModel", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTimeOffset>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("JobId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("JobId");

                    b.ToTable("ImportJobSpan", "importProcessPoc");
                });

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ItemDataModel", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("integer");

                    b.Property<Guid>("TenantId")
                        .HasColumnType("uuid");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer");

                    b.HasKey("Id", "TenantId");

                    b.HasIndex("ParentId", "TenantId");

                    b.ToTable("Item", "importProcessPoc");
                });

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ImportJobHeaderDataModel", b =>
                {
                    b.HasOne("ImportProcessPOC.Database.EF.Models.ImportJobDataModel", "Job")
                        .WithMany("Headers")
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Job");
                });

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ImportJobLineDataModel", b =>
                {
                    b.HasOne("ImportProcessPOC.Database.EF.Models.ImportJobDataModel", "Job")
                        .WithMany("Lines")
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Job");
                });

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ImportJobLineQueueDataModel", b =>
                {
                    b.HasOne("ImportProcessPOC.Database.EF.Models.ImportJobDataModel", "Job")
                        .WithMany("LineQueue")
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Job");
                });

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ImportJobSpanModel", b =>
                {
                    b.HasOne("ImportProcessPOC.Database.EF.Models.ImportJobDataModel", "Job")
                        .WithMany("Spans")
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Job");
                });

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ItemDataModel", b =>
                {
                    b.HasOne("ImportProcessPOC.Database.EF.Models.ItemDataModel", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId", "TenantId");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ImportJobDataModel", b =>
                {
                    b.Navigation("Headers");

                    b.Navigation("LineQueue");

                    b.Navigation("Lines");

                    b.Navigation("Spans");
                });

            modelBuilder.Entity("ImportProcessPOC.Database.EF.Models.ItemDataModel", b =>
                {
                    b.Navigation("Children");
                });
#pragma warning restore 612, 618
        }
    }
}
