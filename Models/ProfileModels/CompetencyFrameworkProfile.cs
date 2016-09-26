using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ProfileModels
{
	public class CompetencyFrameworkProfile : BaseProfile
	{
		//public CompetencyFrameworkProfile()
		//{
		//	Name = "";
		//	CompetencyProfiles= new List<ProfileModels.CompetencyProfile>();
		//	Url = "";
		//	Description = "";
		//}

		public string Name { get; set; }
		public string Url { get; set; }

		public List<CompetencyProfile> CompetencyProfiles { get; set; }
	}
}
