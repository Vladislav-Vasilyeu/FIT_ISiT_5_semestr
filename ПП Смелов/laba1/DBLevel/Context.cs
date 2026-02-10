using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


namespace DBLevel
{
    public class Context : DbContext
    {
        public Context() : base()
        {
            Database.EnsureCreated();
        }
        public DbSet<WSRef>? WSRefs {  get; set; }
        public DbSet<Comment>? Comments { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=VladPC;Database=MyDatabase;Trusted_Connection=True;TrustServerCertificate=True;");

        }
    }
}
