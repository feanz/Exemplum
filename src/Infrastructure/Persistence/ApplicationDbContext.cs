﻿namespace Exemplum.Infrastructure.Persistence
{
    using Application.Common.DomainEvents;
    using Application.Common.Identity;
    using Application.Persistence;
    using DateAndTime;
    using Domain.Audit;
    using Domain.Common;
    using Domain.Common.DateAndTime;
    using Domain.Todo;
    using DomainEvents;
    using ExceptionHandling;
    using Identity;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly IHandleDbExceptions _idbExceptions;
        private readonly IPublishDomainEvents _publishDomainEvents;
        private readonly IClock _clock;
        private readonly ICurrentUserService _currentUserService;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            IHandleDbExceptions idbExceptions,
            IPublishDomainEvents publishDomainEvents,
            IClock clock,
            ICurrentUserService currentUserService) : base(options)
        {
            _idbExceptions = idbExceptions;
            _publishDomainEvents = publishDomainEvents;
            _clock = clock;
            _currentUserService = currentUserService;
        }

        public DbSet<TodoItem> TodoItems => Set<TodoItem>();

        public DbSet<TodoList> TodoLists => Set<TodoList>();

        public DbSet<AuditItem> AuditItems => Set<AuditItem>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = _currentUserService.UserId ?? string.Empty;
                        entry.Entity.Created = _clock.Now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = _currentUserService.UserId ?? string.Empty;
                        entry.Entity.LastModified = _clock.Now;
                        break;
                    default:
                        // we dont need to handle the other entity state for auditability
                        break;
                }
            }

            await DispatchEvents();

            try
            {
                var result = await base.SaveChangesAsync(cancellationToken);
                return result;
            }
            catch (Exception e)
            {
                _idbExceptions.HandleException(e);
                throw;
            }
        }

        private async Task DispatchEvents()
        {
            var domainEvents = ChangeTracker
                .Entries<IHaveDomainEvents>()
                .Select(x => x.Entity.DomainEvents)
                .SelectMany(x => x)
                .ToList();

            foreach (var domainEvent in domainEvents)
            {
                await _publishDomainEvents.Publish(domainEvent);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            IgnoreDomainEvents(builder);

            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }

        /// <summary>
        /// Dont map domains events to storage
        /// </summary>
        private static void IgnoreDomainEvents(ModelBuilder builder)
        {
            var propertyNames = typeof(IHaveDomainEvents).GetProperties()
                .Select(p => p.Name)
                .ToList();

            var entityTypes = builder.Model.GetEntityTypes()
                .Where(t => typeof(IHaveDomainEvents)
                    .IsAssignableFrom(t.ClrType));

            foreach (var entityType in entityTypes)
            {
                var entityTypeBuilder = builder.Entity(entityType.ClrType);
                foreach (var propertyName in propertyNames)
                {
                    entityTypeBuilder.Ignore(propertyName);
                }
            }
        }
    }
}