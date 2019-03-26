using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HelperModels = Models.Helpers;
using Models.Node.Interface;

namespace Models.Node
{
	public class CassInput
	{
		public CassInput()
		{
			Context = new ProfileContext();
			Competencies = new List<HelperModels.Cass.CassCompetency>();
			Framework = new HelperModels.Cass.CassFramework();
		}
		public ProfileContext Context { get; set; }
		public List<HelperModels.Cass.CassCompetency> Competencies { get; set; }
		public HelperModels.Cass.CassFramework Framework { get; set; }
	}

	public class CassInputV2
	{
		public CassInputV2()
		{

		}

		public ProfileContext Context { get; set; }
		public List<HelperModels.Cass.CassCompetencyV2> Competencies { get; set; }
	}
}
