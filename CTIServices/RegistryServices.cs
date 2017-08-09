using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using System.Web;

using Models;
using CM = Models.Common;
using Models.ProfileModels;
using Factories;
using Utilities;
//using JWT;
//using J2 = Utilities.JsonWebToken2;
using Newtonsoft.Json;
using MetadataRegistry;


namespace CTIServices
{
	public class RegistryServices
	{
		static string thisClassName = "RegistryServices";

		#region Publishing 
		/// <summary>
		/// publish a credential to the registry
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="user"></param>
		/// <param name="statusMessage"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public bool PublishCredential( int credentialId,
									Models.AppUser user, 
									ref string statusMessage,
									ref List<SiteActivity> list )
		{
			CM.Credential entity = CredentialServices.GetCredentialForPublish( credentialId, user );
			if ( entity.Id == 0 )
			{
				statusMessage = "Error - invalid Credential identifier. " ;
				if (!string.IsNullOrWhiteSpace(entity.StatusMessage))
				{
					statusMessage += entity.StatusMessage;
				}
				return false;
			}
			if ( !entity.CanEditRecord )
			{
				statusMessage = "Error - not authorized to upload this Credential to the registry";
				return false;
			}
			list  = new List<SiteActivity>();
			bool successful = true;
			string action = "";
			string comment = "";
			var payload = "";
			//bool usingV2 = true;
			//if ( usingV2 )
				payload = new JsonLDServices().GetCredentialV2ForRegistry( entity ).ToString();
			//else
			//	payload = new JsonLDServices().GetSerializedJsonLDCredential( entity );

			//temp until CR changes the prefix
			//payload = payload.Replace( "ceterms:", "cti:" );

			string crEnvelopeId = entity.CredentialRegistryId;

			bool success = Publish( payload.ToString(), user.FullName()
									, "credential_" + credentialId.ToString(),
									ref statusMessage,
									ref crEnvelopeId );
			if ( success )
			{
				//call update regardless
				new CredentialManager().UpdateEnvelopeId( credentialId, crEnvelopeId, user.Id, ref statusMessage );
				//if was an add, update CredentialRegistryId
				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Registered Credential";
					comment = string.Format( "{0} registered credential: {1}. Returned envelopeId: {2}", user.FullName(), credentialId, crEnvelopeId );
				}
				else
				{
					action = "Updated Credential";
					comment = string.Format( "{0} updated previously registered credential: {1}. Returned envelopeId: {2}", user.FullName(), credentialId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Credential", Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = credentialId } );

				//new ActivityServices().AddActivity( "Credential Registry", action, comment, user.Id, 0, credentialId );

				if ( user.FullName().IndexOf( "Incomplete -" ) > -1 )
				{
					LoggingHelper.LogError( string.Format( thisClassName + ".PublishCredential() Error - encountered user with incomplete profile - or more likely issue at login/session creation. User: {0}", user.Email ), true );
				}

				list = ActivityManager.GetPublishHistory( "Credential", credentialId );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not saved in the Credential Registry.";
				successful = false;
			}
			return successful;
		}

		/// <summary>
		/// publish an organization to the registry
		/// </summary>
		/// <param name="orgId"></param>
		/// <param name="user"></param>
		/// <param name="statusMessage"></param>
		/// <param name="crEnvelopeId"></param>
		/// <returns></returns>
		public bool PublishOrganization( int orgId, Models.AppUser user, ref string statusMessage, ref List<SiteActivity> list )
		{
			CM.Organization entity = OrganizationServices.GetOrganizationForPublish( orgId, user );
			if ( entity.Id == 0 )
			{
				statusMessage = "Error - invalid Organization identifier";
				return false;
			}
			if ( !entity.CanEditRecord )
			{
				statusMessage = "Error - not authorized to publish this Organization to the registry";
				return false;
			}
			bool successful = true;
			string payload = "";
			if ( entity.ISQAOrganization)
				payload = new JsonLDServices().GetQAOrganizationForRegistry( entity );
			else
				payload = new JsonLDServices().GetOrganizationV2ForRegistry( entity );
			//temp until CR changes the prefix
			//payload = payload.Replace( "ceterms:", "cti:" );

			string crEnvelopeId = entity.CredentialRegistryId;

			//???
			successful = Publish( payload.ToString(), user.FullName()
									, "organization_" + orgId.ToString(),
									ref statusMessage,
									ref crEnvelopeId );

			string action = "";
			string comment = "";
			if ( successful )
			{
				//call update regardless
				new OrganizationManager().UpdateEnvelopeId( orgId, crEnvelopeId, user.Id, ref statusMessage );

				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Registered Organization";
					comment = string.Format( "{0} registered Organization: {1}. Returned envelopeId: {2}", user.FullName(), orgId, crEnvelopeId );
					
				}
				else
				{
					action = "Updated Organization";
					comment = string.Format( "{0} updated previously registered Organization: {1}. Returned envelopeId: {2}", user.FullName(), orgId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Organization", Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = orgId } );

				list = ActivityManager.GetPublishHistory( "Organization", orgId );
			}
			else
			{
				//ensure a message is returned
				if (string.IsNullOrWhiteSpace(statusMessage))
					statusMessage = "The document was not saved in the Credential Registry.";
				successful = false;
			}
			return successful;
		}

		/// <summary>
		/// publish an assessment to the registry
		/// </summary>
		/// <param name="assessmentId"></param>
		/// <param name="user"></param>
		/// <param name="statusMessage"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public bool PublishAssessment( int assessmentId, 
								Models.AppUser user,
								ref string statusMessage,
								ref List<SiteActivity> list )
		{
			bool successful = true;

			AssessmentProfile entity = AssessmentServices.GetDetail( assessmentId, user );
			if ( !entity.CanEditRecord )
			{
				statusMessage = "Error - not authorized to publish this Assessment to the registry";
				return false;
			}

			list = new List<SiteActivity>();
			string action = "";
			string comment = "";

			var payload = new JsonLDServices().GetAssessmentV2ForRegistry( entity ).ToString();
			//payload = payload.Replace( "@type\": \"ceterms:AssessmentProfile", "@type\": \"AssessmentProfile" );
			//if ( ServiceHelper.IsTestEnv() )
			//	payload = payload.Replace( "ceterms:name", "schema:name" );
			string crEnvelopeId = entity.CredentialRegistryId;
			//payload = payload.Replace( "@type\": \"ceterms:AssessmentProfile", "@type\": \"AssessmentProfile" );
			//ceterms:CredentialAlignmentObject
			//payload = payload.Replace( "@type\": \"AssessmentProfile", "@type\": \"ceterms:AssessmentProfile" );
			//ceterms:assessmentMethodType
		

			bool success = Publish( payload.ToString(), user.FullName(),
									"assessment_" + assessmentId.ToString(),
									ref statusMessage,
									ref crEnvelopeId );
			if ( success )
			{
				//call update regardless
				new AssessmentManager().UpdateEnvelopeId( assessmentId, crEnvelopeId, user.Id, ref statusMessage );
				//if was an add, update CredentialRegistryId
				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Registered Assessment";
					comment = string.Format( "{0} registered Assessment: {1}. Returned envelopeId: {2}", user.FullName(), assessmentId, crEnvelopeId );
				}
				else
				{
					action = "Updated Assessment";
					comment = string.Format( "{0} updated previously registered Assessment: {1}. Returned envelopeId: {2}", user.FullName(), assessmentId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "AssessmentProfile", Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = assessmentId } );

				if ( user.FullName().IndexOf( "Incomplete -" ) > -1 )
				{
					LoggingHelper.LogError( string.Format( thisClassName + ".PublishAssessment() Error - encountered user with incomplete profile - or more likely issue at login/session creation. User: {0}", user.Email ), true );
				}

				list = ActivityManager.GetPublishHistory( "AssessmentProfile", assessmentId );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not saved in the Credential Registry.";
				successful = false;
			}
			return successful;
		}

		public bool PublishLearningOpportunity( int learningOppId,
								Models.AppUser user,
								ref string statusMessage,
								ref List<SiteActivity> list )
		{
			bool successful = true;

			LearningOpportunityProfile entity = LearningOpportunityServices.GetForDetail( learningOppId, user );
			if ( !entity.CanEditRecord )
			{
				statusMessage = "Error - not authorized to publish this Learning Opportunity to the registry";
				return false;
			}

			list = new List<SiteActivity>();
			string action = "";
			string comment = "";

			var payload = new JsonLDServices().GetLearningOpportunityV2ForRegistry( entity ).ToString();
			//if(ServiceHelper.IsTestEnv())
			//	payload = payload.Replace( "ceterms:name", "schema:name" );
			string crEnvelopeId = entity.CredentialRegistryId;

			bool success = Publish( payload.ToString(), user.FullName(),
									"learningOpp_" + learningOppId.ToString(),
									ref statusMessage,
									ref crEnvelopeId );
			if ( success )
			{
				//call update regardless
				new LearningOpportunityManager().UpdateEnvelopeId( learningOppId, crEnvelopeId, user.Id, ref statusMessage );
				//if was an add, update CredentialRegistryId
				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Registered LearningOpportunity";
					comment = string.Format( "{0} registered LearningOpportunity: {1}. Returned envelopeId: {2}", user.FullName(), learningOppId, crEnvelopeId );
				}
				else
				{
					action = "Updated LearningOpportunity";
					comment = string.Format( "{0} updated previously registered LearningOpportunity: {1}. Returned envelopeId: {2}", user.FullName(), learningOppId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "LearningOpportunity", Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = learningOppId } );

				if ( user.FullName().IndexOf( "Incomplete -" ) > -1 )
				{
					LoggingHelper.LogError( string.Format( thisClassName + ".PublishLearningOpportunity() Error - encountered user with incomplete profile - or more likely issue at login/session creation. User: {0}", user.Email ), true );
				}

				list = ActivityManager.GetPublishHistory( "LearningOpportunity", learningOppId );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not saved in the Credential Registry.";
				successful = false;
			}
			return successful;
		}


		public bool PublishConditionManifest( int manifestId,
								Models.AppUser user,
								ref string statusMessage,
								ref List<SiteActivity> list )
		{
			bool successful = true;

			CM.ConditionManifest entity = ConditionManifestServices.GetForDetail( manifestId, user );
			if ( !entity.CanEditRecord )
			{
				statusMessage = "Error - not authorized to publish this Condition Manifest to the registry";
				return false;
			}

			list = new List<SiteActivity>();
			string action = "";
			string comment = "";

			var payload = new JsonLDServices().GetConditionManifestForRegistry( entity ).ToString();
			//if(ServiceHelper.IsTestEnv())
			//	payload = payload.Replace( "ceterms:name", "schema:name" );
			string crEnvelopeId = entity.CredentialRegistryId;

			bool success = Publish( payload.ToString(), user.FullName(),
									"ConditionManifest_" + manifestId.ToString(),
									ref statusMessage,
									ref crEnvelopeId );
			if ( success )
			{
				//call update regardless
				new ConditionManifestManager().UpdateEnvelopeId( manifestId, crEnvelopeId, user.Id, ref statusMessage );
				//if was an add, update CredentialRegistryId
				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Registered ConditionManifest";
					comment = string.Format( "{0} registered ConditionManifest: {1}. Returned envelopeId: {2}", user.FullName(), manifestId, crEnvelopeId );
				}
				else
				{
					action = "Updated ConditionManifest";
					comment = string.Format( "{0} updated previously registered ConditionManifest: {1}. Returned envelopeId: {2}", user.FullName(), manifestId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "ConditionManifest", Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = manifestId } );

				if ( user.FullName().IndexOf( "Incomplete -" ) > -1 )
				{
					LoggingHelper.LogError( string.Format( thisClassName + ".PublishConditionManifest() Error - encountered user with incomplete profile - or more likely issue at login/session creation. User: {0}", user.Email ), true );
				}

				list = ActivityManager.GetPublishHistory( "ConditionManifest", manifestId );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not saved in the Credential Registry.";
				successful = false;
			}
			return successful;
		}


		public bool PublishCostManifest( int manifestId,
								Models.AppUser user,
								ref string statusMessage,
								ref List<SiteActivity> list )
		{
			bool successful = true;

			CM.CostManifest entity = CostManifestServices.GetForDetail( manifestId, user );
			if ( !entity.CanEditRecord )
			{
				statusMessage = "Error - not authorized to publish this Cost Manifest to the registry";
				return false;
			}

			list = new List<SiteActivity>();
			string action = "";
			string comment = "";

			var payload = new JsonLDServices().GetCostManifestForRegistry( entity ).ToString();
			//if(ServiceHelper.IsTestEnv())
			//	payload = payload.Replace( "ceterms:name", "schema:name" );
			string crEnvelopeId = entity.CredentialRegistryId;

			bool success = Publish( payload.ToString(), user.FullName(),
									"CostManifest_" + manifestId.ToString(),
									ref statusMessage,
									ref crEnvelopeId );
			if ( success )
			{
				//call update regardless
				new CostManifestManager().UpdateEnvelopeId( manifestId, crEnvelopeId, user.Id, ref statusMessage );
				//if was an add, update CredentialRegistryId
				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Registered CostManifest";
					comment = string.Format( "{0} registered CostManifest: {1}. Returned envelopeId: {2}", user.FullName(), manifestId, crEnvelopeId );
				}
				else
				{
					action = "Updated CostManifest";
					comment = string.Format( "{0} updated previously registered CostManifest: {1}. Returned envelopeId: {2}", user.FullName(), manifestId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "CostManifest", Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = manifestId } );

				if ( user.FullName().IndexOf( "Incomplete -" ) > -1 )
				{
					LoggingHelper.LogError( string.Format( thisClassName + ".PublishCostManifest() Error - encountered user with incomplete profile - or more likely issue at login/session creation. User: {0}", user.Email ), true );
				}

				list = ActivityManager.GetPublishHistory( "CostManifest", manifestId );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not saved in the Credential Registry.";
				successful = false;
			}
			return successful;
		}



		/// <summary>
		/// Publish a document to the Credential Registry
		/// </summary>
		/// <param name="payload"></param>
		/// <param name="submitter"></param>
		/// <param name="statusMessage"></param>
		/// <param name="crEnvelopeId"></param>
		/// <returns></returns>
		public bool Publish( string payload,
									string submitter,
									string identifier,
									ref string statusMessage,
									ref string crEnvelopeId )
		{
			var successful = true;
			var result = Publish( payload, submitter, identifier, ref successful, ref statusMessage, ref crEnvelopeId );
			return successful;				
		}


		//Used for demo page, and possibly other cases where the raw response is desired
		public string Publish( string payload, 
				string submitter, 
				string identifier, 
				ref bool valid, 
				ref string status, 
				ref string crEnvelopeId, 
				bool forceSkipValidation = false )
		{
			valid = true;
			var publicKeyPath = "";
			var privateKeyPath = "";
			var postBody = "";

			try
			{
				if ( GetKeys( ref publicKeyPath, ref privateKeyPath, ref status ) == false )
				{
					valid = false;
					//no, the proper error is returned from GetKeys
					//status = "Error getting CER Keys";
					return status;
				}

				//todo - need to add signer and other to the content
				//note for new, DO NOT INCLUDE an EnvelopeIdentifier property 
				//		-this is necessary due to a bug, and hopefully can change back to a single call

				LoggingHelper.DoTrace( 5, "RegistryServices.Publish - payload: \r\n" + payload );

				if ( string.IsNullOrWhiteSpace( crEnvelopeId ) )
				{
					Envelope envelope = new Envelope();
					//envelope = RegistryHandler.CreateEnvelope( publicKeyPath, privateKeyPath, payload );
					//OR
					RegistryHandler.CreateEnvelope( publicKeyPath, privateKeyPath, payload, envelope );

					postBody = JsonConvert.SerializeObject( envelope );

					LoggingHelper.DoTrace( 6, "RegistryServices.Publish - ADD envelope: \r\n" + postBody );
				}
				else
				{
					UpdateEnvelope envelope = new UpdateEnvelope();
					RegistryHandler.CreateEnvelope( publicKeyPath, privateKeyPath, payload, envelope );

					//now embed 
					envelope.EnvelopeIdentifier = crEnvelopeId;
					postBody = JsonConvert.SerializeObject( envelope );

					LoggingHelper.DoTrace( 6, "RegistryServices.Publish - update envelope: \r\n" + postBody );
				}

				//Do publish
				string serviceUri = UtilityManager.GetAppKeyValue( "credentialRegistryPublishUrl" );
				var skippingValidation = forceSkipValidation ? true : UtilityManager.GetAppKeyValue( "skippingValidation" ) == "yes";
				if ( skippingValidation )
				{
					if ( serviceUri.ToLower().IndexOf( "skip_validation" ) > 0 )
					{
						//assume OK, or check to change false to true
						serviceUri = serviceUri.Replace( "skip_validation=false", "skip_validation=true" );
					}
					else
					{
						//append
						serviceUri += "&skip_validation=true";
					}
				}
				try
				{
					using ( var client = new HttpClient() )
					{
						client.DefaultRequestHeaders.
							Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );

						var task = client.PostAsync( serviceUri,
							new StringContent( postBody, Encoding.UTF8, "application/json" ) );
						task.Wait();
						var response = task.Result;
						//should get envelope_id from contents?
						var contents = task.Result.Content.ReadAsStringAsync().Result;

						if ( response.IsSuccessStatusCode == false )
						{
							RegistryResponseContent contentsJson = JsonConvert.DeserializeObject<RegistryResponseContent>( contents );
							//
							valid = false;
							string queryString = GetRequestContext();

							LoggingHelper.LogError( identifier + " RegistryServices.Publish Failed:"
								+ "\n\rURL:\n\r " + queryString
								+ "\n\rERRORS:\n\r " + string.Join( ",", contentsJson.Errors.ToArray() )
								+ "\n\rRESPONSE:\n\r " + JsonConvert.SerializeObject( response )
								+ "\n\rCONTENTS:\n\r " + JsonConvert.SerializeObject( contents )
								+ "\n\rPAYLOAD:\n\r " + payload, true, "CredentialRegistry publish failed for " + identifier );
							status = string.Join( ",", contentsJson.Errors.ToArray() );

							LoggingHelper.WriteLogFile( 4, identifier + "_payload_failed", payload, "", false );
							LoggingHelper.WriteLogFile( 4, identifier + "_envelope_failed", postBody, "", false );
							//statusMessage =contents.err contentsJson.Errors.ToString();
						}
						else
						{
							valid = true;
							UpdateEnvelope ue = JsonConvert.DeserializeObject<UpdateEnvelope>( contents );
							crEnvelopeId = ue.EnvelopeIdentifier;

							LoggingHelper.DoTrace( 7, "response: " + JsonConvert.SerializeObject( contents ) );

							LoggingHelper.WriteLogFile( 5, identifier + "_payload_Successful", payload );
							LoggingHelper.WriteLogFile( 6, identifier + "_envelope_Successful", postBody );
						}

						return contents;
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, "RegistryServices.Publish - POST" );
					valid = false;
					status = "Failed on Registry Publish: " + BaseFactory.FormatExceptions( ex );
					return status;
				}
				//Set return values
				//no cr id returned?

			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "RegistryServices.Publish" );

				valid = false;
				crEnvelopeId = "";
				status = "Failed during Registry preperations: " + BaseFactory.FormatExceptions( ex );
				return status;
			}
		}
		//


		//
		private static string GetRequestContext()
		{
			string queryString = "batch";
			try
			{
				queryString = HttpContext.Current.Request.Url.AbsoluteUri.ToString();
			}
			catch ( Exception exc )
			{
				return queryString;
			}
			return queryString;
		}
		private static bool PostRequest( string postBody, string serviceUri, ref string response )
		{
			try
			{
				using ( var client = new HttpClient() )
				{
					client.DefaultRequestHeaders.
						Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );


					var task = client.PostAsync( serviceUri,
						new StringContent( postBody, Encoding.UTF8, "application/json" ) );
					task.Wait();
					var result = task.Result;
					response = JsonConvert.SerializeObject( result );
					var contents = task.Result.Content.ReadAsStringAsync();

					if ( result.IsSuccessStatusCode == false )
					{
						//logging???

						LoggingHelper.LogError( "RegistryServices.PostRequest Failed\n\r" + response + "\n\r" + contents );
					}
					else
					{
						//no doc id?
						LoggingHelper.DoTrace( 6, "result: " + response );
					}


					return result.IsSuccessStatusCode;

				}
			}
			catch ( Exception exc )
			{
				LoggingHelper.LogError( exc, "RegistryServices.PostRequest" );
				return false;

			}

		}
		#endregion 

		#region Deleting
		/// <summary>
		/// Remove a credential from the registry
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="user"></param>
		/// <param name="statusMessage"></param>
		/// <param name="list"></param>
		/// <returns></returns>
		public bool UnregisterCredential( int credentialId,
									Models.AppUser user,
									ref string statusMessage,
									ref List<SiteActivity> list )
		{
			bool successful = true;
			//get credential
			CM.Credential entity = CredentialServices.GetBasicCredential( credentialId );
			if ( entity.Id == 0 )
			{
				statusMessage = "Error - invalid Credential identifier";
				return false;
			}
			if ( !CredentialServices.CanUserUpdateCredential( entity.RowId, user, ref statusMessage ) )
			{
				statusMessage = "Error - not authorized to remove this Credential from the registry";
				return false;
			}

			if ( string.IsNullOrWhiteSpace(entity.CredentialRegistryId))
			{
				statusMessage = "Error - This Credential cannot be removed from the registry as an registry identifier was not found.";
				return false;
			}

			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.CTID, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				new CredentialManager().UnPublish( credentialId, user.Id, ref statusMessage );

				string comment = string.Format( "{0} removed registered credential: {1}. ", user.FullName(), credentialId );

				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Credential", Activity = "Credential Registry", Event = "Removed Credential", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = credentialId } );

				list = ActivityManager.GetPublishHistory( "Credential", credentialId );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not removed from the Credential Registry.";
				else 
				{
					if ( statusMessage == "Couldn't find Envelope" )
					{
						statusMessage = "";
						//just remove the CredentialRegistryId regardless
						if ( new CredentialManager().UnPublish( credentialId, user.Id, ref statusMessage ) )
						{
							statusMessage = "Couldn't find Envelope in registry. The credential has been set to unregistered regardless.";
						}
						else
						{
							statusMessage = "Couldn't find Envelope in registry, and an issue was encountered while attempting to unregister the credential. " + statusMessage;
						}
					}
				}
				successful = false;
			}
			return successful;

		}

		public bool UnregisterOrganization( int orgId,
									Models.AppUser user,
									ref string statusMessage,
									ref List<SiteActivity> list )
		{
			bool successful = true;
			//get credential
			//CM.Organization entity = OrganizationServices.GetLightOrgById( orgId );
			CM.Organization entity = OrganizationServices.GetForSummary( orgId );
			
			if ( entity.Id == 0 )
			{
				statusMessage = "Error - invalid Organization identifier";
				return false;
			}
			if ( !OrganizationServices.CanUserUpdateOrganization( user, entity) )
			{
				statusMessage = "Error - not authorized to remove this Organization from the registry";
				return false;
			}

			if ( string.IsNullOrWhiteSpace( entity.CredentialRegistryId ) )
			{
				statusMessage = "Error - This Organization cannot be removed from the registry as an registry identifier was not found.";
				return false;
			}

			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.ctid, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				if ( new OrganizationManager().UnPublish( orgId, user.Id, ref statusMessage ) )
				{

					string comment = string.Format( "{0} removed registered Organization: {1}. ", user.FullName(), orgId );

					new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Organization", Activity = "Credential Registry", Event = "Removed Organization", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = orgId } );

					list = ActivityManager.GetPublishHistory( "Organization", orgId );
				}
				else
				{

				}
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not removed from the Credential Registry.";
				else
				{
					if ( statusMessage == "Couldn't find Envelope" )
					{
						statusMessage = "";
						//just remove the CredentialRegistryId regardless
						 if ( new OrganizationManager().UnPublish( orgId, user.Id, ref statusMessage ) )
						{
							statusMessage = "Couldn't find Envelope in registry. The organization has been set to unregistered regardless.";
						}
						else
						{
							statusMessage = "Couldn't find Envelope in registry, and an issue was encountered while attempting to unregister the organization. " + statusMessage;
						}
					}
				}
				successful = false;
			}
			return successful;

		}

		public bool Unregister_Assessment( int assessmentId,
								Models.AppUser user,
								ref string statusMessage,
								ref List<SiteActivity> list )
		{
			bool successful = true;
			//get record
			AssessmentProfile entity = AssessmentServices.GetLightAssessmentById( assessmentId );
			if ( entity.Id == 0 )
			{
				statusMessage = "Error - invalid Assessment Profile identifier";
				return false;
			}
			if ( !AssessmentServices.CanUserUpdateAssessment( entity.RowId, user, ref statusMessage ) )
			{
				statusMessage = "Error - not authorized to remove this AssessmentProfile from the registry";
				return false;
			}

			if ( string.IsNullOrWhiteSpace( entity.CredentialRegistryId ) )
			{
				statusMessage = "Error - This AssessmentProfile cannot be removed from the registry as an registry identifier was not found.";
				return false;
			}

			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.ctid, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				new AssessmentManager().UnPublish( assessmentId, user.Id, ref statusMessage );

				string comment = string.Format( "{0} removed registered Assessment: {1}. ", user.FullName(), assessmentId );

				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Assessment", Activity = "Credential Registry", Event = "Removed Assessment", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = assessmentId } );

				list = ActivityManager.GetPublishHistory( "Assessment", assessmentId );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not removed from the Credential Registry.";
				else
				{
					if ( statusMessage == "Couldn't find Envelope" )
					{
						statusMessage = "";
						//just remove the CredentialRegistryId regardless
						if ( new AssessmentManager().UnPublish( assessmentId, user.Id, ref statusMessage ) )
						{
							statusMessage = "Couldn't find Envelope in registry. The Assessment has been set to unregistered regardless.";
						}
						else
						{
							statusMessage = "Couldn't find Envelope in registry, and an issue was encountered while attempting to unregister the Assessment. " + statusMessage;
						}
					}
				}
				successful = false;
			}
			return successful;

		}

		public bool Unregister_LearningOpportunity( int recordId,
								Models.AppUser user,
								ref string statusMessage,
								ref List<SiteActivity> list )
		{
			bool successful = true;
			//get record
			LearningOpportunityProfile entity = LearningOpportunityServices.GetLightLearningOpportunityById( recordId );
			if ( entity.Id == 0 )
			{
				statusMessage = "Error - invalid LearningOpportunity Profile identifier";
				return false;
			}
			if ( !LearningOpportunityServices.CanUserUpdateLearningOpportunity( entity.RowId, user, ref statusMessage ) )
			{
				statusMessage = "Error - not authorized to remove this LearningOpportunityProfile from the registry";
				return false;
			}

			if ( string.IsNullOrWhiteSpace( entity.CredentialRegistryId ) )
			{
				statusMessage = "Error - This LearningOpportunityProfile cannot be removed from the registry as an registry identifier was not found.";
				return false;
			}

			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.ctid, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				new LearningOpportunityManager().UnPublish( recordId, user.Id, ref statusMessage );

				string comment = string.Format( "{0} removed registered LearningOpportunity: {1}. ", user.FullName(), recordId );

				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "LearningOpportunity", Activity = "Credential Registry", Event = "Removed LearningOpportunity", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = recordId } );

				list = ActivityManager.GetPublishHistory( "LearningOpportunity", recordId );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not removed from the Credential Registry.";
				else
				{
					if ( statusMessage == "Couldn't find Envelope" )
					{
						statusMessage = "";
						//just remove the CredentialRegistryId regardless
						if ( new LearningOpportunityManager().UnPublish( recordId, user.Id, ref statusMessage ) )
						{
							statusMessage = "Couldn't find Envelope in registry. The LearningOpportunity has been set to unregistered regardless.";
						}
						else
						{
							statusMessage = "Couldn't find Envelope in registry, and an issue was encountered while attempting to unregister the LearningOpportunity. " + statusMessage;
						}
					}
				}
				successful = false;
			}
			return successful;

		}

		public bool Unregister_ConditionManifest( int recordId,
							Models.AppUser user,
							ref string statusMessage,
							ref List<SiteActivity> list )
		{
			bool successful = true;
			//get record
			CM.ConditionManifest entity = ConditionManifestServices.GetBasic( recordId );
			if ( entity.Id == 0 )
			{
				statusMessage = "Error - invalid ConditionManifest Profile identifier";
				return false;
			}
			if ( !ConditionManifestServices.CanUserUpdateConditionManifest( entity, user, ref statusMessage ) )
			{
				statusMessage = "Error - not authorized to remove this ConditionManifest from the registry";
				return false;
			}

			if ( string.IsNullOrWhiteSpace( entity.CredentialRegistryId ) )
			{
				statusMessage = "Error - This ConditionManifest cannot be removed from the registry as an registry identifier was not found.";
				return false;
			}

			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.CTID, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				new ConditionManifestManager().UnPublish( recordId, user.Id, ref statusMessage );

				string comment = string.Format( "{0} removed registered ConditionManifest: {1}. ", user.FullName(), recordId );

				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "ConditionManifest", Activity = "Credential Registry", Event = "Removed ConditionManifest", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = recordId } );

				list = ActivityManager.GetPublishHistory( "ConditionManifest", recordId );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not removed from the Credential Registry.";
				else
				{
					if ( statusMessage == "Couldn't find Envelope" )
					{
						statusMessage = "";
						//just remove the CredentialRegistryId regardless
						if ( new ConditionManifestManager().UnPublish( recordId, user.Id, ref statusMessage ) )
						{
							statusMessage = "Couldn't find Envelope in registry. The ConditionManifest has been set to unregistered regardless.";
						}
						else
						{
							statusMessage = "Couldn't find Envelope in registry, and an issue was encountered while attempting to unregister the ConditionManifest. " + statusMessage;
						}
					}
				}
				successful = false;
			}
			return successful;

		}

		public bool Unregister_CostManifest( int recordId,
							Models.AppUser user,
							ref string statusMessage,
							ref List<SiteActivity> list )
		{
			bool successful = true;
			//get record
			CM.CostManifest entity = CostManifestServices.GetBasic( recordId );
			if ( entity.Id == 0 )
			{
				statusMessage = "Error - invalid CostManifest Profile identifier";
				return false;
			}
			if ( !CostManifestServices.CanUserUpdateCostManifest( entity, user, ref statusMessage ) )
			{
				statusMessage = "Error - not authorized to remove this CostManifest from the registry";
				return false;
			}

			if ( string.IsNullOrWhiteSpace( entity.CredentialRegistryId ) )
			{
				statusMessage = "Error - This CostManifest cannot be removed from the registry as an registry identifier was not found.";
				return false;
			}

			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.CTID, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				new CostManifestManager().UnPublish( recordId, user.Id, ref statusMessage );

				string comment = string.Format( "{0} removed registered CostManifest: {1}. ", user.FullName(), recordId );

				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "CostManifest", Activity = "Credential Registry", Event = "Removed CostManifest", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = recordId } );

				list = ActivityManager.GetPublishHistory( "CostManifest", recordId );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not removed from the Credential Registry.";
				else
				{
					if ( statusMessage == "Couldn't find Envelope" )
					{
						statusMessage = "";
						//just remove the CredentialRegistryId regardless
						if ( new CostManifestManager().UnPublish( recordId, user.Id, ref statusMessage ) )
						{
							statusMessage = "Couldn't find Envelope in registry. The CostManifest has been set to unregistered regardless.";
						}
						else
						{
							statusMessage = "Couldn't find Envelope in registry, and an issue was encountered while attempting to unregister the CostManifest. " + statusMessage;
						}
					}
				}
				successful = false;
			}
			return successful;

		}



		public bool CredentialRegistry_Delete( string crEnvelopeId, string ctid, string requestedBy, ref string statusMessage)
		{
			string publicKeyPath = "";
			string privateKeyPath = "";
			if (GetKeys(ref publicKeyPath, ref privateKeyPath, ref statusMessage) == false) 
			{
				return false;
			}
			//crEnvelopeId, 
			DeleteEnvelope envelope = RegistryHandler.CreateDeleteEnvelope( publicKeyPath, privateKeyPath, ctid, requestedBy );

			string serviceUri = string.Format( UtilityManager.GetAppKeyValue( "credentialRegistryGet" ), crEnvelopeId );


			string postBody = JsonConvert.SerializeObject( envelope );
			string response = "";
			if ( !DeleteRequest( postBody, serviceUri, ref response ) )
			{
				//failed
				//not sure what to use for a statusMessage message
				statusMessage = response;
				return false;	
			}

			return true;
		}

		
		
		private static bool DeleteRequest( string postBody, string serviceUri, ref string response )
		{
			try
			{
				using ( var client = new HttpClient() )
				{
					HttpRequestMessage request = new HttpRequestMessage
					{
						Content = new StringContent( postBody, Encoding.UTF8, "application/json" ),
						Method = HttpMethod.Delete,
						RequestUri = new Uri( serviceUri )
					};
					var task = client.SendAsync( request );


					//client.DefaultRequestHeaders.
					//	Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );


					//var task = client.PutAsync( serviceUri,
					//	new StringContent( postBody, Encoding.UTF8, "application/json" ) );
					task.Wait();
					var result = task.Result;
					response = JsonConvert.SerializeObject( result );
					var contents = task.Result.Content.ReadAsStringAsync();

					if ( result.IsSuccessStatusCode == false )
					{
						//logging???
						//response = contents.Result;
						LoggingHelper.LogError( "RegistryServices.DeleteRequest Failed\n\r" + response + "\n\rError: " + JsonConvert.SerializeObject( contents ) );

						RegistryResponseContent contentsJson = JsonConvert.DeserializeObject<RegistryResponseContent>( contents.Result );
						response = string.Join( "<br/>", contentsJson.Errors.ToArray() );
					}
					else
					{
						LoggingHelper.DoTrace( 6, "result: " + response );
					}


					return result.IsSuccessStatusCode;

				}
			}
			catch ( Exception exc )
			{
				LoggingHelper.LogError( exc, "RegistryServices.DeleteRequest" );
				return false;

			}

		}

		#endregion 
		#region Helpers
		private static bool GetKeys( ref string publicKeyPath, ref string privateKeyPath, ref string statusMessage )
		{
			bool isValid = true;
			//TODO - validate files exist - some issue on test server???

			string privateKeyLocation = UtilityManager.GetAppKeyValue( "privateKeyLocation", "" );
			if ( string.IsNullOrWhiteSpace( privateKeyLocation ) )
			{
				statusMessage = "Error - missing application key of privateKeyLocation";
				return false;
			}
			string publicKeyLocation = UtilityManager.GetAppKeyValue( "pemKeyLocation", "" );
			if ( string.IsNullOrWhiteSpace( publicKeyLocation ) )
			{
				statusMessage = "Error - missing application key of pemKeyLocation";
				return false;
			}
			//processing for dev env - where full path will vary by machine
			//could use a common location like @logs\keys??
			//if it works, then adjust the value stored in appkeys
			//doens't work = mike.parsons\appData\roaming
			//var fileName = Path.Combine( Environment.GetFolderPath( 	Environment.SpecialFolder.ApplicationData ), publicKeyLocation );

			if ( publicKeyLocation.ToLower().StartsWith( "c:\\" ) )
				publicKeyPath = publicKeyLocation;
			else
				publicKeyPath = Path.Combine( HttpRuntime.AppDomainAppPath, publicKeyLocation );
			//publicKeyData = File.ReadAllText( signingKeyPath );

			if ( privateKeyLocation.ToLower().StartsWith( "c:\\" ) )
				privateKeyPath = privateKeyLocation;
			else
				privateKeyPath = Path.Combine( HttpRuntime.AppDomainAppPath, privateKeyLocation );

			LoggingHelper.DoTrace( 4, string.Format( "files: private: {0}, \r\npublic: {1}", privateKeyPath, publicKeyPath ) );
			if ( !System.IO.File.Exists( privateKeyPath ) )
			{
				statusMessage = "Error - the encoding key was not found";
				isValid = false;
			}
			if ( !System.IO.File.Exists( publicKeyPath ) )
			{
				statusMessage = "Error - the public key was not found";
				isValid = false;
			}
			return isValid;
		}

		#endregion
		#region Reading

		public static ReadEnvelope CredentialRegistry_Get( string crEnvelopeId,
									ref string statusMessage )
		{
			string document = "";
			string serviceUri = string.Format( UtilityManager.GetAppKeyValue( "credentialRegistryGet" ), crEnvelopeId );
			ReadEnvelope envelope = new ReadEnvelope();
			Models.Json.Credential credential = new Models.Json.Credential();
			try
			{

				// Create a request for the URL.         
				WebRequest request = WebRequest.Create( serviceUri );

				// If required by the server, set the credentials.
				request.Credentials = CredentialCache.DefaultCredentials;

				//Get the response.
				HttpWebResponse response = ( HttpWebResponse ) request.GetResponse();

				// Get the stream containing content returned by the server.
				Stream dataStream = response.GetResponseStream();

				// Open the stream using a StreamReader for easy access.
				StreamReader reader = new StreamReader( dataStream );
				// Read the content.
				document = reader.ReadToEnd();

				// Cleanup the streams and the response.

				reader.Close();
				dataStream.Close();
				response.Close();

				//map to the default envelope
				envelope = JsonConvert.DeserializeObject<ReadEnvelope>( document );

				credential = JsonConvert.DeserializeObject<Models.Json.Credential>( envelope.DecodedResource.ToString() );

			}
			catch ( Exception exc )
			{
				LoggingHelper.LogError( exc, "RegistryServices.CredentialRegistry_Get" );
			}
			return envelope;

		}


		public static List<ReadEnvelope> CredentialRegistry_GetLatest( string type, string startingDate, string endingDate, int pageNbr, int pageSize, ref string statusMessage )
		{
			string document = "";
			string filter = "";
			string serviceUri = UtilityManager.GetAppKeyValue( "credentialRegistrySearch" );
			//from=2016-08-22T00:00:00&until=2016-08-31T23:59:59
			//resource_type=credential
			if ( !string.IsNullOrWhiteSpace( type ) )
				filter = string.Format( "resource_type={0}", type.ToLower() );

			SetPaging( pageNbr, pageSize, ref filter );
			SetDateFilters( startingDate, endingDate, ref filter );

			serviceUri += filter.Length > 0 ? "?" + filter : "";

			List<ReadEnvelope> list = new List<ReadEnvelope>();
			ReadEnvelope envelope = new ReadEnvelope();
			Models.Json.Credential credential = new Models.Json.Credential();

			try
			{

				// Create a request for the URL.         
				WebRequest request = WebRequest.Create( serviceUri );

				// If required by the server, set the credentials.
				request.Credentials = CredentialCache.DefaultCredentials;

				//Get the response.
				HttpWebResponse response = ( HttpWebResponse ) request.GetResponse();

				// Get the stream containing content returned by the server.
				Stream dataStream = response.GetResponseStream();
				
				// Open the stream using a StreamReader for easy access.
				StreamReader reader = new StreamReader( dataStream );
				// Read the content.
				document = reader.ReadToEnd();

				// Cleanup the streams and the response.

				reader.Close();
				dataStream.Close();
				response.Close();

				//Link contains links for paging
				var hdr = response.GetResponseHeader( "Link" );
				int total = 0;
				Int32.TryParse( response.GetResponseHeader( "Total"), out total );

				//map to the default envelope
				list = JsonConvert.DeserializeObject < List<ReadEnvelope> > ( document );

				//or maybe do this in caller
				foreach ( ReadEnvelope item in list )
				{
					credential = new Models.Json.Credential();
					credential = JsonConvert.DeserializeObject<Models.Json.Credential>( item.DecodedResource.ToString() );

					//TODO add to a list
				}
				

			}
			catch ( Exception exc )
			{
				LoggingHelper.LogError( exc, "RegistryServices.CredentialRegistry_GetLatest" );
			}
			return list;
}

		private static void SetPaging( int pageNbr, int pageSize, ref string where )
		{
			string AND = "";
			if ( where.Length > 0 )
				AND = "&";

			if ( pageNbr > 0 )
			{
				where = where + AND + string.Format( "page={0}", pageNbr );
				AND = "&";
			}
			if ( pageSize > 0 )
			{
				where = where + AND + string.Format( "per_page={0}", pageSize );
				AND = "&";
			}
		}

		private static void SetDateFilters( string startingDate, string endingDate, ref string where )
		{
			string AND = "";
			if ( where.Length > 0 )
				AND = "&";

			string date = FormatDateFilter( startingDate );
			if ( !string.IsNullOrWhiteSpace( date ) )
			{
				where = where + AND + string.Format( "from={0}", startingDate );
				AND = "&";
			}

			date = FormatDateFilter( endingDate );
			if ( !string.IsNullOrWhiteSpace( date ) )
			{
				where = where + AND + string.Format( "until={0}", startingDate );
				AND = "&";
			}
			//if ( !string.IsNullOrWhiteSpace( endingDate ) && endingDate.Length == 10 )
			//{
			//	where = where + AND + string.Format( "until={0}T23:59:59", endingDate );
			//}
		}
		private static string FormatDateFilter( string date )
		{
			string formatedDate = "";
			if ( string.IsNullOrWhiteSpace( date ) )
				return "";

			//start by checking for just properly formatted date
			if ( !string.IsNullOrWhiteSpace( date ) && date.Length == 10 )
			{
				formatedDate = string.Format( "{0}T00:00:00", date );
			}
			else if ( !string.IsNullOrWhiteSpace( date ) )
			{
				//check if in proper format - perhaps with time provided
				if ( date.IndexOf( "T" ) > 8 )
				{
					formatedDate =string.Format( "{0}", date );
				}
				else
				{
					//not sure how to handle unexpected date except to ignore
					//might be better to send actual DateTime field
				}
			}

			return formatedDate;
		}
		#endregion 
	}
}
