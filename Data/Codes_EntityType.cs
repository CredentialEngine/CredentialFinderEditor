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
    
    public partial class Codes_EntityType
    {
        public Codes_EntityType()
        {
            this.Entity = new HashSet<Entity>();
            this.Import_IdentifierToObjectXref = new HashSet<Import_IdentifierToObjectXref>();
        }
    
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public string SchemaUrl { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> Totals { get; set; }
    
        public virtual ICollection<Entity> Entity { get; set; }
        public virtual ICollection<Import_IdentifierToObjectXref> Import_IdentifierToObjectXref { get; set; }
    }
}
