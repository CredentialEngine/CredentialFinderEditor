using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Script.Serialization;

namespace MetadataRegistry
{

	[DataContract]
    public class RegistryPublish
        {
		 


        }
	[DataContract]
	public class MetadataEnvelope 
	{
		public List<RegistryDocument> documents;
		JavaScriptSerializer jsonSer;
		public string PublishURL;
		public MetadataEnvelope()
		{
			documents = new List<RegistryDocument>();
			jsonSer = new JavaScriptSerializer();
		}
	}
	public class RegistryDocument
	{
		public RegistryDocument()
		{

		}
		/// <summary>
		/// envelope_id will be blank for an add, and contain a valid registry envelope_id for updates and deletes
		/// </summary>
		[DataMember]
		public String envelope_id = "";

		[RequiredField( true )]
		[DataMember]
		public String envelope_type = "resource_data";

		[RequiredField( true )]
		[DataMember]
		public String envelope_version = "1.0.0";

		[RequiredField( true )]
		[DataMember]
		public String envelope_community = "credential_registry";

		[RequiredField( true )]
		[DataMember]
		public String resource = "";


		[RequiredField( true )]
		[DataMember]
		public String resource_format = "json";


		[RequiredField( true )]
		[DataMember]
		public String resource_encoding = "jwt";

		[RequiredField]
		[DataMember]
		public String resource_public_key = "";
	}
	
}
