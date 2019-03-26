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
using CredentialRegistry;
using RAS = RegistryAssistantServices;

namespace CTIServices
{
	public class RegistryServices
	{
		static string thisClassName = "RegistryServices";
		static bool enforcingMinimumDataChecksOnPublish = UtilityManager.GetAppKeyValue( "enforcingMinimumDataChecksOnPublish", true );
		bool usingRegistryAssistant = UtilityManager.GetAppKeyValue( "usingRegistryAssistantForMapping", false );


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

			List<string> messages = new List<string>();
			if ( enforcingMinimumDataChecksOnPublish) 
			{
                if ( CredentialManager.ValidateProfile( entity, ref messages, false ) == false )
                {
                    statusMessage = "Error - Missing minimum required data. " + string.Join( "<br/>", messages.ToArray() );
                    LoggingHelper.DoTrace( 1, string.Format( "PublishCredential for recordId: {0} failed due to missing minimum required data. " + string.Join( "\r\n", messages.ToArray() ), credentialId ) );
                    return false;
                } else if (!entity.OwningOrganization.IsPublished)
                {
                    statusMessage = string.Format( "Error - Owning Organization ({0}) is not published. It must be published before any of the owned artifacts can be published", entity.OwningOrganization.Id) ;
                    LoggingHelper.DoTrace( 1, string.Format( "PublishCredential for recordId: {0} failed due to missing minimum required data. " + statusMessage, credentialId ) );
                    return false;
                }
			}

			list  = new List<SiteActivity>();
			bool successful = true;
			
			string action = "";
			string comment = "";
			var payload = "";
			string crEnvelopeId = entity.CredentialRegistryId;
			var apikey = OrganizationServices.GetAccountOrganizationApikey( entity.OwningOrganization.ctid );

			payload = RAS.CredentialMapper.AssistantRequest( entity, "publish", apikey, user, ref successful, ref messages, ref crEnvelopeId );
				if ( !successful )
				{
					statusMessage = string.Format( "Errors encountered attempting to publish credential: {0} for publish. ", credentialId ) + string.Join( "<br/>", messages.ToArray() );
					LoggingHelper.DoTrace( 1, string.Format( "PublishCredential for recordId: {0} had errors. " + string.Join( "\r\n", messages.ToArray() ), credentialId ) );
					return false;
				} else
					statusMessage = string.Join( "<br/>", messages.ToArray() );

				//eventually do the format and publish in one call
			//}
			//else
			//{
			//	payload = new JsonLDServices().GetCredentialV2ForRegistry( entity ).ToString();
			//	successful = Publish( payload.ToString(), user.FullName(), "credential_" + credentialId.ToString(), ref statusMessage, ref crEnvelopeId );
			//}

			if ( successful )
			{
				//call update regardless
				new CredentialManager().UpdateEnvelopeId( credentialId, crEnvelopeId, user.Id, ref statusMessage );
				//if was an add, update CredentialRegistryId
				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Registered Credential";
					comment = string.Format( "{0} registered credential: {1}. Returned envelopeId: {2}", user.FullName(), entity.Name, crEnvelopeId );
				}
				else
				{
					action = "Updated Credential";
					comment = string.Format( "{0} updated previously registered credential: '{1}' ({2}). Returned envelopeId: {3}", user.FullName(), entity.Name, credentialId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Credential", Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = credentialId, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

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
			bool publishManifests = UtilityManager.GetAppKeyValue( "publishManifestsWithOrg" ,false);

			return PublishOrganization( orgId, user, ref statusMessage, ref list, publishManifests );
		}

		/// <summary>
		/// publish an organization to the registry
		/// </summary>
		/// <param name="orgId"></param>
		/// <param name="user"></param>
		/// <param name="statusMessage"></param>
		/// <param name="list"></param>
		/// <param name="publishManifests">If true, all cost and condition manifests will be published at the same time as the org - pending evaluation of performance.</param>
		/// <returns></returns>
		public bool PublishOrganization( int orgId, Models.AppUser user, ref string statusMessage, ref List<SiteActivity> list, bool publishingManifestsWithOrg = false )
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

            List<string> messages = new List<string>();
            if ( enforcingMinimumDataChecksOnPublish  )
			{
                if ( OrganizationManager.HasMinimumData( entity, ref messages, false ) == false )
                {
                    statusMessage = "Error - Missing minimum required data. " + string.Join( "<br/>", messages.ToArray() );
                    LoggingHelper.DoTrace( 1, string.Format( "PublishOrganization for recordId: {0} failed due to missing minimum required data. " + statusMessage, orgId ) );
                    return false;
                } else
                {
                    //need a check where current is a child org, and parent not published
                }
            } 
			string crEnvelopeId = entity.CredentialRegistryId;
			bool successful = true;
			string action = "";
			string comment = "";
			string apikey = OrganizationServices.GetAccountOrganizationApikey( entity.ctid );

			var payload = RAS.OrganizationMapper.AssistantRequest( entity, "publish", apikey, user, ref successful, ref messages, ref crEnvelopeId );
			if ( !successful )
			{
				statusMessage = string.Format("Errors encountered attempting to publish org: {0} for publish. ", orgId) + string.Join( "<br/>", messages.ToArray() );
				LoggingHelper.DoTrace( 1, string.Format( "PublishOrganization for recordId: {0} had errors. " + string.Join( "\r\n", messages.ToArray() ), orgId ) );

				return false;
			} else
				statusMessage = string.Join( "<br/>", messages.ToArray() );


			if ( successful )
			{
				//call update regardless
				new OrganizationManager().UpdateEnvelopeId( orgId, crEnvelopeId, user.Id, ref statusMessage );

				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Registered Organization";
					comment = string.Format( "{0} registered Organization: {1}. Returned envelopeId: {2}", user.FullName(), entity.Name, crEnvelopeId );
					
				}
				else
				{
					action = "Updated Organization";
					comment = string.Format( "{0} updated previously registered Organization: '{1}' ({2}). Returned envelopeId: {3}", user.FullName(), entity.Name, orgId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Organization", Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = orgId, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.ctid } );

				list = ActivityManager.GetPublishHistory( "Organization", orgId );

				if ( publishingManifestsWithOrg )
				{
					string manifestsStatus = "";
					//publish all manifests for org - temp workaround
					messages.Add( "Also checking for Condition Manifests to publish" );
					PublishConditionManifests( entity, user, ref manifestsStatus,
								ref list );
					if ( !string.IsNullOrWhiteSpace( manifestsStatus ) )
						messages.Add( manifestsStatus );
					messages.Add( "Also checking for Cost Manifests to publish" );
					PublishCostManifests( entity, user, ref manifestsStatus,
								ref list );
					if ( !string.IsNullOrWhiteSpace( manifestsStatus ) )
						messages.Add( manifestsStatus );

					statusMessage = string.Join( "<br/>", messages.ToArray() );
				}
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

			AssessmentProfile entity = AssessmentServices.GetForPublish( assessmentId, user );
			if ( !entity.CanEditRecord )
			{
				statusMessage = "Error - not authorized to publish this Assessment to the registry";
				return false;
			}

			List<string> messages = new List<string>();
            if ( enforcingMinimumDataChecksOnPublish )
            {
                if ( AssessmentManager.HasMinimumData( entity, ref messages, false ) == false )
                {
                    statusMessage = "Error - Missing minimum required data. " + string.Join( "<br/>", messages.ToArray() );
                    LoggingHelper.DoTrace( 1, string.Format( "PublishAssessment for recordId: {0}, Org: {1}, ({2}) failed due to missing minimum required data. " + string.Join( "\r\n", messages.ToArray() ), assessmentId, entity.OwningOrganization.Name, entity.OwningOrganization.Id ) );
                    return false;
                }
                else if ( !entity.OwningOrganization.IsPublished )
                {
                    statusMessage = string.Format( "Error - Owning Organization ({0}) is not published. It must be published before any of the owned artifacts can be published", entity.OwningOrganization.Id );
                    LoggingHelper.DoTrace( 1, string.Format( "PublishAssessment for recordId: {0} failed due to missing minimum required data. " + statusMessage, assessmentId ) );
                    return false;
                }
            }

			list = new List<SiteActivity>();
			string action = "";
			string comment = "";
			string crEnvelopeId = entity.CredentialRegistryId;
			var apikey = OrganizationServices.GetAccountOrganizationApikey( entity.OwningOrganization.ctid );

			var payload = RAS.AssessmentMapper.AssistantRequest( entity, "publish", apikey, user, ref successful, ref messages , ref crEnvelopeId);
			if ( !successful )
			{
				statusMessage = string.Format( "Errors encountered attempting to publish asmt: {0} for publish. ", assessmentId ) + string.Join( "<br/>", messages.ToArray() );
				LoggingHelper.DoTrace( 1, string.Format( "PublishAssessment for recordId: {0} had errors. " + string.Join( "\r\n", messages.ToArray() ), assessmentId ) );
				return false;
			}
			else
				statusMessage = string.Join( "<br/>", messages.ToArray() );

			if ( successful )
			{
				//call update regardless
				new AssessmentManager().UpdateEnvelopeId( assessmentId, crEnvelopeId, user.Id, ref statusMessage );
				//if was an add, update CredentialRegistryId
				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Registered Assessment";
					comment = string.Format( "{0} registered Assessment: {1}. Returned envelopeId: {2}", user.FullName(), entity.Name, crEnvelopeId );
				}
				else
				{
					action = "Updated Assessment";
					comment = string.Format( "{0} updated previously registered Assessment: '{1}' ({2}). Returned envelopeId: {3}", user.FullName(), entity.Name, assessmentId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = SiteActivity.AssessmentType, Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = assessmentId, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

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

			List<string> messages = new List<string>();
            if ( enforcingMinimumDataChecksOnPublish )
            {
                //If the current lopp is a child of another lopp that is associated with a credential, then allow publishing 
                if ( LearningOpportunityManager.HasMinimumData( entity, ref messages, false ) == false )
                {
                    statusMessage = "Error - Missing minimum required data. " + string.Join( "<br/>", messages.ToArray() );
                    LoggingHelper.DoTrace( 1, string.Format( "PublishLearningOpportunity for recordId: {0}, Org: {1}, ({2}) failed due to missing minimum required data. " + string.Join( "\r\n", messages.ToArray() ), learningOppId, entity.OwningOrganization.Name, entity.OwningOrganization.Id ) );
                    return false;
                }
                else if ( !entity.OwningOrganization.IsPublished )
                {
                    statusMessage = string.Format( "Error - Owning Organization ({0}) is not published. It must be published before any of the owned artifacts can be published", entity.OwningOrganization.Id );
                    LoggingHelper.DoTrace( 1, string.Format( "PublishLearningOpportunity for recordId: {0} failed due to missing minimum required data. " + statusMessage, learningOppId ) );
                    return false;
                }
            }

			list = new List<SiteActivity>();
			string action = "";
			string comment = "";
			string crEnvelopeId = entity.CredentialRegistryId;
			var apikey = OrganizationServices.GetAccountOrganizationApikey( entity.OwningOrganization.ctid );

			var payload = RAS.LearningOpportunityMapper.AssistantRequest( entity, "publish", apikey, user, ref successful, ref messages, ref crEnvelopeId );
			if ( !successful )
			{
				statusMessage = string.Format( "Errors encountered attempting to publish lopp: {0} for publish. ", learningOppId ) + string.Join( "<br/>", messages.ToArray() );
				LoggingHelper.DoTrace( 1, string.Format( "PublishLearningOpportunity for recordId: {0} had errors. " + string.Join( "\r\n", messages.ToArray() ), learningOppId ) );

				return false;
			}
			else
				statusMessage = string.Join( "<br/>", messages.ToArray() );
			
			if ( successful )
			{
				//call update regardless
				new LearningOpportunityManager().UpdateEnvelopeId( learningOppId, crEnvelopeId, user.Id, ref statusMessage );
				//if was an add, update CredentialRegistryId
				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Registered LearningOpportunity";
					comment = string.Format( "{0} registered LearningOpportunity: {1}. Returned envelopeId: {2}", user.FullName(), entity.Name, crEnvelopeId );
				}
				else
				{
					action = "Updated LearningOpportunity";
					comment = string.Format( "{0} updated previously registered LearningOpportunity: '{1}' ({2}). Returned envelopeId: {3}", user.FullName(), entity.Name, learningOppId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = SiteActivity.LearningOpportunity, Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = learningOppId, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

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
		public bool PublishConditionManifests( CM.Organization org,
								Models.AppUser user,
								ref string statusMessage,
								ref List<SiteActivity> history )
		{
			bool successful = true;
			statusMessage = "";
			foreach (var item in org.HasConditionManifest)
			{
				PublishConditionManifest( item.Id, user, ref statusMessage, ref history );
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

			List<string> messages = new List<string>();
			if ( ConditionManifestManager.ValidateProfile( entity, ref messages, false ) == false )
			{
				statusMessage = "Error - Missing minimum required data. " + string.Join( "<br/>", messages.ToArray() );
				LoggingHelper.DoTrace( 1, string.Format( "PublishConditionManifest for manifestId: {0} failed due to missing minimum required data. " + string.Join( "\r\n", messages.ToArray()), manifestId) );
				return false;
			}
            else if ( !entity.OwningOrganization.IsPublished )
            {
                statusMessage = string.Format( "Error - Owning Organization ({0}) is not published. It must be published before any of the manifests can be published", entity.OwningOrganization.Id );
                LoggingHelper.DoTrace( 1, string.Format( "PublishConditionManifest for recordId: {0} failed due to missing minimum required data. " + statusMessage, manifestId ) );
                return false;
            }
            string crEnvelopeId = entity.CredentialRegistryId;
			var apikey = OrganizationServices.GetAccountOrganizationApikey( entity.OwningOrganization.ctid );

			var payload = RAS.ConditionManifestMapper.AssistantRequest( entity, "publish", apikey, user, ref successful, ref messages, ref crEnvelopeId );
			if ( !successful )
			{
				statusMessage = string.Format( "Errors encountered attempting to publish condition manifest for publish. Id: {0}, Name: {1}, Organization: {1} ", manifestId, entity.OwningOrganization.Id, entity.OwningOrganization.Name ) + string.Join( "<br/>", messages.ToArray() );
				LoggingHelper.DoTrace( 1, "PublishConditionManifest. " + statusMessage );

				return false;
			}
			else
				statusMessage = string.Join( "<br/>", messages.ToArray() );

			
			if ( successful )
			{
				//call update regardless
				new ConditionManifestManager().UpdateEnvelopeId( manifestId, crEnvelopeId, user.Id, ref statusMessage );
				//if was an add, update CredentialRegistryId
				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Registered";
					comment = string.Format( "{0} registered ConditionManifest: {1}. Returned envelopeId: {2}", user.FullName(), entity.Name, crEnvelopeId );
				}
				else
				{
					action = "Updated";
					comment = string.Format( "{0} updated previously registered ConditionManifest: {1}. Returned envelopeId: {2}", user.FullName(), manifestId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Condition Manifest", Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = manifestId, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

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

		public bool PublishCostManifests( CM.Organization org,
								Models.AppUser user,
								ref string statusMessage,
								ref List<SiteActivity> history )
		{
			bool successful = true;
			statusMessage = "";
			foreach ( var item in org.HasCostManifest )
			{
				PublishCostManifest( item.Id, user, ref statusMessage, ref history );
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
			string crEnvelopeId = entity.CredentialRegistryId;

			List<string> messages = new List<string>();
			if ( CostManifestManager.ValidateProfile( entity, ref messages, false ) == false )
			{
				statusMessage = "Error - Missing minimum required data. " + string.Join( "<br/>", messages.ToArray() );
				LoggingHelper.DoTrace( 1, string.Format( "PublishCostManifest for manifestId: {0} failed due to missing minimum required data. " + string.Join( "\r\n", messages.ToArray() ), manifestId ) );
				return false;
			}
            else if ( !entity.OwningOrganization.IsPublished )
            {
                statusMessage = string.Format("Error - Owning Organization ({0}) is not published. It must be published before any of the manifests can be published", entity.OwningOrganization.Id);
                LoggingHelper.DoTrace( 1, string.Format( "PublishCostManifest for recordId: {0} failed due to missing minimum required data. " + statusMessage, manifestId ) );
                return false;
            }
			var apikey = OrganizationServices.GetAccountOrganizationApikey( entity.OwningOrganization.ctid );

			var payload = RAS.CostManifestMapper.AssistantRequest( entity, "publish", apikey, user, ref successful, ref messages, ref crEnvelopeId );
			if ( !successful )
			{
                statusMessage = string.Format( "Errors encountered attempting to publish cost manifest for publish. Id: {0}, Name: {1}, Organization: {1} ", manifestId, entity.OwningOrganization.Id, entity.OwningOrganization.Name ) + string.Join( "<br/>", messages.ToArray() );
                LoggingHelper.DoTrace( 1, "PublishCostManifest. " + statusMessage );

            return false;
			}
			else
				statusMessage = string.Join( "<br/>", messages.ToArray() );

			
			if ( successful )
			{
				//call update regardless
				new CostManifestManager().UpdateEnvelopeId( manifestId, crEnvelopeId, user.Id, ref statusMessage );
				//if was an add, update CredentialRegistryId
				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Registered";
					comment = string.Format( "{0} registered CostManifest: {1}. Returned envelopeId: {2}", user.FullName(), entity.Name, crEnvelopeId );
				}
				else
				{
					action = "Updated";
					comment = string.Format( "{0} updated previously registered CostManifest: {1}. Returned envelopeId: {2}", user.FullName(), manifestId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Cost Manifest", Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = manifestId, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

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
        /// the check for user being able to publish has already been done.
        /// </summary>
        /// <param name="frameworkCTID"></param>
        /// <param name="frameworkExportJSON"></param>
        /// <param name="user"></param>
        /// <param name="statusMessage"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool PublishCompetencyFramework( CM.CASS_CompetencyFramework entity, Models.AppUser user, ref List<string> messages, ref List<SiteActivity> list )
        {
            bool successful = true;

            string action = "";
            string comment = "";
            string crEnvelopeId = entity.CredentialRegistryId;
			var apikey = OrganizationServices.GetAccountOrganizationApikey( entity.OwningOrganization.ctid );

			var payload = "";
			try
			{
				payload = RAS.CASS_CompetencyFrameworkMapper.AssistantRequest( entity, "publish", apikey, user, ref successful, ref messages, ref crEnvelopeId );
				if ( !successful )
				{
					//statusMessage = "Errors encountered attempting to publish data for publish. " + string.Join( "<br/>", messages.ToArray() );
					LoggingHelper.DoTrace( 1, string.Format( "PublishCompetencyFramework for recordId: {0} had errors. ", entity.Id ) + string.Join( "\r\n", messages.ToArray() ) );

					return false;
				}

				string statusMessage = "";
				if ( successful )
				{
					//call update regardless
					new CASS_CompetencyFrameworkManager().UpdateEnvelopeId( entity.Id, crEnvelopeId, user.Id, ref statusMessage );
					//if was an add, update CredentialRegistryId
					if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
					{
						action = "Registered CASS_CompetencyFramework";
						comment = string.Format( "{0} registered CASS_CompetencyFramework: {1} ({2}). Returned envelopeId: {3}", user.FullName(), entity.FrameworkName, entity.Id, crEnvelopeId );
					}
					else
					{
						action = "Updated CASS_CompetencyFramework";
						comment = string.Format( "{0} updated previously registered CASS_CompetencyFramework: {1} ({2}). Returned envelopeId: {3}", user.FullName(), entity.FrameworkName, entity.Id, crEnvelopeId );

					}
					new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "CASS_CompetencyFramework", Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = entity.Id, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

					if ( user.FullName().IndexOf( "Incomplete -" ) > -1 )
					{
						LoggingHelper.LogError( string.Format( thisClassName + ".PublishCompetencyFramework() Error - encountered user with incomplete profile - or more likely issue at login/session creation. User: {0}", user.Email ), true );
					}

					list = ActivityManager.GetPublishHistory( "CASS_CompetencyFramework", entity.Id );
				}
				else
				{
					//ensure a message is returned
					if ( messages.Count == 0 )
						messages.Add( "The document was not saved in the Credential Registry." );
					successful = false;
				}
			} catch(Exception ex)
			{
				string msg = ServiceHelper.FormatExceptions( ex );
				LoggingHelper.LogError( ex, string.Format( "RegistryServices.PublishCompetencyFramework(). Exception occurred for framework: {0} ({1}) ", entity.FrameworkName, entity.CTID ) );
				messages.Add( string.Format("RegistryServices.PublishCompetencyFramework(). Exception occurred for framework: {0} ({1}) ", entity.FrameworkName, entity.CTID ));
				successful = false;
			}
            return successful;
        }

		public bool PublishConceptScheme( CM.ConceptScheme entity, Models.AppUser user, ref List<string> messages, ref List<SiteActivity> list )
		{
			bool successful = true;

			string action = "";
			string comment = "";
			string crEnvelopeId = entity.CredentialRegistryId;
			var apikey = OrganizationServices.GetAccountOrganizationApikey( entity.OwningOrganization.ctid );
			var payload = "";
			try
			{
				//TBD
				payload = RAS.ConceptSchemeMapper.AssistantRequest( entity, "publish", apikey, user, ref successful, ref messages, ref crEnvelopeId );
				if ( !successful )
				{
					//statusMessage = "Errors encountered attempting to publish data for publish. " + string.Join( "<br/>", messages.ToArray() );
					LoggingHelper.DoTrace( 1, string.Format( "PublishConceptScheme for recordId: {0} had errors. ", entity.Id ) + string.Join( "\r\n", messages.ToArray() ) );

					return false;
				}

				string statusMessage = "";
				if ( successful )
				{
					//call update regardless
					new ConceptSchemeManager().UpdateEnvelopeId( entity.Id, crEnvelopeId, user.Id, ref statusMessage );
					//if was an add, update CredentialRegistryId
					if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
					{
						action = "Registered ConceptScheme";
						comment = string.Format( "{0} registered ConceptScheme: {1}. Returned envelopeId: {2}", user.FullName(), entity.Name, crEnvelopeId );
					}
					else
					{
						action = "Updated ConceptScheme";
						comment = string.Format( "{0} updated previously registered ConceptScheme: {1}. Returned envelopeId: {2}", user.FullName(), entity.Id, crEnvelopeId );

					}
					new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "ConceptScheme", Activity = "Credential Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = entity.Id, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

					if ( user.FullName().IndexOf( "Incomplete -" ) > -1 )
					{
						LoggingHelper.LogError( string.Format( thisClassName + ".PublishConceptScheme() Error - encountered user with incomplete profile - or more likely issue at login/session creation. User: {0}", user.Email ), true );
					}

					list = ActivityManager.GetPublishHistory( "ConceptScheme", entity.Id );
				}
				else
				{
					//ensure a message is returned
					if ( messages.Count == 0 )
						messages.Add( "The document was not saved in the Credential Registry." );
					successful = false;
				}
			}
			catch ( Exception ex )
			{
				string msg = ServiceHelper.FormatExceptions( ex );
				LoggingHelper.LogError( ex, "RegistryServices.PublishConceptScheme()" );
				messages.Add( string.Format( "RegistryServices.PublishConceptScheme(). Exception occurred for conceptScheme: {0} ({1}) ", entity.Name, entity.CTID ) );
				successful = false;
			}
			return successful;
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
		public bool Unregister_Credential( int credentialId,
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
				return true;
			}

			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.CTID, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				new CredentialManager().UnPublish( credentialId, user.Id, ref statusMessage );

				string comment = string.Format( "{0} removed registered credential: {1}. ", user.FullName(), credentialId );

				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Credential", Activity = "Credential Registry", Event = "Removed Credential", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = credentialId, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

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

		public bool Unregister_Organization( int orgId,
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
				return true;
			}

			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.ctid, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				if ( new OrganizationManager().UnPublish( orgId, user.Id, ref statusMessage ) )
				{

					string comment = string.Format( "{0} removed registered Organization: {1}. ", user.FullName(), orgId );

					new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Organization", Activity = "Credential Registry", Event = "Removed Organization", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = orgId, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.ctid } );

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
			AssessmentProfile entity = AssessmentServices.GetBasic( assessmentId );
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
				return true;
			}

			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.ctid, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				new AssessmentManager().UnPublish( assessmentId, user.Id, ref statusMessage );

				string comment = string.Format( "{0} removed registered Assessment: {1}. ", user.FullName(), assessmentId );

				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "AssessmentProfile", Activity = "Credential Registry", Event = "Removed Assessment", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = assessmentId, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

				list = ActivityManager.GetPublishHistory( "AssessmentProfile", assessmentId );
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
			LearningOpportunityProfile entity = LearningOpportunityServices.GetBasic( recordId );
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
				return true;
			}

			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.ctid, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				new LearningOpportunityManager().UnPublish( recordId, user.Id, ref statusMessage );

				string comment = string.Format( "{0} removed registered LearningOpportunity: {1}. ", user.FullName(), recordId );

				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "LearningOpportunity", Activity = "Credential Registry", Event = "Removed LearningOpportunity", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = recordId, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

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
				return true;
			}

			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.CTID, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				new ConditionManifestManager().UnPublish( recordId, user.Id, ref statusMessage );

				string comment = string.Format( "{0} removed registered ConditionManifest: {1}. ", user.FullName(), recordId );

				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "ConditionManifest", Activity = "Credential Registry", Event = "Removed ConditionManifest", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = recordId, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

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
				return true;
			}

			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.CTID, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				new CostManifestManager().UnPublish( recordId, user.Id, ref statusMessage );

				string comment = string.Format( "{0} removed registered CostManifest: {1}. ", user.FullName(), recordId );

				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "CostManifest", Activity = "Credential Registry", Event = "Removed CostManifest", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = recordId, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

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

        public bool UnPublishCompetencyFramework( CM.CASS_CompetencyFramework entity,
                            Models.AppUser user,
                            ref List<string> messages,
                            ref List<SiteActivity> list )
        {
            bool successful = true;

            if (!CASS_CompetencyFrameworkServices.ValidateFrameworkAction( entity, user, ref messages ))
            {
                messages.Add("Error - not authorized to remove this Framework from the registry");
                return false;
            }

            if (string.IsNullOrWhiteSpace( entity.CredentialRegistryId ))
            {
                messages.Add( "Error - This CASS_CompetencyFramework cannot be removed from the registry as an envelope identifier was not found.");
                return true;
            }
            string statusMessage = "";
            successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.CTID, user.FullName(), ref statusMessage );
            if (successful)
            {
                //reset envelope id and status
                new CASS_CompetencyFrameworkManager().UnPublish( entity.Id, user.Id, ref statusMessage );

                string comment = string.Format( "{0} removed registered Competency Framework: {1}. ", user.FullName(), entity.Id );

                new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Competency Framework", Activity = "Credential Registry", Event = "Removed Competency Framework", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = entity.Id, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

                list = ActivityManager.GetPublishHistory( "CASS_CompetencyFramework", entity.Id );
            }
            else
            {
                //ensure a message is returned
                if (string.IsNullOrWhiteSpace( statusMessage ))
                    messages.Add( "The document was not removed from the Credential Registry.");
                else
                {
                    if (statusMessage == "Couldn't find Envelope")
                    {
                        statusMessage = "";
                        //just remove the CredentialRegistryId regardless
                        if (new CASS_CompetencyFrameworkManager().UnPublish( entity.Id, user.Id, ref statusMessage ))
                        {
                            messages.Add("Couldn't find Envelope in registry. The Competency Framework has been set to unregistered regardless.");
                        }
                        else
                        {
                            messages.Add("Couldn't find Envelope in registry, and an issue was encountered while attempting to unregister the Competency Framework. " + statusMessage);
                        }
                    }
                }
                successful = false;
            }
            return successful;

        }

		public bool UnPublishConceptScheme( CM.ConceptScheme entity,
					Models.AppUser user,
					ref List<string> messages,
					ref List<SiteActivity> list )
		{
			bool successful = true;

			if ( !ConceptSchemeServices.ValidateFrameworkAction( entity, user, ref messages ) )
			{
				messages.Add( "Error - not authorized to remove this ConceptScheme from the registry" );
				return false;
			}

			if ( string.IsNullOrWhiteSpace( entity.CredentialRegistryId ) )
			{
				messages.Add( "Error - This ConceptScheme cannot be removed from the registry as an envelope identifier was not found." );
				return true;
			}
			string statusMessage = "";
			successful = CredentialRegistry_Delete( entity.CredentialRegistryId, entity.CTID, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				new CASS_CompetencyFrameworkManager().UnPublish( entity.Id, user.Id, ref statusMessage );

				string comment = string.Format( "{0} removed registered ConceptScheme: {1}. ", user.FullName(), entity.Id );

				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "ConceptScheme", Activity = "Credential Registry", Event = "Removed ConceptScheme", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = entity.Id, ActivityObjectParentEntityUid = entity.RowId, DataOwnerCTID = entity.OwningOrganization.ctid } );

				//list = ActivityManager.GetPublishHistory( "ConceptScheme", entity.Id );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					messages.Add( "The document was not removed from the Credential Registry." );
				else
				{
					if ( statusMessage == "Couldn't find Envelope" )
					{
						statusMessage = "";
						//just remove the CredentialRegistryId regardless
						if ( new ConceptSchemeManager().UnPublish( entity.Id, user.Id, ref statusMessage ) )
						{
							messages.Add( "Couldn't find Envelope in registry. The Competency Framework has been set to unregistered regardless." );
						}
						else
						{
							messages.Add( "Couldn't find Envelope in registry, and an issue was encountered while attempting to unregister the Competency Framework. " + statusMessage );
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

			LoggingHelper.DoTrace( 6, string.Format( "files: private: {0}, \r\npublic: {1}", privateKeyPath, publicKeyPath ) );
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

		/// <summary>
		/// the related json class is out of date
		/// </summary>
		/// <param name="crEnvelopeId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		[Obsolete]
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

		/// <summary>
		/// the related json class is out of date
		/// </summary>
		/// <param name="crEnvelopeId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		[Obsolete]
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
