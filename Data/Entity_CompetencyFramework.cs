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
    
    public partial class Entity_CompetencyFramework
    {
        public Entity_CompetencyFramework()
        {
            this.Entity_CompetencyFrameworkItem = new HashSet<Entity_CompetencyFrameworkItem>();
        }
    
        public int Id { get; set; }
        public int EntityId { get; set; }
        public string EducationalFrameworkName { get; set; }
        public string EducationalFrameworkUrl { get; set; }
        public Nullable<int> AlignmentTypeId { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public System.Guid RowId { get; set; }
        public string AlignmentType { get; set; }
    
        public virtual Entity Entity { get; set; }
        public virtual ICollection<Entity_CompetencyFrameworkItem> Entity_CompetencyFrameworkItem { get; set; }
    }
}