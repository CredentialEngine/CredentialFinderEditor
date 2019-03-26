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

			type = "ceterms:Credential";
		}

		//Basic Properties
		[DataMember( Name = "ceterms:dateEffective" )]
		public string dateEffective { get; set; } //ISO date string

		[DataMember( Name = "ceterms:latestVersion" )]
		public string latestVersion { get; set; } //URL

		[DataMember( Name = "ceterms:replacesVersion" )]
		public string replacesVersion { get; set; } //URL

		[DataMember( Name = "ceterms:versionIdentifier" )]
		public string versionIdentifier { get; set; }

		[DataMember( Name = "ceterms:ctid" )]
		public string ctid { get; set; }

		[DataMember( Name = "dc:hasPart" )]
		public List<string> hasPart { get; set; } //Credential URLs

		[DataMember( Name = "dc:isPartOf" )]
		public List<string> isPartOf { get; set; } //Credential URLs

		[DataMember( Name = "ceterms:description" )]
		public string description { get; set; }

		[DataMember( Name = "ceterms:alternateName" )]
		public string alternateName { get; set; }

		[DataMember( Name = "ceterms:image" )]
		public string image { get; set; } //Image URL

		[DataMember( Name = "ceterms:name" )]
		public string name { get; set; }

		[DataMember( Name = "ceterms:url" )]
		public string url { get; set; } //URL


		//Organization Roles
		[DataMember( Name = "ceterms:creator" )]
		public List<string> creator { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ceterms:owner" )]
		public List<string> owner { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ceterms:updatedVersionBy" )]
		public List<string> updatedVersionBy { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ceterms:verifiedBy" )]
		public List<string> verifiedBy { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ceterms:assessedBy" )]
		public List<string> assessedBy { get; set; } //Organization URL, Person URL

		[DataMember( Name = "ceterms:offeredBy" )]
		public List<string> offeredBy { get; set; } //Organization URL, Person URL


		//Quality Assurance Roles
		[DataMember( Name = "ceterms:accreditedBy" )]
		public List<string> accreditedBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL
		[DataMember( Name = "ceterms:accreditedByAction" )]
		public List<QualityAssuranceAction> accreditedByAction { get; set; }

		[DataMember( Name = "ceterms:approvedBy" )]
		public List<string> approvedBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL
		[DataMember( Name = "ceterms:approvedByAction" )]
		public List<QualityAssuranceAction> approvedByAction { get; set; }

		[DataMember( Name = "ceterms:conferredBy" )]
		public List<string> conferredBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL
		[DataMember( Name = "ceterms:conferredByAction" )]
		public List<QualityAssuranceAction> conferredByAction { get; set; }

		[DataMember( Name = "ceterms:endorsedBy" )]
		public List<string> endorsedBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL
		[DataMember( Name = "ceterms:endorsedByAction" )]
		public List<QualityAssuranceAction> endorsedByAction { get; set; }

		[DataMember( Name = "ceterms:recognizedBy" )]
		public List<string> recognizedBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL
		[DataMember( Name = "ceterms:recognizedByAction" )]
		public List<QualityAssuranceAction> recognizedByAction { get; set; }

		[DataMember( Name = "ceterms:regulatedBy" )]
		public List<string> regulatedBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL
		[DataMember( Name = "ceterms:regulatedByAction" )]
		public List<QualityAssuranceAction> regulatedByAction { get; set; }

		[DataMember( Name = "ceterms:revocationBy" )]
		public List<string> revocationBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL
		[DataMember( Name = "ceterms:revocationByAction" )]
		public List<QualityAssuranceAction> revocationByAction { get; set; }

		[DataMember( Name = "ceterms:renewalBy" )]
		public List<string> renewalBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL
		[DataMember( Name = "ceterms:renewalByAction" )]
		public List<QualityAssuranceAction> renewalByAction { get; set; }

		[DataMember( Name = "ceterms:validatedBy" )]
		public List<string> validatedBy { get; set; } //QualityAssuranceAction, Organization URL, Person URL
		[DataMember( Name = "ceterms:validatedByAction" )]
		public List<QualityAssuranceAction> validatedByAction { get; set; }

		[DataMember( Name = "ceterms:contributor" )]
		public List<string> contributor { get; set; } //QualityAssuranceAction, Organization URL, Person URL
		[DataMember( Name = "ceterms:contributorAction" )]
		public List<QualityAssuranceAction> contributorAction { get; set; }


		//Enumerations
		[DataMember( Name = "ceterms:credentialLevel" )]
		public List<string> credentialLevel { get; set; }

		[DataMember( Name = "ceterms:credentialType" )]
		public List<string> credentialType { get; set; }

		[DataMember( Name = "ceterms:purpose" )]
		public List<string> purpose { get; set; }


		//Profiles
		[DataMember( Name = "ceterms:RecommendedFor" )]
		public List<ConditionProfile> isRecommendedFor { get; set; }

		[DataMember( Name = "ceterms:RequiredFor" )]
		public List<ConditionProfile> isRequiredFor { get; set; }

		[DataMember( Name = "ceterms:recommends" )]
		public List<ConditionProfile> recommends { get; set; }

		[DataMember( Name = "ceterms:renewal" )]
		public List<ConditionProfile> renewal { get; set; }

		[DataMember( Name = "ceterms:requires" )]
		public List<ConditionProfile> requires { get; set; }

		[DataMember( Name = "ceterms:industryCategory" )]
		public List<Enumeration> industryCategory { get; set; }

		[DataMember( Name = "ceterms:occupationCategory" )]
		public List<Enumeration> occupationCategory { get; set; }

		[DataMember( Name = "ceterms:developmentProcess" )]
		public List<ProcessProfile> developmentProcess { get; set; }

		[DataMember( Name = "ceterms:maintenanceProcess" )]
		public List<ProcessProfile> maintenanceProcess { get; set; }

		[DataMember( Name = "ceterms:selectionProcess" )]
		public List<ProcessProfile> selectionProcess { get; set; }

		[DataMember( Name = "ceterms:validationProcess" )]
		public List<ProcessProfile> validationProcess { get; set; }

		[DataMember( Name = "ceterms:estimatedTimeToEarn" )]
		public List<DurationProfile> estimatedTimeToEarn { get; set; }

		[DataMember( Name = "ceterms:jurisdiction" )]
		public List<JurisdictionProfile> jurisdiction { get; set; }

		[DataMember( Name = "ceterms:removal" )]
		public List<RevocationProfile> revocation { get; set; }

		[DataMember( Name = "ceterms:earnings" )]
		public List<EarningsProfile> earnings { get; set; }

		[DataMember( Name = "ceterms:employmentOutcome" )]
		public List<EmploymentOutcomeProfile> employmentOutcome { get; set; }

		//[DataMember( Name = "ceterms:holders" )]
		//public List<HoldersProfile> holders { get; set; }

		//Temporary
		[DataMember(Name = "ceterms:industryCategory_Flat")]
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

		[DataMember( Name = "ceterms:name" )]
		public string name { get; set; }

		[DataMember( Name = "ceterms:url" )]
		public string url { get; set; }

		[DataMember( Name = "unknown:frameworkName" )]
		public string frameworkName { get; set; }

		[DataMember( Name = "unknown:frameworkUrl" )]
		public string frameworkUrl { get; set; }
	}
}
