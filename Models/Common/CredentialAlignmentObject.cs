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
				if ( value.ToLower().Contains( "teaches" ) )
					value = "Teaches";
				else if ( value.ToLower().Contains( "requires" ) )
					value = "Requires";
				else if ( value.ToLower().Contains( "assesses" ) )
					value = "Assesses";
				else
				{
					//let it go
				}
				_alignmentType = value;
			} 
		}
		//public string AssertedBy { get; set; }
		public string EducationalFramework { get; set; } //Is this a framework name or a framework URL? Both "framework" (a URL) and "frameworkName" (a string) exist in CTDL.
		public string TargetDescription { get; set; }
		public string TargetName { get; set; }
		public string TargetUrl { get; set; }
		public string Name { get; set; } //No longer exists in this class in CTDL and should not be used
		public string CodedNotation { get; set; }
		public List<TextValueProfile> Auto_CodedNotation {
			get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( CodedNotation ) )
				{
					result.Add( new TextValueProfile() { TextValue = CodedNotation } );
				}
				return result;
			}
			set
			{
				CodedNotation = value.FirstOrDefault().TextValue;
			}
		}

		//More modern versions of the above properties
		public string FrameworkUrl { get; set; }
		public string FrameworkName { get; set; }
		public string TargetNodeName { get { return ( string.IsNullOrWhiteSpace( TargetName ) ? Name : TargetName); } set { TargetName = value; } }
		public string TargetNodeDescription { get { return ( string.IsNullOrWhiteSpace( TargetDescription ) ? Description : TargetDescription); } set { TargetDescription = value; } }
		public string TargetNode { get { return TargetUrl; } set { TargetUrl = value; } }
	}
	//
	public class CredentialAlignmentObject : CredentialAlignmentObjectProfile
	{
	}
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
				if ( value.ToLower().Contains( "teaches" ) )
					value = "Teaches";
				else if ( value.ToLower().Contains( "requires" ) )
					value = "Requires";
				else if ( value.ToLower().Contains( "assesses" ) )
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

		public static List<CredentialAlignmentObjectProfile> FlattenAlignmentObjects( List<CredentialAlignmentObjectFrameworkProfile> data )
		{
			var result = new List<CredentialAlignmentObjectProfile>();

			foreach ( var framework in data )
			{
				foreach( var item in framework.Items )
				{
					result.Add( new CredentialAlignmentObjectProfile()
					{
						AlignmentType = framework.AlignmentType,
						AlignmentTypeId = framework.AlignmentTypeId,
						EducationalFramework = string.IsNullOrWhiteSpace( framework.EducationalFrameworkUrl ) ? framework.EducationalFrameworkName : framework.EducationalFrameworkUrl,
						FrameworkName = framework.EducationalFrameworkName,
						FrameworkUrl = framework.EducationalFrameworkUrl,
						TargetName = item.TargetName,
						TargetDescription = item.TargetDescription,
						//TargetUrl = string.IsNullOrWhiteSpace( item.TargetUrl ) ? framework.EducationalFrameworkUrl : item.TargetUrl,
						TargetUrl = item.TargetUrl,
						Name = item.Name,
						Description = item.Description,
						CodedNotation = item.CodedNotation
					} );
				}
			}

			return result;
		}
		//

		public static void ExpandAlignmentObjects( List<CredentialAlignmentObjectProfile> data, List<CredentialAlignmentObjectFrameworkProfile> target, string filter )
		{
			target = target ?? new List<CredentialAlignmentObjectFrameworkProfile>();
			target.Clear();
			foreach ( var item in data.Where( m => m.AlignmentType.ToLower().Contains( filter ) ).ToList() )
			{
				var currentFramework = target.FirstOrDefault( m => m.EducationalFrameworkName == item.EducationalFramework || m.EducationalFrameworkUrl == item.EducationalFramework );
				if ( currentFramework == null )
				{
					currentFramework = new CredentialAlignmentObjectFrameworkProfile()
					{
						AlignmentType = item.AlignmentType,
						AlignmentTypeId = item.AlignmentTypeId,
						EducationalFrameworkName = item.EducationalFramework.Contains( "http" ) ? "" : item.EducationalFramework,
						EducationalFrameworkUrl = item.EducationalFramework.Contains( "http" ) ? item.EducationalFramework : ""
					};
					target.Add( currentFramework );
				}
				currentFramework.Items.Add( new CredentialAlignmentObjectItemProfile()
				{
					TargetDescription = item.TargetDescription,
					TargetName = item.TargetName,
					TargetUrl = item.TargetUrl,
					Name = item.Name,
					CodedNotation = item.CodedNotation,
					AlignmentDate = item.AlignmentDate
				} );
			}
		}
		//

	}
	//
	public class CredentialAlignmentObjectItemProfile : BaseProfile
	{
		public string Name { get; set; }
		public string TargetDescription { get; set; }
		public string TargetName { get; set; }
		public string TargetUrl { get; set; }
		
		
		public string CodedNotation { get; set; }

		public string AlignmentDate { get; set; }


		//primarily available from a search

		public int ConnectionTypeId { get; set; }
		public int SourceEntityTypeId { get; set; }
		public int SourceParentId { get; set; }

		public int AlignmentTypeId { get; set; }
		public string AlignmentType { get; set; }

		//for use with CASS comps, initially
		public int CompetencyId { get; set; }
		public string RepositoryUri { get; set; }
	}
	//

}
