using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ProfileModels
{
	public class CompetencyProfile : BaseProfile
	{
		//public CompetencyProfile()
		//{
		//	Name = "";
		//	CodeValue = "";
		//	Url = "";
		//}

		public int CompetencyFrameworkId { get; set; }
		public string Name
		{
			get { return this.ProfileName; }
			set { this.ProfileName = value; }
		}
		public string Url { get; set; }
		public string CodeValue { get; set; }
	
	
	}
}
