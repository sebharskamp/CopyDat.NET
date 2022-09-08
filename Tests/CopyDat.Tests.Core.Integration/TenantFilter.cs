using CopyDat.Core.FilterStrategies;
using CopyDat.Tests.Data.Models.Tenant;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CopyDat.Tests.Core.Integration
{
    internal class TenantFilter: IFilterStrategyPrerequisite<Tenant>, IFilterStrategy<Tenant>
    {
        private readonly int _filterOnTenantId;

        public TenantFilter(int filterOnTenantId)
        {
            _filterOnTenantId = filterOnTenantId;
        }

        public IEnumerable<Tenant> Result { get; private set; }

        public Func<Type, bool> Activator => t => t == typeof(Tenant);

        public Expression<Func<TDbEntity, bool>> Predicate<TDbEntity>() where TDbEntity : Tenant
        {
            return t => t.Id == 1;
        }

        public void SetResult(IEnumerable<Tenant> results)
        {
            Result = results;
        }
    }

    internal class SubscriptionEntityFilter : FilterStrategyWithPrerequisiteBase<TenantEntity, TenantFilter>, IFilterStrategyPrerequisite<Subscription>
    {
        public SubscriptionEntityFilter(TenantFilter prerequisiteFilter, Func<Type, bool> activator) : base(prerequisiteFilter, activator)
        {
        }

        public IEnumerable<Subscription>? Result { get; set; }

        public override Expression<Func<TDbEntity, bool>> Predicate<TDbEntity>()
        {
            return s => s.TenantIdentifier == _prerequisiteFilter.Result.First().Identifier;
        }

        public void SetResult(IEnumerable<Subscription> results)
        {
            Result = results;
        }
    }


    internal class ResourceGroupEntityFilter : FilterStrategyWithPrerequisiteBase<SubscriptionEntity, SubscriptionEntityFilter>
    {
        public ResourceGroupEntityFilter(SubscriptionEntityFilter prerequisiteFilter, Func<Type, bool> activator) : base(prerequisiteFilter, activator)
        {
        }

        public override Expression<Func<TDbEntity, bool>> Predicate<TDbEntity>()
        {
            return PredicateBuilder.New<TDbEntity>(r => _prerequisiteFilter.Result.Select(res => res.Identifier).Any(i => i == r.SubscriptionIdentifier));
        }
    }
}