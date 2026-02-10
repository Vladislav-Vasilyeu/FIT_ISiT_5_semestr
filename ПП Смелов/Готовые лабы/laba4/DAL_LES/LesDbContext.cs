using Microsoft.EntityFrameworkCore;

namespace DAL_LES
{
    public class LesDbContext : DbContext
    {
        public DbSet<Celebrity> Celebrities { get; set; }
        public DbSet<LifeEvent> LifeEvents { get; set; }

        public LesDbContext(DbContextOptions<LesDbContext> options) : base(options){}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Celebrity>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Celebrity>()
                .Property(c => c.Nationality)
                .HasDefaultValue("XX");

            modelBuilder.Entity<LifeEvent>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<LifeEvent>()
                .HasOne(e => e.Celebrity)
                .WithMany(c => c.LifeEvents)
                .HasForeignKey(e => e.CelebrityId);
        }
    }

    

    
}