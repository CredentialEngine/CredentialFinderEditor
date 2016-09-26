using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{

	public class BaseObject
	{
		public BaseObject()
		{
			RowId = new Guid(); //Will be all 0s, which is probably desirable
			//DateEffective = new DateTime();
			Created = new DateTime();
			LastUpdated = new DateTime();
			IsNewVersion = false;
			HasCompetencies = false;
			ChildHasCompetencies = false;
		}
		public int Id { get; set; }
		public bool IsNewVersion { get; set; }
		public bool CanEditRecord { get; set; }
		public bool CanUserEditEntity { 
			get { return this.CanEditRecord; }
			set { this.CanEditRecord = value; }

		}
		public Guid RowId { get; set; }
		public int ParentId { get; set; }
		public bool HasCompetencies { get; set; }
		public bool ChildHasCompetencies { get; set; }
		public string DateEffective { get; set; }

		public DateTime Created { get; set; }
		public int CreatedById { get; set; }
		public string CreatedBy { get; set; }
		public DateTime LastUpdated { get; set; }
		public string LastUpdatedDisplay
		{
			get
			{
				if ( LastUpdated == null )
				{
					if ( Created != null )
					{
						return Created.ToShortDateString();
					}
					return "";
				}
				return LastUpdated.ToShortDateString();
			}
		}
		public int LastUpdatedById { get; set; }
		public string LastUpdatedBy { get; set; }
	}
	//

}
