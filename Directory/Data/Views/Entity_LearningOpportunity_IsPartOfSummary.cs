//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Data.Views
{
    using System;
    using System.Collections.Generic;
    
    public partial class Entity_LearningOpportunity_IsPartOfSummary
    {
        public int Id { get; set; }
        public int EntityId { get; set; }
        public int LearningOpportunityId { get; set; }
        public System.Guid EntityUid { get; set; }
        public int EntityTypeId { get; set; }
        public string Title { get; set; }
        public int parentLoppId { get; set; }
        public string ParentName { get; set; }
        public string ParentDescription { get; set; }
        public string ParentUrl { get; set; }
        public string LearningOpportunity { get; set; }
    }
}
