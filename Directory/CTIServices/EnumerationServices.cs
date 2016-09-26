using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Models;
using MC = Models.Common;
using Factories;

namespace CTIServices
{
	public class EnumerationServices
	{
		/// <summary>
		/// Get an MC.Enumeration (by default a checkbox list) by schemaName
		/// </summary>
		/// <param name="dataSource"></param
		/// <param name="interfaceType"></param>
		/// <param name="showOtherValue">If true, a text box for entering other values will be displayed</param>
		/// <returns></returns>
		public MC.Enumeration GetEnumeration( string dataSource, MC.EnumerationType
				interfaceType = MC.EnumerationType.MULTI_SELECT, 
				bool showOtherValue = true, 
				bool getAll = true)
		{
			MC.Enumeration e = CodesManager.GetEnumeration( dataSource, getAll );
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = showOtherValue;
			return e;
		}

		/// <summary>
		/// Get a list of properties - typically called from Views
		/// </summary>
		/// <param name="dataSource"></param>
		/// <param name="getAll"></param>
		/// <returns></returns>
		public List<CodeItem> GetPropertiesList( string dataSource, bool getAll = true )
		{
			bool insertSelectTitle = false;
			List<CodeItem> list = CodesManager.Property_GetValues( dataSource, insertSelectTitle, getAll );

			return list;
		}
		public List<CodeItem> GetPropertiesList( string dataSource, bool insertSelectTitle, bool getAll = true )
		{
			List<CodeItem> list = CodesManager.Property_GetValues( dataSource, insertSelectTitle, getAll );

			return list;
		}

		#region credential enumerations
		public MC.Enumeration GetCredentialType( MC.EnumerationType interfaceType, bool getAll = true )
		{

			MC.Enumeration e = CodesManager.GetEnumeration( "credentialType", getAll );
			e.ShowOtherValue = true;
			e.InterfaceType = interfaceType;
			return e;
		}
		//
		public MC.Enumeration GetEducationCredentialType( MC.EnumerationType interfaceType, bool getAll = true )
		{

			MC.Enumeration e = CodesManager.GetEnumeration( "credentialType", getAll, true );
			e.ShowOtherValue = true;
			e.InterfaceType = interfaceType;
			return e;
		}
		//
		public MC.Enumeration GetCredentialPurpose( MC.EnumerationType interfaceType, bool getAll = true )
		{
			MC.Enumeration e = CodesManager.GetEnumeration( "purpose", getAll );
			e.ShowOtherValue = true;
			e.InterfaceType = interfaceType;
			return e;
		}
		//

		public MC.Enumeration GetCredentialLevel( MC.EnumerationType interfaceType, bool getAll = true )
		{
			MC.Enumeration e = CodesManager.GetEnumeration( "credentialLevel", getAll );
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = true;
			return e;
		}
		//
		#endregion

		#region agent role enums
		public MC.Enumeration GetCredentialAllAgentRoles( MC.EnumerationType interfaceType )
		{
			MC.Enumeration e = OrganizationRoleManager.GetCredentialOrg_AllRoles( false );
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = true;
			return e;
		}

		/// <summary>
		/// OLD
		/// </summary>
		/// <param name="interfaceType"></param>
		/// <returns></returns>
		public MC.Enumeration GetAllAgentRoles( MC.EnumerationType interfaceType )
		{
			MC.Enumeration e = OrganizationRoleManager.GetAgentToAgentRolesCodes( true );
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = true;
			return e;
		}
		public MC.Enumeration GetAllOrgAgentRoles( MC.EnumerationType interfaceType )
		{
			MC.Enumeration e = OrganizationRoleManager.GetAgentToAgentRolesCodes( true );
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = true;
			return e;
		}

		/// <summary>
		/// Get agent roles for assessments and learning opportunities
		/// Ex: Created By
		/// </summary>
		/// <param name="interfaceType"></param>
		/// <returns></returns>
		public MC.Enumeration GetAssessmentAgentRoles( MC.EnumerationType interfaceType )
		{
			MC.Enumeration e = Entity_AgentRelationshipManager.GetAllOtherAgentRoles( false );
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = true;
			return e;
		}
		public MC.Enumeration GetLearningOppAgentRoles( MC.EnumerationType interfaceType )
		{
			MC.Enumeration e = Entity_AgentRelationshipManager.GetLearningOppAgentRoles( false );
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = true;
			return e;
		}
		/// <summary>
		/// Get INVERSE agent roles for assessments and learning opportunities
		/// Ex: Created
		/// </summary>
		/// <param name="interfaceType"></param>
		/// <returns></returns>
		public MC.Enumeration GetAllOtherAgentRolesInverse( MC.EnumerationType interfaceType )
		{
			MC.Enumeration e = Entity_AgentRelationshipManager.GetAllOtherAgentRoles( true );
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = true;
			return e;
		}
	
		/// <summary>
		/// Get agent roles for assessment and learning opps
		/// </summary>
		/// <param name="interfaceType"></param>
		/// <returns></returns>
		//public MC.Enumeration GetAllLearningOppAgentRoles( MC.EnumerationType interfaceType )
		//{
		//	MC.Enumeration e = Entity_AgentRelationshipManager.GetAllOtherAgentRoles( true );
		//	e.InterfaceType = interfaceType;
		//	e.ShowOtherValue = true;
		//	return e;
		//}
		public MC.Enumeration GetAllAgentReverseRoles( MC.EnumerationType interfaceType )
		{
			MC.Enumeration e = OrganizationRoleManager.GetAgentToAgentRolesCodes( false );
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = true;
			return e;
		}
		/// <summary>
		/// Get only QA roles
		/// </summary>
		/// <param name="interfaceType"></param>
		/// <param name="entityType"></param>
		/// <returns></returns>
		public MC.Enumeration GetCredentialAgentQAActions( MC.EnumerationType interfaceType, string entityType = "Credential", bool getAll = true )
		{
			//get roles as entity to org
			MC.Enumeration e = OrganizationRoleManager.GetEntityAgentQAActions( false, getAll, entityType );
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = true;
			return e;
		}
		public MC.Enumeration GetEntityAgentQAActionFilters( MC.EnumerationType interfaceType, string entityType, bool getAll = true )
		{
			//get roles as entity to org
			MC.Enumeration e = OrganizationRoleManager.GetEntityAgentQAActionFilters( false, getAll, entityType );
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = false;
			return e;
		}
		public MC.Enumeration GetCredentialAgentRoles( MC.EnumerationType interfaceType, string entityType = "Credential" )
		{
			//get roles as entity to org
			MC.Enumeration e = new MC.Enumeration();
			if (Utilities.UtilityManager.GetAppKeyValue("includingAllRolesForOrgRoles", false))
				e = OrganizationRoleManager.GetCredentialOrg_AllRoles( false, entityType );
			else 
				e = OrganizationRoleManager.GetCredentialOrg_NonQARoles( false, entityType );
			
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = true;
			return e;
		}
		#endregion

		#region org enumurations
		public MC.Enumeration GetOrganizationType( MC.EnumerationType interfaceType,
				bool getAll = true )
		{

			MC.Enumeration e = CodesManager.GetEnumeration( "organizationType", getAll );
			e.ShowOtherValue = true;
			e.InterfaceType = interfaceType;
			return e;
		}
		//
		public static MC.Enumeration GetOrganizationIdentifier( MC.EnumerationType interfaceType, bool getAll = true )
		{

			MC.Enumeration e = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS, getAll );
			e.ShowOtherValue = true;
			e.InterfaceType = interfaceType;
			return e;
		}
		//
		/// <summary>
		/// Get candidate list of services for an org
		/// </summary>
		/// <param name="interfaceType"></param>
		/// <returns></returns>
		public MC.Enumeration GetOrganizationServices( MC.EnumerationType interfaceType,
				bool getAll = true )
		{
			//MC.Enumeration e = OrganizationRoleManager.GetCredentialOrgRoles( true, false, "" );
			MC.Enumeration e = OrganizationServiceManager.GetOrgServices( getAll );
			e.InterfaceType = interfaceType;
			e.ShowOtherValue = true;
			return e;
		}
	
		//
		public List<CodeItem> GetOrganizationsAsCodes(bool insertSelectTitle = false)
		{
			
			List<CodeItem> list = OrganizationManager.Organization_SelectAllAsCodes( insertSelectTitle );

			return list;
		}
		public MC.Enumeration GetOrganizations( string schemaName, bool includeDefaultOption, bool forCurrentUser, MC.EnumerationType interfaceType )
		{
			var result = new MC.Enumeration()
			{
				InterfaceType = interfaceType,
				Name = "Organizations",
				SchemaName = schemaName
			};

			AppUser user = AccountServices.GetCurrentUser();
			if ( ( user == null | user.Id == 0 ) )
				return result;
			
			if ( includeDefaultOption )
			{
				result.Items.Add( new MC.EnumeratedItem()
				{
					Id = 0,
					RowId = "",
					Name = "Select an Organization",
					Value = ""
				} );
			}

			//May need some overload to only get orgs for current user
			var organizations = OrganizationServices.OrganizationsForCredentials_Select(user.Id);
			foreach ( var org in organizations )
			{
				result.Items.Add( new MC.EnumeratedItem()
				{
					Id = org.Id,
					RowId = org.RowId.ToString(),
					Name = org.Name,
					Value = org.Url
				} );
			}

			return result;
		}
		//
		#endregion

		#region //Temporary
		//Get a sample enumeration
		public MC.Enumeration GetSampleEnumeration( string dataSource, string schemaName, MC.EnumerationType interfaceType )
		{
			var result = CodesManager.GetSampleEnumeration( dataSource, schemaName );
			result.InterfaceType = interfaceType;

			return result;
		}
		//
		#endregion 		//End Temporary

		#region currencies/countries
		public List<CodeItem> GetCountries()
		{
			List<CodeItem> list = CodesManager.GetCountries_AsCodes();
			return list;
		}
	
		//GetCurrencies
		public MC.Enumeration GetCurrencies( MC.EnumerationType interfaceType )
		{
			MC.Enumeration e = CodesManager.GetCurrencies();
			e.ShowOtherValue = false;
			e.InterfaceType = interfaceType;
			return e;
		}
		#endregion 

		#region SOC
		public static List<CodeItem> SOC_Search( int headerId, string keyword, int pageNumber, int pageSize, ref int totalRows, bool getAll = true )
		{
			return CodesManager.SOC_Search( headerId, keyword, pageNumber, pageSize,  ref totalRows, getAll );
		}
		/// <summary>
		/// SOC Search
		/// TODO - need to include the parentId, so search will not return items already selected
		/// </summary>
		/// <param name="keyword"></param>
		/// <param name="maxTerms"></param>
		/// <returns></returns>
		//public static List<CodeItem> SOC_Search( int headerId = 0, string keyword = "", int pageNumber = 1, int maxRows = 25 )
		//{
		//	int totalRows = 0;
		//	return CodesManager.SOC_Search( headerId, keyword, pageNumber, maxRows, ref totalRows );
		//}
		public static List<CodeItem> SOC_Autocomplete( int credentialId, int headerId = 0, string keyword = "", int maxRows = 25 )
		{
			return CodesManager.SOC_Autocomplete( headerId, keyword, maxRows );
		}
		public static List<CodeItem> SOC_Categories()
		{
			return CodesManager.SOC_Categories();
		}
		public static MC.Enumeration SOC_Categories_Enumeration( bool getAll = true)
		{
			var data = CodesManager.SOC_Categories();
			var result = new MC.Enumeration()
			{
				Id = 11,
				Name = "Standard Occupation Codes (SOC)",
				Items = ConvertCodeItemsToEnumeratedItems( data )
			};
			return result;
		}
		#endregion
		#region NAICS
		public static List<CodeItem> NAICS_Search( int headerId, string keyword, int pageNumber, int pageSize, ref int totalRows, bool getAll = true )
		{
			//int totalRows = 0;
			return CodesManager.NAICS_Search( headerId, keyword, pageNumber, pageSize, getAll, ref totalRows );
		}
		public static List<CodeItem> NAICS_Autocomplete( int credentialId, int headerId = 0, string keyword = "", int maxRows = 25 )
		{
			return CodesManager.NAICS_Autocomplete( headerId, keyword, maxRows );
		}
		public static List<CodeItem> NAICS_Categories()
		{
			return CodesManager.NAICS_Categories();
		}
		public static MC.Enumeration NAICS_Categories_Enumeration( bool getAll = true)
		{
			var data = CodesManager.NAICS_Categories();
			var result = new MC.Enumeration()
			{
				Id = 10,
				Name = "North American Industry Classification System (NAICS)",
				Items = ConvertCodeItemsToEnumeratedItems( data )
			};
			return result;
		}
		#endregion
		#region CIPS
		public static List<CodeItem> CIPS_Search( int headerId = 0, string keyword = "", int pageNumber = 1, int maxRows = 25 )
		{
			int totalRows = 0;
			return CodesManager.CIPS_Search( headerId, keyword, pageNumber, maxRows, ref totalRows );
		}
		public static List<CodeItem> CIPS_Autocomplete( int credentialId, int headerId = 0, string keyword = "", int maxRows = 25 )
		{
			return CodesManager.CIPS_Autocomplete( headerId, keyword, maxRows );
		}
		public static List<CodeItem> CIPS_Categories()
		{
			return CodesManager.CIPS_Categories();
		}
		public static MC.Enumeration CIPS_Categories_Enumeration()
		{
			var data = CodesManager.CIPS_Categories();
			var result = new MC.Enumeration()
			{
				Id = 23,
				Name = "Classification of Instructional Programs (CIP)",
				Items = ConvertCodeItemsToEnumeratedItems( data )
			};
			return result;
		}
		#endregion
		#region Competency framework
		//public static MC.Enumeration CompetencyFrameworks()
		//{
		//	//return CodesManager.CompetencyFrameworks_GetAll();
		//	var data = CodesManager.CompetencyFrameworks_GetAll();
		//	var result = new MC.Enumeration()
		//	{
		//		Id = 11,
		//		Name = "Competency Frameworks",
		//		Items = ConvertCodeItemsToEnumeratedItems( data )
		//	};
		//	return result;
		//}
		#endregion
		#region Helpers
		public static List<MC.EnumeratedItem> ConvertCodeItemsToEnumeratedItems( List<CodeItem> input )
		{
			var output = new List<MC.EnumeratedItem>();

			foreach ( var item in input )
			{
				output.Add( new MC.EnumeratedItem()
				{
					CodeId = item.Id,
					Id = item.Id,
					Value = item.Id.ToString(),
					Name = item.Name,
					Description = item.Description,
					SchemaName = item.SchemaName,
					SortOrder = item.SortOrder,
					URL = item.URL
				} );
			}

			return output;
		}
		#endregion
	}
}