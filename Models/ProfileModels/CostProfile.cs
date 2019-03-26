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
        public string Name
        {
           get { return this.ProfileName; }
            set { this.ProfileName = value; }
        }
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

		public Enumeration CostType { get; set; }
		public int CostTypeId { get; set; }
		public string CostTypeName { get; set; }
		public string CostTypeSchema { get; set; }
		public string PaymentPattern { get; set; }
		public decimal Price { get; set; }

		public Enumeration ResidencyType { get; set; }
		//public Enumeration EnrollmentType { get; set; }

		public Enumeration ApplicableAudienceType { get; set; }

        //convenience properties for use in search
        public string ParentEntityType { get; set; }
        
		public string Currency { get; set; }
		public string CurrencySymbol { get; set; }

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

		public string DirectCostType { get; set; }
		public decimal Price { get; set; }
		public string Name {  get { return ProfileName; } set { ProfileName = value; } }
		public string CostDetails { get; set; }
		
		public static List<CostProfileMerged> FlattenCosts( List<CostProfile> input )
		{
			var result = new List<CostProfileMerged>();
            if ( input == null || input.Count == 0 )
                return result;

			foreach ( var cost in input )
			{
				var currency = "";
				try
				{
					currency = ( cost.CurrencyTypes.Items.FirstOrDefault( m => m.Selected == true || m.CodeId == cost.CurrencyTypeId ) ?? cost.CurrencyTypes.Items.First() ).SchemaName;
				}
				catch { }
				foreach ( var costItem in cost.Items )
				{
					result.Add( new CostProfileMerged()
					{
						AudienceType = costItem.ApplicableAudienceType,
						Description = cost.Description,
						EndDate = cost.EndDate,
						EndTime = cost.EndTime,
						Jurisdiction = cost.Jurisdiction,
						Name = cost.ProfileName,
						StartDate = cost.StartDate,
						StartTime = cost.StartTime,
						CostDetails = cost.DetailsUrl,
						Currency = currency,
						CurrencySymbol = cost.CurrencySymbol,
						CostType = costItem.CostType,
						DirectCostType = costItem.CostTypeSchema,
						PaymentPattern = costItem.PaymentPattern,
						Price = costItem.Price,
						ResidencyType = costItem.ResidencyType,
						Condition = cost.Condition
					} );
				}
				//Handle itemless cost profile
				if( cost.Items.Count() == 0 )
				{
					result.Add( new CostProfileMerged()
					{
						Description = cost.Description,
						EndTime = cost.EndTime,
						Jurisdiction = cost.Jurisdiction,
						Name = cost.ProfileName,
						StartTime = cost.StartTime,
						CostDetails = cost.DetailsUrl,
						Currency = currency,
						CurrencySymbol = cost.CurrencySymbol,
						StartDate = cost.StartDate,
						EndDate = cost.EndDate,
						Condition = cost.Condition,
					} );
				}
			}

			return result;
		}

		public static List<CostProfile> ExpandCosts( List<CostProfileMerged> input )
		{
			var result = new List<CostProfile>();
            if ( input == null || input.Count == 0 )
                return result;

            //First expand each into its own CostProfile with one CostItem
            var holder = new List<CostProfile>();
			foreach ( var merged in input )
			{
				//Create cost profile
				var cost = new CostProfile()
				{
					ProfileName = merged.Name,
					Description = merged.Description,
					Jurisdiction = merged.Jurisdiction,
					StartTime = merged.StartTime,
					EndTime = merged.EndTime,
					StartDate = merged.StartDate,
					EndDate = merged.EndDate,
					DetailsUrl = merged.CostDetails,
					Currency = merged.Currency,
					CurrencySymbol = merged.CurrencySymbol,
					Condition = merged.Condition,
					Items = new List<CostProfileItem>()
				};
				//If there's any data for a cost item, create one
				if ( 
					merged.Price > 0 || 
					!string.IsNullOrWhiteSpace( merged.PaymentPattern ) ||
					merged.AudienceType.Items.Count() > 0 ||
					merged.CostType.Items.Count() > 0 ||
					merged.ResidencyType.Items.Count() > 0
					)
				{
					cost.Items.Add( new CostProfileItem()
					{
						ApplicableAudienceType = merged.AudienceType,
						CostType = merged.CostType,
						PaymentPattern = merged.PaymentPattern,
						Price = merged.Price,
						ResidencyType = merged.ResidencyType
					} );
				}
			}

			//Remove duplicates and hope that pass-by-reference issues don't cause trouble
			while( holder.Count() > 0 )
			{
				//Take the first item in holder and set it aside
				var currentItem = holder.FirstOrDefault();
				//Remove it from the holder list so it doesn't get included in the LINQ query results on the next line
				holder.Remove( currentItem );
				//Find any other items in the holder list that match the item we just took out
				var matches = holder.Where( m =>
					m.ProfileName == currentItem.ProfileName &&
					m.Description == currentItem.Description &&
					m.DetailsUrl == currentItem.DetailsUrl &&
					m.Currency == currentItem.Currency &&
					m.CurrencySymbol == currentItem.CurrencySymbol
				).ToList();
				//For each matching item...
				foreach( var item in matches )
				{
					//Take its cost profile items (if it has any) and add them to the cost profile we set aside
					currentItem.Items = currentItem.Items.Concat( item.Items ).ToList();
					//Remove the item from the holder so it doesn't get detected again, and so that we eventually get out of this "while" loop
					holder.Remove( item );
				}
				//Now that currentItem has all of the cost profile items from all of its matches, add it to the result
				result.Add( currentItem );
			}

			return result;
		}
	}
	//
}
