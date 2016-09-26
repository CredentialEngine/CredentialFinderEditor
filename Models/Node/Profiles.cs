using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{
	//Object to hold code data that may or may not involve user entry - used only to store input, never to carry code tables!
	[Profile( DBType = typeof( Models.ProfileModels.TextValueProfile ) )]
	public class TextValueProfile : BaseProfile
	{
		[Property( DBName = "TextTitle" )]
		public string CodeOther { get; set; } //User-defined alternative name (used when CodeId/CodeName is equivalent to "other")
		[Property( DBName = "TextValue" )]
		public string Value { get; set; } //Actual value from user

		public int CategoryId { get; set; } //ID of the Category of the TextValueProfile (so the database knows what it belongs to)
		public int CodeId { get; set; } //ID from a drop-down list (e.g., "Dun and Bradstreet DUNS Number")
	}
	//

	//Used with Micro Searches
	public class MicroProfile : BaseProfile
	{
		public MicroProfile()
		{
			Properties = new Dictionary<string, object>();
		}

		public Dictionary<string, object> Properties { get; set; }
		public Dictionary<string, object> Selectors { get; set; }
	}
	//

	//Used to help create and immediately associate a profile with a microsearch
	[Profile( DBType = typeof( Models.Node.StarterProfile ) )] //hack
	public class StarterProfile : BaseProfile
	{
		[Property( DBName = "Name" )] //hack
		public override string Name { get; set; }
		public string ProfileType { get; set; }
		public string SearchType { get; set; } //hack
		public string Url { get; set; }
	}
	//

	[Profile( DBType = typeof( Models.ProfileModels.DurationProfile ) )]
	public class DurationProfile : BaseProfile
	{
		public DurationProfile()
		{
			ExactDuration = new DurationItem();
			MinimumDuration = new DurationItem();
			MaximumDuration = new DurationItem();
		}

		//These are always handled inline and never separate from the DurationProfile, so they are not ProfileLink objects
		public DurationItem ExactDuration { get; set; }
		public DurationItem MinimumDuration { get; set; }
		public DurationItem MaximumDuration { get; set; }

		public string MinimumDurationISO8601 { get; set; }
		public string MaximumDurationISO8601 { get; set; }
		public string ExactDurationISO8601 { get; set; }

		public bool IsRange { get { return this.MinimumDuration != null && this.MaximumDuration != null && this.MinimumDuration.HasValue && this.MaximumDuration.HasValue; } }

		//Override the usage of description
		[Property( DBName = "Conditions" )]
		new public string Description { get; set; }

	}
	//

	public class DurationItem //Not sure if this needs to inherit from database
	{
		public int Years { get; set; }
		public int Months { get; set; }
		public int Weeks { get; set; }
		public int Days { get; set; }
		public int Hours { get; set; }
		public int Minutes { get; set; }
		public bool HasValue { get { return Years + Months + Weeks + Days + Hours + Minutes > 0; } }

		public string Print()
		{
			var parts = new List<string>();
			if ( Years > 0 ) { parts.Add( Years + " year" + ( Years == 1 ? "" : "s" ) ); }
			if ( Months > 0 ) { parts.Add( Months + " month" + ( Months == 1 ? "" : "s" ) ); }
			if ( Weeks > 0 ) { parts.Add( Weeks + " week" + ( Weeks == 1 ? "" : "s" ) ); }
			if ( Days > 0 ) { parts.Add( Days + " day" + ( Days == 1 ? "" : "s" ) ); }
			if ( Hours > 0 ) { parts.Add( Hours + " hour" + ( Hours == 1 ? "" : "s" ) ); }
			if ( Minutes > 0 ) { parts.Add( Minutes + " minute" + ( Minutes == 1 ? "" : "s" ) ); }

			return string.Join( ", ", parts );
		}
	}
	//

	[Profile( DBType = typeof( Models.Common.JurisdictionProfile ) )]
	public class JurisdictionProfile : BaseProfile
	{
		public bool IsGlobalJurisdiction { get; set; }
		public bool IsOnlineJurisdiction { get; set; }
		[Property( DBName = "MainJurisdiction", SaveAsProfile = true )]
		public ProfileLink MainRegion { get; set; }
		[Property( DBName = "JurisdictionException", SaveAsProfile = true )]
		public List<ProfileLink> RegionException { get; set; }
		[Property( DBName = "ProfileSummary" )]
		public override string Name { get; set; } //Override the annotation on the base profile name
	}
	//

	//May not be used - will have to see
	public class RegionProfile : BaseProfile
	{
		public string ToponymName { get; set; }
		public string Region { get; set; }
		public string Country { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public string GeonamesUrl { get; set; } //URL of a geonames place
		public string TitleFormatted
		{
			get
			{
				string taxName = string.IsNullOrWhiteSpace( this.ToponymName ) ? "" : this.ToponymName;
				if ( !string.IsNullOrWhiteSpace( this.Name ) )
				{
					return this.Name + ( ( taxName.ToLower() == this.Name.ToLower() || taxName == "" ) ? "" : " (" + taxName + ")" );
				}
				else
				{
					return "";
				}
			}
		}
		public string LocationFormatted { get { return string.IsNullOrWhiteSpace( this.Region ) ? this.Country : this.Region + ", " + this.Country; } }
	}
	//

	[Profile( DBType = typeof( Models.ProfileModels.OrganizationRoleProfile ) )]
	public class AgentRoleProfile : BaseProfile //BaseProfile properties are not currently used, but the inheritance makes processing easier
	{
		[Property( DBName = "ActingAgent", DBType = typeof( Models.Common.Organization ), SaveAsProfile = true )]
		public ProfileLink Actor { get; set; }
		//Could be one of many types - requires special handling in the services layer
		public ProfileLink Recipient { get; set; }
		[Property( DBName = "RoleType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> RoleTypeIds { get; set; }
	}
	public class AgentRoleProfile_Recipient : AgentRoleProfile { }
	public class AgentRoleProfile_Actor : AgentRoleProfile { }
	public class OrganizationRole_Recipient : AgentRoleProfile { }
	//

	[Profile( DBType = typeof( Models.ProfileModels.QualityAssuranceActionProfile ) )]
	public class QualityAssuranceActionProfile : BaseProfile
	{
		[Property( DBName = "ActingAgent", DBType = typeof( Models.Common.Organization ), SaveAsProfile = true )]
		public ProfileLink Actor { get; set; }
		//Could be one of many types - requires special handling in the services layer
		public ProfileLink Recipient { get; set; }
		[Property( DBName = "IssuedCredential", DBType = typeof( Models.Common.Credential ), SaveAsProfile = true )]
		public ProfileLink IssuedCredential { get; set; }
		[Property( DBName = "RoleTypeId" )]
		public int QualityAssuranceTypeId { get; set; }
		public string StartDate { get { return this.DateEffective; } set { this.DateEffective = value; } }
		public string EndDate { get; set; }

		//Not used yet
		public ProfileLink RelatedQualityAssuranceAction { get; set; } //Enables a revoke action to apply to an accredit action
		public List<ProfileLink> SecondaryActor { get; set; } //Enables another org to take part in the action
	}
	public class QualityAssuranceActionProfile_Recipient : QualityAssuranceActionProfile { }
	public class QualityAssuranceActionProfile_Actor : QualityAssuranceActionProfile { }
	//

	[Profile( DBType = typeof( Models.ProfileModels.ConditionProfile ) )]
	public class ConditionProfile : BaseProfile
	{
		public ConditionProfile()
		{
			Other = new Dictionary<string, string>();
		}

		//List-based Info
		public Dictionary<string, string> Other { get; set; }

		[Property( DBName = "AssertedByAgentUid", DBType = typeof( Guid ) )]
		public ProfileLink ConditionProvider { get; set; }
		[Property( DBName="ApplicableAudienceType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> AudienceTypeIds { get; set; }
		[Property( DBName = "RequiredCredential" )]
		public List<ProfileLink> Credential { get; set; }
		[Property( DBName = "CredentialType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> CredentialTypeIds { get; set; }
		[Property( DBName = "TargetLearningOpportunity" )]
		public List<ProfileLink> LearningOpportunity { get; set; }
		[Property( DBName = "ResidentOf" )]
		public List<ProfileLink> Residency { get; set; }
		[Property( DBName = "TargetAssessment" )]
		public List<ProfileLink> Assessment { get; set; }
		[Property( DBName = "TargetTask" )]
		public List<ProfileLink> Task { get; set; }

		//[Property( DBName = "TargetMiniCompetency" )]
		//public List<TextValueProfile> MiniCompetency { get; set; } //Not being used currently

		public string Experience { get; set; }
		public int MinimumAge { get; set; }
		public List<ProfileLink> Jurisdiction { get; set; }

		public List<TextValueProfile> ConditionItem { get; set; }
		public List<TextValueProfile> ReferenceUrl { get; set; }
	}
	//

	[Profile( DBType = typeof( Models.ProfileModels.RevocationProfile ) )]
	public class RevocationProfile : BaseProfile
	{
		public RevocationProfile()
		{
			Other = new Dictionary<string, string>();
		}

		//List-based Info
		public Dictionary<string, string> Other { get; set; }

		[Property( DBName = "RemovalDateEffective" )]
		public string StartDate { get { return this.DateEffective; } set { this.DateEffective = value; } }
		[Property( DBName = "RenewalDateEffective" )]
		public string EndDate { get; set; }
		[Property( DBName = "RevocationCriteriaType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> RevocationCriteriaTypeIds { get; set; }
		[Property( DBName = "RevocationResourceUrl" )]
		public List<TextValueProfile> ReferenceUrl { get; set; }
		public List<ProfileLink> Jurisdiction { get; set; }
	}
	//

	[Profile( DBType = typeof( Models.ProfileModels.TaskProfile ) )]
	public class TaskProfile : BaseProfile
	{
		[Property( DBName = "AffiliatedAgentUid", DBType = typeof( Guid ) )]
		public ProfileLink TaskProvider { get; set; }
		[Property( DBName = "EstimatedCost", DBType = typeof( Models.ProfileModels.CostProfile ) )]
		public List<ProfileLink> Cost { get; set; }
		[Property( DBName = "EstimatedDuration", DBType = typeof( Models.ProfileModels.DurationProfile ) )]
		public List<ProfileLink> Duration { get; set; }
		public List<ProfileLink> Jurisdiction { get; set; }
	}
	//

	[Profile( DBType = typeof( Models.ProfileModels.CostProfile ) )]
	public class CostProfile : BaseProfile
	{
		[Property( DBName = "DateEffective" )]
		public string StartDate { get { return this.DateEffective; } set { this.DateEffective = value; } }

		[Property( DBName = "ExpirationDate" )]
		public string EndDate { get; set; }

		//[Property( DBName = "ReferenceUrl" )]
		public List<TextValueProfile> ReferenceUrl { get; set; }

		[Property( DBName = "Items", DBType = typeof( Models.ProfileModels.CostProfileItem ) )]
		public List<ProfileLink> CostItem { get; set; }

		public string Currency { get; set; } //Not used anymore
		//[Property( DBName = "CurrencyType", DBType = typeof( Models.Common.Enumeration ) )]
		public int CurrencyTypeId { get; set; } 
		public List<ProfileLink> Jurisdiction { get; set; }
	}
	//

	[Profile( DBType = typeof( Models.ProfileModels.CostProfileItem ) )]
	public class CostItemProfile : BaseProfile
	{
		public int CostTypeId { get; set; }
		[Property( DBName = "ResidencyType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> ResidencyTypeIds { get; set; }
		[Property( DBName = "EnrollmentType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> EnrollmentTypeIds { get; set; }
		[Property( DBName = "ApplicableAudienceType", DBType = typeof( Models.Common.Enumeration ) )]
		public List<int> AudienceTypeIds { get; set; }
		[Property( DBName = "PaymentPattern" )]
		public string Payments { get; set; }
		[Property( DBName = "PayeeUid", DBType = typeof( Guid ) )]
		public ProfileLink Recipient { get; set; }

		public decimal Price { get; set; }
	}
	//

	[Profile( DBType = typeof( Models.Common.CredentialAlignmentObjectProfile ) )]
	public class CredentialAlignmentObjectProfile : BaseProfile
	{
		[Property( DBName = "Name" )]
		public override string Name { get; set; }
		public string EducationalFramework { get; set; }
		public string CodedNotation { get; set; }
		public string TargetName { get; set; } //Name of the target, not the profile. Not currently used.
		public string TargetDescription { get; set; } //Description of the target, not the profile. Not currently used.
		public string TargetUrl { get; set; }
	}
	//

	[Profile( DBType = typeof( Models.Common.Address ) )]
	public class AddressProfile : BaseProfile
	{
		[Property( DBName = "Name" )]
		public override string Name { get; set; }
		public bool IsMainAddress { get; set; }
		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string City { get; set; }

		[Property( DBName = "AddressRegion" )]
		public string Region { get; set; } //State, Province, etc.
		public int CountryId { get; set; }
		public string Country { get; set; }
		public string PostalCode { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }

		public string DisplayAddress( string separator = ", " )
		{
			var parts = new List<string>() { Address1 ?? "", Address2 ?? "", City ?? "", Region ?? "", PostalCode ?? "", Country ?? "" };
			var joined = string.Join( separator, parts );
			if ( !string.IsNullOrWhiteSpace( PostalCode ) )
			{
				joined = joined.Replace( PostalCode + separator, PostalCode + " " );
			}
			return joined;
		}

		public bool HasAddress()
		{
			return !( string.IsNullOrWhiteSpace( Address1 )
				&& string.IsNullOrWhiteSpace( Address2 )
				&& string.IsNullOrWhiteSpace( City )
				&& string.IsNullOrWhiteSpace( Region )
				&& string.IsNullOrWhiteSpace( PostalCode )
			);
		}

	}
	//

	[Profile( DBType = typeof( Models.ProfileModels.ProcessProfile ) )]
	public class ProcessProfile : BaseProfile
	{
		public ProfileLink RolePlayer { get; set; }
		public List<int> ProcessTypeIds { get; set; }
		public List<int> ExternalStakeholderTypeIds { get; set; }
		public List<int> ProcessMethodTypeIds { get; set; }
		public List<ProfileLink> MoreInformationUrl { get; set; }
		public List<ProfileLink> Context { get; set; }
		public List<ProfileLink> Frequency { get; set; } //Duration
		public List<ProfileLink> Jurisdiction { get; set; }
	}
	//

	[Profile( DBType = typeof( Models.ProfileModels.AuthenticationProfile ) )]
	public class VerificationServiceProfile : BaseProfile
	{
		[Property( DBName = "EstimatedCost", DBType = typeof( CostProfile ) )]
		public List<ProfileLink> Cost { get; set; }
		[Property( DBName = "RelevantCredential", DBType = typeof( Models.Common.Credential ), SaveAsProfile = true )]
		public ProfileLink Credential { get; set; }

		[Property( DBName = "Provider", DBType = typeof( Models.Common.Organization ) )]
		public List<ProfileLink> Verifier { get; set; } //Agent

		public List<ProfileLink> Jurisdiction { get; set; }
		public bool HolderMustAuthorize { get; set; }
	}
	//

	[Profile( DBType = typeof( Models.ProfileModels.EarningsProfile ) )]
	public class EarningsProfile : BaseProfile
	{
		public int LowEarnings { get; set; }
		public int MedianEarnings { get; set; }
		public int HighEarnings { get; set; }
		public int CurrencyTypeId { get; set; }
		public List<ProfileLink> SourceUrl { get; set; }
		public List<ProfileLink> Jurisdiction { get; set; }
	}
	//

	[Profile( DBType = typeof( Models.ProfileModels.EmploymentOutcomeProfile ) )]
	public class EmploymentOutcomeProfile : BaseProfile
	{
		public int JobsObtained { get; set; }
		public List<ProfileLink> Jurisdiction { get; set; }
	}
	//

	[Profile( DBType = typeof( Models.ProfileModels.HoldersProfile ) )]
	public class HoldersProfile : BaseProfile
	{
		public string DemographicInformation { get; set; }
		public List<ProfileLink> Jurisdiction { get; set; }
		public List<ProfileLink> SourceUrl { get; set; }
		public int NumberAwarded { get; set; }
	}
	//

	//Attribute to make conversion easier
	[AttributeUsage(AttributeTargets.Property)]
	public class Property : Attribute
	{
		public Property()
		{
			Type = typeof( ProfileLink );
			DBType = typeof( string );
		}
		public Type Type { get; set; }
		public Type DBType { get; set; }
		public string DBName { get; set; }
		public bool SaveAsProfile { get; set; } //Indicates whether or not to initialize a new profile during saving - used with micro searches that do not do direct saves
		public string SchemaName { get; set; }
		public string LoadMethod { get; set; }
		public string SaveMethod { get; set; }
	}
	//

	//Attribute to make saving stuff easier
	[AttributeUsage(AttributeTargets.Class)]
	public class Profile : Attribute
	{
		public Profile()
		{
			DBType = typeof( string );
		}
		public Type DBType { get; set; }
	}
	//

}
