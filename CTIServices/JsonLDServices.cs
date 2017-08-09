using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

using Models.Common;
using Models.Json;
using Models.ProfileModels;
using JS = Models.Json;
using PM = Models.ProfileModels;
using CM = Models.Common;
using V2 = Models.JsonV2;
using Utilities;
namespace CTIServices
{
	public class JsonLDServices
	{
		string registryResourceUrl = UtilityManager.GetAppKeyValue( "credRegistryResourceUrl" );
		#region obsolete/not used
		#region credentials [Obsolete]


		//[Obsolete]
		//public JS.Credential GetJsonLDCredential( CM.Credential input )
		//{
		//	var roles = new EnumerationServices().GetCredentialAllAgentRoles( EnumerationType.CUSTOM );
		//	var output = new JS.Credential();

		//	//Basic information
		//	output.dateEffective = GetIso8601Date( input.DateEffective );
		//	output.latestVersion = input.LatestVersionUrl;
		//	output.replacesVersion = input.PreviousVersion;
		//	output.versionIdentifier = input.Version;
		//	output.hasPart = input.EmbeddedCredentials == null ? null : input.EmbeddedCredentials.Select( m => m.SubjectWebpage ).ToList();
		//	output.isPartOf = input.IsPartOf == null ? null : input.IsPartOf.Select( m => m.SubjectWebpage ).ToList();
		//	output.description = input.Description;
		//	output.image = input.ImageUrl;
		//	output.name = input.Name;
		//	output.url = input.SubjectWebpage;
		//	output.ctid = input.ctid;

		//	//Organization Roles
		//	//TODO - if these are already null, should we skip the call (performance)
		//	output.creator = GetOrganizationUrlsByRole( output.creator, input.OrganizationRole, roles.Items );
		//	output.owner = GetOrganizationUrlsByRole( output.owner, input.OrganizationRole, roles.Items );

		//	output.updatedVersionBy = GetOrganizationUrlsByRole( output.updatedVersionBy, input.OrganizationRole, roles.Items );
		//	output.verifiedBy = GetOrganizationUrlsByRole( output.verifiedBy, input.OrganizationRole, roles.Items );
		//	output.assessedBy = GetOrganizationUrlsByRole( output.assessedBy, input.OrganizationRole, roles.Items );
		//	output.offeredBy = GetOrganizationUrlsByRole( output.offeredBy, input.OrganizationRole, roles.Items );

		//	//Quality Assurance Roles
		//	output.accreditedBy = GetOrganizationUrlsByRole( output.accreditedBy, input.OrganizationRole, roles.Items );
		//	output.approvedBy = GetOrganizationUrlsByRole( output.approvedBy, input.OrganizationRole, roles.Items );
		//	output.conferredBy = GetOrganizationUrlsByRole( output.conferredBy, input.OrganizationRole, roles.Items );
		//	output.endorsedBy = GetOrganizationUrlsByRole( output.endorsedBy, input.OrganizationRole, roles.Items );
		//	output.recognizedBy = GetOrganizationUrlsByRole( output.recognizedBy, input.OrganizationRole, roles.Items );
		//	output.regulatedBy = GetOrganizationUrlsByRole( output.regulatedBy, input.OrganizationRole, roles.Items );
		//	output.revocationBy = GetOrganizationUrlsByRole( output.revocationBy, input.OrganizationRole, roles.Items );
		//	output.renewalBy = GetOrganizationUrlsByRole( output.renewalBy, input.OrganizationRole, roles.Items );
		//	output.validatedBy = GetOrganizationUrlsByRole( output.validatedBy, input.OrganizationRole, roles.Items );
		//	output.contributor = GetOrganizationUrlsByRole( output.contributor, input.OrganizationRole, roles.Items );

		//	//Enumerations
		//	output.credentialLevel = GetEnumerationValues( input.AudienceLevelType );
		//	output.credentialType = GetEnumerationValues( input.CredentialType );
		//	//output.purpose = GetEnumerationValues( input.IntendedPurpose );

		//	//Profiles
		//	//output.isRecommendedFor = GetJsonLDConditionProfiles( input.IsRecommendedFor );
		//	//output.isRequiredFor = GetJsonLDConditionProfiles( input.IsRequiredFor );
		//	//output.renewal = GetJsonLDConditionProfiles( input.Renewal );
		//	output.recommends = GetJsonLDConditionProfiles( input.Recommends );

		//	output.requires = GetJsonLDConditionProfiles( input.Requires );



		//	output.industryCategory = GetJsonLDEnumerations( new List<Models.Common.Enumeration>() { input.Industry } );
		//	//TODO - add other industries
		//	output.occupationCategory = GetJsonLDEnumerations( new List<Models.Common.Enumeration>() { input.Occupation } );
		//	//TODO - add other occupations

		//	output.estimatedTimeToEarn = GetJsonLDDurations( input.EstimatedDuration );
		//	output.jurisdiction = GetJsonLDJurisdictions( input.Jurisdiction );
		//	output.revocation = GetJsonLDRevocationProfiles( input.Revocation );

		//	//future
		//	/*
		//	 * process profiles
		//	 output.developmentProcess = GetJsonLDProcessProfiles( output.developmentProcess, input.CredentialProcess );
		//	output.maintenanceProcess = GetJsonLDProcessProfiles( output.maintenanceProcess, input.CredentialProcess );
		//	output.selectionProcess = GetJsonLDProcessProfiles( output.selectionProcess, input.CredentialProcess );
		//	output.validationProcess = GetJsonLDProcessProfiles( output.validationProcess, input.CredentialProcess );
		//	 * 
		//	output.earnings = GetJsonLDEarningsProfiles( input.Earnings );
		//	output.employmentOutcome = GetJSonLDEmploymentOutcomeProfiles( input.EmploymentOutcome );
		//	output.holders = GetJsonLDHoldersProfiles( input.Holders );
		//	*/
		//	//Temporary
		//	output.industryCategoryFlat = new List<TemporaryEnumerationItem>();
		//	try
		//	{
		//		foreach ( var category in output.industryCategory )
		//		{
		//			foreach ( var item in category.items )
		//			{
		//				output.industryCategoryFlat.Add( new TemporaryEnumerationItem()
		//				{
		//					name = item.name,
		//					url = item.url,
		//					frameworkName = category.name,
		//					frameworkUrl = category.url
		//				} );
		//			}
		//		}
		//	}
		//	catch { }

		//	return output;
		//}
		//
		#endregion

		#region organization [Obsolete]
		//[Obsolete]
		//public string GetSerializedJsonLDOrganization( CM.Organization input )
		//{
		//	return GetSerializedJsonLDOrganization( GetJsonLDOrganization( input ) );
		//}
		////
		//[Obsolete]
		//public string GetSerializedJsonLDOrganization( JS.Organization input )
		//{
		//	var settings = new JsonSerializerSettings()
		//	{
		//		NullValueHandling = NullValueHandling.Ignore,
		//		ContractResolver = new AlphaNumericContractResolver()
		//	};

		//	//prob no different than latter
		//	//string test = JsonConvert.SerializeObject( input,
		//	//				Newtonsoft.Json.Formatting.None,
		//	//				new JsonSerializerSettings
		//	//				{
		//	//					NullValueHandling = NullValueHandling.Ignore
		//	//				} );

		//	return JsonConvert.SerializeObject( input, settings );

		//}
		//[Obsolete]
		//public JS.Organization GetJsonLDOrganization( CM.Organization input )
		//{
		//	//TODO: check the enumeration value schemas/values and see why they aren't being retrieved
		//	var roles = new EnumerationServices().GetCredentialAllAgentRoles( EnumerationType.CUSTOM );
		//	var output = new JS.Organization();

		//	//Basic information
		//	output.description = input.Description;
		//	output.image = input.ImageUrl;
		//	output.name = input.Name;
		//	output.url = input.SubjectWebpage;
		//	output.ctid = input.ctid;
		//	output.email = input.Email;

		//	output.fein = GetEnumerationValue( "ceterms:fein", input.Identifiers.Items );
		//	output.identifier = null;
		//	output.opeid = GetEnumerationValue( "ceterms:opeID", input.Identifiers.Items );
		//	output.versioning = "";
		//	output.duns = GetEnumerationValue( "ceterms:duns", input.Identifiers.Items );
		//	output.foundingDate = input.FoundingDate;
		//	output.naics = GetEnumerationValue( "ceterms:naics", input.Identifiers.Items );
		//	output.purpose = input.Purpose;
		//	output.sameAs = input.SocialMediaPages.Select( m => m.TextValue ).ToList();

		//	//Organization Roles
		//	output.creatorOf = GetOrganizationUrlsByRole( output.creatorOf, input.OrganizationRole_Actor, roles.Items );
		//	output.owns = GetOrganizationUrlsByRole( output.owns, input.OrganizationRole_Actor, roles.Items );
		//	output.updatesVersion = GetOrganizationUrlsByRole( output.updatesVersion, input.OrganizationRole_Actor, roles.Items );
		//	output.verifies = GetOrganizationUrlsByRole( output.verifies, input.OrganizationRole_Actor, roles.Items );
		//	output.assesses = GetOrganizationUrlsByRole( output.assesses, input.OrganizationRole_Actor, roles.Items );
		//	output.offersCredential = GetOrganizationUrlsByRole( output.offersCredential, input.OrganizationRole_Actor, roles.Items );

		//	//output.employee = new List<string>();

		//	//output.trainingOffered = new List<string>();

		//	//Quality Assurance Roles
		//	output.accredits = GetOrganizationUrlsByRole( output.accredits, input.OrganizationRole_Actor, roles.Items );
		//	output.approves = GetOrganizationUrlsByRole( output.approves, input.OrganizationRole_Actor, roles.Items );
		//	output.confers = GetOrganizationUrlsByRole( output.confers, input.OrganizationRole_Actor, roles.Items );
		//	output.contributorTo = GetOrganizationUrlsByRole( output.contributorTo, input.OrganizationRole_Actor, roles.Items );
		//	output.endorses = GetOrganizationUrlsByRole( output.endorses, input.OrganizationRole_Actor, roles.Items );
		//	output.potentialAction = null;
		//	output.recognizes = GetOrganizationUrlsByRole( output.recognizes, input.OrganizationRole_Actor, roles.Items );
		//	output.regulates = GetOrganizationUrlsByRole( output.regulates, input.OrganizationRole_Actor, roles.Items );
		//	output.revokes = GetOrganizationUrlsByRole( output.revokes, input.OrganizationRole_Actor, roles.Items );
		//	output.renews = GetOrganizationUrlsByRole( output.renews, input.OrganizationRole_Actor, roles.Items );
		//	output.validates = GetOrganizationUrlsByRole( output.validates, input.OrganizationRole_Actor, roles.Items );

		//	//Enumerations
		//	output.agentCategory = GetEnumerationValues( input.OrganizationType );
		//	output.serviceType = GetEnumerationValues( input.ServiceType );

		//	//Profiles
		//	output.agentProcess = GetJsonLDProcessProfiles( output.agentProcess, input.AgentProcess );
		//	output.authenticationService = GetJsonLDAuthenticationProfiles( input.VerificationServiceProfiles );
		//	output.jurisdiction = null;
		//	output.address = GetJsonLDAddress( input.Address );
		//	output.contactPoint = null;

		//	return output;
		//}
		//
		#endregion

		//Force properties to be serialized in alphanumeric order
		//public class AlphaNumericContractResolver : DefaultContractResolver
		//{
		//	protected override System.Collections.Generic.IList<JsonProperty> CreateProperties( System.Type type, MemberSerialization memberSerialization )
		//	{
		//		return base.CreateProperties( type, memberSerialization ).OrderBy( m => m.PropertyName ).ToList();
		//	}
		//}
		//

		//Strip empty values, default values, etc.
		//public class NoEmptyValuesContractResolver : DefaultContractResolver
		//{
		//	protected override System.Collections.Generic.IList<JsonProperty> CreateProperties( System.Type type, MemberSerialization memberSerialization )
		//	{
		//		var properties = base.CreateProperties( type, memberSerialization ).OrderBy( m => m.PropertyName ).ToList();
		//		var results = new List<JsonProperty>();

		//		foreach ( var property in properties )
		//		{
		//			//if(property.val)
		//		}

		//		return results;
		//	}
		//}
		//
		//public List<JS.DurationProfile> GetJsonLDDurations( List<PM.DurationProfile> input )
		//{
		//	var output = new List<JS.DurationProfile>();


		//	return output;
		//}
		//

		public List<JS.JurisdictionProfile> GetJsonLDJurisdictions( List<CM.JurisdictionProfile> input )
		{
			var output = new List<JS.JurisdictionProfile>();


			return output;
		}
		//

		//public List<JS.RevocationProfile> GetJsonLDRevocationProfiles( List<PM.RevocationProfile> input )
		//{
		//	var output = new List<JS.RevocationProfile>();


		//	return output;
		//}
		////

		//public List<JS.EarningsProfile> GetJsonLDEarningsProfiles( List<PM.EarningsProfile> input )
		//{
		//	var output = new List<JS.EarningsProfile>();


		//	return output;
		//}
		////

		//public List<JS.EmploymentOutcomeProfile> GetJSonLDEmploymentOutcomeProfiles( List<PM.EmploymentOutcomeProfile> input )
		//{
		//	var output = new List<JS.EmploymentOutcomeProfile>();


		//	return output;
		//}
		////

		//public List<JS.HoldersProfile> GetJsonLDHoldersProfiles( List<PM.HoldersProfile> input )
		//{
		//	var output = new List<JS.HoldersProfile>();


		//	return output;
		//}
		////

		//public List<JS.Enumeration> GetJsonLDEnumerations( List<CM.Enumeration> input )
		//{
		//	var output = new List<JS.Enumeration>();

		//	foreach ( var item in input )
		//	{
		//		output.Add( new JS.Enumeration()
		//		{
		//			name = item.Name,
		//			description = item.Description,
		//			url = item.Url,
		//			items = GetJsonLDEnumerationItems( item.Items )
		//		} );
		//	}

		//	return output;
		//}
		//

		//public List<JS.EnumerationItem> GetJsonLDEnumerationItems( List<CM.EnumeratedItem> input )
		//{
		//	var output = new List<JS.EnumerationItem>();

		//	foreach ( var item in input )
		//	{
		//		output.Add( new EnumerationItem()
		//		{
		//			name = item.Name,
		//			url = item.URL
		//		} );
		//	}

		//	return output;
		//}
		////

		//public List<JS.TaskProfile> GetJsonLDTaskProfiles( List<PM.TaskProfile> input )
		//{
		//	var output = new List<JS.TaskProfile>();



		//	return output;
		//}
		////

		//public List<JS.ConditionProfile> GetJsonLDConditionProfiles( List<PM.ConditionProfile> input )
		//{
		//	var output = new List<JS.ConditionProfile>();

		//	foreach ( var profile in input )
		//	{
		//		output.Add( new JS.ConditionProfile()
		//		{
		//			assertedBy = profile.AssertedBy.SubjectWebpage,
		//			description = profile.Description,
		//			experience = profile.Experience,
		//			minimumAge = profile.MinimumAge,
		//			applicableAudienceType = GetEnumerationValues( profile.ApplicableAudienceType ),
		//			educationLevel = GetEnumerationValues( profile.AudienceLevel ),
		//			jurisdiction = GetJsonLDJurisdictions( profile.Jurisdiction ),
		//			residentOf = GetJsonLDJurisdictions( profile.Jurisdiction ),
		//			//targetTask = GetJsonLDTaskProfiles( profile.TargetTask ),

		//			targetAssessment = GetJsonLDAssessmentProfiles( profile.TargetAssessment ),
		//			targetLearningOpportunity = profile.TargetLearningOpportunity != null ? profile.TargetLearningOpportunity.Select( m => m.SubjectWebpage ).ToList() : null,
		//			targetCredential = profile.RequiredCredential != null ? profile.RequiredCredential.Select( m => m.SubjectWebpage ).ToList() : null,
		//		} );
		//	}
		//	//targetCompetency = profile.TargetCompetency != null ? profile.TargetCompetency.Select( m => m.Url ).ToList() : null,
		//	return output;
		//}
		//

		//public List<JS.AssessmentProfile> GetJsonLDAssessmentProfiles( List<PM.AssessmentProfile> input )
		//{
		//	var output = new List<JS.AssessmentProfile>();

		//	foreach ( var item in input )
		//	{
		//		output.Add( new JS.AssessmentProfile()
		//		{
		//			name = item.Name,
		//			description = item.Description,
		//			url = item.SubjectWebpage
		//		} );
		//	}

		//	return output;
		//}
		////

		//public List<JS.ProcessProfile> GetJsonLDProcessProfiles( object property, List<PM.ProcessProfile> input )
		//{
		//	var output = new List<JS.ProcessProfile>();
		//	try 
		//	{ 
		//		//Get the schema name to look for
		//		var processSchema = ( ( DataMemberAttribute ) property.GetType().GetCustomAttributes( typeof( DataMemberAttribute ), false ).FirstOrDefault() ).Name;

		//		//Match the schema name to profiles
		//		//var targetProfiles = input.Where( m => m.ProcessType.SchemaName == processSchema ).ToList();
		//		////Do the conversion
		//		//foreach ( var profile in targetProfiles )
		//		//{
		//		//	output.Add( new Models.Json.ProcessProfile()
		//		//	{
		//		//		//TODO: fill this in
		//		//	} );
		//		//}

		//		return output;
		//	}
		//	catch
		//	{
		//		return null;
		//	}
		//}
		////

		//public List<JS.AuthenticationProfile> GetJsonLDAuthenticationProfiles( List<PM.VerificationServiceProfile> input )
		//{
		//	var output = new List<JS.AuthenticationProfile>();
		//	foreach ( var profile in input )
		//	{
		//		output.Add( new JS.AuthenticationProfile()
		//		{
		//			description = profile.Description
		//		} );
		//	}

		//	return output;
		//}
		////

		//public JS.PostalAddress GetJsonLDAddress( CM.Address input )
		//{
		//	try
		//	{
		//		return new PostalAddress()
		//		{
		//			addressCountry = input.Country.Trim(),
		//			addressLocality = input.City.Trim(),
		//			addressRegion = input.AddressRegion.Trim(),
		//			postalCode = input.PostalCode.Trim(),
		//			streetAddress = input.Address1.Trim() + ( string.IsNullOrWhiteSpace( input.Address2.Trim() ) ? "" : ", " + input.Address2.Trim() )
		//		};
		//	}
		//	catch
		//	{
		//		return null;
		//	}
		//}
		////

		//public List<string> GetOrganizationUrlsByRole( object roleProperty, List<OrganizationRoleProfile> roleList, List<EnumeratedItem> roleCodes )
		//{
		//	try
		//	{
		//		//Get the schema name to look for
		//		var roleSchema = ( ( DataMemberAttribute ) roleProperty.GetType().GetCustomAttributes( typeof( DataMemberAttribute ), false ).FirstOrDefault() ).Name;
		//		//Match the schema name to a code
		//		var roleCode = roleCodes.FirstOrDefault( m => m.SchemaName.ToLower() == roleSchema.ToLower() );
		//		//Match the code to a value and get all the URLs that match that value
		//		return roleList.Where( m => m.RoleTypeId == roleCode.CodeId ).Select( m => m.ActingAgent.SubjectWebpage ).ToList();
		//	}
		//	catch
		//	{
		//		return null;
		//	}
		//}
		////

		//public List<string> GetEnumerationValues( Models.Common.Enumeration input )
		//{
		//	try
		//	{
		//		//return input.Items.Where( m => m.Selected == true ).Select( m => m.SchemaName ).ToList();
		//		//This should be schema name, but it is null, so for now, using label
		//		return input.Items.Where( m => m.Selected == true ).Select( m => m.Name ).ToList(); 
		//	}
		//	catch
		//	{
		//		return null;
		//	}
		//}
		//



		//

		//public string GetEnumerationValue( string identifierSchemaName, List<EnumeratedItem> items )
		//{
		//	try
		//	{
		//		var name = identifierSchemaName.Trim().ToLower();
		//		return items.FirstOrDefault( m => m.SchemaName.Trim().ToLower() == name ).Value;
		//	}
		//	catch
		//	{
		//		return null;
		//	}
		//}

	//	public string SerializeJsonV2( object json )
	//	{
	//		var settings = new JsonSerializerSettings()
	//		{
	//			NullValueHandling = NullValueHandling.Ignore,
	//			DefaultValueHandling = DefaultValueHandling.Ignore,
	//			ContractResolver = new AlphaNumericContractResolver()
	//		};

	//		return JsonConvert.SerializeObject( json );
	//}
		//

		#endregion
	


		public string GetCredentialV2ForRegistry( CM.Credential data )
		{
			Dictionary<string, object> dictionary = GetCredentialV2( data );

			string json = JsonConvert.SerializeObject( dictionary, Formatting.Indented );


			//var entries = dictionary.Select( d =>
			//	string.Format( "\"{0}\": [{1}]", d.Key, string.Join( ",", d.Value ) ) );
			//return "{" + string.Join( ",", entries ) + "}";
			return json;

		}
		//

		public Dictionary<string, object> GetCredentialV2( CM.Credential data )
		{
			//TODO - separate this process from specific code that uses sql server code
			return GetProfileV2( data, 
					new V2.Credential(), 
					new EnumerationServices().GetCredentialAllAgentRoles( EnumerationType.CUSTOM ).Items );
		}
		//

		/// <summary>
		/// The QA properties are the same as the credenial organization at this point
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public string GetQAOrganizationForRegistry( CM.Organization data )
		{
			Dictionary<string, object> dictionary = GetProfileV2( data, new V2.QACredentialOrganization(), new EnumerationServices().GetAllAgentReverseRoles( EnumerationType.CUSTOM ).Items );

			string json = JsonConvert.SerializeObject( dictionary, Formatting.Indented );

			return json;
		}
		//

		public string GetOrganizationV2ForRegistry( CM.Organization data )
		{
			Dictionary<string, object> dictionary = GetProfileV2( data, new V2.CredentialOrganization(), new EnumerationServices().GetAllAgentReverseRoles( EnumerationType.CUSTOM ).Items );

			string json = JsonConvert.SerializeObject( dictionary, Formatting.Indented );

			return json;
		}
		//

		public string GetAssessmentV2ForRegistry( PM.AssessmentProfile data )
		{
			Dictionary<string, object> dictionary = GetAssessmentV2( data );

			string json = JsonConvert.SerializeObject( dictionary, Formatting.Indented );

			return json;
		}
		public Dictionary<string, object> GetAssessmentV2( PM.AssessmentProfile data )
		{
			return GetProfileV2( data, new V2.AssessmentProfile(), new EnumerationServices().GetAssessmentAgentRoles( EnumerationType.CUSTOM ).Items );
		}
		//

		public string GetLearningOpportunityV2ForRegistry( PM.LearningOpportunityProfile data )
		{
			Dictionary<string, object> dictionary = GetLearningOpportunityV2( data );

			string json = JsonConvert.SerializeObject( dictionary, Formatting.Indented );

			return json;
		}
		public Dictionary<string, object> GetLearningOpportunityV2( PM.LearningOpportunityProfile data )
		{
			return GetProfileV2( data, new V2.LearningOpportunityProfile(), new EnumerationServices().GetLearningOppAgentRoles( EnumerationType.CUSTOM ).Items );
		}
		//
		#region Condition Manifest
		public string GetConditionManifestForRegistry( CM.ConditionManifest data )
		{
			Dictionary<string, object> dictionary = GetConditionManifest( data );

			string json = JsonConvert.SerializeObject( dictionary, Formatting.Indented );

			return json;
		}
		public Dictionary<string, object> GetConditionManifest( CM.ConditionManifest data )
		{
			return GetProfileV2( data, new V2.ConditionManifest(), new EnumerationServices().GetLearningOppAgentRoles( EnumerationType.CUSTOM ).Items );
		}
		//
		#endregion

		#region Cost Manifest
		public string GetCostManifestForRegistry( CM.CostManifest data )
		{
			Dictionary<string, object> dictionary = GetCostManifest( data );

			string json = JsonConvert.SerializeObject( dictionary, Formatting.Indented );

			return json;
		}
		public Dictionary<string, object> GetCostManifest( CM.CostManifest data )
		{
			return GetProfileV2( data, new V2.CostManifest(), new EnumerationServices().GetLearningOppAgentRoles( EnumerationType.CUSTOM ).Items );
		}
		//
		#endregion
		public Dictionary<string, object> GetProfileV2( object data, 
				V2.JsonLDDocument jsonTemplate, 
				List<CM.EnumeratedItem> roleCodes )
		{
			var result = new Dictionary<string, object>();

			result.Add( "@context", jsonTemplate.Context );
			//FixQAActionProfiles( data );

			ConvertToJson( data, jsonTemplate, roleCodes, result );

			return result;
		}
		//

		public void ConvertToJson( object data, V2.JsonLDObject jsonTemplate, List<CM.EnumeratedItem> roleCodes, Dictionary<string, object> result )
		{
			var dataProperties = data.GetType().GetProperties();
			var jsonProperties = jsonTemplate.Properties;

			if( jsonTemplate.Type != "@id" ) //This messes up graphs
			{
				result.Add( "@type", jsonTemplate.Type );
			}

			//Add CTID or ID based URI; whichever is appropriate, if either is appropriate
			var ctidProperty = data.GetType().GetProperties().FirstOrDefault( m => m.Name == "CTID" );
			if( ctidProperty != null )
			{
				var ctid = ( string ) ctidProperty.GetValue( data );
				if ( !string.IsNullOrWhiteSpace( ctid ) )
				{
					result.Add( "@id", registryResourceUrl + ctid );
				}
			}

			//Convert the properties
			foreach ( var property in jsonTemplate.Properties )
			{
				if ( 
					property.SchemaName == "ceterms:address"
					)
				{
					var test = "";
				}
				try
				{
					var sourceProperty = dataProperties.FirstOrDefault( m => m.Name == property.Source );
					var sourceValue = sourceProperty.GetValue( data );

					switch ( property.Type )
					{
						case V2.PropertyType.TEXT:
						case V2.PropertyType.NUMBER:
						case V2.PropertyType.URL:
						case V2.PropertyType.DATE:
						case V2.PropertyType.DATETIME:
							{
								switch ( property.SourceType )
								{
									case V2.SourceType.DIRECT:
										{
											AddBasicData( result, sourceValue, property.SchemaName, property.Type );
											break;
										}

									case V2.SourceType.FROM_OBJECT:
										{
											var innerSource = sourceValue.GetType().GetProperties().FirstOrDefault( m => m.Name == property.InnerSource );
											AddBasicData( result, innerSource.GetValue( sourceValue ), property.SchemaName, property.Type );
											break;
										}

									case V2.SourceType.FROM_OBJECT_LIST:
										{
											var itemList = ( List<object> ) ( sourceValue as IEnumerable<object> ).Cast<dynamic>().ToList();
											var test = new List<object>();
											foreach ( var item in itemList )
											{
												var innerSource = item.GetType().GetProperties().FirstOrDefault( m => m.Name == property.InnerSource );
												var testItem = ( string ) innerSource.GetValue( item );
												if ( !string.IsNullOrWhiteSpace( testItem ) )
												{
													if( property.Type == V2.PropertyType.URL )
													{
														if( testItem.IndexOf("http") == -1 )
														{
															testItem = registryResourceUrl + testItem;
														}

														test.Add( new Dictionary<string, object>() { { "@id", testItem } } );
													}
													else
													{
														test.Add( testItem );
													}
												}
											}
											if ( test.Count() > 0 )
											{
												result.Add( property.SchemaName, test );
											}
											break;
										}

									case V2.SourceType.FROM_METHOD:
										{
											sourceValue = typeof( JsonLDServices ).GetMethod( property.InnerSource ).Invoke( this, new object[] { sourceValue } );
											break;
										}

									default: break;
								}
							}
							break;
						case V2.PropertyType.DURATION:
							{
								try
								{
									var minutes = 0;
									var value = Factories.DurationProfileManager.AsSchemaDuration( ( DurationItem ) sourceValue, ref minutes );
									if( !string.IsNullOrWhiteSpace( value ) && minutes > 0 )
									{
										result.Add( property.SchemaName, ( value ) );
									}
								}
								catch { }
							}
							break;

						case V2.PropertyType.BOOLEAN:
							{
								if( sourceValue != null )
								{
									result.Add( property.SchemaName, sourceValue );
								}
							}
							break;

						case V2.PropertyType.ENUMERATION:
							{
								var test = ( ( CM.Enumeration ) sourceValue ).Items.Select( m => m.SchemaName ).ToList();
								if( test.Count() > 0 )
								{
									result.Add( property.SchemaName, test );
								}
								break;
							}

						case V2.PropertyType.ENUMERATION_EXTERNAL:
							{
								var test = ( ( CM.Enumeration ) sourceValue ).Items.Select( m => m.URL ).ToList();
								if ( test.Count() > 0 )
								{
									result.Add( property.SchemaName, test );
								}
								break;
							}

						case V2.PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST:
							{
								var newJson = new V2.CredentialAlignmentObject();
								var newList = new List<Dictionary<string, object>>();

								//Do the conversion
								foreach ( var profile in ( ( CM.Enumeration ) sourceValue ).ItemsAsAlignmentObjects )
								{
									var newResult = new Dictionary<string, object>();
									ConvertToJson( profile, newJson, roleCodes, newResult );
									if ( newResult.Where( m => m.Key != "@type" ).ToList().Count() > 0 )
									{
										newList.Add( newResult );
									}
								}
								if ( newList.Count() > 0 )
								{
									result.Add( property.SchemaName, newList );
								}
								break;
							}

						case V2.PropertyType.PROFILE:
							{
								var newJson = ( V2.JsonLDObject ) Activator.CreateInstance( property.ProfileType );
								var newResult = new Dictionary<string, object>();

								//Apply override mappings
								foreach ( var over in property.OverrideMapping )
								{
									try
									{
										newJson.Properties.FirstOrDefault( m => m.Source == over.Value ).Source = over.Key;
									}
									catch { }
								}

								//Do the conversion
								ConvertToJson( sourceValue, newJson, roleCodes, newResult );
								if ( newResult.Where( m => m.Key != "@type" ).ToList().Count() > 0 )
								{
									result.Add( property.SchemaName, newResult );
								}
							}
							break;

						case V2.PropertyType.PROFILE_EXTERNAL:
							{
								if ( property.SourceType == V2.SourceType.DIRECT )
								{
									result.Add( property.SchemaName, new Dictionary<string, object>() { { "@id", sourceValue } } );
									break;
								}
								break;
							}

						case V2.PropertyType.PROFILE_EXTERNAL_LIST:
							{
								try
								{
									if ( property.SourceType == V2.SourceType.DIRECT )
									{
										var items = ( List<string> ) sourceValue;
										result.Add( property.SchemaName, items.ConvertAll( m => new Dictionary<string, object>() { { "@id", m } } ).ToList() );
										break;
									}

									//Use existing code to determine how to figure out the @id property
									var holder = new Dictionary<string, object>();
									ConvertToJson( sourceValue, new V2.JsonLDObject(), roleCodes, holder );
									result.Add( property.SchemaName, new List<Dictionary<string, object>>() { new Dictionary<string, object>() { { "@id", holder[ "@id" ] } } } );
								}
								catch { }
							}
							break;

						case V2.PropertyType.PROFILE_LIST:
							{
								var newJson = ( V2.JsonLDObject ) Activator.CreateInstance( property.ProfileType );
								var newList = new List<Dictionary<string, object>>();

								//Apply override mappings
								foreach ( var over in property.OverrideMapping )
								{
									try
									{
										newJson.Properties.FirstOrDefault( m => m.Source == over.Value ).Source = over.Key;
									}
									catch { }
								}

								//Apply override method
								if ( property.SourceType == V2.SourceType.FROM_METHOD )
								{
									sourceValue = typeof( JsonLDServices ).GetMethod( property.InnerSource ).Invoke( this, new object[] { sourceValue } );
								}

								//Do the conversion
								foreach ( var profile in ( sourceValue as IEnumerable<object> ).Cast<dynamic>().ToList() )
								{
									var newResult = new Dictionary<string, object>();
									ConvertToJson( profile, newJson, roleCodes, newResult );
									if ( newResult.Where( m => m.Key != "@type" ).ToList().Count() > 0 )
									{
										newList.Add( newResult );
									}
								}
								if ( newList.Count() > 0 )
								{
									result.Add( property.SchemaName, newList );
								}
							}
							break;

						case V2.PropertyType.TEXTVALUE_LIST:
							{
								var test = ( List<TextValueProfile> ) sourceValue;
								var items = new List<string>();
								var tvProperties = typeof( TextValueProfile ).GetProperties();
								foreach ( var item in test )
								{
									if ( string.IsNullOrWhiteSpace( property.InnerSource ) )
									{
										property.InnerSource = "TextValue";
									}
									items.Add( tvProperties.FirstOrDefault( m => m.Name == property.InnerSource ).GetValue( item ) as string );
								}
								if ( items.Count() > 0 )
								{
									result.Add( property.SchemaName, items );
								}
							}
							break;

						case V2.PropertyType.ROLE:
							{
								var test = ( List<OrganizationRoleProfile> ) sourceValue;
								var items = new List<string>();
								if ( result.ContainsKey( property.SchemaName ) )
								{
									items = ( List<string> ) result[ property.SchemaName ];
								}
								var matches = test.Where( m => m.AgentRole.Items.Where( n => MatchSchema( n.SchemaName, property.SchemaName ) ).Count() > 0 ).Select( m => m.ActingAgent.CTID.ToString() ).ToList();
								foreach ( var match in matches )
								{
									if ( !items.Contains( match ) && !string.IsNullOrWhiteSpace( match ) ) 
									{
										items.Add( match );
									}
								}

								if( items.Count() > 0 )
								{
									var itemsToAdd = new List<Dictionary<string, object>>();
									foreach( var item in items.Where( m => m != null ).ToList() )
									{
										itemsToAdd.Add( new Dictionary<string, object>() { { "@id", registryResourceUrl + item } } );
									}
									if ( result.ContainsKey( property.SchemaName ) )
									{
										result[ property.SchemaName ] = itemsToAdd;
									}
									else
									{
										result.Add( property.SchemaName, itemsToAdd );
									}
								}
							}
						break;

						case V2.PropertyType.PARENT_TYPE_OVERRIDE:
							{
								var replacementType = "";
								switch ( property.SourceType )
								{
									case V2.SourceType.DIRECT:
										{
											replacementType = ( string ) sourceValue;
											break;
										}

									case V2.SourceType.FROM_METHOD:
										{
											replacementType = (string) typeof( JsonLDServices ).GetMethod( property.InnerSource ).Invoke( this, new object[] { sourceValue } );
											break;
										}

									case V2.SourceType.FROM_ENUMERATION:
										{
											var test = ( Models.Common.Enumeration ) sourceValue;
											var item = test.Items.FirstOrDefault( m => test.Items.Count() == 1 || m.Selected == true );
											if ( item != null )
											{
												replacementType = item.SchemaName;
											}
											break;
										}
									default: break;
								}
								var normalizedName = replacementType.Replace(property.SchemaName, ""); //Strip out schema if it's already in the name
								normalizedName = normalizedName[ 0 ].ToString().ToUpper() + normalizedName.Substring( 1 ); //Enforce uppercase first letter
								result[ "@type" ] = property.SchemaName + normalizedName; //Prepend schema to class name
							}
						break;

						default: break;
					}
				}
		
				catch { }
			}
		}
		//
		public string GetIso8601Date( string input )
		{
			try
			{
				return DateTime.Parse( input ).ToString( "s", System.Globalization.CultureInfo.InvariantCulture );
			}
			catch
			{
				return "";
			}
		}
		public void AddBasicData( Dictionary<string, object> result, object sourceValue, string propertyName, V2.PropertyType type )
		{
			switch ( type )
			{
				case V2.PropertyType.TEXT: 
				case V2.PropertyType.URL:
				case V2.PropertyType.DATE:
				case V2.PropertyType.DATETIME:
					{
						var test = ( string ) sourceValue;
						if ( !string.IsNullOrWhiteSpace( test ) )
						{
							if ( type == V2.PropertyType.DATETIME )
							{
								var date = GetIso8601Date( test );
								if ( !string.IsNullOrWhiteSpace( date ) )
								{
									result.Add( propertyName, date );
								}
							}
							else if( type == V2.PropertyType.DATE )
							{
								try
								{
									var date = ( string ) sourceValue;
									if ( !string.IsNullOrWhiteSpace( date ) )
									{
										result.Add( propertyName, DateTime.Parse( date ).ToString( "yyyy'-'MM'-'dd" ) );
									}
								}
								catch { }
							}
							else if ( type == V2.PropertyType.URL ) //Handle URLs as identifiers
							{
								result.Add( propertyName, new Dictionary<string, object>() { { "@id", test } } );
							}
							else
							{
								result.Add( propertyName, test );
							}
						}
						break;
					}
				case V2.PropertyType.NUMBER:
					{
						if ( ( dynamic ) sourceValue != 0 )
						{
							result.Add( propertyName, sourceValue );
						}
						break;
					}
				default: break;
			}
		}
		//

		//Compensate for codetable schema inconsistencies
		public bool MatchSchema( string schema1, string schema2 )
		{
			var test1Parts = schema1.ToLower().Split( ':' );
			var test2Parts = schema2.ToLower().Split( ':' );
			var test1 = test1Parts.Length > 1 ? test1Parts[ 1 ] : test1Parts[ 0 ];
			var test2 = test2Parts.Length > 1 ? test2Parts[ 1 ] : test2Parts[ 0 ];
			return test1.Contains( test2 ) || test2.Contains( test1 );
		}
		//


		#region Import Methods

		public CM.Credential Test_ImportCredential( CM.Credential data )
		{
			var jsonData = GetCredentialV2( data );
			var text = JsonConvert.SerializeObject( jsonData );
			var errors = new List<Exception>();
			return ImportCredential( text, errors );
		}

		public CM.Credential ImportCredential( string inputJSON, List<Exception> errors )
		{
			//var inputData = JsonConvert.DeserializeObject<Dictionary<string, object>>( inputJSON );
			var template = new V2.Credential();
			var roleCodes = new EnumerationServices().GetCredentialAllAgentRoles( EnumerationType.CUSTOM ).Items;
			var result = new CM.Credential();
			var data = JsonToDictionary( inputJSON );

			ConvertFromJson( data, template, roleCodes, result, errors );
			return result;
		}

		//Recursively deserialize into a Dictionary<string, object>
		public Dictionary<string, object> JsonToDictionary( string json )
		{
			var result = new Dictionary<string, object>();
			var obj = JObject.Parse( json );
			foreach( var property in obj )
			{
				result.Add( property.Key, JsonToObject( property.Value ) );
			}
			return result;
		}
		public object JsonToObject( JToken token )
		{
			switch ( token.Type )
			{
				case JTokenType.Object:
					{
						return token.Children<JProperty>().ToDictionary( property => property.Name, property => JsonToObject( property.Value ) );
					}
				case JTokenType.Array:
					{
						var result = new List<object>();
						foreach( var obj in token )
						{
							result.Add( JsonToObject( obj ) );
						}
						return result;
					}
				default:
					{
						return ( ( JValue ) token ).Value;
					}
			}
		}

		public void ImportRecord( string ctid, Models.Common.BaseObject holder, V2.JsonLDObject jsonTemplate, List<CM.EnumeratedItem> roleCodes )
		{
			try
			{
				var errors = new List<Exception>();
				var rawResult = new HttpClient().GetAsync( ServiceHelper.GetAppKeyValue( "credRegistryResourceUrl" ) + ctid ).Result;
				var rawData = rawResult.Content.ReadAsStringAsync().Result;
				var data = new Dictionary<string, object>();
				if( !rawResult.IsSuccessStatusCode || rawData.Contains("No matching resource found" ) )
				{
					//Hack
					rawResult = new HttpClient().GetAsync( ServiceHelper.GetAppKeyValue( "credRegistryResourceUrl" ).Replace( "/resources/", "/ce-registry/search?ceterms:ctid=" ) + ctid ).Result;
					rawData = rawResult.Content.ReadAsStringAsync().Result;
					rawData = rawData.Substring( 1, rawData.Length - 2 ); //Remove [ and ] so that the object is no longer an array
					data = ( Dictionary<string, object> ) JsonToDictionary( rawData )[ "decoded_resource" ];
				}
				else
				{
					data = JsonToDictionary( rawData );
				}
				ConvertFromJson( data, jsonTemplate, roleCodes, holder, errors );
				var test = "";
			}
			catch { }
		}

		public void ConvertFromJson( Dictionary<string, object> data, V2.JsonLDObject jsonTemplate, List<CM.EnumeratedItem> roleCodes, object result, List<Exception> errors )
		{
			var jsonProperties = jsonTemplate.Properties;
			var resultProperties = result.GetType().GetProperties();

			foreach ( var property in jsonTemplate.Properties )
			{
				try
				{
					var sourceValue = data[ property.SchemaName ];

					if ( property.SchemaName == "ceterms:address" )
					{
						var test = "";
					}

					var resultProperty = resultProperties.FirstOrDefault( m => m.Name == property.Source );
					if ( resultProperty == null )
					{
						LoggingHelper.DoTrace( 5, "Unable to find property '" + property.Source + "' in " + resultProperties.GetType().Name );
						throw new Exception( "Unable to find property '" + property.Source + "' in " + resultProperties.GetType().Name );
					}

					switch ( property.Type )
					{
						case V2.PropertyType.URL:
						case V2.PropertyType.TEXT:
						case V2.PropertyType.NUMBER:
						case V2.PropertyType.BOOLEAN:
						case V2.PropertyType.DATE:
						case V2.PropertyType.DATETIME:
							{
								switch ( property.SourceType )
								{
									case V2.SourceType.DIRECT:
										{
											if ( property.Type == V2.PropertyType.URL )
											{
												try
												{
													resultProperty.SetValue( result, ( sourceValue as Dictionary<string, object> )[ "@id" ] );
												}
												catch
												{
													resultProperty.SetValue( result, ( string ) sourceValue );
												}
											}
											else
											{
												resultProperty.SetValue( result, sourceValue );
											}
											break;
										}
									case V2.SourceType.FROM_OBJECT:
										{
											//TODO: this
											break;
										}
									case V2.SourceType.FROM_OBJECT_LIST:
										{
											if ( property.Type == V2.PropertyType.URL )
											{
												var newHolder = new List<TextValueProfile>();
												foreach ( var profile in (sourceValue as IEnumerable<object>).Cast<Dictionary<string, object>>().ToList() )
												{
													var newResult = new TextValueProfile()
													{
														TextValue = (string) (profile as Dictionary<string, object>)[ "@id" ]
													};
													newHolder.Add( newResult );
												}
												resultProperty.SetValue( result, newHolder );
											}
											else
											{
												var newHolder = new List<TextValueProfile>();
												foreach ( var profile in (sourceValue as IEnumerable<object>).Cast<string>().ToList() )
												{
													var newResult = new TextValueProfile()
													{
														TextValue = profile
													};
													newHolder.Add( newResult );
												}
												resultProperty.SetValue( result, newHolder );
											}
											break;
										}
									case V2.SourceType.FROM_METHOD:
										{
											//TODO: this
											break;
										}
								}
								break;
							}
						case V2.PropertyType.ENUMERATION:
							{
								//TODO: this
								break;
							}
						case V2.PropertyType.ENUMERATION_EXTERNAL:
							{
								//TODO: this
								break;
							}
						case V2.PropertyType.ENUMERATION_ALIGNMENTOBJECT_LIST:
							{
								var newHolder = new List<CredentialAlignmentObjectProfile>();
								//foreach( var profile in ( sourceValue as IEnumerable<object>).Cast<dynamic>().ToList() )
								foreach( var profile in ( sourceValue as IEnumerable<object> ).Cast<Dictionary<string, object>>().ToList() )
								{
									var newResult = new CredentialAlignmentObjectProfile();
									ConvertFromJson( profile, new Models.JsonV2.CredentialAlignmentObject(), roleCodes, newResult, errors );
									newHolder.Add( newResult );
								}
								var enumeration = new CM.Enumeration() { ItemsAsAlignmentObjects = newHolder };
								resultProperty.SetValue( result, enumeration );
								break;
							}
						case V2.PropertyType.PROFILE:
							{
								//TODO: this
								break;
							}
						case V2.PropertyType.PROFILE_LIST:
							{
								//Source data
								var sourceList = ( sourceValue as IEnumerable<object> ).Cast<Dictionary<string, object>>().ToList();
								//JSON template
								var newTemplate = ( V2.JsonLDObject ) Activator.CreateInstance( property.ProfileType );
								//Property in the parent object that contains this list
								var resultList = result.GetType().GetProperty( resultProperty.Name );
								//The type of objects that are contained in the list
								var isListOfType = resultProperty.PropertyType.GenericTypeArguments.FirstOrDefault();
								//A new list of the same type of objects. 
								//Note the use of System.Collections.IList rather than the generic IList<T> is necessary here. 
								//That alone took about an hour to figure out the hard way, so I'm leaving the reference in here for documentation.
								var holderList = ( System.Collections.IList ) Activator.CreateInstance( typeof( List<> ).MakeGenericType( isListOfType ) );
								foreach ( var item in sourceList )
								{
									//Create the new item
									var holder = Activator.CreateInstance( isListOfType );
									//Populate the new item
									ConvertFromJson( item, newTemplate, roleCodes, holder, errors );
									//Add the item to the holder list
									holderList.Add( holder );
								}
								//Set the value of the parent's property to the holder list
								resultList.SetValue( result, holderList );
								break;
							}
						case V2.PropertyType.PROFILE_EXTERNAL_LIST:
							{
								switch ( property.SourceType )
								{
									case V2.SourceType.DIRECT:
										{
											resultProperty.SetValue( result, sourceValue );
											break;
										}
									case V2.SourceType.FROM_OBJECT:
										{
											var id = ( string ) ( ( List<object> ) sourceValue ).Cast<Dictionary<string, object>>().ToList().FirstOrDefault()[ "@id" ];
											//At this point, it /should/ go and get the data for this ID from the registry. But for now, just create an object with it.
											var holder = Activator.CreateInstance( resultProperty.PropertyType );
											var ctid = id.Split( '/' ).Last();
											holder.GetType().GetProperty( "ctid" ).SetValue( holder, ctid );
											result.GetType().GetProperty( resultProperty.Name ).SetValue( result, holder );
											break;
										}
									case V2.SourceType.FROM_OBJECT_LIST:
										{
											var ids = ( ( List<object> ) sourceValue).Cast<Dictionary<string, object>>().ToList().Select( m => ( string ) m[ "@id" ] ).ToList();
											var resultList = result.GetType().GetProperty( resultProperty.Name );
											var isListOfType = resultProperty.PropertyType.GenericTypeArguments.FirstOrDefault();
											var holderList = ( System.Collections.IList ) Activator.CreateInstance( typeof( List<> ).MakeGenericType( isListOfType ) );
											foreach ( var id in ids )
											{
												var holder = Activator.CreateInstance( isListOfType );
												var ctid = id.Split( '/' ).Last();
												holder.GetType().GetProperty( "ctid" ).SetValue( holder, ctid );
												holderList.Add( holder );
											}
											resultList.SetValue( result, holderList );
											break;
										}
									case V2.SourceType.FROM_METHOD:
										{
											//TODO: this
											break;
										}
								}
								break;
							}
						case V2.PropertyType.TEXTVALUE_LIST:
							{
								//TODO: this
								break;
							}
						case V2.PropertyType.ROLE:
							{
								//In this case we want to add to the parent's list rather than using SetValue, since we don't want to overwrite its existing contents
								//Alternatively we could just get the contents first, add to them, then set the value on the parent, 
								//but leaving this method here as-is means there is an example of doing it this way
								var ids = (( List<object> ) sourceValue).Cast<Dictionary<string, object>>().ToList().Select( m => ( string ) m[ "@id" ] ).ToList();
								var resultList = result.GetType().GetProperty( resultProperty.Name );
								foreach ( var id in ids )
								{
									var holder = new OrganizationRoleProfile();
									var ctid = id.Split( '/' ).Last();
									holder.ActingAgent = new Models.Common.Organization() { ctid = ctid };
									holder.AgentRole = new Models.Common.Enumeration();
									holder.AgentRole.Items.Add( new EnumeratedItem() { SchemaName = property.SchemaName } );
									resultList.PropertyType.GetMethod( "Add" ).Invoke( resultList.GetValue( result ), new object[] { holder } );
								}
								break;
							}
						case V2.PropertyType.PARENT_TYPE_OVERRIDE:
							{
								//TODO: this
								break;
							}
						default:
							{
								errors.Add( new Exception( "No matching PropertyType found for property: " + property.SchemaName ) );
								break;
							}
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.DoTrace( 5, string.Format("ConvertFromJson. Exception: {0}, source: '{1}' in {2}", ex.Message, property.Source, resultProperties.GetType().Name ));
					errors.Add( ex );
				}
			}
		}

		#endregion

		#region not currently used

		public List<Models.Common.GeoCoordinates> WrapAddress( object data )
		{
			var result = new List<Models.Common.GeoCoordinates>();
			
			foreach ( var address in data as List<Address>  )
			{
				result.Add( new CM.GeoCoordinates()
				{
					Address = address,
					Latitude = address.Latitude,
					Longitude = address.Longitude
				} );
			}

			return result;
		}
		//

		public List<Models.ProfileModels.CostProfileMerged> FlattenCosts( object data )
		{
			return CostProfileMerged.FlattenCosts( data as List<CostProfile> );
		}
		//

		public void FixQAActionProfiles( object data )
		{
			var qaList = data.GetType().GetProperties().FirstOrDefault( m => m.Name == "QualityAssuranceAction" );
			if ( qaList != null )
			{
				foreach ( var item in qaList.GetValue( data ) as List<QualityAssuranceActionProfile> )
				{
					try
					{
						item.TargetOverride = data;
						item.QualityAssuranceType = item.ReverseQAActionSchema;  //temporary?

						var credentialData = CredentialServices.GetBasicCredential( item.IssuedCredentialId );
						item.IssuedCredential = new Models.Common.Credential() { Id = credentialData.Id, SubjectWebpage = credentialData.SubjectWebpage, Name = credentialData.Name };
						item.ActingAgent = OrganizationServices.GetLightOrgByRowId( item.ActingAgentUid.ToString() );
					}
					catch { }
				}
			}
		}
		//

		public List<Models.Common.CredentialAlignmentObjectProfile> EnumerationToAlignmentObjects( object data )
		{
			return ( ( CM.Enumeration ) data ).ItemsAsAlignmentObjects;
		}
		//


		#endregion


	}
}
