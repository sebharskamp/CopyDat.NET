using Bogus;
using Microsoft.EntityFrameworkCore;
using System;
using CopyDat.Tests.Data.Models;
using CopyDat.Tests.Data.Models.Tenant;
using System.Collections.Generic;

namespace CopyDat.Tests.Core.Integration.Fixtures.BikeStore
{
    internal partial class Seeder
    {
        internal static Dictionary<Type, object?> GetTenant()
        {
            var tenantIds = 1;
            var tenants = new Faker<Data.Models.Tenant.Tenant>()
                .StrictMode(false)
                .UseSeed(5354)
                .RuleFor(m => m.Id, f => tenantIds++)
                .RuleFor(m => m.Name, f => f.Company.CompanyName())
                .RuleFor(m => m.Identifier, f => f.Random.Guid())
                .Generate(3);

            var subscriptionIds = 1;
            var subscriptions = new Faker<Subscription>()
                .StrictMode(false)
                .UseSeed(5354)
                .RuleFor(m => m.Id, f => subscriptionIds++)
                .RuleFor(m => m.Owner, f => f.Person.FullName)
                .RuleFor(m => m.Identifier, f => f.Random.Guid())
                .RuleFor(m => m.TenantIdentifier, f => f.PickRandom(tenants).Identifier)
                .Generate(19);

            var resourcegroupIds = 1;
            var resourcegroups = new Faker<ResourceGroup>()
                .StrictMode(false)
                .UseSeed(5354)
                .RuleFor(m => m.Id, f => resourcegroupIds++)
                .RuleFor(m => m.Name, f => f.Person.FirstName)
                .RuleFor(m => m.SubscriptionIdentifier, f => f.PickRandom(subscriptions).Identifier)
                .Generate(800);


            return new Dictionary<Type, object?>
            {
                {typeof(Data.Models.Tenant.Tenant), tenants},
                {typeof(Subscription), subscriptions},
                {typeof(ResourceGroup), resourcegroups}
            };
        }
    }
}
