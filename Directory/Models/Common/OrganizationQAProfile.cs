using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{
	public class OrganizationQAProfile : BaseObject
	{
		public int OrganizationId { get; set; }
		public string OrganizationName { get; set; }
		public int BodyTypeId { get; set; }
		public string Description { get; set; }
		public string Url { get; set; }
		public string ManagingConflictsUrl { get; set; }
		public string ComplaintsUrl { get; set; }
		public string AppealsUrl { get; set; }
		public string ExternalRecognitionUrl { get; set; }
		
    
	}
}
