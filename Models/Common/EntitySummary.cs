using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{
	public class EntitySummary
	{
		public int Id { get; set; }
		public int EntityTypeId { get; set; }
		public string EntityType { get; set; }
		public System.Guid EntityUid { get; set; }
		public int BaseId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int StatusId { get; set; }
		public System.DateTime Created { get; set; }
		public int CreatedById { get; set; }
		/// <summary>
		/// Entity.Id of the Parent Entity of the Entity related to the base object
		/// </summary>
		public int parentEntityId { get; set; }
		public int ManagingOrgId { get; set; }
		public Nullable<System.Guid> parentEntityUid { get; set; }
		public string parentEntityType { get; set; }
		public int parentEntityTypeId { get; set; }

		public bool IsTopLevelEntity
		{
			get
			{
				if ( " 1 2 3 7 9 ".IndexOf( EntityTypeId.ToString() ) == -1 )
					return false;
				else
					return true;
			}
		}

	}
}
