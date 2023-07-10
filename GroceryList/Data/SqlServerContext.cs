using GroceryList.Model;
using Microsoft.EntityFrameworkCore;

namespace GroceryList.Data
{
    public class SqlServerContext : DbContext
    {
        public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options) { }
        public DbSet<CategoryModel> Categories { get; set; }
    }
}
