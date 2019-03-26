using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace RA.Models.Input
{
	public class ConditionManifestRequest : BaseRequest
	{
		public ConditionManifestRequest()
		{
			//ConditionManifests = new List<ConditionManifest>();
			ConditionManifest = new ConditionManifest();
		}

		/// <summary>
		/// consider allowing multiple?
		/// Probably too many complications
		/// </summary>
		//public List<ConditionManifest> ConditionManifests { get; set; }

		public ConditionManifest ConditionManifest { get; set; }


	}

	public class ConditionManifest
	{
		public ConditionManifest()
		{
            EntryConditions = new List<ConditionProfile>();
            Requires = new List<ConditionProfile>();
            RenewedConditions = new List<ConditionProfile>();
            RecommendedConditions = new List<ConditionProfile>();
			CorequisiteConditions = new List<ConditionProfile>();
		}

        public string Name { get; set; }
        /// <summary>
        /// Alternately can provide a language map
        /// </summary>
        public LanguageMap Name_Map { get; set; } = new LanguageMap();
        /// <summary>
        /// Description 
        /// Required
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Alternately can provide a language map
        /// </summary>
        public LanguageMap Description_Map { get; set; } = new LanguageMap();
        public string SubjectWebpage { get; set; } //URL
		public string Ctid { get; set; }

        /// <summary>
        /// Organization that owns this ConditionManifest
        /// </summary>
        public OrganizationReference ConditionManifestOf { get; set; } = new OrganizationReference();
        //Alias
        public OrganizationReference OwningOrganization
        {
            get { return ConditionManifestOf; }
            set { ConditionManifestOf = value; }
        }

        /// <summary>
        /// Required condition profiles
        /// </summary>
        public List<ConditionProfile> Requires { get; set; }
        public List<ConditionProfile> RequiredConditions
        {
            get { return Requires; }
            set { Requires = value; }
        }
        /// <summary>
        /// Renewed condition profiles
        /// </summary>
        public List<ConditionProfile> RenewedConditions { get; set; }

        /// <summary>
        /// Recommended condition profiles Recommends
        /// </summary>
        public List<ConditionProfile> RecommendedConditions { get; set; }

		/// <summary>
		/// Entry Conditions condition profiles
		/// </summary>
		public List<ConditionProfile> EntryConditions { get; set; }

        /// <summary>
        /// Corequisite Conditions
        /// ceterms:corequisite
        /// The resource being referenced must be pursued concurrently with the resource being described.
        /// </summary>
        /// See: <a href="http://purl.org/ctdl/terms/corequisite">corequisite</a>
        /// <remarks>Includes dual (double) degrees that cannot be earned independently of each other.</remarks>
        public List<ConditionProfile> CorequisiteConditions { get; set; }
	}
}
