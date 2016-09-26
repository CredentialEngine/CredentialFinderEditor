using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ProfileModels
{

	public class EarningsProfile : BaseProfile
	{
		public double LowEarnings { get; set; }
		public double MedianEarnings { get; set; }
		public double HighEarnings { get; set; }
		public int PostReceiptMonths { get; set; }
		public string SourceUrl { get; set; }
	}
	//

}
