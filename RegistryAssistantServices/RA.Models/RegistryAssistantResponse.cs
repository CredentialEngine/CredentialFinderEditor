using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Models
{
	/// <summary>
	/// Registry Assistant Response
	/// </summary>
	public class RegistryAssistantResponse
	{
		public RegistryAssistantResponse()
		{
			Messages = new List<string>();
			Payload = "";
		}
		public bool Successful { get; set; }

		public List<string> Messages { get; set; }

		public string CTID { get; set; }

		/// <summary>
		/// Identifier for the registry envelope that contains the document just add/updated
		/// </summary>
		public string RegistryEnvelopeIdentifier { get; set; }

		/// <summary>
		/// Payload of request to registry, containing properties formatted as CTDL - JSON-LD
		/// </summary>
		public string Payload { get; set; }
	}
}
