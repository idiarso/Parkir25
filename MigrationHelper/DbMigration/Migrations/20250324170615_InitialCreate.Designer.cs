﻿// <auto-generated />
using System;
using DbMigration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DbMigration.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250324170615_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.27")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("DbMigration.CameraSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Brightness")
                        .HasColumnType("integer");

                    b.Property<int>("CameraIndex")
                        .HasColumnType("integer");

                    b.Property<int>("Contrast")
                        .HasColumnType("integer");

                    b.Property<bool>("EnableTextRecognition")
                        .HasColumnType("boolean");

                    b.Property<int>("Exposure")
                        .HasColumnType("integer");

                    b.Property<bool>("IsDefaultProfile")
                        .HasColumnType("boolean");

                    b.Property<string>("LightingCondition")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("Normal");

                    b.Property<string>("Notes")
                        .HasColumnType("text");

                    b.Property<string>("ProfileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Resolution")
                        .HasColumnType("integer");

                    b.Property<int>("Saturation")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("CameraSettings");
                });

            modelBuilder.Entity("DbMigration.EntryGate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("BaudRate")
                        .HasColumnType("integer");

                    b.Property<int?>("CameraIndex")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("GateType")
                        .HasColumnType("text");

                    b.Property<string>("IpAddress")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<string>("Location")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int?>("Port")
                        .HasColumnType("integer");

                    b.Property<string>("SerialPort")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("EntryGates");
                });

            modelBuilder.Entity("DbMigration.Journal", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EntityId")
                        .HasColumnType("text");

                    b.Property<string>("EntityType")
                        .HasColumnType("text");

                    b.Property<string>("NewValue")
                        .HasColumnType("text");

                    b.Property<string>("OldValue")
                        .HasColumnType("text");

                    b.Property<string>("OperatorId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("OperatorId");

                    b.ToTable("Journals");
                });

            modelBuilder.Entity("DbMigration.Operator", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("AccessLevel")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("EmployeeId")
                        .HasColumnType("text");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsOnDuty")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("Position")
                        .HasColumnType("text");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<DateTime?>("ShiftEndTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ShiftStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<int?>("WorkstationId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("DbMigration.ParkingRateConfiguration", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("BaseRate")
                        .HasColumnType("numeric(18,2)");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("DailyRate")
                        .HasColumnType("numeric(18,2)");

                    b.Property<DateTime>("EffectiveFrom")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("EffectiveTo")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("HourlyRate")
                        .HasColumnType("numeric(18,2)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<string>("LastModifiedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LastModifiedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("MonthlyRate")
                        .HasColumnType("numeric(18,2)");

                    b.Property<string>("Notes")
                        .HasColumnType("text");

                    b.Property<decimal>("PenaltyRate")
                        .HasColumnType("numeric(18,2)");

                    b.Property<string>("VehicleType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("WeeklyRate")
                        .HasColumnType("numeric(18,2)");

                    b.HasKey("Id");

                    b.ToTable("ParkingRates");
                });

            modelBuilder.Entity("DbMigration.ParkingSpace", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("CurrentVehicleId")
                        .HasColumnType("integer");

                    b.Property<decimal>("HourlyRate")
                        .HasColumnType("numeric(18,2)");

                    b.Property<bool>("IsOccupied")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsReserved")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastOccupiedTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Location")
                        .HasColumnType("text");

                    b.Property<string>("ReservedFor")
                        .HasColumnType("text");

                    b.Property<string>("SpaceNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SpaceType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CurrentVehicleId")
                        .IsUnique();

                    b.ToTable("ParkingSpaces");
                });

            modelBuilder.Entity("DbMigration.ParkingTicket", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("BarcodeData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ExitOperatorId")
                        .HasColumnType("text");

                    b.Property<bool>("IsUsed")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("IssueTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("OperatorId")
                        .HasColumnType("text");

                    b.Property<int?>("ShiftId")
                        .HasColumnType("integer");

                    b.Property<string>("TicketNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("UsedTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("VehicleId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("OperatorId");

                    b.HasIndex("ShiftId");

                    b.HasIndex("VehicleId");

                    b.ToTable("ParkingTickets");
                });

            modelBuilder.Entity("DbMigration.ParkingTransaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<decimal?>("Discount")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("EntryTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ExitTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("HourlyRate")
                        .HasColumnType("numeric(18,2)");

                    b.Property<string>("Notes")
                        .HasColumnType("text");

                    b.Property<string>("OperatorId")
                        .HasColumnType("text");

                    b.Property<int>("ParkingSpaceId")
                        .HasColumnType("integer");

                    b.Property<string>("PaymentMethod")
                        .HasColumnType("text");

                    b.Property<string>("PaymentStatus")
                        .HasColumnType("text");

                    b.Property<DateTime?>("PaymentTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal?>("Tax")
                        .HasColumnType("numeric");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("numeric(18,2)");

                    b.Property<string>("TransactionNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("VehicleId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ParkingSpaceId");

                    b.HasIndex("VehicleId");

                    b.ToTable("ParkingTransactions");
                });

            modelBuilder.Entity("DbMigration.PrinterConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool?>("AutoCut")
                        .HasColumnType("boolean");

                    b.Property<int?>("CharactersPerLine")
                        .HasColumnType("integer");

                    b.Property<string>("ConnectionType")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("Serial");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<DateTime>("LastChecked")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Port")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("Offline");

                    b.HasKey("Id");

                    b.ToTable("PrinterConfigs");
                });

            modelBuilder.Entity("DbMigration.Shift", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<TimeSpan>("EndTime")
                        .HasColumnType("interval");

                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastModified")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ModifiedBy")
                        .HasColumnType("text");

                    b.Property<string>("ShiftName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("interval");

                    b.HasKey("Id");

                    b.ToTable("Shifts");
                });

            modelBuilder.Entity("DbMigration.SiteSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .HasColumnType("text");

                    b.Property<string>("ContactEmail")
                        .HasColumnType("text");

                    b.Property<string>("ContactPhone")
                        .HasColumnType("text");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LogoPath")
                        .HasColumnType("text");

                    b.Property<bool?>("ShowLogo")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<string>("SiteName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ThemeColor")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasDefaultValue("#007bff");

                    b.Property<string>("UpdatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("SiteSettings");
                });

            modelBuilder.Entity("DbMigration.Vehicle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("EntryTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ExitTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ImagePath")
                        .HasColumnType("text");

                    b.Property<bool>("IsParked")
                        .HasColumnType("boolean");

                    b.Property<int?>("ParkingSpaceId")
                        .HasColumnType("integer");

                    b.Property<int?>("ShiftId")
                        .HasColumnType("integer");

                    b.Property<string>("VehicleNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("VehicleType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ShiftId");

                    b.ToTable("Vehicles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("OperatorShift", b =>
                {
                    b.Property<string>("OperatorsId")
                        .HasColumnType("text");

                    b.Property<int>("ShiftsId")
                        .HasColumnType("integer");

                    b.HasKey("OperatorsId", "ShiftsId");

                    b.HasIndex("ShiftsId");

                    b.ToTable("OperatorShifts", (string)null);
                });

            modelBuilder.Entity("DbMigration.Journal", b =>
                {
                    b.HasOne("DbMigration.Operator", "Operator")
                        .WithMany()
                        .HasForeignKey("OperatorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Operator");
                });

            modelBuilder.Entity("DbMigration.ParkingSpace", b =>
                {
                    b.HasOne("DbMigration.Vehicle", "CurrentVehicle")
                        .WithOne("ParkingSpace")
                        .HasForeignKey("DbMigration.ParkingSpace", "CurrentVehicleId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("CurrentVehicle");
                });

            modelBuilder.Entity("DbMigration.ParkingTicket", b =>
                {
                    b.HasOne("DbMigration.Operator", "IssuedByOperator")
                        .WithMany()
                        .HasForeignKey("OperatorId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("DbMigration.Shift", "Shift")
                        .WithMany()
                        .HasForeignKey("ShiftId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("DbMigration.Vehicle", "Vehicle")
                        .WithMany()
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("IssuedByOperator");

                    b.Navigation("Shift");

                    b.Navigation("Vehicle");
                });

            modelBuilder.Entity("DbMigration.ParkingTransaction", b =>
                {
                    b.HasOne("DbMigration.ParkingSpace", "ParkingSpace")
                        .WithMany("Transactions")
                        .HasForeignKey("ParkingSpaceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("DbMigration.Vehicle", "Vehicle")
                        .WithMany("Transactions")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("ParkingSpace");

                    b.Navigation("Vehicle");
                });

            modelBuilder.Entity("DbMigration.Vehicle", b =>
                {
                    b.HasOne("DbMigration.Shift", "Shift")
                        .WithMany("Vehicles")
                        .HasForeignKey("ShiftId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Shift");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("DbMigration.Operator", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("DbMigration.Operator", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DbMigration.Operator", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("DbMigration.Operator", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OperatorShift", b =>
                {
                    b.HasOne("DbMigration.Operator", null)
                        .WithMany()
                        .HasForeignKey("OperatorsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DbMigration.Shift", null)
                        .WithMany()
                        .HasForeignKey("ShiftsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DbMigration.ParkingSpace", b =>
                {
                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("DbMigration.Shift", b =>
                {
                    b.Navigation("Vehicles");
                });

            modelBuilder.Entity("DbMigration.Vehicle", b =>
                {
                    b.Navigation("ParkingSpace");

                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
