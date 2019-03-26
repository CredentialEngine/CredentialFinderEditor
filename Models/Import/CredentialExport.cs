using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Import
{
    public class CredentialExport
    {
        public string CTID { get; set; }
        public string UniqueIdentifier { get; set; }
        public string OwnedBy { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string CredentialType { get; set; } 
        public string CredentialStatus { get; set; }
        public string SubjectWebpage { get; set; }


        // roles
        public string OfferedBy { get; set; } 
        public string AccreditedBy { get; set; }
        public string ApprovedBy { get; set; } 
        public string RecognizedBy { get; set; }

        public bool AvailableOnlineAt { get; set; }
        public bool AvailabilityListing { get; set; }
        public bool AlternateName { get; set; }
        public bool CodedNotation { get; set; }
       public string CredentialId { get; set; }
        public string ImageUrl { get; set; }
        public string Language { get; set; }
        public string DateEffective { get; set; }
        public string Keywords { get; set; }
        public string Subjects { get; set; }
        public string AudienceLevelType { get; set; }

        

        public string NaicsList { get; set; }
        //Industry Type
        public string Industries { get; set; }
        public string OnetList { get; set; }
        public string Occupations { get; set; }
        
        public string EstimatedDuration { get; set; }

        #region Costs 
        //public string EstimatedCost { get; set; }
        //costs 
        public string CostIdentifier { get; set; }

        public string CostDescription { get; set; } = "";
        public string CostDetailsUrl { get; set; } = "";
        public string CostCurrencyType { get; set; } = "";
        
        public string CostTypeList{ get; set; }
        #endregion

        #region Condition Profile
        public string ConditionIdentifier { get; set; }
        public string ConditionType { get; set; }
        public string Condition_Description { get; set; } = "";
        public string Condition_SubjectWebpage { get; set; } = "";
        public string Condition_SubmissionOfItems { get; set; } 
        public string Condition_ConditionItems { get; set; }
        public string Condition_Experience { get; set; } = "";
        public string Condition_YearsOfExperience { get; set; } = "";
        #endregion

        public string CopyrightHolder { get; set; }
        public string LatestVersion { get; set; }
        public string PreviousVersion { get; set; }

        public string ProcessStandards { get; set; }

        public string ProcessStandardsDescription { get; set; }
        
        //addresses
        public string AvailableAt { get; set; } 

 
    }

}
