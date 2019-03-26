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

		public int EntityBaseId { get; set; }
		public string EntityBaseName { get; set; }
	} //


	public class EntityReference : BaseObject
	{
		public System.Guid EntityUid { get; set; }
		private int _entityTypeId { get; set; }
		public int EntityTypeId {
			get { return _entityTypeId; }
			set
			{
				_entityTypeId = value;
				if ( _entityTypeId == 1 )
					EntityType = "Credential";
				else if ( _entityTypeId == 2 )
					EntityType = "Organization";
				else if ( _entityTypeId == 3 )
					EntityType = "Assessment";
				else if ( _entityTypeId == 7 )
					EntityType = "LearningOpportunity";
				else if ( _entityTypeId == 19 )
					EntityType = "Condition Manifest";
				else if ( _entityTypeId == 20 )
					EntityType = "Cost Manifest";
				else
					EntityType = string.Format( "Unexpected EntityTypeId of {0}", _entityTypeId );
			}
		}
		public string EntityType { get; set; }

		public int EntityBaseId { get; set; }
		public string EntityBaseName { get; set; }
		public string CTID { get; set; }
		public bool IsValid { get; set; }
	}

    /// <summary>
    /// note: Entity_Approval is instanciated in BaseObject, so can't inherit from here!
    /// </summary>
	public class Entity_Approval 
    {
		public Entity_Approval()
		{
			//redundent, but explicit
			IsActive = false;
		}
		public int Id { get; set; }
		public int EntityId { get; set; }
		public bool IsActive { get; set; }
        public System.DateTime Created { get; set; }
        public int CreatedById { get; set; }
        public string CreatedBy { get; set; }

    }
}
