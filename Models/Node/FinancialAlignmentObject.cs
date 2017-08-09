using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Models.Node
{


	[Profile( DBType = typeof( Models.Common.FinancialAlignmentObject ) )]
	public class FinancialAlignmentObject : BaseProfile
	{
		public FinancialAlignmentObject()
		{
		}

		public string AlignmentDate { get; set; }

		public int AlignmentTypeId { get; set; }

		public string AlignmentType { get; set; }

		/// <summary>
		/// Framework URL
		/// The framework to which the resource being described is aligned.
		/// </summary>
		public string Framework { get; set; }

		/// <summary>
		/// The name of the framework to which the resource being described is aligned.
		/// Frameworks may include, but are not limited to, competency frameworks and concept schemes such as industry, occupation, and instructional program codes.
		/// </summary>
		public string FrameworkName { get; set; }

		/// <summary>
		/// Target Node - URI
		/// The node of a framework targeted by the alignment.
		/// </summary>
		public string TargetNode { get; set; }

		/// <summary>
		/// The description of a node in an established educational framework.
		/// </summary>
		public string TargetNodeDescription { get; set; }

		/// <summary>
		/// The name of a node in an established educational framework.
		/// The name of the competency or concept targeted by the alignment.
		/// </summary>
		public string TargetNodeName { get; set; }

		/// <summary>
		/// An asserted measurement of the weight, degree, percent, or strength of a recommendation, requirement, or comparison.
		/// </summary>
		public decimal Weight { get; set; }

		/// <summary>
		/// Coded Notation
		/// A short set of alpha-numeric symbols that uniquely identifies a resource and supports its discovery.
		/// </summary>
		public string CodedNotation { get; set; }



	}
}
