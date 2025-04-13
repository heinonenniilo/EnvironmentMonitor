﻿// <auto-generated />
using System;
using EnvironmentMonitor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EnvironmentMonitor.Infrastructure.Data.Migrations
{
    [DbContext(typeof(MeasurementDbContext))]
    partial class MeasurementDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

                    b.Property<bool>("HasMotionSensor")
                        .HasColumnType("bit");

                    b.Property<int>("LocationId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.Property<int?>("TypeId")
                        .HasColumnType("int");

                    b.Property<bool>("Visible")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.HasKey("Id");

                    b.HasIndex("DeviceIdentifier")
                        .IsUnique();

                    b.HasIndex("LocationId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("TypeId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.DeviceEvent", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<int>("DeviceId")
                        .HasColumnType("int");

                    b.Property<string>("Message")
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<DateTime>("TimeStamp")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("TimeStampUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("TypeId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId");

                    b.HasIndex("TypeId");

                    b.HasIndex("TimeStamp", "DeviceId");

                    b.ToTable("DeviceEvents");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.DeviceEventType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("DeviceEventTypes");

                    b.HasData(
                        new
                        {
                            Id = 0,
                            Description = "Reboot command",
                            Name = "RebootCommand"
                        },
                        new
                        {
                            Id = 1,
                            Description = "First message after boot / online since",
                            Name = "Online"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Set motion control status",
                            Name = "SetMotionControlStatus"
                        },
                        new
                        {
                            Id = 3,
                            Description = "Set motion control delays",
                            Name = "SetMotionControlDelay"
                        });
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.DeviceType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("DeviceTypes");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.Location", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<Guid>("Identifier")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("NEWID()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)");

                    b.HasKey("Id");

                    b.HasIndex("Identifier")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.LocationSensor", b =>
                {
                    b.Property<int>("LocationId")
                        .HasColumnType("int");

                    b.Property<int>("SensorId")
                        .HasColumnType("int");

                    b.Property<int>("DeviceId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<int?>("TypeId")
                        .HasColumnType("int");

                    b.HasKey("LocationId", "SensorId", "DeviceId");

                    b.HasIndex("TypeId");

                    b.HasIndex("DeviceId", "LocationId");

                    b.HasIndex("SensorId", "DeviceId");

                    b.ToTable("LocationSensors");
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

                    b.HasIndex("Timestamp");

                    b.HasIndex("TypeId");

                    b.HasIndex("SensorId", "Timestamp");

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
                        },
                        new
                        {
                            Id = 3,
                            Name = "Light",
                            Unit = "Lx"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Motion",
                            Unit = "ON/OFF"
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

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.Device", b =>
                {
                    b.HasOne("EnvironmentMonitor.Domain.Entities.Location", "Location")
                        .WithMany("Devices")
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EnvironmentMonitor.Domain.Entities.DeviceType", "Type")
                        .WithMany("Devices")
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Location");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.DeviceEvent", b =>
                {
                    b.HasOne("EnvironmentMonitor.Domain.Entities.Device", "Device")
                        .WithMany("Events")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EnvironmentMonitor.Domain.Entities.DeviceEventType", "Type")
                        .WithMany("Events")
                        .HasForeignKey("TypeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Device");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.LocationSensor", b =>
                {
                    b.HasOne("EnvironmentMonitor.Domain.Entities.Location", "Location")
                        .WithMany("LocationSensors")
                        .HasForeignKey("LocationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EnvironmentMonitor.Domain.Entities.MeasurementType", "MeasurementType")
                        .WithMany("LocationSensors")
                        .HasForeignKey("TypeId");

                    b.HasOne("EnvironmentMonitor.Domain.Entities.Device", "Device")
                        .WithMany("LocationSensors")
                        .HasForeignKey("DeviceId", "LocationId")
                        .HasPrincipalKey("Id", "LocationId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("EnvironmentMonitor.Domain.Entities.Sensor", "Sensor")
                        .WithMany("LocationSensors")
                        .HasForeignKey("SensorId", "DeviceId")
                        .HasPrincipalKey("Id", "DeviceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Device");

                    b.Navigation("Location");

                    b.Navigation("MeasurementType");

                    b.Navigation("Sensor");
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
                    b.Navigation("Events");

                    b.Navigation("LocationSensors");

                    b.Navigation("Sensors");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.DeviceEventType", b =>
                {
                    b.Navigation("Events");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.DeviceType", b =>
                {
                    b.Navigation("Devices");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.Location", b =>
                {
                    b.Navigation("Devices");

                    b.Navigation("LocationSensors");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.MeasurementType", b =>
                {
                    b.Navigation("LocationSensors");

                    b.Navigation("Measurements");
                });

            modelBuilder.Entity("EnvironmentMonitor.Domain.Entities.Sensor", b =>
                {
                    b.Navigation("LocationSensors");

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
