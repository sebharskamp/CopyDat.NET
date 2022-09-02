using System;
using System.Collections.Generic;

namespace CopyDat.Tests.Data.Models.Tenant
{
    public class Tenant
    {
        public Tenant()
        {
            Subscriptions = new HashSet<Subscription>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public Guid Identifier { get; set; }


        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}