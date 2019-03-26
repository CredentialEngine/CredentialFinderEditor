using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Models.Input
{
	public class CredentialAlignmentObject
	{
		//public string AlignmentType { get; set; }
		public string CodedNotation { get; set; }
		/// <summary>
		/// URL
		/// </summary>
		public string Framework { get; set; }
		public string FrameworkName { get; set; }
		/// <summary>
		/// URL
		/// </summary>
		public string TargetNode { get; set; }
		public string TargetNodeDescription { get; set; }
		public string TargetNodeName { get; set; }

		public decimal Weight { get; set; }
	}
}
