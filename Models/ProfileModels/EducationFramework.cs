using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ProfileModels
{
	public class EducationFramework
	{
		public EducationFramework()
		{
			EducationFrameworkCompetencies = new List<EducationFrameworkCompetency>();
		}

		public int Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string FrameworkUrl { get; set; }
		public string RepositoryUri { get; set; }
        public string CTID { get; set; }
        public System.Guid RowId { get; set; }
		public System.DateTime Created { get; set; }
		public int CreatedById { get; set; }
		public int OwningOrganizationId { get; set; }

		public List<EducationFrameworkCompetency> EducationFrameworkCompetencies { get; set; }
		
	}

	public class EducationFrameworkCompetency
	{
		public EducationFrameworkCompetency()
		{
			
		}
		public int Id { get; set; }
		public int EducationFrameworkId { get; set; }
		public string RepositoryUri { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Url { get; set; }
		public string CodedNotation { get; set; }
        public string CTID { get; set; }
        public System.Guid RowId { get; set; }
		public System.DateTime Created { get; set; }
		public int CreatedById { get; set; }

		public EducationFramework EducationFramework { get; set; }
	}
}
