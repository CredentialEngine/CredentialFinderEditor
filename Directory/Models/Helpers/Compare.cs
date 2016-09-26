using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;

namespace Models.Helpers
{

	public class CompareItem
	{
		public string Type { get; set; }
		public int Id { get; set; }
		public string Title { get; set; }
	}
	//

	public class CompareItemSummary
	{
		public CompareItemSummary()
		{
			Credentials = new List<Credential>();
			Organizations = new List<Organization>();
			LearningOpportunities = new List<LearningOpportunityProfile>();
			Assessments = new List<AssessmentProfile>();
		}
		public List<Credential> Credentials { get; set; }
		public List<Organization> Organizations { get; set; }
		public List<LearningOpportunityProfile> LearningOpportunities { get; set; }
		public List<AssessmentProfile> Assessments { get; set; }

		//Get the value of a property for each object in a list - useful for ensuring data goes with the appropriate object even when one object in the list has no data for that property
		public static List<object> GetData<T>( string propertyName, List<T> sources )
		{
			var result = new List<object>();
			try
			{
				foreach ( var item in sources )
				{
					result.Add( item.GetType().GetProperties().FirstOrDefault( m => m.Name == propertyName ).GetValue( item ) );
				}
			}
			catch
			{
				return result;
			}

			return result;
		}
		//

		public static object GetData( string propertyName, object source )
		{
			var result = new object();
			try
			{
				return source.GetType().GetProperties().FirstOrDefault( m => m.Name == propertyName ).GetValue( source );
			}
			catch
			{
				return result;
			}
		}
		//
	}
	//


}
