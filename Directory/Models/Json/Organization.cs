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

			type = "ctdl:Organization";
		}


		//Basic Properties
		[DataMember( Name = "schema:description" )]
		public string description { get; set; }

		[DataMember( Name = "schema:image" )]
		public string image { get; set; } //Image URL

		[DataMember( Name = "schema:name" )]
		public string name { get; set; }

		[DataMember( Name = "schema:url" )]
		public string url { get; set; } //URL

		[DataMember( Name = "ctdl:ctid" )]
		public string ctid { get; set; }

		[DataMember( Name = "schema:email" )]
		public string email { get; set; } //Email

		[DataMember( Name = "schema:fein" )]
		public string fein { get; set; }

		[DataMember( Name = "ctdl:identifier" )]
		public List<object> identifier { get; set; }

		[DataMember( Name = "ctdl:opeid" )]
		public string opeid { get; set; }

		[DataMember( Name = "ctdl:versioning" )]
		public string versioning { get; set; }

		[DataMember( Name = "schema:duns" )]
		public string duns { get; set; }

		[DataMember( Name = "schema:foundingDate" )]
		public string foundingDate { get; set; }

		[DataMember( Name = "schema:naics" )]
		public string naics { get; set; }

		[DataMember( Name = "schema:purpose" )]
		public string purpose { get; set; }

		[DataMember( Name = "schema:sameAs" )]
		public List<string> sameAs { get; set; }



		//Organization Roles
		[DataMember( Name = "ctdl:creatorOf" )]
		public List<string> creatorOf { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ctdl:owns" )]
		public List<string> owns { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ctdl:updatesVersion" )]
		public List<string> updatesVersion { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ctdl:verifies" )]
		public List<string> verifies { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ctdl:assesses" )]
		public List<string> assesses { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ctdl:offersCredential" )]
		public List<string> offersCredential { get; set; } //Organization URL, Person URL

		[DataMember( Name = "schema:employee" )]
		public List<string> employee { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ctdl:trainingOffered" )]
		public List<string> trainingOffered { get; set; } //URLs


		//Quality Assurance Roles
		[DataMember( Name = "ctdl:accredits" )]
		public object accredits { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:approves" )]
		public object approves { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:confers" )]
		public object confers { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "schema:contributorTo" )]
		public object contributorTo { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:endorses" )]
		public object endorses { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "schema:potentialAction" )]
		public object potentialAction { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:recognizes" )]
		public object recognizes { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:regulates" )]
		public object regulates { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:revokes" )]
		public object revokes { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:renews" )]
		public object renews { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:validates" )]
		public object validates { get; set; } //QualityAssuranceAction, Organization URL, Person URL


		//Enumerations
		[DataMember( Name = "ctdl:agentCategory" )]
		public List<string> agentCategory { get; set; } //Organization Type

		[DataMember( Name = "schema:serviceType" )]
		public List<string> serviceType { get; set; }



		//Profiles
		[DataMember( Name = "ctdl:agentProcess" )]
		public List<ProcessProfile> agentProcess { get; set; }

		[DataMember( Name = "ctdl:authenticationService" )]
		public List<AuthenticationProfile> authenticationService { get; set; }

		[DataMember( Name = "ctdl:jurisdiction" )]
		public List<JurisdictionProfile> jurisdiction { get; set; }

		[DataMember( Name = "schema:address" )]
		public PostalAddress address { get; set; }

		[DataMember( Name = "schema:contactPoint" )]
		public object contactPoint { get; set; }



	}
	//
}
