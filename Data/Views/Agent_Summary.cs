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
    
    public partial class Agent_Summary
    {
        public System.Guid AgentRowId { get; set; }
        public int AgentTypeId { get; set; }
        public string AgentType { get; set; }
        public int AgentRelativeId { get; set; }
        public string AgentName { get; set; }
        public string Summary { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string SortOrder { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public string ImageURL { get; set; }
        public string CTID { get; set; }
        public Nullable<bool> ISQAOrganization { get; set; }
        public string AgentReferenceName { get; set; }
        public Nullable<bool> IsThirdPartyOrganization { get; set; }
    }
}
