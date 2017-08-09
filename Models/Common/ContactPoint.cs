using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.ProfileModels;

namespace Models.Common
{
	/// <summary>
	/// Contact Point
	/// </summary>
	public class ContactPoint : BaseObject
	{
		public ContactPoint()
		{
			PhoneNumbers = new List<TextValueProfile>();
			Emails = new List<TextValueProfile>();
			SocialMediaPages = new List<TextValueProfile>();
			
		}

		public Guid ParentRowId { get; set; }

		public string ProfileName { get; set; }
		public string Name {  get { return ProfileName; } set { ProfileName = value; } } //Alias used for publishing
		/// <summary>
		/// Specification of the type of contact.
		/// </summary>
		public string ContactType { get; set; }

		/// <summary>
		/// An option available on this contact point.
		/// For example, a toll-free number or support for hearing-impaired callers.
		/// </summary>
		public string ContactOption { get; set; }
		public string Email { get; set; }
		public string Telephone { get; set; }
		public string FaxNumber { get; set; }
		/// <summary>
		/// A social media resource for the resource being described.
		/// </summary>
		public string SocialMedia { get; set; }

		// ===========  OR  ===================
		public List<TextValueProfile> PhoneNumbers { get; set; }
		public List<TextValueProfile> Emails { get; set; }
		public List<TextValueProfile> SocialMediaPages { get; set; }

		//Aliases used for publishing
		public List<TextValueProfile> Auto_Telephone { get
			{
				var result = new List<TextValueProfile>()
					.Concat( PhoneNumbers.ConvertAll( m => new TextValueProfile() { TextTitle = m.CodeTitle, TextValue = m.TextValue } ) ).ToList();
				if ( !string.IsNullOrWhiteSpace( Telephone ) )
				{
					result.Add( new TextValueProfile()
					{
						TextTitle = ContactType,
						TextValue = Telephone
					} );
				}
				return result;
			}
		}
		public List<TextValueProfile> Auto_FaxNumber
		{
			get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( FaxNumber ) )
				{
					result.Add( new TextValueProfile()
					{
						TextTitle = ContactType,
						TextValue = FaxNumber
					} );
				}
				return result;
			}
		}
		public List<TextValueProfile> Auto_Email { get
			{
				var result = new List<TextValueProfile>()
					.Concat( Emails ).ToList();
				if ( !string.IsNullOrWhiteSpace( Email ) )
				{
					result.Add( new TextValueProfile()
					{
						TextTitle = ContactType,
						TextValue = Email
					} );
				}
				return result;
			}
		}
		public List<TextValueProfile> Auto_SocialMedia { get
			{
				var result = new List<TextValueProfile>()
					.Concat( SocialMediaPages ).ToList();
				if ( !string.IsNullOrWhiteSpace( SocialMedia ) )
				{
					result.Add( new TextValueProfile()
					{
						TextValue = SocialMedia
					} );
				}
				return result;
			}
		}
		public List<TextValueProfile> Auto_ContactOption { get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( ContactOption ) )
				{
					result.Add( new TextValueProfile()
					{
						TextTitle = ContactType,
						TextValue = ContactOption
					} );
				}
				return result;
			}
		}
	}
}
