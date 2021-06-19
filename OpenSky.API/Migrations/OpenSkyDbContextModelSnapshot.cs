﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OpenSky.API;

namespace OpenSky.API.Migrations
{
    [DbContext(typeof(OpenSkyDbContext))]
    partial class OpenSkyDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.7");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("RoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("UserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("RoleId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("UserTokens");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.Aircraft", b =>
                {
                    b.Property<string>("Registry")
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.Property<string>("AirportICAO")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("varchar(5)");

                    b.Property<string>("OwnerID")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<Guid>("TypeID")
                        .HasColumnType("char(36)");

                    b.HasKey("Registry");

                    b.HasIndex("AirportICAO");

                    b.HasIndex("OwnerID");

                    b.HasIndex("TypeID");

                    b.ToTable("Aircraft");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.AircraftType", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("AtcModel")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("AtcType")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("Category")
                        .HasColumnType("int");

                    b.Property<string>("Comments")
                        .HasColumnType("longtext");

                    b.Property<bool>("DetailedChecksDisabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<double>("EmptyWeight")
                        .HasColumnType("double");

                    b.Property<bool>("Enabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("EngineCount")
                        .HasColumnType("int");

                    b.Property<int>("EngineType")
                        .HasColumnType("int");

                    b.Property<bool>("FlapsAvailable")
                        .HasColumnType("tinyint(1)");

                    b.Property<double>("FuelTotalCapacity")
                        .HasColumnType("double");

                    b.Property<bool>("IsGearRetractable")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsVanilla")
                        .HasColumnType("tinyint(1)");

                    b.Property<Guid?>("IsVariantOf")
                        .HasColumnType("char(36)");

                    b.Property<string>("LastEditedByID")
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<double>("MaxGrossWeight")
                        .HasColumnType("double");

                    b.Property<int>("MaxPrice")
                        .HasColumnType("int");

                    b.Property<int>("MinPrice")
                        .HasColumnType("int");

                    b.Property<int>("MinimumRunwayLength")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<bool>("NeedsCoPilot")
                        .HasColumnType("tinyint(1)");

                    b.Property<Guid?>("NextVersion")
                        .HasColumnType("char(36)");

                    b.Property<int>("Simulator")
                        .HasColumnType("int");

                    b.Property<string>("UploaderID")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.Property<int>("VersionNumber")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("IsVariantOf");

                    b.HasIndex("LastEditedByID");

                    b.HasIndex("NextVersion");

                    b.HasIndex("UploaderID");

                    b.ToTable("AircraftTypes");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.Airport", b =>
                {
                    b.Property<string>("ICAO")
                        .HasMaxLength(5)
                        .HasColumnType("varchar(5)");

                    b.Property<int>("Altitude")
                        .HasColumnType("int");

                    b.Property<int?>("AtisFrequency")
                        .HasColumnType("int");

                    b.Property<string>("City")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("Country")
                        .HasColumnType("int");

                    b.Property<int>("GaRamps")
                        .HasColumnType("int");

                    b.Property<int>("Gates")
                        .HasColumnType("int");

                    b.Property<bool>("HasAvGas")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("HasJetFuel")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsClosed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsMilitary")
                        .HasColumnType("tinyint(1)");

                    b.Property<double>("Latitude")
                        .HasColumnType("double");

                    b.Property<int>("LongestRunwayLength")
                        .HasColumnType("int");

                    b.Property<string>("LongestRunwaySurface")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("varchar(7)");

                    b.Property<double>("Longitude")
                        .HasColumnType("double");

                    b.Property<bool>("MSFS")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("RunwayCount")
                        .HasColumnType("int");

                    b.Property<int?>("Size")
                        .HasColumnType("int");

                    b.Property<bool>("SupportsSuper")
                        .HasColumnType("tinyint(1)");

                    b.Property<int?>("TowerFrequency")
                        .HasColumnType("int");

                    b.Property<int?>("UnicomFrequency")
                        .HasColumnType("int");

                    b.HasKey("ICAO");

                    b.ToTable("Airports");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.Approach", b =>
                {
                    b.Property<int>("ID")
                        .HasColumnType("int");

                    b.Property<string>("AirportICAO")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("varchar(5)");

                    b.Property<string>("RunwayName")
                        .HasMaxLength(6)
                        .HasColumnType("varchar(6)");

                    b.Property<string>("Suffix")
                        .HasMaxLength(1)
                        .HasColumnType("varchar(1)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("varchar(25)");

                    b.HasKey("ID");

                    b.HasIndex("AirportICAO");

                    b.ToTable("Approaches");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.DataImport", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime?>("Finished")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ImportDataSource")
                        .HasColumnType("longtext");

                    b.Property<string>("LogText")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Started")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("TotalRecordsProcessed")
                        .HasColumnType("int");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("ID");

                    b.ToTable("DataImports");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.OpenSkyToken", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("Expiry")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("UserID")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("varchar(255)");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("OpenSkyTokens");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.OpenSkyUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("LastLoginGeo")
                        .HasColumnType("longtext");

                    b.Property<string>("LastLoginIP")
                        .HasColumnType("longtext");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("RegisteredOn")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.Runway", b =>
                {
                    b.Property<int>("ID")
                        .HasColumnType("int");

                    b.Property<string>("AirportICAO")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("varchar(5)");

                    b.Property<int>("Altitude")
                        .HasColumnType("int");

                    b.Property<string>("CenterLight")
                        .HasMaxLength(1)
                        .HasColumnType("varchar(1)");

                    b.Property<string>("EdgeLight")
                        .HasMaxLength(1)
                        .HasColumnType("varchar(1)");

                    b.Property<int>("Length")
                        .HasColumnType("int");

                    b.Property<string>("Surface")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("varchar(7)");

                    b.Property<int>("Width")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("AirportICAO");

                    b.ToTable("Runways");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.RunwayEnd", b =>
                {
                    b.Property<int>("ID")
                        .HasColumnType("int");

                    b.Property<string>("ApproachLightSystem")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<bool>("HasClosedMarkings")
                        .HasColumnType("tinyint(1)");

                    b.Property<double>("Heading")
                        .HasColumnType("double");

                    b.Property<double>("Latitude")
                        .HasColumnType("double");

                    b.Property<double?>("LeftVasiPitch")
                        .HasColumnType("double");

                    b.Property<string>("LeftVasiType")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<double>("Longitude")
                        .HasColumnType("double");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("OffsetThreshold")
                        .HasColumnType("int");

                    b.Property<double?>("RightVasiPitch")
                        .HasColumnType("double");

                    b.Property<string>("RightVasiType")
                        .HasMaxLength(15)
                        .HasColumnType("varchar(15)");

                    b.Property<int>("RunwayID")
                        .HasColumnType("int");

                    b.HasKey("ID");

                    b.HasIndex("RunwayID");

                    b.ToTable("RunwayEnds");
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
                    b.HasOne("OpenSky.API.DbModel.OpenSkyUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("OpenSky.API.DbModel.OpenSkyUser", null)
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

                    b.HasOne("OpenSky.API.DbModel.OpenSkyUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("OpenSky.API.DbModel.OpenSkyUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("OpenSky.API.DbModel.Aircraft", b =>
                {
                    b.HasOne("OpenSky.API.DbModel.Airport", "Airport")
                        .WithMany()
                        .HasForeignKey("AirportICAO")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("OpenSky.API.DbModel.OpenSkyUser", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerID");

                    b.HasOne("OpenSky.API.DbModel.AircraftType", "Type")
                        .WithMany()
                        .HasForeignKey("TypeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Airport");

                    b.Navigation("Owner");

                    b.Navigation("Type");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.AircraftType", b =>
                {
                    b.HasOne("OpenSky.API.DbModel.AircraftType", "VariantType")
                        .WithMany()
                        .HasForeignKey("IsVariantOf");

                    b.HasOne("OpenSky.API.DbModel.OpenSkyUser", "LastEditedBy")
                        .WithMany()
                        .HasForeignKey("LastEditedByID");

                    b.HasOne("OpenSky.API.DbModel.AircraftType", "NextVersionType")
                        .WithMany()
                        .HasForeignKey("NextVersion");

                    b.HasOne("OpenSky.API.DbModel.OpenSkyUser", "Uploader")
                        .WithMany()
                        .HasForeignKey("UploaderID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("LastEditedBy");

                    b.Navigation("NextVersionType");

                    b.Navigation("Uploader");

                    b.Navigation("VariantType");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.Approach", b =>
                {
                    b.HasOne("OpenSky.API.DbModel.Airport", "Airport")
                        .WithMany("Approaches")
                        .HasForeignKey("AirportICAO")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Airport");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.OpenSkyToken", b =>
                {
                    b.HasOne("OpenSky.API.DbModel.OpenSkyUser", "User")
                        .WithMany("Tokens")
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.Runway", b =>
                {
                    b.HasOne("OpenSky.API.DbModel.Airport", "Airport")
                        .WithMany("Runways")
                        .HasForeignKey("AirportICAO")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Airport");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.RunwayEnd", b =>
                {
                    b.HasOne("OpenSky.API.DbModel.Runway", "Runway")
                        .WithMany("RunwayEnds")
                        .HasForeignKey("RunwayID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Runway");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.Airport", b =>
                {
                    b.Navigation("Approaches");

                    b.Navigation("Runways");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.OpenSkyUser", b =>
                {
                    b.Navigation("Tokens");
                });

            modelBuilder.Entity("OpenSky.API.DbModel.Runway", b =>
                {
                    b.Navigation("RunwayEnds");
                });
#pragma warning restore 612, 618
        }
    }
}
