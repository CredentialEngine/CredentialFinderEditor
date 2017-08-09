using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Node
{
	[Profile( DBType = typeof( Models.Common.ContactPoint ) )]
	public class ContactPoint : BaseProfile
	{
		public ContactPoint()
		{
			PhoneNumbers = new List<TextValueProfile>();
			Emails = new List<TextValueProfile>();
			SocialMediaPages = new List<TextValueProfile>();
		}
		public string ContactType { get; set; }
		public string ContactOption { get; set; }
		//public string Email { get; set; }
		//public string Telephone { get; set; }
		//public string FaxNumber { get; set; }

		//public string SocialMedia { get; set; }

		//----------------- OR -------------------
		public List<TextValueProfile> SocialMediaPages { get; set; }

		public List<TextValueProfile> PhoneNumbers { get; set; }
		public List<TextValueProfile> Emails { get; set; }

		

	}
}
