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
			//RelevantCredential = new Credential();
			ClaimType = new Enumeration();
			TargetCredential = new List<Credential>();
			//VerificationStatus = new List<VerificationStatus>();
			OfferedBy = new Organization();
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

		public List<Credential> TargetCredential { get; set; }

		//note the credential will now be the context entity
		//public string TargetCredential { get; set; } //url
		public int TargetCredentialId {
			get { return RelevantCredential.Id; }
			set { RelevantCredential.Id = value; }
		}
		//Workaround
		public Credential RelevantCredential { get; set; } = new Credential();

		public Enumeration ClaimType { get; set; }


		public Organization OfferedBy { get; set; } 
		public Guid OfferedByAgentUid { get; set; }
		public List<TextValueProfile> Auto_OfferedBy
		{
			get
			{
				var result = new List<TextValueProfile>();
				if ( OfferedBy == null
					|| OfferedBy.Id == 0
					|| ( OfferedBy.CTID ?? "" ).Length != 39 )
					return result;

				if ( !string.IsNullOrWhiteSpace( OfferedBy.CTID ) && OfferedBy.CTID.IndexOf( "00000000-" ) == -1 )
				{
					result.Add( new TextValueProfile() { TextValue = Utilities.GetWebConfigValue( "credRegistryResourceUrl" ) + OfferedBy.CTID } );
				}
				return result;
			}
		}
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
