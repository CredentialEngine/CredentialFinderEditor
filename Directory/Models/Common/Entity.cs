using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{
	public class Entity : BaseObject
	{
		public System.Guid EntityUid { get; set; }
		public int EntityTypeId { get; set; }
		public string EntityType { get; set; }
		
	}
}
