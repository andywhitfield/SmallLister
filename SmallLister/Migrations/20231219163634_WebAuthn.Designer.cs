﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmallLister.Data;

#nullable disable

namespace SmallLister.Migrations
{
    [DbContext(typeof(SqliteDataContext))]
    [Migration("20231219163634_WebAuthn")]
    partial class WebAuthn
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

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

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
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

            modelBuilder.Entity("SmallLister.Model.UserAccountCredential", b =>
                {
                    b.Property<int>("UserAccountCredentialId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("CredentialId")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("PublicKey")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<uint>("SignatureCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("UserHandle")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.HasKey("UserAccountCredentialId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("UserAccountCredentials");
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

            modelBuilder.Entity("SmallLister.Model.UserAction", b =>
                {
                    b.Property<int>("UserActionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ActionNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ActionType")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsCurrent")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserActionData")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("UserActionId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("UserActions");
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

                    b.Property<DateTime?>("PostponedUntilDate")
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

            modelBuilder.Entity("SmallLister.Model.UserItemWebhookQueue", b =>
                {
                    b.Property<int>("UserItemWebhookQueueId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("EventType")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("SentDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("SentPayload")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserItemId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserItemWebhookQueueId");

                    b.HasIndex("UserItemId");

                    b.ToTable("UserItemWebhookQueue");
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

            modelBuilder.Entity("SmallLister.Model.UserListWebhookQueue", b =>
                {
                    b.Property<int>("UserListWebhookQueueId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("EventType")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("SentDateTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("SentPayload")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserListId")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserListWebhookQueueId");

                    b.HasIndex("UserListId");

                    b.ToTable("UserListWebhookQueue");
                });

            modelBuilder.Entity("SmallLister.Model.UserWebhook", b =>
                {
                    b.Property<int>("UserWebhookId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DeletedDateTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("UserAccountId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Webhook")
                        .HasColumnType("TEXT");

                    b.Property<int>("WebhookType")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserWebhookId");

                    b.HasIndex("UserAccountId");

                    b.ToTable("UserWebhooks");
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

            modelBuilder.Entity("SmallLister.Model.UserAccountCredential", b =>
                {
                    b.HasOne("SmallLister.Model.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

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

            modelBuilder.Entity("SmallLister.Model.UserAction", b =>
                {
                    b.HasOne("SmallLister.Model.UserAccount", "UserAccount")
                        .WithMany()
                        .HasForeignKey("UserAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserAccount");
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

            modelBuilder.Entity("SmallLister.Model.UserItemWebhookQueue", b =>
                {
                    b.HasOne("SmallLister.Model.UserItem", "UserItem")
                        .WithMany()
                        .HasForeignKey("UserItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserItem");
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

            modelBuilder.Entity("SmallLister.Model.UserListWebhookQueue", b =>
                {
                    b.HasOne("SmallLister.Model.UserList", "UserList")
                        .WithMany()
                        .HasForeignKey("UserListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserList");
                });

            modelBuilder.Entity("SmallLister.Model.UserWebhook", b =>
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
