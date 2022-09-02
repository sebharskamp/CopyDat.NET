using System;
using System.Collections.Generic;

namespace CopyDat.Tests.Data.Models.Tenant
{
    public class Subscription : TenantEntity
    {

        public Subscription()
        {
            ResourceGroups = new HashSet<ResourceGroup>();
        }

        public int Id { get; set; }
        public string Owner { get; set; }
        public Guid Identifier { get; set; }

        public virtual ICollection<ResourceGroup> ResourceGroups { get; set; }
    }
}
