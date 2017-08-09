using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;


namespace Models.Json
{
	[DataContract]
	public class QualityAssuranceAction : JsonLDObject
	{
		public QualityAssuranceAction()
		{
			type = "ceterms:QualityAssuranceAction";
		}
	}
}
