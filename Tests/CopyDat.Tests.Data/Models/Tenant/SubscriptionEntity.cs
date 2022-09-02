using System;

namespace CopyDat.Tests.Data.Models.Tenant
{
    public class SubscriptionEntity
    {
        public Guid SubscriptionIdentifier { get; set; }
        public virtual Subscription Subscription { get; set; }
    }
}
