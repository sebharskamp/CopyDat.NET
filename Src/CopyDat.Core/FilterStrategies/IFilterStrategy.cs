using System;
using System.Linq.Expressions;

namespace CopyDat.Core.FilterStrategies
{
    public interface IFilterStrategy
    {
        public Func<Type, bool> Activator { get; }
    }

    public interface IFilterStrategy<in T> : IFilterStrategy
    {
        public Expression<Func<TDbEntity, bool>> Predicate<TDbEntity>() where TDbEntity : T;
    }
    public interface IFilterStrategy<T, in TPrerequisite> : IFilterStrategy<T> where TPrerequisite : IFilterStrategyPrerequisite
    {
    }
}
