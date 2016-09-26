using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{
	public enum EnumerationType
	{
		MULTI_SELECT,
		SINGLE_SELECT,
		FRAMEWORK_SELECT,
		SINGLE_SELECT_ID_ONLY,
		MULTI_SELECT_ID_ONLY,
		CUSTOM
	}

	public class Enumeration
	{
		public Enumeration()
		{
			Items = new List<EnumeratedItem>();
			OtherValue = "";
		}
		public string Name { get; set; }
		public string SchemaName { get; set; }
		public string Description { get; set; }
		public int ParentId { get; set; }
		public string Url { get; set; }
		public string FrameworkVersion { get; set; }
		public List<EnumeratedItem> Items { get; set; }
		public EnumerationType InterfaceType { get; set; }
		public bool ShowOtherValue { get; set; }

		/// <summary>
		/// CategoryId from Codes.PropertyCategory
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// If one of the selected items is other, the related other value is stored here!
		/// </summary>
		public string OtherValue { get; set; }

		/// <summary>
		/// Return true if any Items exist
		/// </summary>
		/// <returns></returns>
		public bool hasItems() //note that for an update, a check may have to be done, in case any previous items were deleted
		{
			return !( Items == null || Items.Count == 0 );
		}

	}
	//

	public class EnumeratedItem
	{
		public EnumeratedItem() {
			IsSpecialValue = false;
		}
		/// <summary>
		/// Database unique ID. 
		/// </summary>
		public int Id { get; set; } 
		public string RowId { get; set; }
		public int ParentId { get; set; } //parent ID. 
		public int CodeId { get; set; } //code table ID. 

		/// <summary>
		/// PK of parent table storing code  - clarify if different than ParentId
		/// </summary>
		public int RecordId { get; set; } 
		/// <summary>
		/// Displayed name
		/// </summary>
		public string Name { get; set; }
 		/// <summary>
		/// Url - optional
 		/// </summary>
		public string URL { get; set; }  
		/// <summary>
		/// Description (if applicable)
		/// </summary>
		public string Description { get; set; } 
		/// <summary>
		/// Schema-based name. Should not contain spaces. May not be necessary.
		/// </summary>
		public string SchemaName { get; set; } 
		/// <summary>
		/// Value referenced in "value" property of HTML objects
		/// </summary>
		public string Value { get; set; }
		public bool IsSpecialValue { get; set; }

		/// <summary>
		/// URL to schema descriptor - can probably delete this
		/// </summary>
		public string SchemaUrl { get; set; } 
		public int SortOrder { get; set; } //Sort Order
		/// <summary>
		/// Indicates whether or not the item is selected
		/// </summary>
		public bool Selected { get; set; }
		public int Totals { get; set; }
		/// <summary>
		/// Datetime this item was created
		/// </summary>
		public DateTime Created { get; set; }
 		/// <summary>
		/// ID of the user that created this item
 		/// </summary>
		public int CreatedById { get; set; }
		public string ItemSummary { get; set; }	
	}
}
