using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models.ProfileModels;
using Models.Common;
using MN = Models.Node;


namespace Models.Import
{
    public class BaseDTO
    {
        public int RowNumber { get; set; }
        public string Action { get; set; }
        public string CTID { get; set; }
        public string ExternalIdentifier { get; set; }
        public bool ApprovingOnSave { get; set; }

		//TODO - replace the following Existing... properties with ParentObject
		
		public bool IsPotentialPartialUpdate { get; set; }
		public bool IsExistingEntity { get; set; }
		/// <summary>
		/// This is the Id of the main artifact, ex. credential
		/// </summary>
		public int ExistingParentId { get; set; }
		//this is the rowId of the main entity, ie Credential, assessment, or Lopp
		public Guid ExistingParentRowId { get; set; }
        public int ExistingParentTypeId { get; set; }


        public ParentObject ExistingRecord { get; set; } = new ParentObject();
        public bool FoundExistingRecord
        {
            get
            {
                if (ExistingRecord != null && ExistingRecord.Id > 0)
                    return true;
                else
                    return false;
            }
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string SubjectWebpage { get; set; }
        public string OrganizationName { get; set; }
        public int OrganizationId { get; set; }
       // public bool DeleteCompetencyFramworkUrl { get; set; }
        //include if persisting - would be in the org though so ...
        //plus may resolve OwningAgentUid  before continuing
        //public int OwningOrganizationExternalIdentifier { get; set; }
        public Guid OwningAgentUid { get; set; }
        public string OwningOrganizationCtid { get; set; } = "";

        public Enumeration OwnerRoles { get; set; } = new Enumeration();

        public string ImageUrl { get; set; }
        //public string InLanguage { get; set; }
        public List<int> LanguageCodeList { get; set; } = new List<int>();
        public List<string> Keywords { get; set; } = new List<string>();
        public List<string> Subjects { get; set; } = new List<string>();

        public List<MN.CassInput> Frameworks { get; set; } = new List<MN.CassInput>();
        public string AvailabilityListing { get; set; }
        public string AvailableOnlineAt { get; set; }
        public string DateEffective { get; set; }
		//frameworks
		public bool DeletingNaicsCodes { get; set; }
		public List<CodeItem> NaicsCodesList { get; set; } = new List<CodeItem>();
		public List<string> NaicsList { get; set; } = new List<string>();
		public List<string> Industries { get; set; } = new List<string>();

		public bool DeletingSOCCodes { get; set; }
		public List<CodeItem> OnetCodesList { get; set; } = new List<CodeItem>();
		public List<string> OnetList { get; set; } = new List<string>();
		public List<string> Occupations { get; set; } = new List<string>();

		public bool DeletingCIPCodes { get; set; }
		public List<CodeItem> CIPCodesList { get; set; } = new List<CodeItem>();
		public List<string> CIPList { get; set; } = new List<string>();
		public List<string> Programs { get; set; } = new List<string>();

		//duration
		public bool DeleteEstimatedDuration { get; set; }
        public bool HasEstimatedDuration
        {
            get
            {
                if (EstimatedDuration != null
                    && (
                        (EstimatedDuration.ExactDuration != null && EstimatedDuration.ExactDuration.HasValue)
                        || (EstimatedDuration.MinimumDuration != null && EstimatedDuration.MinimumDuration.HasValue)
                        || !string.IsNullOrWhiteSpace(EstimatedDuration.Description)
                    )
                    )
                    return true;
                else
                    return false;
            }
        }

        public DurationProfile EstimatedDuration { get; set; } = new DurationProfile();

        public bool DeleteRenewalFrequency { get; set; }
        public bool HasRenewalFrequency
        {
            get
            {
                if (RenewalFrequency != null
                    && (
                        (RenewalFrequency.ExactDuration != null && RenewalFrequency.ExactDuration.HasValue))
                    )
                    return true;
                else
                    return false;
            }
        }
        public DurationProfile RenewalFrequency { get; set; } = new DurationProfile();

        //public string DurationUnit { get; set; }
        //public int DurationAmount { get; set; }
        //addresses
        public List<string> AddressIdentifiers { get; set; } = new List<string>();
        public bool DeleteAvailableAt { get; set; }
        public List<Address> AvailableAt { get; set; } = new List<Address>();

        public string AudienceTypesList { get; set; }
        public Enumeration AudienceType { get; set; } = new Enumeration();

        //Roles
        //can be a reference org 
        public bool DeleteOfferedBy { get; set; }

        public List<Organization> OfferedByList { get; set; } = new List<Organization>();

        public bool DeleteApprovedBy { get; set; }
        public List<Organization> ApprovedByList { get; set; } = new List<Organization>();

        public bool DeleteAccreditedBy { get; set; }
        public List<Organization> AccreditedByList { get; set; } = new List<Organization>();
        public bool DeleteRecognizedBy { get; set; }
        public List<Organization> RecognizedByList { get; set; } = new List<Organization>();
        //not used yet
        public bool DeleteRegulatedBy { get; set; }
        public List<Organization> RegulatedByList { get; set; } = new List<Organization>();
        public bool DeleteRevokledBy { get; set; }
        public List<Organization> RevokededByList { get; set; } = new List<Organization>();
        public bool DeleteRenewedBy { get; set; }
        public List<Organization> RenewedByList { get; set; } = new List<Organization>();

        public bool HasConditionProfile
        {
            get
            {
                if (ConditionProfile == null && ConditionProfiles.Count() == 0 )
					return false;
				else
				{
					if ( ConditionProfiles.Count() > 0 )
						return ConditionProfiles[ 0 ].IsNotEmpty;
					else
						return ConditionProfile.IsNotEmpty;
				}
			}
        }
        public ConditionProfileDTO ConditionProfile { get; set; } = new ConditionProfileDTO();
        public List<ConditionProfileDTO> ConditionProfiles { get; set; } = new List<ConditionProfileDTO>();

        public bool HasConnectionProfile
        {
            get
            {
				if ( ConnectionProfile == null && ConnectionProfiles.Count() == 0 )
					return false;
				else
				{
					if ( ConnectionProfiles.Count() > 0 )
						return ConnectionProfiles[ 0 ].IsNotEmpty;
					else
						return ConnectionProfile.IsNotEmpty;
				}
            }
        }
        public ConditionProfileDTO ConnectionProfile { get; set; } = new ConditionProfileDTO();
		public List<ConditionProfileDTO> ConnectionProfiles { get; set; } = new List<ConditionProfileDTO>();
		//Manifests
		public List<int> CommonCostsIdentifiers { get; set; } = new List<int>();
        public bool DeleteCommonCosts { get; set; }
        public bool DeleteFrameworks { get; set; }
        public List<int> CommonConditionsIdentifiers { get; set; } = new List<int>();
        public bool DeleteCommonConditions { get; set; }

        //start with just one for now
        public CostProfileDTO CostProfile { get; set; } = new CostProfileDTO();
        public List<CostProfileDTO> CostProfiles { get; set; } = new List<CostProfileDTO>();
        public bool HasCostProfileInput
        {
            get
            {
                if (CostProfile == null && CostProfiles.Count() == 0 )
                    return false;
                else
                    return CostProfile.IsNotEmpty;
            }
        }

    }

    public class ParentObject
    {
        public ParentObject()
        {

        }
        public int Id { get; set; }
        public Guid RowId { get; set; }
        public string Name { get; set; }
		public string EntityType { get; set; } = "";
		public int ParentTypeId { get; set; }
        public Guid OwningAgentUid { get; set; }
    }
}
