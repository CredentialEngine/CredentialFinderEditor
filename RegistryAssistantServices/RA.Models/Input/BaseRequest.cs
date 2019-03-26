using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Models.Input
{
	public class BaseRequest
	{
        /// <summary>
        /// DefaultLanguage is used with Language maps where there is more than one entry for InLanguage, and the user doesn't want to have the first language in the list be the language used with language maps. 
        /// </summary>
        public string DefaultLanguage { get; set; }


        /// <summary>
        /// Publisher/Organization making the request
        /// this will always be the credential engine account, but we will want to record the actual publisher - org for the current person, if not site staff
        /// </summary>
        public string PublishByOrganizationIdentifier { get; set; }

		/// <summary>
		/// Identifier for Organization which Owns the data being published
		/// 2017-12-13 - this will be the CTID for the owning org.
		/// </summary>
		public string PublishForOrganizationIdentifier { get; set; }
		
		//public string ApiPublisher { get; set; }

		/// <summary>
		/// Envelope Identifier
		/// Optional property, used where the publishing entity wishes to store the identifier.
		/// Contains registry envelope identifier for a document in the registy. It should be empty for a new document. 
		/// </summary>
		public string RegistryEnvelopeId { get; set; }



	}
}
