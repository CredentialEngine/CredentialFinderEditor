using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;

using Models.Common;
using Models.Json;
using Models.ProfileModels;
using JS = Models.Json;
using PM = Models.ProfileModels;
using CM = Models.Common;
using V2 = Models.JsonV2;

namespace CTIServices
{
	public class JsonLDServices
	{
		#region credentials 
		[Obsolete]
		public string GetSerializedJsonLDCredential( CM.Credential input )
		{
			return GetSerializedJsonLDCredential( GetJsonLDCredential( input ) );
		}
		//

		[Obsolete]
		public string GetSerializedJsonLDCredential( JS.Credential input )
		{
			var settings = new JsonSerializerSettings()
			{
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore,
				ContractResolver = new AlphaNumericContractResolver()
			};
			return JsonConvert.SerializeObject( input, settings );
		}


		public JS.Credential GetJsonLDCredential( CM.Credential input )
		{
			var roles = new EnumerationServices().GetCredentialAllAgentRoles( EnumerationType.CUSTOM );
			var output = new JS.Credential();

			//Basic information
			output.dateEffective = GetIso8601Date( input.DateEffective );
			output.latestVersion = input.LatestVersionUrl;
			output.replacesVersion = input.ReplacesVersionUrl;
			output.versionIdentifier = input.Version;
			output.hasPart = input.EmbeddedCredentials == null ? null : input.EmbeddedCredentials.Select( m => m.Url ).ToList();
			output.isPartOf = input.IsPartOf == null ? null :input.IsPartOf.Select( m => m.Url ).ToList();
			output.description = input.Description;
			output.image = input.ImageUrl;
			output.name = input.Name;
			output.url = input.Url;
			output.ctid = input.ctid;

			//Organization Roles
			//TODO - if these are already null, should we skip the call (performance)
			output.creator = GetOrganizationUrlsByRole( output.creator, input.OrganizationRole, roles.Items );
			output.owner = GetOrganizationUrlsByRole( output.owner, input.OrganizationRole, roles.Items );

			output.updatedVersionBy = GetOrganizationUrlsByRole( output.updatedVersionBy, input.OrganizationRole, roles.Items );
			output.verifiedBy = GetOrganizationUrlsByRole( output.verifiedBy, input.OrganizationRole, roles.Items );
			output.assessedBy = GetOrganizationUrlsByRole( output.assessedBy, input.OrganizationRole, roles.Items );
			output.offeredBy = GetOrganizationUrlsByRole( output.offeredBy, input.OrganizationRole, roles.Items );

			//Quality Assurance Roles
			output.accreditedBy = GetOrganizationUrlsByRole( output.accreditedBy, input.OrganizationRole, roles.Items );
			output.approvedBy = GetOrganizationUrlsByRole( output.approvedBy, input.OrganizationRole, roles.Items );
			output.conferredBy = GetOrganizationUrlsByRole( output.conferredBy, input.OrganizationRole, roles.Items );
			output.endorsedBy = GetOrganizationUrlsByRole( output.endorsedBy, input.OrganizationRole, roles.Items );
			output.recognizedBy = GetOrganizationUrlsByRole( output.recognizedBy, input.OrganizationRole, roles.Items );
			output.regulatedBy = GetOrganizationUrlsByRole( output.regulatedBy, input.OrganizationRole, roles.Items );
			output.revocationBy = GetOrganizationUrlsByRole( output.revocationBy, input.OrganizationRole, roles.Items );
			output.renewalBy = GetOrganizationUrlsByRole( output.renewalBy, input.OrganizationRole, roles.Items );
			output.validatedBy = GetOrganizationUrlsByRole( output.validatedBy, input.OrganizationRole, roles.Items );
			output.contributor = GetOrganizationUrlsByRole( output.contributor, input.OrganizationRole, roles.Items );

			//Enumerations
			output.credentialLevel = GetEnumerationValues( input.CredentialLevel );
			output.credentialType = GetEnumerationValues( input.CredentialType );
			output.purpose = GetEnumerationValues( input.Purpose );

			//Profiles
			output.isRecommendedFor = GetJsonLDConditionProfiles( input.IsRecommendedFor );
			output.isRequiredFor = GetJsonLDConditionProfiles( input.IsRequiredFor );
			output.recommends = GetJsonLDConditionProfiles( input.Recommends );
			output.renewal = GetJsonLDConditionProfiles( input.Renewal );
			output.requires = GetJsonLDConditionProfiles( input.Requires );



			output.industryCategory = GetJsonLDEnumerations( new List<Models.Common.Enumeration>() { input.Industry } );
			output.occupationCategory = GetJsonLDEnumerations( new List<Models.Common.Enumeration>() { input.Occupation } );

			output.estimatedTimeToEarn = GetJsonLDDurations( input.EstimatedTimeToEarn );
			output.jurisdiction = GetJsonLDJurisdictions( input.Jurisdiction );
			output.revocation = GetJsonLDRevocationProfiles( input.Revocation );

			//future
			/*
			 * process profiles
			 output.developmentProcess = GetJsonLDProcessProfiles( output.developmentProcess, input.CredentialProcess );
			output.maintenanceProcess = GetJsonLDProcessProfiles( output.maintenanceProcess, input.CredentialProcess );
			output.selectionProcess = GetJsonLDProcessProfiles( output.selectionProcess, input.CredentialProcess );
			output.validationProcess = GetJsonLDProcessProfiles( output.validationProcess, input.CredentialProcess );
			 * 
			output.earnings = GetJsonLDEarningsProfiles( input.Earnings );
			output.employmentOutcome = GetJSonLDEmploymentOutcomeProfiles( input.EmploymentOutcome );
			output.holders = GetJsonLDHoldersProfiles( input.Holders );
			*/
			//Temporary
			output.industryCategoryFlat = new List<TemporaryEnumerationItem>();
			try
			{
				foreach ( var category in output.industryCategory )
				{
					foreach ( var item in category.items )
					{
						output.industryCategoryFlat.Add( new TemporaryEnumerationItem()
						{
							name = item.name,
							url = item.url,
							frameworkName = category.name,
							frameworkUrl = category.url
						} );
					}
				}
			}
			catch { }

			return output;
		}
		//
		#endregion 

		#region organization
		public string GetSerializedJsonLDOrganization( CM.Organization input )
		{
			return GetSerializedJsonLDOrganization( GetJsonLDOrganization( input ) );
		}
		//

		public string GetSerializedJsonLDOrganization( JS.Organization input )
		{
			var settings = new JsonSerializerSettings()
			{
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new AlphaNumericContractResolver()
			};
		
			//prob no different than latter
			//string test = JsonConvert.SerializeObject( input,
			//				Newtonsoft.Json.Formatting.None,
			//				new JsonSerializerSettings
			//				{
			//					NullValueHandling = NullValueHandling.Ignore
			//				} );

			return JsonConvert.SerializeObject( input, settings );

		}

		public JS.Organization GetJsonLDOrganization( CM.Organization input )
		{
			//TODO: check the enumeration value schemas/values and see why they aren't being retrieved
			var roles = new EnumerationServices().GetCredentialAllAgentRoles( EnumerationType.CUSTOM );
			var output = new JS.Organization();

			//Basic information
			output.description = input.Description;
			output.image = input.ImageUrl;
			output.name = input.Name;
			output.url = input.Url;
			output.email = input.Email;

			output.fein = GetEnumerationValue( "schema:taxid", input.Identifiers.Items );
			output.identifier = null;
			output.opeid = GetEnumerationValue( "cti:opeid", input.Identifiers.Items );
			output.versioning = "";
			output.duns = GetEnumerationValue( "schema:duns", input.Identifiers.Items );
			output.foundingDate = input.FoundingDate;
			output.naics = GetEnumerationValue( "schema:naics", input.Identifiers.Items );
			output.purpose = input.Purpose;
			output.sameAs = input.SocialMediaPages.Select( m => m.TextValue ).ToList();

			//Organization Roles
			output.creatorOf = GetOrganizationUrlsByRole( output.creatorOf, input.OrganizationRole_Actor, roles.Items );
			output.owns = GetOrganizationUrlsByRole( output.owns, input.OrganizationRole_Actor, roles.Items );
			output.updatesVersion = GetOrganizationUrlsByRole( output.updatesVersion, input.OrganizationRole_Actor, roles.Items );
			output.verifies = GetOrganizationUrlsByRole( output.verifies, input.OrganizationRole_Actor, roles.Items );
			output.assesses = GetOrganizationUrlsByRole( output.assesses, input.OrganizationRole_Actor, roles.Items );
			output.offersCredential = GetOrganizationUrlsByRole( output.offersCredential, input.OrganizationRole_Actor, roles.Items );

			//output.employee = new List<string>();

			//output.trainingOffered = new List<string>();

			//Quality Assurance Roles
			output.accredits = GetOrganizationUrlsByRole( output.accredits, input.OrganizationRole_Actor, roles.Items );
			output.approves = GetOrganizationUrlsByRole( output.approves, input.OrganizationRole_Actor, roles.Items );
			output.confers = GetOrganizationUrlsByRole( output.confers, input.OrganizationRole_Actor, roles.Items );
			output.contributorTo = GetOrganizationUrlsByRole( output.contributorTo, input.OrganizationRole_Actor, roles.Items );
			output.endorses = GetOrganizationUrlsByRole( output.endorses, input.OrganizationRole_Actor, roles.Items );
			output.potentialAction = null;
			output.recognizes = GetOrganizationUrlsByRole( output.recognizes, input.OrganizationRole_Actor, roles.Items );
			output.regulates = GetOrganizationUrlsByRole( output.regulates, input.OrganizationRole_Actor, roles.Items );
			output.revokes = GetOrganizationUrlsByRole( output.revokes, input.OrganizationRole_Actor, roles.Items );
			output.renews = GetOrganizationUrlsByRole( output.renews, input.OrganizationRole_Actor, roles.Items );
			output.validates = GetOrganizationUrlsByRole( output.validates, input.OrganizationRole_Actor, roles.Items );

			//Enumerations
			output.agentCategory = GetEnumerationValues( input.OrganizationType );
			output.serviceType = GetEnumerationValues( input.ServiceType );

			//Profiles
			output.agentProcess = GetJsonLDProcessProfiles( output.agentProcess, input.AgentProcess );
			output.authenticationService = GetJsonLDAuthenticationProfiles( input.Authentication );
			output.jurisdiction = null;
			output.address = GetJsonLDAddress( input.Address );
			output.contactPoint = null;
			
			return output;
		}
		//
		#endregion

		//Force properties to be serialized in alphanumeric order
		public class AlphaNumericContractResolver : DefaultContractResolver
		{
			protected override System.Collections.Generic.IList<JsonProperty> CreateProperties( System.Type type, MemberSerialization memberSerialization )
			{
				return base.CreateProperties( type, memberSerialization ).OrderBy( m => m.PropertyName ).ToList();
			}
		}
		//

		//Strip empty values, default values, etc.
		public class NoEmptyValuesContractResolver : DefaultContractResolver
		{
			protected override System.Collections.Generic.IList<JsonProperty> CreateProperties( System.Type type, MemberSerialization memberSerialization )
			{
				var properties = base.CreateProperties( type, memberSerialization ).OrderBy( m => m.PropertyName ).ToList();
				var results = new List<JsonProperty>();

				foreach ( var property in properties )
				{
					//if(property.val)
				}

				return results;
			}
		}
		//
		public List<JS.DurationProfile> GetJsonLDDurations( List<PM.DurationProfile> input )
		{
			var output = new List<JS.DurationProfile>();


			return output;
		}
		//

		public List<JS.JurisdictionProfile> GetJsonLDJurisdictions( List<CM.JurisdictionProfile> input )
		{
			var output = new List<JS.JurisdictionProfile>();


			return output;
		}
		//

		public List<JS.RevocationProfile> GetJsonLDRevocationProfiles( List<PM.RevocationProfile> input )
		{
			var output = new List<JS.RevocationProfile>();


			return output;
		}
		//

		public List<JS.EarningsProfile> GetJsonLDEarningsProfiles( List<PM.EarningsProfile> input )
		{
			var output = new List<JS.EarningsProfile>();


			return output;
		}
		//

		public List<JS.EmploymentOutcomeProfile> GetJSonLDEmploymentOutcomeProfiles( List<PM.EmploymentOutcomeProfile> input )
		{
			var output = new List<JS.EmploymentOutcomeProfile>();


			return output;
		}
		//

		public List<JS.HoldersProfile> GetJsonLDHoldersProfiles( List<PM.HoldersProfile> input )
		{
			var output = new List<JS.HoldersProfile>();


			return output;
		}
		//

		public List<JS.Enumeration> GetJsonLDEnumerations( List<CM.Enumeration> input )
		{
			var output = new List<JS.Enumeration>();

			foreach ( var item in input )
			{
				output.Add( new JS.Enumeration()
				{
					name = item.Name,
					description = item.Description,
					url = item.Url,
					items = GetJsonLDEnumerationItems( item.Items )
				} );
			}

			return output;
		}
		//

		public List<JS.EnumerationItem> GetJsonLDEnumerationItems( List<CM.EnumeratedItem> input )
		{
			var output = new List<JS.EnumerationItem>();

			foreach ( var item in input )
			{
				output.Add( new EnumerationItem()
				{
					name = item.Name,
					url = item.URL
				} );
			}

			return output;
		}
		//

		public List<JS.TaskProfile> GetJsonLDTaskProfiles( List<PM.TaskProfile> input )
		{
			var output = new List<JS.TaskProfile>();



			return output;
		}
		//

		public List<JS.ConditionProfile> GetJsonLDConditionProfiles( List<PM.ConditionProfile> input )
		{
			var output = new List<JS.ConditionProfile>();

			foreach ( var profile in input )
			{
				output.Add( new JS.ConditionProfile()
				{
					assertedBy = profile.AssertedBy.Url,
					description = profile.Description,
					experience = profile.Experience,
					minimumAge = profile.MinimumAge,
					applicableAudienceType = GetEnumerationValues( profile.ApplicableAudienceType ),
					credentialType = GetEnumerationValues( profile.CredentialType ),
					jurisdiction = GetJsonLDJurisdictions( profile.Jurisdiction ),
					residentOf = GetJsonLDJurisdictions( profile.Jurisdiction ),
					targetTask = GetJsonLDTaskProfiles( profile.TargetTask ),
					
					targetAssessment = GetJsonLDAssessmentProfiles( profile.TargetAssessment ),
					targetLearningOpportunity = profile.TargetLearningOpportunity != null ? profile.TargetLearningOpportunity.Select( m => m.Url ).ToList() : null,
					targetCredential = profile.RequiredCredential != null ? profile.RequiredCredential.Select( m => m.Url ).ToList() : null,
				} );
			}
			//targetCompetency = profile.TargetCompetency != null ? profile.TargetCompetency.Select( m => m.Url ).ToList() : null,
			return output;
		}
		//

		public List<JS.AssessmentProfile> GetJsonLDAssessmentProfiles( List<PM.AssessmentProfile> input )
		{
			var output = new List<JS.AssessmentProfile>();

			foreach ( var item in input )
			{
				output.Add( new JS.AssessmentProfile()
				{
					name = item.Name,
					description = item.Description,
					url = item.Url
				} );
			}

			return output;
		}
		//

		public List<JS.ProcessProfile> GetJsonLDProcessProfiles( object property, List<PM.ProcessProfile> input )
		{
			var output = new List<JS.ProcessProfile>();
			try 
			{ 
				//Get the schema name to look for
				var processSchema = ( ( DataMemberAttribute ) property.GetType().GetCustomAttributes( typeof( DataMemberAttribute ), false ).FirstOrDefault() ).Name;
				//Match the schema name to profiles
				var targetProfiles = input.Where( m => m.ProcessType.SchemaName == processSchema ).ToList();
				//Do the conversion
				foreach ( var profile in targetProfiles )
				{
					output.Add( new Models.Json.ProcessProfile()
					{
						//TODO: fill this in
					} );
				}

				return output;
			}
			catch
			{
				return null;
			}
		}
		//

		public List<JS.AuthenticationProfile> GetJsonLDAuthenticationProfiles( List<PM.AuthenticationProfile> input )
		{
			var output = new List<JS.AuthenticationProfile>();
			foreach ( var profile in input )
			{
				output.Add( new JS.AuthenticationProfile()
				{
					description = profile.Description
				} );
			}

			return output;
		}
		//

		public JS.PostalAddress GetJsonLDAddress( CM.Address input )
		{
			try
			{
				return new PostalAddress()
				{
					addressCountry = input.Country.Trim(),
					addressLocality = input.City.Trim(),
					addressRegion = input.AddressRegion.Trim(),
					postalCode = input.PostalCode.Trim(),
					streetAddress = input.Address1.Trim() + ( string.IsNullOrWhiteSpace( input.Address2.Trim() ) ? "" : ", " + input.Address2.Trim() )
				};
			}
			catch
			{
				return null;
			}
		}
		//

		public List<string> GetOrganizationUrlsByRole( object roleProperty, List<OrganizationRoleProfile> roleList, List<EnumeratedItem> roleCodes )
		{
			try
			{
				//Get the schema name to look for
				var roleSchema = ( ( DataMemberAttribute ) roleProperty.GetType().GetCustomAttributes( typeof( DataMemberAttribute ), false ).FirstOrDefault() ).Name;
				//Match the schema name to a code
				var roleCode = roleCodes.FirstOrDefault( m => m.SchemaName.ToLower() == roleSchema.ToLower() );
				//Match the code to a value and get all the URLs that match that value
				return roleList.Where( m => m.RoleTypeId == roleCode.CodeId ).Select( m => m.ActingAgent.Url ).ToList();
			}
			catch
			{
				return null;
			}
		}
		//

		public List<string> GetEnumerationValues( Models.Common.Enumeration input )
		{
			try
			{
				//return input.Items.Where( m => m.Selected == true ).Select( m => m.SchemaName ).ToList();
				//This should be schema name, but it is null, so for now, using label
				return input.Items.Where( m => m.Selected == true ).Select( m => m.Name ).ToList(); 
			}
			catch
			{
				return null;
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
		//

		public string GetEnumerationValue( string identifierSchemaName, List<EnumeratedItem> items )
		{
			try
			{
				var name = identifierSchemaName.Trim().ToLower();
				return items.FirstOrDefault( m => m.SchemaName.Trim().ToLower() == name ).Value;
			}
			catch
			{
				return null;
			}
		}

		public string SerializeJsonV2( object json )
		{
			var settings = new JsonSerializerSettings()
			{
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore,
				ContractResolver = new AlphaNumericContractResolver()
			};

			return JsonConvert.SerializeObject( json );
	}
		//

		public string GetCredentialV2ForRegistry( CM.Credential data )
		{
			Dictionary<string, object> dictionary = GetCredentialV2( data );

			string json = JsonConvert.SerializeObject( dictionary, Formatting.Indented );


			//var entries = dictionary.Select( d =>
			//	string.Format( "\"{0}\": [{1}]", d.Key, string.Join( ",", d.Value ) ) );
			//return "{" + string.Join( ",", entries ) + "}";
			return json;

			//this doesn't work
			//return dictionary.ToString();
		}
		//

		public Dictionary<string, object> GetCredentialV2( CM.Credential data )
		{
			return GetProfileV2( data, new V2.Credential(), new EnumerationServices().GetCredentialAllAgentRoles( EnumerationType.CUSTOM ).Items );
		}
		//

		public string GetOrganizationV2ForRegistry( CM.Organization data )
		{
			Dictionary<string, object> dictionary = GetProfileV2( data, new V2.Organization(), new EnumerationServices().GetAllAgentReverseRoles( EnumerationType.CUSTOM ).Items );

			string json = JsonConvert.SerializeObject( dictionary, Formatting.Indented );

			return json;
		}
		public Dictionary<string, object> GetOrganizationV2( CM.Organization data )
		{
			return GetProfileV2( data, new V2.Organization(), new EnumerationServices().GetAllAgentReverseRoles( EnumerationType.CUSTOM ).Items );
		}
		//

		public Dictionary<string, object> GetAssessmentV2( PM.AssessmentProfile data )
		{
			return GetProfileV2( data, new V2.AssessmentProfile(), new EnumerationServices().GetAssessmentAgentRoles( EnumerationType.CUSTOM ).Items );
		}
		//

		public Dictionary<string, object> GetLearningOpportunityV2( PM.LearningOpportunityProfile data )
		{
			return GetProfileV2( data, new V2.LearningOpportunityProfile(), new EnumerationServices().GetLearningOppAgentRoles( EnumerationType.CUSTOM ).Items );
		}
		//

		public Dictionary<string, object> GetProfileV2( object data, V2.JsonLDDocument jsonTemplate, List<CM.EnumeratedItem> roleCodes )
		{
			var result = new Dictionary<string, object>();

			result.Add( "@context", jsonTemplate.Context );
			FixQAActionProfiles( data );

			ConvertToJson( data, jsonTemplate, roleCodes, result );

			return result;
		}
		//

		public void ConvertToJson( object data, V2.JsonLDObject jsonTemplate, List<CM.EnumeratedItem> roleCodes, Dictionary<string, object> result )
		{
			var dataProperties = data.GetType().GetProperties();
			var jsonProperties = jsonTemplate.Properties;

			result.Add( "@type", jsonTemplate.Type );

			//Convert the properties
			foreach ( var property in jsonTemplate.Properties )
			{
				try
				{
					var sourceProperty = dataProperties.FirstOrDefault( m => m.Name == property.Source );
					var sourceValue = sourceProperty.GetValue( data );

					switch ( property.Type )
					{
						case V2.PropertyType.TEXT:
						case V2.PropertyType.NUMBER:
						case V2.PropertyType.URL:
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
											var test = new List<string>();
											foreach ( var item in itemList )
											{
												var innerSource = item.GetType().GetProperties().FirstOrDefault( m => m.Name == property.InnerSource );
												var testItem = ( string ) innerSource.GetValue( item );
												if ( !string.IsNullOrWhiteSpace( testItem ) )
												{
													test.Add( testItem );
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

						case V2.PropertyType.BOOLEAN:
							{
								result.Add( property.SchemaName, sourceValue );
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

						case V2.PropertyType.TEXTVALUE_LIST:
							{
								//TODO: figure this out
								//var test = ( ( List<PM.TextValueProfile> ) sourceValue ).Where( m => m.CodeSchema == property.SchemaName ).Select( m => m.TextValue ).ToList();
								//if ( test.Count() > 0 )
								//{
								//	result.Add( property.SchemaName, test );
								//}
							}
							break;

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

						case V2.PropertyType.ROLE:
							{
								var test = ( List<OrganizationRoleProfile> ) sourceValue;
								var items = new List<string>();
								if ( result.ContainsKey( property.SchemaName ) )
								{
									items = ( List<string> ) result[ property.SchemaName ];
								}
								var matches = test.Where( m => m.AgentRole.Items.Where( n => n.SchemaName.Contains( property.SchemaName ) ).Count() > 0 ).Select( m => m.ActingAgent.Url ).ToList();
								foreach ( var match in matches )
								{
									if ( !items.Contains( match ) ) 
									{
										items.Add( match );
									}
								}
								if ( result.ContainsKey( property.SchemaName ) )
								{
									result[ property.SchemaName ] = items;
								}
								else
								{
									if ( items.Count() > 0 )
									{
										result.Add( property.SchemaName, items );
									}
								}
							}
						break;

						case V2.PropertyType.PARENT_TYPE_OVERRIDE:
							{
								switch ( property.SourceType )
								{
									case V2.SourceType.DIRECT:
										{
											result[ "@type" ] = ( string ) sourceValue;
											break;
										}

									case V2.SourceType.FROM_METHOD:
										{
											result[ "@type" ] = typeof( JsonLDServices ).GetMethod( property.InnerSource ).Invoke( this, new object[] { sourceValue } );
											break;
										}

									case V2.SourceType.FROM_ENUMERATION:
										{
											var test = ( Models.Common.Enumeration ) sourceValue;
											var item = test.Items.FirstOrDefault( m => test.Items.Count() == 1 || m.Selected == true );
											if ( item != null )
											{
												result[ "@type" ] = item.SchemaName;
											}
											break;
										}
									default: break;
								}
							}
						break;

						default: break;
					}
				}
		
				catch { }
			}
		}
		//

		public void AddBasicData( Dictionary<string, object> result, object sourceValue, string propertyName, V2.PropertyType type )
		{
			switch ( type )
			{
				case V2.PropertyType.TEXT: 
				case V2.PropertyType.URL:
				case V2.PropertyType.DATE:
					{
						var test = ( string ) sourceValue;
						if ( !string.IsNullOrWhiteSpace( test ) )
						{
							if ( type == V2.PropertyType.DATE )
							{
								result.Add( propertyName, GetIso8601Date( test ) );
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
			var result = new List<Models.ProfileModels.CostProfileMerged>();

			foreach ( var cost in data as List<CostProfile> )
			{
				foreach ( var costItem in cost.Items )
				{
					result.Add( new CostProfileMerged()
					{
						ProfileName = cost.ProfileName,
						Description = cost.Description,
						DateEffective = cost.DateEffective,
						ExpirationDate = cost.ExpirationDate,
						ReferenceUrl = cost.ReferenceUrl,
						Currency = ( cost.CurrencyTypes.Items.FirstOrDefault( m => m.Selected == true || m.CodeId == cost.CurrencyTypeId ) ?? cost.CurrencyTypes.Items.First() ).Name,
						Jurisdiction = cost.Jurisdiction,
						CostType = costItem.CostType,
						ResidencyType = costItem.ResidencyType,
						EnrollmentType = costItem.EnrollmentType,
						ApplicableAudienceType = costItem.ApplicableAudienceType,
						PaymentPattern = costItem.PaymentPattern,
						Price = costItem.Price
					} );
				}
			}

			return result;
		}
		//

		public void FixQAActionProfiles( object data )
		{
			var qaList = data.GetType().GetProperties().FirstOrDefault( m => m.Name == "QualityAssuranceAction" );
			foreach ( var item in qaList.GetValue(data) as List<QualityAssuranceActionProfile> )
			{
				try
				{
					item.TargetOverride = data;
					var credentialData = CredentialServices.GetLightCredentialById( item.IssuedCredentialId );
					item.IssuedCredential = new Models.Common.Credential() { Id = credentialData.Id, Url = credentialData.Url, Name = credentialData.Name };
					item.ActingAgent = OrganizationServices.GetLightOrgByRowId( item.ActingAgentUid.ToString() );
					item.QAAction = "ctdl:" + new EnumerationServices().GetCredentialAgentQAActions( EnumerationType.CUSTOM, data.GetType().Name ).Items.FirstOrDefault( m => m.CodeId == item.RoleTypeId ).Name.Split( new string[] { "ed By" }, StringSplitOptions.RemoveEmptyEntries )[ 0 ] + "Action";
				}
				catch { }
			}
		}
		//

	}
}
