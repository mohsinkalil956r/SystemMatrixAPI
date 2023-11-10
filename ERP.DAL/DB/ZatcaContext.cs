using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using ZATCA.DAL.DB.Entities;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace ZATCA.DAL.DB
{
    public class ZatcaContext : IdentityDbContext<SystemUser, Role, int, IdentityUserClaim<int>, UserRole,
        IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public ZatcaContext(DbContextOptions<ZatcaContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            

            //base.OnModelCreating(builder);

            builder.Entity<SystemUser>().ToTable("Users")
                .HasMany(e => e.Roles)
                .WithMany(e => e.Users)
                .UsingEntity<UserRole>();

            builder.Entity<Role>().ToTable("Roles");

            builder.Entity<UserRole>()
                .ToTable("UserRoles")
                .HasKey(x => new { x.UserId, x.RoleId });

            builder.Entity<IdentityUserClaim<int>>(entity => { entity.ToTable("UserClaims"); });

            builder.Entity<IdentityUserLogin<int>>(entity => 
            { 
                entity.ToTable("UserLogins");
                entity.HasKey(x => new { x.LoginProvider, x.ProviderKey });
            });

            builder.Entity<IdentityUserToken<int>>(entity => 
            { 
                entity.ToTable("UserTokens");
                entity.HasKey(x => new { x.UserId, x.LoginProvider, x.Name });
            });

            builder.Entity<IdentityRoleClaim<int>>(entity => { entity.ToTable("RoleClaims"); });

            builder.Entity<Permission>()
                .HasMany(e => e.Users)
                .WithMany(e => e.Permissions)
                .UsingEntity<UsZATCAermission>();





            SeedData.Seed(builder);
        }

        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UsZATCAermission> UsZATCAermissions { get; set; }

        public DbSet<Company> Companies { get; set; }    
        public DbSet<CSR> CSRs { get; set; }    

        public DbSet<Supplier> Suppliers { get; set; }

        public DbSet<LineItem> LineItems { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<InvoiceData> InvoiceData { get; set; }
        public DbSet<InvoiceRequest> InvoiceRequests{ get; set; }
        public DbSet<ZatcaResponse> ZatcaResponses { get; set; }
        public DbSet<InfoMessage> InfoMessages { get; set; }
        public DbSet<WarnngMessage> WarnngMessages { get; set; }
        public DbSet<ErrorMessage> ErrorMessages { get; set; }
        public DbSet<ValidationResult>  ValidationResults{ get; set; }
       

   
        public DbSet<TaxSubTotal> TaxSubTotal { get; set; }
        public DbSet<OnBoardingCSID> onBoardingCSIDs { get; set; }

    }
}
