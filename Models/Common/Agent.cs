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
    [Serializable]
    public class Agent : BaseObject
	{
		public Agent()
		{
			Address = new Address();
			Addresses = new List<Address>();
			IdentificationCodes = new List<TextValueProfile>();
			AlternativeIdentifiers = new List<TextValueProfile>();
			AlternateName = new List<TextValueProfile>();
			PhoneNumbers = new List<TextValueProfile>();
			Keyword = new List<TextValueProfile>();
			//Subjects = new List<TextValueProfile>();
			Emails = new List<TextValueProfile>();
			SocialMediaPages = new List<TextValueProfile>();
			SameAs = new List<TextValueProfile>();
			ContactPoint = new List<Common.ContactPoint>();
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
		//public int AgentRelativeId { get; set; }
		public string Name { get; set; }
		public string FriendlyName { get; set; }
		public string Description { get; set; }
		
		public string SubjectWebpage { get; set; }
		public List<TextValueProfile> Auto_SubjectWebpage { get { return string.IsNullOrWhiteSpace( SubjectWebpage ) ? null : new List<TextValueProfile>() { new TextValueProfile() { TextValue = SubjectWebpage } }; } }

		public string ImageUrl { get; set; }
		public List<TextValueProfile> Auto_ImageUrl
		{
			get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( ImageUrl ) )
				{
					result.Add( new TextValueProfile() { TextValue = ImageUrl } );
				}
				return result;
			}
		}

		//approvals
		public bool IsApproved { get; set; }
		public int ContentApprovedById { get; set; }
		public string ContentApprovedBy { get; set; }
		public string LastApprovalDate { get; set; }

		public string CredentialRegistryId { get; set; }
        public string LastPublishDate { get; set; } = "";
        public bool IsPublished { get; set; }
        //public DateTime EntityLastUpdated { get; set; }
        public string ctid { get; set; }
		public string CTID { get { return ctid; } set { ctid = value; } } //Alias used for publishing

		/// <summary>
		/// Alias used for publishing
		/// Use CTID if present, otherwise SubjectWebpage
		/// </summary>
		public string Auto_OrgURI
		{
			get
			{
				var result = "";
				if ( Id == 0)
					return "";
				if ( (CTID ?? "" ).Length == 39 )
					result = Utilities.GetWebConfigValue( "credRegistryResourceUrl" ) + CTID;
				else if ((SubjectWebpage ?? "").Length > 10)
					result = SubjectWebpage;

				return result;
			}
		}

	
		public Address Address { get; set; }

		public List<Address> Addresses { get; set; }

		//Alias used for publishing
		public List<Address> Auto_Address {  get
			{
				var result = new List<Address>();
				if( Address != null && Address.Id > 0 )
				{
					result.Add( Address );
				}
				result = result.Concat( Addresses ).ToList();

				return result;
			}
		}
		public string AvailabilityListing { get; set; }
		public List<TextValueProfile> Auto_AvailabilityListing { get
			{
				var result = new List<TextValueProfile>();
				if ( !string.IsNullOrWhiteSpace( AvailabilityListing ) )
				{
					result.Add( new TextValueProfile() { TextValue = AvailabilityListing } );
				}
				return result;
			} }
		public string Versioning { get; set; }

		public List<JurisdictionProfile> JurisdictionProfiles { get; set; }
		//SocialMedia is saved as an OrganizationProperty
		// -not anymore
		//public List<TextValueProfile> SocialMedia { get; set; }
		public List<TextValueProfile> SocialMediaPages { get; set; }
		public List<TextValueProfile> Auto_SocialMedia
		{
			get { return SocialMediaPages; }
			set { SocialMediaPages = value; }
		} //Alias used for publishing

		public List<TextValueProfile> SameAs { get; set; }
		public List<TextValueProfile> Auto_SameAs { get { return SameAs; } set { SameAs = value; } } //Alias used for publishing
		
		public List<TextValueProfile> PhoneNumbers { get; set; }

		/// <summary>
		/// only for use in search results
		/// </summary>
		public string MainPhoneNumber { get; set; }
		/// <summary>
		/// Alias used for publishing mapping
		/// Starts by creating a ContactPoint for all the top level emails, etc
		/// ===> Actually used by detail page as well!!!!!!!!!
		/// so created Auto_TargetContactPointForDetail
        /// 18-06-26 mparsons - need to change the handling, as contact point is no longer part of an org, only a place
		/// </summary>
		//public List<ContactPoint> Auto_TargetContactPoint
		//{
		//	get
		//	{
		//		var results = new List<ContactPoint>();

		//		//var autoContact = new ContactPoint()
		//		//{
		//		//	Name = "Organization Contact Information",
		//		//	PhoneNumbers = PhoneNumbers,
		//		//	Emails = Emails,
		//		//	SocialMediaPages = SocialMediaPages
		//		//};
		//		//results.Add( autoContact );

		//		if ( ContactPoint != null && ContactPoint.Count() > 0 )
		//		{
		//			results = results.Concat( ContactPoint ).ToList();
		//		}
		//		/*if( PhoneNumbers != null && PhoneNumbers.Count() > 0 )
		//		{
		//			results = results.Concat(
		//				PhoneNumbers.ConvertAll( m => new Common.ContactPoint()
		//				{
		//					ContactType = (string.IsNullOrWhiteSpace( m.TextTitle ) ? m.CodeTitle : m.TextTitle),
		//					Telephone = m.TextValue,
		//					FaxNumber = (m.CodeTitle == "Fax" ? m.TextValue : "")
		//				} ).ToList()
		//			).ToList();
		//		}
		//		if( Emails != null && Emails.Count() > 0 )
		//		{
		//			results = results.Concat(
		//				Emails.ConvertAll( m => new Common.ContactPoint()
		//				{
		//					ContactType = (string.IsNullOrWhiteSpace( m.TextTitle ) ? m.CodeTitle : m.TextTitle),
		//					Email = m.TextValue
		//				} )
		//			).ToList();
		//		}*/
		//		return results;
		//	} }

		public List<ContactPoint> Auto_TargetContactPointForDetail
		{
			get
			{
				var results = new List<ContactPoint>();

				var autoContact = new ContactPoint()
				{
					Name = "Organization Contact Information",
					PhoneNumbers = PhoneNumbers,
					Emails = Emails,
					SocialMediaPages = SocialMediaPages
				};
				results.Add( autoContact );

				if ( ContactPoint != null && ContactPoint.Count() > 0 )
				{
					results = results.Concat( ContactPoint ).ToList();
				}

				return results;
			}
		}
		public List<TextValueProfile> Emails { get; set; }

		//public List<TextValueProfile> Subjects { get; set; }
		public List<TextValueProfile> Keyword { get; set; }
		public List<TextValueProfile> AlternateName { get; set; }

		public List<ContactPoint> ContactPoint { get; set; }

		public List<TextValueProfile> IdentificationCodes { get; set; }
		public List<TextValueProfile> AlternativeIdentifiers { get; set; }
		public string AlternativeIdentifier { get; set; }

		//public List<IdentifierValue> Auto_AlternativeIdentifier
		//{
		//	get
		//	{
		//		var result = new List<IdentifierValue>();
		//		if ( !string.IsNullOrWhiteSpace( AlternativeIdentifier ) )
		//		{
		//			result.Add( new IdentifierValue()
		//			{
		//				IdentifierValueCode = AlternativeIdentifier
		//			} );
		//		}
		//		return result;
		//	}
		//}
		//Identifier Aliases used for publishing
		public string ID_DUNS { get { return IdentificationCodes.FirstOrDefault( m => m.CodeSchema == "ceterms:duns" )?.TextValue; } }
		public string ID_FEIN { get { return IdentificationCodes.FirstOrDefault( m => m.CodeSchema == "ceterms:fein" )?.TextValue; } }
		public string ID_IPEDSID { get { return IdentificationCodes.FirstOrDefault( m => m.CodeSchema == "ceterms:ipedsID" )?.TextValue; } }
		public string ID_OPEID { get { return IdentificationCodes.FirstOrDefault( m => m.CodeSchema == "ceterms:opeID" )?.TextValue; } }
        public string ID_LEICode { get { return IdentificationCodes.FirstOrDefault( m => m.CodeSchema == "ceterms:leiCode" )?.TextValue; } }
        //Used by detail page
        public List<IdentifierValue> ID_AlternativeIdentifier {
			get {
				return IdentificationCodes.Where( m => m.CodeSchema == "ceterms:alternativeIdentifier" ).ToList().ConvertAll( m =>
				new IdentifierValue()
				{
					IdentifierType = m.TextTitle,
					IdentifierValueCode = m.TextValue
				} );
			} }

	}
}
