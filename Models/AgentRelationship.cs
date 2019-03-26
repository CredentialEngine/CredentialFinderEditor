using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
	public class AgentRelationship
	{
		public AgentRelationship()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public int RelationshipId { get; set; }

		public string Relationship { get; set; }

		public int AgentId { get; set; }
		public System.Guid AgentUid { get; set; }
		public string Agent { get; set; }
        public string AgentUrl { get; set; }
        public string IsThirdPartyOrganization { get; set; }
		public int CategoryId { get; set; }
		public string Category { get; set; }
		public string CategorySchema { get; set; }
		public string EntityType { get; set; }
    }
}
