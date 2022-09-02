using CopyDat.Core.Builders;
using CopyDat.Core.FilterStrategies;
using CopyDat.Tests.Data.Models.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CopyDat.Tests.Core.Integration
{
    internal class TenantFilter: IFilterStrategyPrerequisite<Tenant>, IFilterStrategy<Tenant>
    {
        private readonly int _filterOnTenantId;
        private Tenant _tenant;

        public TenantFilter(int filterOnTenantId)
        {
            _filterOnTenantId = filterOnTenantId;
        }

        public Tenant Result => _tenant;

        public Func<Type, bool> Activator => t => t == typeof(Tenant);

        public Expression<Func<TDbEntity, bool>> Predicate<TDbEntity>() where TDbEntity : Tenant
        {
            return (Expression<Func<TDbEntity, bool>>)ExpressionBuilder.CreateEquals<TDbEntity, int>(t => t.Id, _filterOnTenantId);
        }

        public void SetResult(IEnumerable<Tenant> results)
        {
            _tenant = results.First();
        }
    }

    internal class SubscriptionEntityFilter : IFilterStrategy<TenantEntity, TenantFilter>, IFilterStrategyPrerequisite<Subscription>
    {
        private TenantFilter _tenantFilter;

        public Func<Type, bool> Activator => t => t.BaseType == typeof(TenantEntity);

        public Subscription Result { get; set; }

        public Expression<Func<TDbEntity, bool>> Predicate<TDbEntity>() where TDbEntity : TenantEntity
        {
            return (Expression<Func<TDbEntity, bool>>)ExpressionBuilder.CreateEquals<TDbEntity, Guid>(
                tde => tde.TenantIdentifier, _tenantFilter.Result.Identifier);
        }

        public IFilterStrategy<TenantEntity, TenantFilter> SetPrerequisite(TenantFilter prerequisite)
        {
            _tenantFilter = prerequisite;
            return this;
        }

        public void SetResult(IEnumerable<Subscription> results)
        {
            Result = results.First();
        }
    }


    internal class ResourceGroupEntityFilter : IFilterStrategy<SubscriptionEntity, SubscriptionEntityFilter>
    {
        private SubscriptionEntityFilter _subscriptionFilter;

        public ResourceGroupEntityFilter()
        {
        }

        public Func<Type, bool> Activator => t => t.BaseType == typeof(SubscriptionEntity);

        public Expression<Func<TDbEntity, bool>> Predicate<TDbEntity>() where TDbEntity : SubscriptionEntity
        {
            return (Expression<Func<TDbEntity, bool>>)ExpressionBuilder.CreateEquals<TDbEntity, Guid>
                (tde => tde.SubscriptionIdentifier, _subscriptionFilter.Result.Identifier);

        }

        public IFilterStrategy<SubscriptionEntity, SubscriptionEntityFilter> SetPrerequisite(SubscriptionEntityFilter prerequisite)
        {
            _subscriptionFilter = prerequisite;
            return this;
        }
    }
}