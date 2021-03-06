using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmallLister.Data
{
    // used by the migrations tool only
    public class SqliteDataContextFactory : IDesignTimeDbContextFactory<SqliteDataContext>
    {
        public SqliteDataContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqliteDataContext>();
            optionsBuilder.UseSqlite("Data Source=SmallLister.Web/smalllister.db");
            return new SqliteDataContext(optionsBuilder.Options);
        }
    }
}