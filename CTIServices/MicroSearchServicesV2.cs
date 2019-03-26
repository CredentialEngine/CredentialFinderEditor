using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Node;
using Models.Node.Interface;
using Models.Search;
using MC = Models.Common;
using MP = Models.ProfileModels;
using Factories;
using System.Reflection;

namespace CTIServices
{
	public class MicroSearchServicesV2
	{
		//Do a micro search and return results
		public static List<MicroProfile> DoMicroSearch( MicroSearchInputV2 query, ref int totalResults, ref bool valid, ref string status )
		{
			//Ensure there is a query
		if ( query.Filters.Count() == 0 )
			{
				valid = false;
				status = "No search parameters found!";
				return null;
			}

			//Sanitize query
			foreach ( var item in query.Filters )
			{
				item.Name = ServiceHelper.CleanText( item.Name ?? "" );
				item.Value = ServiceHelper.CleanText( item.Value as string ?? "" );
			}

			//Maybe useful. Based on notes in Search page.
			var mainSearchTypeCode = 0;
			switch ( ( query.ParentSearchType ?? "" ).ToLower() )
			{
				case "credential": mainSearchTypeCode = 1; break;
				case "organization": mainSearchTypeCode = 2; break;
				case "assessment": mainSearchTypeCode = 3; break;
				case "learningopportunity": mainSearchTypeCode = 7; break; 
				default: break;
			}

			totalResults = 0;
			switch ( query.SearchType )
			{
				case "RegionSearch":
					{
						var locationType = query.GetFilterValueString( "LocationType" ).Split( ',' ).ToList();
						var results = new ThirdPartyApiServices().GeoNamesSearch( query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, locationType, ref totalResults, false );
						return results.ConvertAll( m => ConvertRegionToMicroProfile( m ) );
					}
				case "IndustrySearch":
					{
						//TODO - getAll should be set to false if used by a search view (ie credential)
						bool getAll = query.IncludeAllCodes;
						var results = EnumerationServices.NAICS_Search( mainSearchTypeCode,
							query.GetFilterValueInt( "HeaderId" ), 
							query.GetFilterValueString( "Keywords" ), 
							query.PageNumber, 
							query.PageSize, 
							ref totalResults, 
							getAll );
						return results.ConvertAll( m => ConvertCodeItemToMicroProfile( m ) );
					}
				case "OccupationSearch":
					{
						//TODO - IncludeAllCodes should be set to false if used by a search view (ie credential)
						var results = EnumerationServices.SOC_Search( query.GetFilterValueInt( "HeaderId" ), 
							query.GetFilterValueString( "Keywords" ), 
							query.PageNumber, 
							query.PageSize, ref totalResults, query.IncludeAllCodes );
						return results.ConvertAll( m => ConvertCodeItemToMicroProfile( m ) );
					}
				case "CIPSearch":
					{
						//TODO - need entity type

						var results = EnumerationServices.CIPS_Search( 
							mainSearchTypeCode, 
							query.GetFilterValueInt( "HeaderId" ),
							query.GetFilterValueString( "Keywords" ), 
							query.PageNumber, 
							query.PageSize, 
							ref totalResults, 
							query.IncludeAllCodes );
						return results.ConvertAll( m => ConvertCodeItemToMicroProfile( m ) );
					}
				case "AssessmentSearch":
					{
						var results = AssessmentServices.MicroSearch( query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
				case "QACredentialSearch":
					{
						var results = CredentialServices.QACredentialsSearch( 0, query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
				case "CredentialSearch":
					{
                    //for micro searches, may want to remove restrictions for user access
                    //List<MC.CredentialSummary> results = CredentialServices.MicroSearch( query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults );
                    List<MC.CredentialSummary> results = CredentialServices.MicroSearch( query, query.PageNumber, query.PageSize, ref totalResults );
                    return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
                //case "MyCredentialsSearch":
                //{
                //    //for micro searches, may want to remove restrictions for user access
                //    List<MC.CredentialSummary> results = CredentialServices.MyCredentialsSearch( query.GetFilterValueString( "Name" ), query.PageNumber, query.PageSize, ref totalResults );
                //    return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
                //}
                case "QAOrganizationSearch":
					{
						var results = OrganizationServices.QAOrgsSearch( query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
				case "QAOrganizationLookup":
					{
						var results = OrganizationServices.QAOrgsLookup( query.GetFilterValueString( "Name" ), query.GetFilterValueString( "SubjectWebpage" ), 0, query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
				case "OrganizationSearch":
					{
						var results = OrganizationServices.MicroSearch( query, query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
				case "LearningOpportunitySearch":
				case "LearningOpportunityHasPartSearch":
					{
						var results = LearningOpportunityServices.MicroSearch( query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
				case "AddressSearch":
					{
                    //search for org addresses only
						var results = ProfileServices.AddressProfile_Search( query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults, 2 );
						return results.ConvertAll( m => ConvertAddressToMicroProfile( m ) );
					}

				case "CredentialAssetSearch":
					{
						List<MC.Entity> results = new List<MC.Entity>();
						int credId = 0;
						Int32.TryParse( query.GetFilterValueString( "ParentId" ), out credId);
						if ( credId > 0 )
						{
							results = CredentialServices.CredentialAssetsSearch( credId );
						}

						return results.ConvertAll( m => ConvertEntityToMicroProfile( m ) );
					}


				case "CostProfileSearch":
					{
						var results = ProfileServices.CostProfile_Search( query.GetFilterValueString( "Parent" ), query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertCostProfileToMicroProfile( m ) );
					}
				
				case "ConditionManifestSearch":
					{
						List<MC.ConditionManifest> results = new List<MC.ConditionManifest>();
						string orgUid = query.GetFilterValueString( "OwningAgentUid" );
						string parentType = query.GetFilterValueString( "TypeName" );
						int parentId = 0;
						int orgId = 0;
						Int32.TryParse( query.GetFilterValueString( "ParentId" ), out parentId );
						if ( parentId > 0)
						{
							//this is actually the parent/credential id
							if ( parentType == "Credential" )
							{
								MC.Credential cred = CredentialManager.GetBasic( parentId );
								if ( cred != null && cred.OwningOrganizationId > 0 )
									orgId = cred.OwningOrganizationId;
							}
							else if ( parentType == "Organization")
							{
								orgId = parentId;
							} else if ( parentType == "Assessment" )
							{
								MP.AssessmentProfile asmt = AssessmentManager.GetBasic( parentId );
								if ( asmt != null && asmt.OwningOrganizationId > 0 )
									orgId = asmt.OwningOrganizationId;
							} else if ( parentType == "LearningOpportunity" )
							{
								MP.LearningOpportunityProfile lopp = LearningOpportunityManager.GetBasic( parentId );
								if ( lopp != null && lopp.OwningOrganizationId > 0 )
									orgId = lopp.OwningOrganizationId;
							}

							if ( orgId > 0)
								results = ConditionManifestServices.Search( orgId, query.PageNumber, query.PageSize, ref totalResults );
						}
						return results.ConvertAll( m => ConvertConditionManifestToMicroProfile( m ) );
					}
				case "CostManifestSearch":
					{
						List<MC.CostManifest> results = new List<MC.CostManifest>();
						
						string parentType = query.GetFilterValueString( "TypeName" );
						int parentId = 0;
						int orgId = 0;
						Int32.TryParse( query.GetFilterValueString( "ParentId" ), out parentId );
						if ( parentId > 0 )
						{
							//this is actually the parent/credential id
							if ( parentType == "Credential" )
							{
								MC.Credential cred = CredentialManager.GetBasic( parentId );
								if ( cred != null && cred.OwningOrganizationId > 0 )
									orgId = cred.OwningOrganizationId;
							}
							else if ( parentType == "Organization" )
							{
								orgId = parentId;
							}
							else if ( parentType == "Assessment" )
							{
								MP.AssessmentProfile asmt = AssessmentManager.GetBasic( parentId );
								if ( asmt != null && asmt.OwningOrganizationId > 0 )
									orgId = asmt.OwningOrganizationId;
							}
							else if ( parentType == "LearningOpportunity" )
							{
								MP.LearningOpportunityProfile lopp = LearningOpportunityManager.GetBasic( parentId );
								if ( lopp != null && lopp.OwningOrganizationId > 0 )
									orgId = lopp.OwningOrganizationId;
							}

							if ( orgId > 0 )
								results = CostManifestServices.Search( orgId, query.PageNumber, query.PageSize, ref totalResults );
						}
						return results.ConvertAll( m => ConvertCostManifestToMicroProfile( m ) );
					}
				default:
					totalResults = 0;
					valid = false;
					status = "Unable to find Search Type";
					return null;
			}
		}
		//

		public MicroProfile SaveMicroProfile( ProfileContext context, Dictionary<string, object> selectors, string searchType, string property, bool allowMultipleSavedItems, ref bool valid, ref string status )
		{
			AppUser user = AccountServices.GetUserFromSession();
            List<string> messages = new List<string>();

            switch ( searchType )
			{
				case "RegionSearch":
					{
						var js = new JurisdictionServices();
						var region = new MC.GeoCoordinates()
						{
							ParentEntityId = context.Profile.RowId,
							GeoNamesId = GetIntValue( selectors[ "GeoNamesId" ] ),
							Name = ( string ) selectors[ "Name" ],
							IsException = ( bool ) selectors[ "IsException" ],
							ToponymName = ( string ) selectors[ "ToponymName" ],
							Region = ( string ) selectors[ "Region" ],
							Country = ( string ) selectors[ "Country" ],
							//Latitude = ( double ) ( ( decimal ) selectors[ "Latitude" ] ),
							//Longitude = ( double ) ( ( decimal ) selectors[ "Longitude" ] ),
							Latitude = double.Parse( selectors[ "Latitude" ].ToString() ),
							Longitude = double.Parse( selectors[ "Longitude" ].ToString() ),
							Url = ( string ) selectors[ "Url" ]
						};
						valid = js.GeoCoordinates_Add( region, context.Profile.RowId, AccountServices.GetUserFromSession(), ref status );
						return valid ? ConvertRegionToMicroProfile( js.GeoCoordiates_Get( region.Id ) ) : null;
					}
				case "IndustrySearch":
				case "OccupationSearch":
				case "CIPSearch":
					{
						var categoryID = 0;
						switch ( searchType )
						{
							case "IndustrySearch":
								categoryID = CodesManager.PROPERTY_CATEGORY_NAICS;
								break;
							case "OccupationSearch":
								categoryID = CodesManager.PROPERTY_CATEGORY_SOC;
								break;
							case "CIPSearch":
								categoryID = CodesManager.PROPERTY_CATEGORY_CIP;
								break;
							default:
								break;
						}
						var rawData = new ProfileServices().FrameworkItem_Add( context.Profile.RowId, 
							categoryID, 
							GetIntValue( selectors[ "CodeId" ] ), 
							AccountServices.GetUserFromSession(), 
							ref valid, 
							ref status );
						return ConvertEnumeratedItemToMicroProfile( rawData );
					}
				
				case "AssessmentSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
                        if ( context.Profile.TypeName == "ConditionProfile" || context.Profile.TypeName == "ProcessProfile" )
                        {
                            var rawData = new ProfileServices().Assessment_Add( context.Profile.RowId, context.Main.RowId, target.Id, user, ref valid, ref status, allowMultipleSavedItems );
                            if ( rawData == 0 )
                            {
                                valid = false;
                                return null;
                            }
                            else
                            {
                                //if was added to a credential, then add to a condition profile
                                //TODO - need to handle with process profiles
                                if ( context.Profile.TypeName == "Credential" )
                                {
                                    UpsertConditionProfileForAssessment( context.Profile.RowId, target.Id, user, ref status );

                                }

                                var results = AssessmentServices.Get( target.Id );
                                return ConvertProfileToMicroProfile( results );
                            }
                        } else
                        {
                            var results = AssessmentServices.Get( target.Id );
                            return ConvertProfileToMicroProfile( results );
                        }
					}
				
				case "LearningOpportunitySearch":
					{
                    var target = GetProfileLinkFromSelectors( selectors );
                    if ( context.Profile.TypeName == "ConditionProfile" || context.Profile.TypeName == "ProcessProfile" )
                    {
                        var newId = new ProfileServices().LearningOpportunity_Add( context.Profile.RowId, context.Main.RowId, target.Id, user, ref valid, ref status, allowMultipleSavedItems );

                        if ( newId == 0 )
                        {
                            valid = false;
                            return null;
                        }
                        else
                        {
                            //if was added to a credential, then add to a condition profile
                            if ( context.Profile.TypeName == "Credential" )
                            {
                                UpsertConditionProfileForLearningOpp( context.Profile.RowId, target.Id, user, ref status );
                            }
                            var results = LearningOpportunityServices.GetForMicroProfile( target.Id );
                            return ConvertProfileToMicroProfile( results );
                        }
                    }
                    else
                    {
                        var results = LearningOpportunityServices.Get( target.Id );
                        return ConvertProfileToMicroProfile( results );
                    }

                }
				case "LearningOpportunityHasPartSearch":
					{
						//TODO - can we get rowId instead?
						Guid rowId = context.Parent.RowId;

						var target = GetProfileLinkFromSelectors( selectors );
						var rawData = new LearningOpportunityServices().AddLearningOpportunity_AsPart( context.Profile.RowId, target.Id, AccountServices.GetUserFromSession(), ref valid, ref status );
						if ( rawData == 0 )
						{
							valid = false;
							return null;
						}
						else
						{
							var results = LearningOpportunityServices.Get( target.Id );
							return ConvertProfileToMicroProfile( results );
						}
					}
                case "QACredentialSearch":
                case "CredentialSearch":
                {
                    var target = GetProfileLinkFromSelectors( selectors );
                    switch ( context.Profile.TypeName )
                    {
						case "Agent_QAPerformed_Credential":
						{
							//this doesn't save, and actually shouldn't even call this 
							//??what else
							var entity = CredentialServices.GetBasicCredential( target.Id );
							return ConvertProfileToMicroProfile( entity );
						}
						default:
						{
							//use context.Profile.RowId for adding a credential to a condition profile or process profile
							var newId = new ProfileServices().EntityCredential_Save( context.Profile.RowId, target.Id, AccountServices.GetUserFromSession(), allowMultipleSavedItems, ref valid, ref status );
							if ( newId == 0 )
							{
								valid = false;
								return null;
							}
							else
							{
								//??
								var entity = ProfileServices.EntityCredential_Get( newId );
								return ConvertProfileToMicroProfile( entity.Credential );
							}
						}
					}

                }
                case "QAOrganizationSearch":
				case "OrganizationSearch":
					{
						return SaveMicroProfiles_ForOrgSearch( context, selectors, searchType, property, allowMultipleSavedItems, ref valid, ref status );

						//will need different actions dependent on profile type
						//var target = GetProfileLinkFromSelectors( selectors );
						//switch ( context.Profile.TypeName )
						//{
						//	case "Organization":
						//	{
						//		//need parent, and new child to connect, but need role, ie dept, subsidiary, or ????
						//		//NEW - need code to handle adding an org to an entity, like a credential, or role

						//		int roleId = Entity_AgentRelationshipManager.ROLE_TYPE_DEPARTMENT;
						//		if ( property == "OwningOrganization" )
						//		{
						//			//just return the org
						//			var entity = OrganizationServices.GetForSummary( target.Id );
						//			return ConvertProfileToMicroProfile( entity );
						//		}
						//		else if ( property == "Department" )
						//			roleId = Entity_AgentRelationshipManager.ROLE_TYPE_DEPARTMENT;
						//		else
						//			roleId = Entity_AgentRelationshipManager.ROLE_TYPE_SUBSIDIARY;

						//		var newId = new OrganizationServices().AddChildOrganization( context.Main.RowId, target.RowId, roleId, AccountServices.GetUserFromSession(), ref valid, ref status );

						//		if ( newId == 0 )
						//		{
						//			valid = false;
						//			return null;
						//		}
						//		else
						//		{
						//			//??
						//			var entity = OrganizationServices.GetForSummary( target.Id );
						//			return ConvertProfileToMicroProfile( entity );
						//		}
						//	}
							
						//	case "Credential":
						//		{
						//			//actually, if credential, only current action is for owning org - which is not a child relationship. Just return the org?
						//			//??
						//			var entity = OrganizationServices.GetForSummary( target.Id );
						//			return ConvertProfileToMicroProfile( entity );
						//		}
								
						//	case "ConditionProfile":
						//		{
						//			//conditon profile also has org as part of entity, no child. What to return to prevent error?
						//			var entity = OrganizationServices.GetForSummary( target.Id );
						//			return ConvertProfileToMicroProfile( entity );
						//		}
						//		//break;

						//	case "AgentRoleProfile_Recipient":
						//		{
						//			//??what else
						//			var entity = OrganizationServices.GetForSummary( target.Id );
						//			return ConvertProfileToMicroProfile( entity );
						//		}
						//		//break;
						//	default:
						//		break;
						//}
						//return null;
					}
					//no ajax save 
				//case "CredentialAssetSearch":
				//	{
						
				//	}
				case "CostProfileSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						//use 
						var rawData = new ProfileServices().CostProfile_Copy( target.RowId, context.Profile.RowId, AccountServices.GetUserFromSession(), ref valid, ref status );
						if ( rawData == 0 )
						{
							valid = false;
							return null;
						}
						else
						{
							var results = ProfileServices.CostProfile_Get( target.Id );
							return ConvertProfileToMicroProfile( results );
						}
					}
				case "ConditionManifestSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						//use 
						var rawData = new ConditionManifestServices().Entity_CommonCondition_Add( context.Profile.RowId, target.Id, AccountServices.GetUserFromSession(), ref valid, ref status );
						if ( rawData == 0 )
						{
							valid = false;
							return null;
						}
						else
						{
							var results = ConditionManifestServices.GetBasic( target.Id );
							return ConvertProfileToMicroProfile( results );
						}
					}
				case "CostManifestSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						//use 
						var rawData = new CostManifestServices().Entity_CommonCost_Add( context.Profile.RowId, target.Id, AccountServices.GetUserFromSession(), ref valid, ref status );
						if ( rawData == 0 )
						{
							valid = false;
							return null;
						}
						else
						{
							var results = CostManifestServices.GetBasic( target.Id );
							return ConvertProfileToMicroProfile( results );
						}
					}
                case "AddressSearch":
                {
                    //will contain Id to the location, or entity.address
                    var target = GetProfileLinkFromSelectors( selectors );
                    //use 
                    var rawData = new ProfileServices().LocationReferenceAdd( context.Profile.RowId, target.Id, user.Id, ref messages );
                    if ( rawData == 0 )
                    {
                        valid = false;
                        status = string.Join( ",", messages );
                        return null;
                    }
                    else
                    {
                        var results = ProfileServices.AddressProfile_Get( target.Id );
                        return ConvertProfileToMicroProfile( results );
                    }
                }
                default:
					valid = false;
					status = "Microsearch: Unable to find Search Type";
					return null;
			}
		}
		private bool UpsertConditionProfileForAssessment( Guid credentialUid, int entityId, AppUser user, ref string status )
		{
			bool addUpdateCondition = new ConditionProfileServices().UpsertConditionProfileForAssessment( credentialUid, entityId, user, ref status );

			if ( addUpdateCondition )
			{
				//activity tracking prob in latter call?
			}

			return addUpdateCondition;
		}
		private bool UpsertConditionProfileForLearningOpp( Guid credentialUid, int entityId, AppUser user, ref string status )
		{
			bool addUpdateCondition = new ConditionProfileServices().UpsertConditionProfileForLearningOpp( credentialUid, entityId, user, ref status );

			if (addUpdateCondition)
			{
				//activity tracking prob in latter call?
			}

			return addUpdateCondition;
		}
		/// <summary>
		/// Handle saves from an organization search
		/// </summary>
		/// <param name="context"></param>
		/// <param name="selectors"></param>
		/// <param name="searchType"></param>
		/// <param name="property"></param>
		/// <param name="allowMultipleSavedItems"></param>
		/// <param name="valid"></param>
		/// <param name="status"></param>
		/// <returns></returns>
		public static MicroProfile SaveMicroProfiles_ForOrgSearch( ProfileContext context, Dictionary<string, object> selectors, string searchType, string property, bool allowMultipleSavedItems, ref bool valid, ref string status )
		{
			//will need different actions dependent on profile type
			ProfileLink target = GetProfileLinkFromSelectors( selectors );
			switch ( context.Profile.TypeName )
			{
				case "Organization":
				{
					//need parent, and new child to connect, but need role, ie dept, subsidiary, or ????
					//NEW - need code to handle adding an org to an entity, like a credential, or role

					int roleId = Entity_AgentRelationshipManager.ROLE_TYPE_DEPARTMENT;
					if ( property == "OwningOrganization" )
					{
						//just return the org
						var entity = OrganizationServices.GetForSummary( target.Id );
						return ConvertProfileToMicroProfile( entity );
					}
					
					else if( property == "Department" )
						roleId = Entity_AgentRelationshipManager.ROLE_TYPE_DEPARTMENT;
					else
						roleId = Entity_AgentRelationshipManager.ROLE_TYPE_SUBSIDIARY;

					var newId = new OrganizationServices().AddChildOrganization( context.Main.RowId, target.RowId, roleId, AccountServices.GetUserFromSession(), ref valid, ref status );

					if ( newId == 0 )
					{
						valid = false;
						return null;
					}
					else
					{
						//??
						var entity = OrganizationServices.GetForSummary( target.Id );
						return ConvertProfileToMicroProfile( entity );
					}
				}

				case "Credential":
					{
						if ( property == "OfferedByOrganization" )
						{

							if ( new OrganizationServices().EntityAgent_SaveRole( context.Main.RowId,
								target.RowId,
								Entity_AgentRelationshipManager.ROLE_TYPE_OFFERED_BY,
								AccountServices.GetUserFromSession(), ref status ) )
							{
								var entity = OrganizationServices.GetForSummary( target.Id );
								return ConvertProfileToMicroProfile( entity );
							}
							else
							{
								valid = false;
								return null;
							}
						}
						else
						{
							//actually, if credential, only current action is for owning org - which is not a child relationship. Just return the org?
							//??
							var entity = OrganizationServices.GetForSummary( target.Id );
							return ConvertProfileToMicroProfile( entity );
						}
				}

				case "ConditionProfile":
				{
					//conditon profile also has org as part of entity, no child. What to return to prevent error?
					var entity = OrganizationServices.GetForSummary( target.Id );
					return ConvertProfileToMicroProfile( entity );
				}
				//break;

				case "AgentRoleProfile_Recipient":
                case "Agent_QAPerformed_Organization":
				{
					//??what else
					var entity = OrganizationServices.GetForSummary( target.Id );
					return ConvertProfileToMicroProfile( entity );
				}
				//break;
				default:
					break;
			}
			return null;
		}

		//Get data for automated refresh of micro search results
		public static List<MicroProfile> GetMicroProfiles( string searchType, ProfileContext context, string propertyName, ref bool valid, ref string status )
		{
			//Get all items for property and context combination
			var items = new List<ProfileLink>();
			var profile = EditorServices.GetProfile( context, true, ref valid, ref status );
			foreach( var property in profile.GetType().GetProperties() )
			{
				if(	property.Name == propertyName )
				{
					try
					{
						items = ( List<ProfileLink> ) property.GetValue( profile );
					}
					catch { }
				}
			}

			//Get micro profiles
			return GetMicroProfiles( searchType, items, ref valid, ref status );
		}
		//

		//Get data for initial display of micro search results
		public static List<MicroProfile> GetMicroProfiles( string searchType, List<ProfileLink> items, ref bool valid, ref string status )
		{
			var results = new List<MicroProfile>();
			switch ( searchType )
			{
				case "RegionSearch":
					{
						var data = new JurisdictionServices().GeoCoordinates_GetList( items.Select( m => m.Id ).ToList() );
						foreach ( var item in data )
						{
							results.Add( ConvertRegionToMicroProfile( item ) );
						}
						return results;
					}
				case "IndustrySearch":
				case "OccupationSearch":
				case "CIPSearch":
					{
						var data = ProfileServices.FrameworkItem_GetItems( items.Select( m => m.Id ).ToList() );
						foreach ( var item in data )
						{
							results.Add( ConvertEnumeratedItemToMicroProfile( item ) );
						}
						return results;
					}
				case "QACredentialSearch":
				case "CredentialSearch":
					{
						foreach ( var item in items )
						{

							if ( (item.RowId == null || item.RowId == Guid.Empty) && item.Id > 0 ) //No GUID, but ID is present
							{
								results.Add( ConvertProfileToMicroProfile( CredentialServices.GetBasicCredential( item.Id ) ) );
							}
							else 
							{
								results.Add( ConvertProfileToMicroProfile( CredentialServices.GetBasicCredentialAsLink( item.RowId ) ) );
							}
						}
						return results;
					}
				case "QAOrganizationSearch":
				case "OrganizationSearch":
					{
						foreach(var item in items)
						{
							results.Add( ConvertProfileToMicroProfile( OrganizationServices.GetLightOrgByRowId( item.RowId.ToString() ) ) );
						}
						return results;
					}
				case "AssessmentSearch":
					{
						foreach ( var item in items )
						{
							results.Add( ConvertProfileToMicroProfile( AssessmentServices.GetLightAssessmentByRowId( item.RowId.ToString() ) ) );
						}
						return results;
					}
				case "ConditionManifestSearch":
					{
						foreach ( var item in items )
						{
							results.Add( ConvertProfileToMicroProfile( ConditionManifestServices.GetBasic( item.Id ) ) );
						}
						return results;
					}
				case "CostManifestSearch":
					{
						foreach ( var item in items )
						{
							results.Add( ConvertProfileToMicroProfile( CostManifestServices.GetBasic( item.Id ) ) );
						}
						return results;
					}
				case "LearningOpportunitySearch":
				case "LearningOpportunityHasPartSearch":
					{
						foreach ( var item in items )
						{
							results.Add( ConvertProfileToMicroProfile( LearningOpportunityServices.GetLightLearningOpportunityByRowId( item.RowId.ToString() ) ) );
						}
						return results;
					}
				default:
					valid = false;
					status = "Unable to detect Microsearch type";
					return null;
			}
		}
		//

		public static void DeleteMicroProfile( ProfileContext context, Dictionary<string, object> selectors, string searchType, string property, ref bool valid, ref string status )
		{
			var user = AccountServices.GetUserFromSession();
			switch ( searchType )
			{
				case "RegionSearch":
					{
						var targetID = GetIntValue( selectors[ "RecordId" ] );
						new JurisdictionServices().GeoCoordinates_Delete( targetID, ref valid, ref status );
						break;
					}
				case "IndustrySearch":
				case "OccupationSearch":
				case "CIPSearch":
					{
						var targetID = GetIntValue( selectors[ "RecordId" ] );

						valid = new ProfileServices().FrameworkItem_Delete( context.Profile.RowId, targetID, user, ref status );
						break;
					}
				//case "AssessmentSearchOLD":
				//	{
				//		var target = GetProfileLinkFromSelectors( selectors );
				//		valid = new CredentialServices().ConditionProfile_DeleteAsmt( context.Profile.Id, target.Id, user, ref status );
				//		break;
				//	}
				case "AssessmentSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						valid = new ProfileServices().Assessment_Delete( context.Profile.RowId, target.Id, user, ref status );
						break;
					}
				//case "LearningOpportunitySearchOLD":
				//	{
				//		var target = GetProfileLinkFromSelectors( selectors );
				//		valid = new CredentialServices().ConditionProfile_DeleteLearningOpportunity( context.Profile.Id, target.Id, user, ref status );
				//		break;
				//	}
				case "LearningOpportunitySearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						valid = new ProfileServices().LearningOpportunity_Delete( context.Profile.RowId, target.Id, user, ref status );
						break;
					}
				case "LearningOpportunityHasPartSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						valid = new LearningOpportunityServices().DeleteLearningOpportunityPart( context.Profile.RowId, target.Id, user, ref status );
						break;
					}
				case "QACredentialSearch":
				case "CredentialSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						valid = new ProfileServices().EntityCredential_Delete( context.Profile.RowId, target.Id, user, ref status );
						break;
					}
				case "QAOrganizationSearch":
				case "OrganizationSearch":
					{
						//????????????????????? - why no deletes?
						var target = GetProfileLinkFromSelectors( selectors );

						if ( property == "OfferedByOrganization" )
						{
							valid = new OrganizationServices().EntityAgent_DeleteRole( context.Profile.RowId,
								target.RowId,  
								Entity_AgentRelationshipManager.ROLE_TYPE_OFFERED_BY,
								user,
								ref status );
						}
						else if ( property == "Department"  )
						{
							//need to determine if this method should delete the org as well, or just the relationship
							valid = new OrganizationServices().EntityAgent_DeleteRole( context.Profile.RowId,
								target.RowId,
								Entity_AgentRelationshipManager.ROLE_TYPE_DEPARTMENT,
								user,
								ref status );
						}
						else if ( property == "Subsidiary" )
						{
							valid = new OrganizationServices().EntityAgent_DeleteRole( context.Profile.RowId,
								target.RowId,
								Entity_AgentRelationshipManager.ROLE_TYPE_SUBSIDIARY,
								user,
								ref status );
						}
						else if ( "Actor AssertedBy ConditionProvider CopyrightHolder OfferedByAgentUid OwningOrganization ParticipantAgent ProcessingAgent".IndexOf( property ) > -1 )
						{
							//OK, no relation, just stored as propery
							valid = true;
						}
						else
						{
							//add notification for missing deletes 
							string message = string.Format( "Remove/Delete is not being handled for property: {0}, target.Id: {1}, ,target.RowId: {2}", property, target.Id,target.RowId);
							Utilities.EmailManager.NotifyAdmin( "DeleteMicroProfile for  Organization search is not handle", message );
						}
						break;
					}
				case "ConditionManifestSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						valid = new ConditionManifestServices().Entity_CommonCondition_Delete( context.Profile.RowId, target.Id, user, ref status );
						break;
					}
				case "CostManifestSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						valid = new CostManifestServices().Entity_CommonCost_Delete( context.Profile.RowId, target.Id, user, ref status );
						break;
					}
				default:
					valid = false;
					status = "Unable to find Search Type";
					break;
			}
		}
		//

		public static MicroProfile ConvertEnumeratedItemToMicroProfile( MC.EnumeratedItem item )
		{
			var guid = new Guid();
			Guid.TryParse( item.RowId, out guid );

			return new MicroProfile()
			{
				Id = item.Id,
				RowId = guid,
				Name = item.Name,
				Description = item.Description,
				Properties = new Dictionary<string, object>() 
				{ 
					{ "FrameworkCode", item.Value }, 
					{ "Url", item.URL } 
				},
				Selectors = new Dictionary<string, object>()
				{
					{ "CategoryId", item.CodeId },
					{ "CodeId", item.Value },
					{ "RecordId", item.Id }
				}
			};
		}
		//

		public static MicroProfile ConvertCodeItemToMicroProfile( Models.CodeItem item )
		{
			return new MicroProfile()
			{
				Id = item.Id,
				Name = item.Name,
				Description = item.Description,
				Properties = new Dictionary<string, object>()
				{
					{ "FrameworkCode", item.SchemaName },
					{ "Url", item.URL }
				},
				Selectors = new Dictionary<string, object>() {
					{ "CategoryId", item.Code },
					{ "CodeId", item.Id }
				}
			};
		}
		//

		public static MicroProfile ConvertRegionToMicroProfile( MC.GeoCoordinates item )
		{
			return new MicroProfile()
			{
				Id = item.Id,
				RowId = item.RowId,
				Name = item.TitleFormatted,
				Description = item.LocationFormatted,
				Properties = new Dictionary<string, object>()
				{
					{ "Latitude", item.Latitude },
					{ "Longitude", item.Longitude },
					{ "Url", item.Url },
					{ "GeoNamesId", item.GeoNamesId }
				},
				Selectors = new Dictionary<string, object>()
				{
					{ "RecordId", item.Id },
					{ "GeoNamesId", item.GeoNamesId },
					{ "Name", item.Name },
					{ "ToponymName", item.ToponymName },
					{ "Region", item.Region },
					{ "Country", item.Country },
					{ "Latitude", item.Latitude },
					{ "Longitude", item.Longitude },
					{ "Url", item.Url }
				}
			};
		}
		//

		public static MicroProfile ConvertAddressToMicroProfile( MC.Address item )
		{
			return new MicroProfile()
			{
				Id = item.Id,
				RowId = item.RowId,
				Name = item.Name,
				Properties = new Dictionary<string, object>()
				{
					{ "Address1", item.Address1 },
					{ "Address2", item.Address2 },
					{ "City", item.City },
					{ "Region", item.AddressRegion },
					{ "PostalCode", item.PostalCode },
					{ "Country", item.Country },
					{ "CountryId", item.CountryId }
				}
			};
		}
		//
		public static MicroProfile ConvertCostProfileToMicroProfile( MP.CostProfile item )
		{
			return new MicroProfile()
			{
				Id = item.Id,
				RowId = item.RowId,
				Name = item.ProfileName,
				Heading2 = item.ProfileSummary,
				Description = item.Description,
				Selectors = new Dictionary<string, object>()
				{
					{ "RowId", item.RowId },
					{ "Id", item.Id },
					{ "Name", item.ProfileName },
					{ "TypeName", item.GetType().Name }
				}
			};
		}
		public static MicroProfile ConvertEntityToMicroProfile( MC.Entity item )
		{
			return new MicroProfile()
			{
				Id = item.Id,
				RowId = item.EntityUid,
				Name = item.EntityBaseName,
				Heading2 = item.EntityType,
				Description = "",
				Selectors = new Dictionary<string, object>()
				{
					{ "RowId", item.EntityUid },
					{ "Id", item.Id },
					{ "Name", item.EntityBaseName },
					{ "TypeName", item.GetType().Name }
				}
			};
		}
		//
		public static MicroProfile ConvertConditionManifestToMicroProfile( MC.ConditionManifest item )
		{
			return new MicroProfile()
			{
				Id = item.Id,
				RowId = item.RowId,
				Name = item.ProfileName,
				Heading2 = "", // item.ConditionType,
				Description = item.Description,
				Selectors = new Dictionary<string, object>()
				{
					{ "RowId", item.RowId },
					{ "Id", item.Id },
					{ "Name", item.ProfileName },
					{ "TypeName", item.GetType().Name }
				}
			};
		}
		//
		public static MicroProfile ConvertCostManifestToMicroProfile( MC.CostManifest item )
		{
			return new MicroProfile()
			{
				Id = item.Id,
				RowId = item.RowId,
				Name = item.ProfileName,
				Heading2 = "", 
				Description = item.Description,
				Selectors = new Dictionary<string, object>()
				{
					{ "RowId", item.RowId },
					{ "Id", item.Id },
					{ "Name", item.ProfileName },
					{ "TypeName", item.GetType().Name }
				}
			};
		}
		//
		public static MicroProfile ConvertProfileToMicroProfile( object item )
		{
			
			try
			{
				if ( item == null )
					return new MicroProfile();
				var properties = item.GetType().GetProperties();
				return new MicroProfile()
				{
					Id = TryGetValue<int>( item, properties, "Id" ),
					Name = TryGetValue<string>( item, properties, "Name" ),
					Heading2 = TryGetValue<string>( item, properties, "OrganizationName" ),
					Description = TryGetValue<string>( item, properties, "Description" ),
					RowId = TryGetValue<Guid>( item, properties, "RowId" ),
					CTID = TryGetValue<string>( item, properties, "CTID" ),
					SubjectWebpage = TryGetValue<string>( item, properties, "SubjectWebpage" ),
					Selectors = new Dictionary<string, object>() //Create a faux ProfileLink
					{
						{ "RowId", TryGetValue<Guid>( item, properties, "RowId" ) },
						{ "Id", TryGetValue<int>( item, properties, "Id" ) },
						{ "Name", TryGetValue<string>( item, properties, "Name" ) },
						{ "TypeName", item.GetType().Name }
					}
				};
			}
			catch
			{
				return new MicroProfile()
				{
					Name = "Error retrieving this data",
					Description = "There was an error retrieving this item."
				};
			}
		}
		private static T TryGetValue<T>( object source, PropertyInfo[] properties, string name )
		{
			try
			{
				return ( T ) properties.FirstOrDefault( m => m.Name == name ).GetValue( source );
			}
			catch
			{
				return default( T );
			}
		}
		//

		private static int GetIntValue( object value )
		{
			try
			{
				return ( int ) value;
			}
			catch { }
			try
			{
				return int.Parse( ( string ) value );
			}
			catch { }

			return 0;
		}
		//

		private static ProfileLink GetProfileLinkFromSelectors( Dictionary<string, object> selectors )
		{
			try
			{
				return new ProfileLink()
				{
					Id = GetIntValue( selectors[ "Id" ] ),
					Name = ( string ) selectors[ "Name" ],
					RowId = Guid.Parse( ( string ) selectors[ "RowId" ] ),
					TypeName = ( string ) selectors[ "TypeName" ]
				};
			}
			catch
			{
				return new ProfileLink();
			}
		}
		//
	}
}
