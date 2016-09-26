using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Models.ProfileModels
{

	public class LearningOpportunityProfile : BaseProfile
	{
		public LearningOpportunityProfile()
		{
			Provider = new Organization();
			LearningResourceUrl = new List<TextValueProfile>();
			LearningResourceUrl2 = new List<string>();
			
			EstimatedCost = new List<CostProfile>();
			EstimatedDuration = new List<DurationProfile>();
			LearningOpportunityDeliveryType = new Enumeration();
			InstructionalProgramCategory = new Enumeration();
			HasPart = new List<LearningOpportunityProfile>();
			IsPartOf = new List<LearningOpportunityProfile>();
			OrganizationRole = new List<OrganizationRoleProfile>();
			QualityAssuranceAction = new List<QualityAssuranceActionProfile>();
			WhereReferenced = new List<string>();
			//LearningCompetencies = new List<TextValueProfile>();
			Subjects = new List<TextValueProfile>();
			Keywords = new List<TextValueProfile>();
			Addresses = new List<Address>();
		}

		public string Name { get; set; }
		public string Url { get; set; }
		public string AvailableOnlineAt { get; set; }
		public int StatusId { get; set; }
		public int ManagingOrgId { get; set; }
		public string ManagingOrganization { get; set; }
		public string CreatedByOrganization { get; set; }
		public int CreatedByOrganizationId { get; set; }
		public Organization Provider { get; set; }
		//public int ProviderId { get; set; }
		public Guid ProviderUid { get; set; }
		public string IdentificationCode { get; set; }

		[Obsolete]
		public List<string> LearningResourceUrl2 { get; set; }
		public List<TextValueProfile> LearningResourceUrl { get; set; }
		public List<CostProfile> EstimatedCost { get; set; }
		public List<DurationProfile> EstimatedDuration { get; set; }
		public Enumeration LearningOpportunityDeliveryType { get; set; }
		public Enumeration InstructionalProgramCategory { get; set; }

		//public List<TextValueProfile> LearningCompetencies { get; set; }
		public List<CredentialAlignmentObjectProfile> TeachesCompetencies { get; set; }
		public List<CredentialAlignmentObjectProfile> RequiresCompetencies { get; set; }
		public List<LearningOpportunityProfile> HasPart { get; set; }
		public List<LearningOpportunityProfile> IsPartOf { get; set; }
		public List<OrganizationRoleProfile> OrganizationRole { get; set; }
		public List<QualityAssuranceActionProfile> QualityAssuranceAction { get; set; }
		public List<TextValueProfile> Keywords { get; set; }
		public List<TextValueProfile> Subjects { get; set; }
		public List<string> WhereReferenced { get; set; }
		public List<Address> Addresses { get; set; }
	}
	//

}
