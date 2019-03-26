using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Script.Serialization;

using System.Net.Http;
using Models.Search;
using Models.Common;
using Models.Search.ThirdPartyApiModels;
using Models.Helpers.Cass;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Utilities;

namespace CTIServices
{
	public class ThirdPartyApiServices
	{

		#region GeoNames
		protected string allGeoNamesPlaces = "CONT,ADMD,ADM1,ADM2,PCL*,PPL*";

		/// <summary>
		/// Search GeoNames for a list of places that roughly match the query. Returns the raw response from GeoNames API.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public string GeoNamesSearch( string query, bool includeBoundingBox = false )
		{
			return GeoNamesSearch( query, allGeoNamesPlaces.Split( ',' ).ToList(), 1, 5, includeBoundingBox );
		}
		//

		/// <summary>
		/// Search GeoNames for a list of places that roughly match the query. Returns the raw response from GeoNames API.
		/// References feature codes found at http://www.geonames.org/export/codes.html
		/// </summary>
		/// <param name="query"></param>
		/// <param name="locationType"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public string GeoNamesSearch( string query, List<string> locationType, int pageNumber, int pageSize, bool includeBoundingBox )
		{
			var featureCodes = "";
			if ( locationType == null )
			{
				locationType = allGeoNamesPlaces.Split( ',' ).ToList();
			}
			foreach ( var item in locationType )
			{
				featureCodes = featureCodes + "&featureCode=" + item;
			}
			var username = Utilities.ConfigHelper.GetApiKey( "GeoNamesUserName", "CTITechPlanning" );
			var text = HttpUtility.UrlEncode( query );
			var url = "http://api.geonames.org/searchJSON?q=" + text + "&username=" + username + "&fuzzy=0.7&maxRows=" + pageSize + "&startRow=" + ((pageNumber -1) * pageSize) + "&countryBias=US" + featureCodes + (includeBoundingBox ? "&inclBbox=true" : "");

			return MakeRequest( url );
		}
		//

		/// <summary>
		/// Search GeoNames for a list of places that roughly match the query. Returns a list of GeoCoordinates and sets a reference variable for total results.
		/// </summary>
		/// <param name="query"></param>
		/// <param name="pageNumber"></param>
		/// <param name="pageSize"></param>
		/// <param name="totalResults"></param>
		/// <returns></returns>
		public List<GeoCoordinates> GeoNamesSearch( string query, int pageNumber, int pageSize, List<string> locationType, ref int totalResults, bool includeBoundingBox )
		{
			var output = new List<GeoCoordinates>();
			var rawData = GeoNamesSearch( query, locationType, pageNumber, pageSize, includeBoundingBox );
			var data = new JavaScriptSerializer().Deserialize<GeoNames.SearchResultsRaw>( rawData );

			totalResults = data.totalResultsCount;
            if ( data.geonames == null )
                return output;

			foreach ( var result in data.geonames )
			{
				var newResult = new GeoCoordinates
				{
					GeoNamesId = result.geonameId,
					Name = result.name,
					ToponymName = result.toponymName,
					Region = result.adminName1,
					Country = result.countryName,
					Latitude = double.Parse( result.lat ),
					Longitude = double.Parse( result.lng ),
					Url = "http://geonames.org/" + result.geonameId + "/"
				};
				if ( includeBoundingBox )
				{
					try
					{
						newResult.Bounds = new BoundingBox()
						{
							North = result.bbox.north,
							East = result.bbox.east,
							West = result.bbox.west,
							South = result.bbox.south
						};
					}
					catch { }
				}

				output.Add( newResult );
			}

			return output;
		}
		//
		#endregion

		#region Google Maps

		public string GetGoogleMapsApiKey()
		{
			return Utilities.ConfigHelper.GetApiKey( "GoogleMapsApiKey", "" );
		}
		//

		public string GetGoogleGeocodingServerApiKey()
		{
			return Utilities.ConfigHelper.GetApiKey( "GoogleGeocodingServerApiKey", "" );
		}
		//

		public GoogleGeocoding.Results GeocodeAddress( string address )
		{
			var key = GetGoogleGeocodingServerApiKey();
			var url = "https://maps.googleapis.com/maps/api/geocode/json?key=" + key + "&address=" + HttpUtility.UrlEncode( address );
			var rawData = MakeRequest( url );
			var results = new JavaScriptSerializer().Deserialize<GoogleGeocoding.Results>( rawData );
			return results;
		}
		//

		#endregion

		#region CASS - OBSOLETE

		//private static string CurrentCassDomain() //Should get this from web.config
		//{
		//	string cassRootUrl = UtilityManager.GetAppKeyValue( "cassRootUrl" );
		//	return cassRootUrl;
		//	//return "https://dev.cassproject.org";
		//	//return "https://sandbox.service.cassproject.org";
		//	//return "http://cass.credentialfinder.net";
		//}

		///// <summary>
		///// 
		///// From Views/V2/CASS/CassSearchWidgetIncludeOnceV1
		///// </summary>
		///// <typeparam name="T"></typeparam>
		///// <param name="queryText"></param>
		///// <returns></returns>
		//public static List<T> DoCassSearch<T>( string queryText ) where T : CassObject
		//{
		//	var cassSearchURL = CurrentCassDomain() + "/api/custom/sky/repo/search?q=";
		//	var results = new HttpClient().GetAsync( cassSearchURL + queryText ).Result.Content.ReadAsStringAsync().Result;
		//	return DeserializeCassObjectList<T>( results );
		//}

		//public static T GetCassObject<T>( string url ) where T : CassObject
		//{
		//	var results = new HttpClient().GetAsync( url ).Result.Content.ReadAsStringAsync().Result;
		//	return DeserializeCassObject<T>( results, url );
		//}
		////

		//private static T DeserializeCassObject<T>( string data, string selfURL = "" ) where T : CassObject
		//{
		//	try
		//	{
		//		var item = JsonConvert.DeserializeObject<T>( data );
		//		item.Url = selfURL;
		//		return item;
		//	}
		//	catch
		//	{
		//		return default( T );
		//	}
		//}
		////

		//private static List<T> DeserializeCassObjectList<T>( string data ) where T : CassObject
		//{
		//	try
		//	{
		//		return JsonConvert.DeserializeObject<List<T>>( data );
		//	}
		//	catch
		//	{
		//		return default( List<T> );
		//	}
		//}
		////

		//public static string SerializeCassObject( CassObject data, bool useJsonProperties = false )
		//{
		//	if ( useJsonProperties )
		//	{
		//		return JsonConvert.SerializeObject( data );
		//	}
		//	else
		//	{
		//		return JsonConvert.SerializeObject( data, new JsonSerializerSettings() { ContractResolver = new IgnoreJsonPropertyContractResolver() } );
		//	}
		//}
		////

		//private class IgnoreJsonPropertyContractResolver : DefaultContractResolver
		//{
		//	protected override IList<JsonProperty> CreateProperties( Type type, MemberSerialization memberSerialization )
		//	{
		//		IList<JsonProperty> list = base.CreateProperties( type, memberSerialization );
		//		foreach ( JsonProperty prop in list )
		//		{
		//			prop.PropertyName = prop.UnderlyingName;
		//		}
		//		return list;
		//	}
		//}
		//

		//public static void AssembleCassFramework( CassFramework framework )
		//{
		//	//http://cass.credentialfinder.net/api/custom/ce/framework?apiKey=please_input_your_api_key_here&frameworkUrl=https://dev.cassproject.org/api/custom/data/schema.cassproject.org.0.2.Framework/76ffff33-bd15-4bf3-a007-4ce247d2216d
		//	//Get everything
		//	var apiKey = "please_input_your_api_key_here";
		//	var url = CurrentCassDomain() + "/api/custom/ce/framework?";
		//	var fullURL = url + apiKey + "&frameworkUrl=" + framework.Url;
		//	var frameworkData = GetCassObject<CassFrameworkMultiGetResult>( fullURL );

		//	//Set values
		//	framework.Competencies = frameworkData.Competencies;
		//	framework.Relations = frameworkData.Relations;

		//	//Assemble relationships
		//	foreach( var competency in framework.Competencies )
		//	{
		//		//First, get all the children for the node
		//		competency.ChildrenUris = framework.Relations.Where( m => m.Target == competency._Id && m.RelationType.ToLower() == "narrows" ).Select( m => m.Source ).ToList();

		//		//If the node is not a child of any other node, it is a top level node (there doesn't appear to be a relation that links a framework to its own top level)
		//		if( framework.Relations.Where(m => m.Source == competency._Id && m.RelationType == "narrows" ).Count() == 0 )
		//		{
		//			framework.TopLevelCompetencyUris.Add( competency._Id );
		//		}

		//		//Add a reference back to the framework to aid in later storage and retrieval of data
		//		competency.FrameworkUri = framework._Id;
		//	}
		//}
		//

		#endregion

		/// <summary>
		/// Generic method to make a request to a URL and return the raw response.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public string MakeRequest( string url )
		{
			var getter = new HttpClient();
			var response = getter.GetAsync( url ).Result;
			var responseData = response.Content.ReadAsStringAsync().Result;

			return responseData;
		}
		//

	}
}