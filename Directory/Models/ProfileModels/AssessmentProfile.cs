using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{

	public class AssessmentProfile : BaseProfile
	{
		public AssessmentProfile()
		{
			AssessmentType = new Enumeration();
			Modality = new Enumeration();
			DeliveryType = new Enumeration();
			OrganizationRole = new List<OrganizationRoleProfile>();
			QualityAssuranceAction = new List<QualityAssuranceActionProfile>();
			//AssessmentProcess = new List<ProcessProfile>();
			EstimatedCost = new List<CostProfile>();
			EstimatedDuration = new List<DurationProfile>();
			AssessmentExampleUrl = new List<TextValueProfile>();
			AssessmentExampleUrl2 = new List<string>();
			WhereReferenced = new List<string>();
			Subjects = new List<TextValueProfile>();
			Keywords = new List<TextValueProfile>();
			Addresses = new List<Address>();
			Requires = new List<ConditionProfile>();
			IsPartOfConditionProfile = new List<ConditionProfile>();
			
		}
		public string Name { get; set; }
		public int ManagingOrgId { get; set; }
		public string ManagingOrganization { get; set; }
		public int StatusId { get; set; }
		public string AssessedBy { get; set; } // url to organization
		public Enumeration AssessmentType { get; set; }
		public string OtherAssessmentType { get; set; }
		public Enumeration Modality { get; set; }
		public Enumeration DeliveryType { get; set; }
		public List<OrganizationRoleProfile> OrganizationRole { get; set; }
		public List<QualityAssuranceActionProfile> QualityAssuranceAction { get; set; }
		//public List<ProcessProfile> AssessmentProcess { get; set; }
		public List<CostProfile> EstimatedCost { get; set; }
		public List<DurationProfile> EstimatedDuration { get; set; }
		public List<TextValueProfile> AssessmentExampleUrl { get; set; }
		public List<string> AssessmentExampleUrl2 { get; set; }
		public List<TextValueProfile> Subjects { get; set; }
		public List<TextValueProfile> Keywords { get; set; }
		public List<CredentialAlignmentObjectProfile> AssessesCompetencies { get; set; }
		public List<CredentialAlignmentObjectProfile> RequiresCompetencies { get; set; }
		public List<CredentialAlignmentObjectFrameworkProfile> AssessesCompetenciesFrameworks { get; set; }
		public List<CredentialAlignmentObjectFrameworkProfile> RequiresCompetenciesFrameworks { get; set; }

		public string Url { get; set; }
		public string AvailableOnlineAt { get; set; }
		public string IdentificationCode { get; set; }
		public string CreatedByOrganization { get; set; }
		public int CreatedByOrganizationId { get; set; }
		public List<string> WhereReferenced { get; set; }
		public List<ConditionProfile> IsPartOfConditionProfile { get; set; }
		public List<Address> Addresses { get; set; }
		public List<ConditionProfile> Requires { get; set; }
	}
	//

}
