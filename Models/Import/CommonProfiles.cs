using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Import
{
    public class CommonImportRequest
    {


        #region required
        public int CtidHdr { get; set; } = -1;
        public int ActionHdr { get; set; } = -1;
		//public int RecordIdentifierHdr { get; set; } = -1;

		public int ExternalIdentifierHdr { get; set; } = -1;
        public int InternalIdentifierHdr { get; set; } = -1;
        public int NameHdr { get; set; } = -1;
        public int DescHdr { get; set; } = -1;
        
        public int SubjectWebpageHdr { get; set; } = -1;
        public int OrganizationCtidHdr { get; set; } = -1;
        //public int OrganizationHdr { get; set; } = -1;
        public int OrganizationRolesHdr { get; set; } = -1;
        #endregion 

        public int InLanguageHdr { get; set; } = -1;
        public int InLanguageSingleHdr { get; set; } = -1;

        public int AudienceTypeHdr { get; set; } = -1;
        public int AvailabilityListingHdr { get; set; } = -1;
        public int AvailableOnlineAtHdr { get; set; } = -1;
		public int AssessmentDeliveryTypeHdr { get; set; } = -1;
		public int LearningDeliveryTypeHdr { get; set; } = -1;

		public int AvailableAtHdr { get; set; } = -1;
        public int AvailableAtCodesHdr { get; set; } = -1;
        public int KeywordsHdr { get; set; } = -1;
        public int AssessesCompetencyFrameworkHdr { get; set; } = -1;

        public int SubjectsHdr { get; set; } = -1;
        
        public int CommonConditionsHdr { get; set; } = -1;
        public int CommonCostsHdr { get; set; } = -1;

		public int OnetListHdr { get; set; } = -1;
		public int OccupationsHdr { get; set; } = -1;
		public int NaicsListHdr { get; set; } = -1;
		public int IndustriesHdr { get; set; } = -1;

		public int ProgramListHdr { get; set; } = -1;
		public int CIPListHdr { get; set; } = -1;

		//change to credit type
		public int CreditTypeHdr { get; set; } = -1;
		public int CreditMinValueHdr { get; set; } = -1;
		public int CreditMaxValueHdr { get; set; } = -1;

		public int CreditHourTypeHdr { get; set; } = -1;
		public int CreditHourValueHdr { get; set; } = -1;
		public int CreditUnitTypeHdr { get; set; } = -1;
		public int CreditUnitValueHdr { get; set; } = -1;
		public int CreditUnitDescriptionHdr { get; set; } = -1;

		#region Rare
		public int DateEffectiveHdr { get; set; } = -1;

        #endregion


        #region Duration 
        //allow combined - maybe
        public int DurationHdr { get; set; } = -1;
        public int DurationDescHdr { get; set; } = -1;

        //years, months, hours
        //public int DurationTypeHdr { get; set; } = -1;
        //public int DurationAmountHdr { get; set; } = -1;
        #endregion


        #region Conditions
        public bool HasConditionProfile { get; set; }
        public int ConditionExternalIdentifierHdr { get; set; } = -1;
        public int ConditionIdentifierHdr { get; set; } = -1;
        public int ConditionTypeHdr { get; set; } = -1;
        public int ConditionNameHdr { get; set; } = -1;
        public int ConditionDescHdr { get; set; } = -1;
        public int ConditionSubmissionHdr { get; set; } = -1;
        public int ConditionConditionsHdr { get; set; } = -1;

        public int ConditionSubjectWebpageHdr { get; set; } = -1;
        public int ConditionExperienceHdr { get; set; } = -1;
        public int ConditionYearsOfExperienceHdr { get; set; } = -1;

        public int ConditionCreditHourTypeHdr { get; set; } = -1;
        public int ConditionCreditHourValueHdr { get; set; } = -1;
        public int ConditionCreditUnitTypeHdr { get; set; } = -1;
        public int ConditionCreditUnitValueHdr { get; set; } = -1;
        public int ConditionCreditUnitDescriptionHdr { get; set; } = -1;

        //will always need the related identifier for the condition
        //could be a delimited list of asmt ids, versus one per row?
        public int ConditionExistingAsmtHdr { get; set; } = -1;
        //could be a delimited list of asmt ids, versus one per row?
        public int ConditionAsmtIdentifierHdr { get; set; } = -1;
        public int ConditionCredentialsListHdr { get; set; } = -1;
		public int ConditionAsmtsListHdr { get; set; } = -1;
		public int ConditionLoppsListHdr { get; set; } = -1;
		#endregion

		#region Connections
		public bool HasConnectionProfile { get; set; }
		public int ConnectionExternalIdentifierHdr { get; set; } = -1;
		public int ConnectionIdentifierHdr { get; set; } = -1;
		public int ConnectionTypeHdr { get; set; } = -1;
		public int ConnectionDescHdr { get; set; } = -1;

		public int ConnectionCreditHourTypeHdr { get; set; } = -1;
		public int ConnectionCreditHourValueHdr { get; set; } = -1;
		public int ConnectionCreditUnitTypeHdr { get; set; } = -1;
		public int ConnectionCreditUnitValueHdr { get; set; } = -1;
		public int ConnectionCreditUnitDescriptionHdr { get; set; } = -1;

		public int ConnectionCredentialsListHdr { get; set; } = -1;
		public int ConnectionAsmtsListHdr { get; set; } = -1;
		public int ConnectionLoppsListHdr { get; set; } = -1;
        public int ConnectionWeightHdr { get; set; } = -1;
		#endregion

		#region assessments
		public bool HasAssessment { get; set; }
        //could be different from ConditionExistingAsmtHdr - TBD
        public int AssessmentExternalIdentifierHdr { get; set; } = -1;
        public int AssessmentIdentifierHdr { get; set; } = -1;
        //public int ConditionExistingAsmtHdr { get; set; } = -1;
        //could be a delimited list of asmt ids, versus one per row?
        //public int ConditionAsmtIdentifierHdr { get; set; } = -1;

        public int AssessmentNameHdr { get; set; } = -1;
        public int AssessmentDescHdr { get; set; } = -1;
        public int AssessmentSubjectWebpageHdr { get; set; } = -1;

        public int AssessmentAvailableAtHdr { get; set; } = -1;
        public int AssessmentAvailabilityListingHdr { get; set; } = -1;
        //public int AssessmentExampleUrlHdr { get; set; } = -1;

        public int AsmtDateEffectiveHdr { get; set; } = -1;
        public int AsmtIdentificationCodeHdr { get; set; } = -1;
#endregion


        #region Learningopps
        //public bool HasLearningOpp { get; set; }

        ////could be different from ConditionLoppIdentifierHdr - TBD
        //public int ConditionExistingLoppHdr { get; set; } = -1;
        ////could be a delimited list of asmt ids, versus one per row?
        //public int ConditionLoppIdentifierHdr { get; set; } = -1;

        ////external identifier for an lopp
        //public int LearningOppExternalIdentifierHdr { get; set; } = -1;
        ////internal identifier (rowId) 
        //public int LearningOppInternalIdentifierHdr { get; set; } = -1;


        //public int LearningOppNameHdr { get; set; } = -1;
        //public int LearningOppDescHdr { get; set; } = -1;
        //public int LearningOppSubjectWebpageHdr { get; set; } = -1;

        //public int LearningOppAvailableAtHdr { get; set; } = -1;
        //public int LearningOppAvailabilityListingHdr { get; set; } = -1;
        //public int LearningOppLearningResourceUrlHdr { get; set; } = -1;

        //public int LoppIdentificationCodeHdr { get; set; } = -1;
        //public int LoppDateEffectiveHdr { get; set; } = -1;
        #endregion

        #region Org roles
        public int OfferedByListHdr { get; set; } = -1;
        public int AccreditedByListHdr { get; set; } = -1;


        public int ApprovedByListHdr { get; set; } = -1;
        public int RegulatedByListHdr { get; set; } = -1;
        public int RecognizedByListHdr { get; set; } = -1;
        public int RevokedByListHdr { get; set; } = -1;
        public int RenewedByListHdr { get; set; } = -1;
        #endregion

        #region costs
        public bool HasCosts { get; set; }
        public int CostExternalIdentifierHdr { get; set; } = -1;
        public int CostInternalIdentifierHdr { get; set; } = -1;
        public int CostNameHdr { get; set; } = -1;
        public int CostDescriptionHdr { get; set; } = -1;

        public int CostDetailUrlHdr { get; set; } = -1;
        public int CostCurrencyTypeHdr { get; set; } = -1;
        public int CostTypesListHdr { get; set; } = -1;

        #endregion
    }
    public class CredentialImportRequest : CommonImportRequest
    {
        public CredentialImportRequest()
        {

        }

        #region required

        public int TypeHdr { get; set; } = -1;
        public int StatusHdr { get; set; } = -1;

  
        //NOT currently used
        //public int OrganizationExternalIdHdr { get; set; } = -1;
        //public int OrganizationCtidHdr { get; set; } = -1;
        ////public int OrganizationHdr { get; set; } = -1;
        //public int OrganizationRolesHdr { get; set; } = -1;
        #endregion 

        public int ImageUrlHdr { get; set; } = -1;
        public int CodedNotationHdr { get; set; } = -1;
        public int Copyrightholder_CtidHdr { get; set; } = -1;
        public int CredentialIdHdr { get; set; } = -1;
        public int AlternateNameHdr { get; set; } = -1;
        public int AudienceLevelHdr { get; set; } = -1;

        //public int InLanguageHdr { get; set; } = -1;

        //public int AvailabilityListingHdr { get; set; } = -1;
        //public int AvailableOnlineAtHdr { get; set; } = -1;
        ////addresses
        //public int AvailableAtHdr { get; set; } = -1;
        //public int AvailableAtCodesHdr { get; set; } = -1;
        //public int KeywordsHdr { get; set; } = -1;

        //public int SubjectsHdr { get; set; } = -1;
        public int DegreeMajorsHdr { get; set; } = -1;
        public int DegreeMinorHdr { get; set; } = -1;
        public int DegreeConcentrationHdr { get; set; } = -1;
        


		public int RenewalFrequencyHdr { get; set; } = -1;
        //public int CommonCostsHdr { get; set; } = -1;

        #region Rare
        //Globally unique identifier by which the creator, owner or provider of a credential recognizes that credential in transactions with the external environment (e.g., in verifiable claims involving the credential).

        public int VersionIdentifierHdr { get; set; } = -1;
        public int LatestVersionHdr { get; set; } = -1;
        public int PreviousVersionHdr { get; set; } = -1;
        public int ProcessStandardsHdr { get; set; } = -1;
        public int ProcessStandardsDescHdr { get; set; } = -1;

        #endregion


        #region Duration 
        //allow combined - maybe
        //public int DurationHdr { get; set; } = -1;
        //public int DurationDescHdr { get; set; } = -1;

        //years, months, hours
        //public int DurationTypeHdr { get; set; } = -1;
        //public int DurationAmountHdr { get; set; } = -1;
        #endregion


        #region Conditions - moved
        //public bool HasConditionProfile { get; set; }
        //public int ConditionExternalIdentifierHdr { get; set; } = -1;
        //public int ConditionIdentifierHdr { get; set; } = -1;
        //public int ConditionTypeHdr { get; set; } = -1;
        //public int ConditionNameHdr { get; set; } = -1;
        //public int ConditionDescHdr { get; set; } = -1;
        //public int ConditionSubmissionHdr { get; set; } = -1;
        //public int ConditionConditionsHdr { get; set; } = -1;

        //public int ConditionSubjectWebpageHdr { get; set; } = -1;
        //public int ConditionExperienceHdr { get; set; } = -1;
        //public int ConditionYearsOfExperienceHdr { get; set; } = -1;

        //public int ConditionCreditHourTypeHdr { get; set; } = -1;
        //public int ConditionCreditHourValueHdr { get; set; } = -1;
        //public int ConditionCreditUnitTypeHdr { get; set; } = -1;
        //public int ConditionCreditUnitValueHdr { get; set; } = -1;
        //public int ConditionCreditUnitDescriptionHdr { get; set; } = -1;

        ////will always need the related identifier for the condition
        ////could be a delimited list of asmt ids, versus one per row?
        //public int ConditionExistingAsmtHdr { get; set; } = -1;
        ////could be a delimited list of asmt ids, versus one per row?
        //public int ConditionAsmtIdentifierHdr { get; set; } = -1;
        #endregion

    }

    public class AssessmentImportRequest : CommonImportRequest
    {
        #region assessments
        /// <summary>
        /// only used when referenced from a credential!
        /// </summary>
        //public bool HasAssessment { get; set; }

        //will always need the related identifier for the condition
        //could be a delimited list of asmt ids, versus one per row?
        //this may belong with the condition profile. It would be used
        public int ConditionProfileInternalIdentifierHdr { get; set; } = -1;
        //an alternative could be to allow including the actual credential identifier
        //** actually will need to allow a list **
        //then will either create a required condition in the credential ,and add this. 
        //or add to an existing requires condition
        public int TargetCredentialListHdr { get; set; } = -1;
        public int CredentialCtidHdr { get; set; } = -1;
        /// <summary>
        /// Or, allow specifying the condition type. Where can only have one of each type.
        /// </summary>
        public int CredentialConditionTypeHdr { get; set; } = -1;



        //public int NameHdr { get; set; } = -1;
        //public int DescHdr { get; set; } = -1;
        //public int SubjectWebpageHdr { get; set; } = -1;

        //public int AvailableOnlineAtHdr { get; set; } = -1;

        public int AssessmentExampleUrlHdr { get; set; } = -1;
        public int AssessmentExampleDescriptionHdr { get; set; } = -1;
        public int AssessmentMethodTypeHdr { get; set; } = -1;
        //Description of the assessment artifact, performance or examination.
        public int AssessmentOutputHdr { get; set; } = -1;
        public int AssessmentUseTypeHdr { get; set; } = -1;
        public int CodedNotationHdr { get; set; } = -1;

        public int DeliveryTypeHdr { get; set; } = -1;
        public int DeliveryTypeDescriptionHdr { get; set; } = -1;
        public int ExternalResearchHdr { get; set; } = -1;
        public int HasGroupEvaluationHdr { get; set; } = -1;
        public int HasGroupParticipationHdr { get; set; } = -1;
        public int IsProctoredHdr { get; set; } = -1;

        public int ProcessStandardsHdr { get; set; } = -1;
        public int ProcessStandardsDescHdr { get; set; } = -1;
        public int ScoringMethodTypeHdr { get; set; } = -1;
        public int ScoringMethodDescriptionHdr { get; set; } = -1;
        public int ScoringMethodExampleHdr { get; set; } = -1;
        public int ScoringMethodExampleDescriptionHdr { get; set; } = -1;
        public int VerificationMethodDescriptionHdr { get; set; } = -1;
        public int VersionIdentifierHdr { get; set; } = -1;
        //public int DateEffectiveHdr { get; set; } = -1;



        #endregion
    }

    public class LoppImportRequest : CommonImportRequest
    {

        public bool HasLearningOpp { get; set; }
        //will always need the related identifier for the condition
        //could be a delimited list of asmt ids, versus one per row?
        public int ConditionLoppIdentifiersHdr { get; set; } = -1;
        //this may belong with the condition profile. It would be used
        //an alternative could be to allow including the actual credential identifier
        //** actually will need to allow a list **
        //then will either create a required condition in the credential ,and add this. 
        //or add to an existing requires condition
        public int TargetCredentialListHdr { get; set; } = -1;
        public int CredentialCtidHdr { get; set; } = -1;

        /// <summary>
        /// Or, allow specifying the condition type. Where can only have one of each type.
        /// </summary>
        public int CredentialConditionTypeHdr { get; set; } = -1;

        public int CodedNotationHdr { get; set; } = -1;

        public int DeliveryTypeHdr { get; set; } = -1;
        public int DeliveryTypeDescriptionHdr { get; set; } = -1;
        public int LearningMethodTypeHdr { get; set; } = -1;


        public int LearningResourceUrlHdr { get; set; } = -1;
		public int TeachesCompetencyFrameworkHdr { get; set; } = -1;
		public int VersionIdentifierHdr { get; set; } = -1;

        public int VerificationMethodDescriptionHdr { get; set; } = -1;
    }
    public class QAPerformedRequest : CommonImportRequest
    {

        public int ArtifactTypeHdr { get; set; } = -1;
        public int AssertionsHdr { get; set; } = -1;
    }

    public class LocationImportHelper
    {
        public LocationImportHelper()
        {

        }

        #region required
        
        public int IdentifierHdr { get; set; } = -1;
        public int AddressNameHdr { get; set; } = -1;
        public int Address1Hdr { get; set; } = -1;
        public int Address2Hdr { get; set; } = -1;
        public int CityHdr { get; set; } = -1;
        public int RegionHdr { get; set; } = -1;
        public int PostalcodeHdr { get; set; } = -1;
        public int OrganizationExternalIdHdr { get; set; } = -1;
        public int OrganizationCtidHdr { get; set; } = -1;
        public int POBoxHdr { get; set; } = -1;
        public int CountryHdr { get; set; } = -1;
        #endregion 


    }
}
