using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RA.Models;

namespace RA.Models.Input
{
	/// <summary>
	/// Class used with a Credential format or publish request
	/// </summary>
	public class CredentialRequest : BaseRequest
	{
		public CredentialRequest()
		{
			Credential = new Credential();
		}
		/// <summary>
		/// Credential Input Class
		/// </summary>
		public Credential Credential { get; set; }

	}

	public class Credential
	{
		public Credential()
		{
			AudienceLevelType = new List<string>();
            AudienceType = new List<string>();
            Subject = new List<string>();
			OccupationType = new List<FrameworkItem>();
			IndustryType = new List<FrameworkItem>();
			Naics = new List<string>();
			Keyword = new List<string>();
			DegreeConcentration = new List<string>();
			DegreeMajor = new List<string>();
			DegreeMinor = new List<string>();

			EstimatedCost = new List<CostProfile>();
			EstimatedDuration = new List<DurationProfile>();
			RenewalFrequency = new DurationItem();
			
			Jurisdiction = new List<Jurisdiction>();
			// Region = new List<GeoCoordinates>();
			OwnedBy = new List<OrganizationReference>();
			CopyrightHolder = new OrganizationReference();

			AccreditedBy = new List<Input.OrganizationReference>();
			ApprovedBy = new List<Input.OrganizationReference>();
			OfferedBy = new List<Input.OrganizationReference>();
			RecognizedBy = new List<Input.OrganizationReference>();
			RegulatedBy = new List<Input.OrganizationReference>();
			RenewedBy = new List<Input.OrganizationReference>();
			RevokedBy = new List<Input.OrganizationReference>();


			Corequisite = new List<ConditionProfile>();
			Recommends = new List<ConditionProfile>();
			Requires = new List<ConditionProfile>();

			//ProcessProfile = new List<Input.ProcessProfile>();
			CommonConditions = new List<string>();
			CommonCosts = new List<string>();

			HasPart = new List<EntityReference>();
			IsPartOf = new List<EntityReference>();

			FinancialAssistance = new List<Input.FinancialAlignmentObject>();
			AdvancedStandingFrom = new List<Connections>();
			IsAdvancedStandingFor = new List<Connections>();
			PreparationFrom = new List<Connections>();
			IsPreparationFor = new List<Connections>();
			IsRecommendedFor = new List<Connections>();
			IsRequiredFor = new List<Connections>();
			InLanguage = new List<string>();
			AvailableAt = new List<Place>();
			//CodedNotation = new List<string>();
			Renewal = new List<ConditionProfile>();
			Revocation = new List<RevocationProfile>();
			AdministrationProcess = new List<ProcessProfile>();
			DevelopmentProcess = new List<ProcessProfile>();
			MaintenanceProcess = new List<ProcessProfile>();
			AppealProcess = new List<ProcessProfile>();
			ComplaintProcess = new List<ProcessProfile>();
			ReviewProcess = new List<ProcessProfile>();
			RevocationProcess = new List<ProcessProfile>();

			VersionIdentifier = new List<IdentifierValue>();
		}


        #region *** Required Properties ***


        /// <summary>
        /// Name of this credential
        /// Required
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Alternately can provide a language map
        /// </summary>
        public LanguageMap Name_Map { get; set; } = new LanguageMap();
        /// <summary>
        /// Credential description 
        /// Required
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Alternately can provide a language map
        /// </summary>
        public LanguageMap Description_Map { get; set; } = new LanguageMap();
        /// <summary>
        /// Organization that owns this credential
        /// </summary>
        public List<OrganizationReference> OwnedBy { get; set; }

		/// <summary>
		/// The credential type as defined in CTDL
		/// </summary>
		public string CredentialType { get; set; }

        public string CredentialStatusType { get; set; }
        /// <summary>
        /// CTID - unique identifier
        /// If not provided, will be set to ce-UUID
        /// ex: ce-F22CA1DC-2D2E-49C0-9E72-0457AD348873
        /// It will be the primary key for retrieving this entity from the registry. 
        /// Also it must be provided 
        /// </summary>
        public string Ctid { get; set; }

		/// <summary>
		/// SubjectWebpage URL
		/// </summary>
		public string SubjectWebpage { get; set; } //URL

        public List<string> InLanguage { get; set; }
        #endregion

        #region *** Required if available Properties ***

        #endregion


        public string DateEffective { get; set; }
        /// <summary>
        /// AlternateName
        /// </summary>
        public List<string> AlternateName { get; set; } = new List<string>();

        public LanguageMapList AlternateName_Map { get; set; } = new LanguageMapList();
        /// <summary>
        /// Image URL
        /// </summary>
        public string Image { get; set; }


		/// <summary>
		/// AvailableOnlineAt URL
		/// </summary>
		public string AvailableOnlineAt { get; set; }
        /// <summary>
        /// AvailabilityListing URL
        /// </summary>
        public string AvailabilityListing { get; set; }
        public List<Place> AvailableAt { get; set; }

		public OrganizationReference CopyrightHolder { get; set; }
		/// <summary>
		/// could use general input or specific
		/// general has more opportunity for errors
		/// </summary>
		public List<OrganizationReference> AccreditedBy { get; set; }
		public List<OrganizationReference> ApprovedBy { get; set; }
		public List<OrganizationReference> OfferedBy { get; set; }
		public List<OrganizationReference> RecognizedBy { get; set; }
		public List<OrganizationReference> RegulatedBy { get; set; }
		public List<OrganizationReference> RenewedBy { get; set; }
		public List<OrganizationReference> RevokedBy { get; set; }


        public List<EntityReference> HasPart { get; set; }
        public List<EntityReference> IsPartOf { get; set; }


        #region INs
        /// <summary>
        /// Handling assertions in jurisdictions
        /// The current approach is to use a one record per asserting organization. The JurisdictionAssertedInProfile has a list of boolean properties where the assertion(s) can be selected
        /// </summary>
        public List<JurisdictionAssertedInProfile> JurisdictionAssertions { get; set; }

	
		#endregion


		public string CredentialId { get; set; }
		//public string VersionIdentifier { get; set; }
		public List<IdentifierValue> VersionIdentifier { get; set; }
		public string CodedNotation { get; set; }

		public string ProcessStandards { get; set; }
		public string ProcessStandardsDescription { get; set; }
        public LanguageMap ProcessStandardsDescription_Map { get; set; } = new LanguageMap();

        public string LatestVersion { get; set; }
		public string PreviousVersion { get; set; }
		public List<string> Subject { get; set; }
		public List<string> AudienceLevelType { get; set; }
        public List<string> AudienceType { get; set; }
		public List<string> AssessmentDeliveryType { get; set; } = new List<string>();
		public List<string> LearningDeliveryType { get; set; } = new List<string>();

		public List<FrameworkItem> OccupationType { get; set; }
		public List<string> AlternativeOccupationType { get; set; } = new List<string>();
		public List<FrameworkItem> IndustryType { get; set; }
		public List<string> AlternativeIndustryType { get; set; } = new List<string>();
		public List<string> Naics { get; set; }
		public List<FrameworkItem> InstructionalProgramType { get; set; } = new List<FrameworkItem>();
		public List<string> AlternativeInstructionalProgramType { get; set; } = new List<string>();
		public List<string> Keyword { get; set; }
        public LanguageMapList Keyword_Map { get; set; } = new LanguageMapList();
        public List<string> DegreeConcentration { get; set; }
		public List<string> DegreeMajor { get; set; }
		public List<string> DegreeMinor { get; set; }
		//external classes
		public List<CostProfile> EstimatedCost { get; set; }

		public List<DurationProfile> EstimatedDuration { get; set; }
		public DurationItem RenewalFrequency { get; set; }
		
		public List<Jurisdiction> Jurisdiction { get; set; }
		// public List<GeoCoordinates> Region { get; set; }

		public List<ConditionProfile> Requires { get; set; }
		public List<ConditionProfile> Recommends { get; set; }
		public List<ConditionProfile> Corequisite { get; set; }
		public List<ConditionProfile> Renewal { get; set; }

		public List<string> CommonCosts { get; set; }
		public List<string> CommonConditions { get; set; }

		public List<Connections> AdvancedStandingFrom { get; set; }
		public List<Connections> IsAdvancedStandingFor { get; set; }
		public List<Connections> IsPreparationFor { get; set; }
		public List<Connections> IsRecommendedFor { get; set; }
		public List<Connections> IsRequiredFor { get; set; }
		public List<Connections> PreparationFrom { get; set; }

		public List<ProcessProfile> AdministrationProcess { get; set; }
		public List<ProcessProfile> DevelopmentProcess { get; set; }
		public List<ProcessProfile> MaintenanceProcess { get; set; }
		public List<ProcessProfile> AppealProcess { get; set; }
		public List<ProcessProfile> ComplaintProcess { get; set; }
		public List<ProcessProfile> ReviewProcess { get; set; }
		public List<ProcessProfile> RevocationProcess { get; set; }
		public List<FinancialAlignmentObject> FinancialAssistance { get; set; }

		
		public List<RevocationProfile> Revocation { get; set; }
		
	}

	/// <summary>
	/// Revocation Profile
	/// The conditions and methods by which a credential can be removed from a holder.
	/// </summary>
	public class RevocationProfile
	{
		public RevocationProfile()
		{
			Jurisdiction = new List<Input.Jurisdiction>();
			//RevocationCriteria = new List<string>();
		}
		public string Description { get; set; }
        public LanguageMap Description_Map { get; set; } = new LanguageMap();
        //public List<string> CredentialProfiled { get; set; }
        public string DateEffective { get; set; }
		public List<Jurisdiction> Jurisdiction { get; set; }
		public string RevocationCriteria { get; set; }
		public string RevocationCriteriaDescription { get; set; }

	}
}
