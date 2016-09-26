using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Script.Serialization;

using System.Net.Http;
using Models.Search;
using Models.Common;
using Models.Search.ThirdPartyApiModels;

namespace CTIServices
{
	public class ThirdPartyApiServices
	{
		protected string allGeoNamesPlaces = "CONT,ADMD,ADM1,PCL,PCLI,PPL,PPLA1,PPLA2";

		#region GeoNames
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
			var username = Utilities.ConfigHelper.GetApiKey( "GeoNamesUserName", "" );
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