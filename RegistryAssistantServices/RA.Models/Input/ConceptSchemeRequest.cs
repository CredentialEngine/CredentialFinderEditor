using System.Collections.Generic;
using Newtonsoft.Json;

namespace RA.Models.Input
{
	public class ConceptSchemeRequest : BaseRequest
	{
		public ConceptSchemeRequest()
		{
			ConceptScheme = new ConceptScheme();
		}
		public string CTID { get; set; }
		public ConceptScheme ConceptScheme { get; set; } = new ConceptScheme();

		public ConceptSchemeGraph ConceptSchemeGraph { get; set; }
	}

	public class ConceptSchemeGraph
	{

		[JsonProperty( "@context" )]
		public string Context { get; set; }

		[JsonProperty( "@id" )]
		public string Id { get; set; }

		/// <summary>
		/// Main graph object
		/// </summary>
		[JsonProperty( "@graph" )]
		public object Graph { get; set; }

	}

	public class SkosConceptScheme
	{
		/// <summary>
		/// Main graph object
		/// </summary>
		[JsonProperty( "@graph" )]
		public object Graph { get; set; }

		[JsonProperty( "@context" )]
		public string Context { get; set; }
	}



	public class ConceptScheme
	{
		/// <summary>
		/// Not sure if will use this, rather format the id based on the current environment.
		/// If used, would be URI in registry format. 
		/// Example:
		/// https://sandboxcredentialengineregistry.org/resources/ce-5de14407-223a-480f-90db-d6201fee0ab5
		/// Or more likely:
		/// https://credentialengineregistry.org/graph/ce-5de14407-223a-480f-90db-d6201fee0ab5
		/// </summary>
		public string Id { get; set; }

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

		/// <summary>
		/// Alternately can provide a language map
		/// </summary>
		public LanguageMap Name_Map { get; set; } = new LanguageMap();

		/// <summary>
		/// Concept Scheme description 
		/// Required
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Alternately can provide a language map
		/// </summary>
		public LanguageMap Description_Map { get; set; } = new LanguageMap();

		/// <summary>
		/// Top Concepts - list of CTIDs
		/// </summary>
		public List<Concept> TopConcepts { get; set; } = new List<Concept>();
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
