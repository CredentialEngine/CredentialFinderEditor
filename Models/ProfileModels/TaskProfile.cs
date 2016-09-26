using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.Common;

namespace Models.ProfileModels
{
	public class TaskProfile : BaseProfile
	{
		public TaskProfile()
		{
			EstimatedCost = new List<CostProfile>();
			AffiliatedAgent = new Organization();
			Url = "";
			//IsNewVersion = false;
		}
		//public bool IsNewVersion { get; set; }
		public string Url { get; set; }
		public Guid AffiliatedAgentUid { get; set; }
		public List<CostProfile> EstimatedCost { get; set; }
		public List<DurationProfile> EstimatedDuration { get; set; }
		public Organization AffiliatedAgent { get; set; }

	}
	//
}
