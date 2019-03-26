using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Models;
using MC = Models.Common;
using ThisEntity = Models.Common.ConceptScheme;
using ThisRequest = RA.Models.Input.ConceptSchemeRequest;
using RA.Models.Input;

namespace RegistryAssistantServices
{
	public class ConceptSchemeMapper : MappingHelpers
	{
		string className = "ConceptSchemeMapper";

		public static string AssistantRequest( ThisEntity input, string requestType, string orgApiKey, AppUser submitter, ref bool isValid, ref List<string> messages, ref string crEnvelopeId )
		{
			var request = new ThisRequest();
			requestType = requestType.ToLower();
			if ( "publish".IndexOf( requestType ) == -1 )
			{
				messages.Add( "Error - invalid request type. Valid values are only publish." );
				isValid = false;
				return "";
			}
			request.RegistryEnvelopeId = "";// crEnvelopeId;
			request.CTID = input.CTID;
			if ( input.Payload.IndexOf( "not found" ) > -1 )
			{
				messages.Add( "ConceptSchemeMapper.AssistantRequest Error - a framework payload was not provided" );
				isValid = false;
				return "";
			}
			var graph = JsonConvert.DeserializeObject<ConceptSchemeGraph>( input.Payload );

			request.ConceptSchemeGraph = JsonConvert.DeserializeObject<ConceptSchemeGraph>( input.Payload );
			request.PublishForOrganizationIdentifier = input.OwningOrganization.CTID;
			//map to assistant
			//MapToAssistant( input, request.Assessment, ref messages );
			//serialize the input - already serialized
			string jsoninput = JsonConvert.SerializeObject( request.ConceptScheme, GetJsonSettings() );
			string filePrefix = string.Format( "ConceptScheme_{0}", input.Id );
			Utilities.LoggingHelper.WriteLogFile( 5, filePrefix + "_raInput.json", jsoninput, "", false );
			//request.ConceptScheme = null;
			//format the payload
			string postBody = JsonConvert.SerializeObject( request, MappingHelpers.GetJsonSettings() );

			//add option (maybe temp) for publish skos
			AssistantRequestHelper req = new RegistryAssistantServices.AssistantRequestHelper()
			{
				EndpointType = "ConceptScheme",
				RequestType = "publishGraph",
				Identifier = filePrefix,
				Submitter = submitter.FullName(),
				InputPayload = postBody
			};
			//API testing-Nocti
			//if (request.PublishForOrganizationIdentifier == "ce-7b127b5f-9c1f-480e-b90a-ab2b559f7fed")
			//    req.AuthorizationToken = "14a113f1-81d6-4e98-8c73-381b4a024fd7";
			if ( requestType == "publish" )
			{
				if ( IsValidGuid( orgApiKey ) )
				{
					req.OrganizationApiKey = orgApiKey;
				}
				isValid = Services.PublishRequest( req );
			}
			//else
			//    isValid = Services.FormatRequest( req );

			messages.AddRange( req.Messages );

			crEnvelopeId = req.EnvelopeIdentifier ?? "";
			if ( !isValid )
			{
				//anything??
			}
			return req.FormattedPayload;
		}
	}
}
