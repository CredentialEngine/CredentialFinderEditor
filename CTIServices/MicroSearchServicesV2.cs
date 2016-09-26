using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Node;
using Models.Node.Interface;
using Models.Search;
using MC = Models.Common;
using Factories;
using System.Reflection;

namespace CTIServices
{
	public static class MicroSearchServicesV2
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
						//TODO - getAll should be set to false if used by a search view (ie credential
						bool getAll = query.IncludeAllCodes;
						var results = EnumerationServices.NAICS_Search( query.GetFilterValueInt( "HeaderId" ), query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults, getAll );
						return results.ConvertAll( m => ConvertCodeItemToMicroProfile( m ) );
					}
				case "OccupationSearch":
					{
						var results = EnumerationServices.SOC_Search( query.GetFilterValueInt( "HeaderId" ), query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults, query.IncludeAllCodes );
						return results.ConvertAll( m => ConvertCodeItemToMicroProfile( m ) );
					}
				case "CIPSearch":
					{
						var results = CodesManager.CIPS_Search( query.GetFilterValueInt( "HeaderId" ), query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertCodeItemToMicroProfile( m ) );
					}
				case "AssessmentSearch":
					{
						var results = AssessmentServices.Search( query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
				case "QACredentialSearch":
					{
						var results = CredentialServices.QACredentialsSearch( 0, query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
				case "CredentialSearch":
					{
						var results = CredentialServices.Search( query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults );
						//Convert first to Credential so that the resulting results object has the correct Type in its selectors
						return results.ConvertAll( m => new Credential() { Id = m.Id, Name = m.Name, RowId = m.RowId, Description = m.Description } ).ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
				case "QAOrganizationSearch":
					{
						var results = OrganizationServices.QAOrgsSearch( query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
				case "OrganizationSearch":
					{
						var results = OrganizationServices.Search( query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
				case "LearningOpportunitySearch":
				case "LearningOpportunityHasPartSearch":
					{
						var results = LearningOpportunityServices.Search( query.GetFilterValueString( "Keywords" ), query.PageNumber, query.PageSize, ref totalResults );
						return results.ConvertAll( m => ConvertProfileToMicroProfile( m ) );
					}
				default:
					totalResults = 0;
					valid = false;
					status = "Unable to find Search Type";
					return null;
			}
		}
		//

		public static MicroProfile SaveMicroProfile( ProfileContext context, Dictionary<string, object> selectors, string searchType, string property, ref bool valid, ref string status )
		{
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
						valid = js.GeoCoordinates_Add( region, context.Profile.RowId, AccountServices.GetUserFromSession().Id, ref status );
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
				//case "CIPSearch":
				//	{
				//		var categoryID = CodesManager.PROPERTY_CATEGORY_CIP;

				//		var rawData = new ProfileServices().FrameworkItem_Add( context.Profile.RowId, categoryID, GetIntValue( selectors[ "CodeId" ] ), AccountServices.GetUserFromSession(), ref valid, ref status );

				//	//var rawData2 = new CredentialServices().FrameworkItem_Add( context.Profile.Id, categoryID, GetIntValue( selectors[ "CodeId" ] ), AccountServices.GetUserFromSession(), ref valid, ref status );

				//		return ConvertEnumeratedItemToMicroProfile( rawData );
				//	}
				case "AssessmentSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						var rawData = new CredentialServices().ConditionProfile_AddAsmt( context.Profile.Id, target.Id, AccountServices.GetUserFromSession(), ref valid, ref status );
						if ( rawData == 0 )
						{
							valid = false;
							return null;
						}
						else
						{
							var results = AssessmentServices.Get( target.Id );
							return ConvertProfileToMicroProfile( results );
						}
					}
				case "LearningOpportunitySearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						var rawData = new CredentialServices().ConditionProfile_AddLearningOpportunity( context.Profile.Id, target.Id, AccountServices.GetUserFromSession(), ref valid, ref status );

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
				case "LearningOpportunityHasPartSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						var rawData = new LearningOpportunityServices().AddLearningOpportunity_AsPart( context.Profile.Id, target.Id, AccountServices.GetUserFromSession(), ref valid, ref status );
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
						var newId = new ProfileServices().EntityCredential_Save( context.Profile.RowId, target.Id, AccountServices.GetUserFromSession(), ref valid, ref status );
						if ( newId == 0 )
						{
							valid = false;
							return null;
						}
						else
						{
							//??
							var entity = ProfileServices.EntityCredential_Get( newId );
							//???
							return ConvertProfileToMicroProfile( entity.Credential );
						}
					}
				case "QAOrganizationSearch":
				case "OrganizationSearch":
					{
						//will need different actions dependent on profile type
						var target = GetProfileLinkFromSelectors( selectors );
						switch ( context.Profile.TypeName )
						{
							case "Organization":
								{
									//need parent, and new child to connect, but need role, ie dept, subsidiary, or ????

									int roleId = Entity_AgentRelationshipManager.ROLE_TYPE_DEPARTMENT;
									if (property == "Department")
										roleId = Entity_AgentRelationshipManager.ROLE_TYPE_DEPARTMENT;
									else
										roleId = Entity_AgentRelationshipManager.ROLE_TYPE_SUBSIDUARY;
									var newId = new OrganizationServices().AddChildOrganization( context.Main.RowId, target.RowId, roleId, AccountServices.GetUserFromSession(), ref valid, ref status );

									if ( newId == 0 )
									{
										valid = false;
										return null;
									}
									else
									{
										//??
										var entity = OrganizationServices.GetOrganization( target.Id, true );
										return ConvertProfileToMicroProfile( entity );
									}
								}
								break;
								
							default:
								break;
						}
						return null;
					}
				default:
					valid = false;
					status = "Unable to find Search Type";
					return null;
			}
		}

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
							/*if ( item.RowId == null || item.RowId == Guid.Empty && item.Id > 0 ) //No GUID, but ID is present
							{
								results.Add( ConvertProfileToMicroProfile( CredentialServices.GetBasicCredential( item.Id ) ) );
							}
							else if ( item.Id == 0 && item.RowId != null && item.RowId != Guid.Empty ) //No ID, but GUID is present
							{
								results.Add( ConvertProfileToMicroProfile( CredentialServices.GetLightCredentialByRowId( item.RowId.ToString() ) ) );
							}*/

							if ( item.RowId == null || item.RowId == Guid.Empty && item.Id > 0 ) //No GUID, but ID is present
							{
								results.Add( ConvertProfileToMicroProfile( CredentialServices.GetBasicCredential( item.Id ) ) );
							}
							else 
							{
								results.Add( ConvertProfileToMicroProfile( CredentialServices.GetLightCredentialByRowId( item.RowId.ToString() ) ) );
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
				case "AssessmentSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						valid = new CredentialServices().ConditionProfile_DeleteAsmt( context.Profile.Id, target.Id, user, ref status );
						break;
					}
				case "LearningOpportunitySearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						valid = new CredentialServices().ConditionProfile_DeleteLearningOpportunity( context.Profile.Id, target.Id, user, ref status );
						break;
					}
				case "LearningOpportunityHasPartSearch":
					{
						var target = GetProfileLinkFromSelectors( selectors );
						valid = new LearningOpportunityServices().DeleteLearningOpportunityPart( context.Profile.Id, target.Id, user, ref status );
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
						var target = GetProfileLinkFromSelectors( selectors );

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
					Description = TryGetValue<string>( item, properties, "Description" ),
					RowId = TryGetValue<Guid>( item, properties, "RowId" ),
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
