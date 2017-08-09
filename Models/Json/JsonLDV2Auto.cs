using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			Context = new Dictionary<string, object>()
			{
				{ "ceterms", "http://purl.org/ctdl/terms/" },
				{ "dc", "http://purl.org/dc/elements/1.1/" },
				{ "dct", "http://dublincore.org/dc/terms/" },
				{ "foaf", "http://xmlns.com/foaf/0.1/" },
				{ "obi", "https://w3id.org/openbadges#" },
				{ "owl", "http://www.w3.org/2002/07/owl#" },
				{ "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" },
				{ "rdfs", "http://www.w3.org/2000/01/rdf-schema#" },
				{ "schema", "http://schema.org/" },
				{ "skos", "http://www.w3.org/2004/02/skos/core#" },
				{ "vs", "https://www.w3.org/2003/06/sw-vocab-status/ns" },
				{ "xsd", "http://www.w3.org/2001/XMLSchema#" },
				{ "lrmi", "http://purl.org/dcx/lrmi-terms/" },
				{ "asn", "http://purl.org/ASN/schema/core/" },
				{ "vann", "http://purl.org/vocab/vann/" },
				{ "actionStat", "http://purl.org/ctdl/vocabs/actionStat/" },
				{ "agentSector", "http://purl.org/ctdl/vocabs/agentSector/" },
				{ "serviceType", "http://purl.org/ctdl/vocabs/serviceType/" },
				{ "assessMethod", "http://purl.org/ctdl/vocabs/assessMethod/" },
				{ "assessUse", "http://purl.org/ctdl/vocabs/assessUse/" },
				{ "audience", "http://purl.org/ctdl/vocabs/audience/" },
				{ "claimType", "http://purl.org/ctdl/vocabs/claimType/" },
				{ "costType", "http://purl.org/ctdl/vocabs/costType/" },
				{ "credentialStat", "http://purl.org/ctdl/vocabs/credentialStat/" },
				{ "creditUnit", "http://purl.org/ctdl/vocabs/creditUnit/" },
				{ "deliveryType", "http://purl.org/ctdl/vocabs/deliveryType/" },
				{ "inputType", "http://purl.org/ctdl/vocabs/inputType/" },
				{ "learnMethod", "http://purl.org/ctdl/vocabs/learnMethod/" },
				{ "orgType", "http://purl.org/ctdl/vocabs/orgType/" },
				{ "residency", "http://purl.org/ctdl/vocabs/residency/" },
				{ "score", "http://purl.org/ctdl/vocabs/score/" },
				{ "@language", "en-US" } //Default. May change later if/when we implement language selection
			};
		}
		public Dictionary<string, object> Context { get; set; }
	}

	public enum PropertyType
	{
		TEXT, URL, NUMBER, DATE, DATETIME, DURATION, BOOLEAN, ENUMERATION, ENUMERATION_EXTERNAL, PROFILE, PROFILE_EXTERNAL, PROFILE_EXTERNAL_LIST, PROFILE_LIST, ROLE, TEXTVALUE_LIST, PARENT_TYPE_OVERRIDE, ENUMERATION_ALIGNMENTOBJECT_LIST
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

		/// <summary>
		/// Override automatic mapping for properties.
		/// Primarily for use with TextValueProfiles when they need to map to an object and not just a string. 
		/// 
		/// Values should always take the form of 
		///		{ "MyProperty", "ForeignProperty" } 
		///	where "MyProperty" belongs to the object that contains the mapping and "ForeignProperty" belongs to the other object.
		/// For instance, "MyProperty" might be "TextTitle" and "ForeignProperty" might be "TargetName".
		/// </summary>
		public Dictionary<string, string> OverrideMapping { get; set; } 
	}

	[AttributeUsage( AttributeTargets.Property )]
	public class Property : Attribute
	{
		public string SchemaName { get; set; }
	}
	//

	[AttributeUsage( AttributeTargets.Class )]
	public class Profile : Attribute
	{
		public string SchemaName { get; set; }

	}
	//

	public class JsonLDIdentifier : JsonLDObject
	{
		public JsonLDIdentifier()
		{
			Type = "@id";
			Properties = new List<PropertyData>()
			{
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "@id", Source = "Url", Label = "Identifier" }
			};
		}
	}

	/* BEGIN AUTO GENERATED PROPERTIES */


	public class CommonProperties
	{
		public PropertyData AccreditedBy = new PropertyData() {
			Type = PropertyType.ROLE,
			SchemaName = "ceterms:accreditedBy",
			Source = "OrganizationRole",
			Label = "Accredited By"
		};

		public PropertyData AccreditedIn = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:accreditedIn", Source = "AccreditedIn", Label = "Accredited In", ProfileType = typeof( JurisdictionProfile ) };
		public PropertyData ActingAgent = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:actingAgent", Source = "ActingAgent", Label = "Acting Agent", ProfileType = typeof( JsonLDIdentifier ) };
		//public PropertyData ActionStatusType = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:actionStatusType", Source = "ActionStatusType", Label = "Action Status Type", ProfileType = typeof( ERROR ) };

		public PropertyData Address = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:address", Source = "Auto_Address", Label = "Address", ProfileType = typeof( PostalAddress ) };

		public PropertyData AdministrationProcess = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:administrationProcess", Source = "AdministrationProcess", Label = "Administration Process", ProfileType = typeof( ProcessProfile ) };

		public PropertyData AdvancedStandingFrom = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:advancedStandingFrom", Source = "AdvancedStandingFrom", Label = "Advanced Standing From", ProfileType = typeof( ConditionProfile ) };

		public PropertyData Agent = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:agent", Source = "Agent", Label = "Agent", ProfileType = typeof( JsonLDIdentifier ) };

		public PropertyData AgentPurpose = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:agentPurpose", Source = "AgentPurpose", Label = "Agent Purpose" };
		public PropertyData AgentPurposeDescription = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:agentPurposeDescription", Source = "AgentPurposeDescription", Label = "Agent Purpose Description" };
		public PropertyData AgentSectorType = new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:agentSectorType", Source = "AgentSectorType", Label = "Agent Sector Type" };
		public PropertyData AgentType = new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:agentType", Source = "OrganizationType", Label = "Agent Type" };
		public PropertyData AlignmentDate = new PropertyData() { Type = PropertyType.DATE, SchemaName = "ceterms:alignmentDate", Source = "AlignmentDate", Label = "Alignment Date" };
		public PropertyData AlignmentType = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:alignmentType", Source = "AlignmentType", Label = "Alignment Type" };
		public PropertyData AlternateName = new PropertyData() {
			Type = PropertyType.TEXT,
			SchemaName = "ceterms:alternateName",
			Source = "Auto_AlternateName",
			Label = "Alternate Name",
			SourceType = SourceType.FROM_OBJECT_LIST,
			InnerSource = "TextValue" };
		public PropertyData AlternativeIdentifier = new PropertyData() {
			Type = PropertyType.PROFILE_LIST,
			SchemaName = "ceterms:alternativeIdentifier",
			Source = "Auto_AlternativeIdentifier",
			Label = "Alternative Identifier",
			ProfileType = typeof( IdentifierValue )
		};
		public PropertyData AppealProcess = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:appealProcess", Source = "AppealProcess", Label = "Appeal Process", ProfileType = typeof( ProcessProfile ) };

		public PropertyData ComplaintProcess = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:complaintProcess", Source = "ComplaintProcess", Label = "Complaint Process", ProfileType = typeof( ProcessProfile ) };
		public PropertyData ReviewProcess = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:reviewProcess", Source = "ReviewProcess", Label = "Review Process", ProfileType = typeof( ProcessProfile ) };

		public PropertyData RevocationProcess = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:revocationProcess", Source = "RevocationProcess", Label = "Revocation Process", ProfileType = typeof( ProcessProfile ) };


		public PropertyData ApprovedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:approvedBy", Source = "OrganizationRole", Label = "Approved By" };
		public PropertyData ApprovedIn = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:approvedIn", Source = "ApprovedIn", Label = "Approved In", ProfileType = typeof( JurisdictionProfile ) };
		public PropertyData Approves = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:approves", Source = "Approves", Label = "Approves", ProfileType = typeof( JsonLDIdentifier ) };

		public PropertyData AssertedBy = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:assertedBy", Source = "Auto_AssertedBy", Label = "Asserted By", ProfileType = typeof( JsonLDIdentifier ), SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };

		public PropertyData ConditionManifestOf = new PropertyData() { Type = PropertyType.PROFILE_EXTERNAL_LIST,
			SchemaName = "ceterms:conditionManifestOf",
			Source = "Auto_OrgURI",
			Label = "Condition Manifest Of",
			ProfileType = typeof( JsonLDIdentifier ),
			SourceType = SourceType.DIRECT };

		public PropertyData CostManifestOf = new PropertyData() { Type = PropertyType.PROFILE_EXTERNAL_LIST,
			SchemaName = "ceterms:costManifestOf",
			Source = "Auto_OrgURI",
			Label = "Cost Manifest Of",
			ProfileType = typeof( JsonLDIdentifier ),
			SourceType = SourceType.DIRECT };

		public PropertyData AudienceLevelType = new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:audienceLevelType", Source = "AudienceLevelType", Label = "Audience Level Type" };
		public PropertyData AudienceType = new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:audienceType", Source = "AudienceType", Label = "Audience Type" };
		public PropertyData AvailabilityListing = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:availabilityListing", Source = "Auto_AvailabilityListing", Label = "Availability Listing", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData AvailableAt = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:availableAt", Source = "AvailableAt", Label = "Available At", ProfileType = typeof( GeoCoordinates ) };
		public PropertyData AvailableOnlineAt = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:availableOnlineAt", Source = "Auto_AvailableOnlineAt", Label = "Available Online At", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData BroadAlignment = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:broadAlignment", Source = "BroadAlignment", Label = "Broad Alignment", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData CodedNotation = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:codedNotation", Source = "Auto_CodedNotation", Label = "Coded Notation", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		//public PropertyData ContactPoint = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:contactPoint", Source = "ContactPoint", Label = "Contact Point", ProfileType = typeof( ContactPoint ) };
		public PropertyData Condition = new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ceterms:condition", Source = "Condition", Label = "Condition" };
		public PropertyData CopyrightHolder = new PropertyData() { Type = PropertyType.PROFILE_EXTERNAL_LIST, SchemaName = "ceterms:copyrightHolder", Source = "CopyrightHolderOrganization", Label = "Copyright Holder", SourceType = SourceType.FROM_OBJECT };

		public PropertyData Corequisite = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:corequisite", Source = "Corequisite", Label = "Corequisite", ProfileType = typeof( ConditionProfile ) };
		public PropertyData CommonConditions = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:commonConditions", Source = "CommonConditions", Label = "Common Conditions", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData CostDetails = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:costDetails", Source = "CostDetails", Label = "Cost Details" };
		public PropertyData CredentialIdentifier = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:credentialId", Source = "CredentialId", Label = "Credential Identifier" };
		public PropertyData CredentialingAction = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:credentialingAction", Source = "CredentialingAction", Label = "Credentialing Action", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData CredentialProfiled = new PropertyData() {
			Type = PropertyType.PROFILE_LIST,
			SchemaName = "ceterms:credentialProfiled",
			Source = "CredentialProfiled",
			Label = "Credential Profiled", ProfileType = typeof( JsonLDIdentifier )
		};
		public PropertyData CredentialStatusType = new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:credentialStatusType", Source = "CredentialStatusType", Label = "Credential Status Type" };
		public PropertyData CreditHourType = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:creditHourType", Source = "CreditHourType", Label = "Credit Hour Type" };
		public PropertyData CreditHourValue = new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:creditHourValue", Source = "CreditHourValue", Label = "Credit Hour Value" };
		public PropertyData CreditUnitType = new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:creditUnitType", Source = "CreditUnitType", Label = "Credit Unit Type" };
		public PropertyData CreditUnitTypeDescription = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:creditUnitTypeDescription", Source = "CreditUnitTypeDescription", Label = "Credit Unit Type Description" };
		public PropertyData CreditUnitValue = new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:creditUnitValue", Source = "CreditUnitValue", Label = "Credit Unit Value" };
		public PropertyData CTID = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:ctid", Source = "CTID", Label = "CTID" };
		public PropertyData DateEffective = new PropertyData() { Type = PropertyType.DATE, SchemaName = "ceterms:dateEffective", Source = "DateEffective", Label = "Date Effective" };
		public PropertyData DegreeConcentration = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:degreeConcentration", Source = "Auto_DegreeConcentration", Label = "Degree Concentration", ProfileType = typeof( CredentialAlignmentObject ) };
		public PropertyData DegreeMajor = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:degreeMajor", Source = "Auto_DegreeMajor", Label = "Degree Major", ProfileType = typeof( CredentialAlignmentObject ) };
		public PropertyData DegreeMinor = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:degreeMinor", Source = "Auto_DegreeMinor", Label = "Degree Minor", ProfileType = typeof( CredentialAlignmentObject ) };
		public PropertyData DeliveryType = new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:deliveryType", Source = "DeliveryType", Label = "Delivery Type" };
		public PropertyData DeliveryTypeDescription = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:deliveryTypeDescription", Source = "DeliveryTypeDescription", Label = "Delivery Type Description" };
		//public PropertyData Department = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:department", Source = "Department", Label = "Department", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData Department = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:department", Source = "Auto_OrganizationRole_Dept", Label = "Department", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };

		public PropertyData Description = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:description", Source = "Description", Label = "Description" };
		public PropertyData DevelopmentProcess = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:developmentProcess", Source = "DevelopmentProcess", Label = "Development Process", ProfileType = typeof( ProcessProfile ) };
		public PropertyData DUNS = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:duns", Source = "ID_DUNS", Label = "DUNS" };
		public PropertyData Earnings = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:earnings", Source = "Earnings", Label = "Earnings", ProfileType = typeof( EarningsProfile ) };
		public PropertyData Email = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:email", Source = "Auto_Email", Label = "Email", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData Employee = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:employee", Source = "Employee", Label = "Employee", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData EmploymentOutcome = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:employmentOutcome", Source = "EmploymentOutcome", Label = "Employment Outcome", ProfileType = typeof( EmploymentOutcomeProfile ) };
		public PropertyData EndTime = new PropertyData() { Type = PropertyType.DATETIME, SchemaName = "ceterms:endTime", Source = "EndTime", Label = "End Time" };
		public PropertyData EndDate = new PropertyData() { Type = PropertyType.DATE, SchemaName = "ceterms:endDate", Source = "EndDate", Label = "End Date" };
		public PropertyData EntryCondition = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:entryCondition", Source = "EntryCondition", Label = "Entry Condition", ProfileType = typeof( ConditionProfile ) };

		public PropertyData EstimatedCost = new PropertyData() {
			Type = PropertyType.PROFILE_LIST,
			SchemaName = "ceterms:estimatedCost",
			Source = "EstimatedCost_Merged",
			Label = "Estimated Cost",
			ProfileType = typeof( CostProfile )
		};

		public PropertyData EstimatedDuration = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:estimatedDuration", Source = "EstimatedDuration", Label = "Estimated Duration", ProfileType = typeof( DurationProfile ) };
		public PropertyData ExactAlignment = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:exactAlignment", Source = "ExactAlignment", Label = "Exact Alignment", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData FEIN = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:fein", Source = "ID_FEIN", Label = "FEIN" };
		public PropertyData FinancialAssistance = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:financialAssistance", Source = "FinancialAssistance", ProfileType = typeof( FinancialAlignmentObject ) };
		public PropertyData FoundingDate = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:foundingDate", Source = "FoundingDate", Label = "Founding Date" };
		public PropertyData FrameworkURL = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:framework", Source = "FrameworkUrl", Label = "Framework URL" };
		public PropertyData FrameworkName = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:frameworkName", Source = "FrameworkName", Label = "Framework Name" };
		public PropertyData HasConditionManifest = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:hasConditionManifest",
			Source = "HasConditionManifest",
			Label = "Has Condition Manifest",
			ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData HasPart = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:hasPart", Source = "HasPart", Label = "Has Part", ProfileType = typeof( JsonLDIdentifier ) };

		public PropertyData HasVerificationService = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:hasVerificationService", Source = "VerificationServiceProfiles", Label = "Has Verification Service", ProfileType = typeof( VerificationServiceProfile ) };

		public PropertyData Holders = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:holders", Source = "Holders", Label = "Holders", ProfileType = typeof( HoldersProfile ) };
		public PropertyData Image = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:image", Source = "Auto_ImageUrl", Label = "Image URL", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData IndustryType = new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:industryType", Source = "IndustryType", Label = "Industry Type" };
		public PropertyData InLanguage = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:inLanguage", Source = "Auto_InLanguageCode", Label = "In Language", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };

		public PropertyData InstructionalProgramType = new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:instructionalProgramType", Source = "InstructionalProgramType", Label = "Instructional Program Type" };

		public PropertyData Instrument = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:instrument", Source = "Instrument", Label = "Instrument", ProfileType = typeof( JsonLDIdentifier ) };

		public PropertyData IPEDSID = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:ipedsID", Source = "ID_IPEDSID", Label = "iPEDS ID" };
		public PropertyData IsAdvancedStandingFor = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:isAdvancedStandingFor", Source = "IsAdvancedStandingFor", Label = "Is Advanced Standing For", ProfileType = typeof( ConditionProfile ) };
		public PropertyData IsPartOf = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:isPartOf", Source = "IsPartOf", Label = "Is Part Of", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData IsPreparationFor = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:isPreparationFor", Source = "IsPreparationFor", Label = "Is Preparation For", ProfileType = typeof( ConditionProfile ) };
		public PropertyData IsRecommendedFor = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:isRecommendedFor", Source = "IsRecommendedFor", Label = "Is Recommended For", ProfileType = typeof( ConditionProfile ) };
		public PropertyData IsRequiredFor = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:isRequiredFor", Source = "IsRequiredFor", Label = "Is Required For", ProfileType = typeof( ConditionProfile ) };
		public PropertyData Jurisdiction = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:jurisdiction", Source = "Jurisdiction", Label = "Jurisdiction", ProfileType = typeof( JurisdictionProfile ) };
		public PropertyData Keyword = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:keyword", Source = "Keyword", Label = "Keywords", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData LatestVersion = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:latestVersion", Source = "Auto_LatestVersion", Label = "Latest Version", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData LearningOpportunityOffered = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:learningOpportunityOffered", Source = "LearningOpportunityOffered", Label = "Learning Opportunity Offered", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData MaintenanceProcess = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:maintenanceProcess", Source = "MaintenanceProcess", Label = "Maintenance Process", ProfileType = typeof( ProcessProfile ) };
		public PropertyData MajorAlignment = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:majorAlignment", Source = "MajorAlignment", Label = "Major Alignment", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData MaximumDuration = new PropertyData() { Type = PropertyType.DURATION, SchemaName = "ceterms:maximumDuration", Source = "MaximumDuration", Label = "Maximum Duration" };
		public PropertyData MinorAlignment = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:minorAlignment", Source = "MinorAlignment", Label = "Minor Alignment", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData MissionAndGoalsStatement = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:missionAndGoalsStatement", Source = "MissionAndGoalsStatement", Label = "Mission and Goals Statement" };
		public PropertyData MissionAndGoalsStatementDescription = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:missionAndGoalsStatementDescription", Source = "MissionAndGoalsStatementDescription", Label = "Mission and Goals Statement Description" };
		public PropertyData NAICS = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:naics", Source = "NAICS", Label = "NAICS" };
		public PropertyData Name = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:name", Source = "Name", Label = "Name" };
		public PropertyData NarrowAlignment = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:narrowAlignment", Source = "NarrowAlignment", Label = "Narrow Alignment", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData Object = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:object", Source = "Object", Label = "Object", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData OccupationType = new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:occupationType", Source = "OccupationType", Label = "Occupation Type" };
		public PropertyData OfferedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:offeredBy", Source = "OrganizationRole", Label = "Offered By" };
		public PropertyData OfferedIn = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:offeredIn", Source = "OfferedIn", Label = "Offered In", ProfileType = typeof( JurisdictionProfile ) };
		public PropertyData Offers = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:offers", Source = "Offers", Label = "Offers", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData OPEID = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:opeID", Source = "ID_OPEID", Label = "OPE ID" };

		public PropertyData OwnedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:ownedBy", Source = "OrganizationRole", Label = "Owned By" };

		public PropertyData Owns = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:owns", Source = "Owns", Label = "Owns", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData ParentOrganization = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:parentOrganization", Source = "ParentOrganization", Label = "Parent Organization", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData Participant = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:participant", Source = "Participant", Label = "Participant", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData PreparationFrom = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:preparationFrom", Source = "PreparationFrom", Label = "Preparation From", ProfileType = typeof( ConditionProfile ) };
		public PropertyData PreviousVersion = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:previousVersion", Source = "Auto_PreviousVersion", Label = "Previous Version", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData ProcessStandards = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:processStandards", Source = "ProcessStandards", Label = "Process Standards" };
		public PropertyData ProcessStandardsDescription = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:processStandardsDescription", Source = "ProcessStandardsDescription", Label = "Process Standards Description" };
		public PropertyData PurposeType = new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:purposeType", Source = "PurposeType", Label = "Purpose Type" };
		public PropertyData RecognizedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:recognizedBy", Source = "OrganizationRole", Label = "Recognized By" };
		public PropertyData RecognizedIn = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:recognizedIn", Source = "RecognizedIn", Label = "Recognized In", ProfileType = typeof( JurisdictionProfile ) };
		public PropertyData Recognizes = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:recognizes", Source = "Recognizes", Label = "Recognizes", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData Recommends = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:recommends", Source = "Recommends", Label = "Recommends", ProfileType = typeof( ConditionProfile ) };
		public PropertyData Region = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:region", Source = "Region", Label = "Region", ProfileType = typeof( GeoCoordinates ) };
		public PropertyData RegulatedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:regulatedBy", Source = "OrganizationRole", Label = "Regulated By" };
		public PropertyData RegulatedIn = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:regulatedIn", Source = "RegulatedIn", Label = "Regulated In", ProfileType = typeof( JurisdictionProfile ) };
		public PropertyData RelatedAction = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:relatedAction", Source = "RelatedAction", Label = "Related Action", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData Renewal = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:renewal", Source = "Renewal", Label = "Renewal", ProfileType = typeof( ConditionProfile ) };
		public PropertyData RenewedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:renewedBy", Source = "OrganizationRole", Label = "Renewed By" };
		public PropertyData RenewedIn = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:renewedIn", Source = "RenewedIn", Label = "Renewed In", ProfileType = typeof( JurisdictionProfile ) };
		public PropertyData Renews = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:renews", Source = "Renews", Label = "Renews", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData Requires = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:requires", Source = "Requires", Label = "Requires", ProfileType = typeof( ConditionProfile ) };
		public PropertyData ResultingAward = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:resultingAward", Source = "ResultingAward", Label = "Resulting Award", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData Revocation = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:revocation", Source = "Revocation", Label = "Revocation", ProfileType = typeof( RevocationProfile ) };
		public PropertyData RevokedBy = new PropertyData() { Type = PropertyType.ROLE, SchemaName = "ceterms:revokedBy", Source = "OrganizationRole", Label = "Revoked By" };
		public PropertyData RevokedIn = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:revokedIn", Source = "RevokedIn", Label = "Revoked In", ProfileType = typeof( JurisdictionProfile ) };
		public PropertyData Revokes = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:revokes", Source = "Revokes", Label = "Revokes", ProfileType = typeof( JsonLDIdentifier ) };
		//public PropertyData SameAs = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:sameAs", Source = "SameAs", Label = "Same As", ProfileType = typeof( ERROR ) };
		public PropertyData ScoringMethodDescription = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:scoringMethodDescription", Source = "ScoringMethodDescription", Label = "Scoring Method Description" };
		public PropertyData ScoringMethodExample = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:scoringMethodExample", Source = "ScoringMethodExample", Label = "Scoring Method Example" };
		public PropertyData ScoringMethodExampleDescription = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:scoringMethodExampleDescription", Source = "ScoringMethodExampleDescription", Label = "Scoring Method Example Description" };
		public PropertyData ServiceType = new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:serviceType", Source = "ServiceType", Label = "Service Type" };
		public PropertyData SocialMedia = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:socialMedia", Source = "Auto_SocialMedia", Label = "Social Media", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData SourceURL = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:source", Source = "SourceURL", Label = "Source URL" };
		public PropertyData StartTime = new PropertyData() { Type = PropertyType.DATETIME, SchemaName = "ceterms:startTime", Source = "StartTime", Label = "Start Time" };
		public PropertyData StartDate = new PropertyData() { Type = PropertyType.DATE, SchemaName = "ceterms:startDate", Source = "StartDate", Label = "Start Date" };
		public PropertyData Subject = new PropertyData() {
			Type = PropertyType.PROFILE_LIST,
			SchemaName = "ceterms:subject",
			Source = "Auto_Subject",
			Label = "Subjects",
			ProfileType = typeof( CredentialAlignmentObject )
		};
		public PropertyData SubjectWebpage = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:subjectWebpage", Source = "Auto_SubjectWebpage", Label = "Subject Webpage", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		//public PropertyData SubOrganization = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:subOrganization", Source = "SubOrganization", Label = "Sub-Organization", ProfileType = typeof( JsonLDIdentifier ) };
		public PropertyData SubOrganization = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:subOrganization", Source = "Auto_OrganizationRole_SubOrganization", Label = "Sub-Organization", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" };
		public PropertyData TargetAssessment = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:targetAssessment", Source = "TargetAssessment", Label = "Target Assessment", ProfileType = typeof( JsonLDIdentifier ) };

		public PropertyData TargetCompetency = new PropertyData() { Type = PropertyType.PROFILE_LIST,
			SchemaName = "ceterms:targetCompetency",
			Source = "TargetCompetency",
			Label = "Target Competency",
			ProfileType = typeof( CredentialAlignmentObject ) };

		public PropertyData TeachesCompetency = new PropertyData()
		{
			Type = PropertyType.PROFILE_LIST,
			SchemaName = "ceterms:teaches",
			Source = "TargetCompetency",
			Label = "Target Competency",
			ProfileType = typeof( CredentialAlignmentObject )
		};

		public PropertyData TargetContactPoint = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:targetContactPoint", Source = "Auto_TargetContactPoint", Label = "Target Contact Point", ProfileType = typeof( ContactPoint ) };
		public PropertyData TargetCredential = new PropertyData() {
			Type = PropertyType.PROFILE_LIST,
			SchemaName = "ceterms:targetCredential",
			Source = "TargetCredential",
			Label = "Target Credential",
			ProfileType = typeof( JsonLDIdentifier )
		};
		public PropertyData TargetLearningOpportunity = new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:targetLearningOpportunity", Source = "TargetLearningOpportunity", Label = "Target Learning Opportunity", ProfileType = typeof( JsonLDIdentifier ) };

		public PropertyData TargetNode = new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:targetNode", Source = "TargetNode", Label = "Target Node" };
		public PropertyData TargetDescription = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:targetNodeDescription", Source = "TargetNodeDescription", Label = "Target Description" };
		public PropertyData TargetNodeName = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:targetNodeName", Source = "TargetNodeName", Label = "Target Node Name" };
		public PropertyData ValidationMethodDescription = new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:verificationMethodDescription", Source = "VerificationMethodDescription", Label = "Method Description" };
		public PropertyData VersionIdentifier = new PropertyData() {
			Type = PropertyType.PROFILE_LIST,
			SchemaName = "ceterms:versionIdentifier",
			Source = "Auto_VersionIdentifier",
			Label = "Version Identifier",
			ProfileType = typeof( IdentifierValue ) };
		public PropertyData Weight = new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:weight", Source = "Weight", Label = "Weight" };
	}
	public class AssessmentProfile : JsonLDDocument
	{
		public AssessmentProfile()
		{
			Type = "ceterms:AssessmentProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.AccreditedBy,
				CommonProperties.AccreditedIn,
				CommonProperties.AdministrationProcess,
				CommonProperties.AppealProcess,
				CommonProperties.ComplaintProcess,
				CommonProperties.ReviewProcess,
				CommonProperties.RevocationProcess,
				CommonProperties.AdvancedStandingFrom,
				CommonProperties.ApprovedBy,
				CommonProperties.ApprovedIn,
				CommonProperties.AvailabilityListing,
				CommonProperties.AvailableAt,
				CommonProperties.AvailableOnlineAt,
				CommonProperties.CodedNotation,
				CommonProperties.CommonConditions,
				CommonProperties.Corequisite,
				CommonProperties.CreditHourType,
				CommonProperties.CreditHourValue,
				CommonProperties.CreditUnitType,
				CommonProperties.CreditUnitTypeDescription,
				CommonProperties.CreditUnitValue,
				CommonProperties.CTID,
				CommonProperties.DateEffective,
				CommonProperties.DeliveryType,
				CommonProperties.DeliveryTypeDescription,
				CommonProperties.Description,
				CommonProperties.DevelopmentProcess,
				CommonProperties.EntryCondition,
				CommonProperties.EstimatedCost,
				CommonProperties.EstimatedDuration,
				CommonProperties.FinancialAssistance,
				CommonProperties.InLanguage,
				CommonProperties.InstructionalProgramType,
				CommonProperties.IsAdvancedStandingFor,
				CommonProperties.IsPreparationFor,
				CommonProperties.IsRecommendedFor,
				CommonProperties.IsRequiredFor,
				CommonProperties.Jurisdiction,
				CommonProperties.Keyword,
				CommonProperties.MaintenanceProcess,
				CommonProperties.Name,
				CommonProperties.OfferedBy,
				CommonProperties.OfferedIn,
				CommonProperties.OwnedBy,
				CommonProperties.PreparationFrom,
				CommonProperties.ProcessStandards,
				CommonProperties.ProcessStandardsDescription,
				CommonProperties.RecognizedBy,
				CommonProperties.RecognizedIn,
				CommonProperties.Recommends,
				CommonProperties.Region,
				CommonProperties.RegulatedBy,
				CommonProperties.RegulatedIn,
				CommonProperties.Requires,
				CommonProperties.ScoringMethodDescription,
				CommonProperties.ScoringMethodExample,
				CommonProperties.ScoringMethodExampleDescription,
				CommonProperties.Subject,
				CommonProperties.SubjectWebpage,
				CommonProperties.TargetAssessment,
				CommonProperties.TargetCompetency,
				CommonProperties.ValidationMethodDescription,
				CommonProperties.VersionIdentifier,
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:assessmentExample", Source = "AssessmentExample", Label = "Assessment Example" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:assessmentExampleDescription", Source = "AssessmentExampleDescription", Label = "Assessment Example Description" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:assessmentMethodType", Source = "AssessmentMethodType", Label = "Assessment Method Type" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:assessmentOutput", Source = "AssessmentOutput", Label = "Assessment Output" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:assessmentUseType", Source = "AssessmentUseType", Label = "Assessment Use Type" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:externalResearch", Source = "Auto_ExternalResearch", Label = "External Research", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.BOOLEAN, SchemaName = "ceterms:hasGroupEvaluation", Source = "HasGroupEvaluation", Label = "Has Group Evaluation" },
				new PropertyData() { Type = PropertyType.BOOLEAN, SchemaName = "ceterms:hasGroupParticipation", Source = "HasGroupParticipation", Label = "Has Group Participation" },
				new PropertyData() { Type = PropertyType.BOOLEAN, SchemaName = "ceterms:isProctored", Source = "IsProctored", Label = "Is Proctored" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:scoringMethodType", Source = "ScoringMethodType", Label = "Scoring Method Type" },
			};
		}
	}
	public class ConditionManifest :JsonLDDocument
	{
		public ConditionManifest()
		{
			Type = "ceterms:ConditionManifest";
			Properties = new List<PropertyData>()
			{
				CommonProperties.CTID,
				CommonProperties.Description,
				CommonProperties.EntryCondition,
				CommonProperties.Corequisite,
				CommonProperties.ConditionManifestOf,
				CommonProperties.Name,
				CommonProperties.Recommends,
				CommonProperties.Requires,
				CommonProperties.SubjectWebpage
			};
		}
	}
	public class ConditionProfile : JsonLDObject
	{
		public ConditionProfile()
		{
			Type = "ceterms:ConditionProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.AssertedBy,
				CommonProperties.AudienceLevelType,
				CommonProperties.AudienceType,
				CommonProperties.Condition,
				CommonProperties.CredentialProfiled,
				CommonProperties.CreditHourType,
				CommonProperties.CreditHourValue,
				CommonProperties.CreditUnitType,
				CommonProperties.CreditUnitTypeDescription,
				CommonProperties.CreditUnitValue,
				CommonProperties.DateEffective,
				CommonProperties.Description,
				CommonProperties.EstimatedCost,
				CommonProperties.Jurisdiction,
				CommonProperties.Name,
				CommonProperties.TargetAssessment,
				CommonProperties.TargetCompetency,
				CommonProperties.TargetCredential,
				CommonProperties.TargetLearningOpportunity,
				CommonProperties.Weight,
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:additionalCondition", Source = "AdditionalCondition", Label = "Additional Condition", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:alternativeCondition", Source = "AlternativeCondition", Label = "Alternative Condition", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:experience", Source = "Experience", Label = "Experience" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:minimumAge", Source = "MinimumAge", Label = "Minimum Age" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:residentOf", Source = "ResidentOf", Label = "Resident Of", ProfileType = typeof( JurisdictionProfile ) },
				new PropertyData() { Type = PropertyType.TEXTVALUE_LIST, SchemaName = "ceterms:submissionOf", Source = "SubmissionOf", Label = "Submission Of" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:targetPathway", Source = "CareerPathway", Label = "Career Pathway", ProfileType = typeof( JsonLDIdentifier ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:targetTask", Source = "TargetTask", Label = "Target Task", ProfileType = typeof( TaskProfile ) },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:yearsOfExperience", Source = "YearsOfExperience", Label = "Years of Experience" },
			};
		}
	}
	public class ContactPoint : JsonLDObject
	{
		public ContactPoint()
		{
			Type = "ceterms:ContactPoint";
			Properties = new List<PropertyData>()
			{
				CommonProperties.Email,
				CommonProperties.Name,
				CommonProperties.SocialMedia,
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:contactOption", Source = "Auto_ContactOption", Label = "Contact Option", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:contactType", Source = "ContactType", Label = "Contact Type" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:faxNumber", Source = "Auto_FaxNumber", Label = "Fax Number", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:telephone", Source = "Auto_Telephone", Label = "Telephone", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
			};
		}
	}
	public class CostManifest : JsonLDDocument
	{
		public CostManifest()
		{
			Type = "ceterms:CostManifest";
			Properties = new List<PropertyData>()
			{
				CommonProperties.CostDetails,
				CommonProperties.CTID,
				CommonProperties.Description,
				CommonProperties.EndDate,
				CommonProperties.EstimatedCost,
				CommonProperties.CostManifestOf,
				CommonProperties.Name,
				CommonProperties.StartDate
			};
		}
	}
	public class CostProfile : JsonLDObject
	{
		public CostProfile()
		{
			Type = "ceterms:CostProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.AudienceType,
				CommonProperties.Condition,
				CommonProperties.CostDetails,
				CommonProperties.Description,
				CommonProperties.EndDate,
				CommonProperties.Jurisdiction,
				CommonProperties.Name,
				CommonProperties.Region,
				CommonProperties.StartDate,
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:assessmentProfiled", Source = "AssessmentProfile", Label = "Assessment Profile", ProfileType = typeof( JsonLDIdentifier ) },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:currency", Source = "Currency", Label = "Currency" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:directCostType", Source = "CostType", Label = "Cost Type" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:learningOpportunityProfiled", Source = "LearningOpportunityProfiled", Label = "Learning Opportunity Profiled", ProfileType = typeof( JsonLDIdentifier ) },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:paymentPattern", Source = "PaymentPattern", Label = "Payment Pattern" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:price", Source = "Price", Label = "Price" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:residencyType", Source = "ResidencyType", Label = "Residency Type" },
			};
		}
	}
	public class Credential : JsonLDDocument
	{
		public Credential()
		{
			Type = "ceterms:Credential";
			Properties = new List<PropertyData>()
			{
				CommonProperties.AccreditedBy,
				CommonProperties.AccreditedIn,
				CommonProperties.AdministrationProcess,
				CommonProperties.AppealProcess,
				CommonProperties.ComplaintProcess,
				CommonProperties.ReviewProcess,
				CommonProperties.RevocationProcess,
				CommonProperties.AdvancedStandingFrom,
				CommonProperties.AlternateName,
				CommonProperties.ApprovedBy,
				CommonProperties.ApprovedIn,
				CommonProperties.AudienceLevelType,
				CommonProperties.AvailabilityListing,
				CommonProperties.AvailableAt,
				CommonProperties.AvailableOnlineAt,
				CommonProperties.BroadAlignment,
				CommonProperties.CodedNotation,
				CommonProperties.CommonConditions,
				CommonProperties.CopyrightHolder,
				CommonProperties.Corequisite,
				CommonProperties.CredentialIdentifier,
				CommonProperties.CredentialStatusType,
				CommonProperties.CTID,
				CommonProperties.DateEffective,
				CommonProperties.Description,
				CommonProperties.DevelopmentProcess,
				CommonProperties.DegreeConcentration,
				CommonProperties.DegreeMajor,
				CommonProperties.DegreeMinor,
				CommonProperties.Earnings,
				CommonProperties.EmploymentOutcome,
				CommonProperties.EstimatedCost,
				CommonProperties.EstimatedDuration,
				CommonProperties.ExactAlignment,
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:hasPart", Source = "EmbeddedCredentials", Label = "Embedded Credential", SourceType = SourceType.FROM_OBJECT_LIST, InnerSource = "ctid" },
				CommonProperties.FinancialAssistance,
				CommonProperties.Holders,
				CommonProperties.Image,
				CommonProperties.IndustryType,
				CommonProperties.InLanguage,
				CommonProperties.IsAdvancedStandingFor,
				CommonProperties.IsPartOf,
				CommonProperties.IsPreparationFor,
				CommonProperties.IsRecommendedFor,
				CommonProperties.IsRequiredFor,
				CommonProperties.Jurisdiction,
				CommonProperties.Keyword,
				CommonProperties.LatestVersion,
				CommonProperties.MaintenanceProcess,
				CommonProperties.MajorAlignment,
				CommonProperties.MaximumDuration,
				CommonProperties.MinorAlignment,
				CommonProperties.Name,
				CommonProperties.NarrowAlignment,
				CommonProperties.OccupationType,
				CommonProperties.OfferedBy,
				CommonProperties.OfferedIn,
				CommonProperties.OwnedBy,
				CommonProperties.PreparationFrom,
				CommonProperties.PreviousVersion,
				CommonProperties.ProcessStandards,
				CommonProperties.ProcessStandardsDescription,
				CommonProperties.PurposeType,
				CommonProperties.RecognizedBy,
				CommonProperties.RecognizedIn,
				CommonProperties.Recommends,
				CommonProperties.Region,
				CommonProperties.RegulatedBy,
				CommonProperties.RegulatedIn,
				CommonProperties.RelatedAction,
				CommonProperties.Renewal,
				CommonProperties.RenewedBy,
				CommonProperties.RenewedIn,
				CommonProperties.Requires,
				CommonProperties.Revocation,
				CommonProperties.RevokedBy,
				CommonProperties.RevokedIn,
				CommonProperties.Subject,
				CommonProperties.SubjectWebpage,
				CommonProperties.VersionIdentifier,
				new PropertyData() { Type = PropertyType.PARENT_TYPE_OVERRIDE, SchemaName = "ceterms:", Source = "CredentialType", Label = "Credential Type", SourceType = SourceType.FROM_ENUMERATION },
			};
		}
	}
	public class CredentialAlignmentObject : JsonLDObject
	{
		public CredentialAlignmentObject()
		{
			Type = "ceterms:CredentialAlignmentObject";
			Properties = new List<PropertyData>()
			{
				CommonProperties.AlignmentDate,
				CommonProperties.AlignmentType,
				CommonProperties.CodedNotation,
				CommonProperties.FrameworkURL,
				CommonProperties.FrameworkName,
				CommonProperties.TargetNode,
				CommonProperties.TargetDescription,
				CommonProperties.TargetNodeName,
				CommonProperties.Weight,
			};
		}
	}
	public class CredentialingAction : JsonLDObject
	{
		public CredentialingAction()
		{
			Type = "ceterms:CredentialingAction";
			Properties = new List<PropertyData>()
			{
				CommonProperties.ActingAgent,
				//CommonProperties.ActionStatusType,
				CommonProperties.Agent,
				CommonProperties.Description,
				CommonProperties.EndDate,
				CommonProperties.Instrument,
				CommonProperties.Object,
				CommonProperties.Participant,
				CommonProperties.ResultingAward,
				CommonProperties.StartDate,
			};
		}
	}
	public class CredentialOrganization : JsonLDDocument
	{
		public CredentialOrganization()
		{
			Type = "ceterms:CredentialOrganization";
			Properties = new List<PropertyData>()
			{
				CommonProperties.AccreditedBy,
				CommonProperties.AccreditedIn,
				CommonProperties.Address,
				CommonProperties.AdministrationProcess,
				CommonProperties.AppealProcess,
				CommonProperties.ComplaintProcess,
				CommonProperties.ReviewProcess,
				CommonProperties.RevocationProcess,
				CommonProperties.AgentPurpose,
				CommonProperties.AgentPurposeDescription,
				CommonProperties.AgentSectorType,
				CommonProperties.AgentType,
				CommonProperties.AlternativeIdentifier,
				CommonProperties.ApprovedBy,
				CommonProperties.ApprovedIn,
				CommonProperties.Approves,
				CommonProperties.AvailabilityListing,
				//CommonProperties.ContactPoint,
				CommonProperties.CredentialingAction,
				CommonProperties.CTID,
				CommonProperties.Department,
				CommonProperties.Description,
				CommonProperties.DevelopmentProcess,
				CommonProperties.DUNS,
				CommonProperties.Email,
				CommonProperties.Employee,
				CommonProperties.FEIN,
				CommonProperties.FoundingDate,
				CommonProperties.HasConditionManifest,
				CommonProperties.HasVerificationService,
				CommonProperties.Image,
				CommonProperties.IndustryType,
				CommonProperties.IPEDSID,
				CommonProperties.Jurisdiction,
				CommonProperties.Keyword,
				CommonProperties.LearningOpportunityOffered,
				CommonProperties.MaintenanceProcess,
				CommonProperties.MissionAndGoalsStatement,
				CommonProperties.MissionAndGoalsStatementDescription,
				CommonProperties.NAICS,
				CommonProperties.Name,
				CommonProperties.Offers,
				CommonProperties.OPEID,
				CommonProperties.Owns,
				CommonProperties.ParentOrganization,
				CommonProperties.RecognizedBy,
				CommonProperties.RecognizedIn,
				CommonProperties.Recognizes,
				CommonProperties.RegulatedBy,
				CommonProperties.RegulatedIn,
				CommonProperties.Renews,
				CommonProperties.Revokes,
				//CommonProperties.SameAs,
				CommonProperties.ServiceType,
				CommonProperties.SocialMedia,
				CommonProperties.SubjectWebpage,
				CommonProperties.SubOrganization,
				CommonProperties.TargetContactPoint,
			};
		}
	}
	public class QACredentialOrganization : JsonLDDocument
	{
		public QACredentialOrganization()
		{
			Type = "ceterms:QACredentialOrganization";
			Properties = new List<PropertyData>()
			{
				CommonProperties.AccreditedBy,
				CommonProperties.AccreditedIn,
				CommonProperties.Address,
				CommonProperties.AdministrationProcess,
				CommonProperties.AppealProcess,
				CommonProperties.ComplaintProcess,
				CommonProperties.ReviewProcess,
				CommonProperties.RevocationProcess,
				CommonProperties.AgentPurpose,
				CommonProperties.AgentPurposeDescription,
				CommonProperties.AgentSectorType,
				CommonProperties.AgentType,
				CommonProperties.AlternativeIdentifier,
				CommonProperties.ApprovedBy,
				CommonProperties.ApprovedIn,
				CommonProperties.Approves,
				CommonProperties.AvailabilityListing,
				//CommonProperties.ContactPoint,
			  	CommonProperties.CTID,
				CommonProperties.CredentialingAction,
				CommonProperties.Department,
				CommonProperties.Description,
				CommonProperties.DevelopmentProcess,
				CommonProperties.DUNS,
				CommonProperties.Email,
				CommonProperties.Employee,
				CommonProperties.FEIN,
				CommonProperties.FoundingDate,
				CommonProperties.HasConditionManifest,
				CommonProperties.HasVerificationService,
				CommonProperties.Image,
				CommonProperties.IndustryType,
				CommonProperties.IPEDSID,
				CommonProperties.Jurisdiction,
				CommonProperties.Keyword,
				CommonProperties.LearningOpportunityOffered,
				CommonProperties.MaintenanceProcess,
				CommonProperties.MissionAndGoalsStatement,
				CommonProperties.MissionAndGoalsStatementDescription,
				CommonProperties.NAICS,
				CommonProperties.Name,
				CommonProperties.Offers,
				CommonProperties.OPEID,
				CommonProperties.Owns,
				CommonProperties.ParentOrganization,
				CommonProperties.RecognizedBy,
				CommonProperties.RecognizedIn,
				CommonProperties.Recognizes,
				CommonProperties.RegulatedBy,
				CommonProperties.RegulatedIn,
				CommonProperties.Renews,
				CommonProperties.Revokes,
				//CommonProperties.SameAs,
				CommonProperties.ServiceType,
				CommonProperties.SocialMedia,
				CommonProperties.SubjectWebpage,
				CommonProperties.SubOrganization,
				CommonProperties.TargetContactPoint,
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:accredits", Source = "Accredits", Label = "Accredits", ProfileType = typeof( JsonLDIdentifier ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:qualityAssuranceTargetType", Source = "QualityAssuranceTargetType", Label = "Quality Assurance Target Type", ProfileType = typeof( JsonLDIdentifier ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:regulates", Source = "Regulates", Label = "Regulates", ProfileType = typeof( JsonLDIdentifier ) },
			};
		}
	}

	public class CredentialPerson : JsonLDObject
	{
		public CredentialPerson()
		{
			Type = "ceterms:CredentialPerson";
			Properties = new List<PropertyData>()
			{
				CommonProperties.AccreditedBy,
				CommonProperties.Address,
				CommonProperties.AgentType,
				CommonProperties.AlternativeIdentifier,
				CommonProperties.ApprovedBy,
				CommonProperties.Approves,
				//CommonProperties.ContactPoint,
				CommonProperties.CredentialingAction,
				CommonProperties.Description,
				CommonProperties.DUNS,
				CommonProperties.Email,
				CommonProperties.FEIN,
				CommonProperties.HasVerificationService,
				CommonProperties.Jurisdiction,
				CommonProperties.Keyword,
				CommonProperties.MissionAndGoalsStatement,
				CommonProperties.MissionAndGoalsStatementDescription,
				CommonProperties.NAICS,
				CommonProperties.Offers,
				CommonProperties.Owns,
				CommonProperties.RecognizedBy,
				CommonProperties.RecognizedIn,
				CommonProperties.Recognizes,
				CommonProperties.RegulatedBy,
				CommonProperties.Renews,
				CommonProperties.Revokes,
				//CommonProperties.SameAs,
				CommonProperties.ServiceType,
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:affiliation", Source = "Affiliation", Label = "Affiliation", ProfileType = typeof( JsonLDIdentifier ) },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:familyName", Source = "FamilyName", Label = "Family Name" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:givenName", Source = "GivenName", Label = "Given Name" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:honorificSuffix", Source = "HonorificSuffix", Label = "Honorific Suffix" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:trainingOffered", Source = "TrainingOffered", Label = "Training Offered", ProfileType = typeof( JsonLDIdentifier ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:worksFor", Source = "WorksFor", Label = "Works For", ProfileType = typeof( JsonLDIdentifier ) },
			};
		}
	}
	public class DurationProfile : JsonLDObject
	{
		public DurationProfile()
		{
			Type = "ceterms:DurationProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.Description,
				CommonProperties.MaximumDuration,
				new PropertyData() { Type = PropertyType.DURATION, SchemaName = "ceterms:exactDuration", Source = "ExactDuration", Label = "Exact Duration" },
				new PropertyData() { Type = PropertyType.DURATION, SchemaName = "ceterms:minimumDuration", Source = "MinimumDuration", Label = "Minimum Duration" },
			};
		}
	}
	public class EarningsProfile : JsonLDObject
	{
		public EarningsProfile()
		{
			Type = "ceterms:EarningsProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.CredentialProfiled,
				CommonProperties.DateEffective,
				CommonProperties.Jurisdiction,
				CommonProperties.Region,
				CommonProperties.SourceURL,
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:highEarnings", Source = "HighEarnings", Label = "High Earnings" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:lowEarnings", Source = "LowEarnings", Label = "Low Earnings" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:medianEarnings", Source = "MedianEarnings", Label = "Median earnings" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:postReceiptMonths", Source = "PostReceiptMonths", Label = "Post Receipt Months" },
			};
		}
	}
	public class EmploymentOutcomeProfile : JsonLDObject
	{
		public EmploymentOutcomeProfile()
		{
			Type = "ceterms:EmploymentOutcomeProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.CredentialProfiled,
				CommonProperties.DateEffective,
				CommonProperties.Description,
				CommonProperties.Jurisdiction,
				CommonProperties.SourceURL,
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:jobsObtained", Source = "JobsObtained", Label = "Jobs Obtained" },
			};
		}
	}
	public class FinancialAlignmentObject : JsonLDObject
	{
		public FinancialAlignmentObject()
		{
			Type = "ceterms:FinancialAlignmentObject";
			Properties = new List<PropertyData>()
			{
				CommonProperties.AlignmentDate,
				CommonProperties.AlignmentType,
				CommonProperties.CodedNotation,
				CommonProperties.FrameworkURL,
				CommonProperties.FrameworkName,
				CommonProperties.TargetNode,
				CommonProperties.TargetDescription,
				CommonProperties.TargetNodeName,
				CommonProperties.Weight,
			};
		}
	}
	public class GeoCoordinates : JsonLDObject
	{
		public GeoCoordinates()
		{
			Type = "ceterms:GeoCoordinates";
			Properties = new List<PropertyData>()
			{
				CommonProperties.Address,
				CommonProperties.Name,
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:geoURI", Source = "GeoURI", Label = "Geographic URI" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:latitude", Source = "Latitude", Label = "Latitude" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:longititude", Source = "Longitude", Label = "Longitude" },
			};
		}
	}
	public class HoldersProfile : JsonLDObject
	{
		public HoldersProfile()
		{
			Type = "ceterms:HoldersProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.CredentialProfiled,
				CommonProperties.DateEffective,
				CommonProperties.Description,
				CommonProperties.Jurisdiction,
				CommonProperties.Region,
				CommonProperties.SourceURL,
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:demographicInformation", Source = "DemographicInformation", Label = "Demographic Information" },
				new PropertyData() { Type = PropertyType.NUMBER, SchemaName = "ceterms:numberAwarded", Source = "NumberAwarded", Label = "Number Awarded" },
			};
		}
	}
	public class IdentifierValue : JsonLDObject
	{
		public IdentifierValue()
		{
			Type = "ceterms:IdentifierValue";
			Properties = new List<PropertyData>()
			{
				CommonProperties.Description,
				CommonProperties.Name,
				new PropertyData() {
					Type = PropertyType.TEXT,
					SchemaName = "ceterms:identifierType",
					Source = "IdentifierType",
					Label = "Identifier Type"
				},
				new PropertyData() {
					Type = PropertyType.TEXT,
					SchemaName = "ceterms:identifierValueCode",
					Source = "IdentifierValueCode",
					Label = "Identifier Value Code"
				},
			};
		}
	}
	public class JurisdictionProfile : JsonLDObject
	{
		public JurisdictionProfile()
		{
			Type = "ceterms:JurisdictionProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.AssertedBy,
				CommonProperties.Description,
				new PropertyData() { Type = PropertyType.BOOLEAN, SchemaName = "ceterms:globalJurisdiction", Source = "GlobalJurisdiction", Label = "Global Jurisdiction" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:jurisdictionException", Source = "JurisdictionException", Label = "Jurisdiction Exception", ProfileType = typeof( GeoCoordinates ) },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:mainJurisdiction", Source = "Auto_MainJurisdiction", Label = "Main Jurisdiction", ProfileType = typeof( GeoCoordinates ) },
			};
		}
	}
	public class LearningOpportunityProfile : JsonLDDocument
	{
		public LearningOpportunityProfile()
		{
			Type = "ceterms:LearningOpportunityProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.AccreditedBy,
				CommonProperties.AccreditedIn,
				CommonProperties.AdvancedStandingFrom,
				CommonProperties.ApprovedBy,
				CommonProperties.ApprovedIn,
				CommonProperties.AvailabilityListing,
				CommonProperties.AvailableAt,
				CommonProperties.AvailableOnlineAt,
				CommonProperties.CodedNotation,
				CommonProperties.CommonConditions,
				CommonProperties.Corequisite,
				CommonProperties.CreditHourType,
				CommonProperties.CreditHourValue,
				CommonProperties.CreditUnitType,
				CommonProperties.CreditUnitTypeDescription,
				CommonProperties.CreditUnitValue,
			  	CommonProperties.CTID,
				CommonProperties.DateEffective,
				CommonProperties.DeliveryType,
				CommonProperties.DeliveryTypeDescription,
				CommonProperties.Description,
				CommonProperties.EntryCondition,
				CommonProperties.EstimatedCost,
				CommonProperties.EstimatedDuration,
				CommonProperties.FinancialAssistance,
				CommonProperties.HasPart,
				CommonProperties.InLanguage,
				CommonProperties.InstructionalProgramType,
				CommonProperties.IsAdvancedStandingFor,
				CommonProperties.IsPreparationFor,
				CommonProperties.IsRecommendedFor,
				CommonProperties.IsRequiredFor,
				CommonProperties.IsPartOf,
				CommonProperties.Jurisdiction,
				CommonProperties.Keyword,
				CommonProperties.Name,
				CommonProperties.OfferedBy,
				CommonProperties.OfferedIn,
				CommonProperties.OwnedBy,
				CommonProperties.PreparationFrom,
				CommonProperties.RecognizedBy,
				CommonProperties.RecognizedIn,
				CommonProperties.Recommends,
				CommonProperties.Region,
				CommonProperties.RegulatedBy,
				CommonProperties.RegulatedIn,
				CommonProperties.Requires,
				CommonProperties.Subject,
				CommonProperties.SubjectWebpage,
				CommonProperties.TargetAssessment,
				CommonProperties.TargetCompetency,
				CommonProperties.TargetLearningOpportunity,
				CommonProperties.ValidationMethodDescription,
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:conditionProfiled", Source = "ConditionProfiled", Label = "Condition Profiled", ProfileType = typeof( ConditionProfile ) },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:learningMethodType", Source = "LearningMethodType", Label = "Learning Method Type" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:targetLearningResource", Source = "TargetLearningResource", Label = "Target Learning Resource", ProfileType = typeof( JsonLDIdentifier ) },
			};
		}
	}
	public class PostalAddress : JsonLDObject
	{
		public PostalAddress()
		{
			Type = "ceterms:PostalAddress";
			Properties = new List<PropertyData>()
			{
				CommonProperties.Name,
				CommonProperties.TargetContactPoint,
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:addressCountry", Source = "AddressCountry", Label = "Address Country" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:addressLocality", Source = "AddressLocality", Label = "Address Locality" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:addressRegion", Source = "AddressRegion", Label = "Address Region" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:postalCode", Source = "PostalCode", Label = "Postal Code" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:postOfficeBoxNumber", Source = "PostOfficeBoxNumber", Label = "Post Office Box Number" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:streetAddress", Source = "StreetAddress", Label = "Street Address" },
			};
		}
	}
	public class ProcessProfile : JsonLDObject
	{
		public ProcessProfile()
		{
			Type = "ceterms:ProcessProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.DateEffective,
				CommonProperties.Description,
				CommonProperties.Jurisdiction,
				CommonProperties.ProcessStandards,
				CommonProperties.ProcessStandardsDescription,
				CommonProperties.Region,
				CommonProperties.ScoringMethodDescription,
				CommonProperties.ScoringMethodExample,
				CommonProperties.ScoringMethodExampleDescription,
				CommonProperties.TargetAssessment,
				CommonProperties.TargetCredential,
				CommonProperties.TargetLearningOpportunity,
				CommonProperties.ValidationMethodDescription,
				new PropertyData() {
					Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST,
					SchemaName = "ceterms:externalInputType",
					Source = "ExternalInputType",
					Label = "External Input Type" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:processFrequency", Source = "ProcessFrequency", Label = "Process Frequency" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:processingAgent", Source = "ProcessingAgent", Label = "Processing Agent", ProfileType = typeof( JsonLDIdentifier ) },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:processMethod", Source = "ProcessMethod", Label = "ProcessMethod" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:processMethodDescription", Source = "ProcessMethodDescription", Label = "Process Method Description" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:processMethodType", Source = "ProcessMethodType", Label = "Process Method Type" },
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:targetCompetencyFramework", Source = "TargetCompetencyFramework", Label = "Target Competency Framework", ProfileType = typeof( JsonLDIdentifier ) },
			};
		}
	}
	
	public class RevocationProfile : JsonLDObject
	{
		public RevocationProfile()
		{
			Type = "ceterms:RevocationProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.CredentialProfiled,
				CommonProperties.DateEffective,
				CommonProperties.Description,
				CommonProperties.Jurisdiction,
				CommonProperties.Region,
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:revocationCriteria", Source = "RevocationCriteria", Label = "Revocation Criteria" },
				new PropertyData() { Type = PropertyType.TEXT, SchemaName = "ceterms:revocationCriteriaDescription", Source = "RevocationCriteria", Label = "Revocation Criteria" },
			};
		}
	}
	public class TaskProfile : JsonLDObject
	{
		public TaskProfile()
		{
			Type = "ceterms:TaskProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.AvailabilityListing,
				CommonProperties.AvailableAt,
				CommonProperties.AvailableOnlineAt,
				CommonProperties.DateEffective,
				CommonProperties.Description,
				CommonProperties.EstimatedCost,
				CommonProperties.EstimatedDuration,
				CommonProperties.Jurisdiction,
				CommonProperties.Name,
				new PropertyData() { Type = PropertyType.PROFILE_LIST, SchemaName = "ceterms:affiliatedAgent", Source = "AffiliatedAgent", Label = "Affiliated Agent", ProfileType = typeof( JsonLDIdentifier ) },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:taskDetails", Source = "TaskDetails", Label = "Task Details" },
			};
		}
	}
	public class VerificationServiceProfile : JsonLDObject
	{
		public VerificationServiceProfile()
		{
			Type = "ceterms:VerificationServiceProfile";
			Properties = new List<PropertyData>()
			{
				CommonProperties.DateEffective,
				CommonProperties.Description,
				CommonProperties.EstimatedCost,
				CommonProperties.Jurisdiction,
				CommonProperties.OfferedBy,
				CommonProperties.OfferedIn,
				CommonProperties.Region,
				CommonProperties.TargetCredential,
				CommonProperties.ValidationMethodDescription,
				new PropertyData() { Type = PropertyType.BOOLEAN, SchemaName = "ceterms:holderMustAuthorize", Source = "HolderMustAuthorize", Label = "Holder Must Authorize" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:verificationDirectory", Source = "Auto_VerificationDirectory", Label = "Verification Directory", SourceType= SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.URL, SchemaName = "ceterms:verificationService", Source = "Auto_VerificationService", Label = "Verification Service URL", SourceType= SourceType.FROM_OBJECT_LIST, InnerSource = "TextValue" },
				new PropertyData() { Type = PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST, SchemaName = "ceterms:verifiedClaimType", Source = "VerifiedClaimType", Label = "Verified Claim Type" },
			};
		}
	}







	/* END AUTO GENERATED PROPERTIES */
}
