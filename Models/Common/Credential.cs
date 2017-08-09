using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Models.ProfileModels;

namespace Models.Common
{
	public class Credential : BaseObject
	{
		public Credential()
		{
			//defaults to true
			IsDescriptionRequired = true;

			Addresses = new List<Address>();
			
			//CredentialAgentRelationships = new List<CredentialAgentRelationship>();
			AudienceLevelType = new Enumeration();
			CredentialStatusType = new Enumeration();
			CredentialType = new Enumeration();
			//Earnings = new List<EarningsProfile>();
			EmbeddedCredentials = new List<Credential>();
			//EmploymentOutcome = new List<EmploymentOutcomeProfile>();
			//Holders = new List<HoldersProfile>();
			EstimatedCosts = new List<CostProfile>();
			FinancialAssistance = new List<FinancialAlignmentObject>();
			EstimatedDuration = new List<DurationProfile>();
			HasPartIds = new List<int>();
			
			Id = 0;
			Industry = new Enumeration();
			IsPartOf = new List<Credential>();
			IsPartOfIds = new List<int>();

			//AdvancedStandingFor = new List<ConditionProfile>();
			//AdvancedStandingFrom = new List<ConditionProfile>();
			//IsPreparationFor = new List<ConditionProfile>();
			//IsRecommendedFor = new List<ConditionProfile>();
			//IsRequiredFor = new List<ConditionProfile>();
			//PreparationFrom = new List<ConditionProfile>();
			//Renewal = new List<ConditionProfile>();

			Jurisdiction = new List<JurisdictionProfile>();
			Region = new List<JurisdictionProfile>();
			JurisdictionAssertions = new List<JurisdictionProfile>();
			Keyword = new List<TextValueProfile>();
			MilitaryOccupation = new Enumeration();
			Name = "";
			Occupation = new Enumeration();
			OrganizationRole = new List<OrganizationRoleProfile>();
			OfferedByOrganizationRole = new List<OrganizationRoleProfile>();
			OfferedByOrganization = new List<Organization>();

			//Purpose = new Enumeration();
			QualityAssuranceAction = new List<QualityAssuranceActionProfile>();
			CommonCosts = new List<CostManifest>(); 
			CommonConditions = new List<ConditionManifest>();

			AllConditions = new List<ConditionProfile>();
			CredentialConnections = new List<ConditionProfile>();
			//AssessmentConnections = new List<ConditionProfile>();
			//LearningOppConnections = new List<ConditionProfile>();
			CommonConditions = new List<ConditionManifest>();

			Recommends = new List<ConditionProfile>();
			Requires = new List<ConditionProfile>();
			Renewal = new List<ConditionProfile>();
			Corequisite = new List<ConditionProfile>();

			RequiresCompetencies = new List<CredentialAlignmentObjectProfile>();
			Revocation = new List<RevocationProfile>();
			StatusId = 1;
			Subject = new List<TextValueProfile>();
			DegreeConcentration = new List<TextValueProfile>();
			DegreeMajor = new List<TextValueProfile>();
			DegreeMinor = new List<TextValueProfile>();

			OwningOrganization = new Organization();
			OwnerRoles = new Enumeration();
			OwnerOrganizationRoles = new List<OrganizationRoleProfile>();
			VerificationServiceProfiles = new List<VerificationServiceProfile>();

			//TargetCredential = new List<Credential>();
			TargetAssessment = new List<AssessmentProfile>();
			TargetLearningOpportunity = new List<LearningOpportunityProfile>();
			AssessmentEstimatedCosts = new List<CostProfile>();
			LearningOpportunityEstimatedCosts = new List<CostProfile>();

			OtherIndustries = new List<ProfileModels.TextValueProfile>();
			OtherOccupations = new List<TextValueProfile>();

			CredentialProcess = new List<ProcessProfile>();
			AdministrationProcess = new List<ProcessProfile>();
			DevelopmentProcess = new List<ProcessProfile>();
			MaintenanceProcess = new List<ProcessProfile>();
            ComplaintProcess = new List<ProcessProfile>();
            AppealProcess = new List<ProcessProfile>();
            ReviewProcess = new List<ProcessProfile>();
            RevocationProcess = new List<ProcessProfile>();

            InLanguageCodeList = new List<TextValueProfile>();
		}
		/// <summary>
		/// Credential name
		/// </summary>
		[Required]
		[Display( Name = "Credential Name" )]
		public string Name { get; set; }
		/// <summary>
		/// OwningAgentUid
		///  (Nov2016)
		/// </summary>
		 
		public string OwnerNameAndCredentialName {  get
			{
				try
				{
					return Name + " (" + OwningOrganization.Name + ")";
				}
				catch
				{
					return Name;
				}
			} }
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

		public Guid CopyrightHolder { get; set; }
		public Organization CopyrightHolderOrganization { get; set; }


		/// <summary>
		/// CredentialOrganizationTypeId (Nov2016)
		/// </summary>
		//public int CredentialOrganizationTypeId { get; set; }
		//public Enumeration CredentialOrganizationTypeId { get; set; }
		/// <summary>
		/// EarningCredentialPrimaryMethodId (Nov2016)
		/// </summary>
		public int EarningCredentialPrimaryMethodId { get; set; }
		//public Enumeration EarningCredentialPrimaryMethodId { get; set; }

		public bool FeatureLearningOpportunities { get; set; }
		public bool FeatureAssessments { get; set; }

		/// <summary>
		/// IsDescriptionRequired - defaults to true. Can be set to false where a process to quickly create a credential doesn't include a description
		/// </summary>
		public bool IsDescriptionRequired { get; set; }
		public string AlternateName { get; set; }
		public List<TextValueProfile> Auto_AlternateName { get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( AlternateName ) )
				{
					result.Add( new TextValueProfile() { TextValue = AlternateName } );
				}
				return result;
			} }
		public string Description { get; set; }
		public string VersionIdentifier { get; set; }
		//public string VersionIdentifier { get { return Version; } set { Version = value; } } //Alias used for publishing
		public List<IdentifierValue> Auto_VersionIdentifier
		{
			get
			{
				var result = new List<IdentifierValue>();
				if ( !string.IsNullOrWhiteSpace( VersionIdentifier ) )
				{
					result.Add( new IdentifierValue()
					{
						IdentifierValueCode = VersionIdentifier
					} );
				}
				return result;
			}
		}
		public int StatusId { get; set; }
		public string ctid { get; set; }
		public string CTID { get { return ctid; } set { ctid = value; } }
		/// <summary>
		/// Envelope Idenfier from the Credential Registry
		/// </summary>
		public string CredentialRegistryId { get; set; }
		
		public string LatestVersionUrl { get; set; }
		public string LatestVersion { get { return LatestVersionUrl; } set { LatestVersionUrl = value; } } //Alias used for publishing
		public List<TextValueProfile> Auto_LatestVersion
		{
			get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( LatestVersion ) )
				{
					result.Add( new TextValueProfile() { TextValue = LatestVersion } );
				}
				return result;
			}
		}
		public string PreviousVersion { get; set; }
		public List<TextValueProfile> Auto_PreviousVersion
		{
			get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( PreviousVersion ) )
				{
					result.Add( new TextValueProfile() { TextValue = PreviousVersion } );
				}
				return result;
			}
		}

		[ Obsolete]
		public string ReplacesVersionUrl { 
			get {return PreviousVersion;}
			set { PreviousVersion = value; } 
		}

		/// <summary>
		/// 17-03-06 Apparantly can have both Url and subject webpage
		/// </summary>
		//public string Url { get; set; }

		/// <summary>
		/// rename Url to SubjectWebpage (Nov2016)
		/// </summary>
		public string SubjectWebpage { get; set; }
		public List<TextValueProfile> Auto_SubjectWebpage { get { return string.IsNullOrWhiteSpace( SubjectWebpage ) ? null : new List<TextValueProfile>() { new TextValueProfile() { TextValue = SubjectWebpage } }; } }


		public string AvailableOnlineAt { get; set; }
		public List<TextValueProfile> Auto_AvailableOnlineAt {
			get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( AvailableOnlineAt ) )
				{
					result.Add( new TextValueProfile() { TextValue = AvailableOnlineAt } );
				}
				return result;
			}
			set
			{
				AvailableOnlineAt = value.FirstOrDefault().TextValue;
			}
		}
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
		public string ImageUrl { get; set; } //image URL
		public List<TextValueProfile> Auto_ImageUrl
		{
			get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( ImageUrl ) )
				{
					result.Add( new TextValueProfile() { TextValue = ImageUrl } );
				}
				return result;
			}
		}
		public List<Address> Addresses { get; set; }
		
		/// <summary>
		/// Region  (Nov2016)
		/// Soon(TM) to be replaced by AvailableAT??
		/// </summary>
		//public List<GeoCoordinates> AvailableAT { get; set; }
		//public List<GeoCoordinates> Region
		//{
		//	get { return AvailableAT; }
		//	set { this.AvailableAT = value; }
		//}
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
				Addresses = new List<Address>();
				foreach( var geo in value )
				{
					var address = geo.Address;
					address.Latitude = geo.Latitude;
					address.Longitude = geo.Longitude;
					Addresses.Add( address );
				}
			}
		} //Alias used for publishing
		public List<JurisdictionProfile> Jurisdiction { get; set; }
		public List<JurisdictionProfile> Region { get; set; }
		public List<JurisdictionProfile> JurisdictionAssertions { get; set; }

		public List<DurationProfile> EstimatedDuration { get; set; }
		/// <summary>
		/// EstimatedTimeToEarn - alias for EstimatedDuration
		/// </summary>
		/// <remarks>OBSOLETE</remarks>
		//public List<DurationProfile> EstimatedTimeToEarn
		//{
		//	get { return EstimatedDuration; }
		//	set { this.EstimatedDuration = value; }
		//}
		public Enumeration IntendedPurpose { get; set; }
		public Enumeration CredentialType { get; set; }
		[Display( Name = "Credential Type" )]
		[Required]
		public int CredentialTypeId { get; set; }
		public string CredentialTypeDisplay { get; set; }
		public string CredentialTypeSchema{ get; set; }
		public string OwningOrgDisplay { get; set; }
		public string ProfileType {
			get
			{
				EnumeratedItem item = CredentialType.GetFirstItem();
				if ( item != null && item.Id > 0 )
				{
					return item.Name;
				}
				else
					return "";
			}
		}

		public Enumeration AudienceLevelType { get; set; }

		public Enumeration CredentialStatusType { get; set; }
		public List<Credential> EmbeddedCredentials { get; set; } //bundled/sub-credentials
		public List<Credential> IsPartOf { get; set; } //pseudo-"parent" credentials that this credential is a part of or included with (could be multiple)
		public List<int> HasPartIds { get; set; }
		public List<int> IsPartOfIds { get; set; }

		/// <summary>
		/// placeholder for all Processes
		/// </summary>
		public List<ProcessProfile> CredentialProcess { get; set; }
		public List<ProcessProfile> AdministrationProcess { get; set; }
		public List<ProcessProfile> DevelopmentProcess { get; set; }
		public List<ProcessProfile> MaintenanceProcess { get; set; }
        public List<ProcessProfile> AppealProcess { get; set; }
        public List<ProcessProfile> ComplaintProcess { get; set; }
        public List<ProcessProfile> RevocationProcess { get; set; }
        public List<ProcessProfile> ReviewProcess { get; set; }

        //public List<EarningsProfile> Earnings { get; set; }
        //public List<EmploymentOutcomeProfile> EmploymentOutcome { get; set; }
        //public List<HoldersProfile> Holders { get; set; }

        public Enumeration Industry { get; set; }
		public Enumeration IndustryType
		{
			get
			{
				return new Enumeration()
				{
					Items = new List<EnumeratedItem>()
					.Concat( Industry.Items )
					.Concat( OtherIndustries.ConvertAll( m => new EnumeratedItem() { Name = m.TextTitle, Description = m.TextValue } ) ).ToList()
				};
			}
			set { Industry = value; }
		} //Used for publishing
		public List<TextValueProfile> OtherIndustries { get; set; }
		public Enumeration Occupation { get; set; }
		public Enumeration OccupationType
		{
			get
			{
				return new Enumeration()
				{
					Items = new List<EnumeratedItem>()
					.Concat( Occupation.Items )
					.Concat( OtherOccupations.ConvertAll( m => new EnumeratedItem() { Name = m.TextTitle, Description = m.TextValue } ) ).ToList()
				};
			}
			set { Occupation = value; }
		} //Used for publishing
		public List<TextValueProfile> OtherOccupations { get; set; }
		public Enumeration MilitaryOccupation { get; set; }

		public List<OrganizationRoleProfile> OrganizationRole { get; set; }
		public List<OrganizationRoleProfile> OfferedByOrganizationRole { get; set; }
		public List<Organization> OfferedByOrganization { get; set; }

		public List<QualityAssuranceActionProfile> QualityAssuranceAction { get; set; }

		#region Condition Profiles

		//actually, get all conditions and then split out to other types
		public List<ConditionProfile> AllConditions { get; set; }
		public List<ConditionProfile> CredentialConnections{ get; set; }
		//public List<ConditionProfile> AssessmentConnections { get; set; }
		//public List<ConditionProfile> LearningOppConnections { get; set; }

		public List<CostManifest> CommonCosts { get; set; }
		public List<ConditionManifest> CommonConditions { get; set; }

		public List<ConditionProfile> Requires { get; set; }
		public List<ConditionProfile> Recommends { get; set; }
		public List<ConditionProfile> PreparationFrom { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( CredentialConnections ).PreparationFrom; } }
		public List<ConditionProfile> AdvancedStandingFrom { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( CredentialConnections ).AdvancedStandingFrom; } }
		public List<ConditionProfile> IsRequiredFor { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( CredentialConnections ).IsRequiredFor; } }
		public List<ConditionProfile> IsRecommendedFor { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( CredentialConnections ).IsRecommendedFor; } }
		public List<ConditionProfile> IsAdvancedStandingFor { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( CredentialConnections ).IsAdvancedStandingFor; } }
		public List<ConditionProfile> IsPreparationFor { get { return ConditionManifestExpanded.DisambiguateConditionProfiles( CredentialConnections ).IsPreparationFor; } }
		public List<ConditionProfile> Renewal { get; set; }

		/// <summary>
		/// The resource being referenced must be pursued concurrently with the resource being described.
		/// </summary>
		public List<ConditionProfile> Corequisite { get; set; }
		//public List<ConditionProfile> AdvancedStandingFrom { get; set; }
		//public List<ConditionProfile> PreparationFrom { get; set; }

		////next steps
		//public List<ConditionProfile> IsRequiredFor { get; set; }
		//public List<ConditionProfile> IsRecommendedFor { get; set; }
		//public List<ConditionProfile> AdvancedStandingFor { get; set; }
		//public List<ConditionProfile> IsPreparationFor { get; set; }
		#endregion

		public List<RevocationProfile> Revocation { get; set; }
		
		//public int OwnerOrganizationId { get; set; }
		public int ManagingOrgId { get; set; }
		public string CredentialId { get; set; }
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

		public List<TextValueProfile> Keyword { get; set; }
		public List<TextValueProfile> Subject { get; set; }
		public List<CredentialAlignmentObject> Auto_Subject { get { return Subject.ConvertAll( m => new CredentialAlignmentObject() { TargetName = m.TextValue } ).ToList(); } }
		//public List<string> SubjectsList { get; set; }


		public List<TextValueProfile> DegreeConcentration { get; set; }
		public List<TextValueProfile> DegreeMajor{ get; set; }
		public List<TextValueProfile> DegreeMinor{ get; set; }

		/// <summary>
		/// Only publish the related property, if the credential's type is a degree or a subclass of degree
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		private List<CredentialAlignmentObject> IsValidDegreeType( List<TextValueProfile> data )
		{
			try
			{
				var validDegreeTypes = new List<string>() { "ceterms:AssociateDegree", "ceterms:BachelorDegree", "ceterms:Degree", "ceterms:DoctoralDegree", "ceterms:MasterDegree" };
				if ( CredentialType.Items.Where( m => validDegreeTypes.Contains( m.SchemaName ) ).Count() > 0 )
				{
					return data.ConvertAll( m => new CredentialAlignmentObject()
					{
						TargetNodeName = m.TextValue
					} ).ToList();
				}
				return new List<CredentialAlignmentObject>();
			}
			catch
			{
				return new List<CredentialAlignmentObject>();
			}
		}


		/// <summary>
		/// only publish for a degree type
		/// </summary>
		public List<CredentialAlignmentObject> Auto_DegreeConcentration { get { return IsValidDegreeType( DegreeConcentration ); } }

		/// <summary>
		/// only publish for a degree type
		/// </summary>
		public List<CredentialAlignmentObject> Auto_DegreeMajor { get { return IsValidDegreeType( DegreeMajor ); } }

		/// <summary>
		/// only publish for a degree type
		/// </summary>
		public List<CredentialAlignmentObject> Auto_DegreeMinor { get { return IsValidDegreeType( DegreeMinor ); } }

		public List<CostProfile> EstimatedCosts { get; set; }
		public List<CostProfile> AssessmentEstimatedCosts { get; set; }
		public List<CostProfile> LearningOpportunityEstimatedCosts { get; set; }
		/// <summary>
		/// Alias used for publishing
		/// </summary>
		public List<CostProfile> EstimatedCost {
			get { return EstimatedCosts; }
			set { EstimatedCosts = value; }
		}

		/// <summary>
		/// Used for publishing
		/// </summary>
		public List<CostProfileMerged> EstimatedCost_Merged {
			get { return CostProfileMerged.FlattenCosts( EstimatedCosts ); }
		} //

		public List<FinancialAlignmentObject> FinancialAssistance { get; set; }

		public List<CredentialAlignmentObjectProfile> RequiresCompetencies { get; set; }


		/// <summary>
		/// Credentials related by a condition profile
		/// - hold, see if needed - may not as the condition types are very specific
		/// </summary>
		//public List<Credential> TargetCredential { get; set; } 
		/// <summary>
		/// Assessments related by a condition profile
		/// </summary>
		public List<AssessmentProfile> TargetAssessment { get; set; }
		/// <summary>
		/// Learning Opportunities related by a condition profile
		/// </summary>
		public List<LearningOpportunityProfile> TargetLearningOpportunity { get; set; }

		/// <summary>
		/// processStandards (Nov2016)
		/// URL
		/// </summary>
		public string ProcessStandards { get; set; }
		/// <summary>
		/// ProcessStandardsDescription (Nov2016)
		/// </summary>
		public string ProcessStandardsDescription { get; set; }

		public List<VerificationServiceProfile> VerificationServiceProfiles { get; set; }

		//public void InitializeConnectionProfiles()
		//{
		//	Requires = new List<ConditionProfile>();
		//	Recommends = new List<ConditionProfile>();
		//	Renewal = new List<ConditionProfile>();
		//	IsRequiredFor = new List<ConditionProfile>();
		//	IsRecommendedFor = new List<ConditionProfile>();
		//}

		public bool HasVerificationType_Badge { get; set; }
	}
}
