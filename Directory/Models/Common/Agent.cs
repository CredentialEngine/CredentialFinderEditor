using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.ProfileModels;

namespace Models.Common
{
	/// <summary>
	/// Agent could be an org or a person
	/// This could just inherit from org?
	/// </summary>
	public class Agent : BaseObject
	{
		public Agent()
		{
			Address = new Address();
			Addresses = new List<Address>();
			IdentificationCodes = new List<TextValueProfile>();
			PhoneNumbers = new List<TextValueProfile>();
			Keywords = new List<TextValueProfile>();
			//Subjects = new List<TextValueProfile>();
			Emails = new List<TextValueProfile>();
			SocialMediaPages = new List<TextValueProfile>();
			//SocialMedia = new List<TextValueProfile>();
		}
		/// <summary>
		/// Organization or Person
		/// </summary>
		public string AgentType { get; set; }
		/// <summary>
		/// 1-Organization; 2-Person
		/// </summary>
		public int AgentTypeId { get; set; }
		//????
		public int AgentRelativeId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Url { get; set; }
		public string UniqueURI { get; set; }
		public string ImageUrl { get; set; }
		public string CredentialRegistryId { get; set; }
		public string ctid { get; set; }
		public string Email { get; set; }
		public string MainPhoneNumber { get; set; }
		//public string FaxNumber { get; set; }
		//public string TollFreeNumber { get; set; }
		//public string FoundingDate { get; set; }
		public Address Address { get; set; }
		public Address MainAddress { 
			get {
				if ( Address == null )
				{
					if ( Addresses != null && Addresses.Count > 0 )
						Address = Addresses[ 0 ];
				}
				return Address;}
			set {Address = value; } 
		}
		public List<Address> Addresses { get; set; }
		public List<JurisdictionProfile> JurisdictionProfiles { get; set; }
		//SocialMedia is saved as an OrganizationProperty
		// -not anymore
		//public List<TextValueProfile> SocialMedia { get; set; }
		public List<TextValueProfile> SocialMediaPages { get; set; }
		public List<TextValueProfile> IdentificationCodes { get; set; }
		public List<TextValueProfile> PhoneNumbers { get; set; }
		public List<TextValueProfile> Emails { get; set; }
		//public List<TextValueProfile> Subjects { get; set; }
		public List<TextValueProfile> Keywords { get; set; }
	}
}
