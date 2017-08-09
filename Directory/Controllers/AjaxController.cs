using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

using Newtonsoft.Json;

using CTIServices;
using Models;
using Models.Helpers.Cass;

namespace CTI.Directory.Controllers
{
	public class AjaxController : Controller
  {
		//Useful
		public JsonResult GetNewGuid()
		{
			return Utilities.JsonHelper.GetJsonWithWrapper( Guid.NewGuid().ToString() );
		}

		//Convenience
		public JsonResult GetToolTipTermData( string term, string vocabulary = "" )
		{
			if ( string.IsNullOrWhiteSpace( vocabulary ) )
			{
				return GetTermJson( term );
			}
			else
			{
				return GetVocabularyTermJson( vocabulary, term );
			}
		}

		public JsonResult GetTermJson( string term )
		{
			try{
				//may want to be configurable to use pending
				var ctdlUrl = "http://credreg.net/ctdl/terms/";

				var target = term.Contains( ':' ) ? term.Split( ':' )[ 1 ] : term;
				var targetOrig = term;

				target = Char.ToLowerInvariant( target[ 0 ] ) + target.Substring( 1 );

				var rawJson = new HttpClient().GetAsync( ctdlUrl + target + "/json" ).Result.Content.ReadAsStringAsync().Result;
				var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiTermResult>( rawJson );
				var data = deserialized.graph.First();
				//could have a check that returned Id matches the request, as if not found, returns the first item
				if ( data != null && data.id.ToLower().IndexOf( term.ToLower() ) == -1 )
				{
					//try again with original
					bool found = false;
					if ( target != targetOrig )
					{
						rawJson = new HttpClient().GetAsync( "http://credreg.net/ctdl/terms/" + targetOrig + "/json" ).Result.Content.ReadAsStringAsync().Result;
						deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiTermResult>( rawJson );
						data = deserialized.graph.First();
						if ( data != null && data.id.ToLower().IndexOf( term.ToLower() ) > -1 )
						{
							found = true;
						}
					}
					if ( found == false )
					{
						var missing = new ToolTipData()
						{
							Term = term,
							Name = "Not found",
							Definition = "",
							UsageNote = "",
							Comment = ""
						};

						return Utilities.JsonHelper.GetJsonWithWrapper( missing, true, "okay", term );
					}
				
				}

				var result = new ToolTipData()
				{
					Term = data.id,
					Name = data.rdfs_label,
					Definition = ( data.rdfs_comment ?? "" ),
					UsageNote = ( data.skos_scopeNote.FirstOrDefault() ?? "" ),
					Comment = string.IsNullOrWhiteSpace(data.dcterms_description) ? "" : data.dcterms_description
				};

				return Utilities.JsonHelper.GetJsonWithWrapper( result, true, "okay", term );
			}
			catch ( Exception ex )
			{
				return Utilities.JsonHelper.GetJsonWithWrapper( null, false, ex.Message, term );
			}
		}

		public JsonResult GetVocabularyTermJson( string vocabulary, string term )
		{
			try
			{
				//may want to be configurable to use pending
				var ctdlUrl = "http://credreg.net/ctdl/vocabs/";
				
				var targetVocab = vocabulary.Contains( ':' ) ? vocabulary.Split( ':' )[ 1 ] : vocabulary;
				//temp workarounds
				//if ( targetVocab == "AudienceLevelType" )
				//	targetVocab = "AudienceLevel";

				targetVocab = GetVocabularyConceptScheme( vocabulary );
				if ( string.IsNullOrWhiteSpace( targetVocab ) )
				{
					CodeItem item = EnumerationServices.GetPropertyBySchema( vocabulary, term );
					if ( item != null && item.Id > 0 )
					{
						var result2 = new ToolTipData()
						{
							Term = item.SchemaName,
							Name = item.Title,
							Definition = item.Description,
							UsageNote = "",
							Comment = ""
						};

						return Utilities.JsonHelper.GetJsonWithWrapper( result2, true, "okay", term );
					}
					//return GetTermJson( term );
					
				}

				var targetTerm = term.Contains( ':' ) ? term.Split( ':' )[ 1 ] : term;
				var rawJson = new HttpClient().GetAsync( ctdlUrl + targetVocab + "/" + targetTerm + "/json" ).Result.Content.ReadAsStringAsync().Result;
				var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiTermResult>( rawJson );
				var data = deserialized.graph.First();
				//could have a check that returned Id matches the request, as if not found, returns the first item
				if ( data != null && data.id.ToLower().IndexOf( term.ToLower() ) == -1 )
				{
					var missing = new ToolTipData()
					{
						Term = vocabulary,
						Name = "Not found",
						Definition = "",
						UsageNote = "",
						Comment = ""
					};

					return Utilities.JsonHelper.GetJsonWithWrapper( missing, true, "okay", term );
				}
				var result = new ToolTipData()
				{
					Term = data.id,
					Name = data.skos_prefLabel,
					Definition = (data.skos_definition ?? ""),
					UsageNote = ( data.skos_scopeNote.FirstOrDefault() ?? "" ),
					Comment = string.IsNullOrWhiteSpace( data.dcterms_description ) ? "" : data.dcterms_description
				};

				return Utilities.JsonHelper.GetJsonWithWrapper( result, true, "okay", term );
			}
			catch ( Exception ex )
			{
				return Utilities.JsonHelper.GetJsonWithWrapper( null, false, ex.Message, term );
			}
}

		public string GetVocabularyConceptScheme( string vocabulary )
		{
			var concept = "";
			bool isValid = false;
			vocabulary = Char.ToLowerInvariant( vocabulary[ 0 ] ) + vocabulary.Substring( 1 );

			string key = "vocabulary_" + vocabulary;
			//check cache for vocabulary
			if ( HttpRuntime.Cache[ key ] != null )
			{
				concept = ( string ) HttpRuntime.Cache[ key ];
				return concept;
			}
			else
			{
				var targetTerm = GetVocabularyFromTermJson( vocabulary, ref isValid );
				if ( isValid )
				{
					concept = targetTerm.Contains( ':' ) ? targetTerm.Split( ':' )[ 1 ] : targetTerm;
					HttpRuntime.Cache.Insert( key, concept );
					return concept;
				}
				else
				{
					if ( vocabulary == "audienceLevelType" )
					{
						concept = "AudienceLevel";
						HttpRuntime.Cache.Insert( key, concept );
						return concept;
					}
				}
			}

			return "";
		}
		public string GetVocabularyFromTermJson( string term, ref bool isValid )
		{
			try
			{
				isValid = false;
				var ctdlUrl = "http://credreg.net/ctdl/terms/";

				var target = term.Contains( ':' ) ? term.Split( ':' )[ 1 ] : term;
				var rawJson = new HttpClient().GetAsync( ctdlUrl + target + "/json" ).Result.Content.ReadAsStringAsync().Result;
				var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiTermResult>( rawJson );
				var data = deserialized.graph.First();
				if ( data.targetConceptScheme != null && !string.IsNullOrWhiteSpace( data.targetConceptScheme.id ) )
				{
					isValid = true;
					return data.targetConceptScheme.id;
				}
				else
				{
					return "";
				}
			}
			catch ( Exception ex )
			{
				return "";
			}
		}

		public JsonResult GetVocabularyFromTermJson( string term )
		{
			try
			{
				var ctdlUrl = "http://credreg.net/ctdl/terms/";

				var target = term.Contains( ':' ) ? term.Split( ':' )[ 1 ] : term;
				var rawJson = new HttpClient().GetAsync( ctdlUrl + target + "/json" ).Result.Content.ReadAsStringAsync().Result;
				var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiTermResult>( rawJson );
				var data = deserialized.graph.First();
				if( data.targetConceptScheme != null && !string.IsNullOrWhiteSpace( data.targetConceptScheme.id ) )
				{
					return GetTermJson( data.targetConceptScheme.id );
				}
				else
				{
					var missing = new ToolTipData()
					{
						Term = term,
						Name = "No concept scheme found for that term",
						Definition = "",
						UsageNote = "",
						Comment = ""
					};

					return Utilities.JsonHelper.GetJsonWithWrapper( missing, true, "okay", term );
				}
			}
			catch (Exception ex )
			{
				return Utilities.JsonHelper.GetJsonWithWrapper( null, false, ex.Message, term );
			}
		}
		//

		//Do a CASS search - uses vanilla serialization since we don't want the CASS property names when this data gets returned to the client
		//TODO - sanitize user input
		public JsonResult CassSearch( string keyword, string type, string inFramework, int start = 1, int size = 10 )
		{
			keyword = keyword.Replace( "(", "" ).Replace( ")", "" ).Trim();
			if ( !string.IsNullOrWhiteSpace( keyword ) )
			{
				switch ( type )
				{
					case "framework":
						{
							//var typeFilter = "(@type:\"http://schema.eduworks.com/cass/0.1/framework\")"; //Should get this from web.config. use for sandbox
							var typeFilter = "(@type:\"framework\")"; //Should get this from web.config
							var query = typeFilter + " AND (" + keyword + ")";
							var results = ThirdPartyApiServices.DoCassSearch<CassFramework>( query );
							//Don't send tons of extra data back to the client
							foreach( var result in results )
							{
								result.UtilityData.Add( "TotalCompetencies", result.CompetencyUris.Count() );
								result.UtilityData.Add( "TotalRelations", result.RelationUris.Count() );
								result.CompetencyUris = new List<string>();
								result.RelationUris = new List<string>();
							}
							return Utilities.JsonHelper.GetJsonWithWrapper( results, true, "", null );
						}
					case "competency":
						{
							//var typeFilter = "(@type:\"http://schema.eduworks.com/cass/0.1/competency\")"; //Should get this from web.config. use for sandbox
							var typeFilter = "(@type:\"competency\")"; //Should get this from web.config
							var query = typeFilter + " AND (" + keyword + ")";
							var results = ThirdPartyApiServices.DoCassSearch<CassCompetency>( query );
							return Utilities.JsonHelper.GetJsonWithWrapper( results, true, "", null );
						}
					default:
						{
							return Utilities.JsonHelper.GetJsonWithWrapper( null, false, "Unable to determine search type", null );
						}
				}
			}
			return Utilities.JsonHelper.GetJsonWithWrapper( null, false, "No text entered", null );
		}
		//

		//Get a CASS object
		public JsonResult CassGetObject( string uri, string type )
		{
			switch ( type )
			{
				case "framework":
					{
						var data = ThirdPartyApiServices.GetCassObject<CassFramework>( uri );
						ThirdPartyApiServices.AssembleCassFramework( data );
						return Utilities.JsonHelper.GetJsonWithWrapper( data, true, "", null );
					}
				case "competency":
					{
						var data = ThirdPartyApiServices.GetCassObject<CassCompetency>( uri );
						return Utilities.JsonHelper.GetJsonWithWrapper( data, true, "", null );
					}
				default:
					{
						return Utilities.JsonHelper.GetJsonWithWrapper( null, false, "Unable to determine object type", null );
					}
			}
		}

		
		public class ApiTermResult
		{
			public ApiTermResult()
			{
				graph = new List<ApiTerm>();
			}
			[JsonProperty("@graph")]
			public List<ApiTerm> graph { get; set; }
		}
		public class ApiTerm
		{
			public ApiTerm()
			{
				targetConceptScheme = new JsonLDUri();
				skos_scopeNote = new List<string>();
			}
			[JsonProperty( "@id" )]
			public string id { get; set; } //The @id field contains the term with prefix
			[JsonProperty( "@type" )]
			public string type { get; set; }
			[JsonProperty("rdfs:label")]
			public string rdfs_label { get; set; } //Name
			[JsonProperty( "rdfs:comment" )]
			public string rdfs_comment { get; set; } //Definition
			[JsonProperty( "vann:usageNote" )]
			public List<string> skos_scopeNote { get; set; } //Usage Note
			[JsonProperty( "dcterms:description" )]
			public string dcterms_description { get; set; } //Comment
			[JsonProperty( "skos:prefLabel" )]
			public string skos_prefLabel { get; set; } //Name (vocab term)
			[JsonProperty( "skos:definition" )]
			public string skos_definition { get; set; } //Definition (vocab term)
			[JsonProperty( "meta:targetConceptScheme" )]
			public JsonLDUri targetConceptScheme { get; set; } //Used by terms that point to vocabularies
		}
		public class LanguageString
		{
			public LanguageString()
			{
				language = "";
				value = "";
			}
			[JsonProperty("@language")]
			public string language { get; set; }
			[JsonProperty( "@value" )]
			public string value { get; set; }
		}
		public class JsonLDUri
		{
			[JsonProperty( "@id" )]
			public string id { get; set; }
		}
		public class ToolTipData
		{
			public string Term { get; set; }
			public string Name { get; set; }
			public string Definition { get; set; }
			public string UsageNote { get; set; }
			public string Comment { get; set; }
		}

		/// <summary>
		/// Search GeoNames.org for a location and return similar results
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		//[ AcceptVerbs ( HttpVerbs.Get | HttpVerbs.Post ) ]
		//public JsonResult GeoNamesSearch( string text )
		//{
		//	var result = new ThirdPartyApiServices().GeoNamesSearch( text );
		//	return JsonHelper.GetJsonWithWrapper( result );
		//}
		////

		/// <summary>
		/// Take a MicroSearchInput and return the results
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		//[AcceptVerbs( HttpVerbs.Post )]
		//public JsonResult MicroSearch( MicroSearchInput data )
		//{
		//	var valid = true;
		//	var status = "";
		//	var result = new MicroSearchServices().MicroSearch( data, ref valid, ref status );

		//	return JsonHelper.GetJsonWithWrapper( result, valid, status, null );
		
		//}
		////

		//[AcceptVerbs( HttpVerbs.Post )]
		//public JsonResult SelectMicroSearchResult( MicroSearchSelection data )
		//{
		//	var valid = true;
		//	var status = "";
		//	var result = new MicroSearchServices().SelectResult( data, ref valid, ref status );

		//	return JsonHelper.GetJsonWithWrapper( result, valid, status, null );

		//}
		////

		//[AcceptVerbs( HttpVerbs.Post )]
		//public JsonResult DeleteMicroSearchResult( MicroSearchSelection data )
		//{
		//	var valid = true;
		//	var status = "";
		//	new MicroSearchServices().DeleteResult( data, ref valid, ref status );

		//	return JsonHelper.GetJsonWithWrapper( null, valid, status, null );

		//}
		//

	}
}