using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{
	public class Person : Agent
	{

		public Person()
		{
			AgentType = "Person";
			AgentTypeId = 1;
			FirstName = "";
			LastName = "";
		}

		public string FirstName { get; set; }
		public string LastName { get; set; }

		public string Name
		{
			get
			{
				return FirstName + " " + LastName;
			}
		}
	}
}
