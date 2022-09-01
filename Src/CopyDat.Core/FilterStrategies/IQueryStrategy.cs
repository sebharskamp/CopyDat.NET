using System;

namespace CopyDat.Core.FilterStrategies
{
    public interface IQueryStrategy
    {
        public Func<Type, bool> Activator { get; }
    }
}
