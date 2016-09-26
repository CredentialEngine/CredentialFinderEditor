using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Search
{
	public class MainSearchInput
	{
		public MainSearchInput()
		{
			SearchType = "credential";
			StartPage = 1;
			PageSize = 20;
			Keywords = "";
			SortOrder = "relevance";
			Filters = new List<MainSearchFilter>();
		}

		public string SearchType { get; set; }
		public int StartPage { get; set; }
		public int PageSize { get; set; }
		public string Keywords { get; set; }
		public string CompetenciesKeywords { get; set; }
		public string SortOrder { get; set; }
		public List<MainSearchFilter> Filters { get; set; }

		public List<string> GetFilterValues_Strings( string name )
		{
			try
			{
				return Filters.FirstOrDefault( m => m.Name.ToLower() == name.ToLower() ).Items ?? new List<string>();
			}
			catch
			{
				return new List<string>();
			}
		}
		public List<int> GetFilterValues_Ints( string name )
		{
			try
			{
				return GetFilterValues_Strings( name ).Select( int.Parse ).ToList();
			}
			catch
			{
				return new List<int>();
			}
		}
		public List<string> GetFilterValues_Strings( int categoryID )
		{
			try
			{
				return Filters.FirstOrDefault( m => m.CategoryId == categoryID ).Items ?? new List<string>();
			}
			catch
			{
				return new List<string>();
			}
		}
		public List<int> GetFilterValues_Ints( int categoryID )
		{
			try
			{
				return GetFilterValues_Strings( categoryID ).Select( int.Parse ).ToList();
			}
			catch
			{
				return new List<int>();
			}
		}
	}
	//



	public class MainSearchFilter
	{
		public MainSearchFilter()
		{
			Name = "";
			Items = new List<string>();
			Boundaries = new Common.BoundingBox();
		}
		public string Name { get; set; }
		public int CategoryId { get; set; }
		public List<string> Items { get; set; }
		public Common.BoundingBox Boundaries { get; set; }
	}
	//

	public class MainSearchResults
	{
		public MainSearchResults()
		{
			TotalResults = 0;
			Results = new List<MainSearchResult>();
		}

		public string SearchType { get; set; }
		public int TotalResults { get; set; }
		public List<MainSearchResult> Results { get; set; }
	}
	//

	public class MainSearchResult
	{
		public MainSearchResult()
		{
			Properties = new Dictionary<string, object>();
		}
		public string Name { get; set; }
		public string Description { get; set; }
		public int RecordId { get; set; }
		public Dictionary<string, object> Properties { get; set; }
	}
	//

	//Used by EnumerationFilter partial
	public class HtmlEnumerationFilterSettings
	{
		public HtmlEnumerationFilterSettings()
		{
			CssClasses = new List<string>();
			Enumeration = new Common.Enumeration();
			PreselectedFilters = new Dictionary<int, List<int>>();
		}

		public List<string> CssClasses { get; set; }
		public string SearchType { get; set; }
		public string FilterName { get; set; }
		public int CategoryId { get; set; }
		public Models.Common.Enumeration Enumeration { get; set; }
		public Dictionary<int, List<int>> PreselectedFilters { get; set; }
		public string Guidance { get; set; }
	}
	//

	//Used for micro searches that are used as filters on the search page
	public class MicroSearchFilterSettings
	{
		public MicroSearchFilterSettings()
		{
			InputTitle = "";
			SelectedTitle = "";
			PageSize = 5;
			IncludeKeywords = true;
			ParentSearchType = "";
			FilterName = "";
			MicroSearchType = "";
			Filters = new List<MicroSearchSettings_FilterV2>();
			PreselectedFilters = new Dictionary<int, List<int>>();
		}

		public string InputTitle { get; set; }
		public string SelectedTitle { get; set; }
		public bool IncludeKeywords { get; set; }
		public int PageSize { get; set; }
		public string ParentSearchType { get; set; }
		public string FilterName { get; set; }
		public int CategoryId { get; set; }
		public string MicroSearchType { get; set; }
		public List<MicroSearchSettings_FilterV2> Filters { get; set; }
		public Dictionary<int, List<int>> PreselectedFilters { get; set; }
		public string Guidance { get; set; }
	}
	//

	public class TextFilterSettings
	{
		public TextFilterSettings()
		{
			InputTitle = "";
			Guidance = "";
			SearchType = "";
			FilterName = "";
			Fields = new List<string>();
			Placeholder = "Search...";
		}

		public string InputTitle { get; set; }
		public string TagTitle { get; set; }
		public string Guidance { get; set; }
		public string SearchType { get; set; }
		public string FilterName { get; set; }
		public string CategoryId { get; set; }
		public List<string> Fields { get; set; }
		public string Placeholder { get; set; }
	}
	//

}
