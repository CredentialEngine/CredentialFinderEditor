using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{

	public class CostProfile : BaseProfile
	{
		public CostProfile()
		{
			ExpirationDate = ""; //			new DateTime();
			Items = new List<CostProfileItem>();
		}
		public int EntityId { get; set; }

		public Guid ParentUid { get; set; }
		public int ParentTypeId { get; set; }
		public string ParentType { get; set; }
		public Enumeration CurrencyTypes { get; set; }
		public int CurrencyTypeId { get; set; }
		public string Currency { get; set; }
		public string ExpirationDate { get; set; }
		public List<TextValueProfile> ReferenceUrl { get; set; }
		[Obsolete]
		public string DetailsUrl { get; set; }
		public List<CostProfileItem> Items { get; set; }
	}
	//

	public class CostProfileItem : BaseObject
	{
		public CostProfileItem()
		{
			CostType = new Enumeration();
			ResidencyType = new Enumeration();
			EnrollmentType = new Enumeration();
			ApplicableAudienceType = new Enumeration();
			Payee = new Organization();
		}

		public int CostProfileId
		{
			get { return ParentId; }
			set { this.ParentId = value; }
		}
		public string ProfileName { get; set; }
		public string ProfileSummary
		{
			get { return ProfileName; }
		}
		public Enumeration CostType { get; set; }
		public int CostTypeId { get; set; }
		public string CostTypeOther { get; set; }
		public Enumeration ResidencyType { get; set; }
		public Enumeration EnrollmentType { get; set; }
		public Enumeration ApplicableAudienceType { get; set; }
		public string OtherResidencyType { get; set; }
		public string OtherEnrollmentType { get; set; }
		public string OtherApplicableAudienceType { get; set; }
		public string PaymentPattern { get; set; }
		public decimal Price { get; set; }
		public string Description { get; set; }
		public Organization Payee { get; set; }
		public Guid PayeeUid { get; set; }
	}
	//

}
