using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.Helpers
{

	public class CompareItem
	{
		public string Type { get; set; }
		public int Id { get; set; }
		public string Title { get; set; }
	}
	//

	public class CompareItemSummary
	{
		public CompareItemSummary()
		{
			Credentials = new List<Credential>();
			Organizations = new List<Organization>();
		}
		public List<Credential> Credentials { get; set; }
		public List<Organization> Organizations { get; set; }
	}
	//

}
