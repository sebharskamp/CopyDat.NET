using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CopyDat.Core.Extensions;
using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

[assembly: InternalsVisibleTo("CopyDat.Tests.Core.Integration")]
namespace CopyDat.Core.Handlers
{
    public class DatabaseHandler<TDbContext> where TDbContext : DbContext
    {
        private readonly TDbContext _context;
        private static readonly HashSet<Type> _databaseEntityTypes = 
            typeof(TDbContext).GetProperties().Where(p => p.PropertyType.Name.Contains("DbSet")).Select(pt => pt.PropertyType.GenericTypeArguments.First()).ToHashSet();
        private BulkConfig _bulkConfig;

        public DatabaseHandler(TDbContext context)
        {
            _context = context;
        }

        private static Expression<Func<TSet, TSet>> NoRelations<TSet>(HashSet<Type> propertiesToExclude)
        {
            var entity = Expression.Parameter(typeof(TSet), "entity");
            var parameters = new List<ParameterExpression> { entity };
            var instance = Expression.New(typeof(TSet));

            var propertiesToAssign = typeof(TSet).GetProperties().Where(property => !propertiesToExclude.Contains(property.PropertyType) && !property.PropertyType.GenericTypeArguments.Any(gt => propertiesToExclude.Contains(gt))).ToArray();
            var propertyAssignments = new MemberAssignment[propertiesToAssign.Length];

            for (var i = 0; i < propertiesToAssign.Length; i++)
            {
                propertyAssignments[i] = Expression.Bind(propertiesToAssign[i], Expression.Property(entity, propertiesToAssign[i]));
            }

            Expression body = Expression.MemberInit(instance, propertyAssignments);
            return Expression.Lambda<Func<TSet, TSet>>(body, parameters);
        }

        internal async Task<IEnumerable<TSet>> GetGenericValuesAsync<TSet>(Expression<Func<TSet, bool>>? predicate = null) where TSet : class
        {
            var entities = await (predicate != null ? _context.Set<TSet>().Where(predicate) : _context.Set<TSet>()).Select(NoRelations<TSet>(_databaseEntityTypes)).ToListAsync().ConfigureAwait(false);
            return entities;
        }

        internal async Task SetGenericValuesAsync<TSet>(IList<TSet> items) where TSet : class
        {
            await _context.Set<TSet>().AddRangeAsync(items).ConfigureAwait(false);
        }

        internal async Task<object?> GetValuesAsync(Type entityType, object? predicate)
        {
            var getValues = this.GetType().GetMethod(nameof(this.GetGenericValuesAsync), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new MissingMethodException(nameof(this.GetGenericValuesAsync));
            getValues = getValues.MakeGenericMethod(entityType);
            return await getValues.InvokeAsync(this, new object?[] { predicate });
        }

        internal async Task SetValuesAsync(Type entityType, object? results)
        {
            var setValues = this.GetType().GetMethod(nameof(this.SetGenericValuesAsync), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new MissingMethodException(nameof(this.SetGenericValuesAsync));
            setValues = setValues.MakeGenericMethod(entityType);
            await setValues.InvokeAsync(this, results);
        }

        private static Dictionary<EntityState, KeyValuePair<string, int>> EntityStateBulkMethodDict => new Dictionary<EntityState, KeyValuePair<string, int>>()
        {
            { EntityState.Deleted, new KeyValuePair<string, int>(nameof(DbContextBulkExtensions.BulkDelete), 1) },
            { EntityState.Modified, new KeyValuePair<string, int>(nameof(DbContextBulkExtensions.BulkUpdate), 2) },
            { EntityState.Added, new KeyValuePair<string, int>(nameof(DbContextBulkExtensions.BulkInsert), 3)},
        };


        internal async Task SaveChangesAsync(bool trackAddedEntities = false, CancellationToken cancellationToken = default)
        {
            var bulkConfig = new BulkConfig { SetOutputIdentity = true, SqlBulkCopyOptions = SqlBulkCopyOptions.KeepIdentity};

            var entries = _context.ChangeTracker.Entries();
            var entriesGroupedByEntity = entries.GroupBy(a => new { EntityType = a.Entity.GetType(), a.State },
                                                         (entry, group) => new { entry.EntityType, EntityState = entry.State, Entities = group.Select(a => a.Entity).ToList() });
            var entriesGroupedChanged = entriesGroupedByEntity.Where(a => EntityStateBulkMethodDict.ContainsKey(a.EntityState) & a.Entities.Count >= 0);
            var entriesGroupedChangedSorted = entriesGroupedChanged.OrderBy(a => a.EntityState.ToString() != EntityState.Modified.ToString()).ToList();
            if (entriesGroupedChangedSorted.Count == 0)
                return;

            await _context.Database.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

            var connection = _context.GetUnderlyingConnection(bulkConfig);

            bool doExplicitCommit = false;
            if (_context.Database.CurrentTransaction == null)
            {
                doExplicitCommit = true;
            }

            try
            {
                var transaction = _context.Database.CurrentTransaction ?? _context.Database.BeginTransaction();

                Dictionary<string, Dictionary<string, FastProperty>> fastPropertyDicts = new Dictionary<string, Dictionary<string, FastProperty>>();
                var addedEntryTypes = new List<Type>();
                foreach (var entryGroup in entriesGroupedChangedSorted)
                {
                    Type entityType = entryGroup.EntityType;
                    entityType = (entityType.Namespace == "Castle.Proxies") ? entityType.BaseType! : entityType;
                    if (addedEntryTypes.Contains(entityType))
                        continue;

                    var entityModelType = _context.Model.FindEntityType(entityType) ?? throw new ArgumentNullException($"Unable to determine EntityType from given type with name {entityType.Name}");

                    var entityPropertyDict = new Dictionary<string, FastProperty>();
                    if (!fastPropertyDicts.ContainsKey(entityType.Name))
                    {
                        var properties = entityModelType.GetProperties();
                        var navigationPropertiesInfo = entityModelType.GetNavigations().Select(x => x.PropertyInfo);

                        foreach (var property in properties)
                        {
                            if (property.PropertyInfo != null) // skip Shadow Property
                            {
                                entityPropertyDict.Add(property.Name, FastProperty.GetOrCreate(property.PropertyInfo));
                            }
                        }
                        foreach (var navigationPropertyInfo in navigationPropertiesInfo)
                        {
                            if (navigationPropertyInfo != null)
                            {
                                entityPropertyDict.Add(navigationPropertyInfo.Name, FastProperty.GetOrCreate(navigationPropertyInfo));
                            }
                        }
                        fastPropertyDicts.Add(entityType.Name, entityPropertyDict);
                    }
                    else
                    {
                        entityPropertyDict = fastPropertyDicts[entityType.Name];
                    }

                    var navigations = entityModelType.GetNavigations().Where(x => !(x.IsCollection()));
                    if (navigations.Any())
                    {
                        foreach (var navigation in navigations)
                        {
                           
                            if (fastPropertyDicts.ContainsKey(navigation.ClrType.Name))
                            {
                                var parentPropertyDict = fastPropertyDicts[navigation.ClrType.Name];

                                var fkName = navigation.ForeignKey.Properties.Count > 0
                                    ? navigation.ForeignKey.Properties[0].Name
                                    : null;

                                var pkName = navigation.ForeignKey.PrincipalKey.Properties.Count > 0
                                    ? navigation.ForeignKey.PrincipalKey.Properties[0].Name
                                    : null;

                                if (pkName != null && fkName != null)
                                {
                                    foreach (var entity in entryGroup.Entities)
                                    {
                                        var parentEntity = entityPropertyDict[navigation.Name].Get(entity);
                                        if (parentEntity != null)
                                        {
                                            var pkValue = parentPropertyDict[pkName].Get(parentEntity);
                                            if (pkValue != null)
                                            {
                                                entityPropertyDict[fkName].Set(entity, pkValue);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    string methodName = EntityStateBulkMethodDict[entryGroup.EntityState].Key;
                    await _context.BulkInsertAsync(entryGroup.Entities, bulkConfig, null, entityType, cancellationToken).ConfigureAwait(false);

                }

                if (doExplicitCommit)
                {
                    transaction.Commit();
                    _context.ChangeTracker.AcceptAllChanges();
                }
            }
            finally
            {
                if (doExplicitCommit)
                {
                    await _context.Database.CloseConnectionAsync().ConfigureAwait(false);
                }
            }
        }

        private void RetrieveIncludedDbEntities(Type entityType, List<Type> includedEntities)
        {
            if (includedEntities.Contains(entityType)) return;
            includedEntities.Add(entityType);

            foreach (var property in entityType.GetProperties())
            {
                if(_databaseEntityTypes.Contains(property.PropertyType) || property.PropertyType.GenericTypeArguments.Any(gt => _databaseEntityTypes.Contains(gt)))
                {
                    var type = _databaseEntityTypes.Contains(property.PropertyType) ? property.PropertyType : property.PropertyType.GenericTypeArguments.First();
                    RetrieveIncludedDbEntities(type, includedEntities);
                }
            }
            return;
        }
    }
}
