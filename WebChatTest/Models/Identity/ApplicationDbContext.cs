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
        }

        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
