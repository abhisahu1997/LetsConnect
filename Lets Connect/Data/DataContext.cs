using Lets_Connect.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace Lets_Connect.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> contextOptions) : base(contextOptions)
        {
            
        }

        public DbSet<User> Users { get; set; }
    }
}
