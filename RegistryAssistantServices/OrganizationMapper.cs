using System.Collections.Generic;
using System.Linq;

using Factories;
//using ThisMgr = RA.Services.AssessmentServices;
using Models;
using Newtonsoft.Json;
using RA.Models.Input;
using Utilities;
using RAResponse = RA.Models.RegistryAssistantResponse;
using RMI = RA.Models.Input;
using ThisEntity = Models.Common.Organization;
using ThisRequest = RA.Models.Input.OrganizationRequest;
using ThisRequestEntity = RA.Models.Input.Organization;

namespace RegistryAssistantServices
{
    public class OrganizationMapper : MappingHelpers
    {

        string className = "OrganizationMapper";

		public static string FormatPayload( ThisEntity input, ref bool isValid, ref List<string> messages )
		{
			var request = new ThisRequest();
            if ( input.InLanguageCodeList.Any() )
            {
                request.DefaultLanguage = input.InLanguageCodeList[ 0 ].LanguageCode;
            }
            else
                request.DefaultLanguage = "en";
            string payload = "";
			//map to assistant
			//AssistantMonitor monitor = new AssistantMonitor();
			MapToAssistant( input, request.Organization, ref messages );

			//format the payload
			string postBody = JsonConvert.SerializeObject( request, MappingHelpers.GetJsonSettings() );
			//string payload = ThisMgr.FormatAsJson( request, ref isValid, ref messages );
			string jsoninput = JsonConvert.SerializeObject( request.Organization, GetJsonSettings() );
			string filePrefix = string.Format( "Organization_{0}", input.Id );
			Utilities.LoggingHelper.WriteLogFile( 2, filePrefix + "_raInput.json", jsoninput, "", false );

			var response = new RAResponse();
			if ( Services.FormatRequest( postBody, "Organization", ref response ) )
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
            if (input.InLanguageCodeList.Any())
            {
                request.DefaultLanguage = input.InLanguageCodeList[ 0 ].LanguageCode;
            } else 
                request.DefaultLanguage = "en";

			requestType = requestType.ToLower();
			if ( "format publish".IndexOf( requestType ) == -1 )
			{
				messages.Add( "Error - invalid request type. Valid values are format or publish." );
				isValid = false;
				return "";
			}
			request.RegistryEnvelopeId = crEnvelopeId;
			//map to assistant
			//AssistantMonitor monitor = new AssistantMonitor();
			MapToAssistant( input, request.Organization, ref messages );
			//serialize the input (for logging)
			string jsoninput = JsonConvert.SerializeObject( request, GetJsonSettings() );
			string filePrefix = string.Format( "Organization_{0}", input.Id );
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
			ThisEntity myOrg = OrganizationManager.GetForSummary( submitter.PrimaryOrgId );

			//if the current org is child org, will need to get parent org CTID
			request.PublishForOrganizationIdentifier = input.ctid;
            //if not staff, this will be the CTID for the publishing org
            request.PublishByOrganizationIdentifier = input.ctid;

            #endregion 

            //format the payload
            string postBody = JsonConvert.SerializeObject( request, MappingHelpers.GetJsonSettings() );

			AssistantRequestHelper req = new RegistryAssistantServices.AssistantRequestHelper()
			{
				EndpointType = "organization",
				RequestType = requestType,
				Identifier = filePrefix,
				InputPayload = postBody
            };
			//AuthorizationToken = credentialEngineApiKey

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
			//globalMonitor.Payload = req.FormattedPayload;
			//return globalMonitor;
			return req.FormattedPayload;
		}

		public static void MapToAssistant( ThisEntity input, ThisRequestEntity output, ref List<string> messages )
		{
			globalMonitor = new AssistantMonitor() { RequestType = "Organization" };
			//List<string> messages = new List<string>();

			//an organization will need to designate whether the organization is a QA org or not
			//definition:
			//	A quality assurance organization that plays one or more key roles in the lifecycle of a resource.
			if ( input.ISQAOrganization )
				output.Type = "QACredentialOrganization";
			else
				output.Type = "CredentialOrganization";

			output.Name = input.Name;
			output.Description = input.Description;
			output.SubjectWebpage = input.SubjectWebpage;
			//A ctid is required, and would have to be maintained by the organization for use in making updates to previously published organizations
			output.Ctid = input.CTID;
			//custom method for mapping a list of possible multiple types, to a list of strings (with valid concept terms)
			output.AgentType = MapEnumermationToStringList( input.OrganizationType );

			output.AgentSectorType = MapSingleEnumermationToString( input.AgentSectorType );

            //phone numbers are handled in TargetContactPoint
            bool publishingOrgEmailsAsContactPoints = UtilityManager.GetAppKeyValue( "publishOrgEmailsAsContactPoints", false );
			//email is a list of strings
			//however, doing this results in the loss of the type of email
			//this may be ok if the emails have some context, like info@, support@, etc
			//if do this way, need to remove the option to enter a type from the editor.
			//OR handle in import - not everyone will use contact point
			//or make customizable
			//output.Email = MapToString( input.Emails );
			output.Email = new List<string>();
			RMI.ContactPoint cpi = new RA.Models.Input.ContactPoint();
			if ( input.Emails != null && input.Emails.Count > 0 )
			{
				foreach ( var item in input.Emails )
				{
					//if ( publishingOrgEmailsAsContactPoints && !string.IsNullOrWhiteSpace( item.TextTitle ) )
					//{
					//	cpi = new RA.Models.Input.ContactPoint();
					//	cpi.ContactOption.Add( item.TextTitle );
					//	cpi.Emails.Add( item.TextValue );
					//	output.ContactPoint.Add( cpi );
					//} else
					{
						output.Email.Add( item.TextValue );
					}
				}
			}

            //custom method for mapping addresses
            //contact point is no longer at org level, so phones should be moved, or added to a default address, or create an address with just a contact point!
            output.Address = FormatPlacesList(input.Addresses);
            cpi = new RA.Models.Input.ContactPoint();
            List<RMI.ContactPoint> defaultContacts = new List<RMI.ContactPoint>();

            if ( input.PhoneNumbers != null && input.PhoneNumbers.Count > 0 )
            {
                foreach ( var item in input.PhoneNumbers )
                {
                    cpi = new RA.Models.Input.ContactPoint();
                    cpi.ContactType = item.TextTitle;
                    cpi.PhoneNumbers.Add(item.TextValue);
                    //output.ContactPoint.Add( cpi );
                    defaultContacts.Add(cpi);
                }
                if ( defaultContacts .Count > 0)
                {
                    output.Address.Add(new RA.Models.Input.Place() { ContactPoint = defaultContacts });
                }
            }

            //Social media is referenced at org level, so to be pure, we will not pub as contact point
            output.SocialMedia = MapToStringList( input.SocialMediaPages );
/*
			cpi = new RA.Models.Input.ContactPoint();
			if ( input.SocialMediaPages != null && input.SocialMediaPages.Count > 0 )
			{
				//these can be all in one
				cpi.Name = "Organization Social Media";
				cpi.SocialMediaPages = MapToString( input.SocialMediaPages);
				output.ContactPoint.Add( cpi );
			}
*/
			//custom formatting for contact points
			//the auto method is fine for existing contact points, but cannot include context titles
			//foreach ( var cp in input.Auto_TargetContactPoint )
			//{
			//	output.ContactPoint.Add( new RMI.ContactPoint
			//	{
			//		Name = cp.Name,
			//		ContactType = cp.ContactType,
			//		Emails = MapToStringList( cp.Emails ),
			//		ContactOption = MapToStringList( cp.Auto_ContactOption ),
			//		PhoneNumbers = MapToPhoneString( cp.PhoneNumbers),
			//		SocialMediaPages = MapToStringList( cp.SocialMediaPages )
			//	} );
			//}

			//==============================================



			output.Image = input.ImageUrl;
			//concrete identity properties
			output.Duns = input.ID_DUNS;
			output.Fein = input.ID_FEIN;
			output.IpedsId = input.ID_IPEDSID;
			output.OpeId = input.ID_OPEID;
            output.LEICode = input.ID_LEICode;
            //output.AlternativeIdentifier = input.AlternativeIdentifier;
            output.AlternativeIdentifier = MappingHelpers.AssignTextValueProfileListToList(input.AlternativeIdentifiers);
			//custom method for mapping a list of possible multiple types, to a list of strings (with valid concept terms)
			output.ServiceType = MapEnumermationToStringList( input.ServiceType );

			output.Keyword = MapToStringList( input.Keyword );
			output.AlternateName = MapToStringList( input.AlternateName );
			output.FoundingDate = input.FoundingDate;
            if (!string.IsNullOrWhiteSpace( input.AvailabilityListing ) )
			    output.AvailabilityListing.Add( input.AvailabilityListing );

			output.MissionAndGoalsStatement = input.MissionAndGoalsStatement;
			output.MissionAndGoalsStatementDescription = input.MissionAndGoalsStatementDescription;
			output.AgentPurpose = input.AgentPurpose;
			output.AgentPurposeDescription = input.AgentPurposeDescription;

			output.IndustryType = MapEnumermationToFrameworkItem( input.Industry, "NAICS", "https://www.census.gov/eos/www/naics/" );
			output.Naics = MapNaicsToStringList( input.Industry );
			//handle others
			if ( input.AlternativeIndustries != null && input.AlternativeIndustries.Count > 0 )
			{
				output.IndustryType.AddRange( MapTextValueProfileToFrameworkItem( input.AlternativeIndustries ) );
				//output.AlternativeIndustryType.AddRange( MapTextValueProfileToStringList( input.AlternativeIndustries ) );
			}

			output.Jurisdiction = MapJurisdictions( input.Jurisdiction, ref messages );
			output.SameAs = MapToStringList( input.SameAs );

			output.HasConditionManifest = MapToStringList( input.HasConditionManifest );
			output.HasCostManifest = MapToStringList( input.HasCostManifest );
			

			if ( input.OrganizationRole != null && input.OrganizationRole.Count > 0 )
			{
				output.AccreditedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_AccreditedBy );
				output.ApprovedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_ApprovedBy );
				output.RecognizedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_RecognizedBy );
				output.RegulatedBy = MapToOrgRef( input.OrganizationRole, ROLE_TYPE_RegulatedBy );
			}

			output.JurisdictionAssertions = MapJurisdictionAssertions( input.JurisdictionAssertions, ref messages );

			output.AdministrationProcess = MapProcessProfiles( input.AdministrationProcess, ref messages );
			output.MaintenanceProcess = MapProcessProfiles( input.MaintenanceProcess, ref messages );
			output.DevelopmentProcess = MapProcessProfiles( input.DevelopmentProcess, ref messages );

			output.AppealProcess = MapProcessProfiles( input.AppealProcess, ref messages );
			output.RevocationProcess = MapProcessProfiles( input.RevocationProcess, ref messages );
			output.ReviewProcess = MapProcessProfiles( input.ReviewProcess, ref messages );
			output.ComplaintProcess = MapProcessProfiles( input.ComplaintProcess, ref messages );


			foreach ( var vsp in input.VerificationServiceProfiles )
			{
				var vs = new VerificationServiceProfile
				{
					DateEffective = vsp.DateEffective,
					Description = vsp.Description,
					EstimatedCost = MapToEstimatedCosts( vsp.EstimatedCost ),
					HolderMustAuthorize = vsp.HolderMustAuthorize,
                    SubjectWebpage = vsp.SubjectWebpage,
                    VerificationDirectory = vsp.VerificationDirectory,
					VerificationMethodDescription = vsp.VerificationMethodDescription,
                    VerificationService = vsp.VerificationServiceUrl, //not currently visible
					VerifiedClaimType = MapEnumermationToStringList( vsp.ClaimType )
				};

				vs.OfferedBy = MapToOrgReferences( vsp.OfferedBy );
				
				foreach ( var ta in vsp.TargetCredential )
				{
					vs.TargetCredential.Add( MapToEntityRef( ta ) );
				}

				vs.Jurisdiction = MapJurisdictions( vsp.Jurisdiction, ref messages );
				//cp.Region = MapRegions( item.Region, ref messages );

				output.VerificationServiceProfiles.Add( vs );
			} //VerificationServiceProfile


			//Add mapping to roles
			//output.Owns = MapToEntityRefList( input.Owns_Auto_Organization_OwnsCredentials );

			RMI.EntityReference er = new RMI.EntityReference();
            //TODO - may no longer be doing this. May only publish first party assertions. 
            //      - at least for QA related
            //NOTE: all of these assertions are made by a third party context (ex. from a credential), so these may not be published with the org.
            //TBD - ensure assistant allows no owns - CURRENTLY does NOT have a requirement for owns/offers
            //bool includingThirdPartyAssertions = UtilityManager.GetAppKeyValue( "includeAllThirdPartyAssertionsInOrgPublish", true );
            //if ( includingThirdPartyAssertions )
			    foreach (var item in input.OrganizationThirdPartyAssertions)
			    {
				    er = MapToEntityRef( item );
				    if ( er != null )
				    {
                        if ( item.RelationshipTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_OFFERED_BY )
                            output.Offers.Add( er );
                        else if ( item.RelationshipTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_OWNER )
                            output.Owns.Add( er );
                        else if ( item.RelationshipTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_RENEWED_BY )
                            output.Renews.Add( er );
                        else if ( item.RelationshipTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_REVOKED_BY )
                            output.Revokes.Add( er );
					//else if ( item.RelationshipTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_RECOGNIZED_BY )
					//    output.Recognizes.Add( er );


					//TBD - may need to merge these. Currently taking approach where QA org is not 'aware' of third party QA assertions.
					//NOTE: Accredits3rdParty is populated but not recognized by the API, so is ignored!
					if ( input.ISQAOrganization)
                        {
                            //add to 3rdParty property, for reference, in case change approach
                            if ( item.RelationshipTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_Accredits )
                                output.Accredits3rdParty.Add( er );
                            else if ( item.RelationshipTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_Approves )
                                output.Approves3rdParty.Add( er );
                            else if ( item.RelationshipTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_RECOGNIZES )
                                output.Recognizes3rdParty.Add( er );
                            else if ( item.RelationshipTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_REGULATES )
                                output.Regulates3rdParty.Add( er );

                        }

                    }
			    }
            //if ( input.ISQAOrganization ) //probably can't have this restriction
            //{
                foreach ( var item in input.OrganizationFirstPartyAssertions )
                {
                    er = MapToEntityRef( item );
                    if ( er != null )
                    {
                        if ( item.AssertionTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_Accredits )
                            output.Accredits.Add( er );
                        else if ( item.AssertionTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_Approves )
                            output.Approves.Add( er );
                        else if ( item.AssertionTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_RECOGNIZES )
                            output.Recognizes.Add( er );
                        else if ( item.AssertionTypeId == Entity_AgentRelationshipManager.ROLE_TYPE_REGULATES )
                            output.Regulates.Add( er );

                    }
                }
            //}
                

            //this is fine for existing data, need to add custom assigning of data
            //output.Department = FormatOrganizationReferenceIds( input.Auto_OrganizationRole_Dept );
            //output.SubOrganization = FormatOrganizationReferenceIds( input.Auto_OrganizationRole_SubOrganization );

            //format organization references for a parent organization
            output.ParentOrganization = FormatOrganizationReferenceIds( input.Auto_OrganizationRole_ParentOrganizations );

			output.Department = MapToOrgRef( input.OrganizationRole_Dept, ROLE_TYPE_DEPARTMENT );
			output.SubOrganization = MapToOrgRef( input.OrganizationRole_Subsidiary, ROLE_TYPE_SUBSIDIARY );


			if ( messages.Count > 0 )
				globalMonitor.Messages.AddRange( messages );
		}
	}
}
