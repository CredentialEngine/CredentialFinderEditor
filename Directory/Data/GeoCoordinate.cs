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
    
    public partial class GeoCoordinate
    {
        public System.Guid ParentId { get; set; }
        public int Id { get; set; }
        public Nullable<int> GeoNamesId { get; set; }
        public string Name { get; set; }
        public string AddressRegion { get; set; }
        public string Country { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Url { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public Nullable<int> LastUpdatedById { get; set; }
        public Nullable<int> JurisdictionId { get; set; }
        public Nullable<bool> IsException { get; set; }
    }
}
