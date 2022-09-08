using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using CopyDat.Core.FilterStrategies;
using System.Linq;
using System.Collections.Generic;

namespace CopyDat.Tests.Core.Integration
{
    public static class AssertionSets
    {
        public static async Task EntitiesShouldBeEqual<Tcontext, T>(this Tcontext expectedContext, Tcontext contextToAssert, System.Func<FluentAssertions.Equivalency.EquivalencyAssertionOptions<T>, FluentAssertions.Equivalency.EquivalencyAssertionOptions<T>>? fluentOptions = null)
            where Tcontext : DbContext
            where T : class
        {
            var expectedEntities = await expectedContext.Set<T>().ToListAsync();
            var entitiesToAssert = await contextToAssert.Set<T>().ToListAsync();
            Assert.NotEmpty(expectedEntities);
            entitiesToAssert.Should().BeEquivalentTo(expectedEntities);
        }

        public static async Task EntitiesShouldBeEqual<Tcontext, T>(this Tcontext expectedContext, Tcontext contextToAssert, IFilterStrategy<T> filterStrategy, System.Func<FluentAssertions.Equivalency.EquivalencyAssertionOptions<T>, FluentAssertions.Equivalency.EquivalencyAssertionOptions<T>>? fluentOptions = null)
            where Tcontext : DbContext
            where T : class
        {
            System.Linq.Expressions.Expression<System.Func<T, bool>> predicate = filterStrategy.Predicate<T>();
            var expectedEntities = await expectedContext.Set<T>().Where(predicate).ToListAsync();
            var entitiesToAssert = await contextToAssert.Set<T>().ToListAsync();
            Assert.NotEmpty(expectedEntities);
            fluentOptions ??= options =>
                {
                    options.WithStrictOrdering();
                    return options;
                };
            entitiesToAssert.Should().BeEquivalentTo(expectedEntities, fluentOptions);
            var setResultMethod = filterStrategy.GetType().GetMethod(nameof(IFilterStrategyPrerequisite<T>.SetResult));
            if(setResultMethod != null)
            {
                setResultMethod.Invoke(filterStrategy, new[] { expectedEntities });
            }
        }
    }
}
