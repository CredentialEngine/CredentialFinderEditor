using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{
	public class VerificationServiceProfile : BaseProfile
	{
		public VerificationServiceProfile()
		{
			EstimatedCost = new List<CostProfile>();
			RelevantCredential = new Credential();
			ClaimType = new Enumeration();
			//VerificationStatus = new List<VerificationStatus>();
		}

		public string SubjectWebpage { get; set; }
		public string VerificationServiceUrl { get; set; }
		public List<TextValueProfile> Auto_VerificationService
		{
			get
			{
				return Auto_Helper( VerificationServiceUrl );
				//var result = new List<TextValueProfile>();
				//if ( !string.IsNullOrWhiteSpace( VerificationServiceUrl ) )
				//{
				//	result.Add( new TextValueProfile() { TextValue = VerificationServiceUrl } );
				//}
				//return result;
			}
		}
		public bool? HolderMustAuthorize { get; set; }
		public List<CostProfile> EstimatedCost { get; set; }
		public List<CostProfile> EstimatedCosts { get { return EstimatedCost; } set { EstimatedCost = value; } } //Convenience for publishing


		//note the credential will now be the context entity
		public string TargetCredential { get; set; } //url
		public int TargetCredentialId {
			get { return RelevantCredential.Id; }
			set { RelevantCredential.Id = value; }
		}
		public Credential RelevantCredential { get; set; } //Workaround

		public Enumeration ClaimType { get; set; }


		public Guid OfferedByAgentUid { get; set; }
		public string VerificationDirectory { get; set; }
		public List<TextValueProfile> Auto_VerificationDirectory
		{
			get
			{
				//return Auto_Helper( VerificationDirectory );
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( VerificationDirectory ) )
				{
					result.Add( new TextValueProfile() { TextValue = VerificationDirectory } );
				}
				return result;
			}
		}
		public string VerificationMethodDescription { get; set; }

		public List<JurisdictionProfile> Region { get; set; }
		public List<JurisdictionProfile> JurisdictionAssertions { get; set; }

		//public List<VerificationStatus> VerificationStatus { get; set; }
	}


}
