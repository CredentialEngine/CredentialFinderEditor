using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.CTDL
{
	//public class Domain
	//{
	//	public Domain()
	//	{
	//		DomainClasses = new List<CTDLClass>();
	//	}

	//	List<CTDLClass> DomainClasses { get; set; }
	//}
	public class CTDLClass
	{
		public CTDLClass()
		{
			ClassProperties = new List<CTDLProperty>();
		}

		public int Id { get; set; }
		public string Class { get; set; }
		public string URI { get; set; }
		public string LabelTitle { get; set; }
		public string Definition { get; set; }
		public string Comment { get; set; }
		//public string TermType { get; set; }
		public string SubclassOf { get; set; }
		public string EquivalentClass { get; set; }
		public bool IsDomainClass { get; set; }
		public string UsageNote { get; set; }

		List<CTDLProperty> ClassProperties { get; set; }
	}
	public class CTDLProperty
	{
		public CTDLProperty()
		{
			PropertyRanges = new List<CTDLClass>();
			//PropertyDomains = new List<CTDLClass>();
		}
		public int Id { get; set; }
		public int ClassId { get; set; }
		public string URI { get; set; }
		public string SubpropertyOf { get; set; }
		public string LabelTitle { get; set; }
		public string Definition { get; set; }
		public string Elaboration { get; set; }
		public string ActualDataType { get; set; }
		public string EquivalentProperty { get; set; }
		public string UsageNote { get; set; }

		//range can be a single thing like a xsd:float,  xsd:dateTime,rdfs:Literal, or
		//	refer to a class - which can be multiple
		//so check for prefix of ctdl
		//OR just create classes for the latter xsd types, etc.
		List<CTDLClass> PropertyRanges { get; set; }
		//List<CTDLClass> PropertyDomains { get; set; }
		List<PropertyVocabulary> PropertyVocabulary { get; set; }

}

	public class PropertyRange
	{
		public int Id { get; set; }
		public int PropertyId { get; set; }
		public string Property { get; set; }
		public int ClassId { get; set; }
		public string Class { get; set; }
	}

	public class PropertyVocabulary
	{
		public PropertyVocabulary()
		{
		}
		public int Id { get; set; }
		public int PropertyId { get; set; }
		public string Vocabulary { get; set; }
		//note the term uses concatentated format of :
		//vocabularyName:vocabulary
		//ex:	costType:EnrollmentFee
		//not all data matches the latter, but assuming will
		public string Term { get; set; }
		public string Label { get; set; }
		public string Definition { get; set; }
		public string ScopeNote { get; set; }
		public string Broader { get; set; }
		public string Narrower { get; set; }
	
	}

}

