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
    
    public partial class LoyaltyPointMovement
    {
        public int MovementId { get; set; }
        public int ClientId { get; set; }
        public int MovementTypeId { get; set; }
        public int PointsAmount { get; set; }
        public System.DateTime MovementDate { get; set; }
        public string Notes { get; set; }
    
        public virtual Client Client { get; set; }
        public virtual LoyaltyPointMovementType LoyaltyPointMovementType { get; set; }
    }
}
