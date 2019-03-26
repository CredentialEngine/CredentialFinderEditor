using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{
	public class ConceptScheme : BaseObject
	{

		public int OrgId { get; set; }
		public Organization OwningOrganization { get; set; } = new Organization();
		public string Source { get; set; }

		/// <summary>
		/// CTID - identifier for Concept Scheme. 
		/// Format: ce-UUID (lowercase)
		/// example: ce-534ec203-be18-49c3-a806-7e01d1cf0460
		/// </summary>
		public string CTID { get; set; }
		/// <summary>
		/// Name of the Concept Scheme
		/// Required
		/// </summary>
		public string Name { get; set; }

		public string EditorUri { get; set; }

		/// <summary>
		/// Concept Scheme description 
		/// Required
		/// </summary>
		public string Description { get; set; }

		//

		/// <summary>
		/// Top Concepts - list of CTIDs
		/// </summary>
		public List<Concept> TopConcepts { get; set; } = new List<Concept>();

		public bool IsApproved { get; set; }

		public string CredentialRegistryId { get; set; }
		public int LastPublishedById { get; set; }
		public bool IsPublished { get; set; }
		public string Payload { get; set; }
	}

	public class Concept
	{
		/// <summary>
		/// CTID - identifier for concept. 
		/// Format: ce-UUID (lowercase)
		/// example: ce-a044dbd5-12ec-4747-97bd-a8311eb0a042
		/// </summary>
		public string CTID { get; set; }


		/// <summary>
		/// Concept 
		/// Required
		/// </summary>
		public string PrefLabel { get; set; }

		/// <summary>
		/// Alternately can provide a language map
		/// </summary>
		public LanguageMap PrefLabel_Map { get; set; } = new LanguageMap();

		/// <summary>
		/// Concetpt description 
		/// Required
		/// </summary>
		public string Definition { get; set; }

		/// <summary>
		/// Alternately can provide a language map
		/// </summary>
		public LanguageMap Definition_Map { get; set; } = new LanguageMap();

		/// <summary>
		/// If 
		/// </summary>
		public string topConceptOf { get; set; }

		public string inScheme { get; set; }

		/// <summary>
		/// Last modified date for concept
		/// </summary>
		public string dateModified { get; set; }
	}
}
