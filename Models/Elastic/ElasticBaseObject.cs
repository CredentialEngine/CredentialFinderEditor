using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Elastic
{
	public class ElasticBaseObject
	{
		public ElasticBaseObject()
		{
			RowId = new Guid(); 
			Created = new DateTime();
			LastUpdated = new DateTime();
			//HasCompetencies = false;
			//ChildHasCompetencies = false;
		}
		public int Id { get; set; }
		public Guid RowId { get; set; }
		//??
		//public int EntityTypeId { get; set; }
		//public int EntityId { get; set; }
		
		public int ParentId { get; set; }
		public int StatusId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Url { get; set; }
		public string ImageUrl { get; set; } 
		public int ManagingOrgId { get; set; }

		//maybe
		public string CTID { get; set; }
		public string CredentialRegistryId { get; set; }
		//public bool HasCompetencies { get; set; }
		//public bool ChildHasCompetencies { get; set; }

		public DateTime DateEffective { get; set; }
		public DateTime Created { get; set; }
		public DateTime LastUpdated { get; set; }

	}
	//
}
