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
        private readonly TDbContext _sourceContext;
        private readonly TDbContext _targetContext;
        protected CopyDat(TDbContext sourceContext, TDbContext targetContext)
        {
            _sourceContext = sourceContext;
            _targetContext = targetContext;
        }

        public static async ValueTask<CopyDat<TDbContext>> InitializeAsync(TDbContext source, TDbContext target)
        {
            return new CopyDat<TDbContext>(source, target);
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
            var dataBaseEntities = Assembly.GetAssembly(typeof(TDbContext)).DefinedTypes;

            var entities = typeof(TDbContext).GetProperties().Where(p => p.PropertyType.Name.Contains("DbSet")).Select(pt => pt.PropertyType).ToList();

            var orderedEntities = DbContextAnalyzer.GetEntityHierarchy(ref entities);

            var sourceHandler = new DatabaseHandler<TDbContext>(_sourceContext);
            var targetHandler = new DatabaseHandler<TDbContext>(_targetContext);

            foreach (var entity in orderedEntities)
            {
                var getValues = sourceHandler.GetType().GetMethod(nameof(sourceHandler.GetValues));
                getValues = getValues.MakeGenericMethod(entity);
                var results = await getValues.InvokeAsync(sourceHandler, new object[] { null });


                var setValues = targetHandler.GetType().GetMethod(nameof(targetHandler.SetValues));
                setValues = setValues.MakeGenericMethod(entity);
                await setValues.InvokeAsync(targetHandler, results);
            }
        }

        public async Task CopyAsync(List<IQueryStrategy> query = null)
        {
            var entities = typeof(TDbContext).GetProperties().Where(p => p.PropertyType.Name.Contains("DbSet")).Select(pt => pt.PropertyType).ToList();

            var orderedEntities = DbContextAnalyzer.GetEntityHierarchy(ref entities);

            var sourceHandler = new DatabaseHandler<TDbContext>(_sourceContext);
            var targetHandler = new DatabaseHandler<TDbContext>(_targetContext);

            foreach (var entity in orderedEntities)
            {
                var q = query.FirstOrDefault(q => q.Activator(entity));
                var getValues = sourceHandler.GetType().GetMethod(nameof(sourceHandler.GetValues));
                getValues = getValues.MakeGenericMethod(entity);
                object? predicate = null;
                if (q != null)
                {
                    var type = q.GetType();
                    if (type.GetInterfaces().Any(i => i.GetGenericArguments().Any(a => a.GetInterfaces().Any(ai => ai == typeof(IQueryStrategyPrerequisite)))))
                    {
                        var setPrereq = type.GetMethod("SetPrerequisite");
                        setPrereq.Invoke(q,new []{query.First(qq => qq.GetType().GetInterface(nameof(IQueryStrategyPrerequisite)) != null)});
                    }
                    var predicates = type.GetMethod(nameof(IQueryStrategy<Type>.Predicate));
                    predicates = predicates.MakeGenericMethod(new[] { entity });
                    predicate = predicates?.Invoke(q, null);
                }
                var results = await getValues.InvokeAsync(sourceHandler, new object[] { predicate });
                if (q.GetType().GetInterfaces().Any(i =>
                        i == typeof(IQueryStrategyPrerequisite)))
                {
                    var setResuly = q.GetType().GetMethod(nameof(IQueryStrategyPrerequisite<TDbContext>.SetResult));
                    setResuly.Invoke(q, new[] { results });
                }

                var setValues = targetHandler.GetType().GetMethod(nameof(targetHandler.SetValues));
                setValues = setValues.MakeGenericMethod(entity);
                await setValues.InvokeAsync(targetHandler, results);
            }
        }

        public void Dispose()
        {
            _sourceContext.Dispose();
            _targetContext.Dispose();
        }
    }
}
