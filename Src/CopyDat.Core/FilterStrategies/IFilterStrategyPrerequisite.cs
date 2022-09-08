using System.Collections.Generic;

namespace CopyDat.Core.FilterStrategies
{
    public interface IFilterStrategyPrerequisite
    {
    }

    public interface IFilterStrategyPrerequisite<T> : IFilterStrategyPrerequisite
    {
        public IEnumerable<T> Result { get; }
        public void SetResult(IEnumerable<T> results);
    }
}
