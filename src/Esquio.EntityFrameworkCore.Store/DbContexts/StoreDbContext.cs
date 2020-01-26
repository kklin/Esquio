﻿using Esquio.EntityFrameworkCore.Store.Entities;
using Esquio.EntityFrameworkCore.Store.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Esquio.EntityFrameworkCore.Store
{
    public class StoreDbContext : DbContext
    {
        const string UNKNOWN = nameof(UNKNOWN);

        private readonly StoreOptions storeOptions;

        public StoreDbContext(DbContextOptions<StoreDbContext> options) : this(options, new StoreOptions()) { }

        public StoreDbContext(DbContextOptions<StoreDbContext> options, StoreOptions storeOptions) : base(options)
        {
            this.storeOptions = storeOptions;
        }

        public DbSet<ProductEntity> Products { get; set; }
        public DbSet<FeatureEntity> Features { get; set; }
        public DbSet<ToggleEntity> Toggles { get; set; }
        public DbSet<TagEntity> Tags { get; set; }
        public DbSet<ParameterEntity> Parameters { get; set; }
        public DbSet<FeatureTagEntity> FeatureTagEntities { get; set; }
        public DbSet<ApiKeyEntity> ApiKeys { get; set; }
        public DbSet<HistoryEntity> History { get; set; }
        public DbSet<PermissionEntity> Permissions { get; set; }
        public DbSet<RingEntity> Rings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StoreDbContext).Assembly, storeOptions);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            //get history entries and save current changes
            var historyEntities = GetHistoryEntityBeforeSave();
            var changes = base.SaveChanges(acceptAllChangesOnSuccess);

            //set new values for history entries and save it
            AddHistoryEntityAfterSave(historyEntities);
            base.SaveChanges(acceptAllChangesOnSuccess);

            return changes;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            //get history entries and save current changes
            var historyEntities = GetHistoryEntityBeforeSave();
            var changes = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            //set new values for history entries and save it
            AddHistoryEntityAfterSave(historyEntities);
            await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            return changes;
        }

        private IEnumerable<HistoryEntity> GetHistoryEntityBeforeSave()
        {
            ChangeTracker.DetectChanges();

            var historyEntities = new List<HistoryEntity>();

            foreach (var entry in ChangeTracker.Entries<IAuditable>())
            {
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                {
                    continue;
                }
                else
                {
                    var historyEntity = new HistoryEntity();

                    historyEntity.Action = entry.State switch
                    {
                        EntityState.Deleted => $"{nameof(EntityState.Deleted)} {entry.Entity.GetType().Name}",
                        EntityState.Added => $"{nameof(EntityState.Added)} {entry.Entity.GetType().Name}",
                        EntityState.Modified => $"{nameof(EntityState.Modified)} {entry.Entity.GetType().Name}",
                        _ => UNKNOWN,
                    };

                    historyEntity.CreatedAt = DateTime.UtcNow;
                    historyEntity.FeatureName = GetFeatureNameFromEntry(entry);
                    historyEntity.ProductName = GetProductNameFromEntry(entry);
                    historyEntity.OldValues = GetOldValues(entry);
                    historyEntity.NewValues = GetNewValues(entry);

                    historyEntities.Add(historyEntity);
                }
            }

            return historyEntities;
        }

        private void AddHistoryEntityAfterSave(IEnumerable<HistoryEntity> historyEntities)
        {
            if ( historyEntities != null
                &&
                historyEntities.Any())
            {
                History.AddRange(historyEntities);
            }
        }

        private string GetFeatureNameFromEntry(EntityEntry entry)
        {
            if (entry.Entity is FeatureEntity feature)
            {
                return feature.Name;
            }
            else if (entry.Entity is ToggleEntity toggle)
            {
                return toggle.FeatureEntity?.Name ?? UNKNOWN;
            }
            else if (entry.Entity is ParameterEntity parameter)
            {
                return parameter.ToggleEntity?.FeatureEntity?.Name ?? UNKNOWN;
            }

            return default;
        }

        private string GetProductNameFromEntry(EntityEntry entry)
        {
            if (entry.Entity is FeatureEntity feature)
            {
                return feature.ProductEntity?.Name ?? UNKNOWN;
            }
            else if (entry.Entity is ToggleEntity toggle)
            {
                return toggle.FeatureEntity?.ProductEntity?.Name ?? UNKNOWN;
            }
            else if (entry.Entity is ParameterEntity parameter)
            {
                return parameter.ToggleEntity?.FeatureEntity?.ProductEntity?.Name ?? UNKNOWN;
            }


            return default;
        }

        private string GetOldValues(EntityEntry entry)
        {
            var oldValues = new Dictionary<string, object>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsForeignKey()
                    ||
                    property.IsTemporary
                    ||
                    property.Metadata.IsPrimaryKey())
                {
                    continue;
                }

                var propertyName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Deleted:
                        oldValues[propertyName] = property.OriginalValue;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            oldValues[propertyName] = property.OriginalValue;
                        }
                        break;
                    default:
                        break;
                }
            }

            return oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : "{}";
        }

        private string GetNewValues(EntityEntry entry)
        {
            var newValues = new Dictionary<string, object>();

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsForeignKey() 
                    ||
                    property.IsTemporary
                    ||
                    property.Metadata.IsPrimaryKey())
                {
                    continue;
                }

                var propertyName = property.Metadata.Name;
                switch (entry.State)
                {
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                    case EntityState.Added:
                        newValues[propertyName] = property.CurrentValue;
                        break;
                    default:
                        break;
                }
            }

            return newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : "{}";
        }
    }
}
