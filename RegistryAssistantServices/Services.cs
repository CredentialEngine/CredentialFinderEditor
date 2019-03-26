using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;

using RAResponse = RA.Models.RegistryAssistantResponse;
using Utilities;

namespace RegistryAssistantServices
{
	public class Services
	{
		public static string thisClassName = "RegistryAssistantServices.Services";
		#region Json settings
		public static JsonSerializerSettings GetJsonSettings()
		{
			var settings = new JsonSerializerSettings()
			{
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore,
				ContractResolver = new AlphaNumericContractResolver(),
				Formatting = Formatting.Indented
			};

			return settings;
		}
		//Force properties to be serialized in alphanumeric order
		public class AlphaNumericContractResolver : DefaultContractResolver
		{
			protected override System.Collections.Generic.IList<JsonProperty> CreateProperties( System.Type type, MemberSerialization memberSerialization )
			{
				return base.CreateProperties( type, memberSerialization ).OrderBy( m => m.PropertyName ).ToList();
			}
		}
		#endregion

		public static bool FormatRequest( string postBody, string requestType, ref RAResponse response )
		{
			string status = "";
			AssistantRequestHelper req = new RegistryAssistantServices.AssistantRequestHelper()
			{
				EndpointType = requestType,
				RequestType = "Format",
				Identifier = requestType,
				InputPayload = postBody
			};
			string serviceUri = UtilityManager.GetAppKeyValue( "registryAssistantApi" );
			//NOTE: the V2 will be added later
			req.EndpointUrl = serviceUri + string.Format( "{0}/format", requestType );

			if ( PostRequest( req ) )
			{
				response.Payload = req.FormattedPayload;
				response.RegistryEnvelopeIdentifier = req.EnvelopeIdentifier;
				return true;
			} else
			{
				response.Payload = req.FormattedPayload;
				response.Messages.AddRange( req.Messages );

				//status = req.Status;
				return false;
			}
		}

		/// <summary>
		/// Method for a Format Request 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static bool FormatRequest( AssistantRequestHelper request )
		{
			string serviceUri = UtilityManager.GetAppKeyValue( "registryAssistantApi" );
			request.EndpointUrl = serviceUri + string.Format( "{0}/format", request.EndpointType );


			return PostRequest( request );
		}
		
		public static bool PublishRequest( AssistantRequestHelper request )
		{
            //"https://credentialengine.org/raSandbox/"
            string serviceUri = UtilityManager.GetAppKeyValue( "registryAssistantApi" );

            request.EndpointUrl = serviceUri + string.Format( "{0}/{1}", request.EndpointType, request.RequestType );

			return PostRequest( request );
		}

		public static bool PostRequest( AssistantRequestHelper request )
		{
			RAResponse response = new RAResponse();
            string apiPublisherIdentifier = UtilityManager.GetAppKeyValue( "apiPublisherIdentifier" );
            //string registryAssistantApiVersion = UtilityManager.GetAppKeyValue( "registryAssistantApiVersion" );
            //if ( !string.IsNullOrWhiteSpace( registryAssistantApiVersion ) )
            //    request.EndpointUrl += registryAssistantApiVersion;

            //string cePublisherToken = UtilityManager.GetAppKeyValue( "cePublisherToken" );
            
            try
            {
				using ( var client = new HttpClient() )
				{
					client.DefaultRequestHeaders.
						Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
					//for initial prototyping, check for OrganizationApiKey
					//if ( UtilityManager.GetAppKeyValue( "envType" ) != "production" && 
						if (string.IsNullOrWhiteSpace( request.AuthorizationToken ) )
							request.AuthorizationToken = request.OrganizationApiKey;


					if ( !string.IsNullOrWhiteSpace( request.AuthorizationToken ) )
					{
						client.DefaultRequestHeaders.Add( "Authorization", "ApiToken " + request.AuthorizationToken );
					}

                    //add special header for the publisher
                    if (!string.IsNullOrWhiteSpace( apiPublisherIdentifier ))
                        client.DefaultRequestHeaders.Add( "Proxy-Authorization", apiPublisherIdentifier );

                    var task = client.PostAsync( request.EndpointUrl,
						new StringContent( request.InputPayload, Encoding.UTF8, "application/json" ) );
					task.Wait();
					var result = task.Result;
					var contents = task.Result.Content.ReadAsStringAsync().Result;

					if ( result.IsSuccessStatusCode == false )
					{
						response = JsonConvert.DeserializeObject<RAResponse>( contents );
						//logging???
						string queryString = GetRequestContext();
						string status = string.Join( ",", response.Messages.ToArray() );
						request.FormattedPayload = response.Payload ?? "";
						request.Messages.AddRange( response.Messages );

						LoggingHelper.DoTrace( 4, thisClassName + string.Format( ".PostRequest() {0} {1} failed: {2}", request.EndpointType, request.RequestType, status ) );
						LoggingHelper.LogError( thisClassName + string.Format( ".PostRequest()  {0} {1}. Failed\n\rMessages: {2}" + "\r\nResponse: " + response + "\n\r" + contents + ". payload: " + response.Payload, request.EndpointType, request.RequestType, status ) );

					}
					else
					{
						response = JsonConvert.DeserializeObject<RAResponse>( contents );
						//
						if ( response.Successful)
						{
							LoggingHelper.DoTrace( 7, thisClassName + " PostRequest. envelopeId: " + response.RegistryEnvelopeIdentifier );
							LoggingHelper.WriteLogFile( 5, request.Identifier + "_payload_Successful.json", response.Payload, "", false );

							request.FormattedPayload = response.Payload;
							request.EnvelopeIdentifier = response.RegistryEnvelopeIdentifier;
							//may have some warnings to display
							request.Messages.AddRange( response.Messages );
						} else
						{
							LoggingHelper.DoTrace( 5, thisClassName + " PostRequest FAILED. result: " + response );
							request.Messages.AddRange( response.Messages );
							request.FormattedPayload = response.Payload;
							//LoggingHelper.WriteLogFile( 5, request.Identifier + "_payload_FAILED.json", response.Payload, "", false );
							return false;
						}
						
					}
					return result.IsSuccessStatusCode;
				}
			}
			catch ( Exception exc )
			{
				LoggingHelper.LogError( exc, string.Format( "PostRequest. RequestType:{0}, Identifier: {1}", request.RequestType, request.Identifier ));
				string message = LoggingHelper.FormatExceptions( exc );
				request.Messages.Add( message );
				return false;

			}
			//return valid;
		}
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

		public void SetAuthorizationTokens()
		{

		}
	}

	public class AssistantRequestHelper
	{
		public AssistantRequestHelper ()
		{
			//response = new RAResponse();
			Messages = new List<string>();
			OrganizationApiKey = "";
		}
		//input
		public string RequestType { get; set; }
		public string AuthorizationToken { get; set; }

		//when we are ready to really use the apiKey, change to an alias for AuthorizationToken
		public string OrganizationApiKey { get; set; }

		public string Submitter { get; set; }
		public string InputPayload { get; set; }
		public string EndpointType { get; set; }
		public string EndpointUrl { get; set; }
		
		public string Identifier { get; set; }
		public string EnvelopeIdentifier { get; set; }

		public string FormattedPayload { get; set; }
		
		//public string Status { get; set; }
		public List<string> Messages { get; set; }
		//public RAResponse response { get; set; }
	}
}
