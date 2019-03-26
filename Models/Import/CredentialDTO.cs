using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.ProfileModels;
using Models.Common;
namespace Models.Import
{
    public class CredentialDTO : BaseDTO
    {

		public new Credential ExistingRecord { get; set; } = new Credential();

		public new bool FoundExistingRecord
		{
			get
			{
				if ( ExistingRecord != null && ExistingRecord.Id > 0 )
					return true;
				else
					return false;
			}
		}




		public string CredentialTypeSchema { get; set; }
        public Enumeration CredentialType { get; set; } = new Enumeration();
        public string CredentialStatusSchema { get; set; }
        public Enumeration CredentialStatus { get; set; } = new Enumeration();      


        public Guid CopyrightHolder { get; set; }
        public bool DeleteCopyrightHolder { get; set; }

        public string AlternateName { get; set; }
        public string AudienceLevelTypesList { get; set; }
        public Enumeration AudienceLevelType { get; set; } = new Enumeration();

		public string AssessmentDeliveryTypeList { get; set; }
		public Enumeration AssessmentDeliveryType { get; set; } = new Enumeration();
		public string LearningDeliveryTypeList { get; set; }
		public Enumeration LearningDeliveryType { get; set; } = new Enumeration();

		public string CodedNotation { get; set; }
        public string CredentialId { get; set; }
        public string VersionIdentifier { get; set; }

        public string LatestVersion { get; set; }
        
		public string ProcessStandards { get; set; }

        public string ProcessStandardsDescription { get; set; }

        public string PreviousVersion { get; set; }
        
        public List<string> DegreeMajors { get; set; } = new List<string>();
        public List<string> DegreeMinors { get; set; } = new List<string>();
        public List<string> DegreeConcentrations { get; set; } = new List<string>();


        public bool HasAssessments
        {
            get
            {
                if ( Assessments == null || Assessments.Count == 0 )
                    return false;
                else //not really correct
                    return Assessments[0].IsNotEmpty;
            }
        }
        public List<AssessmentProfile> Assessments { get; set; } = new List<AssessmentProfile>();
        //public List<AssessmentDTO> AssessmentsDTO { get; set; } = new List<AssessmentDTO>();
        public bool HasLearningOpps
        {
            get
            {
                if ( LearningOpps == null || LearningOpps.Count == 0 )
                    return false;
                else //not really correct
                    return LearningOpps[ 0 ].IsNotEmpty;
            }
        }
        public List<LearningOppDTO> LearningOpps { get; set; } = new List<LearningOppDTO>();




    }
    public class ConditionProfileDTO
    {
        public bool DeletingProfile { get; set; }
        public Guid RowId { get; set; }
        public string ExternalIdentifier { get; set; }
        //NOTE: may only need to use ConditionTypeId
        public string ConditionType { get; set; }
        public int ConditionTypeId { get; set; }
		/// <summary>
		/// 1-condition profile
		/// 2-credential connection
		/// 3-assessment connection
		/// 4-learningopportunity connection
		/// </summary>
        public int ConditionSubTypeId { get; set; } = 1;
		public bool IsAConnectionProfile
		{
			get
			{
				if ( ConditionSubTypeId < 2 )
					return false;
				else
					return true;
			}
		}
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string SubjectWebpage { get; set; } = "";
        public List<string> SubmissionItems { get; set; } = new List<string>();
        public List<string> ConditionItems { get; set; } = new List<string>();
        public string Experience { get; set; } = "";
        //use -99 as an indicator to delete the value
        public Decimal YearsOfExperience { get; set; }
        public string YearsOfExperienceDisplay { get; set; }

        public CreditStuffDTO CreditStuffDTO { get; set; } = new CreditStuffDTO();
        public string CreditHourType { get; set; }
        public decimal CreditHourValue { get; set; }
        public int CreditUnitTypeId { get; set; }
        public string CreditUnitType { get; set; } = "";
        //public Enumeration CreditUnitType { get; set; } //Used for publishing
        public string CreditUnitTypeDescription { get; set; }
        public decimal CreditUnitValue { get; set; }
		public Decimal Weight { get; set; }
		//temp work property
		//public string TargetCtid { get; set; } = "";
        public List<AssessmentProfile> TargetAssessmentList { get; set; } = new List<AssessmentProfile>();
        public bool DeleteTargetAssessments { get; set; }
        public List<Credential> TargetCredentialList { get; set; } = new List<Credential>();
        public bool DeleteTargetCredentials { get; set; }

        public List<LearningOpportunityProfile> TargetLearningOpportunityList { get; set; } = new List<LearningOpportunityProfile>();
        public bool DeleteTargetLearningOpportunities { get; set; }
        public bool IsNotEmpty
        {
            //future
            //                    || ( TargetAssessment != null && TargetAssessment.Id > 0 )
            get
            {
                if (!string.IsNullOrEmpty( Description )
                    || ( SubmissionItems != null && SubmissionItems.Count > 0 )
                    || ( ConditionItems != null && ConditionItems.Count > 0 )
                    || ( !string.IsNullOrEmpty( Description ) )
                    || ( !string.IsNullOrEmpty( SubjectWebpage ) )
                    || ( !string.IsNullOrEmpty( Experience ))
                    || ( YearsOfExperience  > 0)
					|| ( CreditHourValue > 0 )
					|| ( CreditUnitValue > 0 )
					|| ( !string.IsNullOrEmpty( CreditHourType ) )
					|| ( !string.IsNullOrEmpty( CreditUnitTypeDescription ) )
					|| ( Weight > 0 )
					|| DeletingProfile
                    )
                    return true;
                else
                    return false;
            }
        }
    }
    public class CreditStuffDTO
    {
        public string CreditHourType { get; set; }
        public decimal CreditHourValue { get; set; }
        public int CreditUnitTypeId { get; set; }
        public string CreditUnitTypeList { get; set; } = "";
        public Enumeration CreditUnitType { get; set; } //Used for publishing
        public string CreditUnitTypeDescription { get; set; }
        public decimal CreditUnitValue { get; set; }

        public bool IsNotEmpty
        {
            get
            {
                if ( !string.IsNullOrEmpty( CreditHourType )
                    || ( !string.IsNullOrEmpty( CreditUnitTypeDescription ) )
                    || ( !string.IsNullOrEmpty( CreditUnitTypeList ) )
                    || ( CreditHourValue > 0 )
                    || ( CreditUnitValue > 0 )
                    )
                    return true;
                else
                    return false;
            }
        }
    }
    public class CostProfileDTO
    {
        public bool DeletingProfile { get; set; }
        public Guid Identifier { get; set; }
        public string ExternalIdentifier { get; set; }

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string DetailsUrl { get; set; } = "";
        public string CurrencyType { get; set; } = "";
        public int CurrencyTypeId { get; set; } 

        public bool DeleteCostItems { get; set; }
        public List<CostProfileItemDTO> CostItems { get; set; } = new List<CostProfileItemDTO>();

        public CostProfile ExistingCostProfile { get; set; } = new CostProfile();
        public bool IsExistingCostProfile
        {
            get
            {
                if ( ExistingCostProfile != null && ExistingCostProfile.Id > 0 )
                    return true;
                else
                    return false;
            }
        }

        public bool IsNotEmpty
        {
            get
            {
                if ( !string.IsNullOrEmpty( Name )
                    || ( CostItems != null && CostItems.Count > 0 )
                    || ( !string.IsNullOrEmpty( Description ) )
                    || ( !string.IsNullOrEmpty( DetailsUrl ) )
                    )
                    return true;
                else
                    return false;
            }
        }
    }
    public class CostProfileItemDTO
    {
        public EnumeratedItem CostItem { get; set; }
        public int DirectCostTypeId { get; set; }
        public string DirectCostType { get; set; } = "";
        public decimal Price { get; set; }
    }

    public class AssessmentDTO : BaseDTO
    {
        public bool DeleteProfile { get; set; }
        public Guid Identifier { get; set; }

        /// <summary>
        /// Proposal: allow including the external identifier used for a credential upload. 
        /// - allow specifying the condition type.
        /// - check if a condition profile exists
        ///     - if not create the specified type, and add as target asmt
        ///     - otherwise add to existing as target
        ///     - need to check if already part of condition
        /// </summary>
        public string CredentialExternalIdentifier { get; set; }
        public int CredentialConditionTypeId { get; set; }
//        public string CredentialConditionProfileType { get; set; }
        public List<Credential> TargetCredentials { get; set; } = new List<Credential>();

		public new AssessmentProfile ExistingRecord { get; set; } = new AssessmentProfile();
		public new bool FoundExistingRecord
		{
			get
			{
				if ( ExistingRecord != null && ExistingRecord.Id > 0 )
					return true;
				else
					return false;
			}
		}

		public string AssessmentExampleUrl { get; set; }
        public string AssessmentExampleDescription { get; set; }
        

        public string AssessmentMethodTypeList { get; set; }
        public Enumeration AssessmentMethodType { get; set; } = new Enumeration();
        public string AssessmentOutput { get; set; }
        public string AssessmentUseTypeList { get; set; }
        public Enumeration AssessmentUseType { get; set; } = new Enumeration();

        public string CodedNotation { get; set; }
		//Credit type
		public CreditStuffDTO CreditStuffDTO { get; set; } = new CreditStuffDTO();
		public string CreditHourType { get; set; }
        public decimal CreditHourValue { get; set; }
        public int CreditUnitTypeId { get; set; }
        public string CreditUnitType { get; set; } = "";
        //public Enumeration CreditUnitType { get; set; } //Used for publishing
        public string CreditUnitTypeDescription { get; set; }
        public decimal CreditUnitValue { get; set; }

        public string DeliveryTypeList { get; set; }
        public Enumeration DeliveryType { get; set; } = new Enumeration();
        public string DeliveryTypeDescription { get; set; }
        //url
        public string ExternalResearch { get; set; }

        /// 0 - if entered false
        /// 1 - if entered true
        /// 2 - if entered #delete
        /// 3 - no entry
        public int HasGroupEvaluation { get; set; } 
        public int HasGroupParticipation { get; set; } 
        public int IsProctored { get; set; }

		
		//public string AudienceTypesList { get; set; }
		//public Enumeration AudienceType { get; set; } = new Enumeration();
		public string ProcessStandards { get; set; }
        public string ProcessStandardsDescription { get; set; }

        public string ScoringMethodTypeList { get; set; }
        public Enumeration ScoringMethodType { get; set; } = new Enumeration();
        public string ScoringMethodDescription { get; set; }
        //url
        public string ScoringMethodExample { get; set; }
        public string ScoringMethodExampleDescription { get; set; }
        public string VerificationMethodDescription { get; set; }
        public string VersionIdentifier { get; set; }
        
        public bool IsNotEmpty
        {
            get
            {
                if ( !string.IsNullOrEmpty( Name )
                    || ( !string.IsNullOrEmpty( Description ) )
                    || ( !string.IsNullOrEmpty( SubjectWebpage ) )
                    )
                    return true;
                else
                    return false;
            }
        }
    }
    
}
