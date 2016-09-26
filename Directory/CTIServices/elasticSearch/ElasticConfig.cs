using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nest;
using Utilities;

namespace CTIServices.elasticSearch
{
	public static class ElasticConfig
	{
		public static string IndexName
		{
			get { return UtilityManager.GetAppKeyValue("indexName"); }
		}

		public static string ElasticSearchUrl
		{
			get { return UtilityManager.GetAppKeyValue("elasticSearchUrl"); }
		}

		public static IElasticClient GetClient()
		{
			var node = new Uri(ElasticSearchUrl);
			var settings = new ConnectionSettings(node);
			settings.DefaultIndex(IndexName);
			return new ElasticClient(settings);
		}
	}
}
