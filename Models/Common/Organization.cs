using System.Collections.Generic;
using System.Linq;
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
			//QAPurposeType = new Enumeration();
			//QATargetType = new Enumeration();
			OrganizationSectorType = new Enumeration();
			OrganizationRole_Dept = new List<OrganizationRoleProfile>();
			OrganizationRole_Subsidiary = new List<OrganizationRoleProfile>();
			ParentOrganizations = new List<OrganizationRoleProfile>();

			OrganizationRole_Actor = new List<OrganizationRoleProfile>();
			OrganizationRole_Recipient = new List<OrganizationRoleProfile>();

			//QualityAssuranceAction = new List<QualityAssuranceActionProfile>();
			//QualityAssuranceActor = new List<QualityAssuranceActionProfile>();
			Identifiers = new Enumeration();
			VerificationServiceProfiles = new List<VerificationServiceProfile>();
			

			CreatedCredentials = new List<Credential>();
			QACredentials = new List<Credential>();
			IsAQAOrg = false;
			ISQAOrganization = false;
			IsACredentialingOrg = false;
			FoundingDate = "";
			FoundingYear = "";
			FoundingMonth = "";
			FoundingDay = "";
			JurisdictionAssertions = new List<JurisdictionProfile>();
			Jurisdiction = new List<JurisdictionProfile>();
			//QA
			AgentProcess = new List<ProcessProfile>();
            ReviewProcess = new List<ProcessProfile>();

			RevocationProcess = new List<ProcessProfile>();
			AppealProcess = new List<ProcessProfile>();
			ComplaintProcess = new List<ProcessProfile>();

			DevelopmentProcess = new List<ProcessProfile>();
			AdministrationProcess = new List<ProcessProfile>();
			MaintenanceProcess = new List<ProcessProfile>();

			VerificationStatus = new List<VerificationStatus>();

			Industry = new Enumeration();
			OtherIndustries = new List<TextValueProfile>();

			HasConditionManifest = new List<ConditionManifest>();
			HasCostManifest = new List<CostManifest>();
		}


		public int StatusId { get; set; }

		public string FoundingDate { get; set; }
		public string FoundingYear { get; set; }
		public string FoundingMonth { get; set; }
		public string FoundingDay { get; set; }
		public string Founded {
			get { return string.IsNullOrWhiteSpace( this.FoundingDate ) ? GetListSpaced( this.FoundingDay ) + GetListSpaced( this.FoundingMonth ) + GetListSpaced( this.FoundingYear ) : this.FoundingDate; } 
			set { this.FoundingDate = value; } 
		}

		
		
		//OrganizationType is saved as an OrganizationProperty
		public Enumeration OrganizationType { get; set; }
		public Enumeration OrganizationSectorType { get; set; }
		public Enumeration AgentSectorType { get { return OrganizationSectorType; } set { OrganizationSectorType = value; } } //Alias used for publishing
		//public List<int> OrganizationTypeIds { get; set; }
		public Enumeration ServiceType { get; set; }
		//public List<int> ServiceTypeIds { get; set; }
		public string ServiceTypeOther { get; set; }

		//public Enumeration QAPurposeType { get; set; }
		//public Enumeration QATargetType { get; set; }
		
		public bool IsACredentialingOrg { get; set; }

		/// <summary>
		/// TODO - should only have one QA property????
		/// </summary>
		public bool IsAQAOrg { get; set; }
		public bool ISQAOrganization { get; set; }

		public List<ConditionManifest> HasConditionManifest { get; set; }
		public List<CostManifest> HasCostManifest { get; set; }

		public string MissionAndGoalsStatement { get; set; }
		public string MissionAndGoalsStatementDescription { get; set; }
		public string AgentPurposeUrl { get; set; }
		public string AgentPurpose {  get { return AgentPurposeUrl; } set { AgentPurposeUrl = value; } } //Alias used for publishing
		//public string Versioning { get; set; }

		//Added to agent
		//public List<TextValueProfile> Phones { get; set; }
		//added to Agent
		//public List<TextValueProfile> Emails { get; set; }

			/// <summary>
			/// Should only be one parent, but using list for consistancy
			/// </summary>
		public List<OrganizationRoleProfile> ParentOrganizations { get; set; }
		public List<OrganizationRoleProfile> OrganizationRole_Dept { get; set; }
		public List<TextValueProfile> Auto_OrganizationRole_Dept { get
			{
				return OrganizationRole_Dept.ConvertAll( m => new TextValueProfile() { TextValue = m.ActingAgent.CTID } );
			} }
		public List<OrganizationRoleProfile> OrganizationRole_Subsidiary{ get; set; }
		public List<TextValueProfile> Auto_OrganizationRole_SubOrganization { get
			{
				return OrganizationRole_Subsidiary.ConvertAll( m => new TextValueProfile() { TextValue = m.ActingAgent.CTID } );
			} }

		/// <summary>
		/// roles where org is the actor, ie Accredits something
		/// </summary>
		public List<OrganizationRoleProfile> OrganizationRole_Actor { get; set; }

		/// <summary>
		/// Roles where org was acted upon - that is accrdedited by another agent
		/// </summary>
		public List<OrganizationRoleProfile> OrganizationRole_Recipient { get; set; }
		public List<OrganizationRoleProfile> OrganizationRole {
			get { return OrganizationRole_Recipient; }
			set { OrganizationRole_Recipient = value; } 
		}
		//public List<QualityAssuranceActionProfile> QualityAssuranceAction { get; set; }
		//public List<QualityAssuranceActionProfile> QualityAssuranceActor { get; set; }

		//Identifiers is saved as an OrganizationProperty
		public Enumeration Identifiers { get; set; }
		public List<VerificationServiceProfile> VerificationServiceProfiles { get; set; }

		public List<Credential> CreatedCredentials { get; set; }
		public List<Credential> QACredentials { get; set; }

		public string Purpose { get; set; }
		public string AgentPurposeDescription { get { return Purpose; } set { Purpose = value; } } //Alias used for publishing
		public List<JurisdictionProfile> Jurisdiction { get; set; }
		public List<JurisdictionProfile> JurisdictionAssertions { get; set; }
		public List<ProcessProfile> AgentProcess { get; set; }

		private static string GetListSpaced(string input)
		{
			return string.IsNullOrWhiteSpace( input ) ? "" : input + " ";
		}
		public Enumeration Industry { get; set; }
		public Enumeration IndustryType
		{
			get
			{
				return new Enumeration()
				{
					Items = new List<EnumeratedItem>()
					.Concat( Industry.Items )
					.Concat( OtherIndustries.ConvertAll( m => new EnumeratedItem() { Name = m.TextTitle, Description = m.TextValue } ) ).ToList()
				};
			}
			set { Industry = value; }
		} //Used for publishing
		public List<TextValueProfile> OtherIndustries { get; set; }

		//QA =====================================
		public List<ProcessProfile> AppealProcess { get; set; }
		public List<ProcessProfile> ComplaintProcess { get; set; }
		public List<ProcessProfile> ReviewProcess { get; set; }
		public List<ProcessProfile> RevocationProcess { get; set; }

		public List<ProcessProfile> AdministrationProcess { get; set; }
		public List<ProcessProfile> DevelopmentProcess { get; set; }
		public List<ProcessProfile> MaintenanceProcess { get; set; }

		public List<VerificationStatus> VerificationStatus { get; set; }

	}

}
