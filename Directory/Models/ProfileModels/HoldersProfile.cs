using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{

	public class HoldersProfile : BaseProfile
	{
		public string DemographicInformation { get; set; }
		public int NumberAwarded { get; set; }
		public string SourceUrl { get; set; }
	}
	//

}
