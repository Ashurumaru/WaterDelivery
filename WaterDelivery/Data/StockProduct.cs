//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WaterDelivery.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class StockProduct
    {
        public int StockId { get; set; }
        public int WarehouseId { get; set; }
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual Warehouse Warehouse { get; set; }
    }
}
