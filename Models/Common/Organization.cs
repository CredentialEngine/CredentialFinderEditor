using System;
using System.Collections.Generic;
using System.Linq;
using Models.ProfileModels;

namespace Models.Common
{
    [Serializable]
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

			OrganizationRole_QAPerformed = new List<OrganizationRoleProfile>();
			OrganizationRole_Recipient = new List<OrganizationRoleProfile>();

			//QualityAssuranceAction = new List<QualityAssuranceActionProfile>();
			//QualityAssuranceActor = new List<QualityAssuranceActionProfile>();
			Identifiers = new Enumeration();
			VerificationServiceProfiles = new List<VerificationServiceProfile>();

            CredentialAssertions = new List<OrganizationAssertion>();
            OrganizationAssertions = new List<OrganizationAssertion>();
            AssessmentAssertions = new List<OrganizationAssertion>();
            LoppAssertions = new List<OrganizationAssertion>();

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
			AlternativeIndustries = new List<TextValueProfile>();

			HasConditionManifest = new List<ConditionManifest>();
			HasCostManifest = new List<CostManifest>();
			OrganizationThirdPartyAssertions = new List<OrganizationThirdPartyAssertion>();
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

		public Enumeration ServiceType { get; set; }
		//public List<int> ServiceTypeIds { get; set; }
		public string ServiceTypeOther { get; set; }
		
		public bool IsThirdPartyOrganization { get; set; }
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

        /// <summary>
        /// Should only be one parent, but using list for consistancy
        /// </summary>
        public List<OrganizationRoleProfile> ParentOrganizations { get; set; } = new List<OrganizationRoleProfile>();
		public List<TextValueProfile> Auto_OrganizationRole_ParentOrganizations
		{
			get
			{
				return ParentOrganizations.ConvertAll( m => new TextValueProfile() { TextValue = m.ActingAgent.CTID } );
			}
		}
        public List<OrganizationRoleProfile> OrganizationRole_Dept { get; set; } = new List<OrganizationRoleProfile>();
		public List<TextValueProfile> Auto_OrganizationRole_Dept { get
			{
				return OrganizationRole_Dept.ConvertAll( m => new TextValueProfile() { TextValue = m.ActingAgent.CTID } );
			} }
		public List<OrganizationRoleProfile> OrganizationRole_Subsidiary { get; set; } = new List<OrganizationRoleProfile>();
        public List<TextValueProfile> Auto_OrganizationRole_SubOrganization { get
			{
				return OrganizationRole_Subsidiary.ConvertAll( m => new TextValueProfile() { TextValue = m.ActingAgent.CTID } );
			} }

        #region quality assurance performed
        /// <summary>
        /// roles where org is the actor, ie Accredits something
        /// These should be roles where added from organization not from, say a credential.
        /// </summary>
        public List<OrganizationRoleProfile> OrganizationRole_QAPerformed { get; set; }
        public List<OrganizationRoleProfile> OrganizationRole_Actor
        {
            get { return OrganizationRole_QAPerformed; }
        }
        /// <summary>
        /// Entity assertions by other organizations referencing, for example approved by, accredited by
        /// </summary>
        public List<OrganizationThirdPartyAssertion> OrganizationThirdPartyAssertions { get; set; }

        /// <summary>
        /// Entity assertions by this organization, for example approves, accredits
        /// </summary>
        public List<OrganizationAssertion> OrganizationFirstPartyAssertions { get; set; } = new List<OrganizationAssertion>();
        public List<OrganizationAssertion> CredentialAssertions { get; set; }
        public List<OrganizationAssertion> OrganizationAssertions { get; set; }
        public List<OrganizationAssertion> AssessmentAssertions { get; set; }
        public List<OrganizationAssertion> LoppAssertions { get; set; }
        #endregion

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


		#region  HasPart
		/// <summary>
		/// Created credentials, should be ownedBy relationships
		/// </summary>
		public List<Credential> CreatedCredentials { get; set; }
		//may need an auto_property for publishing, that will get all owned credentials
		public List<TextValueProfile> Owns_Auto_Organization_OwnsCredentials
		{
			get
			{
				//to specific, need to handle without CTID
				return CreatedCredentials.ConvertAll( m => new TextValueProfile() { TextValue = m.OwningOrganization.CTID } );
			}
		}

        public List<Credential> OfferedCredentials { get; set; } = new List<Credential>();
        public List<AssessmentProfile> OwnedAssessments { get; set; } = new List<AssessmentProfile>();
        public List<LearningOpportunityProfile> OwnedLearningOpportunities { get; set; } = new List<LearningOpportunityProfile>();

        public bool OwnsOrOffersCredentials
        {
            get
            {
                if ( CreatedCredentials.Count > 0 || OfferedCredentials.Count > 0 )
                    return true;
                else
                    return false;
            }
        }
        public bool ChildOrganizationsHaveCredentials { get; set; }
		
	
		#endregion

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
		/// <summary>
		/// Concat Industry and OtherIndustry
		/// </summary>
		public Enumeration IndustryType
		{
			get
			{
				return new Enumeration()
				{
					Items = new List<EnumeratedItem>()
					.Concat( Industry.Items )
					.Concat( AlternativeIndustries.ConvertAll( m => new EnumeratedItem() { Name = m.TextTitle, Description = m.TextValue } ) ).ToList()
				};
			}
			set { Industry = value; }
		} //Used for publishing
		public List<TextValueProfile> AlternativeIndustries { get; set; }

        public List<int> InLanguageIds
        { get
            {
                if ( InLanguageCodeList == null || InLanguageCodeList.Count == 0 )
                    return new List<int>();

                return InLanguageCodeList.Select( x => x.LanguageCodeId ).ToList();
            }
        }
        public List<LanguageProfile> InLanguageCodeList { get; set; }

        //QA =====================================
        public List<ProcessProfile> AppealProcess { get; set; }
		public List<ProcessProfile> ComplaintProcess { get; set; }
		public List<ProcessProfile> ReviewProcess { get; set; }
		public List<ProcessProfile> RevocationProcess { get; set; }

		public List<ProcessProfile> AdministrationProcess { get; set; }
		public List<ProcessProfile> DevelopmentProcess { get; set; }
		public List<ProcessProfile> MaintenanceProcess { get; set; }

		public List<VerificationStatus> VerificationStatus { get; set; }

  //      public bool HasMinimumData (ref string message)
		//{
  //          /*
  //           * 18-10-01 mp - replaced by OrganizationManager.HasMinimumData
		//	 * Required
  //              ● Name – the name of the resource being described.
  //              ● Description – a short description of the resource being described.
  //              ● Organization Type – enumerations of organization types. Examples include, but are not limited to, terms representing educational institutions, governmental bodies, credentialing and assurance bodies, and labor unions.
  //              ● Organization Sector Type – sectors include public, private for profit, public for profit, and business industry association.
  //              ● Contact Information – a means of contacting a resource or its representative(s). May include physical address, email address, and phone number.
  //              ● Subject Webpage – the web page where the resource being described is located.
  //              ● CTID – a globally unique credential transparency identifier; the equivalent of a version identifier for the
  //              resource.
		//	 * 
		//	 */

  //          bool isValid = true;
		//	message = "";
		//	if ( string.IsNullOrWhiteSpace( Name ) )
		//		message += "Organization Name is required<br/>";
		//	if ( string.IsNullOrWhiteSpace( Description ) )
		//		message += "Organization Description is required<br/>";
		//	if ( string.IsNullOrWhiteSpace( CTID ) || ctid.Length !=39 )
		//		message += "A valid CTID (ce-UUID) is required<br/>";
		//	if ( string.IsNullOrWhiteSpace( SubjectWebpage ) )
		//		message += "Organization Subject Webpage is required<br/>";
		//	if ( OrganizationType  == null || OrganizationType.HasItems() ==false)
		//		message += "At least one Organization Type is required<br/>";
		//	if ( OrganizationSectorType == null || OrganizationSectorType.HasItems() == false )
		//		message += "At least one Organization Sector Type is required<br/>";

		//	//check for at least one email, or phone number or contact point, or address
		//	if ((Emails == null || Emails.Count == 0) &&
		//		( PhoneNumbers == null || PhoneNumbers.Count == 0 ) &&
		//		( Addresses == null || Addresses.Count == 0 ) &&
		//		( ContactPoint == null || ContactPoint.Count == 0 )
		//		)
		//		message += "At least one type of contact property, like  physical address, contact point, email address, or phone number. is required<br/>";

  //          //need to check offered by as well!
  //          if (!ISQAOrganization 
  //              && (CreatedCredentials == null || CreatedCredentials.Count == 0)
  //              && ( OfferedCredentials == null || OfferedCredentials.Count == 0 )
  //              )
  //          {
  //              //ensure only done with minimum data checks
  //              message += "A credentialing organization must 'own' or 'offer' at least one credential in order to be published to the Credential Registry.</br> ";
  //          }
		//	if ( message.Length > 0 )
		//		isValid = false;

		//	return isValid;
		//}
    } //

    public class OrganizationThirdPartyAssertion 
    {
        public int Id { get; set; }
        public int RelationshipTypeId { get; set; }
        public string Relationship { get; set; }
        public string Name { get; set; }
        public string CTID { get; set; }
        public string SubjectWebpage { get; set; }
        public string Description { get; set; }
        public int EntityTypeId { get; set; }
        public string EntityType { get; set; }
        public string CtdlType { get; set; }
        public bool IsReferenceVersion { get; set; }

        //public string AssetType {
        //	get
        //	{
        //		if ( EntityTypeId == 0)
        //			return "";
        //		else if ( EntityTypeId == 2 )
        //			return "ceterms:CredentialOrganization";
        //		else if ( EntityTypeId == 3)
        //			return "ceterms:AssessmentProfile";
        //		else if ( EntityTypeId == 7 )
        //			return "ceterms:LearningOpportunityProfile";
        //		else if ( EntityTypeId == 1 )
        //		{
        //			//need to get actually credential type schema
        //		}

        //	}
        //}
    } //
}
