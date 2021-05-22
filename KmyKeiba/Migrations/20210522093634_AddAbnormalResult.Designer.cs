﻿// <auto-generated />
using System;
using KmyKeiba.Models.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KmyKeiba.Migrations
{
    [DbContext(typeof(MyContext))]
    [Migration("20210522093634_AddAbnormalResult")]
    partial class AddAbnormalResult
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.6");

            modelBuilder.Entity("KmyKeiba.Models.Data.RaceData", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    b.Property<short>("Course")
                        .HasColumnType("smallint");

                    b.Property<int>("CourseRaceNumber")
                        .HasColumnType("int");

                    b.Property<short>("DataStatus")
                        .HasColumnType("smallint");

                    b.Property<int>("Distance")
                        .HasColumnType("int");

                    b.Property<int>("Grade")
                        .HasColumnType("int");

                    b.Property<int>("HorsesCount")
                        .HasColumnType("int");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name6Chars")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("SubName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("SubjectAge2")
                        .HasColumnType("int");

                    b.Property<int>("SubjectAge3")
                        .HasColumnType("int");

                    b.Property<int>("SubjectAge4")
                        .HasColumnType("int");

                    b.Property<int>("SubjectAge5")
                        .HasColumnType("int");

                    b.Property<int>("SubjectAgeYounger")
                        .HasColumnType("int");

                    b.Property<string>("SubjectName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<short>("TrackCornerDirection")
                        .HasColumnType("smallint");

                    b.Property<short>("TrackGround")
                        .HasColumnType("smallint");

                    b.Property<short>("TrackOption")
                        .HasColumnType("smallint");

                    b.Property<short>("TrackType")
                        .HasColumnType("smallint");

                    b.HasKey("Id");

                    b.ToTable("Races");
                });

            modelBuilder.Entity("KmyKeiba.Models.Data.RaceHorseData", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    b.Property<short>("AbnormalResult")
                        .HasColumnType("smallint");

                    b.Property<TimeSpan>("AfterThirdHalongTime")
                        .HasColumnType("time(6)");

                    b.Property<int>("AfterThirdHalongTimeOrder")
                        .HasColumnType("int");

                    b.Property<short>("DataStatus")
                        .HasColumnType("smallint");

                    b.Property<int>("FirstCornerOrder")
                        .HasColumnType("int");

                    b.Property<int>("FourthCornerOrder")
                        .HasColumnType("int");

                    b.Property<int>("FrameNumber")
                        .HasColumnType("int");

                    b.Property<bool>("IsBlinkers")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Number")
                        .HasColumnType("int");

                    b.Property<float>("Odds")
                        .HasColumnType("float");

                    b.Property<int>("Popular")
                        .HasColumnType("int");

                    b.Property<string>("RaceKey")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("ResultOrder")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("ResultTime")
                        .HasColumnType("time(6)");

                    b.Property<string>("RiderCode")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("RiderName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<float>("RiderWeight")
                        .HasColumnType("float");

                    b.Property<short>("RunningStyle")
                        .HasColumnType("smallint");

                    b.Property<int>("SecondCornerOrder")
                        .HasColumnType("int");

                    b.Property<int>("ThirdCornerOrder")
                        .HasColumnType("int");

                    b.Property<short>("Weight")
                        .HasColumnType("smallint");

                    b.Property<short>("WeightDiff")
                        .HasColumnType("smallint");

                    b.HasKey("Id");

                    b.ToTable("RaceHorses");
                });

            modelBuilder.Entity("KmyKeiba.Models.Data.SystemData", b =>
                {
                    b.Property<uint>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int unsigned");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("SystemData");
                });
#pragma warning restore 612, 618
        }
    }
}
