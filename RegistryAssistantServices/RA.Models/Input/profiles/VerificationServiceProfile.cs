using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Models.Input
{
    /// <summary>
    /// Common input class for all verification profiles
    /// </summary>
    public class VerificationServiceProfile
    {
        public VerificationServiceProfile()
        {
            EstimatedCost = new List<CostProfile>();
            Jurisdiction = new List<Jurisdiction>();
            //Region = new List<GeoCoordinates>();
            OfferedBy = new List<Input.OrganizationReference>();
            TargetCredential = new List<EntityReference>();
        }        
        public string Description { get; set; }
		public string SubjectWebpage { get; set; }
		public string DateEffective { get; set; }
        public List<CostProfile> EstimatedCost { get; set; }
        public bool? HolderMustAuthorize { get; set; }
        public List<EntityReference> TargetCredential { get; set; }
        public string VerificationDirectory { get; set; }
        public string VerificationMethodDescription { get; set; }
        public string VerificationService { get; set; }
        public List<string> VerifiedClaimType { get; set; }
        public List<OrganizationReference> OfferedBy { get; set; }
        public List<Jurisdiction> Jurisdiction { get; set; }
       // public List<GeoCoordinates> Region { get; set; }

    }
}



