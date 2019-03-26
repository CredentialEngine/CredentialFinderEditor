using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.ProfileModels;

namespace Models.Node
{
    [Profile( DBType = typeof( Models.Common.Organization ) )]
    public class Organization : BaseMainProfile
    {
        //Basic Info
        public string FoundingDate { get; set; }
        public string FoundingYear { get; set; }
        public string FoundingMonth { get; set; }
        public string FoundingDay { get; set; }
        public string Purpose { get; set; }
        public string ImageUrl { get; set; }
        public bool ISQAOrganization { get; set; }
        public bool IsThirdPartyOrganization { get; set; }
        public string AlternativeIdentifier { get; set; }
        //SubjectWebpage was added to BaseMainProfile.
        //public string SubjectWebpage { get; set; }
        public int ManagingOrgId { get; set; }
        //public string Versioning { get; set; }
        public string MissionAndGoalsStatement { get; set; }
        public string MissionAndGoalsStatementDescription { get; set; }
        public string AgentPurposeUrl { get; set; }

        //List-based Info
        [Property( DBName = "OrganizationType", DBType = typeof( Models.Common.Enumeration ) )]
        public List<int> OrganizationTypeIds { get; set; }

        [Property( DBName = "ServiceType", DBType = typeof( Models.Common.Enumeration ) )]
        public List<int> OrganizationServiceTypeIds { get; set; }

        [Property( DBName = "OrganizationSectorType", DBType = typeof( Models.Common.Enumeration ) )]
        public int OrganizationSectorTypeId { get; set; }
        public string CredentialRegistryId { get; set; }
        public string CTID { get; set; }


        //[Property( DBName = "QAPurposeType", DBType = typeof( Models.Common.Enumeration ) )]
        //public List<int> QAPurposeTypeIds { get; set; }

        //[Property( DBName = "QATargetType", DBType = typeof( Models.Common.Enumeration ) )]
        //public List<int> QATargetTypeIds { get; set; }

        public List<TextValueProfile> AlternateName { get; set; }
        public List<TextValueProfile> Keyword { get; set; }
        //Text Value Items
        public List<TextValueProfile> SocialMediaPages { get; set; }

        public List<TextValueProfile> IdentificationCodes { get; set; } //Currently uses enumeration server-side
        public List<TextValueProfile> PhoneNumbers { get; set; }
        public List<TextValueProfile> Emails { get; set; }

        public List<int> InLanguageIds { get; set; }
        public List<LanguageProfile> InLanguageCodeList
        {
            get
            {
                var list = new List<LanguageProfile>();
                foreach ( var item in InLanguageIds )
                {
                    var newItem = new LanguageProfile { LanguageCodeId = item };
                    list.Add( newItem );
                }
                return list;
            }
        }
        //Profiles
        [Property( DBName = "Addresses", DBType = typeof( Models.Common.Address ) )]
        public List<ProfileLink> Address { get; set; }

        [Property( DBName = "ContactPoint", DBType = typeof( Models.Common.ContactPoint ) )]
        public List<ProfileLink> ContactPoint { get; set; }

        public string AvailabilityListing { get; set; }

        [Property( DBName = "OrganizationRole_Dept", DBType = typeof( AgentRoleProfile_Recipient ) )]
        public List<ProfileLink> Department { get; set; }

        [Property( DBName = "OrganizationRole_Subsidiary", DBType = typeof( AgentRoleProfile_Recipient ) )]
        public List<ProfileLink> Subsidiary { get; set; }

        [Property( DBName = "OrganizationRole_QAPerformed", DBType = typeof( AgentRoleProfile_Actor ) )]
        public List<ProfileLink> OrganizationRole_QAPerformed { get; set; }

        [Property( Type = typeof( Agent_QAPerformed_Credential ) )] 
        public List<ProfileLink> CredentialAssertions { get; set; }

        [Property( Type = typeof( Agent_QAPerformed_Organization ) )]
        public List<ProfileLink> OrganizationAssertions { get; set; }
        [Property( Type = typeof( Agent_QAPerformed_Assessment) )]
        public List<ProfileLink> AssessmentAssertions { get; set; }
        [Property( Type = typeof( Agent_QAPerformed_Lopp) )]
        public List<ProfileLink> LoppAssertions { get; set; }

        [Property( DBName = "VerificationServiceProfiles", DBType = typeof( Models.ProfileModels.VerificationServiceProfile ) )]
        public List<ProfileLink> VerificationService { get; set; }

        //Not available
        [Property( DBName = "QualityAssuranceAction", DBType = typeof( AgentRoleProfile_Actor ) )]
        public List<ProfileLink> QualityAssuranceAction_Actor { get; set; }

        [Property( Type = typeof( MicroProfile ), DBType = typeof( Models.Common.Enumeration ) )]
        public List<ProfileLink> Industry { get; set; }

        public List<TextValueProfile> AlternativeIndustries { get; set; }

        [Property( Type = typeof( Credential ) )]
        public List<ProfileLink> CreatedCredentials { get; set; }

        [Property( Type = typeof( ConditionManifest ) )]
        public List<ProfileLink> HasConditionManifest { get; set; }

        [Property( Type = typeof( CostManifest ) )]
        public List<ProfileLink> HasCostManifest { get; set; }
        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> AppealProcess { get; set; }

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> ComplaintProcess { get; set; }

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> ReviewProcess { get; set; }

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> RevocationProcess { get; set; }

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> DevelopmentProcess { get; set; }

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> AdministrationProcess { get; set; }

        [Property( Type = typeof( ProcessProfile ) )]
        public List<ProfileLink> MaintenanceProcess { get; set; }


        [Property( Type = typeof( RevocationProfile ) )]
        public List<ProfileLink> Revocation { get; set; }

        [Property( DBType = typeof( VerificationStatus ) )]
        public List<ProfileLink> VerificationStatus { get; set; }

        [Property( Type = typeof( JurisdictionProfile ) )]
        public List<ProfileLink> JurisdictionAssertions { get; set; }
    }
    //


    [Profile( DBType = typeof( Models.Common.QAOrganization ) )]
    public class QAOrganization : Organization
    {
        //concept??
    }

    [Profile( DBType = typeof( Models.ProfileModels.VerificationStatus ) )]
    public class VerificationStatus : BaseProfile
    {
        //public string Name { get; set; }
        //public string Url { get; set; }
        //public string Description { get; set; }
    }

}
