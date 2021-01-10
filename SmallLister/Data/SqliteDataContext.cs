using Microsoft.EntityFrameworkCore;
using SmallLister.Model;

namespace SmallLister.Data
{
    public class SqliteDataContext : DbContext
    {
        public SqliteDataContext(DbContextOptions<SqliteDataContext> options) : base(options) { }

        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<UserList> UserLists { get; set; }
        public DbSet<UserItem> UserItems { get; set; }
        public DbSet<ApiClient> ApiClients { get; set; }
        public DbSet<UserAccountToken> UserAccountTokens { get; set; }
        public DbSet<UserAccountApiAccess> UserAccountApiAccesses { get; set; }
    }
}