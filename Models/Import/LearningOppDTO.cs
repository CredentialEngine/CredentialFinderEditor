using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.ProfileModels;
using Models.Common;

namespace Models.Import
{
    public class LearningOppDTO : BaseDTO
    {
        public bool DeleteProfile { get; set; }
        public Guid Identifier { get; set; }

        /// <summary>
        /// Proposal: allow including the external identifier used for a credential upload. 
        /// - allow specifying the condition type.
        /// - check if a condition profile exists
        ///     - if not create the specified type, and add as target asmt
        ///     - otherwise add to existing as target
        ///     - need to check if already part of condition
        /// </summary>
        public string CredentialExternalIdentifier { get; set; }
        public int CredentialConditionTypeId { get; set; }

        public List<Credential> TargetCredentials { get; set; } = new List<Credential>();

		public new LearningOpportunityProfile ExistingRecord { get; set; } = new LearningOpportunityProfile();
		public new bool FoundExistingRecord
		{
			get
			{
				if ( ExistingRecord != null && ExistingRecord.Id > 0 )
					return true;
				else
					return false;
			}
		}

		public string CodedNotation { get; set; }

		public CreditStuffDTO CreditStuffDTO { get; set; } = new CreditStuffDTO();
		public decimal CreditMinValue { get; set; }
		public decimal CreditMaxValue { get; set; }

		public string CreditHourType { get; set; }
        public decimal CreditHourValue { get; set; }
        public int CreditUnitTypeId { get; set; }
        public string CreditUnitType { get; set; } = "";
        //public Enumeration CreditUnitType { get; set; } //Used for publishing
        public string CreditUnitTypeDescription { get; set; }
        public decimal CreditUnitValue { get; set; }
        public string DeliveryTypeList { get; set; }
        public Enumeration DeliveryType { get; set; } = new Enumeration();
        public string DeliveryTypeDescription { get; set; }
        public string LearningMethodTypeList { get; set; }
        public Enumeration LearningMethodType { get; set; } = new Enumeration();
        public string LearningResourceUrl { get; set; }
        public string IdentificationCode { get; set; }

		public string VerificationMethodDescription { get; set; }
        public string VersionIdentifier { get; set; }

        public bool IsNotEmpty
        {
            get
            {
                if ( !string.IsNullOrEmpty( Name )
                    || ( !string.IsNullOrEmpty( Description ) )
                    || ( !string.IsNullOrEmpty( SubjectWebpage ) )
                    )
                    return true;
                else
                    return false;
            }
        }
    }
}
