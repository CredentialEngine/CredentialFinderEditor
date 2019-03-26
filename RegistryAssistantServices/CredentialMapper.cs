using System.Linq;
using System.Collections.Generic;
using Factories;
using Models;
using Newtonsoft.Json;
using RA.Models.Input;
using MC = Models.Common;
using RAResponse = RA.Models.RegistryAssistantResponse;
using RMI = RA.Models.Input;
using ThisEntity = Models.Common.Credential;
using ThisRequest = RA.Models.Input.CredentialRequest;
using ThisRequestEntity = RA.Models.Input.Credential;

namespace RegistryAssistantServices
{
    public class CredentialMapper : MappingHelpers
	{
		string className = "CredentialMapper";
		string registryEnvelopeId = "";
		
		public static string FormatPayload( ThisEntity input, ref bool isValid, ref List<string> messages )
		{
			var request = new ThisRequest();
			AssistantMonitor monitor = new AssistantMonitor();
			string payload = "";
			//map to assistant
			//, ref monitor
			MapToAssistant( input, request, ref messages );
			//format the payload
			string postBody = JsonConvert.SerializeObject( request, MappingHelpers.GetJsonSettings() );
			//string payload = ThisMgr.FormatAsJson( request, ref isValid, ref messages );
			string jsoninput = JsonConvert.SerializeObject( request.Credential, GetJsonSettings() );
			string filePrefix = string.Format( "Credential_{0}", input.Id );
			Utilities.LoggingHelper.WriteLogFile( 2, filePrefix + "_raInput.json", jsoninput, "", false );

			var response = new RAResponse();
			if ( Services.FormatRequest( postBody, "credential", ref response ) )
			{
				//get payload from response
				payload = response.Payload;
			}
			else
			{
				isValid = false;
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
			AssistantMonitor monitor = new AssistantMonitor();
			MapToAssistant( input, request, ref messages );
			//serialize the input
			string jsoninput = JsonConvert.SerializeObject( request, GetJsonSettings() );
			string filePrefix = string.Format( "Credential_{0}", input.Id );
			Utilities.LoggingHelper.WriteLogFile( 5, filePrefix + "_raInput.json", jsoninput, "", false );
            if ( globalMonitor.Messages.Count > 0 )
            {
                messages.AddRange( globalMonitor.Messages );
                isValid = false;
                return "";
            }
            #region  Authorization settings
            //add auth data

            //do we have the org of the current user?
            //need to distinguisg site staff
            MC.Organization myOrg = OrganizationManager.GetForSummary( submitter.PrimaryOrgId );


            //if the current org is child org, will need to get parent org CTID
            //request.PublishForOrganizationIdentifier = input.ctid;
            request.PublishForOrganizationIdentifier = input.OwningOrganization.CTID;
			
            //if not staff, this will be the CTID for the publishing org
            request.PublishByOrganizationIdentifier = myOrg.ctid;

			#endregion
			//format the payload
			string postBody = JsonConvert.SerializeObject( request, MappingHelpers.GetJsonSettings() );

			AssistantRequestHelper req = new RegistryAssistantServices.AssistantRequestHelper()
			{
				EndpointType = "credential",
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
			//ReportRelatedEntitiesToBePublished( ref messages );

			crEnvelopeId = req.EnvelopeIdentifier ?? "";
			if ( !isValid )
			{
				//anything??
			}
			return req.FormattedPayload;
		}
		public static void MapToAssistant( ThisEntity input, ThisRequest request, ref List<string> messages )
		{
			//, ref AssistantMonitor monitor
			globalMonitor = new AssistantMonitor() { RequestType = "Credential" };
            ThisRequestEntity output = request.Credential;
            //language for maps
            if ( input.InLanguageCodeList.Any() )
            {
                request.DefaultLanguage = input.InLanguageCodeList[ 0 ].LanguageCode;
            }
            else
                request.DefaultLanguage = "en";
            if ( Utilities.UtilityManager.GetAppKeyValue( "envType" ) == "development")
            {
                var lml = new LanguageMapList();
                lml.Add( "fr", new List<string>() { "Maison", "Autobus", "Bibliotech" } );

                output.AlternateName_Map = lml;
                output.ProcessStandardsDescription_Map.Add( "de", "Hallo wie geht es dir heute" );
            }
            output.Name = input.Name;
			output.Description = input.Description;
            if (!string.IsNullOrWhiteSpace( input.AlternateName ) )
			    output.AlternateName.Add(input.AlternateName);
            //output.AlternateName = MapToStringList( input.AlternateNames );

            output.CodedNotation = input.CodedNotation;
            output.CredentialId = input.CredentialId;
			output.CredentialType = input.CredentialType.Items[ 0 ].SchemaName;
			output.Ctid = input.ctid;
			output.DateEffective = input.DateEffective;

			output.Image = input.ImageUrl;
			//TODO - change source to a list
			//if ( !string.IsNullOrWhiteSpace( input.InLanguageCode ) )
			//	output.InLanguage.Add( input.InLanguageCode );
            if (input.Auto_InLanguageCode != null && input.Auto_InLanguageCode.Count > 0)
            {
                foreach(var item in input.Auto_InLanguageCode )
                    output.InLanguage.Add( item.LanguageCode );
            }
			//new method output map these
			output.Subject = MapToStringList( input.Subject );
			output.Keyword = MapToStringList( input.Keyword );

			output.DegreeConcentration = MapToStringList( input.DegreeConcentration );
			output.DegreeMajor = MapToStringList( input.DegreeMajor );
			output.DegreeMinor = MapToStringList( input.DegreeMinor );

			output.SubjectWebpage = input.SubjectWebpage;

			output.AvailableOnlineAt = input.AvailableOnlineAt;
			output.AvailabilityListing = input.AvailabilityListing;
			output.LatestVersion = input.LatestVersion;
			output.PreviousVersion = input.PreviousVersion;
			output.VersionIdentifier = AssignIdentifierValueToList( input.VersionIdentifier);

			output.AudienceLevelType = MapEnumermationToStringList( input.AudienceLevelType );
            output.AudienceType = MapEnumermationToStringList( input.AudienceType );
			output.AssessmentDeliveryType = MapEnumermationToStringList( input.AssessmentDeliveryType );
			output.LearningDeliveryType = MapEnumermationToStringList( input.LearningDeliveryType );

			output.ProcessStandards = input.ProcessStandards;
			output.ProcessStandardsDescription = input.ProcessStandardsDescription;

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

			//=======================
			output.OwnedBy = MapToOrgReferences( input.OwningOrganization );
			if ( IsValidGuid( input.CopyrightHolder ) )
				output.CopyrightHolder = MapToOrgRef( input.CopyrightHolderOrganization );
			

			if ( input.OrganizationRole != null && input.OrganizationRole.Count > 0 )
			{
				output.AccreditedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_AccreditedBy );
				output.ApprovedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_ApprovedBy );
				output.RecognizedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_RecognizedBy );
				output.OfferedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_OFFERED_BY );

				output.RenewedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_RenewedBy );
				output.RevokedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_RevokedBy );
				output.RegulatedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_RegulatedBy );
			}
			

			output.Jurisdiction = MapJurisdictions( input.Jurisdiction, ref messages );
			output.JurisdictionAssertions = MapJurisdictionAssertions( input.JurisdictionAssertions, ref messages );

			output.EstimatedCost = MapToEstimatedCosts( input.EstimatedCosts );
			output.EstimatedDuration = MapToEstimatedDuration( input.EstimatedDuration );

			output.RenewalFrequency = MapDurationItem( input.RenewalFrequency_Publish );

			foreach ( var c in input.EmbeddedCredentials )
			{
				output.HasPart.Add( MapToEntityRef( c ) );
			}

			foreach ( var c in input.IsPartOf )
			{
				output.IsPartOf.Add( MapToEntityRef( c ) );
			}

			output.Requires = MapConditionProfiles( input.Requires );
			output.Corequisite = MapConditionProfiles( input.Corequisite );
			output.Recommends = MapConditionProfiles( input.Recommends );
			output.Renewal = MapConditionProfiles( input.Renewal );

			output.AdministrationProcess = MapProcessProfiles( input.AdministrationProcess, ref messages );
			output.MaintenanceProcess = MapProcessProfiles( input.MaintenanceProcess, ref messages );
			output.DevelopmentProcess = MapProcessProfiles( input.DevelopmentProcess, ref messages );
			output.AppealProcess = MapProcessProfiles( input.AppealProcess, ref messages );
			output.RevocationProcess = MapProcessProfiles( input.RevocationProcess, ref messages );
			output.ReviewProcess = MapProcessProfiles( input.ReviewProcess, ref messages );
			output.ComplaintProcess = MapProcessProfiles( input.ComplaintProcess, ref messages );

			if ( input.Addresses != null && input.Addresses.Count > 0 )
			{
				output.AvailableAt = FormatAvailableAt( input.Addresses );
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

			string url = "";
			//only complete - or should be valid if in a reference
			foreach ( var cc in input.CommonConditions )
			{
				if ( FormatRegistryId( cc.CTID, ref url ) )
					output.CommonConditions.Add( url );
			}

			foreach ( var co in input.CommonCosts )
			{
				if ( FormatRegistryId( co.CTID, ref url ) )
					output.CommonCosts.Add( url );
			}

			output.CredentialStatusType = MapSingleEnumermationToString( input.CredentialStatusType );

			output.AdvancedStandingFrom = FormatCredentialConnections( input.AdvancedStandingFrom );
			output.PreparationFrom = FormatCredentialConnections( input.PreparationFrom );
			output.IsAdvancedStandingFor = FormatCredentialConnections( input.IsAdvancedStandingFor );
			output.IsRequiredFor = FormatCredentialConnections( input.IsRequiredFor );
			output.IsRecommendedFor = FormatCredentialConnections( input.IsRecommendedFor );
			output.IsPreparationFor = FormatCredentialConnections( input.IsPreparationFor );

			

			foreach ( var r in input.Revocation )
			{
				var rp = new RevocationProfile();
				rp.Description = r.Description;
				rp.DateEffective = r.DateEffective;
				rp.RevocationCriteria = r.RevocationCriteriaUrl;
				//rp.RevocationCriteria.Add( r.RevocationCriteriaUrl );
				rp.RevocationCriteriaDescription = r.RevocationCriteriaDescription;
				rp.Jurisdiction = MapJurisdictions( r.Jurisdiction, ref messages );
				output.Revocation.Add( rp );
			}

			if ( messages.Count > 0 )
				globalMonitor.Messages.AddRange( messages );
		}
	}
}
