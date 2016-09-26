using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace Models.JsonV2
{

	public class JsonLDObject
	{
		public JsonLDObject()
		{
			Type = "";
			Properties = new List<PropertyData>();
			CommonProperties = new CommonProperties();
		}
		public string Type { get; set; }
		public List<PropertyData> Properties { get; set; }
		public CommonProperties CommonProperties { get; set; }
	}

	public class JsonLDDocument : JsonLDObject
	{
		public JsonLDDocument()
		{
			Context = new Dictionary<string, object>();
		}
		public Dictionary<string, object> Context { get; set; }
	}

	public class CommonProperties
	{
		public PropertyData Name = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "schema:name", Source = "Name", Label = "Name" };
		public PropertyData ProfileName = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "schema:name", Source = "ProfileName", Label = "Name" };
		public PropertyData Description = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "schema:description", Source = "Description", Label = "Description" };
		public PropertyData DateEffective = new PropertyData() { Type = PropertyType.DATE, SchemaName = "ctdl:dateEffective", Source = "DateEffective", Label = "Date Effective" };
		public PropertyData Url = new PropertyData() { Type = PropertyType.URL, SchemaName = "schema:url", Source = "Url", Label = "Url" };
		public PropertyData Jurisdiction = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:jurisdiction", Source = "Jurisdiction", Label = "Jurisdiction", ProfileType = typeof( JurisdictionProfile ) };
		public PropertyData Keyword = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:keyword", Source = "Keywords", Label = "Keywords", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData Subject = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:subject", Source = "Subjects", Label = "Subjects", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData EstimatedCosts = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:estimatedCost", Source = "EstimatedCosts", Label = "Estimated Costs", ProfileType = typeof( CostProfile ), SourceType = SourceType.FROM_METHOD, InnerSource = "FlattenCosts" };

		//Roles
		public PropertyData AccreditedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:accreditedBy", Source = "OrganizationRole", Label = "Accredited By" };
		public PropertyData ApprovedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:approvedBy", Source = "OrganizationRole", Label = "Approved By" };
		public PropertyData AssessedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:assessedBy", Source = "OrganizationRole", Label = "Assessed By" };
		public PropertyData ConferredBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:conferredBy", Source = "OrganizationRole", Label = "Conferred By" };
		public PropertyData EndorsedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:endorsedBy", Source = "OrganizationRole", Label = "Endorsed By" };
		public PropertyData MonitoredBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:monitoredBy", Source = "OrganizationRole", Label = "Monitored By" };
		public PropertyData OfferedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:offeredBy", Source = "OrganizationRole", Label = "Offered By" };
		public PropertyData Owner = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:owner", Source = "OrganizationRole", Label = "Owner" };
		public PropertyData RecognizedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:recognizedBy", Source = "OrganizationRole", Label = "Recognized By" };
		public PropertyData RegulatedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:regulatedBy", Source = "OrganizationRole", Label = "Regulated By" };
		public PropertyData RenewalBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:renewalsBy", Source = "OrganizationRole", Label = "Renewals By" };
		public PropertyData RevocationBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:revocationBy", Source = "OrganizationRole", Label = "Revocation By" };
		public PropertyData UpdatedVersionBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:updatedVersionBy", Source = "OrganizationRole", Label = "Updated Version By" };
		public PropertyData ValidatedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:validatedBy", Source = "OrganizationRole", Label = "Validated By" };
		public PropertyData VerifiedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:verifiedBy", Source = "OrganizationRole", Label = "Verified By" };
		public PropertyData Creator = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "schema:creator", Source = "OrganizationRole", Label = "Creator" };
		public PropertyData Contributor = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "schema:contributor", Source = "OrganizationRole", Label = "Contributor" };
	}
	//


	public class Credential : JsonLDDocument
	{
		public Credential()
		{
			Type = "ctdl:Credential";
			Context = new Dictionary<string, object>()
			{
				{ "schema", "http://schema.org/" },
				{ "dc", "http://purl.org/dc/elements/1.1/" },
				{ "dct", "http://dublincore.org/terms/" },
				{ "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" },
				{ "rdfs", "http://www.w3.org/2000/01/rdf-schema#" },
				{ "ctdl", "[CTI Namespace Not Determined Yet]" }
			};

			Properties = new List<PropertyData>() {
				CommonProperties.Name,
				CommonProperties.Description,
				CommonProperties.DateEffective,
				CommonProperties.Url,
				CommonProperties.Jurisdiction,
				CommonProperties.AccreditedBy,
				CommonProperties.ApprovedBy,
				CommonProperties.AssessedBy,
				CommonProperties.ConferredBy,
				CommonProperties.EndorsedBy,
				CommonProperties.MonitoredBy,
				CommonProperties.OfferedBy,
				CommonProperties.Owner,
				CommonProperties.RecognizedBy,
				CommonProperties.RegulatedBy,
				CommonProperties.RenewalBy,
				CommonProperties.RevocationBy,
				CommonProperties.UpdatedVersionBy,
				CommonProperties.ValidatedBy,
				CommonProperties.VerifiedBy,
				CommonProperties.Creator,
				CommonProperties.Contributor,
				CommonProperties.Subject,
				CommonProperties.Keyword,
				CommonProperties.EstimatedCosts,
				new PropertyData() { Type = PropertyType.PARENT_TYPE_OVERRIDE, SchemaName = "ctdl:", Source = "CredentialType", Label = "Credential Type", SourceType = SourceType.FROM_ENUMERATION },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:alternateName", Source = "AlternateName", Label = "Alternate Name" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:versionIdentifier", Source = "Version", Label = "Version" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:ctid", Source = "ctid", Label = "CTID" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:latestVersion", Source = "LatestVersionUrl", Label = "Latest Version" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:previousVersion", Source = "ReplacesVersionUrl", Label = "Previous Version" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "schema:hasPart", Source = "EmbeddedCredentials", Label = "Embedded Credential", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "schema:image", Source = "ImageUrl", Label = "Image URL" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:availableOnlineAt", Source = "AvailableOnlineAt", Label = "Available Online At" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:typicalAudienceLevelType", Source = "CredentialLevel", Label = "Credential Level" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:purposeType", Source = "Purpose", Label = "Purpose" },
				new PropertyData() { Type = PropertyType.ENUMERATION_EXTERNAL, SchemaName = "ctdl:industryType", Source = "Industry", Label = "Industry Category" },
				new PropertyData() { Type = PropertyType.ENUMERATION_EXTERNAL, SchemaName = "ctdl:occupationType", Source = "Occupation", Label = "Occupation Category" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:estimatedDuration", Source = "EstimatedTimeToEarn", Label = "Estimated Time to Earn", ProfileType = typeof( DurationProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:recommendedFor", Source = "IsRecommendedFor", Label = "Recommended For", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:requiredFor", Source = "IsRequiredFor", Label = "Required For", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:recommends", Source = "Recommends", Label = "Recommends", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:renewal", Source = "Renewal", Label = "Renewal", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:requires", Source = "Requires", Label = "Requires", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:revocation", Source = "Revocation", Label = "Revocation", ProfileType = typeof( RevocationProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:relatedAction", Source = "QualityAssuranceAction", Label = "Related Action", ProfileType = typeof( QualityAssuranceActionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:availableAt", Source = "Addresses", Label = "Available At", ProfileType = typeof( GeoCoordinates ), SourceType = SourceType.FROM_METHOD, InnerSource = "WrapAddress" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:industryTypeOther", Source = "OtherIndustries", Label = "Other Industries", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:occupationTypeOther", Source = "OtherOccupations", Label = "Other Occupations", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },

				//Not Implemented Yet
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:codedNotation", Source = "MISSING", Label = "Coded Notation" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:credentialStatusType", Source = "MISSING", Label = "Credential Status" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:developmentProcess", Source = "MISSING", Label = "Development Process" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:employmentOutcome", Source = "MISSING", Label = "Employment Outcome Statistics" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:holders", Source = "MISSING", Label = "Holders Statistics" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:maintenanceProcess", Source = "MISSING", Label = "Maintenance Process" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:earnings", Source = "MISSING", Label = "Earnings Statistics" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:selectionProcess", Source = "MISSING", Label = "Selection Process" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:validationProcess", Source = "MISSING", Label = "Validation Process" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:contributor", Source = "MISSING", Label = "Contributor", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:copyrightHolder", Source = "MISSING", Label = "Copyright Holder", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:sameAs", Source = "MISSING", Label = "Same As" }
			};
		}
	}
	//

	public class Organization : JsonLDDocument
	{
		public Organization()
		{
			Type = "ctdl:Organization";
			Context = new Dictionary<string, object>()
			{
				{ "schema", "http://schema.org/" },
				{ "dc", "http://purl.org/dc/elements/1.1/" },
				{ "dct", "http://dublincore.org/terms/" },
				{ "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" },
				{ "rdfs", "http://www.w3.org/2000/01/rdf-schema#" },
				{ "ctdl", "[CTI Namespace Not Determined Yet]" }
			};
			Properties = new List<PropertyData>()
			{
				CommonProperties.Name,
				CommonProperties.Description,
				CommonProperties.Url,
				CommonProperties.Jurisdiction,
				CommonProperties.AccreditedBy,
				CommonProperties.ApprovedBy,
				CommonProperties.AssessedBy,
				CommonProperties.EndorsedBy,
				CommonProperties.RecognizedBy,
				CommonProperties.RegulatedBy,
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:agentType", Source = "OrganizationType", Label = "Organization Type" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:ctid", Source = "ctid", Label = "CTID" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:agentSectorType", Source = "OrganizationSectorType", Label = "Organization Sector Type" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:hasVerificationService", Source = "Authentication", Label = "Verification Services" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:agentQualityAssurancePurpose", Source = "QAPurposeType", Label = "Quality Assurance Purpose Type" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:serviceType", Source = "ServiceType", Label = "Organization Service Type" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:address", Source = "Address", Label = "Address", ProfileType = typeof( AddressProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:agentProcess", Source = "AgentProcess", Label = "Agent Process", ProfileType = typeof( ProcessProfile ) },
				//new PropertyData() { Type = PropertyType.TEXT_LIST, SchemaName = "ctdl:email", Source = "Emails", Label = "Email", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue"  },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ctdl:email", Source = "Emails", Label = "Email", InnerSource = "TextValue"  },
				new PropertyData() { Type = PropertyType.DATE, SchemaName = "ctdl:foundingDate", Source = "DateEffective", Label = "Founding Date" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:sameAs", Source = "SocialMediaPages", Label = "Social Media", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },

				//Roles
				new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:subOrganization", Source = "subsiduary", Label = "Subsidiary Organization", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ctdl:department", Source = "department", Label = "Department", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },

				//Special Handling
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ctdl:alternativeIdentifier", Source = "IdentificationCodes", Label = "Alternative Identifier", InnerFilter = "", InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ctdl:fein", Source = "IdentificationCodes", Label = "Federal Employer identification Number", InnerFilter = "fein", InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ctdl:ipedsID", Source = "IdentificationCodes", Label = "Federal Employer identification Number", InnerFilter = "ipedsID", InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ctdl:opeID", Source = "IdentificationCodes", Label = "Federal Employer identification Number", InnerFilter = "opeID", InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ctdl:duns", Source = "IdentificationCodes", Label = "Dun and Bradstreet DUNS Number", InnerFilter = "duns", InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ctdl:naics", Source = "IdentificationCodes", Label = "North American Industry Classification System", InnerFilter = "naics", InnerSource = "TextValue" },

				//Not Implemented
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:missionAndGoalsStatement", Source = "MISSING", Label = "Verification Services" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:qualityAssuranceTargetType", Source = "MISSING", Label = "Quality Assurance Target Type" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:versioning", Source = "MISSING", Label = "Versioning" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:contactPoint", Source = "MISSING", Label = "Contact Point" },

			};
		}
	}
	//

	public class AssessmentProfile : JsonLDDocument
	{
		public AssessmentProfile()
		{
			Type = "ctdl:AssessmentProfile";
			Context = new Dictionary<string, object>()
			{
				{ "schema", "http://schema.org/" },
				{ "dc", "http://purl.org/dc/elements/1.1/" },
				{ "dct", "http://dublincore.org/terms/" },
				{ "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" },
				{ "rdfs", "http://www.w3.org/2000/01/rdf-schema#" },
				{ "ctdl", "[CTI Namespace Not Determined Yet]" }
			};
			Properties = new List<PropertyData>()
			{
				CommonProperties.Name,
				CommonProperties.Description,
				CommonProperties.DateEffective,
				CommonProperties.Url,
				CommonProperties.Jurisdiction,
				CommonProperties.AccreditedBy,
				CommonProperties.ApprovedBy,
				CommonProperties.AssessedBy,
				CommonProperties.EndorsedBy,
				CommonProperties.MonitoredBy,
				CommonProperties.OfferedBy,
				CommonProperties.Owner,
				CommonProperties.RecognizedBy,
				CommonProperties.RegulatedBy,
				CommonProperties.UpdatedVersionBy,
				CommonProperties.ValidatedBy,
				CommonProperties.VerifiedBy,
				CommonProperties.Creator,
				CommonProperties.Subject,
				CommonProperties.Keyword
			};

		}
	}
	//

	public class LearningOpportunityProfile : JsonLDDocument
	{
		public LearningOpportunityProfile()
		{
			Type = "ctdl:LearningOpportunityProfile";
			Context = new Dictionary<string, object>()
			{
				{ "schema", "http://schema.org/" },
				{ "dc", "http://purl.org/dc/elements/1.1/" },
				{ "dct", "http://dublincore.org/terms/" },
				{ "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" },
				{ "rdfs", "http://www.w3.org/2000/01/rdf-schema#" },
				{ "ctdl", "[CTI Namespace Not Determined Yet]" }
			};
			Properties = new List<PropertyData>()
			{
				CommonProperties.Name,
				CommonProperties.Description,
				CommonProperties.DateEffective,
				CommonProperties.Url,
				CommonProperties.Jurisdiction,
				CommonProperties.AccreditedBy,
				CommonProperties.ApprovedBy,
				CommonProperties.AssessedBy,
				CommonProperties.EndorsedBy,
				CommonProperties.MonitoredBy,
				CommonProperties.OfferedBy,
				CommonProperties.Owner,
				CommonProperties.RecognizedBy,
				CommonProperties.RegulatedBy,
				CommonProperties.UpdatedVersionBy,
				CommonProperties.ValidatedBy,
				CommonProperties.VerifiedBy,
				CommonProperties.Creator,
				CommonProperties.Keyword,
				CommonProperties.Subject,
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:targetCompetency", Source = "TeachesCompetencies", Label = "Competencies", ProfileType = typeof( CredentialAlignmentObjectProfile ), IsMultiSource = true },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:codedNotation", Source = "IdentificationCode", Label = "Identification Code" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:estimatedCost", Source = "EstimatedCost", Label = "Estimated Cost", ProfileType = typeof( CostProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:estimatedDuration", Source = "EstimatedDuration", Label = "Estimated Duration", ProfileType = typeof( DurationProfile ) },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:hasPart", Source = "HasPart", Label = "Embedded Learning Opportunities", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.ENUMERATION_EXTERNAL, SchemaName = "ctdl:instructionalProgramType", Source = "InstructionalProgramCategory", Label = "Instructional Program" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:learningDeliveryType", Source = "LearningOpportunityDeliveryType", Label = "Learning Delivery Type" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:provider", Source = "Provider", Label = "Provider", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },

				//Not implemented yet
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:hasAssessment", Source = "MISSING", Label = "Assessments", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" }
			};
		}
	}
	//

	public class ConditionProfile : JsonLDObject
	{
		public ConditionProfile()
		{
			Type = "ctdl:ConditionProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.ProfileName,
				CommonProperties.Description,
				CommonProperties.DateEffective,
				CommonProperties.Jurisdiction,
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:audienceCategory", Source = "ApplicableAudienceType", Label = "Applicable Audience Type" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:assertedBy", Source = "AssertedBy", Label = "Conditions Asserted By", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:credentialCategory", Source = "CredentialType", Label = "Applicable Credential Type" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:experience", Source = "ConditionItem", Label = "Experience" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:residentOf", Source = "ResidentOf", Label = "Residency" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:resourceUrl", Source = "ReferenceUrl", Label = "Reference URL", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:targetAssessment", Source = "TargetAssessment", Label = "Assessments", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ctdl:minimumAge", Source = "MinimumAge", Label = "Minimum Age" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ctdl:yearsOfExperience", Source = "YearsOfExperience", Label = "Years of Experience" },
				//new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:targetCompetency", Source = "TargetMiniCompetency", Label = "Competencies", ProfileType = typeof( CredentialAlignmentObjectProfile ), OverrideMapping = new Dictionary<string,string>() { { "TextTitle", "TargetName" }, { "TextValue", "TargetUrl" } } },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:targetCompetency", Source = "RequiresCompetencies", Label = "Competencies", ProfileType = typeof( CredentialAlignmentObjectProfile ) },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:targetCredential", Source = "TargetCredential", Label = "Credentials", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:targetLearningOpportunity", Source = "TargetLearningOpportunity", Label = "Learning Opportunities", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:targetTask", Source = "TargetTask", Label = "Tasks", ProfileType = typeof( TaskProfile ) },
			};
		}
	}
	//

	public class RevocationProfile : JsonLDObject
	{
		public RevocationProfile()
		{
			Type = "ctdl:RevocationProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.ProfileName,
				CommonProperties.Description,
				CommonProperties.Jurisdiction,
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:revocationCriteria", Source = "RevocationCriteriaUrl", Label = "Revocation Criteria" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:resourceUrl", Source = "RevocationResourceUrl", Label = "Reference URL", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.DATE, SchemaName = "ctdl:dateEffective", Source = "RemovalDateEffective", Label = "Effective Date" },
			};
		}
	}
	//


	public class CredentialAlignmentObjectProfile : JsonLDObject
	{
		public CredentialAlignmentObjectProfile()
		{
			Type = "ctdl:CredentialAlignmentObject";
			Properties = new List<PropertyData>() 
			{
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:name", Source = "Name", Label = "Name" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:description", Source = "Description", Label = "Description" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:url", Source = "Url", Label = "URL" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:targetName", Source = "TargetName", Label = "Target Name" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:targetDescription", Source = "TargetDescription", Label = "Target Description" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:targetUrl", Source = "TargetUrl", Label = "Target URL" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:alignmentType", Source = "AlignmentType", Label = "Alignment Type" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:codedNotation", Source = "CodedNotation", Label = "Coded Notation" }
			};
		}
	}
	//

	public class TaskProfile : JsonLDObject
	{
		public TaskProfile() {
			Type = "ctdl:TaskProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.Name,
				CommonProperties.Description,
				CommonProperties.Jurisdiction
			};
		}
	}
	//

	public class QualityAssuranceActionProfile : JsonLDObject
	{
		public QualityAssuranceActionProfile()
		{
			Type = "ctdl:QualityAssuranceAction";
			SetupProperties();
		}

		public QualityAssuranceActionProfile( string actionName )
		{
			Type = "ctdl:" + actionName + "Action";
			SetupProperties();
		}

		private void SetupProperties()
		{
			Properties = new List<PropertyData>() 
			{
				CommonProperties.Description,
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:agent", Source = "ActingAgent", Label = "Conditions Asserted By", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:instrument", Source = "IssuedCredential", Label = "Awarded Credential", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:object", Source = "TargetOverride", Label = "Quality Assurance Recipient", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.DATE, SchemaName = "ctdl:startTime", Source = "StartDate", Label = "Start Date" },
				new PropertyData() { Type = PropertyType.DATE, SchemaName = "ctdl:endTime", Source = "EndDate", Label = "End Date" },
				new PropertyData() { Type = PropertyType.PARENT_TYPE_OVERRIDE, SchemaName = "ctdl:", Source = "QAAction", Label = "Quality Assurance Type" }
			};
		}
	}
	//

	public class JurisdictionProfile : JsonLDObject
	{
		public JurisdictionProfile()
		{
			Type = "ctdl:JurisdictionProfile";
			Properties = new List<PropertyData>()
			{
				//No name?
				CommonProperties.Description,
				new PropertyData() { Type = PropertyType.BOOLEAN, SchemaName = "ctdl:onlineJurisdiction", Source = "IsOnlineJurisdiction", Label = "Is Online Jurisdiction" },
				new PropertyData() { Type = PropertyType.BOOLEAN, SchemaName = "ctdl:globalJurisdiction", Source = "IsGlobalJurisdiction", Label = "Is Global Jurisdiction" },
				new PropertyData() { Type = PropertyType.PROFILE, SchemaName = "ctdl:mainJurisdiction", Source = "MainJurisdiction", Label = "Main Jurisdiction", ProfileType = typeof( GeoCoordinates ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ctdl:jurisdictionException", Source = "JurisdictionException", Label = "Jurisdiction Exceptions", ProfileType = typeof( GeoCoordinates ) },
			};
		}
	}
	//

	public class AddressProfile : JsonLDObject
	{
		public AddressProfile()
		{
			Type = "ctdl:PostalAddress";
			Properties = new List<PropertyData>()
			{
				CommonProperties.Name,
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:addressCountry", Source = "Country", Label = "Country" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:addressRegion", Source = "AddressRegion", Label = "State, Province, or Region" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:addressLocality", Source = "City", Label = "City or Locality" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:streetAddress", Source = "StreetAddress", Label = "Street Address" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:postalCode", Source = "PostalCode", Label = "Postal Code" },
				//Not Implemented
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:postOfficeBoxNumber", Source = "", Label = "Post Office Box" }
			};
		}
	}
	//

	public class ProcessProfile : JsonLDObject
	{
		public ProcessProfile()
		{
			Type = "ctdl:ProcessProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.ProfileName,
				CommonProperties.Description
			};
		}
	}
	//

	public class CostProfile : JsonLDObject
	{
		public CostProfile()
		{
			Type = "ctdl:CostProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.ProfileName,
				CommonProperties.Description,
				CommonProperties.Jurisdiction,
				new PropertyData() { Type = PropertyType.DATE, SchemaName = "ctdl:startTime", Source = "DateEffective", Label = "Start Date" },
				new PropertyData() { Type = PropertyType.DATE, SchemaName = "ctdl:endTime", Source = "ExpirationDate", Label = "End Date" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ctdl:resourceUrl", Source = "ReferenceUrl", Label = "Reference URL", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:paymentPattern", Source = "PaymentPattern", Label = "Payment Pattern" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:currency", Source = "Currency", Label = "Currency" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ctdl:price", Source = "Price", Label = "Price" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:costType", Source = "CostType", Label = "Cost Type Type" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:residencyType", Source = "ResidencyType", Label = "Residency Type" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:scheduleType", Source = "EnrollmentType", Label = "Enrollment Type" },
				new PropertyData() { Type = PropertyType.ENUMERATION, SchemaName = "ctdl:audienceType", Source = "ApplicableAudienceType", Label = "Applicable Audience Type" },
			};
		}
	}
	//

	public class GeoCoordinates : JsonLDObject
	{
		public GeoCoordinates()
		{
			Type = "ctdl:GeoCoordinates";
			Properties = new List<PropertyData>()
			{
				CommonProperties.Name,
				CommonProperties.Url,
				new PropertyData() { Type = PropertyType.PROFILE, SchemaName = "ctdl:address", Source = "Address", Label = "Address", ProfileType = typeof( AddressProfile ) },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ctdl:latitude", Source = "Latitude", Label = "Latitude" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ctdl:longitude", Source = "Longitude", Label = "Longitude" },
			};
		}
	}
	//

	public class DurationProfile : JsonLDObject
	{
		public DurationProfile()
		{
			Type = "ctdl:DurationProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.ProfileName,
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "schema:description", Source = "Conditions", Label = "Description" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:minimumDuration", Source = "MinimumDurationISO8601", Label = "Minimum Duration" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:maximumDuration", Source = "MaximumDurationISO8601", Label = "Maximum Duration" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ctdl:exactDuration", Source = "ExactDurationISO8601", Label = "Exact Duration" }
			};
		}
	}
	//

	public enum PropertyType
	{
		TEXT, URL, NUMBER, DATE, BOOLEAN, ENUMERATION, ENUMERATION_EXTERNAL, PROFILE, PROFILE_LIST, ROLE, TEXTVALUE_LIST, PARENT_TYPE_OVERRIDE
	}

	public enum SourceType
	{
		DIRECT, FROM_OBJECT, FROM_OBJECT_LIST, FROM_ENUMERATION, FROM_METHOD
	}

	public class PropertyData
	{
		public PropertyData()
		{
			SchemaName = "";
			Source = "";
			Type = PropertyType.TEXT;
			SourceType = SourceType.DIRECT;
			ProfileType = typeof( string );
			Label = "";
			InnerSource = "";
			InnerFilter = "";
			OverrideMapping = new Dictionary<string, string>();
			IsMultiSource = false;
		}
		public string SchemaName { get; set; }
		public string Label { get; set; }
		public string Source { get; set; }
		public PropertyType Type { get; set; }
		public Type ProfileType { get; set; }
		public SourceType SourceType { get; set; }
		public string InnerSource { get; set; }
		public string InnerFilter { get; set; }
		public bool IsMultiSource { get; set; }
		public Dictionary<string, string> OverrideMapping { get; set; } //Override automatic mapping for properties. Primarily for use with TextValueProfiles when they need to map to an object and not just a string. Values should always take the form of { "MyProperty", "ForeginProperty" } where "MyProperty" belongs to the object that contains the mapping and "ForeignProperty" belongs to the other object (for instance, "MyProperty" might be "TextTitle" and "ForeignProperty" might be "TargetName")
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class Property : Attribute
	{
		public string SchemaName { get; set; }
	}
	//

	[AttributeUsage(AttributeTargets.Class)]
	public class Profile : Attribute
	{
		public string SchemaName { get; set; }
		
	}
	//
}
