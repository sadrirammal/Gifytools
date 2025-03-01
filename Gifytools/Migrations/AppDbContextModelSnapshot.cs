﻿// <auto-generated />
using System;
using Gifytools.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Gifytools.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Gifytools.Database.Entities.ConversionRequestEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("CompressionLevel")
                        .HasColumnType("integer");

                    b.Property<int>("ConversionStatus")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("CropHeight")
                        .HasColumnType("integer");

                    b.Property<int>("CropWidth")
                        .HasColumnType("integer");

                    b.Property<int>("CropX")
                        .HasColumnType("integer");

                    b.Property<int>("CropY")
                        .HasColumnType("integer");

                    b.Property<TimeSpan?>("EndTime")
                        .HasColumnType("interval");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("text");

                    b.Property<DateTime?>("ExecutedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Fps")
                        .HasColumnType("integer");

                    b.Property<int>("FrameSkipInterval")
                        .HasColumnType("integer");

                    b.Property<string>("GifOutputPath")
                        .HasColumnType("text");

                    b.Property<bool>("SetCompression")
                        .HasColumnType("boolean");

                    b.Property<bool>("SetCrop")
                        .HasColumnType("boolean");

                    b.Property<bool>("SetEndTime")
                        .HasColumnType("boolean");

                    b.Property<bool>("SetFps")
                        .HasColumnType("boolean");

                    b.Property<bool>("SetReduceFrames")
                        .HasColumnType("boolean");

                    b.Property<bool>("SetReverse")
                        .HasColumnType("boolean");

                    b.Property<bool>("SetSpeed")
                        .HasColumnType("boolean");

                    b.Property<bool>("SetStartTime")
                        .HasColumnType("boolean");

                    b.Property<bool>("SetWatermark")
                        .HasColumnType("boolean");

                    b.Property<double>("SpeedMultiplier")
                        .HasColumnType("double precision");

                    b.Property<TimeSpan?>("StartTime")
                        .HasColumnType("interval");

                    b.Property<string>("VideoInputPath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("WatermarkFont")
                        .HasColumnType("text");

                    b.Property<string>("WatermarkText")
                        .HasColumnType("text");

                    b.Property<int>("Width")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("ConversionRequests", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
