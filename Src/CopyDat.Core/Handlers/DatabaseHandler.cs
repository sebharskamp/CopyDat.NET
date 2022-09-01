using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<TSet>> GetValues<TSet>(Expression<Func<TSet, bool>> predicate = null) where TSet : class
        {
            var entities = await (predicate != null ? _context.Set<TSet>().Where(predicate) : _context.Set<TSet>()).ToListAsync().ConfigureAwait(false);
            return entities;
        }

        public async Task SetValues<TSet>(IList<TSet> items) where TSet : class
        {
            using var transaction = _context.Database.BeginTransaction();
            await _context.BulkInsertOrUpdateAsync(items, new BulkConfig { SetOutputIdentity = true, SqlBulkCopyOptions = SqlBulkCopyOptions.KeepIdentity, UseTempDB = true, TrackingEntities = true }).ConfigureAwait(false);
            transaction.Commit();

        }
    }
}
