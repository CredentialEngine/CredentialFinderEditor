using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Data.Development
{
	//Temporary holder for sets of enumerations
	public static class TemporaryEnumerationData
	{
		public static Enumeration GetEnumeration( string dataSource )
		{
			return GetEnumeration( dataSource, dataSource );
		}
		public static Enumeration GetEnumeration( string dataSource, string schemaName )
		{
			var result = new Enumeration();
			result.Items = GetEnumerationItems( dataSource );
			result.SchemaName = schemaName;
			switch ( dataSource )
			{
				case "sample":
					result.Name = "Sample Enumeration";
					break;
				case "timeToEarn":
					result.Name = "Time To Earn";
					break;
				case "jurisdiction":
					result.Name = "Jurisdiction";
					break;
				case "purpose":
					result.Name = "Purpose";
					break;
				case "organizationRoleProfile_roleType":
					result.Name = "Organization Role";
					break;
				default: 
					result.Name = "Enumeration Name Here";
					break;
			}

			return result;
		}
		public static List<EnumeratedItem> GetEnumerationItems( string dataSource )
		{
			switch ( dataSource )
			{
				case "sample":
					return MakeFakeList( new List<string>() { "Sample 1", "Sample 2", "Sample 3" } );

				case "timeToEarn":
					return MakeFakeList( new List<string>() { "Less than one month", "1-6 months", "6-12 months",	"1-2 years", "2-4 years",	"4-6 years", "Longer than 6 years" } );

				case "jurisdiction":
					return MakeFakeList( new List<string>()	{	"Sub-state or Metropolitan Region",	"State or Province", "Sub-national Region",	"National",	"International", "Other" } );

				case "purpose":
					return MakeFakeList( new List<string>()	{	"Employment and career preparation and advancement", "Entry level within an occupation", "Mid-career level within an occupation", "Senior career level within an occupation", "Used as a speciality within an occupation", "General education including civic and cultural engagement", "Preparation for higher level credentials in a related discipline", "Other" } );

				case "organizationRoleProfile_roleType":
					return MakeFakeList( new List<string>()	{	"Accredit", "Approve", "Assure Quality", "Confer", "Creator", "Owner", "Endorse", "Offer Assessment", "Offer Credential", "Offer Training", "Recognize", "Regulate", "Remove", "Renew", "Version" } );

				case "costProfile_costType":
					return MakeFakeList( new List<string>() { "Application Fees", "Background Check", "Learning Resource Fees (e.g., Textbooks and Online Materials)", "Standalone Assessments", "Tuition Fees", "Renewal Fees", "Other" } );

				case "processProfile_externalInput":
					return MakeFakeList( new List<string>() { "Industry subject matter experts", "Industry employers", "Industry practitioners", "Industry professional associations", "Education administrators", "Education faculty", "Education students", "Consumers", "Federal/National government", "State/province government", "Local government", "Other" } );

				case "processProfile_processMethod":
					return MakeFakeList( new List<string>() { "Advisory group meetings", "Subject matter expert focus groups and meetings", "Subject matter expert reviews", "Validation surveys", "Other" } );

				case "processProfile_processType":
					return MakeFakeList( new List<string>() { "Selection", "Development", "Validation", "Maintenance" } );

				case "recognitionProfile_recognitionType":
					return MakeFakeList( new List<string>() { "Employers validated importance of competencies in credential", "Employers give preferences in hiring for those with credential", "Employed at a higher than normal level", "Employers actively recruit credential holders", "Employers \"endorse or recognize\" the credential as having value", "Other" } );

				case "removalProfile_removalCriteria":
					return MakeFakeList( new List<string>() { "Failure to maintain good standing in the occupation/profession ( disciplinary action )", "Failure to continue working in the profession/occupation addressed in the credential", "Other" } );

				case "renewalProfile_renewalCriteria":
					return MakeFakeList( new List<string>() { "Continuing Educational Units (CEU)", "Re-examination", "Continuing Professional Development (CPD)", "Continued work experience", "Continued professional activities", "Other" } );

				case "credentialTypeProfile_credentialType":
					return MakeFakeList( new List<string>() { "Non-Registered Apprenticeship", "Registered Apprenticeship", "Job Skills Certificate", "Industry Certificate", "Certification", "Digital Badge", "Diploma", "License", "Micro-Credential", "Associate's Degree", "Bachelor's Degree", "Master's Degree", "Doctor's Degree", "Other" } );

				case "credentialTypeProfile_credentialLevel":
					return MakeFakeList( new List<string>() { "Kindergarten", "Elementary School", "Middle School", "High School", "Postsecondary (Less than 1 year)", "Postsecondary (1-3 years)", "Postsecondary (3-6 years)", "Postsecondary (6+ years)", "Technical" } );

				case "transferValueProfile_transferValueType":
					return MakeFakeList( new List<string>() { "Eligibility to apply for other credentials", "Advanced standing for another credential", "Bundled into more comprehensive credential", "Preparation for another credential without formal agreements on other transfer value relationships", "Other" } );

				case "assessmentProfile_assessmentType":
					return MakeFakeList( new List<string>() { "Capstone Projects or Assessments", "Class Participation Observation", "Classroom Assignments", "Multiple Choice/Short Answer Exams", "Observed and Rated Performance", "On-the-job or supervisor evaluation", "Oral Exams/Presentations", "Portfolios", "Projects or Demonstrations", "Written Exams (other than short-answer) Using Scoring Rubrics", "Other" } );

				case "courseProfile_courseType":
					return MakeFakeList( new List<string>() { "Broadcast", "Correspondence", "Early College", "Interactive Audio/Video", "Online", "Independent Study", "Face to Face", "Blended Learning", "Other" } );

				case "organizationType":
					return MakeFakeList( new List<string>() { "Adult Education Provider ", "Certification Organization", "Consortium ", "Elementary Education Entity ", "Employer ", "Government Agency ", "Industry Trade Association ", "Labor or Trade Union ", "Local Education Agency ", "Middle School Education Organization", "Secondary (High School) Education Organization", "Military Branch", "Postsecondary Educational Institution (Private)", "Postsecondary Educational Institution (Public)", "Product or Service Vendor", "Professional Member Organization", "Other" } );

				case "organizationRoleProfile_targetType":
					return MakeFakeList( new List<string>() { "Credential", "Assessment", "Competency Framework", "Competency", "Training Program", "Organization", "Person" } );

				case "qualityAssuranceProfile_assuranceResult":
					return MakeFakeList( new List<string>() { "Report of a major change", "Report of a major incident", "Site visit", "Written report(s) from the provider", "Other" } );

				case "qualityAssuranceProfile_staffEvaluationType":
					return MakeFakeList( new List<string>() { "Internal Evaluations", "External Evaluations by customers", "Peer Review mechanisms", "Other" } );

				case "qualityAssuranceProfile_credentialAssuranceType":
					return MakeFakeList( new List<string>() { "Business/Industry Organization", "Federal Government Agency", "National/International Certification or Education Certificate Accreditation Organization", "Professional Organization", "Specialized Higher Education Accreditation Agency", "State Government Agency", "Other" } );

				case "qualityAssuranceProfile_organizationAssuranceType":
					return MakeFakeList( new List<string>() { "National Higher Education Accreditation Organization", "Regional Higher Education Accreditation Organization", "Federal Government Agency", "State Government Agency", "Business/Industry Organization", "Professional Organization", "National/International Certification or Educational Certificate Accreditation Organization", "Other" } );

				case "qualityAssuranceProfile_outcomeReviewType":
					return MakeFakeList( new List<string>() { "Competencies achieved as claimed", "Competencies demonstrated in practice", "Program outcomes", "Institutional outcomes", "Individual demonstrated achievement", "Program completion", "Certification or Licensure", "Obtaining employment", "Other" } );

				case "qualityAssuranceProfile_purpose":
					return MakeFakeList( new List<string>() { "Continuous quality improvement beyond minimum requirements", "Monitoring", "Verifying/Validating", "Compliance", "Fulfillment of a standard", "Other" } );

				case "qualityAssuranceProfile_useType":
					return MakeFakeList( new List<string>() { "Accepted for credit transfer or recognition", "Assists employers seeking competent practitioners", "Enables eligibility for financial aid support from employer tuition assistance", "Enables eligibility for financial aid support from federal agencies", "Enables eligibility for financial aid support from foundations for those programs that do not have regional or national presences", "Enables eligibility for financial aid support from state agencies", "Entry to practice", "Federal, state, or local requirements", "Fulfills the eligibility requirement for applicants seeking advanced certification or degrees", "Promotes professional advancement and career mobility of program graduates", "Other" } );

				default:
					return MakeFakeList( new List<string>() { "Item List Here" } );
			};

		}
		//

		private static List<EnumeratedItem> MakeFakeList( List<string> names )
		{
			var result = new List<EnumeratedItem>();
			var counter = 0;
			foreach ( var item in names )
			{
				result.Add( new EnumeratedItem() { Name = item, SchemaName = item.ToLower().Replace( " ", "_" ).Replace( "-", "_" ).Replace( "/", "_" ).Replace( ".", "_" ).Replace( "'", "_" ).Replace( "\"", "_" ), Value = counter.ToString() } );
				counter++;
			}
			return result;
		}
	}
	//

}
