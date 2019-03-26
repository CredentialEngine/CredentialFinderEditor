using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Models.Input
{
	/// <summary>
	/// Duration Profile
	/// Either enter an ExactDuration or a range using Minimum duration, and maximum duration
	/// </summary>
	public class DurationProfile 
	{
		public DurationProfile()
		{
			MinimumDuration = new DurationItem();
			MaximumDuration = new DurationItem();
			ExactDuration = new DurationItem();
		}

		/// <summary>
		/// Description of this duration profile - optional
		/// </summary>
		public string Description { get; set; }
		public DurationItem MinimumDuration { get; set; }
		public DurationItem MaximumDuration { get; set; }
		public DurationItem ExactDuration { get; set; }

	}
	//

	/// <summary>
	/// Enter either the Duration_ISO8601 value, or the necessary combination of years, months, weeks, etc
	/// </summary>
	public class DurationItem
	{
		public string Duration_ISO8601 { get; set; }
		public int Years { get; set; }
		public int Months { get; set; }
		public int Weeks { get; set; }
		public int Days { get; set; }
		public int Hours { get; set; }
		public int Minutes { get; set; }

	}
}
