using EnvironmentMonitor.Domain.Entities;
using EnvironmentMonitor.Domain.Interfaces;
using EnvironmentMonitor.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnvironmentMonitor.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationUserRole, string, ApplicationUserClaim, IdentityUserRole<string>, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        private readonly ICurrentUser? _currentUser;
        private readonly IDateService _dateService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUser? currentUser, IDateService dateService)
            : base(options)
        {
            _currentUser = currentUser;
            _dateService = dateService;
        }

        public DbSet<ApiSecret> ApiSecrets { get; set; }
        public DbSet<SecretClaim> SecretClaims { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("application");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly, type =>
                type.Namespace == "EnvironmentMonitor.Infrastructure.Identity.Configurations"
            );
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            StampEntities();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            StampEntities();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void StampEntities()
        {
            var updatedById = _currentUser?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var now = _dateService.CurrentTime();
            var utcNow = _dateService.LocalToUtc(now);

            foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.Updated = now;
                    entry.Entity.UpdatedUtc = utcNow;
                    entry.Entity.UpdatedById = updatedById;
                }
            }
            foreach (var entry in ChangeTracker.Entries<ApplicationUserClaim>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Created = now;
                        entry.Entity.CreatedUtc = utcNow;
                        entry.Entity.UpdatedById = updatedById;
                        break;
                    case EntityState.Modified:
                        entry.Entity.Updated = now;
                        entry.Entity.UpdatedUtc = utcNow;
                        entry.Entity.UpdatedById = updatedById;
                        break;
                }
            }
        }
    }
}
