using Lets_Connect.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lets_Connect.Data
{
    public class DataContext(DbContextOptions options) : IdentityDbContext<User, Roles, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, 
                                IdentityRoleClaim<int>, IdentityUserToken<int>>(options)
    {

        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasMany(ur => ur.UserRoles).WithOne(u => u.User).HasForeignKey(u => u.UserId).IsRequired();

            modelBuilder.Entity<Roles>().HasMany(ur => ur.UserRoles).WithOne(u => u.Role).HasForeignKey(u => u.RoleId).IsRequired();

            modelBuilder.Entity<UserLike>().HasKey(k => new {k.SourceUserId, k.TargetUserId});
            modelBuilder.Entity<UserLike>().HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(s  => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLike>().HasOne(t => t.TargetUser)
                .WithMany(l => l.LikedByUsers)
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>().HasOne(x => x.Recepient)
                .WithMany(x => x.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>().HasOne(x => x.Sender)
                .WithMany(x => x.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
