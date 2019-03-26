using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Models.Input
{
    /// <summary>
    /// Common input class for all process profiles 
    /// </summary>
    public class ProcessProfile
    {
        public ProcessProfile()
        {
            ExternalInputType = new List<string>();
            Jurisdiction = new List<Jurisdiction>();

            TargetCredential = new List<EntityReference>();
            TargetAssessment = new List<EntityReference>();
            TargetLearningOpportunity = new List<EntityReference>();
            TargetCompetencyFramework = new List<EntityReference>();
            //Region = new List<GeoCoordinates>();
        }
        public string Description { get; set; }
        public string SubjectWebpage { get; set; }
        public string DateEffective { get; set; }
        //public List<CredentialAlignmentObject> ExternalInputType { get; set; }
        public List<string> ExternalInputType { get; set; }
        public string ProcessFrequency { get; set; }
        public List<OrganizationReference> ProcessingAgent { get; set; } = new List<OrganizationReference>();
        public string ProcessMethod { get; set; }
        public string ProcessMethodDescription { get; set; }
        public string ProcessStandards { get; set; }
        public string ProcessStandardsDescription { get; set; }
        public string ScoringMethodDescription { get; set; }
        public string ScoringMethodExample { get; set; }
        public string ScoringMethodExampleDescription { get; set; }
        //public List<string> ScoringMethodType { get; set; }
        public List<EntityReference> TargetCredential { get; set; }
        public List<EntityReference> TargetAssessment { get; set; }
        public List<EntityReference> TargetLearningOpportunity { get; set; }
        public List<EntityReference> TargetCompetencyFramework { get; set; }
        public string VerificationMethodDescription { get; set; }
        public List<Jurisdiction> Jurisdiction { get; set; }
        //public List<GeoCoordinates> Region { get; set; }

    }
}



