using CopyDat.Core.Handlers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public static class DbContextExtensions
{
    public static async Task PopulateAsync<T>(this T context, Dictionary<Type, object?> seed) where T : DbContext
    {
        var dbHandler = new DatabaseHandler<T>(context);
        foreach(var (entityType, values) in seed)
        {
            await dbHandler.SetValuesAsync(entityType, values);
        }
        await dbHandler.SaveChangesAsync();
    }
}
