using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Models
{
	public class RequestStatus
	{
		public RequestStatus()
		{
			Messages = new List<RequestMessage>();
			HasErrors = false;
			CodeValidationType = "warn";
		}

		/// <summary>
		/// CodeValidationType - actions for code validation
		/// rigid-concepts must match ctdl 
		/// warn - allow exceptions, return a warning message
		/// skip - no validation of concept scheme concepts
		/// </summary>
		public string CodeValidationType { get; set; }

		public string Payload { get; set; }
		/// <summary>
		/// return the registr envelope id
		/// </summary>
		public string RegistryEnvelopeId { get; set; }

		public List<RequestMessage> Messages { get; set; }
		public bool HasErrors { get; set; }
		public void AddError( string message )
		{
			Messages.Add( new Models.RequestMessage() { Message = message});
			HasErrors = true;
		}
		public void AddWarning( string message )
		{
			Messages.Add( new Models.RequestMessage() { Message = message, IsWarning = true } );
		}

		public List<string> GetAllMessages()
		{
			List<string> messages = new List<string>();
			string prefix = "";
			foreach(RequestMessage msg in Messages.OrderBy(m => m.IsWarning))
			{
				if ( msg.IsWarning )
					prefix = "Warning - ";
				else
					prefix = "Error - ";
				messages.Add( prefix + msg.Message );
			}
			

			return messages;
		}

		public void SetMessages( List<string> messages )
		{
			//just treat all as errors for now
			string prefix = "";
			foreach ( string msg in messages )
			{
				AddError( msg );
			}

		}
	}

	public class RequestMessage
	{
		public string Message { get; set; }
		public bool IsWarning { get; set; }
	}
}
