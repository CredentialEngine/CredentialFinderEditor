using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Helpers.Review
{
	public class ReviewWrapper
	{
		public string EntityType { get; set; }
		public string EntityTypeTitle { get; set; }
		public int EntityId { get; set; }
	}
	//

	public class Schema
	{
		public Schema()
		{
			foreach ( var prop in this.GetType().GetProperties().Where( m => m.PropertyType == typeof( List<MetadataTerm> ) && m.CanWrite ).ToList() )
			{
				prop.SetValue( this, new List<MetadataTerm>() );
			}
		}
		public List<MetadataTerm> Context { get; set; }
		public List<MetadataTerm> LinkedData { get; set; }
		public List<MetadataTerm> Classes { get; set; }
		public List<MetadataTerm> Properties { get; set; }
		public List<MetadataTerm> ConceptSchemes { get; set; }
		public List<MetadataTerm> Concepts { get; set; }

		public List<MetadataTerm> AllTerms { get { return LinkedData.Concat( Classes ).Concat( Properties ).Concat( ConceptSchemes ).Concat( Concepts ).ToList(); } }
	}
	//

	public class MetadataTerm
	{
		public MetadataTerm()
		{
			foreach ( var prop in this.GetType().GetProperties().Where( m => m.PropertyType == typeof( List<string> ) ).ToList() )
			{
				prop.SetValue( this, new List<string>() );
			}
		}
		public string Type { get; set; }
		public string Id { get; set; }
		public string Label { get; set; }
		public string Description { get; set; }
		public string Comment { get; set; }
		public string UsageNote { get; set; }
		public List<string> SubThingOf { get; set; }
		public List<string> EquivalentTo { get; set; }
		public List<string> HasTerms { get; set; }
		public List<string> IsTermFor { get; set; }
		public List<string> Range { get; set; }

	}
	//
}
