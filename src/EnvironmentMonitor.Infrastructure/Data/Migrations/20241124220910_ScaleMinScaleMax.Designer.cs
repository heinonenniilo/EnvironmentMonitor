﻿// <auto-generated />
using System;
using EnvironmentMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    [DbContext(typeof(MeasurementDbContext))]
    [Migration("20241124220910_ScaleMinScaleMax")]
    partial class ScaleMinScaleMax
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.Device", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DeviceIdentifier")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.HasKey("Id");

                    b.HasIndex("DeviceIdentifier")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.Measurement", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<DateTime>("CreatedAtUtc")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETUTCDATE()");

                    b.Property<int>("SensorId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("TimestampUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("TypeId")
                        .HasColumnType("int");

                    b.Property<double>("Value")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.HasIndex("SensorId");

                    b.HasIndex("TypeId");

                    b.ToTable("Measurements");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.MeasurementType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("MeasurementTypes");

                    b.HasData(
                        new
                        {
                            Id = 0,
                            Name = "Undefined",
                            Unit = "-"
                        },
                        new
                        {
                            Id = 1,
                            Name = "Temperature",
                            Unit = "C"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Humidity",
                            Unit = "%"
                        });
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.Sensor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("DeviceId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<double?>("ScaleMax")
                        .HasColumnType("float");

                    b.Property<double?>("ScaleMin")
                        .HasColumnType("float");

                    b.Property<int>("SensorId")
                        .HasColumnType("int");

                    b.Property<int?>("TypeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("TypeId");

                    b.HasIndex("DeviceId", "SensorId")
                        .IsUnique();

                    b.ToTable("Sensors");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.SensorType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Unit")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("SensorTypes");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.Measurement", b =>
                {
                    b.HasOne("EnvironmentMonitor.Domain.Entities.Sensor", "Sensor")
                        .WithMany("Measurements")
                        .HasForeignKey("SensorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("EnvironmentMonitor.Domain.Entities.MeasurementType", "Type")
                        .WithMany("Measurements")
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Sensor");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.Sensor", b =>
                {
                    b.HasOne("EnvironmentMonitor.Domain.Entities.Device", "Device")
                        .WithMany("Sensors")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("EnvironmentMonitor.Domain.Entities.SensorType", "Type")
                        .WithMany("Sensors")
                        .HasForeignKey("TypeId");

                    b.Navigation("Device");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.Device", b =>
                {
                    b.Navigation("Sensors");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.MeasurementType", b =>
                {
                    b.Navigation("Measurements");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.Sensor", b =>
                {
                    b.Navigation("Measurements");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.SensorType", b =>
                {
                    b.Navigation("Sensors");
                });
#pragma warning restore 612, 618
        }
    }
}
