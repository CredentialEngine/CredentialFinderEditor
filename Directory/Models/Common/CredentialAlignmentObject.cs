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

		public int AlignmentTypeId { get; set; }

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
		//public string AssertedBy { get; set; }
		public string EducationalFramework { get; set; }
		public string TargetDescription { get; set; }
		public string TargetName { get; set; }
		public string TargetUrl { get; set; }
		public string Name { get; set; }
		public string CodedNotation { get; set; }
	}
	//

	/* Split Profiles */
	public class CredentialAlignmentObjectFrameworkProfile : BaseProfile
	{
		public CredentialAlignmentObjectFrameworkProfile()
		{
			Items = new List<CredentialAlignmentObjectItemProfile>();
		}
		//public string AlignmentDate { get; set; }
		public int AlignmentTypeId { get; set; }
		private string _alignmentType = "";
		public string AlignmentType
		{
			get { return _alignmentType; }
			set
			{
				if ( value.ToLower() == "teachescompetencies" 
					|| value.ToLower() == "teachescompetenciesframeworks" )
					value = "Teaches";
				else if ( value.ToLower() == "requirescompetencies"
					|| value.ToLower() == "requirescompetenciesframeworks" )
					value = "Requires";
				else if ( value.ToLower() == "assessescompetencies"
					|| value.ToLower() == "assessescompetenciesframeworks" )
					value = "Assesses";
				else
				{
					//let it go
				}
				_alignmentType = value;
			}
		}
		//public string AssertedBy { get; set; }
		public string EducationalFrameworkName { get; set; }
		public string EducationalFrameworkUrl { get; set; }
		public List<CredentialAlignmentObjectItemProfile> Items { get; set; }
	}
	//
	public class CredentialAlignmentObjectItemProfile : BaseProfile
	{
		public string TargetDescription { get; set; }
		public string TargetName { get; set; }
		public string TargetUrl { get; set; }
		public string Name { get; set; }
		public string CodedNotation { get; set; }

		public string AlignmentDate { get; set; }
	}
	//

}
