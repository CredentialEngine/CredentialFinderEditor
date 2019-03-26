using System;
using System.Collections.Generic;
using System.Linq;
using Factories;
//using ThisMgr = RA.Services.AssessmentServices;
using Models;
using Newtonsoft.Json;
using MC = Models.Common;
using RAResponse = RA.Models.RegistryAssistantResponse;
using RMI = RA.Models.Input;
using ThisEntity = Models.ProfileModels.LearningOpportunityProfile;
using ThisRequest = RA.Models.Input.LearningOpportunityRequest;
using ThisRequestEntity = RA.Models.Input.LearningOpportunity;

namespace RegistryAssistantServices
{
    public class LearningOpportunityMapper : MappingHelpers
    {

		string className = "LearningOpportunityMapper";
		public static string FormatPayload( ThisEntity input, ref bool isValid, ref List<string> messages )
		{
			var request = new ThisRequest();
			string payload = "";
			globalMonitor = new AssistantMonitor() { RequestType = "LearningOpportunity" };
			//map to assistant
			MapToAssistant( input, request, ref globalMonitor );
			//format the payload
			string postBody = JsonConvert.SerializeObject( request, MappingHelpers.GetJsonSettings() );
			//string payload = ThisMgr.FormatAsJson( request, ref isValid, ref messages );
			string jsoninput = JsonConvert.SerializeObject( request.LearningOpportunity, GetJsonSettings() );
			string filePrefix = string.Format( "LearningOpportunity_{0}", input.Id );
			Utilities.LoggingHelper.WriteLogFile( 2, filePrefix + "_raInput.json", jsoninput, "", false );

			var response = new RAResponse();
			if ( Services.FormatRequest( postBody, "LearningOpportunity", ref response ) )
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
			globalMonitor = new AssistantMonitor() { RequestType = "LearningOpportunity" };
			requestType = requestType.ToLower();
			if ( "format publish".IndexOf( requestType ) == -1 )
			{
				messages.Add( "Error - invalid request type. Valid values are format or publish." );
				isValid = false;
				return "";
			}
			request.RegistryEnvelopeId = crEnvelopeId;
			//map to assistant
			MapToAssistant( input, request, ref globalMonitor );
			//serialize the input
			string jsoninput = JsonConvert.SerializeObject( request, GetJsonSettings() );
			string filePrefix = string.Format( "LearningOpp_{0}", input.Id );
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
				EndpointType = "learningopportunity",
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


		public static void MapToAssistant( ThisEntity input, ThisRequest request, ref AssistantMonitor globalMonitor )
		{	
			List<string> messages = new List<string>();
            ThisRequestEntity output = request.LearningOpportunity;
            //language for maps
            if ( input.InLanguageCodeList.Any() )
            {
                request.DefaultLanguage = input.InLanguageCodeList[ 0 ].LanguageCode;
            }
            else
                request.DefaultLanguage = "en";

            output.Description = input.Description;
			output.Name = input.Name;
			output.Ctid = input.ctid;
			
			output.SubjectWebpage = input.SubjectWebpage;

			output.Subject = MapToStringList( input.Subject );
			output.Keyword = MapToStringList( input.Keyword );

            if ( input.Auto_InLanguageCode != null && input.Auto_InLanguageCode.Count > 0 )
            {
                foreach ( var item in input.Auto_InLanguageCode )
                    output.InLanguage.Add( item.LanguageCode );
            }

			//frameworks =========================================
			//these need to handle including the Naics framework - and others in the future. 
			output.IndustryType = MapEnumermationToFrameworkItem( input.Industry, "NAICS", "https://www.census.gov/eos/www/naics/" );
			output.Naics = MapNaicsToStringList( input.Industry );
			//handle others
			if ( input.AlternativeIndustries != null && input.AlternativeIndustries.Count > 0 )
			{
				output.IndustryType.AddRange( MapTextValueProfileToFrameworkItem( input.AlternativeIndustries ) );
				//output.AlternativeIndustryType.AddRange( MapTextValueProfileToStringList( input.AlternativeIndustries ) );
			}

			output.OccupationType = MapEnumermationToFrameworkItem( input.Occupation, "Standard Occupational Classification", "https://www.bls.gov/soc/" );
			//handle others
			if ( input.AlternativeOccupations != null && input.AlternativeOccupations.Count > 0 )
			{
				output.OccupationType.AddRange( MapTextValueProfileToFrameworkItem( input.AlternativeOccupations ) );
				//output.AlternativeOccupationType.AddRange( MapTextValueProfileToStringList( input.AlternativeOccupations ) );
			}

			output.InstructionalProgramType = MapEnumermationToFrameworkItem( input.InstructionalProgramType, "Classification of Instructional Programs" );
			//handle others
			if ( input.AlternativeInstructionalProgramType != null && input.AlternativeInstructionalProgramType.Count > 0 )
			{
				output.InstructionalProgramType.AddRange( MapTextValueProfileToFrameworkItem( input.AlternativeInstructionalProgramType ) );
				//output.AlternativeInstructionalProgramType.AddRange( MapTextValueProfileToStringList( input.AlternativeInstructionalProgramType ) );
			}

			//==================
			output.CreditHourType = input.CreditHourType;
			output.CreditUnitType = MapSingleEnumermationToString( input.CreditUnitType );
			output.CreditHourValue = input.CreditHourValue;
			output.CreditUnitValue = input.CreditUnitValue;
			output.CreditUnitTypeDescription = input.CreditUnitTypeDescription;

			output.AvailabilityListing = MapToStringList( input.Auto_AvailabilityListing );
			output.AvailableOnlineAt = MapToStringList( input.Auto_AvailableOnlineAt );
			//output.CodedNotation = MapToStringList( input.Auto_CodedNotation );
            output.CodedNotation = input.CodedNotation;

            if ( !string.IsNullOrEmpty( input.DateEffective ) )
				output.DateEffective = Convert.ToDateTime( input.DateEffective ).ToString( "yyyy-MM-dd" );
			else
				output.DateEffective = null;
			output.LearningMethodType = MapEnumermationToStringList( input.LearningMethodType );

            output.AudienceType = MapEnumermationToStringList(input.AudienceType);
            output.DeliveryType = MapEnumermationToStringList( input.DeliveryType );
			output.DeliveryTypeDescription = input.DeliveryTypeDescription;

			output.Jurisdiction = MapJurisdictions( input.Jurisdiction, ref messages );
			output.JurisdictionAssertions = MapJurisdictionAssertions( input.JurisdictionAssertions, ref messages );
			output.EstimatedCost = MapToEstimatedCosts( input.EstimatedCost );
			output.EstimatedDuration = MapToEstimatedDuration( input.EstimatedDuration );
			output.Requires = MapConditionProfiles( input.Requires );
			output.Corequisite = MapConditionProfiles( input.Corequisite );
			output.Recommends = MapConditionProfiles( input.Recommends );
			output.EntryCondition = MapConditionProfiles( input.EntryCondition );

			output.AdvancedStandingFrom = FormatCredentialConnections( input.AdvancedStandingFrom );
			output.IsAdvancedStandingFor = FormatCredentialConnections( input.IsAdvancedStandingFor );
			output.IsPreparationFor = FormatCredentialConnections( input.IsPreparationFor );
			output.PreparationFrom = FormatCredentialConnections( input.PreparationFrom );

			output.IsRequiredFor = FormatCredentialConnections( input.IsRequiredFor );
			output.IsRecommendedFor = FormatCredentialConnections( input.IsRecommendedFor );

			if ( input.Addresses != null && input.Addresses.Count > 0 )
			{
				output.AvailableAt = FormatAvailableAt( input.Addresses );
			}
           
            output.VerificationMethodDescription = input.VerificationMethodDescription;

            //competencies
            //either TeachesCompetenciesFrameworks or TeachesCompetencies
            if ( input.TargetCompetency != null && input.TargetCompetency.Count > 0 )
			{
				foreach ( var item in input.TargetCompetency )
				{
					output.Teaches.Add( MapCompetencyToCredentialAlignmentObject( item ) );
				}
			}
			else if ( input.TeachesCompetenciesFrameworks != null && input.TeachesCompetenciesFrameworks.Count > 0 )
			{
				output.Teaches = MapCompetenciesToCredentialAlignmentObject( input.TeachesCompetenciesFrameworks );
			}
			else
				foreach ( var item in input.TeachesCompetencies )
				{
					output.Teaches.Add( MapCompetencyToCredentialAlignmentObject( item ) );
				}



			output.OwnedBy = MapToOrgReferences( input.OwningOrganization );

			if ( input.OrganizationRole != null && input.OrganizationRole.Count > 0 )
			{
				output.AccreditedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_AccreditedBy );
				output.ApprovedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_ApprovedBy );
				output.RecognizedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_RecognizedBy );
                output.RegulatedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_RegulatedBy );
                output.OfferedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_OFFERED_BY );
			}


			foreach ( var item in input.FinancialAssistance )
			{
				var fa = new RMI.FinancialAlignmentObject
				{
					AlignmentType = item.AlignmentType,
					AlignmentDate = item.AlignmentDate,
					CodedNotation = item.CodedNotation,
					Framework = item.Framework,
					FrameworkName = item.FrameworkName,
					TargetNode = item.TargetNode,
					TargetNodeDescription = item.TargetNodeDescription,
					TargetNodeName = item.TargetNodeName,
					Weight = item.Weight
				};
				output.FinancialAssistance.Add( fa );
			}

			foreach ( var hp in input.HasPart )
				output.HasPart.Add( MapToEntityRef( hp ) );

			//isPartOf -
			foreach ( var hp in input.IsPartOf )
				output.IsPartOfLearningOpportunity.Add( MapToEntityRef( hp ) );

			string url = "";
			//only complete - or should be valid if in a reference
			foreach ( var cc in input.CommonConditions )
			{
				if ( FormatRegistryId( cc.CTID, ref url ) )
					output.CommonConditions.Add( url );
			}

			foreach ( var co in input.CommonCosts )
				if ( FormatRegistryId( co.CTID, ref url ) )
					output.CommonCosts.Add( url );

			output.VersionIdentifier = AssignIdentifierValueToList( input.VersionIdentifier );


			if ( messages.Count > 0 )
				globalMonitor.Messages.AddRange( messages );
		}
	}
}
