using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Data;

namespace CTIServices
{
	public class SiteActivity : ActivityLog
	{
		public SiteActivity()
		{
			this.ActivityType = "Audit";
			this.CreatedDate = DateTime.Now;
		}
	}
}
