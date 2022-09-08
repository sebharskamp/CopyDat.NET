using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using CopyDat.Core.Extensions;
using CopyDat.Core.FilterStrategies;
using CopyDat.Core.Handlers;
using Microsoft.EntityFrameworkCore;

namespace CopyDat.Core
{
    public class CopyDat<TDbContext> : IDisposable where TDbContext : DbContext
    {
        private const string _predicateMethodName = nameof(IFilterStrategy<object>.Predicate);
        private readonly TDbContext _sourceContext;
        private readonly TDbContext _targetContext;
        protected CopyDat(TDbContext sourceContext, TDbContext targetContext)
        {
            _sourceContext = sourceContext;
            _targetContext = targetContext;
        }

        public static ValueTask<CopyDat<TDbContext>> InitializeAsync(TDbContext source, TDbContext target)
        {
            return new ValueTask<CopyDat<TDbContext>>(new CopyDat<TDbContext>(source, target));
        }
        
        public static async ValueTask<CopyDat<TDbContext>> InitializeAsync(DbContextOptionsBuilder<TDbContext> source, TDbContext target)
        {
            return new CopyDat<TDbContext>(await source.InitiateConnectionAsync(true), target);
        }
        public static async ValueTask<CopyDat<TDbContext>> InitializeAsync(TDbContext source, DbContextOptionsBuilder<TDbContext> target)
        {
            return new CopyDat<TDbContext>(source, await target.InitiateConnectionAsync(true));
        }

        public static async ValueTask<CopyDat<TDbContext>> InitializeAsync(DbContextOptionsBuilder<TDbContext> source, DbContextOptionsBuilder<TDbContext> target)
        {
            return new CopyDat<TDbContext>(await source.InitiateConnectionAsync(true), await target.InitiateConnectionAsync(true));
        }

        public async Task CopyAsync()
        {
            var dataBaseEntities = Assembly.GetAssembly(typeof(TDbContext))?.DefinedTypes ?? throw new OperationCanceledException("No assembly of database found.");

            var entities = typeof(TDbContext).GetProperties().Where(p => p.PropertyType.Name.Contains("DbSet")).Select(pt => pt.PropertyType).ToList();

            var orderedEntities = DbContextAnalyzer.GetEntityHierarchy(ref entities);

            var sourceHandler = new DatabaseHandler<TDbContext>(_sourceContext);
            var targetHandler = new DatabaseHandler<TDbContext>(_targetContext);

            foreach (var entityType in orderedEntities)
            {
                var results = await sourceHandler.GetValuesAsync(entityType, null);

                await targetHandler.SetValuesAsync(entityType, results);
            }
            await targetHandler.SaveChangesAsync(true);
        }

        public async Task CopyAsync(List<IFilterStrategy> query)
        {
            var entities = typeof(TDbContext).GetProperties().Where(p => p.PropertyType.Name.Contains("DbSet")).Select(pt => pt.PropertyType).ToList();

            var orderedEntities = DbContextAnalyzer.GetEntityHierarchy(ref entities);

            var sourceHandler = new DatabaseHandler<TDbContext>(_sourceContext);
            var targetHandler = new DatabaseHandler<TDbContext>(_targetContext);

            foreach (var entityType in orderedEntities)
            {
                var (predicate, filter) = BuildFilterPredicate(query, entityType);

                var results = await sourceHandler.GetValuesAsync(entityType, predicate);

                if(filter != null)
                    AddResultToFilterPrerequisites(filter, results);

                await targetHandler.SetValuesAsync(entityType, results);
            }
            await targetHandler.SaveChangesAsync(true);
        }

        private static void AddResultToFilterPrerequisites(IFilterStrategy q, object? results)
        {
            if (q.GetType().GetInterfaces().Any(i =>
                    i == typeof(IFilterStrategyPrerequisite)))
            {
                var setResuly = q.GetType().GetMethod(nameof(IFilterStrategyPrerequisite<TDbContext>.SetResult)) 
                    ?? throw new MissingMethodException(nameof(IFilterStrategyPrerequisite<TDbContext>.SetResult));
                setResuly.Invoke(q, new[] { results });
            }
        }

        private static (object?, IFilterStrategy?) BuildFilterPredicate(List<IFilterStrategy>? filters, Type entityType)
        {
            var filter = filters.FirstOrDefault(q => q.Activator(entityType));

            object? predicate = null;
            if (filter != null)
            {
                var filterStrategy = filter.GetType();
/*                if (filterStrategy.GetInterfaces().Any(i => i.GetGenericArguments().Any(a => a.GetInterfaces().Any(ai => ai == typeof(IFilterStrategyPrerequisite)))))
                {
                    var setPrerequisite = filterStrategy.GetMethod(_setPrerequisiteMethodName) ?? throw new MissingMethodException(_setPrerequisiteMethodName);
                    setPrerequisite.Invoke(filter, new[] { filters.First(qq => qq.GetType().GetInterface(nameof(IFilterStrategyPrerequisite)) != null) });
                }*/
                var predicates = filterStrategy.GetMethod(_predicateMethodName) ?? throw new MissingMethodException(_predicateMethodName);
                predicates = predicates.MakeGenericMethod(new[] { entityType });
                predicate = predicates?.Invoke(filter, null);
            }

            return (predicate, filter);
        }

        public void Dispose()
        {
            _sourceContext.Dispose();
            _targetContext.Dispose();
        }
    }
}
