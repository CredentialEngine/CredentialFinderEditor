//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class Entity_Approval
    {
        public int Id { get; set; }
        public int EntityId { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime Created { get; set; }
        public int CreatedById { get; set; }
    
        public virtual Account Account { get; set; }
        public virtual Entity Entity { get; set; }
    }
}
