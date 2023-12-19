using Microsoft.EntityFrameworkCore;
using SmallLister.Model;

namespace SmallLister.Data;

public class SqliteDataContext(DbContextOptions<SqliteDataContext> options) : DbContext(options), ISqliteDataContext
{
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<UserAccountCredential> UserAccountCredentials { get; set; }
    public DbSet<UserList> UserLists { get; set; }
    public DbSet<UserItem> UserItems { get; set; }
    public DbSet<ApiClient> ApiClients { get; set; }
    public DbSet<UserAccountToken> UserAccountTokens { get; set; }
    public DbSet<UserAccountApiAccess> UserAccountApiAccesses { get; set; }
    public DbSet<UserFeed> UserFeeds { get; set; }
    public DbSet<UserAction> UserActions { get; set; }
    public DbSet<UserWebhook> UserWebhooks { get; set; }
    public DbSet<UserListWebhookQueue> UserListWebhookQueue { get; set; }
    public DbSet<UserItemWebhookQueue> UserItemWebhookQueue { get; set; }
    public void Migrate() => Database.Migrate();
}