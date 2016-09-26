using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace Models.Node
{
	public class ProfileLink
	{
		public ProfileLink()
		{
			Type = this.GetType();
			RowId = new Guid(); //All zeroes
		}

		public int Id { get; set; }
		public Guid RowId { get; set; }
		public string Name { get; set; }
		public string Property { get; set; }
		public string TypeName { get { return Type.Name; } set { this.Type = Type.GetType( "Models.Node." + value ); } }

		[JsonIgnore][ScriptIgnore]
		public Type Type { get; set; }
	}

}
