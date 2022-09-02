using Microsoft.EntityFrameworkCore;
using System;

namespace CopyDat.Tests.Data.Models.Tenant
{
    public class TenantEntity
    {
        public Guid TenantIdentifier { get; set; }
        public virtual Tenant Tenant { get; set; }

    }
}
