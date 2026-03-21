using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Domain.Entities.Users;
using Domain.Interfaces.Common;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IDomainEventDispatcher _dispatcher;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDomainEventDispatcher dispatcher) : base(options)
        {
            _dispatcher = dispatcher;
        }

        public DbSet<User> Users { get; set; }

        public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            ApplyAudit();

            var result = await base.SaveChangesAsync(ct);

            await DispatchDomainEventsAsync(ct);

            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            // Configurações adicionais de mapeamento podem ser feitas aqui
        }

        private void ApplyAudit()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = now;

                if (entry.State is EntityState.Added or EntityState.Modified)
                    entry.Entity.UpdatedAt = now;
            }
        }

        private async Task DispatchDomainEventsAsync(CancellationToken ct)
        {
            var entities = ChangeTracker
                .Entries<BaseAuditableEntity>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            var events = entities
                .SelectMany(e => e.DomainEvents)
                .ToList();

            // Limpa antes de publicar para evitar loop infinito
            entities.ForEach(e => e.ClearDomainEvents());

            await _dispatcher.DispatchAsync(events, ct);
        }
    }
}