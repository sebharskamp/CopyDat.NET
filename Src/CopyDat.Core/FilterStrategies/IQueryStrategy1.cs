using System;
using System.Linq.Expressions;

namespace CopyDat.Core.FilterStrategies
{
    public interface IQueryStrategy<in T> : IQueryStrategy
    {
        public Expression<Func<TDbEntity, bool>> Predicate<TDbEntity>() where TDbEntity : T;

    }
}
