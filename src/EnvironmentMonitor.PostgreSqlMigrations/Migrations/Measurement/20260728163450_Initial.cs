using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EnvironmentMonitor.PostgreSqlMigrations.Migrations.Measurement
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Extension = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OriginalName = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    FullPath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    IsSecret = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunicationChannels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunicationChannels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceAttributeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAttributeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceEventTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEventTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceQueuedCommandTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceQueuedCommandTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Identifier = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Message = table.Column<string>(type: "text", maxLength: 256, nullable: true),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "LOCALTIMESTAMP"),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Updated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Identifier = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Visible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Unit = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SensorTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Unit = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DeviceIdentifier = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Visible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    TypeId = table.Column<int>(type: "integer", nullable: true),
                    HasMotionSensor = table.Column<bool>(type: "boolean", nullable: false),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    IsVirtual = table.Column<bool>(type: "boolean", nullable: false),
                    CommunicationChannelId = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.UniqueConstraint("AK_Devices_Id_LocationId", x => new { x.Id, x.LocationId });
                    table.ForeignKey(
                        name: "FK_Devices_CommunicationChannels_CommunicationChannelId",
                        column: x => x.CommunicationChannelId,
                        principalTable: "CommunicationChannels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Devices_DeviceTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DeviceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Devices_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceAttachments",
                columns: table => new
                {
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    AttachmentId = table.Column<int>(type: "integer", nullable: false),
                    Guid = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    IsDefaultImage = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeviceImage = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAttachments", x => new { x.DeviceId, x.AttachmentId });
                    table.ForeignKey(
                        name: "FK_DeviceAttachments_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceAttachments_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceAttributes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeStampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceAttributes_DeviceAttributeTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DeviceAttributeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceAttributes_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceContacts_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimeStamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeStampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    TypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceEvents_DeviceEventTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "DeviceEventTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceEvents_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeStampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: true),
                    FirstMessage = table.Column<bool>(type: "boolean", nullable: false),
                    Uptime = table.Column<long>(type: "bigint", nullable: true),
                    Identifier = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LoopCount = table.Column<long>(type: "bigint", nullable: true),
                    MessageCount = table.Column<long>(type: "bigint", nullable: true),
                    IsDuplicate = table.Column<bool>(type: "boolean", nullable: false),
                    SourceId = table.Column<int>(type: "integer", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceMessages_CommunicationChannels_SourceId",
                        column: x => x.SourceId,
                        principalTable: "CommunicationChannels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceMessages_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeviceQueuedCommands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PopReceipt = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    Scheduled = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ScheduledUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExecutedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", maxLength: 256, nullable: false),
                    IsRemoved = table.Column<bool>(type: "boolean", nullable: false),
                    OriginalId = table.Column<int>(type: "integer", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceQueuedCommands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceQueuedCommands_DeviceQueuedCommandTypes_Type",
                        column: x => x.Type,
                        principalTable: "DeviceQueuedCommandTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceQueuedCommands_DeviceQueuedCommands_OriginalId",
                        column: x => x.OriginalId,
                        principalTable: "DeviceQueuedCommands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DeviceQueuedCommands_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    SensorId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: true),
                    ScaleMin = table.Column<double>(type: "double precision", nullable: true),
                    ScaleMax = table.Column<double>(type: "double precision", nullable: true),
                    IsVirtual = table.Column<bool>(type: "boolean", nullable: false),
                    AggregationType = table.Column<int>(type: "integer", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.Id);
                    table.UniqueConstraint("AK_Sensors_Id_DeviceId", x => new { x.Id, x.DeviceId });
                    table.ForeignKey(
                        name: "FK_Sensors_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sensors_SensorTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "SensorTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeviceStatusChanges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimeStampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "text", maxLength: 256, nullable: true),
                    DeviceMessageId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceStatusChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceStatusChanges_DeviceMessages_DeviceMessageId",
                        column: x => x.DeviceMessageId,
                        principalTable: "DeviceMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeviceStatusChanges_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationSensors",
                columns: table => new
                {
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    SensorId = table.Column<int>(type: "integer", nullable: false),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationSensors", x => new { x.LocationId, x.SensorId, x.DeviceId });
                    table.ForeignKey(
                        name: "FK_LocationSensors_Devices_DeviceId_LocationId",
                        columns: x => new { x.DeviceId, x.LocationId },
                        principalTable: "Devices",
                        principalColumns: new[] { "Id", "LocationId" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LocationSensors_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationSensors_MeasurementTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "MeasurementTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocationSensors_Sensors_SensorId_DeviceId",
                        columns: x => new { x.SensorId, x.DeviceId },
                        principalTable: "Sensors",
                        principalColumns: new[] { "Id", "DeviceId" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Measurements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SensorId = table.Column<int>(type: "integer", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "LOCALTIMESTAMP"),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "LOCALTIMESTAMP"),
                    DeviceMessageId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Measurements_DeviceMessages_DeviceMessageId",
                        column: x => x.DeviceMessageId,
                        principalTable: "DeviceMessages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Measurements_MeasurementTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "MeasurementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Measurements_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PublicSensors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identifier = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SensorId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicSensors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicSensors_MeasurementTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "MeasurementTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PublicSensors_Sensors_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VirtualSensorRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VirtualSensorId = table.Column<int>(type: "integer", nullable: false),
                    ValueSensorId = table.Column<int>(type: "integer", nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "LOCALTIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualSensorRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VirtualSensorRows_MeasurementTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "MeasurementTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VirtualSensorRows_Sensors_ValueSensorId",
                        column: x => x.ValueSensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VirtualSensorRows_Sensors_VirtualSensorId",
                        column: x => x.VirtualSensorId,
                        principalTable: "Sensors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CommunicationChannels",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 0, "IoT Hub", "IotHub" },
                    { 1, "Rest API", "RestApi" },
                    { 2, "Ilmatieteenlaitos Open Api", "IlmatieteenLaitos" }
                });

            migrationBuilder.InsertData(
                table: "DeviceAttributeTypes",
                columns: new[] { "Id", "Description", "Name", "Type" },
                values: new object[,]
                {
                    { 0, "Motion control status. 0=AlwaysOff,1=AlwaysOn,2=MotionControl", "MotionControlStatus", "int" },
                    { 1, "Output delay in ms when motion control is on.", "OnDelay", "int" }
                });

            migrationBuilder.InsertData(
                table: "DeviceEventTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 0, "Reboot command", "RebootCommand" },
                    { 1, "First message after boot / online since", "Online" },
                    { 2, "Set motion control status", "SetMotionControlStatus" },
                    { 3, "Set motion control delays", "SetMotionControlDelay" },
                    { 4, "Send stored attributes", "SendAttributes" }
                });

            migrationBuilder.InsertData(
                table: "DeviceQueuedCommandTypes",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 0, "Send stored device attributes", "SendDeviceAttributes" },
                    { 1, "Set motion control status", "SetMotionControlStatus" },
                    { 2, "Set motion control delay", "SetMotionControlOnDelay" },
                    { 3, "Send email about a device", "SendDeviceEmail" },
                    { 4, "Process forget user password request", "ProcessForgetUserPasswordRequest" }
                });

            migrationBuilder.InsertData(
                table: "EmailTemplates",
                columns: new[] { "Id", "Created", "Message", "Name", "Title", "Updated" },
                values: new object[,]
                {
                    { 0, null, null, "Device connection lost", null, null },
                    { 1, null, null, "Device connection restored", null, null },
                    { 2, null, "Please confirm your email address by clicking the link below:\n\n{ConfirmationLink}\n\nIf you did not create an account, please ignore this email.", "Confirm Email", "Confirm Your Email Address", null },
                    { 3, null, "You have requested to reset your password. Please click the link below to set a new password:\n\n{ResetLink}\n\nIf you did not request a password reset, please ignore this email.", "User Password Reset", "Reset Your Password", null }
                });

            migrationBuilder.InsertData(
                table: "MeasurementTypes",
                columns: new[] { "Id", "Name", "Unit" },
                values: new object[,]
                {
                    { 0, "Undefined", "-" },
                    { 1, "Temperature", "C" },
                    { 2, "Humidity", "%" },
                    { 3, "Light", "Lx" },
                    { 4, "Motion", "ON/OFF" },
                    { 5, "Pressure", "-" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_Name",
                table: "Attachments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAttachments_AttachmentId",
                table: "DeviceAttachments",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAttachments_DeviceId",
                table: "DeviceAttachments",
                column: "DeviceId",
                unique: true,
                filter: "\"IsDefaultImage\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAttachments_Guid",
                table: "DeviceAttachments",
                column: "Guid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAttributes_DeviceId_TypeId",
                table: "DeviceAttributes",
                columns: new[] { "DeviceId", "TypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAttributes_TimeStamp_DeviceId",
                table: "DeviceAttributes",
                columns: new[] { "TimeStamp", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAttributes_TypeId",
                table: "DeviceAttributes",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceAttributeTypes_Name",
                table: "DeviceAttributeTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceContacts_DeviceId_Email",
                table: "DeviceContacts",
                columns: new[] { "DeviceId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceContacts_Identifier",
                table: "DeviceContacts",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_DeviceId",
                table: "DeviceEvents",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_TimeStamp_DeviceId",
                table: "DeviceEvents",
                columns: new[] { "TimeStamp", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_TypeId",
                table: "DeviceEvents",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEventTypes_Name",
                table: "DeviceEventTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMessages_DeviceId_TimeStamp",
                table: "DeviceMessages",
                columns: new[] { "DeviceId", "TimeStamp" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMessages_Identifier_DeviceId",
                table: "DeviceMessages",
                columns: new[] { "Identifier", "DeviceId" },
                unique: true,
                filter: "\"Identifier\" IS NOT NULL AND \"IsDuplicate\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMessages_SourceId",
                table: "DeviceMessages",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceMessages_TimeStamp",
                table: "DeviceMessages",
                column: "TimeStamp");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceQueuedCommands_DeviceId_ExecutedAtUtc",
                table: "DeviceQueuedCommands",
                columns: new[] { "DeviceId", "ExecutedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceQueuedCommands_DeviceId_ScheduledUtc",
                table: "DeviceQueuedCommands",
                columns: new[] { "DeviceId", "ScheduledUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceQueuedCommands_MessageId",
                table: "DeviceQueuedCommands",
                column: "MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceQueuedCommands_OriginalId",
                table: "DeviceQueuedCommands",
                column: "OriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceQueuedCommands_Type",
                table: "DeviceQueuedCommands",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceQueuedCommandTypes_Name",
                table: "DeviceQueuedCommandTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_CommunicationChannelId",
                table: "Devices",
                column: "CommunicationChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceIdentifier",
                table: "Devices",
                column: "DeviceIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Identifier",
                table: "Devices",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_LocationId",
                table: "Devices",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Name_LocationId",
                table: "Devices",
                columns: new[] { "Name", "LocationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_TypeId",
                table: "Devices",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatusChanges_DeviceId",
                table: "DeviceStatusChanges",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatusChanges_DeviceMessageId",
                table: "DeviceStatusChanges",
                column: "DeviceMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceStatusChanges_TimeStamp_DeviceId_Status",
                table: "DeviceStatusChanges",
                columns: new[] { "TimeStamp", "DeviceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTypes_Name",
                table: "DeviceTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Identifier",
                table: "EmailTemplates",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Name",
                table: "EmailTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Identifier",
                table: "Locations",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationSensors_DeviceId_LocationId",
                table: "LocationSensors",
                columns: new[] { "DeviceId", "LocationId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationSensors_SensorId_DeviceId",
                table: "LocationSensors",
                columns: new[] { "SensorId", "DeviceId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationSensors_TypeId",
                table: "LocationSensors",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_DeviceMessageId",
                table: "Measurements",
                column: "DeviceMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_SensorId",
                table: "Measurements",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_SensorId_Timestamp",
                table: "Measurements",
                columns: new[] { "SensorId", "Timestamp" })
                .Annotation("Npgsql:IndexInclude", new[] { "Value", "TypeId", "TimestampUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_Timestamp",
                table: "Measurements",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Measurements_TypeId",
                table: "Measurements",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementTypes_Name",
                table: "MeasurementTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicSensors_Identifier",
                table: "PublicSensors",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicSensors_SensorId",
                table: "PublicSensors",
                column: "SensorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicSensors_TypeId",
                table: "PublicSensors",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_DeviceId_Name",
                table: "Sensors",
                columns: new[] { "DeviceId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_DeviceId_SensorId",
                table: "Sensors",
                columns: new[] { "DeviceId", "SensorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_Identifier",
                table: "Sensors",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sensors_TypeId",
                table: "Sensors",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorTypes_Name",
                table: "SensorTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VirtualSensorRows_TypeId",
                table: "VirtualSensorRows",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualSensorRows_ValueSensorId",
                table: "VirtualSensorRows",
                column: "ValueSensorId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualSensorRows_VirtualSensorId_ValueSensorId",
                table: "VirtualSensorRows",
                columns: new[] { "VirtualSensorId", "ValueSensorId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceAttachments");

            migrationBuilder.DropTable(
                name: "DeviceAttributes");

            migrationBuilder.DropTable(
                name: "DeviceContacts");

            migrationBuilder.DropTable(
                name: "DeviceEvents");

            migrationBuilder.DropTable(
                name: "DeviceQueuedCommands");

            migrationBuilder.DropTable(
                name: "DeviceStatusChanges");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "LocationSensors");

            migrationBuilder.DropTable(
                name: "Measurements");

            migrationBuilder.DropTable(
                name: "PublicSensors");

            migrationBuilder.DropTable(
                name: "VirtualSensorRows");

            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "DeviceAttributeTypes");

            migrationBuilder.DropTable(
                name: "DeviceEventTypes");

            migrationBuilder.DropTable(
                name: "DeviceQueuedCommandTypes");

            migrationBuilder.DropTable(
                name: "DeviceMessages");

            migrationBuilder.DropTable(
                name: "MeasurementTypes");

            migrationBuilder.DropTable(
                name: "Sensors");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "SensorTypes");

            migrationBuilder.DropTable(
                name: "CommunicationChannels");

            migrationBuilder.DropTable(
                name: "DeviceTypes");

            migrationBuilder.DropTable(
                name: "Locations");
        }
    }
}
