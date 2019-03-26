using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.ProfileModels
{
	public partial class Entity_Competency
	{
		public Entity_Competency()
		{
			FrameworkCompetency = new EducationFrameworkCompetency();
		}
		public int Id { get; set; }
		public int EntityId { get; set; }
		public int CompetencyId { get; set; }
		public DateTime Created { get; set; }
		public int CreatedById { get; set; }
		public string Uri { get; set; }
		public string CTID { get; set; }

		#region External Properties
		public int FrameworkCompetencyId
		{
			get
			{
				if ( FrameworkCompetency == null )
					return 0;
				else
					return FrameworkCompetency.Id;
			}
		}
		public int EducationFrameworkId
		{
			get
			{
				if ( FrameworkCompetency == null )
					return 0;
				else
					return FrameworkCompetency.EducationFrameworkId;
			}
		}
		public string RepositoryUri
		{
			get
			{
				if ( FrameworkCompetency == null )
					return "";
				else
					return FrameworkCompetency.RepositoryUri;
			}
		}
		public string Name
		{
			get {
				if ( FrameworkCompetency == null )
					return "";
				else
					return FrameworkCompetency.Name;
				}
		}
		public string Description
		{
			get
			{
				if ( FrameworkCompetency == null )
					return "";
				else
					return FrameworkCompetency.Description;
			}
		}
		public string Url
		{
			get
			{
				if ( FrameworkCompetency == null )
					return "";
				else
					return FrameworkCompetency.Url;
			}
		}
		public string CodedNotation
		{
			get
			{
				if ( FrameworkCompetency == null )
					return "";
				else
					return FrameworkCompetency.CodedNotation;
			}
		}

		public EducationFramework EducationFramework
		{
			get
			{
				if ( FrameworkCompetency == null 
				  || FrameworkCompetency.EducationFramework  == null)
					return null;
				else
					return FrameworkCompetency.EducationFramework;
			}
		}

		public EducationFrameworkCompetency FrameworkCompetency { get; set; }

		#endregion
	}
}
