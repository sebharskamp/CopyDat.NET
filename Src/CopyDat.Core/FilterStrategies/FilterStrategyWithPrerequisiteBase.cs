using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CopyDat.Core.FilterStrategies
{
    public abstract class FilterStrategyWithPrerequisiteBase<T, TPrerequisite> : IFilterStrategy<T, TPrerequisite> where TPrerequisite : IFilterStrategyPrerequisite
    {
        protected readonly TPrerequisite _prerequisiteFilter;
        private readonly Func<Type, bool> _activator;

        public FilterStrategyWithPrerequisiteBase(TPrerequisite prerequisiteFilter, Func<Type, bool> activator)
        {
            _prerequisiteFilter = prerequisiteFilter;
            _activator = activator;
        }

        public Func<Type, bool> Activator => _activator;

        public abstract Expression<Func<TDbEntity, bool>> Predicate<TDbEntity>() where TDbEntity : T;
    }
}
