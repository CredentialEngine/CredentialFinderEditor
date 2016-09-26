using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Common;
using Data;
using EM = Data;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;

namespace Factories
{
	public class CodesManager
	{
		#region constants - property categories
		public static int PROPERTY_CATEGORY_JURISDICTION = 1;
		public static int PROPERTY_CATEGORY_CREDENTIAL_TYPE = 2;
		public static int PROPERTY_CATEGORY_CREDENTIAL_PURPOSE = 3;
		public static int PROPERTY_CATEGORY_CREDENTIAL_LEVEL = 4;

		public static int PROPERTY_CATEGORY_CREDENTIAL_ATTAINMENT_COST = 5;
		public static int PROPERTY_CATEGORY_ORG_SERVICE  = 6;
		public static int PROPERTY_CATEGORY_ORGANIZATION_TYPE = 7;
		public static int PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA = 8;
		public static int PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS = 9;

		public static int PROPERTY_CATEGORY_NAICS = 10;
		public static int PROPERTY_CATEGORY_SOC = 11;
		public static int PROPERTY_CATEGORY_MOC	= 12;
		public static int PROPERTY_CATEGORY_CIP = 23;

		public static int PROPERTY_CATEGORY_ENTITY_AGENT_ROLE = 13;
		public static int PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE = 13;
		public static int PROPERTY_CATEGORY_AUDIENCE_TYPE = 14;
		public static int PROPERTY_CATEGORY_CONNECTION_PROFILE_TYPE = 15;
		public static int PROPERTY_CATEGORY_ASSESSMENT_TYPE = 16;
		public static int PROPERTY_CATEGORY_MODALITY_TYPE = 18;
		public static int PROPERTY_CATEGORY_ENROLLMENT_TYPE = 19;
		public static int PROPERTY_CATEGORY_RESIDENCY_TYPE = 20;

		public static int PROPERTY_CATEGORY_LEARNING_OPP_DELIVERY_TYPE = 21;
		public static int PROPERTY_CATEGORY_JURISDICTION_PROFILE_PURPOSE = 22;
		public static int PROPERTY_CATEGORY_CIPCODE = 23;
		public static int PROPERTY_CATEGORY_CURRENCIES = 24;
		public static int PROPERTY_CATEGORY_REFERENCE_URLS = 25;
		public static int PROPERTY_CATEGORY_REVOCATION_CRITERIA_TYPE = 26;
		public static int PROPERTY_CATEGORY_CREDENTIAL_URLS = 27;
		public static int PROPERTY_CATEGORY_CONDITION_ITEM = 28;
		public static int PROPERTY_CATEGORY_COMPETENCY = 29;

		public static int PROPERTY_CATEGORY_ORGANIZATION_SECTORTYPE = 30;
		public static int PROPERTY_CATEGORY_PHONE_TYPE= 31;
		public static int PROPERTY_CATEGORY_EMAIL_TYPE = 32;
		public static int PROPERTY_CATEGORY_AGENT_QAPURPOSE_TYPE = 33;
		public static int PROPERTY_CATEGORY_SUBJECT = 34;
		public static int PROPERTY_CATEGORY_KEYWORD = 35;
		public static int PROPERTY_CATEGORY_ALIGNMENT_TYPE = 36;
		public static int PROPERTY_CATEGORY_ASSESSMENT_MODE = 37;
		public static int PROPERTY_CATEGORY_AUDIENCE_LEVEL = 38;

		#endregion
		#region constants - entity types. 
		//An Entity is typically created only where it can have a child relationship, ex: Entity.Property
		public static int ENTITY_TYPE_CREDENTIAL = 1;
		public static int ENTITY_TYPE_ORGANIZATION = 2;
		public static int ENTITY_TYPE_ASSESSMENT_PROFILE = 3;
		public static int ENTITY_TYPE_CONNECTION_PROFILE = 4;
		public static int ENTITY_TYPE_COST_PROFILE = 5;
		public static int ENTITY_TYPE_COST_PROFILE_ITEM = 6;
		public static int ENTITY_TYPE_LEARNING_OPP_PROFILE = 7;
		public static int ENTITY_TYPE_TASK_PROFILE = 8;
		public static int ENTITY_TYPE_PERSON = 9;

		public static int ENTITY_TYPE_COMPETENCY_FRAMEWORK = 10;

		public static int ENTITY_TYPE_REVOCATION_PROFILE = 12;
		public static int ENTITY_TYPE_VERIFICATION_PROFILE = 13;

		#endregion
		#region constants - entity status
		public static int ENTITY_STATUS_IN_PROGRESS = 1;
		public static int ENTITY_STATUS_PUBLISHED = 2;

		public static int ENTITY_STATUS_DELETED = 6;
		#endregion
		public static Enumeration GetSampleEnumeration( string dataSource, string schemaName )
		{
			var result = Data.Development.TemporaryEnumerationData.GetEnumeration( dataSource, schemaName );

			return result;
		}

		#region Persistance
		/// <summary>
		/// Add a new code
		/// </summary>
		/// <returns></returns>
		public bool CodePropertyAdd( int categoryId, EnumeratedItem item )
		{
			bool isOK = true;
			using ( var context = new EM.CTIEntities() )
			{
				//first ensure doesn't exist

				//now add. 
				

			}

			return isOK;
		}
		#endregion

		#region NON-json emumerations retrieve
		/// <summary>
		/// Get an enumeration
		/// </summary>
		/// <param name="datasource"></param>
		/// <param name="getAll">If false, only return codes with Totals > 0</param>
		/// <returns></returns>
		public static Enumeration GetEnumeration( string datasource, bool getAll = true, bool onlySubType1 = false )
		{
			using ( var context = new EM.CTIEntities() )
			{
				//context.Configuration.LazyLoadingEnabled = false;

				Codes_PropertyCategory category = context.Codes_PropertyCategory
							.SingleOrDefault( s => s.CodeName.ToLower() == datasource && s.IsActive == true );

				return FillEnumeration( category, getAll, onlySubType1 );
			}
		
		}

		public static Enumeration GetEnumeration( int categoryId, bool getAll = true )
		{
			using ( var context = new EM.CTIEntities() )
			{
				
				Codes_PropertyCategory category = context.Codes_PropertyCategory
							.SingleOrDefault( s => s.Id == categoryId && s.IsActive == true );

				return FillEnumeration( category, getAll, false );
				
			}

		}
		private static Enumeration FillEnumeration( Codes_PropertyCategory category, bool getAll, bool onlySubType1 )
		{
			Enumeration entity = new Enumeration();
			using ( var context = new EM.CTIEntities() )
			{
				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();
					//just in case??
					//actually do always to get proper sort order
					//typically has properties, unless non-property related like SOC, NAICS, etc
					if ( category.Codes_PropertyValue == null 
						|| category.Codes_PropertyValue.Count == 0
						|| getAll  == false
						|| onlySubType1 )
					{
						category.Codes_PropertyValue.Clear();
						category.Codes_PropertyValue = context.Codes_PropertyValue
									.Where( s => s.CategoryId == category.Id && s.IsActive == true
									&& (getAll || s.Totals > 0)
									&& ( 
										(!onlySubType1 ) 
										|| ( onlySubType1 && s.IsSubType1 == true )
										)
									)
									.OrderBy( s => s.SortOrder ).ThenBy( s => s.Title )
									.ToList();
					}

					if ( category.Codes_PropertyValue != null && category.Codes_PropertyValue.Count > 0 )
					{
						EnumeratedItem val = new EnumeratedItem();
						var sortedList = category.Codes_PropertyValue.Where( s => s.IsActive == true ).OrderBy( x => x.SortOrder ).ThenBy( z => z.Title ).ToList();

						if ( category.InterfaceType == 2 )
						{
							//val = new EnumeratedItem();
							//val.Id = 0;
							//val.CodeId = 0;
							//val.Name = "Select a " + category.Title;
							//val.Description = "";
							//val.SortOrder = 0;
							//val.SchemaName = "";
							//val.SchemaUrl = "0";
							//entity.Items.Add( val );
						}

						//foreach ( Codes_PropertyValue item in category.Codes_PropertyValue )
						foreach ( Codes_PropertyValue item in sortedList )
						{
							val = new EnumeratedItem();
							val.Id = item.Id;
							val.CodeId = item.Id;
							val.Name = item.Title;
							val.Description = item.Description != null ? item.Description : "";
							val.SortOrder = item.SortOrder != null ? ( int ) item.SortOrder : 0;
							val.SchemaName = item.SchemaName != null ? item.SchemaName : "";
							val.SchemaUrl = item.SchemaUrl;
							val.Value = item.Id.ToString();
							val.Totals = item.Totals ?? 0;

							entity.Items.Add( val );
						}
						//need to reorder the Items by sortOrder, then name. 
					}
					else
					{
						//typically categories without properties, like Naics, SOC, etc
						if ( " 6 10 11 12 13 23".IndexOf( category.Id .ToString())  == -1) {
						Utilities.LoggingHelper.DoTrace( 6, string.Format("$$$$$$ no properties were found for categoryId: {0}, Category: {1}", category.Id, category.Title ));
						}
					}
				}
			}

			return entity;
		}

		/// <summary>
		/// Get the selected item from an enumeration that only allows a singles selection
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public static int GetEnumerationSelection( Enumeration e )
		{
			int selectedId = 0;
			if ( e == null || e.Items == null || e.Items.Count() == 0 )
			{
				return 0;
			}

			foreach ( EnumeratedItem item in e.Items )
			{
				if ( item.Selected )
				{
					selectedId = item.Id;
					break;
				}
			}

			return selectedId;

		}
		#endregion

		#region JSON ENNUMERATIONS - NOT USED APPARENTLY ===
		/*
		/// <summary>
		/// Get an enumeration (code category and property values)
		/// ==> TODO: add caching
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static Enumeration GetJsonEnumeration( string name )
		{
			Enumeration entity = new Enumeration();
			using ( var context = new EM.CTIEntities() )
			{

				Codes_PropertyCategory category = context.Codes_PropertyCategory
							.Include( "Codes_PropertyValue" )
							.SingleOrDefault( s => s.Title.ToLower() == name.ToLower() 
												&& s.IsActive == true );

				return FillJsonEnumeration( category );
			}

		}

		public static Enumeration GetJsonEnumeration( int categoryId )
		{
			Enumeration entity = new Enumeration();
			using ( var context = new EM.CTIEntities() )
			{

				Codes_PropertyCategory category = context.Codes_PropertyCategory
							.Include( "Codes_PropertyValue" )
							.SingleOrDefault( s => s.Id == categoryId && s.IsActive == true );
				return FillJsonEnumeration( category );
			}

		}

		public static Enumeration FillJsonEnumeration( Codes_PropertyCategory category )
		{
			Enumeration entity = new Enumeration();
			using ( var context = new EM.CTIEntities() )
			{

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();
					//just incase??
					if ( category.Codes_PropertyValue == null )
					{
						category.Codes_PropertyValue = context.Codes_PropertyValue
									.Where( s => s.CategoryId == category.Id && s.IsActive == true )
									.OrderBy( s => s.SortOrder ).ThenBy( s => s.Title )
									.ToList();
					}

					if ( category.Codes_PropertyValue != null && category.Codes_PropertyValue.Count > 0 )
					{
						EnumeratedItem val = new EnumeratedItem();

						foreach ( Codes_PropertyValue item in category.Codes_PropertyValue )
						{
							val = new EnumeratedItem();
							val.Id = item.Id;
							val.Name = item.Title;
							val.Description = item.Description != null ? item.Description : "";
							val.SortOrder = item.SortOrder != null ? ( int ) item.SortOrder : 0;
							val.SchemaName = item.SchemaName != null ? item.SchemaName : "";
							val.Url = item.SchemaUrl;

							entity.Items.Add( val );
						}

					}


				}
			}

			return entity;
		}
		*/
		#endregion

		#region Code Items
		public static List<CodeItem> Property_GetValues( string categoryCodeName, bool insertSelectTitle, bool getAll = true )
		{
			using ( var context = new EM.CTIEntities() )
			{
				Codes_PropertyCategory category = context.Codes_PropertyCategory
							.SingleOrDefault( s => s.CodeName.ToLower() == categoryCodeName && s.IsActive == true );

				return Property_GetValues( category.Id, category.Title, insertSelectTitle, getAll );
			}

		}
		//public static List<CodeItem> Property_GetValues( string categoryCodeName, bool insertSelectTitle = true )
		//{
		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		Codes_PropertyCategory category = context.Codes_PropertyCategory
		//					.SingleOrDefault( s => s.CodeName.ToLower() == categoryCodeName && s.IsActive == true );

		//		return Property_GetValues( category, insertSelectTitle );
		//	}

		//}
		//	public static List<CodeItem> Property_GetValues( Codes_PropertyCategory category, bool insertingSelectTitle = true )
		//{
		public static List<CodeItem> Property_GetValues( int categoryId, string categoryTitle, bool insertingSelectTitle = true, bool getAll = true )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem code;

			using ( var context = new EM.CTIEntities() )
			{
				List<Data.Codes_PropertyValue> results = context.Codes_PropertyValue
					.Where (s => s.CategoryId == categoryId
							&& (s.Totals > 0 || getAll))
							.OrderBy( s => s.SortOrder ).ThenBy( s => s.Title )
							.ToList();

				if ( results != null && results.Count > 0 )
				{
					if ( insertingSelectTitle )
					{
						code = new CodeItem();
						code.Id = 0;
						code.Title = "Select " + categoryTitle;
						code.URL = "";
						list.Add( code );
					}
					foreach ( Codes_PropertyValue item in results )
					{
						code = new CodeItem();
						code.Id = (int) item.Id;
						code.Title = item.Title;
						code.URL = item.SchemaUrl;
						code.Totals = item.Totals ?? 0;

						list.Add( code );
					}
				}
			}
			return list;
		}
		public static List<CodeItem> Property_GetSummaryValues( int categoryId, bool insertingSelectTitle = true )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem code;

			using ( var context = new ViewContext() )
			{
				List<Views.CodesProperty_Summary> results = context.CodesProperty_Summary
					.Where( s => s.CategoryId == categoryId )
							.OrderBy( s => s.SortOrder ).ThenBy( s => s.Property )
							.ToList();

				if ( results != null && results.Count > 0 )
				{
					if ( insertingSelectTitle )
					{
						code = new CodeItem();
						code.Id = 0;
						code.Title = "Select " + results[ 0 ].Category;
						code.URL = "";
						list.Add( code );
					}
					foreach ( Views.CodesProperty_Summary item in results )
					{
						code = new CodeItem();
						code.Id = ( int ) item.PropertyId;
						code.Title = item.Property;
						code.URL = item.PropertySchemaUrl;

						list.Add( code );
					}
				}
			}
			return list;
		}

		/// <summary>
		/// Get a code item by category and title
		/// </summary>
		/// <param name="categoryId"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		public static CodeItem Codes_PropertyValue_Get( int categoryId, string title )
		{
			CodeItem code = new CodeItem();

			using ( var context = new EM.CTIEntities() )
			{
				List<Data.Codes_PropertyValue> results = context.Codes_PropertyValue
					.Where( s => s.CategoryId == categoryId
							&& s.Title.ToLower() == title.ToLower())
							.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Codes_PropertyValue item in results )
					{
						code = new CodeItem();
						code.Id = ( int ) item.Id;
						code.Title = item.Title;
						code.URL = item.SchemaUrl;
						code.Totals = item.Totals ?? 0;
						break;
					}
				}
			}
			return code;
		}
		#endregion

		#region country/Currency Codes
		public static List<CodeItem> GetCountries_AsCodes()
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();
			using ( var context = new Data.CTIEntities() )
			{
				List<EM.Codes_Countries> results = context.Codes_Countries
					.Where( s => s.IsActive == true )
									.OrderBy( s => s.SortOrder ).ThenBy(s => s.CommonName)
									.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( EM.Codes_Countries item in results )
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Name = item.CommonName;

						list.Add( entity );
					}
				}
			}

			return list;
		}
		public static Enumeration GetCountries()
		{

			Enumeration entity = new Enumeration();
			EnumeratedItem val = new EnumeratedItem();

			using ( var context = new EM.CTIEntities() )
			{
				Codes_PropertyCategory category = context.Codes_PropertyCategory
				.SingleOrDefault( s => s.Id == PROPERTY_CATEGORY_CURRENCIES );

				entity.Id = category.Id;
				entity.Name = category.Title;
				entity.SchemaName = category.SchemaName;
				entity.Url = category.SchemaUrl;
				entity.Items = new List<EnumeratedItem>();

				List<Codes_Countries> results = context.Codes_Countries
					.Where(s => s.IsActive == true)
					.OrderBy( s => s.SortOrder )
					.ThenBy( s => s.CommonName )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					//val = new EnumeratedItem();
					//val.Id = 0;
					//val.CodeId = val.Id;
					//val.Name = "Select a Currency";
					//val.Description = "";
					//val.SortOrder = 0;
					//val.Value = val.Id.ToString();

					//entity.Items.Add( val );

					foreach ( Codes_Countries item in results )
					{
						val = new EnumeratedItem();
						//not sure if should use Id or countryNumber. The latter should be the published value. 
						//there are duplicate country numbers, all of which have set inactive for now
						val.Id = ( int ) item.CountryNumber;
						val.CodeId = val.Id;
						val.Name = item.CommonName + " (" + item.CurrencyCode + ")";
						val.Description = item.CommonName + " (" + item.CurrencyCode + ")";
						val.SortOrder = item.SortOrder;
						val.Value = val.Id.ToString();

						entity.Items.Add( val );
					}
				}
			}

			return entity;
		}
		public static Enumeration GetCurrencies()
		{

			Enumeration entity = new Enumeration();
			EnumeratedItem val = new EnumeratedItem();

			using ( var context = new EM.CTIEntities() )
			{
				Codes_PropertyCategory category = context.Codes_PropertyCategory
				.SingleOrDefault( s => s.Id == PROPERTY_CATEGORY_CURRENCIES );

				entity.Id = category.Id;
				entity.Name = category.Title;
				entity.SchemaName = category.SchemaName;
				entity.Url = category.SchemaUrl;
				entity.Items = new List<EnumeratedItem>();

				List<Codes_Currency> results = context.Codes_Currency
					.OrderBy( s => s.SortOrder )
					.ThenBy( s => s.Currency )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					//val = new EnumeratedItem();
					//val.Id = 0;
					//val.CodeId = val.Id;
					//val.Name = "Select a Currency";
					//val.Description = "";
					//val.SortOrder = 0;
					//val.Value = val.Id.ToString();

					//entity.Items.Add( val );

					foreach ( Codes_Currency item in results )
					{
						val = new EnumeratedItem();
						val.Id = ( int ) item.NumericCode;
						val.CodeId = val.Id;
						val.Name = item.Currency + " (" + item.AlphabeticCode + ")";
						val.Description = item.Currency;
						val.SortOrder = item.SortOrder != null ? ( int ) item.SortOrder : 0;
						val.Value = val.Id.ToString();

						entity.Items.Add( val );
					}
				}
			}

			return entity;
		}
		#endregion
		#region SOC
		//public static List<CodeItem> SOC_Search( int credentialId, int headerId = 0, string keyword = "", int pageNumber = 1, int pageSize = 0 )
		//{
		//	int totalRows = 0;

		//	return SOC_Search( credentialId, headerId, keyword, pageNumber, pageSize, ref totalRows );
		//}
		public static List<CodeItem> SOC_Search( int headerId, string keyword, int pageNumber, int pageSize, ref int totalRows, bool getAll = true )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();
			keyword = keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 100;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = (pageNumber - 1 ) * pageSize;

			using ( var context = new EM.CTIEntities() )
			{
				List<ONET_SOC> results = context.ONET_SOC
					.Where( s => (headerId == 0 || s.OnetSocCode.Substring(0,2) == headerId.ToString())
						&& (keyword == "" 
						|| s.OnetSocCode.Contains( keyword )
						|| s.SOC_code.Contains( keyword ) 
						|| s.Title.Contains( keyword ))
						&& ( s.Totals > 0 || getAll )
						)
					.OrderBy( s => s.OnetSocCode )
					.Skip(skip)
					.Take( pageSize )
					.ToList();

				totalRows = context.ONET_SOC
					.Where( s => ( headerId == 0 || s.OnetSocCode.Substring( 0, 2 ) == headerId.ToString() )
						&& (keyword == "" 
						|| s.OnetSocCode.Contains( keyword )
						|| s.SOC_code.Contains( keyword ) 
						|| s.Title.Contains( keyword ))
						&& ( s.Totals > 0 || getAll )
						)
					.ToList().Count();

				if ( results != null && results.Count > 0 )
				{
					foreach ( ONET_SOC item in results )
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Name = item.Title;// +" ( " + item.OnetSocCode + " )";
						entity.Description = item.Description ;
						entity.URL = item.URL;
						entity.SchemaName = item.OnetSocCode;
						entity.Code = item.JobFamily.ToString();
						entity.Totals = item.Totals ?? 0;

						list.Add( entity );
					}
				}
			}

			return list;
		}
		/// <summary>
		/// ONET SOC autocomplete
		/// </summary>
		/// <param name="headerId"></param>
		/// <param name="keyword"></param>
		/// <param name="pageSize"></param>
		/// <param name="sortField">Description or SOC_code</param>
		/// <returns></returns>
		public static List<CodeItem> SOC_Autocomplete( int headerId = 0, string keyword = "", int pageSize = 0, string sortField = "Description" )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();
			keyword = keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 100;

			using ( var context = new EM.CTIEntities() )
			{
				//List<ONET_SOC> results1 = context.ONET_SOC
				//	.Where( s => ( headerId == 0 || s.OnetSocCode.Substring( 0, 2 ) == headerId.ToString() )
				//		&& ( keyword == ""
				//		|| s.OnetSocCode.Contains( keyword )
				//		|| s.Title.Contains( keyword ) ) )
				//	.Take( pageSize )
				//	.OrderBy( s => s.OnetSocCode )
				//	.ToList();

				var Query = from P in context.ONET_SOC
							.Where( s => ( headerId == 0 || s.OnetSocCode.Substring( 0, 2 ) == headerId.ToString() )
						&& ( keyword == ""
						|| s.OnetSocCode.Contains( keyword )
						|| s.Title.Contains( keyword ) ) )
							select P;

				if ( sortField == "SOC_code" )
				{
					Query = Query.OrderBy( p => p.SOC_code );
				}
				else
				{
					Query = Query.OrderBy( p => p.Title );
				}
				var count = Query.Count();
				var results = Query.Take( pageSize )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( ONET_SOC item in results )
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Name = item.Title;
						entity.Description = " ( " + item.OnetSocCode + " )" + item.Title ;
						list.Add( entity );
					}
				}
			}

			return list;
		}

		public static List<CodeItem> SOC_Categories( string sortField = "Description", bool includeCategoryCode = false )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem code;

			using ( var context = new EM.CTIEntities() )
			{
				var Query = from P in context.ONET_SOC_JobFamily
							select P;

				if ( sortField == "JobFamilyId" )
				{
					Query = Query.OrderBy( p => p.JobFamilyId );
				} else
				{
					Query = Query.OrderBy( p => p.Description );
				}
				var count = Query.Count();
				var results = Query.ToList();
				//List<ONET_SOC_JobFamily> results2 = context.ONET_SOC_JobFamily
				//	.OrderBy( s => s.Description )
				//	.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( ONET_SOC_JobFamily item in results )
					{
						code = new CodeItem();
						code.Id = item.JobFamilyId;
						if ( includeCategoryCode )
						{
							if ( sortField == "JobFamilyId" )
								code.Title = item.JobFamilyId + " - " + item.Description;
							else
								code.Title = item.Description + " (" + item.JobFamilyId + ")";
						}
						else
							code.Title = item.Description;

						list.Add( code );
					}
				}
			}
			return list;
		}
		#endregion


		#region NAICS
		public static List<CodeItem> NAICS_Search( int headerId = 0, string keyword = "", int pageNumber = 1, int pageSize = 0, bool getAll = true )
		{
			int totalRows = 0;

			return NAICS_Search( headerId, keyword, pageNumber, pageSize, getAll, ref totalRows );
		}
		public static List<CodeItem> NAICS_Search( int headerId, string keyword, int pageNumber, int pageSize, bool getAll, ref int totalRows )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();
			keyword = keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 100;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;

			using ( var context = new EM.CTIEntities() )
			{
				List<NAICS> results = context.NAICS
						.Where( s => ( headerId == 0 || s.NaicsCode.Substring( 0, 2 ) == headerId.ToString() )
						&& ( keyword == ""
						|| s.NaicsCode.Contains( keyword )
						|| s.NaicsTitle.Contains( keyword ) )
						&& ( s.Totals > 0 || getAll )
						)
					.OrderBy( s => s.NaicsCode )
					.Skip( skip )
					.Take( pageSize )
					.ToList();
				totalRows = context.NAICS
						.Where( s => ( headerId == 0 || s.NaicsCode.Substring( 0, 2 ) == headerId.ToString() )
						&& ( keyword == ""
						|| s.NaicsCode.Contains( keyword )
						|| s.NaicsTitle.Contains( keyword ) )
						&& ( s.Totals > 0 || getAll )
						)
					.ToList().Count();
				
				if ( results != null && results.Count > 0 )
				{
					foreach ( NAICS item in results )
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Name = item.NaicsTitle;// + " ( " + item.NaicsCode + " )";
						entity.Description = "";// 						item.NaicsTitle + " ( " + item.NaicsCode + " )";
						entity.URL = item.URL;
						entity.SchemaName = item.NaicsCode;
						entity.Code = item.NaicsGroup.ToString();
						entity.Totals = item.Totals ?? 0;

						list.Add( entity );
					}
				}
			}

			return list;
		}

		public static List<CodeItem> NAICS_Autocomplete( int headerId = 0, string keyword = "", int pageSize = 0 )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();
			keyword = keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 100;

			using ( var context = new EM.CTIEntities() )
			{
				List<NAICS> results = context.NAICS
						.Where( s => ( headerId == 0 || s.NaicsCode.Substring( 0, 2 ) == headerId.ToString() )
						&& ( keyword == ""
						|| s.NaicsCode.Contains( keyword )
						|| s.NaicsTitle.Contains( keyword ) ) )
						.OrderBy( s => s.NaicsCode )
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( NAICS item in results )
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Name = item.NaicsTitle;
						entity.Description = item.NaicsTitle + " ( " + item.NaicsCode + " )";
						entity.URL = item.URL;
						entity.Totals = item.Totals ?? 0;
						list.Add( entity );
					}
				}
			}

			return list;
		}

		public static List<CodeItem> NAICS_Categories( string sortField = "Description", bool includeCategoryCode = false )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity;
			using ( var context = new EM.CTIEntities() )
			{
				//List<NAICS> results = context.NAICS
				//	.Where( s => s.NaicsCode.Length == 2 && s.NaicsGroup > 10)
				//	.OrderBy( s => s.NaicsCode )
				//	.ToList();
				var Query = from P in context.NAICS
							.Where( s => s.NaicsCode.Length == 2 && s.NaicsGroup > 10 )
							select P;

				if ( sortField == "NaicsGroup" )
				{
					Query = Query.OrderBy( p => p.NaicsGroup );
				}
				else
				{
					Query = Query.OrderBy( p => p.NaicsTitle );
				}
				var results = Query.ToList();
				if ( results != null && results.Count > 0 )
				{
					foreach ( NAICS item in results )
					{
						entity = new CodeItem();
						entity.Id = Int32.Parse( item.NaicsCode );
						
						if ( includeCategoryCode )
						{
							if ( sortField == "NaicsGroup" )
								entity.Title = item.NaicsCode + " - " + item.NaicsTitle;
							else
								entity.Title = item.NaicsTitle + " (" + item.NaicsCode + ")";
						}
						else
							entity.Title = item.NaicsTitle;

						entity.URL = item.URL;

						list.Add( entity );
					}
				}
			}

			return list;
		}
		#endregion

		#region CIPS
		public static List<CodeItem> CIPS_Search( int headerId = 0, string keyword = "", int pageNumber = 1, int pageSize = 0 )
		{
			int totalRows = 0;

			return CIPS_Search( headerId, keyword, pageNumber, pageSize, ref totalRows );
		}
		public static List<CodeItem> CIPS_Search( int headerId, string keyword, int pageNumber, int pageSize, ref int totalRows )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();
			string header = headerId.ToString();
			if ( headerId > 0 && headerId < 10 )
				header = "0" + header;
			keyword = keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 100;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;

			using ( var context = new EM.CTIEntities() )
			{
				List<CIPCode2010> results = context.CIPCode2010
						.Where( s => ( headerId == 0 || s.CIPCode.Substring( 0, 2 ) == header )
						&& ( keyword == ""
						|| s.CIPCode.Contains( keyword )
						|| s.CIPTitle.Contains( keyword ) ) )
					.OrderBy( s => s.CIPCode )
					.Skip( skip )
					.Take( pageSize )
					.ToList();
				totalRows = context.CIPCode2010
						.Where( s => ( headerId == 0 || s.CIPCode.Substring( 0, 2 ) == header )
						&& ( keyword == ""
						|| s.CIPCode.Contains( keyword )
						|| s.CIPTitle.Contains( keyword ) ) )
					.ToList().Count();

				if ( results != null && results.Count > 0 )
				{
					foreach ( CIPCode2010 item in results )
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Name = item.CIPTitle  + " ( " + item.CIPCode + " )";
						entity.Description = item.CIPDefinition;
						//entity.URL = item.URL;
						entity.SchemaName = item.CIPCode;
						entity.Code = item.CIPFamily;
						entity.Totals = item.Totals ?? 0;
						list.Add( entity );
					}
				}
			}

			return list;
		}

		public static List<CodeItem> CIPS_Autocomplete( int headerId = 0, string keyword = "", int pageSize = 0 )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();
			keyword = keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 100;

			using ( var context = new EM.CTIEntities() )
			{
				List<CIPCode2010> results = context.CIPCode2010
						.Where( s => ( headerId == 0 || s.CIPFamily == headerId.ToString() )
						&& ( keyword == ""
						|| s.CIPTitle.Contains( keyword )
						|| s.CIPDefinition.Contains( keyword ) ) )
						.OrderBy( s => s.CIPCode )
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( CIPCode2010 item in results )
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Name = item.CIPTitle;
						entity.Description = item.CIPTitle + " ( " + item.CIPCode + " )";
						//entity.URL = item.URL;
						entity.Totals = item.Totals ?? 0;
						list.Add( entity );
					}
				}
			}

			return list;
		}

		public static List<CodeItem> CIPS_Categories( string sortField = "CIPFamily" )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity;
			using ( var context = new EM.CTIEntities() )
			{
				//List<CIPS> results = context.CIPS
				//	.Where( s => s.NaicsCode.Length == 2 && s.NaicsGroup > 10)
				//	.OrderBy( s => s.NaicsCode )
				//	.ToList();
				var Query = from P in context.CIPCode2010
							.Where( s => s.CIPCode.Length == 2 )
							select P;

				if ( sortField == "CIPFamily" )
				{
					Query = Query.OrderBy( p => p.CIPFamily );
				}
				else
				{
					Query = Query.OrderBy( p => p.CIPTitle );
				}
				var results = Query.ToList();
				if ( results != null && results.Count > 0 )
				{
					foreach ( CIPCode2010 item in results )
					{
						entity = new CodeItem();
						entity.Id = Int32.Parse( item.CIPFamily );
						if ( sortField == "CIPFamily" )
							entity.Title = item.CIPCode + " - " + item.CIPTitle;
						else
							entity.Title = item.CIPTitle + " (" + item.CIPCode + ")";
						//entity.URL = item.URL;

						list.Add( entity );
					}
				}
			}

			return list;
		}
		#endregion

		#region Competency Frameworks
		//public static List<CodeItem> CompetencyFrameworks_GetAll()
		//{
		//	List<CodeItem> list = new List<CodeItem>();
		//	CodeItem code;

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		List<Data.CompetencyFramework> results = context.CompetencyFramework
		//					.OrderBy( s => s.Name )
		//					.ToList();

		//		if ( results != null && results.Count > 0 )
		//		{
		//			foreach ( CompetencyFramework item in results )
		//			{
		//				code = new CodeItem();
		//				code.Id = item.Id;
		//				code.Title = item.Name;
		//				code.URL = item.Url;

		//				list.Add( code );
		//			}
		//		}
		//	}
		//	return list;
		//}
		#endregion
	}
}
