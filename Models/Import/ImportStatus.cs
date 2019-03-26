using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Import
{

    public class ImportStatus
    {
        public ImportStatus()
        {
            Messages = new List<StatusMessage>();
            HasErrors = false;
        }

        public string Ctid { get; set; }

        public int RecordsAdded { get; set; }
        public int RecordsUpdated { get; set; }
        public int RecordsFailed { get; set; }
        public List<StatusMessage> Messages { get; set; }
        /// <summary>
        /// If true, error encountered somewhere during workflow
        /// </summary>
        public bool HasErrors { get; set; }
        /// <summary>
        /// Reset HasSectionErrors to false at the start of a new section of validation. Then check at th end of the section for any errors in the section
        /// </summary>
        public bool HasSectionErrors { get; set; }
        public void AddError( string message )
        {
            Messages.Add( new StatusMessage() { Message = message } );
            HasErrors = true;
            HasSectionErrors = true;
        }
        public void AddErrorRange( List<string> messages )
        {
            foreach (string msg in messages)
                Messages.Add( new StatusMessage() { Message = msg } );
        }
        public void AddErrorRange( string leadMessage, List<string> messages )
        {
            if (messages.Count == 1)
            {
                AddError( leadMessage + " " + messages[ 0 ] );
            }
            else
            {
                AddError( leadMessage);
                foreach (string msg in messages)
                    Messages.Add( new StatusMessage() { Message = msg } );
            }
        }
        public void AddWarning( string message )
        {
            Messages.Add( new StatusMessage() { Message = message, IsWarning = true } );
        }
        public void AddInformation( string message )
        {
            Messages.Add( new StatusMessage() { Message = message, IsInformation = true } );
        }
        public void AddWarningRange( List<string> messages )
        {
            foreach (string msg in messages)
                Messages.Add( new StatusMessage() { Message = msg, IsWarning = true } );
        }
        public List<string> GetAllMessages()
        {
            List<string> messages = new List<string>();
            string prefix = "";
            foreach (StatusMessage msg in Messages.OrderBy( m => m.IsWarning ))
            {
                if (msg.IsWarning )
                    if (!msg.Message.ToLower().StartsWith("warning"))
                        prefix = "Warning - ";
                else if( msg.IsInformation )
                    if (!msg.Message.ToLower().StartsWith( "information" ))
                    prefix = "Information - ";
                else
                    if (!msg.Message.ToLower().StartsWith( "error" ))
                        prefix = "Error - ";
                messages.Add( prefix + msg.Message );
            }

            return messages;
        }

        public string GetErrorsAsString( string separator = "\r\n" )
        {
            if (Messages.Count > 0)
                return string.Join( separator, GetAllMessages() );
            else
                return "";
        }

        public void SetMessages( List<string> messages, bool isAllWarning )
        {
            //just treat all as errors for now
            //string prefix = "";
            foreach (string msg in messages)
            {
                if (isAllWarning)
                    AddWarning( msg );
                else
                    AddError( msg );
            }
        }
    }

    public class StatusMessage
    {
        public string Message { get; set; }
        public bool IsWarning { get; set; }
        public bool IsInformation { get; set; }
    }
}
