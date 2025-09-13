using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartEducation.Entities;
using System.Text.Json;

namespace SmartEducation.dbContext
{
    public class SmartEduDbContext : IdentityDbContext<User>
    {
        public DbSet<ActivityRecommendation> ActivityRecommendations { get; set; }
        public DbSet<NGSS_Detailed_Standard> NGSS_Detailed_Standard { get; set; }
        public DbSet<OtpVerification> OtpVerifications { get; set; }
        public DbSet<Grade_Standards> NGSS_Standard { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Kid> Kids { get; set; }

        public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
        {
            UserManager<User> userManager =
                serviceProvider.GetRequiredService<UserManager<User>>();
            RoleManager<IdentityRole> roleManager = serviceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = { "Admin", "OrganizationAdmin", "User" };

            // Create roles if they don't exist
            foreach (var roleName in roleNames)
            {
                if (await roleManager.FindByNameAsync(roleName) == null)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            string adminEmail = "admin@gmail.com";
            string adminPassword = "Sesame123#";

            if (await userManager.FindByNameAsync(adminEmail) == null)
            {
                User user = new User { UserName = adminEmail, Email = adminEmail };
                var result = await userManager.CreateAsync(user, adminPassword);

                if (result.Succeeded)
                {
                    // Assign the "Admin" role
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }

        public SmartEduDbContext(DbContextOptions<SmartEduDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the one-to-many relationship between User and Kid
            modelBuilder.Entity<User>()
                .HasMany(u => u.Kids)
                .WithOne(k => k.User)
                .HasForeignKey(k => k.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.List_ActivityRecommendations)
                .WithOne(k => k.User)
                .HasForeignKey(k => k.UserId);

            // Configure the one-to-many relationship between Organization and User
            modelBuilder.Entity<Organization>()
                .HasMany(o => o.Users)
                .WithOne(u => u.Organization)
                .HasForeignKey(u => u.OrganizationId)
                // Fforeign key optional
                .IsRequired(false);

            modelBuilder.Entity<ActivityRecommendation>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<NGSS_Detailed_Standard>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Grade_Standards>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<OtpVerification>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();
        }
    }
}
