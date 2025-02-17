﻿// <auto-generated />
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Data.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20250106104955_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("Data.FormData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Forms");
                });

            modelBuilder.Entity("Data.FormData", b =>
                {
                    b.OwnsMany("Data.FormDataProperty", "Properties", b1 =>
                        {
                            b1.Property<int>("FormDataId")
                                .HasColumnType("INTEGER");

                            b1.Property<int>("__synthesizedOrdinal")
                                .ValueGeneratedOnAddOrUpdate()
                                .HasColumnType("INTEGER");

                            b1.Property<int>("ComponentType")
                                .HasColumnType("INTEGER");

                            b1.Property<string>("LabelName")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("Name")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.Property<string>("Type")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("FormDataId", "__synthesizedOrdinal");

                            b1.ToTable("Forms");

                            b1.ToJson("Properties");

                            b1.WithOwner()
                                .HasForeignKey("FormDataId");
                        });

                    b.Navigation("Properties");
                });
#pragma warning restore 612, 618
        }
    }
}
