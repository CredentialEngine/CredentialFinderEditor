using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{

	public class LearningOpportunityProfile : BaseProfile
	{
		public LearningOpportunityProfile()
		{
			OwningOrganization = new Organization();
			//LearningResourceUrl = new List<TextValueProfile>();
			//ResourceUrls = new List<TextValueProfile>();
			//LearningResourceUrls = new List<TextValueProfile>();

			EstimatedCost = new List<CostProfile>();
			FinancialAssistance = new List<FinancialAlignmentObject>();
			EstimatedDuration = new List<DurationProfile>();
			DeliveryType = new Enumeration();
			InstructionalProgramCategory = new Enumeration();
			HasPart = new List<LearningOpportunityProfile>();
			IsPartOf = new List<LearningOpportunityProfile>();
			//IsPartOfConditionProfile = new List<ConditionProfile>();
			OrganizationRole = new List<OrganizationRoleProfile>();
			//QualityAssuranceAction = new List<QualityAssuranceActionProfile>();
			WhereReferenced = new List<string>();
			//LearningCompetencies = new List<TextValueProfile>();
			Subject = new List<TextValueProfile>();
			Keyword = new List<TextValueProfile>();
			Addresses = new List<Address>();
			CommonCosts = new List<CostManifest>();
			CommonConditions = new List<ConditionManifest>();
			Requires = new List<ConditionProfile>();
			Recommends = new List<ConditionProfile>();
			Corequisite = new List<ConditionProfile>();
			EntryCondition = new List<ConditionProfile>();
			LearningOppConnections = new List<ConditionProfile>();
			IsPartOfConditionProfile = new List<ConditionProfile>();
			IsPartOfCredential = new List<Credential>();
			TeachesCompetenciesFrameworks = new List<CredentialAlignmentObjectFrameworkProfile>();
			RequiresCompetenciesFrameworks = new List<CredentialAlignmentObjectFrameworkProfile>();

			TeachesCompetencies = new List<CredentialAlignmentObjectProfile>();
			RequiresCompetencies = new List<CredentialAlignmentObjectProfile>();
			//EmbeddedAssessment = new List<AssessmentProfile>();
			//LearningOpportunityProcess = new List<ProcessProfile>();

			Region = new List<JurisdictionProfile>();
			JurisdictionAssertions = new List<JurisdictionProfile>();
			LearningMethodType = new Enumeration();
			OwnerRoles = new Enumeration();

			InLanguageCodeList = new List<TextValueProfile>();
		}

		public string Name { get; set; }
		public string FriendlyName { get; set; }
		public string SubjectWebpage { get; set; }
		public List<TextValueProfile> Auto_SubjectWebpage { get { return string.IsNullOrWhiteSpace( SubjectWebpage ) ? new List<TextValueProfile>() : new List<TextValueProfile>() { new TextValueProfile() { TextValue = SubjectWebpage } }; } }

		//[Obsolete]
		//public string Url
		//{
		//	get { return SubjectWebpage; }
		//	set { SubjectWebpage = value; }
		//}
		public string AvailableOnlineAt { get; set; }
		public List<TextValueProfile> Auto_AvailableOnlineAt { get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( AvailableOnlineAt ) )
				{
					result.Add( new TextValueProfile() { TextValue = AvailableOnlineAt } );
				}
				return result;
			} }
		public List<GeoCoordinates> AvailableAt
		{
			get
			{
				return Addresses.ConvertAll( m => new GeoCoordinates()
				{
					Address = m,
					Latitude = m.Latitude,
					Longitude = m.Longitude,
					Name = m.Name
					//Url = ???
				} ).ToList();
			}
			set
			{
				Addresses = value.ConvertAll( m => new Address()
				{
					GeoCoordinates = m,
					Latitude = m.Latitude,
					Longitude = m.Longitude,
					Name = m.Name
					//??? = m.Url
				} ).ToList();
			}
		} //Alias used for publishing
		public int StatusId { get; set; }
		public string CredentialRegistryId { get; set; }
		public string ctid { get; set; }
		public string CTID { get { return ctid; } set { ctid = value; } } //Alias used for publishing

		public int ManagingOrgId { get; set; }
		public string ManagingOrganization { get; set; }

		/// <summary>
		/// OwningAgentUid
		///  (Nov2016)
		/// </summary>
		public Guid OwningAgentUid { get; set; }
		public Organization OwningOrganization { get; set; }
		public string OrganizationName
		{
			get
			{
				if ( OwningOrganization != null && OwningOrganization.Id > 0 )
					return OwningOrganization.Name;
				else
					return "";
			}
		}
		public int OwningOrganizationId
		{
			get
			{
				if ( OwningOrganization != null && OwningOrganization.Id > 0 )
					return OwningOrganization.Id;
				else
					return 0;
			}
		}

		public Enumeration OwnerRoles { get; set; }
		public List<OrganizationRoleProfile> OwnerOrganizationRoles { get; set; }
		
		//public string CreatedByOrganization { get; set; }
		//public int CreatedByOrganizationId { get; set; }
		//public Organization Provider { get; set; }
		//public int ProviderId { get; set; }
		public Guid ProviderUid { get; set; }
		//public string IdentificationCode { get; set; }
		/// <summary>
		/// CodedNotation replaces IdentificationCode
		/// </summary>
		public string CodedNotation { get; set; }
		public List<TextValueProfile> Auto_CodedNotation { get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( CodedNotation ) )
				{
					result.Add( new TextValueProfile() { TextValue = CodedNotation } );
				}
				return result;
			} }
		public int InLanguageId { get; set; }
		public string InLanguage { get; set; }
		public string InLanguageCode { get; set; }
		public List<TextValueProfile> InLanguageCodeList { get; set; }
		public List<TextValueProfile> Auto_InLanguageCode
		{
			get
			{
				var result = new List<TextValueProfile>().Concat( InLanguageCodeList ).ToList();
				if ( !string.IsNullOrWhiteSpace( InLanguageCode ) )
				{
					result.Add( new TextValueProfile()
					{
						CodeId = InLanguageId,
						TextTitle = InLanguage,
						TextValue = InLanguageCode
					} );
				}
				return result;
			}
		}

		public string CreditHourType { get; set; }
		public decimal CreditHourValue { get; set; }
		public Enumeration CreditUnitType { get; set; } //Used for publishing
		public int CreditUnitTypeId { get; set; }
		public string CreditUnitTypeDescription { get; set; }
		public decimal CreditUnitValue { get; set; }


		//public List<TextValueProfile> LearningResourceUrls { get; set; }

		public List<DurationProfile> EstimatedDuration { get; set; }

		public Enumeration DeliveryType { get; set; }
		public string DeliveryTypeDescription { get; set; }
		public string VerificationMethodDescription { get; set; }

		public Enumeration InstructionalProgramCategory { get; set; }
		public List<TextValueProfile> OtherInstructionalProgramCategory { get; set; }
		public Enumeration InstructionalProgramType
		{
			get
			{
				return new Enumeration()
				{
					Items = new List<EnumeratedItem>()
					.Concat( ( InstructionalProgramCategory ?? new Enumeration() ).Items )
					.Concat( ( OtherInstructionalProgramCategory ?? new List<TextValueProfile>() ).ConvertAll( m => new EnumeratedItem() { Name = m.TextTitle, Description = m.TextValue } ) ).ToList()
				};
			}
			set
			{
				InstructionalProgramCategory = value;
			}
		} //Used for publishing

		public List<LearningOpportunityProfile> HasPart { get; set; }
		public List<LearningOpportunityProfile> IsPartOf { get; set; }

		public List<OrganizationRoleProfile> OrganizationRole { get; set; }

		public List<TextValueProfile> Keyword { get; set; }
		public List<TextValueProfile> Subject { get; set; }
		public List<CredentialAlignmentObject> Auto_Subject { get { return Subject.ConvertAll( m => new CredentialAlignmentObject() { TargetName = m.TextValue } ).ToList(); } }
		public List<string> WhereReferenced { get; set; }
		public List<Address> Addresses { get; set; }
		public string AvailabilityListing { get; set; }
		public List<TextValueProfile> Auto_AvailabilityListing { get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( AvailabilityListing ) )
				{
					result.Add( new TextValueProfile() { TextValue = AvailabilityListing } );
				}
				return result;
			} }

		public List<ConditionProfile> IsPartOfConditionProfile { get; set; }
		public List<Credential> IsPartOfCredential { get; set; }

		public List<CredentialAlignmentObjectProfile> TeachesCompetencies { get; set; }
		public List<CredentialAlignmentObjectProfile> RequiresCompetencies { get; set; }
		//public List<CredentialAlignmentObjectProfile> TargetCompetency { get { return TeachesCompetencies.Concat( RequiresCompetencies ).ToList(); } }


		public List<CredentialAlignmentObjectFrameworkProfile> TeachesCompetenciesFrameworks { get; set; }
		public List<CredentialAlignmentObjectFrameworkProfile> RequiresCompetenciesFrameworks { get; set; }
		public List<CredentialAlignmentObjectProfile> TargetCompetencies
		{
			get
			{
				return CredentialAlignmentObjectFrameworkProfile.FlattenAlignmentObjects( TeachesCompetenciesFrameworks.Concat( RequiresCompetenciesFrameworks ).ToList() );
			}
			set
			{
				CredentialAlignmentObjectFrameworkProfile.ExpandAlignmentObjects( value, TeachesCompetenciesFrameworks, "assesses" );
				CredentialAlignmentObjectFrameworkProfile.ExpandAlignmentObjects( value, RequiresCompetenciesFrameworks, "requires" );
			}
		}
		public List<CredentialAlignmentObjectProfile> TargetCompetency {
			get { return TargetCompetencies; }
			set { TargetCompetencies = value; }
		} //Alias used for publishing

		public List<JurisdictionProfile> Region { get; set; }
		public List<JurisdictionProfile> JurisdictionAssertions { get; set; }

		public Enumeration LearningMethodType { get; set; }

		public List<CostProfile> EstimatedCost { get; set; }
		public List<CostProfileMerged> EstimatedCost_Merged { get { return CostProfileMerged.FlattenCosts( EstimatedCost ); } } //Used for publishing

		public List<FinancialAlignmentObject> FinancialAssistance { get; set; }
		
		public List<CostManifest> CommonCosts { get; set; }
		public List<ConditionManifest> CommonConditions { get; set; }

		#region CONDITION PROFILES
		public List<ConditionProfile> Requires { get; set; }
		public List<ConditionProfile> Recommends { get; set; }
		public List<ConditionProfile> PreparationFrom { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( LearningOppConnections ).PreparationFrom; } }
		public List<ConditionProfile> AdvancedStandingFrom { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( LearningOppConnections ).AdvancedStandingFrom; } }
		public List<ConditionProfile> IsRequiredFor { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( LearningOppConnections ).IsRequiredFor; } }
		public List<ConditionProfile> IsRecommendedFor { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( LearningOppConnections ).IsRecommendedFor; } }
		public List<ConditionProfile> IsAdvancedStandingFor { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( LearningOppConnections ).IsAdvancedStandingFor; } }
		public List<ConditionProfile> IsPreparationFor { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( LearningOppConnections ).IsPreparationFor; } }

		/// <summary>
		/// The resource being referenced must be pursued concurrently with the resource being described.
		/// </summary>
		public List<ConditionProfile> Corequisite { get; set; }

		/// <summary>
		/// The prerequisites for entry into the resource being described.
		/// Comment:
		/// Such requirements might include transcripts, previous experience, lower-level learning opportunities, etc.
		/// </summary>
		public List<ConditionProfile> EntryCondition { get; set; }

		public List<ConditionProfile> LearningOppConnections { get; set; }

		#endregion
	}
	//

}
