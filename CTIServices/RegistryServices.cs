using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

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
		/// <param name="crEnvelopeId"></param>
		/// <returns></returns>
		public bool MetadataRegistry_PublishCredential( int credentialId,
									Models.AppUser user, 
									ref string statusMessage,
									ref List<SiteActivity> list )
		{
			CM.Credential entity = CredentialServices.GetCredentialDetail( credentialId, user );
			if ( entity.Id == 0 )
			{
				statusMessage = "Error - invalid Credential identifier";
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
			bool usingV2 = true;
			if ( usingV2 )
				payload = new JsonLDServices().GetCredentialV2ForRegistry( entity ).ToString();
			else
				payload = new JsonLDServices().GetSerializedJsonLDCredential( entity );

			//temp until CR changes the prefix
			//payload = payload.Replace( "ctdl:", "cti:" );

			string crEnvelopeId = entity.CredentialRegistryId;

			bool success = MetadataRegistry_Publish( payload.ToString(), user.FullName(),
									ref statusMessage,
									ref crEnvelopeId );
			if ( success )
			{
				//call update regardless
				new CredentialManager().UpdateEnvelopeId( credentialId, crEnvelopeId, user.Id, ref statusMessage );
				//if was an add, update CredentialRegistryId
				if ( crEnvelopeId != null && crEnvelopeId != entity.CredentialRegistryId )
				{
					action = "Register Credential";
					comment = string.Format( "{0} registered credential: {1}. Returned envelopeId: {2}", user.FullName(), credentialId, crEnvelopeId );
				}
				else
				{
					action = "Update Credential";
					comment = string.Format( "{0} updated previously registered credential: {1}. Returned envelopeId: {2}", user.FullName(), credentialId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Credential", Activity = "Metadata Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = credentialId } );

				//new ActivityServices().AddActivity( "Metadata Registry", action, comment, user.Id, 0, credentialId );

				if ( user.FullName().IndexOf( "Incomplete -" ) > -1 )
				{
					LoggingHelper.LogError( string.Format( thisClassName + ".MetadataRegistry_PublishCredential() Error - encountered user with incomplete profile - or more likely issue at login/session creation. User: {0}", user.Email ), true );
				}

				list = ActivityManager.GetPublishHistory( "Credential", credentialId );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not saved in the Metadata Registry.";
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
		public bool MetadataRegistry_PublishOrganization( int orgId, Models.AppUser user, ref string statusMessage, ref List<SiteActivity> list )
		{
			CM.Organization entity = OrganizationServices.GetOrganizationDetail( orgId, user );
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

			string payload = new JsonLDServices().GetOrganizationV2ForRegistry( entity );
			//temp until CR changes the prefix
			//payload = payload.Replace( "ctdl:", "cti:" );

			string crEnvelopeId = entity.CredentialRegistryId;

			//???
			successful = MetadataRegistry_Publish( payload.ToString(), user.FullName(),
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
					action = "Register Organization";
					comment = string.Format( "{0} registered Organization: {1}. Returned envelopeId: {2}", user.FullName(), orgId, crEnvelopeId );
					
				}
				else
				{
					action = "UpdateOrganization";
					comment = string.Format( "{0} updated previously registered Organization: {1}. Returned envelopeId: {2}", user.FullName(), orgId, crEnvelopeId );

				}
				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Organization", Activity = "Metadata Registry", Event = action, Comment = comment, ActionByUserId = user.Id, ActivityObjectId = orgId } );

				list = ActivityManager.GetPublishHistory( "Organization", orgId );
			}
			else
			{

			}
			return successful;
		}

		/// <summary>
		/// publish an assessment to the registry
		/// </summary>
		/// <param name="orgId"></param>
		/// <param name="user"></param>
		/// <param name="statusMessage"></param>
		/// <param name="crEnvelopeId"></param>
		/// <returns></returns>
		public bool MetadataRegistry_PublishAssessment( int orgId, Models.AppUser user,
								ref string statusMessage,
								ref string crEnvelopeId )
		{
			bool successful = true;

			AssessmentProfile entity = AssessmentServices.GetDetail( orgId, user );
			if ( !entity.CanEditRecord )
			{
				statusMessage = "Error - not authorized to publish this Organization to the registry";
				return false;
			}
			
			//TODO
			// Not developed yet.
			throw new NotImplementedException();

			//return successful;
		}

	
		/// <summary>
		/// Publish a document to the metadata registry
		/// </summary>
		/// <param name="payload"></param>
		/// <param name="submitter"></param>
		/// <param name="statusMessage"></param>
		/// <param name="crEnvelopeId"></param>
		/// <returns></returns>
		public bool MetadataRegistry_Publish( string payload,
									string submitter,
									ref string statusMessage,
									ref string crEnvelopeId )
		{

			bool successful = true;
			string publicKeyPath = "";
			string privateKeyPath = "";
			string postBody = "";
			try
			{
				if (GetKeys(ref publicKeyPath, ref privateKeyPath, ref statusMessage) == false) 
				{
					return false;
				}

				//todo - need to add signer and other to the content
				//note for new, DO NOT INCLUDE an EnvelopeIdentifier property 
				//		-this is necessary due to a bug, and hopefully can change back to a single call

				if ( string.IsNullOrWhiteSpace( crEnvelopeId ) )
				{
					Envelope envelope = new Envelope();
					//envelope = RegistryHandler.CreateEnvelope( publicKeyPath, privateKeyPath, payload );
					//OR
					RegistryHandler.CreateEnvelope( publicKeyPath, privateKeyPath, payload, envelope );

					postBody = JsonConvert.SerializeObject( envelope );
				} else 
				{
					UpdateEnvelope envelope = new UpdateEnvelope();
					RegistryHandler.CreateEnvelope( publicKeyPath, privateKeyPath, payload, envelope );

					//now embed 
					envelope.EnvelopeIdentifier = crEnvelopeId;
					postBody = JsonConvert.SerializeObject( envelope );
				}

				//Do publish
				string serviceUri = UtilityManager.GetAppKeyValue( "metadataRegistryPublishUrl" );

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
						var contents = task.Result.Content.ReadAsStringAsync();

						if ( response.IsSuccessStatusCode == false )
						{
							RegistryResponseContent contentsJson = JsonConvert.DeserializeObject<RegistryResponseContent>( contents.Result );
							//logging???
							successful = false;
							LoggingHelper.LogError( "RegistryServices.MetadataRegistry_Publish Failed\n\r" + JsonConvert.SerializeObject( response )
								+ "\n\r" + contents );
							statusMessage = string.Join( ",", contentsJson.Errors.ToArray() );

							//statusMessage =contents.err contentsJson.Errors.ToString();
						}
						else
						{
							UpdateEnvelope ue = JsonConvert.DeserializeObject<UpdateEnvelope>( contents.Result );
							crEnvelopeId = ue.EnvelopeIdentifier;

							LoggingHelper.DoTrace( 6, "response: " + JsonConvert.SerializeObject( contents ) );
						}

					}
				}
				catch ( Exception exc )
				{
					LoggingHelper.LogError( exc, "RegistryServices.MetadataRegistry_Publish - POST" );
					successful = false;
					statusMessage = "Failed to Publish: " + exc.Message;
					return false;
				}
				//Set return values
				//no cr id returned?
				
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, "RegistryServices.MetadataRegistry_Publish" );

				successful = false;
				crEnvelopeId = "";
				statusMessage = "Failed to Publish: " + ex.Message;
			}

			return successful;
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
			CM.CredentialSummary entity = CredentialServices.GetLightCredentialById( credentialId );
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

			successful = MetadataRegistry_Delete( entity.CredentialRegistryId, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				new CredentialManager().UnPublish( credentialId, user.Id, ref statusMessage );

				string comment = string.Format( "{0} removed registered credential: {1}. ", user.FullName(), credentialId );

				new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Credential", Activity = "Metadata Registry", Event = "Removed", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = credentialId } );

				list = ActivityManager.GetPublishHistory( "Credential", credentialId );
			}
			else
			{
				//ensure a message is returned
				if ( string.IsNullOrWhiteSpace( statusMessage ) )
					statusMessage = "The document was not removed from the Metadata Registry.";
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
			CM.Organization entity = OrganizationServices.GetLightOrgById( orgId );
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

			successful = MetadataRegistry_Delete( entity.CredentialRegistryId, user.FullName(), ref statusMessage );
			if ( successful )
			{
				//reset envelope id and status
				if ( new OrganizationManager().UnPublish( orgId, user.Id, ref statusMessage ) )
				{

					string comment = string.Format( "{0} removed registered Organization: {1}. ", user.FullName(), orgId );

					new ActivityServices().AddActivity( new SiteActivity() { ActivityType = "Organization", Activity = "Metadata Registry", Event = "Removed", Comment = comment, ActionByUserId = user.Id, ActivityObjectId = orgId } );

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
					statusMessage = "The document was not removed from the Metadata Registry.";
				successful = false;
			}
			return successful;

		}
		public bool MetadataRegistry_Delete( string crEnvelopeId,
									string requestedBy,
									ref string statusMessage)
		{
			string publicKeyPath = "";
			string privateKeyPath = "";
			if (GetKeys(ref publicKeyPath, ref privateKeyPath, ref statusMessage) == false) 
			{
				return false;
			}
			//crEnvelopeId, 
			DeleteEnvelope envelope = RegistryHandler.CreateDeleteEnvelope( publicKeyPath, privateKeyPath, requestedBy );

			string serviceUri = string.Format( UtilityManager.GetAppKeyValue( "metadataRegistryGet" ), crEnvelopeId );


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
						LoggingHelper.LogError( "RegistryServices.PostRequest Failed\n\r" + response + "\n\rError: " + contents.ToString() );

						RegistryResponseContent contentsJson = JsonConvert.DeserializeObject<RegistryResponseContent>( contents.Result );
						response = string.Join( ",", contentsJson.Errors.ToArray() );
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
				statusMessage = "Error - missing application key of publicKeyLocation";
				return false;
			}

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

		public static ReadEnvelope MetadataRegistry_Get( string crEnvelopeId,
									ref string statusMessage )
		{
			string document = "";
			string serviceUri = string.Format( UtilityManager.GetAppKeyValue( "metadataRegistryGet" ), crEnvelopeId );
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
				LoggingHelper.LogError( exc, "RegistryServices.MetadataRegistry_Get" );
			}
			return envelope;

		}


		public static List<ReadEnvelope> MetadataRegistry_GetLatest( string type, string startingDate, string endingDate, int pageNbr, int pageSize, ref string statusMessage )
		{
			string document = "";
			string filter = "";
			string serviceUri = UtilityManager.GetAppKeyValue( "metadataRegistrySearch" );
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
					credential = JsonConvert.DeserializeObject<Models.Json.Credential>( envelope.DecodedResource.ToString() );

					//TODO add to a list
				}
				

			}
			catch ( Exception exc )
			{
				LoggingHelper.LogError( exc, "RegistryServices.MetadataRegistry_GetLatest" );
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
