using System.Collections.Generic;

namespace RA.Models.Input
{
	public class AssessmentRequest : BaseRequest
	{
		public AssessmentRequest()
		{
			Assessment = new Assessment();
		}

		public Assessment Assessment { get; set; }


	}
	public class Assessment
    {
        public Assessment()
        {
            Subject = new List<string>();
            Keyword = new List<string>();
			
			AssessmentMethodType = new List<string>();
            AudienceType = new List<string>();
            //CodedNotation = new List<string>();
			AssessmentUseType = new List<string>();

            AvailabilityListing = new List<string>();
            AvailableOnlineAt = new List<string>();
			Jurisdiction = new List<Input.Jurisdiction>();
			JurisdictionAssertions = new List<JurisdictionAssertedInProfile>();
			DeliveryType = new List<string>();

            EstimatedCost = new List<CostProfile>();
            EstimatedDuration = new List<DurationProfile>();
			//
            ScoringMethodType = new List<string>();
			
            AccreditedBy = new List<Input.OrganizationReference>();
            ApprovedBy = new List<Input.OrganizationReference>();
            OfferedBy = new List<Input.OrganizationReference>();
            RecognizedBy = new List<Input.OrganizationReference>();
            RegulatedBy = new List<Input.OrganizationReference>();

	        Corequisite = new List<ConditionProfile>();
            Recommends = new List<ConditionProfile>();
            Requires = new List<ConditionProfile>();
            EntryCondition = new List<ConditionProfile>();
			
			AdministrationProcess = new List<ProcessProfile>();
			DevelopmentProcess = new List<ProcessProfile>();
			MaintenanceProcess = new List<ProcessProfile>();

            Assesses = new List<CredentialAlignmentObject>();
			//RequiresCompetency = new List<CredentialAlignmentObject>();

			AvailableAt = new List<Place>();
			
			AdvancedStandingFrom = new List<Connections>();
			IsAdvancedStandingFor = new List<Connections>();
			PreparationFrom = new List<Connections>();
			IsPreparationFor = new List<Connections>();
			IsRecommendedFor = new List<Connections>();
			IsRequiredFor = new List<Connections>();

			ExternalResearch = new List<string>();
			InLanguage = new List<string>();
            CommonConditions = new List<string>();
            CommonCosts = new List<string>();
            FinancialAssistance = new List<Input.FinancialAlignmentObject>();
			VersionIdentifier = new List<IdentifierValue>();

		}



        #region *** Required Properties ***
        public string Name { get; set; }
        /// <summary>
        /// Alternately can provide a language map
        /// </summary>
        public LanguageMap Name_Map { get; set; } = new LanguageMap();
        /// <summary>
        /// Assessment Description 
        /// Required
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Alternately can provide a language map
        /// </summary>
        public LanguageMap Description_Map { get; set; } = new LanguageMap();

        public string Ctid { get; set; }
		public string SubjectWebpage { get; set; } //URL

        #region at least one of

        /// <summary>
        /// Organization that owns this resource
        /// </summary>
        public List<OrganizationReference> OwnedBy { get; set; } = new List<OrganizationReference>();
        //OR
        /// <summary>
        /// Organization(s) that offer this resource
        /// </summary>
        public List<OrganizationReference> OfferedBy { get; set; }
        #endregion

        #region at least one of the following
        public List<string> AvailableOnlineAt { get; set; } //URL
        public List<string> AvailabilityListing { get; set; } //URL
        public List<Place> AvailableAt { get; set; }
        #endregion

        #endregion

        #region *** Required if available Properties ***
        public List<CredentialAlignmentObject> Assesses { get; set; }

        public List<string> AssessmentMethodType { get; set; }
        public List<string> DeliveryType { get; set; }
        public string DeliveryTypeDescription { get; set; }
        #endregion

        #region *** Recommended Properties ***
        public string DateEffective { get; set; }

        //List of language codes. ex: en, es
        public List<string> InLanguage { get; set; }

        public List<DurationProfile> EstimatedDuration { get; set; }
        public List<ConditionProfile> Requires { get; set; }

		//Credit Information
		public QuantitiveValue CreditType { get; set; }
		public string CreditHourType { get; set; }
        public string CreditUnitType { get; set; }
        public decimal CreditHourValue { get; set; }
        public decimal CreditUnitValue { get; set; }
        public string CreditUnitTypeDescription { get; set; }

        public List<string> Keyword { get; set; }
        public List<string> Subject { get; set; }

        public string AssessmentExample { get; set; }
        public List<string> AssessmentUseType { get; set; }
        #endregion


        public string CodedNotation { get; set; }
          
        public string AssessmentExampleDescription { get; set; }
        public string AssessmentOutput { get; set; }
        
        
		public List<Jurisdiction> Jurisdiction { get; set; }
        public List<JurisdictionAssertedInProfile> JurisdictionAssertions { get; set; }

        public string ProcessStandards { get; set; }

        public string ProcessStandardsDescription { get; set; }
		//
		public List<FrameworkItem> OccupationType { get; set; }
		public List<string> AlternativeOccupationType { get; set; } = new List<string>();

		/// <summary>
		/// List of valid O*Net codes. See:
		/// https://www.onetonline.org/find/
		/// </summary>
		public List<string> ONET_Codes { get; set; } = new List<string>();

		public List<FrameworkItem> IndustryType { get; set; }
		public List<string> AlternativeIndustryType { get; set; } = new List<string>();

		public List<FrameworkItem> InstructionalProgramType { get; set; } = new List<FrameworkItem>();
		public List<string> AlternativeInstructionalProgramType { get; set; } = new List<string>();
		/// <summary>
		/// List of valid Classification of Instructional Program codes. See:
		/// https://nces.ed.gov/ipeds/cipcode/search.aspx?y=55
		/// </summary>
		public List<string> CIP_Codes { get; set; } = new List<string>();
		//
		public bool? IsProctored { get; set; }
        public bool? HasGroupEvaluation { get; set; }
        public bool? HasGroupParticipation { get; set; }

            
        //external classes
        public List<CostProfile> EstimatedCost { get; set; }

        public List<string> AudienceType { get; set; }
        public string ScoringMethodDescription { get; set; }
        public string ScoringMethodExample { get; set; }
        public string ScoringMethodExampleDescription { get; set; }
        public List<string> ScoringMethodType { get; set; }
        
        //Connections
		public List<Connections> AdvancedStandingFrom { get; set; }
		public List<Connections> IsAdvancedStandingFor { get; set; }
		public List<Connections> PreparationFrom { get; set; }
		public List<Connections> IsPreparationFor { get; set; }
		public List<Connections> IsRecommendedFor { get; set; }
		public List<Connections> IsRequiredFor { get; set; }

        //quality assurance
		public List<OrganizationReference> AccreditedBy { get; set; }
        public List<OrganizationReference> ApprovedBy { get; set; }
        
        public List<OrganizationReference> RecognizedBy { get; set; }
        public List<OrganizationReference> RegulatedBy { get; set; }

        

		//conditions
        public List<ConditionProfile> Corequisite { get; set; }
        public List<ConditionProfile> Recommends { get; set; }
        public List<ConditionProfile> EntryCondition { get; set; }

        //Processes
        public List<ProcessProfile> AdministrationProcess { get; set; }
        public List<ProcessProfile> DevelopmentProcess { get; set; }
        public List<ProcessProfile> MaintenanceProcess { get; set; }
        public List<string> ExternalResearch { get; set; }

		
        //required competencies are handled with condition profiles
        //public List<CredentialAlignmentObject> RequiresCompetency { get; set; }

        
        public List<string> CommonCosts { get; set; }
        public List<string> CommonConditions { get; set; }
        public List<FinancialAlignmentObject> FinancialAssistance { get; set; }
		public List<IdentifierValue> VersionIdentifier { get; set; }
	}
}
