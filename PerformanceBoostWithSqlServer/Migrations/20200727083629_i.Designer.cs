﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PerformanceBoostWithSqlServer.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20200727083629_i")]
    partial class i
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0-preview.7.20365.15");

            modelBuilder.Entity("Course", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("Level")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.ToTable("Courses");

                    b
                        .HasAnnotation("SqlServer:MemoryOptimized", true);
                });

            modelBuilder.Entity("CourseEnrollment", b =>
                {
                    b.Property<int>("CourseId")
                        .HasColumnType("int");

                    b.Property<int>("StudentId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateEnrolled")
                        .HasColumnType("datetime2");

                    b.HasKey("CourseId", "StudentId")
                        .IsClustered(false);

                    b.HasIndex("StudentId")
                        .IsClustered(false);

                    b.ToTable("CourseEnrollments");

                    b
                        .HasAnnotation("SqlServer:MemoryOptimized", true);
                });

            modelBuilder.Entity("Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .UseIdentityColumn();

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.ToTable("Persons");

                    b.HasDiscriminator<int>("Role");

                    b
                        .HasAnnotation("SqlServer:MemoryOptimized", true);
                });

            modelBuilder.Entity("Student", b =>
                {
                    b.HasBaseType("Person");

                    b.Property<int>("SubscriptionLevel")
                        .HasColumnType("int");

                    b.HasDiscriminator().HasValue(1);

                    b
                        .HasAnnotation("SqlServer:MemoryOptimized", true);
                });

            modelBuilder.Entity("Teacher", b =>
                {
                    b.HasBaseType("Person");

                    b.HasDiscriminator().HasValue(2);

                    b
                        .HasAnnotation("SqlServer:MemoryOptimized", true);
                });

            modelBuilder.Entity("CourseEnrollment", b =>
                {
                    b.HasOne("Course", "Course")
                        .WithMany()
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Student", "Student")
                        .WithMany()
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
