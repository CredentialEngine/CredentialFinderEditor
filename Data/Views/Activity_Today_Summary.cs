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
    
    public partial class Activity_Today_Summary
    {
        public int Id { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string ActivityType { get; set; }
        public string Activity { get; set; }
        public string Event { get; set; }
        public string Comment { get; set; }
        public Nullable<int> TargetUserId { get; set; }
        public Nullable<int> ActionByUserId { get; set; }
        public string ActionByUser { get; set; }
        public Nullable<int> ActivityObjectId { get; set; }
        public Nullable<int> ObjectRelatedId { get; set; }
        public Nullable<int> TargetObjectId { get; set; }
        public string SessionId { get; set; }
        public string IPAddress { get; set; }
        public string Referrer { get; set; }
        public Nullable<bool> IsBot { get; set; }
    }
}
