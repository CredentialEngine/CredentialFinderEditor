using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class CodeItem
	{
		public CodeItem()
		{
			SortOrder = 10;
			IsActive = true;
		}

		//the code PK is either Id or Code - the caller will know the context
		public int Id { get; set; }
		public string Code { get; set; }
		public string Name 
		{
			get
			{
				return Title;
			}
			set
			{
				this.Title = value.Trim();
			}
		}
		public string Title { get; set; }
		
		public string Description { get; set; }
		public string URL { get; set; }
		public string SchemaName { get; set; }
		public bool IsActive { get; set; }
		public int SortOrder { get; set; }
		public int Totals { get; set; }
		
	}
}
