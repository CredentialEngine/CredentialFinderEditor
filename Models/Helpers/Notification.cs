using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.Helpers
{
	//Based on the implementation used in the accounts system to maximize compatibility if the two email systems are ever merged
	public class Notification : CoreObject
	{
		public Notification()
		{
			ToEmails = new List<string>();
			Tags = new List<string>();
		}
		public Guid ForAccountRowId { get; set; } 
		public bool IsRead { get; set; }
		public List<string> ToEmails { get; set; }
		public string FromEmail { get; set; }
		public string Subject { get; set; }
		public string BodyHtml { get; set; }
		public string BodyText { get; set; }
		public List<string> Tags { get; set; }
	}
	//

	public class NotificationQuery
	{
		public string Keywords { get; set; }
		public int PageSize { get; set; }
		public int PageNumber { get; set; }
		public Guid ForAccountRowId { get; set; }
		public string ToEmails { get; set; }
	}
	//
}
