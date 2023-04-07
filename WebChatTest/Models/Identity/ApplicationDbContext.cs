using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebChatTest.Models.Identity
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ChatRoom>()
                .HasIndex(c => c.Name)
                .IsUnique();

            builder.Entity<ChatRoom>()
                .HasOne(r => r.Admin)
                .WithMany(a => a.AdminsRooms)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ChatRoom>()
                .HasMany(r => r.Users)
                .WithMany(u => u.ChatRooms);
        }

        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
