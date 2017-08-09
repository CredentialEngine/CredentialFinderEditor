using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Models.Json
{
	[DataContract]
	public class Organization : JsonLDDocument
	{
		public Organization()
		{
			foreach ( var item in this.GetType().GetProperties() )
			{
				if ( item == null )
				{
					if ( item.PropertyType == typeof( string ) )
					{
						item.SetValue( this, "" );
					}
					else
					{
						item.SetValue( this, Activator.CreateInstance( item.PropertyType ) );
					}
				}
			}

			type = "ceterms:Organization";
		}


		//Basic Properties
		[DataMember( Name = "ceterms:description" )]
		public string description { get; set; }

		[DataMember( Name = "ceterms:image" )]
		public string image { get; set; } //Image URL

		[DataMember( Name = "ceterms:name" )]
		public string name { get; set; }

		[DataMember( Name = "ceterms:url" )]
		public string url { get; set; } //URL

		[DataMember( Name = "ceterms:ctid" )]
		public string ctid { get; set; }

		[DataMember( Name = "ceterms:email" )]
		public string email { get; set; } //Email

		[DataMember( Name = "ceterms:fein" )]
		public string fein { get; set; }

		[DataMember( Name = "ceterms:identifier" )]
		public List<object> identifier { get; set; }

		[DataMember( Name = "ceterms:opeid" )]
		public string opeid { get; set; }

		[DataMember( Name = "ceterms:versioning" )]
		public string versioning { get; set; }

		[DataMember( Name = "ceterms:duns" )]
		public string duns { get; set; }

		[DataMember( Name = "ceterms:foundingDate" )]
		public string foundingDate { get; set; }

		[DataMember( Name = "ceterms:naics" )]
		public string naics { get; set; }

		[DataMember( Name = "ceterms:purpose" )]
		public string purpose { get; set; }

		[DataMember( Name = "ceterms:sameAs" )]
		public List<string> sameAs { get; set; }



		//Organization Roles
		[DataMember( Name = "ceterms:creatorOf" )]
		public List<string> creatorOf { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ceterms:owns" )]
		public List<string> owns { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ceterms:updatesVersion" )]
		public List<string> updatesVersion { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ceterms:verifies" )]
		public List<string> verifies { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ceterms:assesses" )]
		public List<string> assesses { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ceterms:offersCredential" )]
		public List<string> offersCredential { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ceterms:employee" )]
		public List<string> employee { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ceterms:trainingOffered" )]
		public List<string> trainingOffered { get; set; } //URLs


		//Quality Assurance Roles
		[DataMember( Name = "ceterms:accredits" )]
		public object accredits { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ceterms:approves" )]
		public object approves { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ceterms:confers" )]
		public object confers { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ceterms:contributorTo" )]
		public object contributorTo { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ceterms:endorses" )]
		public object endorses { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ceterms:potentialAction" )]
		public object potentialAction { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ceterms:recognizes" )]
		public object recognizes { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ceterms:regulates" )]
		public object regulates { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ceterms:revokes" )]
		public object revokes { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ceterms:renews" )]
		public object renews { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ceterms:validates" )]
		public object validates { get; set; } //QualityAssuranceAction, Organization URL, Person URL


		//Enumerations
		[DataMember( Name = "ceterms:agentCategory" )]
		public List<string> agentCategory { get; set; } //Organization Type

		[DataMember( Name = "ceterms:serviceType" )]
		public List<string> serviceType { get; set; }



		//Profiles
		[DataMember( Name = "ceterms:agentProcess" )]
		public List<ProcessProfile> agentProcess { get; set; }

		[DataMember( Name = "ceterms:authenticationService" )]
		public List<AuthenticationProfile> authenticationService { get; set; }

		[DataMember( Name = "ceterms:jurisdiction" )]
		public List<JurisdictionProfile> jurisdiction { get; set; }

		[DataMember( Name = "ceterms:address" )]
		public PostalAddress address { get; set; }

		[DataMember( Name = "ceterms:contactPoint" )]
		public object contactPoint { get; set; }



	}
	//
}
