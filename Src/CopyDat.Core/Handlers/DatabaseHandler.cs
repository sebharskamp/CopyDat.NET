using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CopyDat.Core.Extensions;
using EFCore.BulkExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CopyDat.Core.Handlers
{
    public class DatabaseHandler<TDbContext> where TDbContext : DbContext
    {
        private readonly TDbContext _context;

        public DatabaseHandler(TDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TSet>> GetValuesAsync<TSet>(Expression<Func<TSet, bool>>? predicate = null) where TSet : class
        {
            var entities = await (predicate != null ? _context.Set<TSet>().Where(predicate) : _context.Set<TSet>()).ToListAsync().ConfigureAwait(false);
            return entities;
        }

        public async Task SetValuesAsync<TSet>(IList<TSet> items) where TSet : class
        {
            using var transaction = _context.Database.BeginTransaction();
            await _context.BulkInsertOrUpdateAsync(items, new BulkConfig { SetOutputIdentity = true, SqlBulkCopyOptions = SqlBulkCopyOptions.KeepIdentity, UseTempDB = true, TrackingEntities = true }).ConfigureAwait(false);
            transaction.Commit();

        }

        internal async Task<object?> GetValuesAsync(Type entityType, object? predicate)
        {
            var getValues = this.GetType().GetMethod(nameof(this.GetValuesAsync)) ?? throw new MissingMethodException(nameof(this.GetValuesAsync));
            getValues = getValues.MakeGenericMethod(entityType);
            return await getValues.InvokeAsync(this, new object?[] { predicate });
        }

        internal async Task SetValuesAsync(Type entityType, object? results)
        {
            var setValues = this.GetType().GetMethod(nameof(this.SetValuesAsync)) ?? throw new MissingMethodException(nameof(this.SetValuesAsync));
            setValues = setValues.MakeGenericMethod(entityType);
            await setValues.InvokeAsync(this, results);
        }
    }
}
