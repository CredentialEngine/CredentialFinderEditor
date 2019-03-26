using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace Models.Common
{
	[Serializable]
	public class CoreObject
	{
		//Lets auto-mapping methods check to see if this property should be skipped, which helps ensure critical properties don't get overwritten by mistake
		public class UpdateAttribute : Attribute
		{
			public bool SkipPropertyOnUpdate { get; set; } 
		}

		//Convenience method to get skippable properties
		public static List<PropertyInfo> GetSkippableProperties( object data )
		{
			var result = new List<PropertyInfo>();
			foreach( var property in data.GetType().GetProperties() )
			{
				var updateAttribute = (UpdateAttribute) property.GetCustomAttribute( typeof( UpdateAttribute ) );
				if ( updateAttribute != null && updateAttribute.SkipPropertyOnUpdate )
				{
					result.Add( property );
				}
			}
			return result;
		}
		public List<PropertyInfo> GetSkippableProperties()
		{
			return GetSkippableProperties( this );
		}

		//Normal object stuff
		public CoreObject()
		{
			//Probably don't need to initialize anything here as long as BaseObject is still initializing things, since most stuff inherits from that
		}
		[Update(SkipPropertyOnUpdate = true)]
		public int Id { get; set; }
		[Update( SkipPropertyOnUpdate = true )]
		public Guid RowId { get; set; }
		[Update( SkipPropertyOnUpdate = true )]
		public DateTime Created { get; set; }
		[Update( SkipPropertyOnUpdate = true )]
		public int CreatedById { get; set; }
		public DateTime LastUpdated { get; set; }
		public int LastUpdatedById { get; set; }
	}

    [Serializable]
    public class BaseObject : CoreObject
	{
		public BaseObject()
		{
			RowId = new Guid(); //Will be all 0s, which is probably desirable
			Created = new DateTime();
			LastUpdated = new DateTime();
			//DateEffective = new DateTime();
			IsStarterProfile = false;
			//IsNewVersion = true;
			HasCompetencies = false;
			ChildHasCompetencies = false;
			//Publish_Type = "ceterms:entity";
			StatusMessage = "";
			EntityApproval = new Entity_Approval();
		}
        //not sure if we can force this to be an integer - pretty likely, but?
        public string ExternalIdentifier { get; set; }
        public bool IsStarterProfile{ get; set; }
		public bool IsReferenceVersion { get; set; }
		public bool IsNewVersion { get; set; }
		public bool CanEditRecord { get; set; }
		public bool CanUserEditEntity { 
			get { return this.CanEditRecord; }
			set { 
				this.CanEditRecord = value; 
			}

		}
		public bool CanViewRecord { get; set; }
		public int ParentId { get; set; }
		public bool HasCompetencies { get; set; }
		public bool ChildHasCompetencies { get; set; }
		public string DateEffective { get; set; }
		public string StatusMessage { get; set; }

		public string CreatedBy { get; set; }
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
		public string LastUpdatedBy { get; set; }

		//****NOTE: will get stack overflow initializing here. 
		//current code will always check for nulls
		public Entity RelatedEntity { get; set; } 
        public DateTime EntityLastUpdated { get; set; }

        //Approval properties
        public DateTime LastApproved { get; set; }
        public int LastApprovedById { get; set; }

        //TODO - this is to be removed, and replace by concrete properties.
        public Entity_Approval EntityApproval { get; set; }
		public bool IsEntityApproved()
		{
			if ( EntityApproval != null && EntityApproval.IsActive )
				return true;
			else if ( LastApprovedById > 0 )
                return true;
            else
                return false;
		}
        //Publishing properties
        public DateTime LastPublished { get; set; }
        public int LastPubishedById { get; set; }
        public string Publish_Type { get; set; }

		//public virtual Dictionary<string, object> Publish_GetPublishableVersion()
		//{
		//	return new Dictionary<string, object>()
		//	{
		//		{ "@type", Publish_Type },
		//		{ "@id", "http://credentialengineregistry.org/resource/" + RowId.ToString() }
		//	};
		//}
		//

	}
	//

}
