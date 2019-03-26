using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models
{
	public class SiteActivity : BaseObject
	{
        public static string AssessmentType = "AssessmentProfile";
        public static string LearningOpportunity = "LearningOpportunity";

        public SiteActivity()
		{
			CreatedDate = System.DateTime.Now;
			ActivityType = "Audit";
		}
		//public int Id { get; set; }
		public DateTime CreatedDate {
			get { return this.Created; }
			set { this.Created = value; }
		}
		public bool IsExternalActivity { get; set; }
		public string DisplayDate
		{
			get 
			{
				if ( Created != null )
				{
					return this.Created.ToString( "yyyy-MM-dd HH.mm.ss" );
				} else 
				return ""; 
			}
		}
		public string ActivityType { get; set; }
		public string Activity { get; set; }
		public string Event { get; set; }
		public string Comment { get; set; }
		public Nullable<int> TargetUserId { get; set; }
		public Nullable<int> ActionByUserId { get; set; }
		public string ActionByUser { get; set; }
		public Nullable<int> ActivityObjectId { get; set; }
		public Nullable<int> ObjectRelatedId { get; set; }
        public Guid? ActivityObjectParentEntityUid { get; set; } = null;

		public string DataOwnerCTID { get; set; } 
		public Nullable<int> TargetObjectId { get; set; }
		public string SessionId { get; set; }
		public string IPAddress { get; set; }
		public string Referrer { get; set; }
		public Nullable<bool> IsBot { get; set; }

        //for search results
        public Nullable<int> ParentEntityTypeId { get; set; }
        public Nullable<int> ParentRecordId { get; set; }
        public string ParentObject { get; set; }
    }
}
