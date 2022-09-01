namespace CopyDat.Core.FilterStrategies
{
    public interface IQueryStrategy<T, in TPrerequisite> : IQueryStrategy<T> where TPrerequisite : IQueryStrategyPrerequisite
    {
        public IQueryStrategy<T, TPrerequisite> SetPrerequisite(TPrerequisite prerequisite);
    }
}
