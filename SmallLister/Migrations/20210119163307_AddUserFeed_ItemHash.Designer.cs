﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmallLister.Data;

namespace SmallLister.Migrations
{
    [DbContext(typeof(SqliteDataContext))]
    [Migration("20210119163307_AddUserFeed_ItemHash")]
    partial class AddUserFeed_ItemHash
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.2");

            modelBuilder.Entity("SmallLister.Model.ApiClient", b =>
                {
                    b.Property<int>("ApiClientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AppKey")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AppSecretHash")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("AppSecretSalt")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("CreatedById")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("RedirectUri")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ApiClientId");

                    b.HasIndex("CreatedById");

                    b.ToTable("ApiClients");
                });

            modelBuilder.Entity("SmallLister.Model.UserAccount", b =>
                {
                    b.Property<int>("UserAccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AuthenticationUri")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int?>("LastSelectedUserListId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.HasKey("UserAccountId");

                    b.ToTable("UserAccounts");
                });

            modelBuilder.Entity("SmallLister.Model.UserAccountApiAccess", b =>
                {
                    b.Property<int>("UserAccountApiAccessId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ApiClientId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("RevokedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserAccountId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserAccountApiAccessId");

                    b.HasIndex("ApiClientId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("UserAccountApiAccesses");
                });

            modelBuilder.Entity("SmallLister.Model.UserAccountToken", b =>
                {
                    b.Property<int>("UserAccountTokenId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ExpiryDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("TokenData")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("UserAccountApiAccessId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserAccountTokenId");

                    b.HasIndex("UserAccountApiAccessId");

                    b.ToTable("UserAccountTokens");
                });

            modelBuilder.Entity("SmallLister.Model.UserFeed", b =>
                {
                    b.Property<int>("UserFeedId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("FeedType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ItemDisplay")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ItemHash")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserFeedIdentifier")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("UserFeedId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("UserFeeds");
                });

            modelBuilder.Entity("SmallLister.Model.UserItem", b =>
                {
                    b.Property<int>("UserItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("CompletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("NextDueDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Repeat")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SortOrder")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("UserListId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserItemId");

                    b.HasIndex("UserAccountId");

                    b.HasIndex("UserListId");

                    b.ToTable("UserItems");
                });

            modelBuilder.Entity("SmallLister.Model.UserList", b =>
                {
                    b.Property<int>("UserListId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ItemSortOrder")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("SortOrder")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserAccountId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserListId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("UserLists");
                });

            modelBuilder.Entity("SmallLister.Model.ApiClient", b =>
                {
                    b.HasOne("SmallLister.Model.UserAccount", "CreatedBy")
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CreatedBy");
                });

            modelBuilder.Entity("SmallLister.Model.UserAccountApiAccess", b =>
                {
                    b.HasOne("SmallLister.Model.ApiClient", "ApiClient")
                        .WithMany()
                        .HasForeignKey("ApiClientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SmallLister.Model.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ApiClient");

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("SmallLister.Model.UserAccountToken", b =>
                {
                    b.HasOne("SmallLister.Model.UserAccountApiAccess", "UserAccountApiAccess")
                        .WithMany()
                        .HasForeignKey("UserAccountApiAccessId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserAccountApiAccess");
                });

            modelBuilder.Entity("SmallLister.Model.UserFeed", b =>
                {
                    b.HasOne("SmallLister.Model.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserAccount");
                });

            modelBuilder.Entity("SmallLister.Model.UserItem", b =>
                {
                    b.HasOne("SmallLister.Model.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SmallLister.Model.UserList", "UserList")
                        .WithMany()
                        .HasForeignKey("UserListId");

                    b.Navigation("UserAccount");

                    b.Navigation("UserList");
                });

            modelBuilder.Entity("SmallLister.Model.UserList", b =>
                {
                    b.HasOne("SmallLister.Model.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserAccount");
                });
#pragma warning restore 612, 618
        }
    }
}