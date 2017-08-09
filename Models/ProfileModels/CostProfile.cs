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
			//ReferenceUrl = new List<TextValueProfile>();
			CurrencyTypes = new Enumeration();
			Region = new List<JurisdictionProfile>();
			Condition = new List<TextValueProfile>();
		}
		public int EntityId { get; set; }

		public Guid ParentUid { get; set; }
		public int ParentTypeId { get; set; }
		public string ParentType { get; set; }
		public Enumeration CurrencyTypes { get; set; }
		public int CurrencyTypeId { get; set; }
		//not persisted, but used for display
		public string Currency { get; set; }
		public string CurrencySymbol { get; set; }
		public string ExpirationDate { get; set; }
		//public List<TextValueProfile> ReferenceUrl { get; set; }
	
		public string DetailsUrl { get; set; }
		public List<JurisdictionProfile> Region { get; set; }

		public List<CostProfileItem> Items { get; set; }

		public string StartTime { get { return DateEffective; } set { DateEffective = value; } } //Alias used for publishing
		public string EndTime { get { return ExpirationDate; } set { ExpirationDate = value; } } //Alias used for publishing
		public string StartDate { get { return DateEffective; } set { DateEffective = value; } } //Alias used for publishing
		public string EndDate { get { return ExpirationDate; } set { ExpirationDate = value; } } //Alias used for publishing
		public List<TextValueProfile> Condition { get; set; }
	}
	//

	public class CostProfileItem : BaseObject
	{
		public CostProfileItem()
		{
			ProfileName = "";
			CostType = new Enumeration();
			ResidencyType = new Enumeration();
			//EnrollmentType = new Enumeration();
			ApplicableAudienceType = new Enumeration();
			//Payee = new Organization();
		}

		public int CostProfileId
		{
			get { return ParentId; }
			set { this.ParentId = value; }
		}
		/// <summary>
		/// Not persisted, just used for display
		/// </summary>
		public string ProfileName { get; set; }
		//public string ProfileSummary
		//{
		//	get { return ProfileName; }
		//}
		public Enumeration CostType { get; set; }
		public int CostTypeId { get; set; }
		public string CostTypeName { get; set; }
		public string PaymentPattern { get; set; }
		public decimal Price { get; set; }

		[Obsolete]
		public string CostTypeOther { get; set; }
		public Enumeration ResidencyType { get; set; }
		//public Enumeration EnrollmentType { get; set; }

		public Enumeration ApplicableAudienceType { get; set; }
		[Obsolete]
		public string OtherResidencyType { get; set; }

		[Obsolete]
		public string OtherApplicableAudienceType { get; set; }

		public string ParentEntityType { get; set; }
		public string Currency { get; set; }
		public string CurrencySymbol { get; set; }

		[Obsolete]
		public string Description { get; set; }
		[Obsolete]
		public Organization Payee { get; set; }
		//public Guid PayeeUid { get; set; }
		[Obsolete]
		public string OtherEnrollmentType { get; set; }
	}
	//

	//Used for publishing
	public class CostProfileMerged : BaseProfile
	{
		public CostProfileMerged()
		{
			CostType = new Enumeration();
			ResidencyType = new Enumeration();
			//EnrollmentType = new Enumeration();
			AudienceType = new Enumeration();
			Condition = new List<TextValueProfile>();
		
		}
		public Enumeration CostType { get; set; }
		public Enumeration ResidencyType { get; set; }
		//public Enumeration EnrollmentType { get; set; }
		public Enumeration AudienceType { get; set; }
		public List<TextValueProfile> Condition { get; set; }
		
		public string PaymentPattern { get; set; }
		public string Currency { get; set; }
		public string CurrencySymbol { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public string StartDate { get; set; }
		public string EndDate { get; set; }

		public decimal Price { get; set; }
		public string Name {  get { return ProfileName; } set { ProfileName = value; } }
		public string CostDetails { get; set; }
		
		public static List<CostProfileMerged> FlattenCosts( List<CostProfile> input )
		{
			var result = new List<CostProfileMerged>();

			foreach ( var cost in input )
			{
				foreach ( var costItem in cost.Items )
				{
					var currency = "MISSING_CURRENCY";
					try
					{
						currency = ( cost.CurrencyTypes.Items.FirstOrDefault( m => m.Selected == true || m.CodeId == cost.CurrencyTypeId ) ?? cost.CurrencyTypes.Items.First() ).SchemaName;
					}
					catch { }
					result.Add( new CostProfileMerged()
					{
						AudienceType = costItem.ApplicableAudienceType,
						Description = cost.Description,
						EndTime = cost.EndTime,
						Jurisdiction = cost.Jurisdiction,
						Name = cost.ProfileName,
						StartTime = cost.StartTime,
						CostDetails = cost.DetailsUrl,
						Currency = currency,
						CurrencySymbol = cost.CurrencySymbol,
						CostType = costItem.CostType,
						PaymentPattern = costItem.PaymentPattern,
						Price = costItem.Price,
						ResidencyType = costItem.ResidencyType,
						StartDate = cost.StartDate,
						EndDate = cost.EndDate,
						Condition = cost.Condition,
						//ReferenceUrl = cost.ReferenceUrl,
						//EnrollmentType = costItem.EnrollmentType,
					} );
				}
			}

			return result;
		}
	}
	//
}
