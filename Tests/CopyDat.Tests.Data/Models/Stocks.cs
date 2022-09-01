using System;
using System.Collections.Generic;

namespace CopyDat.Data.Models
{
    public partial class Stocks: IContainProduct
    {
        public int StoreId { get; set; }
        public int ProductId { get; set; }
        public int? Quantity { get; set; }

        public virtual Products Product { get; set; }
        public virtual Stores Store { get; set; }
    }
}
