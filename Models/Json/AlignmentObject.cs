using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Serialization;

namespace Models.Json
{
	[DataContract]
	public class AlignmentObject : JsonLDObject
	{
		public AlignmentObject()
		{
			type = "ceterms:AlignmentObject";
		}

	}
}
