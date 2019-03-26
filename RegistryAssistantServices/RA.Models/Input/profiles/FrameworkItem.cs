using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Models.Input
{
	/// <summary>
	/// Coded Framework
	/// Examples
	/// SOC - occupations
	/// NAICS - industries
	/// CIP
	/// </summary>
	public class FrameworkItem
	{
		public string Framework { get; set; }
		public string FrameworkName { get; set; }

		public string CodedNotation { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		

		public string URL { get; set; }
	}
}
