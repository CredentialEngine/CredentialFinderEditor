using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Factories;
using Models;
using Models.Helpers.Reports;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CTIServices
{
	public class ReportServices
	{
		public static CommonTotals SiteTotals()
		{
			CommonTotals totals = ActivityManager.SiteTotals_Get();
			totals.MainEntityTotals = ReportServices.MainEntityTotals();
			

			//vm.TotalDirectCredentials = list.FirstOrDefault( x => x.Id == 1 ).Totals;
			//vm.TotalOrganizations = list.FirstOrDefault( x => x.Id == 2 ).Totals;
			//vm.TotalQAOrganizations = list.FirstOrDefault( x => x.Id == 99 ).Totals;
			totals.AgentServiceTypes = OrganizationServiceManager.GetOrgServices();

			totals.PropertiesTotals = ReportServices.PropertyTotals();
			totals.PropertiesTotalsByEntity = CodesManager.Property_GetTotalsByEntity();

			totals.SOC_Groups = CodesManager.SOC_Categories();
			//totals.NAICs_Groups = CodesManager.NAICS_Categories();
			//totals.CIP_Groups = CodesManager.CIPS_Categories();
			totals.PropertiesTotals.AddRange( CodesManager.SOC_Categories() );
			totals.PropertiesTotals.AddRange( CodesManager.NAICS_Categories() );
			totals.PropertiesTotals.AddRange( CodesManager.CIPS_Categories() );

			return totals;
		}

		/// <summary>
		/// Get Entity Codes with totals for Credential, Organization, assessments, and learning opp
		/// </summary>
		/// <returns></returns>
		public static List<CodeItem> MainEntityTotals()
		{
			List<CodeItem> list = CodesManager.CodeEntity_GetMainClassTotals();

			return list;
		}

		/// <summary>
		/// Get property totals, by category or all active properties
		/// </summary>
		/// <param name="categoryId"></param>
		/// <returns></returns>
		public static List<CodeItem> PropertyTotals( int categoryId = 0)
		{
			List<CodeItem> list = CodesManager.Property_GetSummaryTotals( categoryId );

			return list;
		}

		/// <summary>
		/// Get totals for a single organization - used by the accounts system for reports
		/// </summary>
		/// <param name="organizationCTID"></param>
		/// <returns></returns>
		public static OrganizationStatistics GetOrganizationStatistics( string organizationCTID )
		{
			var result = new OrganizationStatistics();

			//TODO: Get the data

			return result;
		}
		//

		public static bool UpdateOrganizationStatisticsInAccountsSystem( string organizationCTID )
		{
			var data = GetOrganizationStatistics( organizationCTID );
			var password = Utilities.ConfigHelper.GetConfigValue( "CEAccountSystemStaticPassword", "" );
			var url = Utilities.ConfigHelper.GetConfigValue( "CEAccountOrganizationStatisticsUpdateApi", "" );
			var client = new HttpClient();
			var wrapper = new { password = password, data = data };
			var requestContent = new StringContent( JsonConvert.SerializeObject( wrapper ), Encoding.UTF8, "application/json" );
			var result = client.PostAsync( url, requestContent ).Result;
			var body = result.Content.ReadAsStringAsync().Result; //Should be the standard JsonResponse structure (data, valid, status, extra)
			var response = JObject.Parse( body ); //May want to return the whole response for error messages?
			return (bool) response[ "valid" ];
		}
		//
	}
}
