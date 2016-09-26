using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Models.Json
{
	
	[DataContract]
	public class Credential : JsonLDDocument
	{
		public Credential()
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

			type = "ctdl:Credential";
		}

		//Basic Properties
		[DataMember( Name = "ctdl:dateEffective" )]
		public string dateEffective { get; set; } //ISO date string

		[DataMember( Name = "ctdl:latestVersion" )]
		public string latestVersion { get; set; } //URL

		[DataMember( Name = "ctdl:replacesVersion" )]
		public string replacesVersion { get; set; } //URL

		[DataMember( Name = "ctdl:versionIdentifier" )]
		public string versionIdentifier { get; set; }

		[DataMember( Name = "ctdl:ctid" )]
		public string ctid { get; set; }

		[DataMember( Name = "dc:hasPart" )]
		public List<string> hasPart { get; set; } //Credential URLs

		[DataMember( Name = "dc:isPartOf" )]
		public List<string> isPartOf { get; set; } //Credential URLs

		[DataMember( Name = "schema:description" )]
		public string description { get; set; }

		[DataMember( Name = "schema:alternateName" )]
		public string alternateName { get; set; }

		[DataMember( Name = "schema:image" )]
		public string image { get; set; } //Image URL

		[DataMember( Name = "schema:name" )]
		public string name { get; set; }

		[DataMember( Name = "schema:url" )]
		public string url { get; set; } //URL


		//Organization Roles
		[DataMember( Name = "schema:creator" )]
		public List<string> creator { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ctdl:owner" )]
		public List<string> owner { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ctdl:updatedVersionBy" )]
		public List<string> updatedVersionBy { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ctdl:verifiedBy" )]
		public List<string> verifiedBy { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ctdl:assessedBy" )]
		public List<string> assessedBy { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ctdl:offeredBy" )]
		public List<string> offeredBy { get; set; } //Organization URL, Person URL


		//Quality Assurance Roles
		[DataMember( Name="ctdl:accreditedBy" )]
		public object accreditedBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:approvedBy" )]
		public object approvedBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:conferredBy" )]
		public object conferredBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:endorsedBy" )]
		public object endorsedBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:recognizedBy" )]
		public object recognizedBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:regulatedBy" )]
		public object regulatedBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:revocationBy" )]
		public object revocationBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:renewalBy" )]
		public object renewalBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "ctdl:validatedBy" )]
		public object validatedBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL

		[DataMember( Name = "schema:contributor" )]
		public object contributor { get; set; } //QualityAssuranceAction, Organization URL, Person URL


		//Enumerations
		[DataMember( Name = "ctdl:credentialLevel" )]
		public List<string> credentialLevel { get; set; }

		[DataMember( Name = "ctdl:credentialType" )]
		public List<string> credentialType { get; set; }

		[DataMember( Name = "ctdl:purpose" )]
		public List<string> purpose { get; set; }


		//Profiles
		[DataMember( Name = "ctdl:RecommendedFor" )]
		public List<ConditionProfile> isRecommendedFor { get; set; }

		[DataMember( Name = "ctdl:RequiredFor" )]
		public List<ConditionProfile> isRequiredFor { get; set; }

		[DataMember( Name = "ctdl:recommends" )]
		public List<ConditionProfile> recommends { get; set; }

		[DataMember( Name = "ctdl:renewal" )]
		public List<ConditionProfile> renewal { get; set; }

		[DataMember( Name = "ctdl:requires" )]
		public List<ConditionProfile> requires { get; set; }

		[DataMember( Name = "ctdl:industryCategory" )]
		public List<Enumeration> industryCategory { get; set; }

		[DataMember( Name = "ctdl:occupationCategory" )]
		public List<Enumeration> occupationCategory { get; set; }

		[DataMember( Name = "ctdl:developmentProcess" )]
		public List<ProcessProfile> developmentProcess { get; set; }

		[DataMember( Name = "ctdl:maintenanceProcess" )]
		public List<ProcessProfile> maintenanceProcess { get; set; }

		[DataMember( Name = "ctdl:selectionProcess" )]
		public List<ProcessProfile> selectionProcess { get; set; }

		[DataMember( Name = "ctdl:validationProcess" )]
		public List<ProcessProfile> validationProcess { get; set; }

		[DataMember( Name = "ctdl:estimatedTimeToEarn" )]
		public List<DurationProfile> estimatedTimeToEarn { get; set; }

		[DataMember( Name = "ctdl:jurisdiction" )]
		public List<JurisdictionProfile> jurisdiction { get; set; }

		[DataMember( Name = "ctdl:removal" )]
		public List<RevocationProfile> revocation { get; set; }

		[DataMember( Name = "ctdl:earnings" )]
		public List<EarningsProfile> earnings { get; set; }

		[DataMember( Name = "ctdl:employmentOutcome" )]
		public List<EmploymentOutcomeProfile> employmentOutcome { get; set; }

		[DataMember( Name = "ctdl:holders" )]
		public List<HoldersProfile> holders { get; set; }

		//Temporary
		[DataMember(Name = "ctdl:industryCategory_Flat")]
		public List<TemporaryEnumerationItem> industryCategoryFlat { get; set; }
	}

	//Temporary
	[DataContract]
	public class TemporaryEnumerationItem : JsonLDObject
	{
		public TemporaryEnumerationItem()
		{
			type = "unknown:EnumerationItem";
		}

		[DataMember( Name = "schema:name" )]
		public string name { get; set; }

		[DataMember( Name = "schema:url" )]
		public string url { get; set; }

		[DataMember( Name = "unknown:frameworkName" )]
		public string frameworkName { get; set; }

		[DataMember( Name = "unknown:frameworkUrl" )]
		public string frameworkUrl { get; set; }
	}
}
