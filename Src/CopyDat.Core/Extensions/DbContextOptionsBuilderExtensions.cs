using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;

namespace CopyDat.Core.Extensions
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static async Task<T> InitiateConnectionAsync<T>(this DbContextOptionsBuilder<T> optionsBuilder, bool ensureCreated) where T : DbContext
        {
            var connectionString = string.Empty;
            var sqlServerOptionsExtension =
                optionsBuilder.Options.FindExtension<SqlServerOptionsExtension>();
            if (sqlServerOptionsExtension != null)
            {
                connectionString = sqlServerOptionsExtension.ConnectionString;
            }
            var context = (T)(Activator.CreateInstance(typeof(T), new object[] { optionsBuilder.Options }) ?? throw new InvalidOperationException($"Unable to establish connection with {connectionString}"));
            if (ensureCreated)
            {
                await context.Database.EnsureCreatedAsync();
            }
            return context;
        }
    }

}
