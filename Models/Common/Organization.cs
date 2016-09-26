using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.ProfileModels;

namespace Models.Common
{

	public class Organization : Agent
	{
		public Organization()
		{
			AgentType = "Organization";
			AgentTypeId = 1;

			//Address = new Address(); //see Agent
			OrganizationType = new Enumeration();
			ServiceType = new Enumeration();
			OrganizationSectorType = new Enumeration();
			OrganizationRole_Dept = new List<OrganizationRoleProfile>();
			OrganizationRole_Subsiduary = new List<OrganizationRoleProfile>();

			OrganizationRole_Actor = new List<OrganizationRoleProfile>();
			OrganizationRole_Recipient = new List<OrganizationRoleProfile>();

			QualityAssuranceAction = new List<QualityAssuranceActionProfile>();
			Identifiers = new Enumeration();
			Authentication = new List<AuthenticationProfile>();
			

			CreatedCredentials = new List<Credential>();
			IsAQAOrg = false;
			IsACredentialingOrg = false;
			FoundingDate = "";
			FoundingYear = "";
			FoundingMonth = "";
			FoundingDay = "";
		}
		//public int Id { get; set; }

		//public string Name { get; set; }
		public int StatusId { get; set; }
		//public string Description { get; set; }
		//public string Url { get; set; }
		//public string UniqueURI { get; set; }
		//public string ImageUrl { get; set; }
		//public string Email { get; set; }
		//public string MainPhoneNumber { get; set; }
		//public string FaxNumber { get; set; }
		//public string TTYNumber { get; set; }
		//public string TollFreeNumber { get; set; }
		//public DateTime FoundingDateOld { get; set; }
		public string FoundingDate { get; set; }
		public string FoundingYear { get; set; }
		public string FoundingMonth { get; set; }
		public string FoundingDay { get; set; }
		public string Founded {
			get { return string.IsNullOrWhiteSpace( this.FoundingDate ) ? GetListSpaced( this.FoundingDay ) + GetListSpaced( this.FoundingMonth ) + GetListSpaced( this.FoundingYear ) : this.FoundingDate; } 
			set { this.FoundingDate = value; } 
		}

		//public Address Address { get; set; }
		//OrganizationType is saved as an OrganizationProperty
		public Enumeration OrganizationType { get; set; }
		public Enumeration OrganizationSectorType { get; set; }
		//public List<int> OrganizationTypeIds { get; set; }
		public Enumeration ServiceType { get; set; }
		//public List<int> ServiceTypeIds { get; set; }
		public string ServiceTypeOther { get; set; }

		public Enumeration QAPurposeType { get; set; }
		
		public bool IsACredentialingOrg { get; set; }
		public bool IsAQAOrg { get; set; }


		//Added to agent
		//public List<TextValueProfile> Phones { get; set; }
		//added to Agent
		//public List<TextValueProfile> Emails { get; set; }
		

		//public List<OrganizationRoleProfile> OrganizationRole { get; set; }
		public List<OrganizationRoleProfile> OrganizationRole_Dept { get; set; }
		public List<OrganizationRoleProfile> OrganizationRole_Subsiduary{ get; set; }
		public List<OrganizationRoleProfile> OrganizationRole_Actor { get; set; }
		public List<OrganizationRoleProfile> OrganizationRole_Recipient { get; set; }
		public List<OrganizationRoleProfile> OrganizationRole {
			get { return OrganizationRole_Recipient; }
			set { OrganizationRole_Recipient = value; } 
		}
		public List<QualityAssuranceActionProfile> QualityAssuranceAction { get; set; }
		//Identifiers is saved as an OrganizationProperty
		public Enumeration Identifiers { get; set; }
		public List<AuthenticationProfile> Authentication { get; set; }

		public List<Credential> CreatedCredentials { get; set; }

		public string Purpose { get; set; }
		public List<JurisdictionProfile> Jurisdiction { get; set; }
		public List<ProcessProfile> AgentProcess { get; set; }

		private static string GetListSpaced(string input)
		{
			return string.IsNullOrWhiteSpace( input ) ? "" : input + " ";
		}
	}

}
