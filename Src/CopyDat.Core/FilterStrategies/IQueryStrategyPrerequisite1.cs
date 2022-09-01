using System.Collections.Generic;

namespace CopyDat.Core.FilterStrategies
{
    public interface IQueryStrategyPrerequisite<T> : IQueryStrategyPrerequisite
    {
        public T Result { get; }
        public void SetResult(IEnumerable<T> results);
    }
}
