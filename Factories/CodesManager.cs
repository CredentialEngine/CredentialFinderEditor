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
using EntityContext = Data.CTIEntities;
namespace Factories
{
	public class CodesManager : BaseFactory
	{
		#region constants - property categories
		public static int PROPERTY_CATEGORY_JURISDICTION = 1;   //N/A
		public static int PROPERTY_CATEGORY_CREDENTIAL_TYPE = 2;
		public static int PROPERTY_CATEGORY_CREDENTIAL_PURPOSE = 3;
		/// <summary>
		/// AudienceLevelType
		/// </summary>
		public static int PROPERTY_CATEGORY_AUDIENCE_LEVEL = 4;

		public static int PROPERTY_CATEGORY_CREDENTIAL_ATTAINMENT_COST = 5;
		public static int PROPERTY_CATEGORY_ORG_SERVICE = 6;
		public static int PROPERTY_CATEGORY_ORGANIZATION_TYPE = 7;
		public static int PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA = 8;
		public static int PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS = 9;

		public static int PROPERTY_CATEGORY_NAICS = 10;
		public static int PROPERTY_CATEGORY_SOC = 11;
		public static int PROPERTY_CATEGORY_MOC = 12;

		public static int PROPERTY_CATEGORY_ENTITY_AGENT_ROLE = 13;
		public static int PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE = 13;
		public static int PROPERTY_CATEGORY_AUDIENCE_TYPE = 14;
		public static int PROPERTY_CATEGORY_CONNECTION_PROFILE_TYPE = 15;
		public static int PROPERTY_CATEGORY_ASSESSMENT_TYPE = 16;

		public static int PROPERTY_CATEGORY_ASMT_DELIVERY_TYPE = 18;
		public static int PROPERTY_CATEGORY_ENROLLMENT_TYPE = 19;
		public static int PROPERTY_CATEGORY_RESIDENCY_TYPE = 20;

		public static int PROPERTY_CATEGORY_DELIVERY_TYPE = 21;
		public static int PROPERTY_CATEGORY_JURISDICTION_PROFILE_PURPOSE = 22;
		public static int PROPERTY_CATEGORY_CIP = 23;
		public static int PROPERTY_CATEGORY_CURRENCIES = 24;
		public static int PROPERTY_CATEGORY_REFERENCE_URLS = 25;
		public static int PROPERTY_CATEGORY_REVOCATION_CRITERIA_TYPE = 26;
		public static int PROPERTY_CATEGORY_CREDENTIAL_URLS = 27;
		public static int PROPERTY_CATEGORY_CONDITION_ITEM = 28;
		public static int PROPERTY_CATEGORY_COMPETENCY = 29;

		public static int PROPERTY_CATEGORY_ORGANIZATION_SECTORTYPE = 30;
		public static int PROPERTY_CATEGORY_PHONE_TYPE = 31;
		public static int PROPERTY_CATEGORY_EMAIL_TYPE = 32;
		public static int PROPERTY_CATEGORY_AGENT_QAPURPOSE_TYPE = 33;
		public static int PROPERTY_CATEGORY_SUBJECT = 34;
		public static int PROPERTY_CATEGORY_KEYWORD = 35;
		public static int PROPERTY_CATEGORY_ALIGNMENT_TYPE = 36;
		public static int PROPERTY_CATEGORY_ASSESSMENT_USE_TYPE = 37;
		public static int PROPERTY_CATEGORY_ALTERNATE_NAME = 38;
		public static int PROPERTY_CATEGORY_CREDENTIAL_STATUS_TYPE = 39;

		public static int PROPERTY_CATEGORY_ACTION_STATUS_TYPE = 40;
		public static int PROPERTY_CATEGORY_CLAIM_TYPE = 41;
		public static int PROPERTY_CATEGORY_EXTERNAL_INPUT_TYPE = 42;
		//      [Obsolete]
		//      public static int PROPERTY_CATEGORY_PROCESS_METHOD = 43;
		//      [Obsolete]
		//      public static int PROPERTY_CATEGORY_STAFF_EVALUATION_METHOD = 44;
		public static int PROPERTY_CATEGORY_QA_TARGET_TYPE = 45;
		public static int PROPERTY_CATEGORY_LEARNING_RESOURCE_URLS = 46;
		public static int PROPERTY_CATEGORY_OWNING_ORGANIZATION_TYPE = 47;
		public static int PROPERTY_CATEGORY_PRIMARY_EARN_METHOD = 48;

		public static int PROPERTY_CATEGORY_CREDIT_UNIT_TYPE = 50;
		public static int PROPERTY_CATEGORY_JurisdictionAssertionType = 52;
		public static int PROPERTY_CATEGORY_Learning_Method_Type = 53;
		public static int PROPERTY_CATEGORY_Scoring_Method = 54;

		public static int PROPERTY_CATEGORY_Assessment_Method_Type = 56;

		public static int PROPERTY_CATEGORY_SUBMISSION_ITEM = 57;

		//reporting

		//continued
		public static int PROPERTY_CATEGORY_DEGREE_CONCENTRATION = 62;
		public static int PROPERTY_CATEGORY_DEGREE_MAJOR = 63;
		public static int PROPERTY_CATEGORY_DEGREE_MINOR = 64;
		public static int PROPERTY_CATEGORY_LANGUAGE = 65;


		#endregion
		#region constants - entity types. 
		//An Entity is typically created only where it can have a child relationship, ex: Entity.Property
		public static int ENTITY_TYPE_CREDENTIAL = 1;
		public static int ENTITY_TYPE_ORGANIZATION = 2;
		public static int ENTITY_TYPE_ASSESSMENT_PROFILE = 3;
		public static int ENTITY_TYPE_CONNECTION_PROFILE = 4;
		public static int ENTITY_TYPE_CONDITION_PROFILE = 4;
		public static int ENTITY_TYPE_COST_PROFILE = 5;
		public static int ENTITY_TYPE_COST_PROFILE_ITEM = 6;
		public static int ENTITY_TYPE_LEARNING_OPP_PROFILE = 7;
		public static int ENTITY_TYPE_TASK_PROFILE = 8;
		public static int ENTITY_TYPE_PERSON = 9;

		public static int ENTITY_TYPE_COMPETENCY_FRAMEWORK = 10;
		public static int ENTITY_TYPE_CONCEPT_SCHEME = 11;

		public static int ENTITY_TYPE_REVOCATION_PROFILE = 12;
		public static int ENTITY_TYPE_VERIFICATION_PROFILE = 13;
		public static int ENTITY_TYPE_PROCESS_PROFILE = 14;
		public static int ENTITY_TYPE_CONTACT_POINT = 15;
		public static int ENTITY_TYPE_ADDRESS_PROFILE = 16;
		public static int ENTITY_TYPE_CASS_COMPETENCY_FRAMEWORK = 17;
		//...see below
		public static int ENTITY_TYPE_CONDITION_MANIFEST = 19;
		public static int ENTITY_TYPE_COST_MANIFEST = 20;
		/// <summary>
		/// Placeholder for stats, will not actually have an entity
		/// </summary>
		public static int ENTITY_TYPE_JURISDICTION_PROFILE = 18;
		public static int ENTITY_TYPE_DURATION_PROFILE = 22;

		#endregion
		#region constants - entity status
		public static int ENTITY_STATUS_IN_PROGRESS = 1;
		public static int ENTITY_STATUS_PUBLISHED = 2;
		public static int ENTITY_STATUS_EXTERNAL_REFERENCE = 3;

		public static int ENTITY_STATUS_DELETED = 6;
		#endregion
		//public static Enumeration GetSampleEnumeration( string dataSource, string schemaName )
		//{
		//	var result = Data.Development.TemporaryEnumerationData.GetEnumeration( dataSource, schemaName );

		//	return result;
		//}

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

		#region NON-json enumerations retrieve
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
							.FirstOrDefault( s => s.CodeName.ToLower() == datasource.ToLower() && s.IsActive == true );

				return FillEnumeration( category, getAll, onlySubType1 );
			}

		}

		public static Enumeration GetEnumeration( int categoryId, bool getAll = true )
		{
			using ( var context = new EM.CTIEntities() )
			{

				Codes_PropertyCategory category = context.Codes_PropertyCategory
							.FirstOrDefault( s => s.Id == categoryId && s.IsActive == true );

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
					entity.Description = category.Description;

					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();
					//just in case??
					//actually do always to get proper sort order
					//typically has properties, unless non-property related like SOC, NAICS, etc
					if ( category.Codes_PropertyValue == null
						|| category.Codes_PropertyValue.Count == 0
						|| getAll == false
						|| onlySubType1 )
					{
						category.Codes_PropertyValue.Clear();
						category.Codes_PropertyValue = context.Codes_PropertyValue
									.Where( s => s.CategoryId == category.Id && s.IsActive == true
									&& ( getAll || s.Totals > 0 )
									&& (
										( !onlySubType1 )
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
							val.SchemaName = item.SchemaName ?? "";
							val.SchemaUrl = item.SchemaUrl;
							val.ParentSchemaName = item.ParentSchemaName ?? "";
							val.Value = item.Id.ToString();
							val.Totals = item.Totals ?? 0;

							if ( IsDevEnv() )
								val.Name += string.Format( " ({0})", val.Totals );
							entity.Items.Add( val );
						}
						//need to reorder the Items by sortOrder, then name. 
					}
					else
					{
						//typically categories without properties, like Naics, SOC, etc
						if ( " 6 10 11 12 13 23".IndexOf( category.Id.ToString() ) == -1 )
						{
							Utilities.LoggingHelper.DoTrace( 6, string.Format( "$$$$$$ no properties were found for categoryId: {0}, Category: {1}", category.Id, category.Title ) );
						}
					}
				}
			}

			return entity;
		}

		public static Enumeration GetCodeAsEnumeration( int categoryId, string title )
		{
			Enumeration entity = new Enumeration();
			if ( string.IsNullOrWhiteSpace( title ) )
				return entity;
			string schemaTag = "";
			//if (!title.ToLower().StartsWith( "ceterm" ))
			//    schemaTag = "ceterm:" + title.Replace( " ", "" ).ToLower();
			title = title.ToLower().Trim();
			schemaTag = title.Replace( " ", "" ).ToLower().Trim();
			if ( schemaTag.IndexOf( ":" ) == -1 )
				schemaTag = ":" + schemaTag;


			using ( var context = new EM.CTIEntities() )
			{
				Codes_PropertyCategory category = context.Codes_PropertyCategory
							.FirstOrDefault( s => s.Id == categoryId && s.IsActive == true );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					EnumeratedItem code = new EnumeratedItem();
					Data.Codes_PropertyValue item = context.Codes_PropertyValue
							.FirstOrDefault( s => s.CategoryId == categoryId
							 && ( s.Title.ToLower() == title
							|| s.SchemaName.ToLower().Contains( schemaTag ) )
							);

					if ( item != null && item.Id > 0 )
					{
						code.Id = ( int ) item.Id;
						code.Name = item.Title;
						code.Description = item.Description;
						code.URL = item.SchemaUrl;
						code.SchemaName = item.SchemaName;
						code.ParentSchemaName = item.ParentSchemaName;
						code.Totals = item.Totals ?? 0;
						entity.Items.Add( code );
					}
				}
			}
			return entity;
		}
		public static EnumeratedItem GetCodeAsEnumerationItem( int categoryId, string title )
		{
			EnumeratedItem code = new EnumeratedItem();
			if ( string.IsNullOrWhiteSpace( title ) )
				return code;
			string schemaTag = "";
			//string schemaPart = "";
			//can't do this as there are different prefixes for each concept scheme
			//if (!title.ToLower().StartsWith( "ceterm" ))
			//    schemaTag = "ceterm:" + title.Replace( " ", "" ).ToLower();
			//schemaPart = title.Replace( " ", "" ).ToLower();
			schemaTag = title.Replace( " ", "" ).ToLower();
			if ( schemaTag.IndexOf( ":" ) == -1 )
				schemaTag = ":" + schemaTag;

			using ( var context = new EM.CTIEntities() )
			{
				Data.Codes_PropertyValue item = context.Codes_PropertyValue
						.FirstOrDefault( s => s.CategoryId == categoryId
						 &&
						 ( s.Title.ToLower() == title.ToLower()
						 || s.SchemaName.ToLower().Contains( schemaTag )
						|| s.SchemaName.ToLower() == schemaTag
						)
						);
				if ( item != null && item.Id > 0 )
				{
					code.Id = ( int ) item.Id;
					code.Name = item.Title;
					code.Description = item.Description;
					code.URL = item.SchemaUrl;
					code.SchemaName = item.SchemaName;
					code.ParentSchemaName = item.ParentSchemaName;
					code.Totals = item.Totals ?? 0;
				}

			}

			return code;
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

		#region Condition profile type
		/// <summary>
		/// Using Codes_ConditionProfileType not Codes_PropertyCategory
		/// </summary>
		/// <param name="getAll"></param>
		/// <param name="usingConditionManifestTitles"></param>
		/// <returns></returns>
		public static Enumeration GetCommonConditionProfileTypes( bool getAll = true, bool usingConditionManifestTitles = false )
		{
			Enumeration entity = new Enumeration();

			using ( var context = new EM.CTIEntities() )
			{
				//get the property category
				EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
							.FirstOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_CONNECTION_PROFILE_TYPE );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.Description = category.Description;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					EnumeratedItem val = new EnumeratedItem();
					var results = context.Codes_ConditionProfileType
							.Where( s => s.IsActive == true && s.IsCommonCondtionType == true && s.Id != 11 )
							.OrderBy( p => p.Title )
							.ToList();

					foreach ( EM.Codes_ConditionProfileType item in results )
					{
						val = new EnumeratedItem
						{
							Id = item.Id,
							CodeId = item.Id,
							Value = item.Id.ToString(),
							Description = item.Description,
							Name = item.Title,
							SchemaName = item.SchemaName,
							Totals = item.Totals ?? 0
						};
						if ( usingConditionManifestTitles )
							val.Name = item.ConditionManifestTitle;

						entity.Items.Add( val );
					}

				}
			}

			return entity;
		}

		public static Enumeration GetCredentialsConditionProfileTypes( bool getAll = true, bool usingConditionManifestTitles = false )
		{
			Enumeration entity = new Enumeration();

			using ( var context = new EM.CTIEntities() )
			{
				//get the property category
				EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
							.FirstOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_CONNECTION_PROFILE_TYPE );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.Description = category.Description;

					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					EnumeratedItem val = new EnumeratedItem();
					var results = context.Codes_ConditionProfileType
							.Where( s => s.IsActive == true && s.IsCredentialsConnectionType == true )
							.OrderBy( p => p.Title )
							.ToList();

					foreach ( EM.Codes_ConditionProfileType item in results )
					{
						val = new EnumeratedItem
						{
							Id = item.Id,
							CodeId = item.Id,
							Value = item.Id.ToString(),
							Description = item.Description,
							Name = item.Title,
							SchemaName = item.SchemaName,
							Totals = item.Totals ?? 0
						};
						if ( usingConditionManifestTitles )
							val.Name = item.ConditionManifestTitle;

						if ( getAll || val.Totals > 0 )
							entity.Items.Add( val );
					}

				}
			}

			return entity;
		}
		public static EnumeratedItem ConditionProfileTypesCodeAsEnumerationItem( string title )
		{
			EnumeratedItem code = new EnumeratedItem();
			if ( string.IsNullOrWhiteSpace( title ) )
				return code;
			string schemaTag = "";
			//string schemaPart = "";
			//can't do this as there are different prefixes for each concept scheme
			//if (!title.ToLower().StartsWith( "ceterm" ))
			//    schemaTag = "ceterm:" + title.Replace( " ", "" ).ToLower();
			//schemaPart = title.Replace( " ", "" ).ToLower();
			schemaTag = title.Replace( " ", "" ).ToLower();
			if ( schemaTag.IndexOf( ":" ) == -1 )
				schemaTag = ":" + schemaTag;

			using ( var context = new EM.CTIEntities() )
			{
				var item = context.Codes_ConditionProfileType
						.FirstOrDefault( s => 
						 ( s.Title.ToLower() == title.ToLower()
						 || s.SchemaName.ToLower().Contains( schemaTag )
						|| s.SchemaName.ToLower() == schemaTag
						)
						);
				if ( item != null && item.Id > 0 )
				{
					code.Id = ( int )item.Id;
					code.Name = item.ConditionManifestTitle;
					code.Description = item.Description;
					code.SchemaName = item.SchemaName;
					code.Totals = item.Totals ?? 0;
				}

			}

			return code;
		}
		public static Enumeration GetConditionManifestConditionTypes( bool getAll = true )
		{
			Enumeration entity = new Enumeration();

			using ( var context = new EM.CTIEntities() )
			{
				//get the property category
				EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
							.FirstOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_CONNECTION_PROFILE_TYPE );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.Description = category.Description;

					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					EnumeratedItem val = new EnumeratedItem();
					var results = context.Codes_ConditionProfileType
							.Where( s => s.IsActive == true && s.IsCredentialsConnectionType == true )
							.OrderBy( p => p.Title )
							.ToList();

					foreach ( EM.Codes_ConditionProfileType item in results )
					{
						val = new EnumeratedItem();
						val.Id = item.Id;
						val.CodeId = item.Id;
						val.Value = item.Id.ToString();
						val.Description = item.Description;
						val.Name = item.ConditionManifestTitle;
						val.SchemaName = item.SchemaName;
						val.Totals = item.Totals ?? 0;

						if ( getAll || val.Totals > 0 )
							entity.Items.Add( val );
					}

				}
			}

			return entity;
		}

		#endregion

		#region Jurisdiction assertions
		public static Enumeration GetJurisdictionAssertions_ForCredentials()
		{
			Enumeration entity = new Enumeration();
			EnumeratedItem val = new EnumeratedItem();
			using ( var context = new EM.CTIEntities() )
			{
				//get the property category
				EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
							.FirstOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_JurisdictionAssertionType );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.Description = category.Description;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					foreach ( EM.Codes_PropertyValue item in category.Codes_PropertyValue )
					{
						val = new EnumeratedItem();
						val.Id = item.Id;
						val.CodeId = item.Id;
						val.Value = item.Id.ToString();
						val.Description = item.Description;
						val.Name = item.Title;
						entity.Items.Add( val );
					}

				}
			}

			return entity;
		}

		public static Enumeration GetJurisdictionAssertions_Filtered( string filter )
		{
			Enumeration entity = new Enumeration();
			EnumeratedItem val = new EnumeratedItem();
			using ( var context = new EM.CTIEntities() )
			{
				//get the property category
				EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
							.FirstOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_JurisdictionAssertionType );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.Description = category.Description;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					foreach ( EM.Codes_PropertyValue item in category.Codes_PropertyValue )
					{
						if ( item.ParentSchemaName.IndexOf( filter ) > -1 )
						{
							val = new EnumeratedItem();
							val.Id = item.Id;
							val.CodeId = item.Id;
							val.Value = item.Id.ToString();
							val.Description = item.Description;
							val.Name = item.Title;

							entity.Items.Add( val );
						}

					}

				}
			}

			return entity;
		}

		#endregion
		#region Code Items
		public static CodeItem Codes_PropertyCategory_Get( int categoryId )
		{
			CodeItem code = new CodeItem();

			using ( var context = new EM.CTIEntities() )
			{
				Codes_PropertyCategory category = context.Codes_PropertyCategory
							.FirstOrDefault( s => s.Id == categoryId && s.IsActive == true );

				if ( category != null && category.Id > 0 )
				{
					code = new CodeItem();
					code.Id = category.Id;
					code.Title = category.Title;
					code.Description = category.Description;
					code.URL = category.SchemaUrl;
					code.SchemaName = category.SchemaName;
				}
			}
			return code;
		}
		public static List<CodeItem> Property_GetValues( string categoryCodeName, bool insertSelectTitle, bool getAll = true )
		{
			using ( var context = new EM.CTIEntities() )
			{
				Codes_PropertyCategory category = context.Codes_PropertyCategory
							.FirstOrDefault( s => s.CodeName.ToLower() == categoryCodeName && s.IsActive == true );

				return Property_GetValues( category.Id, category.Title, insertSelectTitle, getAll );
			}

		}



		public static List<CodeItem> Property_GetValues( int categoryId, string categoryTitle, bool insertingSelectTitle = true, bool getAll = true )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem code;

			using ( var context = new EM.CTIEntities() )
			{
				List<Data.Codes_PropertyValue> results = context.Codes_PropertyValue
					.Where( s => s.CategoryId == categoryId
							&& ( s.Totals > 0 || getAll ) )
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
						code.Id = ( int ) item.Id;
						code.Title = item.Title;
						code.Description = item.Description;
						code.URL = item.SchemaUrl;
						code.SchemaName = item.SchemaName;
						code.ParentSchemaName = item.ParentSchemaName;
						code.Totals = item.Totals ?? 0;

						list.Add( code );
					}
				}
			}
			return list;
		}


		/// <summary>
		/// Check if the provided property schema is valid
		/// </summary>
		/// <param name="category"></param>
		/// <param name="schemaName"></param>
		/// <returns></returns>
		public static bool IsPropertySchemaValid( string categoryCode, ref string schemaName )
		{
			CodeItem item = GetPropertyBySchema( categoryCode, schemaName );

			if ( item != null && item.Id > 0 )
			{
				//the lookup is case insensitive
				//return the actual schema name value
				schemaName = item.SchemaName;
				return true;
			}
			else
				return false;
		}

		public static bool IsPropertySchemaValid( string categoryCode, string schemaName, ref CodeItem item )
		{
			item = GetPropertyBySchema( categoryCode, schemaName );

			if ( item != null && item.Id > 0 )
			{
				return true;
			}
			else
				return false;
		}
		/// <summary>
		/// Get a single property using the category code, and property schema name
		/// </summary>
		/// <param name="category"></param>
		/// <param name="schemaName"></param>
		/// <returns></returns>
		public static CodeItem GetPropertyBySchema( string categoryCode, string schemaName )
		{
			CodeItem code = new CodeItem();

			using ( var context = new EM.CTIEntities() )
			{
				//for the most part, the code schema name should be unique. We may want a extra check on the categoryCode?
				//TODO - need to ensure the schemas are accurate - and not make sense to check here
				Codes_PropertyCategory category = context.Codes_PropertyCategory
							.FirstOrDefault( s => s.SchemaName.ToLower() == categoryCode.ToLower() && s.IsActive == true );

				Data.Codes_PropertyValue item = context.Codes_PropertyValue
					.FirstOrDefault( s => s.SchemaName == schemaName );
				if ( item != null && item.Id > 0 )
				{
					//could have an additional check that the returned category is correct - no guarentees though
					code = new CodeItem();
					code.Id = ( int ) item.Id;
					code.CategoryId = item.CategoryId;
					code.Title = item.Title;
					code.Description = item.Description;
					code.URL = item.SchemaUrl;
					code.SchemaName = item.SchemaName;
					code.ParentSchemaName = item.ParentSchemaName;
					code.Totals = item.Totals ?? 0;
				}
			}
			return code;
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
							&& s.Title.ToLower() == title.ToLower() )
							.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Codes_PropertyValue item in results )
					{
						code = new CodeItem();
						code.Id = ( int ) item.Id;
						code.Title = item.Title;
						code.Description = item.Description;
						code.URL = item.SchemaUrl;
						code.SchemaName = item.SchemaName;
						code.ParentSchemaName = item.ParentSchemaName;
						code.Totals = item.Totals ?? 0;
						break;
					}
				}
			}
			return code;
		}

		/// <summary>
		/// Retrieve a code item by categoryId and schema (or title)
		/// </summary>
		/// <param name="categoryId"></param>
		/// <param name="propertySchema"></param>
		/// <returns></returns>
		public static CodeItem GetPropertyByCategoryAndText( int categoryId, string title )
		{
			CodeItem code = new CodeItem();
			if ( string.IsNullOrWhiteSpace( title ) )
				return code;
			string schemaTag = "";
			if ( !title.ToLower().StartsWith( "ceterm" ) )
				schemaTag = "ceterm:" + title.Replace( " ", "" ).ToLower();

			using ( var context = new EM.CTIEntities() )
			{
				Data.Codes_PropertyValue item = context.Codes_PropertyValue
					.FirstOrDefault( s => s.CategoryId == categoryId
							 && ( s.Title.ToLower() == title.ToLower()
							|| s.SchemaName.ToLower() == schemaTag ) );

				if ( item != null && item.Id > 0 )
				{
					code.Id = ( int ) item.Id;
					code.Title = item.Title;
					code.Description = item.Description;
					code.URL = item.SchemaUrl;
					code.SchemaName = item.SchemaName;
					code.ParentSchemaName = item.ParentSchemaName;
					code.Totals = item.Totals ?? 0;
				}
			}
			return code;
		}
		public static CodeItem Codes_PropertyValue_Get( int propertyId )
		{
			CodeItem code = new CodeItem();

			using ( var context = new EM.CTIEntities() )
			{
				List<Data.Codes_PropertyValue> results = context.Codes_PropertyValue
					.Where( s => s.Id == propertyId )
							.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Codes_PropertyValue item in results )
					{
						code = new CodeItem();
						code.Id = ( int ) item.Id;
						code.Title = item.Title;
						code.Description = item.Description;
						code.URL = item.SchemaUrl;
						code.SchemaName = item.SchemaName;
						code.ParentSchemaName = item.ParentSchemaName;
						code.Totals = item.Totals ?? 0;
						break;
					}
				}
			}
			return code;
		}
		#endregion

		#region country/Currency/Language Codes
		public static List<CodeItem> GetCountries_AsCodes()
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();
			using ( var context = new Data.CTIEntities() )
			{
				List<EM.Codes_Countries> results = context.Codes_Countries
					.Where( s => s.IsActive == true )
									.OrderBy( s => s.SortOrder ).ThenBy( s => s.CommonName )
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
				.FirstOrDefault( s => s.Id == PROPERTY_CATEGORY_CURRENCIES );

				entity.Id = category.Id;
				entity.Name = category.Title;
				entity.Description = category.Description;

				entity.SchemaName = category.SchemaName;
				entity.Url = category.SchemaUrl;
				entity.Items = new List<EnumeratedItem>();

				List<Codes_Countries> results = context.Codes_Countries
					.Where( s => s.IsActive == true )
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
		public static CodeItem GetCountry( string country )
		{
			//EnumeratedItem val = new EnumeratedItem();
			CodeItem code = new CodeItem();
			using ( var context = new EM.CTIEntities() )
			{
				var results = context.Codes_Countries
					.Where( s => s.IsActive == true
					&&
						(
							s.CommonName.ToLower() == country ||
							s.FormalName.ToLower() == country ||
							s.TwoLetterCode.ToLower() == country ||
							s.ThreeLetterCode.ToLower() == country
						)
					)
					.OrderBy( s => s.SortOrder )
					.ThenBy( s => s.CommonName )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Codes_Countries item in results )
					{
						//val = new EnumeratedItem();
						//not sure if should use Id or countryNumber. The latter should be the published value. 
						//there are duplicate country numbers, all of which have set inactive for now
						//val.Id = ( int ) item.CountryNumber;
						//val.CodeId = val.Id;
						//val.Name = item.CommonName + " (" + item.CurrencyCode + ")";
						//val.Description = item.CommonName + " (" + item.CurrencyCode + ")";
						//val.SortOrder = item.SortOrder;
						//val.Value = val.Id.ToString();


						code.Id = item.Id;
						code.Name = item.CommonName;
						code.Description = item.FormalName + " (" + item.CurrencyCode + ")";
						code.SortOrder = item.SortOrder;
						code.Code = ( ( int ) item.CountryNumber ).ToString();
						//should only be one
						break;

					}
				}
			}

			return code;
		}
		public static Enumeration GetCurrencies()
		{

			Enumeration entity = new Enumeration();
			EnumeratedItem val = new EnumeratedItem();

			using ( var context = new EM.CTIEntities() )
			{
				Codes_PropertyCategory category = context.Codes_PropertyCategory
				.FirstOrDefault( s => s.Id == PROPERTY_CATEGORY_CURRENCIES );

				entity.Id = category.Id;
				entity.Name = category.Title;
				entity.Description = category.Description;

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
						val.SchemaName = item.AlphabeticCode; //Need this in publishing and other places - NA 3/17/2017

						entity.Items.Add( val );
					}
				}
			}

			return entity;
		}
		public static EnumeratedItem GetCurrencyItem( string currencyCode )
		{
			EnumeratedItem val = new EnumeratedItem();

			using ( var context = new EM.CTIEntities() )
			{
				Codes_Currency item = context.Codes_Currency
					.FirstOrDefault( s => s.AlphabeticCode.ToLower() == currencyCode.ToLower() );
				if ( item != null && item.NumericCode > 0 )
				{
					val.Id = item.NumericCode;
					val.Value = item.AlphabeticCode;
					val.Name = item.Currency;
					val.SortOrder = item.SortOrder != null ? ( int ) item.SortOrder : 0;
				}
			}

			return val;
		}

		public static Enumeration GetLanguagesForEditor( int entityTypeId = 0, bool getAll = true )
		{
			Enumeration entity = new Enumeration();
			EnumeratedItem val = new EnumeratedItem();

			using ( var context = new EM.CTIEntities() )
			{
				Codes_PropertyCategory category = context.Codes_PropertyCategory
				.FirstOrDefault( s => s.Id == PROPERTY_CATEGORY_LANGUAGE );

				entity.Id = category.Id;
				entity.Name = category.Title;
				entity.Description = category.Description;

				entity.SchemaName = category.SchemaName;
				entity.Url = category.SchemaUrl;
				entity.Items = new List<EnumeratedItem>();

				List<Codes_Language> results = context.Codes_Language
					.Where
					(
						s => ( entityTypeId == 0 || getAll == true )
						|| ( entityTypeId == 1 && s.CredentialTotals > 0 )
						|| ( entityTypeId == 3 && s.AssessmentTotals > 0 )
						|| ( entityTypeId == 7 && s.LearningOpportunityTotals > 0 )
					)
					.OrderBy( s => s.SortOrder )
					.ThenBy( s => s.LanguageName )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Codes_Language item in results )
					{
						val = new EnumeratedItem();
						val.Id = ( int )item.Id;
						val.Value = item.LangugeCode;
						val.Name = item.LanguageName;
						val.Description = item.LanguageName;
						val.SortOrder = item.SortOrder != null ? ( int )item.SortOrder : 0;
						//might not need this if the above where clause gets the necessary data
						if ( getAll || (
							( entityTypeId == 1 && ( int )item.CredentialTotals > 0 ) ||
							( entityTypeId == 3 && ( int )item.AssessmentTotals > 0 ) ||
							( entityTypeId == 7 && ( int )item.LearningOpportunityTotals > 0 ) )
							)
							if ( IsDevEnv() )
								val.Name += string.Format( " ({0})", entityTypeId == 1 ? ( int )item.CredentialTotals : entityTypeId == 3 ? ( int )item.AssessmentTotals :
							 entityTypeId == 7 ? ( int )item.LearningOpportunityTotals : val.Totals );
						entity.Items.Add( val );
					}
				}
			}

			return entity;
		}
		public static Enumeration GetLanguages( int entityTypeId, bool getAll = true )
		{
			Enumeration entity = new Enumeration();
			EnumeratedItem val = new EnumeratedItem();

			using ( var context = new EM.CTIEntities() )
			{
				Codes_PropertyCategory category = context.Codes_PropertyCategory
				.FirstOrDefault( s => s.Id == PROPERTY_CATEGORY_LANGUAGE );

				entity.Id = category.Id;
				entity.Name = category.Title;
				entity.Description = category.Description;

				entity.SchemaName = category.SchemaName;
				entity.Url = category.SchemaUrl;
				entity.Items = new List<EnumeratedItem>();

				var work = from lang in context.Codes_Language
							   join cst in context.Counts_SiteTotals
									on lang.Id equals cst.CodeId into gj
							   from x in gj.DefaultIfEmpty()
							   where x.EntityTypeId == entityTypeId && x.CategoryId == 65
							   select new
							   {
								   lang.Id,
								   lang.LangugeCode,
								   lang.LanguageName,
								   lang.SortOrder,
								   Totals = ( x == null ? 0 : x.Totals )
							   };

				var results = work.OrderBy( s => s.SortOrder )
								.ThenBy( s => s.LanguageName )
								.ToList();

				//List < Codes_Language > results = context.Codes_Language
				//	.Where
				//	(
				//		s => ( entityTypeId == 0 || getAll == true )
				//		|| ( entityTypeId == 1 && s.CredentialTotals > 0 )
				//		|| ( entityTypeId == 3 && s.AssessmentTotals > 0 )
				//		|| ( entityTypeId == 7 && s.LearningOpportunityTotals > 0 )
				//	)
				//	.OrderBy( s => s.SortOrder )
				//	.ThenBy( s => s.LanguageName )
				//	.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( var item in results )
					{
						val = new EnumeratedItem();
						val.Id = ( int ) item.Id;
						val.Value = item.LangugeCode;
						val.Name = item.LanguageName;
						val.Description = item.LanguageName;
						val.SortOrder = item.SortOrder != null ? ( int ) item.SortOrder : 0;
						val.Totals = item.Totals != null ? ( int )item.Totals : 0;
						if ( getAll || ( val.Totals > 0))
						{
							if ( IsDevEnv() )
								val.Name += string.Format( " ({0})", val.Totals );

							entity.Items.Add( val );
						}

						//might not need this if the above where clause gets the necessary data
						//if ( getAll || (
						//	( entityTypeId == 1 && ( int ) item.CredentialTotals > 0 ) ||
						//	( entityTypeId == 3 && ( int ) item.AssessmentTotals > 0 ) ||
						//	( entityTypeId == 7 && ( int ) item.LearningOpportunityTotals > 0 ) )
						//	)

						//	if ( IsDevEnv() )
						//		val.Name += string.Format( " ({0})", entityTypeId == 1 ? ( int ) item.CredentialTotals : entityTypeId == 3 ? ( int ) item.AssessmentTotals :
						//	 entityTypeId == 7 ? ( int ) item.LearningOpportunityTotals : val.Totals );
						//entity.Items.Add( val );
					}
				}
			}

			return entity;
		}

		//public static EnumeratedItem GetLanguage( int languageId )
		//{
		//	EnumeratedItem val = new EnumeratedItem();

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		Codes_Language item = context.Codes_Language
		//			.FirstOrDefault( s => s.Id == languageId );
		//		if ( item != null && item.Id > 0 )
		//		{
		//			val.Id = item.Id;
		//			val.Value = item.LangugeCode;
		//			val.Name = item.LanguageName;
		//			val.Description = item.LanguageName;
		//			val.SortOrder = item.SortOrder != null ? ( int ) item.SortOrder : 0;
		//			val.SchemaName = item.LangugeCode;
		//		}
		//	}

		//	return val;
		//}
		public static int GetLanguageId( string language )
		{
			EnumeratedItem val = new EnumeratedItem();
			if ( language.ToLower().StartsWith( "eng" ) )
			{
				language = "english";
			}
			int languageId = 0;
			using ( var context = new EM.CTIEntities() )
			{
				Codes_Language item = context.Codes_Language
					.FirstOrDefault( s => s.LanguageName.ToLower() == language.ToLower()
						|| s.LangugeCode.ToLower() == language.ToLower()
					);

				if ( item != null && item.Id > 0 )
				{
					languageId = item.Id;
					val.Id = item.Id;
					val.Value = item.LangugeCode;
					val.Name = item.LanguageName;
					val.Description = item.LanguageName;
					val.SortOrder = item.SortOrder != null ? ( int ) item.SortOrder : 0;
					val.SchemaName = item.LangugeCode;
				}
			}

			return languageId;
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
			keyword = ( keyword ?? "" ).Trim();
			if ( pageSize == 0 )
				pageSize = 100;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;
			string notKeyword = "Except " + keyword;

			using ( var context = new EM.CTIEntities() )
			{
				List<ONET_SOC> results = context.ONET_SOC
					.Where( s => ( headerId == 0 || s.OnetSocCode.Substring( 0, 2 ) == headerId.ToString() )
						&& ( keyword == ""
						|| s.OnetSocCode.Contains( keyword )
						|| s.SOC_code.Contains( keyword )
						|| ( s.Title.Contains( keyword ) && s.Title.Contains( notKeyword ) == false )
						)
						&& ( s.Totals > 0 || getAll )
						)
					.OrderBy( s => s.Title )
					.Skip( skip )
					.Take( pageSize )
					.ToList();

				totalRows = context.ONET_SOC
					.Where( s => ( headerId == 0 || s.OnetSocCode.Substring( 0, 2 ) == headerId.ToString() )
						&& ( keyword == ""
						|| s.OnetSocCode.Contains( keyword )
						|| s.SOC_code.Contains( keyword )
						|| s.Title.Contains( keyword ) )
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
						if ( !string.IsNullOrEmpty( item.SOC_code ) )
							entity.Name = string.Format( "{0} ({1})", item.Title, item.SOC_code );
						entity.Description = item.Description;
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

		public static List<CodeItem> SOC_Search( string code )
		{
			List<string> codes = new List<string>() { code };
			return SOC_Search( codes );
		}
		public static List<CodeItem> SOC_Search( List<string> codes )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();

			using ( var context = new EM.CTIEntities() )
			{
				List<ONET_SOC> results = ( from p in context.ONET_SOC
										   where codes.Any( w => p.OnetSocCode.Equals( w )
												 || p.SOC_code.Equals( w.Replace( ".00", "" ).Replace( "-", "" ) )
												 )
										   select p ).ToList();


				if ( results != null && results.Count > 0 )
				{
					foreach ( ONET_SOC item in results )
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Name = item.Title;// +" ( " + item.OnetSocCode + " )";
						entity.Description = item.Description;
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
		public static List<CodeItem> SOC_Search( string[] codes )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();

			using ( var context = new EM.CTIEntities() )
			{
				List<ONET_SOC> results = ( from p in context.ONET_SOC
										   where codes.Any( w => p.OnetSocCode.Contains( w )
												 || p.SOC_code.Contains( w.Replace( ".00", "" ).Replace( "-", "" ) )
												 )
										   select p ).ToList();


				if ( results != null && results.Count > 0 )
				{
					foreach ( ONET_SOC item in results )
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Name = item.Title;// +" ( " + item.OnetSocCode + " )";
						entity.Description = item.Description;
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
						entity.Description = " ( " + item.OnetSocCode + " )" + item.Title;
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
				}
				else
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
						code.Totals = ( int ) ( item.Totals ?? 0 );
						code.CategorySchema = "ctdl:SocGroup";
						list.Add( code );
					}
				}
			}
			return list;
		}

		public static List<CodeItem> SOC_CategoriesInUse( string sortField = "Description" )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem code;

			using ( var context = new Views.CTIEntities1() )
			{
				var results = context.Entity_FrameworkOccupationGroupSummary
							.OrderBy( x => x.FrameworkGroupTitle )
							.ToList();

				//var Query = from P in context.Entity_FrameworkOccupationGroupSummary
				//			select P;

				//if ( sortField == "JobFamilyId" )
				//{
				//	Query = Query.OrderBy( p => p.CodeGroup );
				//}
				//else
				//{
				//	Query = Query.OrderBy( p => p.FrameworkGroupTitle );
				//}
				//var count = Query.Count();
				//var results = Query.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Views.Entity_FrameworkOccupationGroupSummary item in results )
					{
						code = new CodeItem();
						code.Id = ( int ) item.CodeGroup;
						code.Title = item.FrameworkGroupTitle;
						code.Totals = ( int ) ( item.groupCount ?? 0 );
						code.CategorySchema = "ctdl:SocGroup";
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
			string notKeyword = "Except " + keyword;

			using ( var context = new EM.CTIEntities() )
			{
				List<NAICS> results = context.NAICS
						.Where( s => ( headerId == 0 || s.NaicsCode.Substring( 0, 2 ) == headerId.ToString() )
						&& ( keyword == ""
						|| s.NaicsCode.Contains( keyword )
						|| s.NaicsTitle.Contains( keyword ) )
						&& ( s.Totals > 0 || getAll )
						)
					.OrderBy( s => s.NaicsTitle )
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
						entity.Name = item.NaicsTitle;
						if ( !string.IsNullOrEmpty( item.NaicsCode ) )
							entity.Name = string.Format( "{0} ({1})", item.NaicsTitle, item.NaicsCode ); // + " ( " + item.NaicsCode + " )";
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
		public static List<CodeItem> NAICS_SearchInUse( int entityTypeId, int headerId, string keyword, int pageNumber, int pageSize, ref int totalRows )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();
			keyword = keyword.Trim();
			if ( pageSize == 0 )
				pageSize = 100;
			int skip = 0;
			if ( pageNumber > 1 )
				skip = ( pageNumber - 1 ) * pageSize;
			string notKeyword = "Except " + keyword;

			using ( var context = new Views.CTIEntities1() )
			{
				List<Views.Entity_FrameworkIndustryCodeSummary> results = context.Entity_FrameworkIndustryCodeSummary
						.Where( s => ( headerId == 0 || s.CodeGroup == headerId )
						&& ( s.EntityTypeId == entityTypeId )
						&& ( keyword == ""
						|| s.NaicsCode.Contains( keyword )
						|| s.NaicsTitle.Contains( keyword ) )
						&& ( s.Totals > 0 )
						)
					.OrderBy( s => s.NaicsTitle )
					.Skip( skip )
					.Take( pageSize )
					.ToList();
				totalRows = context.Entity_FrameworkIndustryCodeSummary
						.Where( s => ( headerId == 0 || s.CodeGroup == headerId )
						&& ( s.EntityTypeId == entityTypeId )
						&& ( keyword == ""
						|| s.NaicsCode.Contains( keyword )
						|| s.NaicsTitle.Contains( keyword ) )
						&& ( s.Totals > 0 )
						)
					.ToList().Count();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Views.Entity_FrameworkIndustryCodeSummary item in results )
					{
						entity = new CodeItem();
						entity.Id = ( int ) item.Id;
						entity.Name = item.NaicsTitle;// + " ( " + item.NaicsCode + " )";
						entity.Description = "";// 						item.NaicsTitle + " ( " + item.NaicsCode + " )";
						entity.URL = item.URL;
						entity.SchemaName = item.NaicsCode;
						entity.Code = item.CodeGroup.ToString();
						entity.Totals = item.Totals ?? 0;

						list.Add( entity );
					}
				}
			}

			return list;
		}

		public static List<CodeItem> NAICS_Search( string code )
		{
			List<string> codes = new List<string>() { code };
			return NAICS_Search( codes );
		}
		public static List<CodeItem> NAICS_Search( List<string> codes )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();

			using ( var context = new EM.CTIEntities() )
			{
				List<NAICS> results = ( from p in context.NAICS
										where codes.Any( w => p.NaicsCode == w )
										select p ).ToList();


				if ( results != null && results.Count > 0 )
				{
					foreach ( NAICS item in results )
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Name = item.NaicsTitle;
						entity.Description = item.Description;
						entity.SchemaName = item.NaicsCode;
						entity.URL = item.URL;
						entity.Code = item.NAICS_NaicsGroup.ToString();
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
						entity.Totals = ( int ) ( item.Totals ?? 0 );
						entity.CategorySchema = "ctdl:NaicsGroup";

						list.Add( entity );
					}
				}
			}

			return list;
		}
		public static List<CodeItem> NAICS_CategoriesInUse( int entityTypeId )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem code;
			//, string sortField = "Description"
			using ( var context = new Views.CTIEntities1() )
			{
				//var Query = from P in context.Entity_FrameworkIndustryGroupSummary
				//			.Where( a => a.EntityTypeId == entityTypeId )
				//			select P;

				//if ( sortField == "codeId" )
				//{
				//	Query = Query.OrderBy( p => p.CodeGroup );
				//}
				//else
				//{
				//	Query = Query.OrderBy( p => p.FrameworkGroupTitle );
				//}
				//var count = Query.Count();
				//var results = Query.ToList();

				List<Views.Entity_FrameworkIndustryGroupSummary> results = context.Entity_FrameworkIndustryGroupSummary
							.Where( s => s.EntityTypeId == entityTypeId )
							.OrderBy( x => x.FrameworkGroupTitle )
							.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Views.Entity_FrameworkIndustryGroupSummary item in results )
					{
						code = new CodeItem();
						code.Id = ( int ) item.CodeGroup;
						code.Title = item.FrameworkGroupTitle;
						code.Totals = ( int ) ( item.groupCount ?? 0 );
						code.CategorySchema = "ctdl:IndustryGroup";
						list.Add( code );
					}
				}
			}
			return list;
		}
		#endregion


		#region CIPS
		//helper to search for a single code using exiting search
		public static List<CodeItem> CIP_Search( string code )
		{
			List<string> codes = new List<string>() { code };
			return CIP_Search( codes );
		}
		public static List<CodeItem> CIP_Search( List<string> codes )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem entity = new CodeItem();

			using ( var context = new EM.CTIEntities() )
			{
				//was going to add this option: || p.CIPCode.Equals( w.Replace( ".", "" ) ), 
				//but decided to skip, as could get unexpected results, then decided to leave for now
				List<CIPCode2010> results = ( from p in context.CIPCode2010
											  where codes.Any( w => p.CIPCode.Equals( w ) || p.CIPCode.Equals( w.Replace( ".", "" ) ) )
											  select p ).ToList();


				if ( results != null && results.Count > 0 )
				{
					foreach ( var item in results )
					{
						entity = new CodeItem
						{
							Id = item.Id,
							Name = item.CIPTitle,
							Description = item.CIPDefinition,
							//entity.URL = item.URL;
							SchemaName = item.CIPCode,
							Code = item.CIPFamily.ToString(),
							Totals = item.Totals ?? 0
						};

						list.Add( entity );
					}
				}
			}

			return list;
		}

		public static List<CodeItem> CIPS_Search( int headerId, string keyword, int pageNumber, int pageSize, ref int totalRows, bool getAll = true )
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
						|| s.CIPTitle.Contains( keyword )
						)
						&& ( s.Totals > 0 || getAll )
						)
					.OrderBy( s => s.CIPTitle )
					.Skip( skip )
					.Take( pageSize )
					.ToList();

				totalRows = context.CIPCode2010
						.Where( s => ( headerId == 0 || s.CIPCode.Substring( 0, 2 ) == header )
						&& ( keyword == ""
						|| s.CIPCode.Contains( keyword )
						|| s.CIPTitle.Contains( keyword ) )
						&& ( s.Totals > 0 || getAll )
						)
					.ToList().Count();

				if ( results != null && results.Count > 0 )
				{
					foreach ( CIPCode2010 item in results )
					{
						entity = new CodeItem();
						entity.Id = item.Id;
						entity.Name = item.CIPTitle + " ( " + item.CIPCode + " )";
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
		public static List<CodeItem> CIPS_SearchInUse( int entityTypeId, int headerId, string keyword, int pageNumber, int pageSize, ref int totalRows )
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

			using ( var context = new Views.CTIEntities1() )
			{
				List<Views.Entity_FrameworkCIPCodeSummary> results = context.Entity_FrameworkCIPCodeSummary
						.Where( s => ( headerId == 0 || s.CodeGroup == header )
						&& ( s.EntityTypeId == entityTypeId )
						&& ( keyword == ""
						|| s.CIPCode.Contains( keyword )
						|| s.CIPTitle.Contains( keyword )
						)
						&& ( s.Totals > 0 )
						)
					.OrderBy( s => s.CIPTitle )
					.Skip( skip )
					.Take( pageSize )
					.ToList();

				totalRows = context.Entity_FrameworkCIPCodeSummary
						.Where( s => ( headerId == 0 || s.CodeGroup == header )
						&& ( s.EntityTypeId == entityTypeId )
						&& ( keyword == ""
						|| s.CIPCode.Contains( keyword )
						|| s.CIPTitle.Contains( keyword )
						)
						&& ( s.Totals > 0 )
						)
					.ToList().Count();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Views.Entity_FrameworkCIPCodeSummary item in results )
					{
						entity = new CodeItem();
						entity.Id = ( int ) item.Id;
						entity.Name = item.CIPTitle + " ( " + item.CIPCode + " )";
						//entity.Description = item.CIPDefinition;
						entity.URL = item.URL;
						entity.SchemaName = item.CIPCode;
						entity.Code = item.CodeGroup;
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

						entity.Totals = ( int ) ( item.Totals ?? 0 );
						entity.CategorySchema = "ctdl:CipsGroup";
						list.Add( entity );
					}
				}
			}

			return list;
		}

		public static List<CodeItem> CIPS_CategoriesInUse( int entityTypeId, string sortField = "codeId" )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem code;

			using ( var context = new Views.CTIEntities1() )
			{
				var Query = from P in context.Entity_FrameworkCIPGroupSummary
							.Where( a => a.EntityTypeId == entityTypeId )
							select P;

				if ( sortField == "codeId" )
				{
					Query = Query.OrderBy( p => p.CodeGroup );
				}
				else
				{
					Query = Query.OrderBy( p => p.FrameworkGroupTitle );
				}
				var count = Query.Count();
				var results = Query.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Views.Entity_FrameworkCIPGroupSummary item in results )
					{
						code = new CodeItem();
						//???
						code.Id = Int32.Parse( item.CodeGroup );
						code.Code = item.CodeGroup;
						code.Title = item.FrameworkGroupTitle;
						code.Totals = ( int ) ( item.groupCount ?? 0 );
						code.CategorySchema = "ctdl:CIP";
						list.Add( code );
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


		#region Reporting 

		/// <summary>
		/// Get Properties Summary with totals
		/// </summary>
		/// <param name="categoryId">If zero, will return all</param>
		/// <returns></returns>
		public static List<CodeItem> Property_GetSummaryTotals( int categoryId = 0 )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem code;

			using ( var context = new ViewContext() )
			{
				List<Views.CodesProperty_Summary> results = context.CodesProperty_Summary
					.Where( s => s.CategoryId == categoryId || categoryId == 0 )
							.OrderBy( s => s.Category )
							.ThenBy( s => s.SortOrder )
							.ThenBy( s => s.Property )
							.ToList();

				if ( results != null && results.Count > 0 )
				{

					foreach ( Views.CodesProperty_Summary item in results )
					{
						code = new CodeItem();
						code.Id = ( int ) item.PropertyId;
						code.Title = item.Property;
						code.SchemaName = item.PropertySchemaName;
						//note this is used as a hack on some properties
						code.ParentSchemaName = item.ParentSchemaName;
						code.URL = item.PropertySchemaUrl;

						code.Description = item.PropertyDescription;
						code.CategoryId = item.CategoryId;
						code.Category = item.Category;
						code.CategorySchema = item.CategorySchemaName;
						code.Totals = item.Totals;
						code.SortOrder = item.SortOrder ?? 25;
						list.Add( code );
					}
				}
			}
			return list;
		}

		public static List<CodeItem> Property_GetTotalsByEntity( int categoryId = 0 )
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem code;

			using ( var context = new ViewContext() )
			{
                List<Views.CodesProperty_Counts_ByEntity> results = context.CodesProperty_Counts_ByEntity
                    .Where( s => s.CategoryId == categoryId || categoryId == 0 )
							.OrderBy( s => s.Entity )
							.ThenBy( s => s.Category )
							.ThenBy( s => s.SortOrder )
							.ThenBy( s => s.Property )
							.ToList();
				//
				if ( results != null && results.Count > 0 )
				{

                    foreach ( Views.CodesProperty_Counts_ByEntity item in results )
                    {
						code = new CodeItem();
						code.EntityType = item.Entity;
						code.EntityTypeId = item.EntityTypeId;
						code.Id = ( int ) item.PropertyId;
						code.Title = item.Property;
						code.SchemaName = item.SchemaName;
						//note this is used as a hack on some properties
						code.ParentSchemaName = item.CategorySchema;
						//code.URL = item.PropertySchemaUrl;

						code.Description = item.Description;
						code.CategoryId = item.CategoryId;
						code.Category = item.Category;
						code.CategorySchema = item.CategorySchema;
						code.Totals = ( int ) item.EntityPropertyCount;

						list.Add( code );
					}
				}
			}
			return list;
		}
       
        public static string CodeEntity_GetEntityType( int entityTypeId )
		{
			string entityType = "";
			using ( var context = new Data.CTIEntities() )
			{
				var item = context.Codes_EntityType
					.FirstOrDefault( s => s.Id == entityTypeId );

				if ( item != null && item.Id > 0 )
				{
					entityType = item.Title;
				}

			}
			return entityType;
		}
		/// <summary>
		/// Get Entity Codes with totals for Credential, Organization, assessments, and learning opp
		/// </summary>
		/// <returns></returns>
		public static List<CodeItem> CodeEntity_GetMainClassTotals()
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem code;

			using ( var context = new Data.CTIEntities() )
			{
				List<EM.Codes_EntityType> results = context.Codes_EntityType
					.Where( s => s.Id < 4 || s.Id == 7 )
							.OrderBy( s => s.Id )
							.ToList();

				if ( results != null && results.Count > 0 )
				{

					foreach ( EM.Codes_EntityType item in results )
					{
						code = new CodeItem();
						code.Id = item.Id;
						code.Title = item.Title;
						code.Totals = ( int ) item.Totals;

						code.Description = item.Description;
						list.Add( code );
					}
				}
				//add QA orgs, and others
				//
				code = new CodeItem();
				code.Id = 99;
				code.Title = "QA Organization";
				code.Totals = OrganizationManager.QAOrgCounts();
				list.Add( code );

			}
			return list;
		}

		#region Counts.SiteTotals
		public static Enumeration GetSiteTotalsAsEnumeration( int categoryId, int entityTypeId, bool getAll = true )
		{
			using ( var context = new EntityContext() )
			{
				Codes_PropertyCategory category = context.Codes_PropertyCategory
							.FirstOrDefault( s => s.Id == categoryId && s.IsActive == true );
				return FillSiteTotalsAsEnumeration( category, entityTypeId, getAll, false );
			}
		}
		private static Enumeration FillSiteTotalsAsEnumeration( Codes_PropertyCategory category, int entityTypeId, bool getAll, bool onlySubType1 )
		{
			Enumeration entity = new Enumeration();
			using ( var context = new EntityContext() )
			{
				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.Description = category.Description;

					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();


					var results = context.Counts_SiteTotals
							.Where( s => s.CategoryId == category.Id && s.EntityTypeId == entityTypeId
								 && ( getAll || s.Totals > 0 ) )
							.OrderBy( p => p.Description )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						EnumeratedItem val = new EnumeratedItem();
						foreach ( var item in results )
						{
							val = new EnumeratedItem();
							val.Id = item.CodeId ?? 0;
							val.CodeId = item.CodeId ?? 0;
							val.ParentId = category.Id;
							val.Name = item.Description;

							if ( category.Id == 65 )
							{
								val.Value = item.Description;
							}
							else
							{
								val.Value = item.CodeId.ToString();
							}
							val.Totals = item.Totals ?? 0;
							if ( IsDevEnv() )
								val.Name += string.Format( " ({0})", val.Totals );
							entity.Items.Add( val );
						}
					}
				}
			}

			return entity;
		}
		#endregion
		public static List<CodeItem> CodeEntity_GetCountsSiteTotals()
		{
			List<CodeItem> list = new List<CodeItem>();
			CodeItem code;

			using ( var context = new Data.CTIEntities() )
			{
				List<EM.Counts_SiteTotals> results = context.Counts_SiteTotals
							.OrderBy( s => s.CategoryId )
							.ThenBy( x => x.EntityTypeId )
							.ThenBy( y => y.CodeId )
							.ToList();

				if ( results != null && results.Count > 0 )
				{

					foreach ( EM.Counts_SiteTotals item in results )
					{
						code = new CodeItem();
						code.Id = item.Id;
						code.CategoryId = item.CategoryId;
						//?? - need entity type for filtering
						code.EntityTypeId = item.EntityTypeId;
                        //code.EntityType = item.EntityTypeId.ToString();

                        code.Code = item.CodeId.ToString();
						code.Title = item.Description;
						code.Totals = ( int ) item.Totals;

						code.Description = item.Description;
						list.Add( code );
					}
				}


			}
			return list;
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
							.FirstOrDefault( s => s.Title.ToLower() == name.ToLower() 
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
							.FirstOrDefault( s => s.Id == categoryId && s.IsActive == true );
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

	}
}
