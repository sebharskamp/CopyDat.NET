using CopyDat.Tests.Data.Models.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CopyDat.Tests.Data.Models.Tenant
{
    public partial class TenantContext : DbContext
    {
        private readonly Action<ModelBuilder> _modelSeeder = null;

        public TenantContext()
        {
        }

        public TenantContext(DbContextOptions<TenantContext> options)
            : base(options)
        {
        }

        public TenantContext(DbContextOptions<TenantContext> options, Action<ModelBuilder> modelSeeder)
            : base(options)
        {
            _modelSeeder = modelSeeder;
        }

        public virtual DbSet<Tenant> Tenants { get; set; }
        public virtual DbSet<ResourceGroup> ResourceGroups { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.\\SQLExpress;Database=Tenant;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Identifier)
                    .HasName("PK__Tenant__5E5A8E221837AEF2");

                entity.ToTable("tenant");

                entity.Property(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Identifier)
                    .IsRequired()
                    .HasColumnName("identifier")
                    .HasMaxLength(Guid.NewGuid().ToString().Length)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ResourceGroup>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK__ResourceGroup__5E5A8E273837AEF2");

                entity.ToTable("ResourceGroup");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(e => e.Subscription)
                  .WithMany(p => p.ResourceGroups)
                  .HasForeignKey(d => d.SubscriptionIdentifier)
                  .HasConstraintName("FK__subscription__resourcegroup__4E88ABD4");
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Identifier)
                    .HasName("PK__Subscription__5E5A8E273837AEF2");

                entity.ToTable("Subscription");

                entity.Property(e => e.Owner)
                    .IsRequired()
                    .HasColumnName("owner")
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Identifier)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(Guid.NewGuid().ToString().Length)
                    .IsUnicode(false);

                entity.HasTenant(e => e.Subscriptions);
            });

            _modelSeeder?.Invoke(modelBuilder);

        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}


public static class EntityTypeBuilderExtensiosn
{
    public static EntityTypeBuilder<T> HasTenant<T>(this EntityTypeBuilder<T> entity, Expression<Func<Tenant, IEnumerable<T>>> withMany) where T : TenantEntity
    {
        string typeName = typeof(T).Name;
        entity.HasOne(d => d.Tenant)
                .WithMany(withMany)
                .HasForeignKey(d => d.TenantIdentifier)
                .HasConstraintName($"FK__{typeName.ToLower()}__{nameof(Tenant).ToLower()}_id__{Convert.ToBase64String(Encoding.Unicode.GetBytes(typeName)).Take(6)}");

        return entity;
    }
}

