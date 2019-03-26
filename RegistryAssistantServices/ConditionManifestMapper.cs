using System.Collections.Generic;
using System.Linq;
using Factories;
//using ThisMgr = RA.Services.AssessmentServices;
using Models;
using Newtonsoft.Json;
using MC = Models.Common;
using RAResponse = RA.Models.RegistryAssistantResponse;
using ThisEntity = Models.Common.ConditionManifest;
using ThisRequest = RA.Models.Input.ConditionManifestRequest;
using ThisRequestEntity = RA.Models.Input.ConditionManifest;

namespace RegistryAssistantServices
{
    public class ConditionManifestMapper : MappingHelpers
	{

		string className = "ConditionManifestMapper";


		//public static string FormatPayload2( ThisEntity input, ref bool isValid, ref List<string> messages )
		//{
		//	var request = new ThisRequest();
		//	//map to assistant
		//	MapToAssistant( input, request.ConditionManifest );

		//	RA.Models.RequestStatus reqStatus = new RA.Models.RequestStatus();
		//	//format the payload
		//	string payload = ThisMgr.FormatAsJson( request, ref isValid, reqStatus );

		//	if ( reqStatus.HasErrors)
		//	{
		//		isValid = false;
		//		foreach ( var item in reqStatus.Messages )
		//			messages.Add( item.Message );
		//	}
		//	return payload;
		//}

		public static string FormatPayload( ThisEntity input, ref bool isValid, ref List<string> messages )
		{
			var request = new ThisRequest();
            string payload = "";
			//map to assistant
			MapToAssistant( input, request, ref messages );
			//format the payload
			string postBody = JsonConvert.SerializeObject( request, MappingHelpers.GetJsonSettings() );
			//string payload = ThisMgr.FormatAsJson( request, ref isValid, ref messages );

			var response = new RAResponse();
			if ( Services.FormatRequest( postBody, "ConditionManifest", ref response ) )
			{
				//get payload from response
				payload = response.Payload;
			}
			else
			{
				//always get payload from response
				if ( response != null && response.Payload != null )
					payload = response.Payload;
				messages.AddRange( response.Messages );
			}
			return payload;
		}

		/// <summary>
		/// Make a request to the assistant api
		/// </summary>
		/// <param name="input"></param>
		/// <param name="requestType">format or "publish</param>
		/// <param name="submitter"></param>
		/// <param name="isValid"></param>
		/// <param name="messages"></param>
		/// <param name="crEnvelopeId"></param>
		/// <returns></returns>
		public static string AssistantRequest( ThisEntity input, string requestType, string orgApiKey, AppUser submitter, ref bool isValid, ref List<string> messages, ref string crEnvelopeId )
		{
			var request = new ThisRequest();
            requestType = requestType.ToLower();
			if ( "format publish".IndexOf( requestType ) == -1 )
			{
				messages.Add( "Error - invalid request type. Valid values are format or publish." );
				isValid = false;
				return "";
			}
			request.RegistryEnvelopeId = crEnvelopeId;
			//map to assistant
			MapToAssistant( input, request, ref messages );
			//serialize the input
			string jsoninput = JsonConvert.SerializeObject( request, GetJsonSettings() );
			string filePrefix = string.Format( "ConditionManifest_{0}", input.Id );
			Utilities.LoggingHelper.WriteLogFile( 5, filePrefix + "_raInput.json", jsoninput, "", false );
            if ( globalMonitor.Messages.Count > 0 )
            {
                messages.AddRange( globalMonitor.Messages );
                isValid = false;
                return "";
            }
            #region  Authorization settings
            //do we have the org of the current user?
            //need to distinguisg site staff
            MC.Organization myOrg = OrganizationManager.GetForSummary( submitter.PrimaryOrgId );

            //if the current org is child org, will need to get parent org CTID
            request.PublishForOrganizationIdentifier = input.OwningOrganization.ctid;
            //if not staff, this will be the CTID for the publishing org
            request.PublishByOrganizationIdentifier = myOrg.ctid;

            #endregion
            //format the payload
            string postBody = JsonConvert.SerializeObject( request, MappingHelpers.GetJsonSettings() );

			AssistantRequestHelper req = new RegistryAssistantServices.AssistantRequestHelper()
			{
				EndpointType = "conditionmanifest",
				RequestType = requestType,
				Identifier = filePrefix,
				Submitter = submitter.FullName(),
				InputPayload = postBody
			};
			if ( IsValidGuid( orgApiKey ) )
			{
				req.OrganizationApiKey = orgApiKey;
			}
			isValid = Services.PublishRequest( req );
			messages.AddRange( req.Messages );
			ReportRelatedEntitiesToBePublished( ref messages );
			crEnvelopeId = req.EnvelopeIdentifier ?? "";
			if ( !isValid )
			{
				//anything??
			}
			return req.FormattedPayload;
		}
		public static void MapToAssistant( ThisEntity input, ThisRequest request, ref List<string> messages )
		{
			globalMonitor = new AssistantMonitor() { RequestType = "CondtionManifest" };
            ThisRequestEntity output = request.ConditionManifest;
            if ( input.OwningOrganization != null && input.OwningOrganization.InLanguageCodeList.Any() )
            {
                request.DefaultLanguage = input.OwningOrganization.InLanguageCodeList[ 0 ].LanguageCode;
            }
            else
                request.DefaultLanguage = "en";

            output.Name = input.Name;
			output.Ctid = input.CTID;
			output.Description = input.Description;
			output.SubjectWebpage = input.SubjectWebpage;

			output.ConditionManifestOf = MapToOrgRef( input.OwningOrganization );

			output.Requires = MapConditionProfiles( input.Requires );
            output.RenewedConditions = MapConditionProfiles( input.Renewal );
			output.CorequisiteConditions = MapConditionProfiles( input.Corequisite );
			output.RecommendedConditions = MapConditionProfiles( input.Recommends );
			output.EntryConditions = MapConditionProfiles( input.EntryCondition );

		}
	}
}
