using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace Models.JsonV2Manual
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
		public PropertyData Name = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:name", Source = "Name", Label = "Name" };
		public PropertyData ProfileName = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:name", Source = "ProfileName", Label = "Name" };
		public PropertyData Description = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:description", Source = "Description", Label = "Description" };
		public PropertyData DateEffective = new PropertyData() { Type = PropertyType.DATE, SchemaName = "ceterms:dateEffective", Source = "DateEffective", Label = "Date Effective" };
		public PropertyData Url = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:url", Source = "Url", Label = "Url" };
		public PropertyData Jurisdiction = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:jurisdiction", Source = "Jurisdiction", Label = "Jurisdiction", ProfileType = typeof( JurisdictionProfile ) };
		public PropertyData Keyword = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:keyword", Source = "Keyword", Label = "Keywords", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData Subject = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:subject", Source = "Subject", Label = "Subjects", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData EstimatedCosts = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:estimatedCost", Source = "EstimatedCosts", Label = "Estimated Costs", ProfileType = typeof( CostProfile ), SourceType = SourceType.FROM_METHOD, InnerSource = "FlattenCosts" };
		public PropertyData QualityAssuranceActionsReceived = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:relatedAction", Source = "QualityAssuranceAction", Label = "Related Action", ProfileType = typeof( QualityAssuranceActionProfile ) };
		public PropertyData AvailableOnlineAt = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:availableOnlineAt", Source = "AvailableOnlineAt", Label = "Available Online At" };
		public PropertyData AvailabilityListing = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:availabilityListing", Source = "AvailabilityListing", Label = "Availability Listing" };
		public PropertyData AvailableAt = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:availableAt", Source = "Addresses", Label = "Available At", ProfileType = typeof( GeoCoordinates ), SourceType = SourceType.FROM_METHOD, InnerSource = "WrapAddress" };
		public PropertyData ReferenceUrl = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:resourceUrl", Source = "ReferenceUrl", Label = "Reference URL", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };

		//Roles
		public PropertyData AccreditedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:accreditedBy", Source = "OrganizationRole", Label = "Accredited By" };
		public PropertyData ApprovedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:approvedBy", Source = "OrganizationRole", Label = "Approved By" };
		public PropertyData AssessedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:assessedBy", Source = "OrganizationRole", Label = "Assessed By" };
		public PropertyData ConferredBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:conferredBy", Source = "OrganizationRole", Label = "Conferred By" };
		public PropertyData EndorsedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:endorsedBy", Source = "OrganizationRole", Label = "Endorsed By" };
		public PropertyData MonitoredBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:monitoredBy", Source = "OrganizationRole", Label = "Monitored By" };
		public PropertyData OfferedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:offeredBy", Source = "OrganizationRole", Label = "Offered By" };
		public PropertyData Owner = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:owner", Source = "OrganizationRole", Label = "Owner" };
		public PropertyData RecognizedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:recognizedBy", Source = "OrganizationRole", Label = "Recognized By" };
		public PropertyData RegulatedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:regulatedBy", Source = "OrganizationRole", Label = "Regulated By" };
		public PropertyData RenewalBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:renewalsBy", Source = "OrganizationRole", Label = "Renewals By" };
		public PropertyData RevocationBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:revocationBy", Source = "OrganizationRole", Label = "Revocation By" };
		public PropertyData UpdatedVersionBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:updatedVersionBy", Source = "OrganizationRole", Label = "Updated Version By" };
		public PropertyData ValidatedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:validatedBy", Source = "OrganizationRole", Label = "Validated By" };
		public PropertyData VerifiedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:verifiedBy", Source = "OrganizationRole", Label = "Verified By" };
		public PropertyData Creator = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:creator", Source = "OrganizationRole", Label = "Creator" };
		public PropertyData Contributor = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:contributor", Source = "OrganizationRole", Label = "Contributor" };
		public PropertyData Provider = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:provider", Source = "OrganizationRole", Label = "Provider" };
		public PropertyData EvaluatedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:evaluatedBy", Source = "OrganizationRole", Label = "Evaluated By" };

		//Location Roles
		public PropertyData AccreditedIn = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:accreditedIn", Source = "MISSING", Label = "Accredited In" };
		public PropertyData ApprovedIn = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:approvedIn", Source = "MISSING", Label = "Approved In" };
		public PropertyData OfferedIn = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:offeredIn", Source = "MISSING", Label = "Offered In" };
		public PropertyData RecognizedIn = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:recognizedIn", Source = "MISSING", Label = "Recognized In" };
		public PropertyData RegulatedIn = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:regulatedIn", Source = "MISSING", Label = "Regulated In" };
		public PropertyData RenewedIn = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:renewedIn", Source = "MISSING", Label = "Renewed In" };
		public PropertyData RevokedIn = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:revokedIn", Source = "MISSING", Label = "Revoked In" };

		//Is Similar To
		public PropertyData BroadAlignment = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:broadAlignment", Source = "MISSING", Label = "Broad Alignment" };
		public PropertyData ExactAlignment = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:exactAlignment", Source = "MISSING", Label = "Exact Alignment" };
		public PropertyData MajorAlignment = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:majorAlignment", Source = "MISSING", Label = "Major Alignment" };
		public PropertyData MinorAlignment = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:minorAlignment", Source = "MISSING", Label = "Minor Alignment" };
		public PropertyData NarrowAlignment = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:narrowAlignment", Source = "MISSING", Label = "Narrow Alignment" };
	}
	//


	public class Credential : JsonLDDocument
	{
		public Credential()
		{
			Type = "ceterms:Credential";
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
				CommonProperties.QualityAssuranceActionsReceived,
				CommonProperties.AvailableAt,
				CommonProperties.AvailableOnlineAt,
				CommonProperties.AvailabilityListing,
				new PropertyData() { Type = PropertyType.PARENT_TYPE_OVERRIDE, SchemaName = "ceterms:", Source = "CredentialType", Label = "Credential Type", SourceType = SourceType.FROM_ENUMERATION },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:alternateName", Source = "AlternateName", Label = "Alternate Name" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:versionIdentifier", Source = "Version", Label = "Version" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:ctid", Source = "ctid", Label = "CTID" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:latestVersion", Source = "LatestVersionUrl", Label = "Latest Version" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:previousVersion", Source = "PreviousVersion", Label = "Previous Version" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:hasPart", Source = "EmbeddedCredentials", Label = "Embedded Credential", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "ctid" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:image", Source = "ImageUrl", Label = "Image URL" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:typicalAudienceLevelType", Source = "CredentialLevel", Label = "Credential Level" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:purposeType", Source = "Purpose", Label = "Purpose" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:industryType", Source = "Industry", Label = "Industry Category" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:occupationType", Source = "Occupation", Label = "Occupation Category" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:estimatedDuration", Source = "EstimatedTimeToEarn", Label = "Estimated Time to Earn", ProfileType = typeof( DurationProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:recommendedFor", Source = "IsRecommendedFor", Label = "Recommended For", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:requiredFor", Source = "IsRequiredFor", Label = "Required For", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:recommends", Source = "Recommends", Label = "Recommends", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:renewal", Source = "Renewal", Label = "Renewal", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:requires", Source = "Requires", Label = "Requires", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:isAdvancedStandingFor", Source = "AdvancedStandingFor", Label = "Advanced Standing For", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:advancedStandingFrom", Source = "AdvancedStandingFrom", Label = "Advanced Standing From", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:isPreparationFor", Source = "AdvancedStandingFor", Label = "Advanced Standing For", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:preparationFrom", Source = "AdvancedStandingFrom", Label = "Advanced Standing From", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:revocation", Source = "Revocation", Label = "Revocation", ProfileType = typeof( RevocationProfile ) },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:industryTypeOther", Source = "OtherIndustries", Label = "Other Industries", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:occupationTypeOther", Source = "OtherOccupations", Label = "Other Occupations", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },

				//Not Implemented Yet
				CommonProperties.AccreditedIn,
				CommonProperties.ApprovedIn,
				CommonProperties.OfferedIn,
				CommonProperties.RecognizedIn,
				CommonProperties.RegulatedIn,
				CommonProperties.RenewedIn,
				CommonProperties.RevokedIn,
				CommonProperties.BroadAlignment,
				CommonProperties.ExactAlignment,
				CommonProperties.MajorAlignment,
				CommonProperties.MinorAlignment,
				CommonProperties.NarrowAlignment,
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:codedNotation", Source = "MISSING", Label = "Coded Notation" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:credentialStatusType", Source = "MISSING", Label = "Credential Status" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:developmentProcess", Source = "MISSING", Label = "Development Process" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:employmentOutcome", Source = "MISSING", Label = "Employment Outcome Statistics" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:holders", Source = "MISSING", Label = "Holders Statistics" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:maintenanceProcess", Source = "MISSING", Label = "Maintenance Process" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:earnings", Source = "MISSING", Label = "Earnings Statistics" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:selectionProcess", Source = "MISSING", Label = "Selection Process" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:validationProcess", Source = "MISSING", Label = "Validation Process" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:contributor", Source = "MISSING", Label = "Contributor", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:copyrightHolder", Source = "MISSING", Label = "Copyright Holder", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:sameAs", Source = "MISSING", Label = "Same As" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:credentialId", Source = "MISSING", Label = "Credential ID" }
			};
		}
	}
	//

	public class Organization : JsonLDDocument
	{
		public Organization()
		{
			Type = "ceterms:Organization";
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
				CommonProperties.MonitoredBy,
				CommonProperties.RecognizedBy,
				CommonProperties.RegulatedBy,
				CommonProperties.ValidatedBy,
				CommonProperties.VerifiedBy,
				CommonProperties.QualityAssuranceActionsReceived,
				CommonProperties.Keyword,
				CommonProperties.AvailabilityListing,
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:agentType", Source = "OrganizationType", Label = "Organization Type" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:ctid", Source = "ctid", Label = "CTID" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:versioning", Source = "Versioning", Label = "Versioning" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:agentSectorType", Source = "OrganizationSectorType", Label = "Organization Sector Type" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:hasVerificationService", Source = "Authentication", Label = "Verification Services", ProfileType = typeof( VerificationServiceProfile ) },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:agentQualityAssurancePurpose", Source = "QAPurposeType", Label = "Quality Assurance Purpose Type" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:qualityAssuranceTargetType", Source = "QATargetType", Label = "Quality Assurance Target Type" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:serviceType", Source = "ServiceType", Label = "Organization Service Type" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:address", Source = "Addresses", Label = "Address", ProfileType = typeof( GeoCoordinates ), SourceType = SourceType.FROM_METHOD, InnerSource = "WrapAddress" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:agentProcess", Source = "AgentProcess", Label = "Agent Process", ProfileType = typeof( ProcessProfile ) },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:email", Source = "Emails", Label = "Email", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue"  },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:telephone", Source = "PhoneNumbers", SourceType = SourceType.FROM_OBJECT_LIST, Label = "Phone Numbers", InnerSource = "TextValue"  },
				new PropertyData() { Type = PropertyType.DATE, SchemaName = "ceterms:foundingDate", Source = "DateEffective", Label = "Founding Date" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:sameAs", Source = "SocialMediaPages", Label = "Social Media", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },


				//Roles
				new PropertyData() { Type = PropertyType.ROLE, SchemaName = "subOrganization", Source = "OrganizationRole", Label = "Subsidiary Organization" },
				new PropertyData() { Type = PropertyType.ROLE, SchemaName = "department", Source = "OrganizationRole", Label = "Department" },

				//Special Handling
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ceterms:alternativeIdentifier", Source = "IdentificationCodes", Label = "Alternative Identifier", InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ceterms:fein", Source = "IdentificationCodes", Label = "Federal Employer identification Number", InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ceterms:ipedsID", Source = "IdentificationCodes", Label = "Integrated Postsecondary Education Data System", InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ceterms:opeID", Source = "IdentificationCodes", Label = "Office of Postsecondary Education Identification", InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ceterms:duns", Source = "IdentificationCodes", Label = "Dun and Bradstreet DUNS Number", InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ceterms:naics", Source = "IdentificationCodes", Label = "North American Industry Classification System", InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ceterms:taxID", Source = "IdentificationCodes", Label = "Federal Tax ID", InnerSource = "TextValue" },

				//Not Implemented
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:agentProcess", Source = "AgentProcess", Label = "Agent Process", ProfileType = typeof( ProcessProfile ) },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:missionAndGoalsStatement", Source = "MISSING", Label = "Verification Services" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:qualityAssuranceTargetType", Source = "MISSING", Label = "Quality Assurance Target Type" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:versioning", Source = "MISSING", Label = "Versioning" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:contactPoint", Source = "MISSING", Label = "Contact Point" },

			};
		}
	}
	//

	public class AssessmentProfile : JsonLDDocument
	{
		public AssessmentProfile()
		{
			Type = "ceterms:AssessmentProfile";
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
				CommonProperties.Provider,
				CommonProperties.Subject,
				CommonProperties.Keyword,
				CommonProperties.QualityAssuranceActionsReceived,
				CommonProperties.AvailableAt,
				CommonProperties.AvailableOnlineAt,
				CommonProperties.AvailabilityListing,
				CommonProperties.EstimatedCosts,
				CommonProperties.ReferenceUrl,
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:assessmentMethodType", Source = "AssessmentType", Label = "Assessment Method Type" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:ctid", Source = "ctid", Label = "CTID" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:learningDeliveryType", Source = "DeliveryType", Label = "Assessment Delivery Type" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:assessmentModeType", Source = "AssessmentUseType", Label = "Assessment Use Type" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:estimatedDuration", Source = "EstimatedDuration", Label = "Estimated Duration", ProfileType = typeof( DurationProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:targetCompetency", Source = "TargetCompetencies", Label = "Competencies", ProfileType = typeof( CredentialAlignmentObjectProfile ) },
				//new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:assessmentExample", Source = "AssessmentExampleUrl", Label = "Assessment Example" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:assessmentInformation", Source = "AssessmentInformationUrl", Label = "Assessment Information" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:resourceUrl", Source = "ResourceUrl", Label = "Resource URL", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:assessmentExample", Source = "AssessmentExample", Label = "Assessment Exmaple URL", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:codedNotation", Source = "CodedNotation", Label = "Coded Notation" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:requires", Source = "Requires", Label = "Requires", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:recommends", Source = "Recommends", Label = "Recommends", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:instructionalProgramType", Source = "InstructionalProgramCategory", Label = "Instructional Program" },

				//Not Implemented
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:targetAssessment", Source = "MISSING", Label = "Target Assessment" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:assessmentInformation", Source = "MISSING", Label = "Assessment Example", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:administrationProcess", Source = "MISSING", Label = "Administration Process", ProfileType = typeof( ProcessProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:developmentProcess", Source = "MISSING", Label = "Development Process", ProfileType = typeof( ProcessProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:maintenanceProcess", Source = "MISSING", Label = "Maintenance Process", ProfileType = typeof( ProcessProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:selectionProcess", Source = "MISSING", Label = "Selection Process", ProfileType = typeof( ProcessProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:validationProcess", Source = "MISSING", Label = "Validation Process", ProfileType = typeof( ProcessProfile ) },

			};

		}
	}
	//

	public class LearningOpportunityProfile : JsonLDDocument
	{
		public LearningOpportunityProfile()
		{
			Type = "ceterms:LearningOpportunityProfile";
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
				CommonProperties.EvaluatedBy,
				CommonProperties.MonitoredBy,
				CommonProperties.OfferedBy,
				CommonProperties.Owner,
				CommonProperties.RecognizedBy,
				CommonProperties.RegulatedBy,
				CommonProperties.UpdatedVersionBy,
				CommonProperties.ValidatedBy,
				CommonProperties.VerifiedBy,
				CommonProperties.Creator,
				CommonProperties.Provider,
				CommonProperties.Keyword,
				CommonProperties.Subject,
				CommonProperties.QualityAssuranceActionsReceived,
				CommonProperties.AvailableAt,
				CommonProperties.AvailableOnlineAt,
				CommonProperties.AvailabilityListing,
				CommonProperties.EstimatedCosts,
				CommonProperties.ReferenceUrl,
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:learningResource", Source = "LearningResourceUrls", Label = "Reference URL", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:ctid", Source = "ctid", Label = "CTID" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:targetCompetency", Source = "TargetCompetencies", Label = "Competencies", ProfileType = typeof( CredentialAlignmentObjectProfile ) },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:codedNotation", Source = "CodedNotation", Label = "Identification Code" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:estimatedCost", Source = "EstimatedCost", Label = "Estimated Cost", ProfileType = typeof( CostProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:estimatedDuration", Source = "EstimatedDuration", Label = "Estimated Duration", ProfileType = typeof( DurationProfile ) },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:hasPart", Source = "HasPart", Label = "Embedded Learning Opportunities", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:instructionalProgramType", Source = "InstructionalProgramCategory", Label = "Instructional Program" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:learningDeliveryType", Source = "deliveryType", Label = "Learning Delivery Type" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:assessmentProfile", Source = "EmbeddedAssessment", Label = "Assessments", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:requires", Source = "Requires", Label = "Requires", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:recommends", Source = "Recommends", Label = "Recommends", ProfileType = typeof( ConditionProfile ) },

				//Not implemented yet
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:learningResource", Source = "LearningResourceUrl", Label = "Learning Resources", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:assessmentProfile", Source = "MISSING", Label = "Assessments", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:targetCredential", Source = "MISSING", Label = "Credentials", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
			};
		}
	}
	//

	public class ConditionProfile : JsonLDObject
	{
		public ConditionProfile()
		{
			Type = "ceterms:ConditionProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.ProfileName,
				CommonProperties.Description,
				CommonProperties.DateEffective,
				CommonProperties.Jurisdiction,
				CommonProperties.ReferenceUrl,
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:audienceCategory", Source = "ApplicableAudienceType", Label = "Applicable Audience Type" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:assertedBy", Source = "AssertedBy", Label = "Conditions Asserted By", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:credentialCategory", Source = "CredentialType", Label = "Applicable Credential Type" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:experience", Source = "ConditionItem", Label = "Experience" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:residentOf", Source = "ResidentOf", Label = "Residency" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:targetAssessment", Source = "TargetAssessment", Label = "Assessments", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:minimumAge", Source = "MinimumAge", Label = "Minimum Age" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:yearsOfExperience", Source = "YearsOfExperience", Label = "Years of Experience" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:weight", Source = "Weight", Label = "Weight" },
				//new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:targetCompetency", Source = "TargetMiniCompetency", Label = "Competencies", ProfileType = typeof( CredentialAlignmentObjectProfile ), OverrideMapping = new Dictionary<string,string>() { { "TextTitle", "TargetName" }, { "TextValue", "TargetUrl" } } },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:targetCompetency", Source = "RequiresCompetencies", Label = "Competencies", ProfileType = typeof( CredentialAlignmentObjectProfile ) },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:targetCredential", Source = "TargetCredential", Label = "Credentials", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:targetLearningOpportunity", Source = "TargetLearningOpportunity", Label = "Learning Opportunities", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:targetTask", Source = "TargetTask", Label = "Tasks", ProfileType = typeof( TaskProfile ) },
			};
		}
	}
	//

	public class RevocationProfile : JsonLDObject
	{
		public RevocationProfile()
		{
			Type = "ceterms:RevocationProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.ProfileName,
				CommonProperties.Description,
				CommonProperties.Jurisdiction,
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:revocationCriteria", Source = "RevocationCriteriaUrl", Label = "Revocation Criteria" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:resourceUrl", Source = "RevocationResourceUrl", Label = "Reference URL", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:description", Source = "RevocationItems", Label = "Revocation Items", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.DATE, SchemaName = "ceterms:dateEffective", Source = "RemovalDateEffective", Label = "Effective Date" },
			};
		}
	}
	//

	public class VerificationServiceProfile : JsonLDObject
	{
		public VerificationServiceProfile()
		{
			Type = "ctld:VerificationServiceProfile";
			Properties = new List<PropertyData>() 
			{
				//No name?
				CommonProperties.Description,
				CommonProperties.Jurisdiction,
				CommonProperties.DateEffective,
				CommonProperties.EstimatedCosts,
				new PropertyData() { Type = PropertyType.BOOLEAN, SchemaName = "ceterms:holderMustAuthorize", Source = "HolderMustAuthorize", Label = "Holder must authorize this service" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:targetCredential", Source = "RelevantCredential", Label = "Related Credential", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },
				//Not Implemented
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:verifiedClaimType", Source = "MISSING", Label = "Verified Claim Type" },
				new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:provider", Source = "OrganizationRole", Label = "Provided By" }
			};
		}
	}
	//

	public class CredentialAlignmentObjectProfile : JsonLDObject
	{
		public CredentialAlignmentObjectProfile()
		{
			Type = "ceterms:CredentialAlignmentObject";
			Properties = new List<PropertyData>() 
			{
				//new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:name", Source = "Name", Label = "Name" },
				//new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:description", Source = "Description", Label = "Description" },
				//new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:url", Source = "Url", Label = "URL" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:targetName", Source = "Name", Label = "Target Name" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:targetDescription", Source = "Description", Label = "Target Description" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:targetUrl", Source = "Url", Label = "Target URL" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:alignmentType", Source = "AlignmentType", Label = "Alignment Type" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:codedNotation", Source = "CodedNotation", Label = "Coded Notation" }
			};
		}
	}
	//

	public class TaskProfile : JsonLDObject
	{
		public TaskProfile() {
			Type = "ceterms:TaskProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.Name,
				CommonProperties.Description,
				CommonProperties.DateEffective,
				CommonProperties.Jurisdiction,
				CommonProperties.AvailableOnlineAt,
				CommonProperties.AvailableAt,
				CommonProperties.AvailabilityListing,
				CommonProperties.EstimatedCosts,
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:resourceUrl", Source = "ReferenceUrl", Label = "Resource URL", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:affiliatedAgent", Source = "AffiliatedAgent", Label = "Task Provided By", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:estimatedDuration", Source = "EstimatedDuration", Label = "Estimated Duration", ProfileType = typeof( DurationProfile ) }
			};
		}
	}
	//

	public class QualityAssuranceActionProfile : JsonLDObject
	{
		public QualityAssuranceActionProfile()
		{
			Type = "ceterms:QualityAssuranceAction";
			SetupProperties();
		}

		public QualityAssuranceActionProfile( string actionName )
		{
			Type = "ceterms:" + actionName + "Action";
			SetupProperties();
		}

		private void SetupProperties()
		{
			Properties = new List<PropertyData>() 
			{
				CommonProperties.Description,
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:agent", Source = "ActingAgent", Label = "Conditions Asserted By", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:instrument", Source = "IssuedCredential", Label = "Awarded Credential", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:object", Source = "TargetOverride", Label = "Quality Assurance Recipient", SourceType = SourceType.FROM_OBJECT, InnerSource = "Url" },
				new PropertyData() { Type = PropertyType.DATE, SchemaName = "ceterms:startTime", Source = "StartDate", Label = "Start Date" },
				new PropertyData() { Type = PropertyType.DATE, SchemaName = "ceterms:endTime", Source = "EndDate", Label = "End Date" },
				new PropertyData() { Type = PropertyType.PARENT_TYPE_OVERRIDE, SchemaName = "ceterms:", Source = "QualityAssuranceType", Label = "Quality Assurance Type" }
			};
		}
	}
	//

	public class JurisdictionProfile : JsonLDObject
	{
		public JurisdictionProfile()
		{
							//new PropertyData() { Type = PropertyType.BOOLEAN, SchemaName = "ceterms:onlineJurisdiction", Source = "IsOnlineJurisdiction", Label = "Is Online Jurisdiction" },

			Type = "ceterms:JurisdictionProfile";
			Properties = new List<PropertyData>()
			{
				//No name?
				CommonProperties.Description,
				new PropertyData() { Type = PropertyType.BOOLEAN, SchemaName = "ceterms:globalJurisdiction", Source = "IsGlobalJurisdiction", Label = "Is Global Jurisdiction" },
				new PropertyData() { Type = PropertyType.PROFILE, SchemaName = "ceterms:mainJurisdiction", Source = "MainJurisdiction", Label = "Main Jurisdiction", ProfileType = typeof( GeoCoordinates ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:jurisdictionException", Source = "JurisdictionException", Label = "Jurisdiction Exceptions", ProfileType = typeof( GeoCoordinates ) },
			};
		}
	}
	//

	public class AddressProfile : JsonLDObject
	{
		public AddressProfile()
		{
			Type = "ceterms:PostalAddress";
			Properties = new List<PropertyData>()
			{
				CommonProperties.Name,
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:addressCountry", Source = "Country", Label = "Country" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:addressRegion", Source = "AddressRegion", Label = "State, Province, or Region" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:addressLocality", Source = "City", Label = "City or Locality" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:streetAddress", Source = "StreetAddress", Label = "Street Address" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:postalCode", Source = "PostalCode", Label = "Postal Code" },
				//Not Implemented
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:postOfficeBoxNumber", Source = "", Label = "Post Office Box" }
			};
		}
	}
	//

	public class ProcessProfile : JsonLDObject
	{
		public ProcessProfile()
		{
			Type = "ceterms:ProcessProfile";
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
			Type = "ceterms:CostProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.ProfileName,
				CommonProperties.Description,
				CommonProperties.Jurisdiction,
				CommonProperties.ReferenceUrl,
				new PropertyData() { Type = PropertyType.DATE, SchemaName = "ceterms:startTime", Source = "DateEffective", Label = "Start Date" },
				new PropertyData() { Type = PropertyType.DATE, SchemaName = "ceterms:endTime", Source = "ExpirationDate", Label = "End Date" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:paymentPattern", Source = "PaymentPattern", Label = "Payment Pattern" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:currency", Source = "Currency", Label = "Currency" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:price", Source = "Price", Label = "Price" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:costType", Source = "CostType", Label = "Cost Type Type" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:residencyType", Source = "ResidencyType", Label = "Residency Type" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:scheduleType", Source = "EnrollmentType", Label = "Enrollment Type" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:audienceType", Source = "ApplicableAudienceType", Label = "Applicable Audience Type" },
			};
		}
	}
	//

	public class GeoCoordinates : JsonLDObject
	{
		public GeoCoordinates()
		{
			Type = "ceterms:GeoCoordinates";
			Properties = new List<PropertyData>()
			{
				CommonProperties.Name,
				CommonProperties.Url,
				new PropertyData() { Type = PropertyType.PROFILE, SchemaName = "ceterms:address", Source = "Address", Label = "Address", ProfileType = typeof( AddressProfile ) },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:latitude", Source = "Latitude", Label = "Latitude" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:longitude", Source = "Longitude", Label = "Longitude" },
			};
		}
	}
	//

	public class DurationProfile : JsonLDObject
	{
		public DurationProfile()
		{
			Type = "ceterms:DurationProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.ProfileName,
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:description", Source = "Conditions", Label = "Description" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:minimumDuration", Source = "MinimumDurationISO8601", Label = "Minimum Duration" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:maximumDuration", Source = "MaximumDurationISO8601", Label = "Maximum Duration" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:exactDuration", Source = "ExactDurationISO8601", Label = "Exact Duration" }
			};
		}
	}
	//

	public enum PropertyType
	{
		TEXT, URL, NUMBER, DATE, BOOLEAN, ENUMERATION, ENUMERATION_EXTERNAL, PROFILE, PROFILE_LIST, ROLE, TEXTVALUE_LIST, PARENT_TYPE_OVERRIDE, ENUMERATION_ALIGNMENTOBJECT_LIST
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
