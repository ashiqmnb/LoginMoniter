using LoginMonitering.Models;
using Microsoft.EntityFrameworkCore;

namespace LoginMonitering.DbContexts
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


        public DbSet<User> Users { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }
        public DbSet<RiskEvaluation> RiskEvaluations { get; set; }
        public DbSet<IpBlacklist> IpBlacklists { get; set; }
        public DbSet<EmailOtp> EmailOtps { get; set; }
        public DbSet<GeoLocation> GeoLocations { get; set; }
        public DbSet<RiskSettings> RiskSettingses { get; set; }
        public DbSet<RiskLevel> RiskLevels { get; set; }
        public DbSet<LoginStatus> LoginStatuses { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<EmailOtp>()
                .HasOne(e => e.User)
                .WithMany(u => u.EmailOtps)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<LoginAttempt>()
                .HasOne(l => l.User)
                .WithMany(u => u.LoginAttempts)
                .HasForeignKey(l => l.UserId);

            modelBuilder.Entity<LoginAttempt>()
                .HasOne(l => l.RiskEvaluation)
                .WithMany(r => r.LoginAttempts)
                .HasForeignKey(l => l.RiskEvaluationId);

            modelBuilder.Entity<LoginAttempt>()
                .HasOne(la => la.GeoLocation)
                .WithOne(gl => gl.LoginAttempt)
                .HasForeignKey<LoginAttempt>(la => la.GeoLocationId);

        }

    }
}
