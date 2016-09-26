using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.ProfileModels;

namespace Models.Common
{

	public class CredentialAlignmentObjectProfile : BaseProfile
	{
		public string AlignmentDate { get; set; }

		private string _alignmentType = "";
		public string AlignmentType {
			get { return _alignmentType; }
			set
			{
				if ( value.ToLower() == "teachescompetencies" )
					value = "Teaches";
				else if ( value.ToLower() == "requirescompetencies" )
					value = "Requires";
				else if ( value.ToLower() == "assessescompetencies" )
					value = "Assesses";
				else
				{
					//let it go
				}
				_alignmentType = value;
			} 
		}
		public string AssertedBy { get; set; }
		public string EducationalFramework { get; set; }
		public string TargetDescription { get; set; }
		public string TargetName { get; set; }
		public string TargetUrl { get; set; }
		public string Description { get; set; }
		public string Name { get; set; }
		public string CodedNotation { get; set; }
	}
	//

}
