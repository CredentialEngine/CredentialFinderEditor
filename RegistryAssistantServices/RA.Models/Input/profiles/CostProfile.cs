using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Models.Input
{
	/// <summary>
	/// CostProfile class
	/// </summary>
	//public class CostProfile : ICloneable
	public class CostProfile 
	{
        /// <summary>
        /// Inialize lists
        /// </summary>
        public CostProfile()
        {
            Jurisdiction = new List<Jurisdiction>();
            //Region = new List<GeoCoordinates>();
            CostItems = new List<CostProfileItem>();
            //ResidencyType = new List<string>();
            //AudienceType = new List<string>();
            //DirectCostType = new List<string>();
			Condition = new List<string>();

		}

        /// <summary>
        /// Details URL
        /// </summary>
        public string CostDetails { get; set; }
        /// <summary>
        /// A currency code, for ex USD
        /// </summary>
        public string Currency { get; set; }
        /// <summary>
        /// Description of the cost profile
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Name of the cost profile
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Start date or effective date of this cost profile
        /// </summary>
        public string StartDate { get; set; }
        /// <summary>
        /// End date or expiry date of this cost profile
        /// </summary>
        public string EndDate { get; set; }

		/// <summary>
		/// List of condtions, containing:
		/// A single condition or aspect of experience that refines the conditions under which the resource being described is applicable.
		/// </summary>
		public List<string> Condition { get; set; }

		/// <summary>
		/// List of jurisdictions
		/// </summary>
		public List<Jurisdiction> Jurisdiction { get; set; }
		/// <summary>
		/// List of regions (jurisdictions)
		/// </summary>
		//  public List<GeoCoordinates> Region { get; set; }

		/// <summary>
		/// List of cost items
		/// This seems to be the logical approach at this time
		/// </summary>
		public List<CostProfileItem> CostItems { get; set; }

        //OR 

		//public string DirectCostType { get; set; }
		//public List<string> ResidencyType { get; set; }
  //      public List<string> AudienceType { get; set; }
  //      public decimal Price { get; set; }

  //      public string PaymentPattern { get; set; }



        //public object Clone()
        //{
        //    var cp = new CostProfile();
        //    cp.CostDetails = CostDetails;
        //    cp.Currency = Currency;
        //    cp.Description = Description;
        //    cp.Name = Name;
        //    cp.EndDate = EndDate;
        //    cp.StartDate = StartDate;
        //    cp.Jurisdiction = Jurisdiction;
        //   // cp.Region = Region;
        //    return cp;
        //}
    }

	/// <summary>
	/// Cost item class
	/// </summary>
	public class CostProfileItem
	{
		/// <summary>
		/// Initialize
		/// </summary>
		public CostProfileItem()
		{
			ResidencyType = new List<string>();
			AudienceType = new List<string>();
		}

		/// <summary>
		/// Must be a valid CTDL cost type
		/// </summary>
		public string DirectCostType { get; set; }
		/// <summary>
		/// List of Residency items
		/// </summary>
		public List<string> ResidencyType { get; set; }

		/// <summary>
		/// List of Audience Types
		/// </summary>
		public List<string> AudienceType { get; set; }

		/// <summary>
		/// Payment Pattern
		/// </summary>
		public string PaymentPattern { get; set; }
		/// <summary>
		/// Price for this cost - optional
		/// </summary>
		public decimal Price { get; set; }




	}
	/// <summary>
	/// Version of Cost Profile where all cost items are flattened into a single record.
	/// That is:
	/// - if there is a single cost profile with three costs
	/// - Output will be three cost profiles with the cost profile specific properties repeated for each cost profile item
	/// </summary>
	public class CostProfileFlattened : CostProfile
	{
		public CostProfileFlattened()
		{
			ResidencyType = new List<string>();
			AudienceType = new List<string>();
		}
		/// <summary>
		/// Must be a valid CTDL cost type
		/// </summary>
		public string DirectCostType { get; set; }
		/// <summary>
		/// List of Residency items
		/// </summary>
		public List<string> ResidencyType { get; set; }

		/// <summary>
		/// List of Audience Types
		/// </summary>
		public List<string> AudienceType { get; set; }

		/// <summary>
		/// Price for this cost - optional
		/// </summary>
		public decimal Price { get; set; }

		/// <summary>
		/// Payment Pattern
		/// </summary>
		public string PaymentPattern { get; set; }
	}


}
