using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Models.Helpers.Cass;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Utilities;

namespace CTIServices
{
	public class CassServices
	{
		static string cassRootUrl = UtilityManager.GetAppKeyValue( "cassRootUrl" );
		public static T GetCassObject<T>( string url ) where T : CassObject
		{
			var results = new HttpClient().GetAsync( url ).Result.Content.ReadAsStringAsync().Result;
			return DeserializeCassObject<T>( results, url );
		}
		//

		private static T DeserializeCassObject<T>( string data, string selfURL = "" ) where T : CassObject
		{
			try
			{
				var item = JsonConvert.DeserializeObject<T>( data );
				item.Url = selfURL;
				return item;
			}
			catch
			{
				return default( T );
			}
		}
		//

		//TODO - figure out why data isn't making it from the client to the FrameworkModel here
		public static FrameworkModel SaveFrameworkData( FrameworkModel data )
		{
			//Get keys from Keys.Config
			var privateKey = ConfigHelper.GetApiKey( "CassPrivateKey", "" );
			var publicKey = ConfigHelper.GetApiKey( "CassPublicKey", "" );

			//Delete the framework if it exists
			DeleteFrameworkData( data.FrameworkNode.Guid );

			//Create the object
			var container = new Dictionary<string, object>()
			{
				{ "@context", "http://schema.eduworks.com/cass/0.1" },
				{ "@type", "http://schema.eduworks.com/cass/0.1/framework" },
				{ "name", data.FrameworkNode.Name },
				{ "description", data.FrameworkNode.Description }
			};

			//Sign the data
			container.Add( "@signature", new List<string>() { SignJson( container, privateKey ) } );

			//Add the owner to the object.
			var ownerFlattened = publicKey.Replace( "\r\n", "" ).Replace( "\n", "" );
			container.Add( "@owner", new List<string>() { ownerFlattened } );

			//Create the ID
			var url = cassRootUrl + "/framework/" + data.FrameworkNode.Guid;
			container.Add( "@id", url );

			//Create a signature
			var signature = new Dictionary<string, object>()
			{
				{ "@context", "http://schema.eduworks.com/ebac/0.1/" },
				{ "@type", "http://schema.eduworks.com/ebac/0.1/timeLimitedSignature" },
				{ "expiry", ( int ) ( DateTime.UtcNow.Subtract( new DateTime( 1970, 1, 1 ) ).TotalMilliseconds + 30000 ) },
				{ "server", cassRootUrl },
			};

			//Sign the signature - yes, really
			signature.Add( "@signature", new List<string>() { SignJson( container, privateKey ) } );
			signature.Add( "@owner", new List<string>() { ownerFlattened } );

			//Upload the data
			var postContent = new MultipartFormDataContent();
			postContent.Add( new StringContent( JsonConvert.SerializeObject( container ), Encoding.UTF8 ), "data" );
			postContent.Add( new StringContent( JsonConvert.SerializeObject( signature ), Encoding.UTF8 ), "signature" );
			var result = new HttpClient().PutAsync( url, postContent ).Result.Content.ReadAsStringAsync().Result;

			//Retrieve the freshly-uploaded framework
			var framework = GetFrameworkData( data.FrameworkNode.Guid );
			return framework;
		}
		//

		public static string SignJson( Dictionary<string, object> json, string privateKey )
		{
			//Serialize a copy of the object in JSON format with the keys sorted in ASCII order and with no whitespace.
			var stringified = JsonConvert.SerializeObject( json, new JsonSerializerSettings() { ContractResolver = new AlphaNumericContractResolver() } );

			//Sign the serialized copy of the object using a SHA1 digest of the serialized data and RSA-2048 bit encryption with the user’s private key.
			var base64Signature = "";
			using ( var hasher = new SHA1Managed() )
			using ( var crypto = new RSACryptoServiceProvider( 2048 ) )
			{
				//Get hash
				var hash = hasher.ComputeHash( Encoding.UTF8.GetBytes( stringified ) );
				var sha1 = string.Join( "", hash.Select( m => m.ToString( "X2" ) ).ToArray() );

				//Get parameters from a deserialiation of a stream of the private key string
				var priv = ( AsymmetricCipherKeyPair ) new Org.BouncyCastle.OpenSsl.PemReader( new StringReader( privateKey ) ).ReadObject();
				var priv2 = DotNetUtilities.ToRSAParameters( ( RsaPrivateCrtKeyParameters ) priv.Private );
				//crypto.ImportParameters( ( RSAParameters ) new XmlSerializer( typeof( RSAParameters ) ).Deserialize( new StringReader( privateKey ) ) );
				crypto.ImportParameters( priv2 );
				var encrypted = crypto.Encrypt( Encoding.Unicode.GetBytes( sha1 ), false );

				//Convert to base64
				base64Signature = Convert.ToBase64String( encrypted );
			}

			return base64Signature;
		}

		public static FrameworkModel GetFrameworkData( string frameworkGUID )
		{
			var url = cassRootUrl + "/framework/" + frameworkGUID;
			var rawData = new HttpClient().GetAsync( url ).Result.Content.ReadAsStringAsync().Result;
			var frameworkData = JsonConvert.DeserializeObject<CassFramework>( rawData );
			var result = new FrameworkModel()
			{
				Nodes = frameworkData.CompetencyUris.ConvertAll( m => new NodeModel() { Url = m } ),
				Relations = frameworkData.RelationUris.ConvertAll( m => new NodeRelation() { Url = m } ),
				FrameworkNode = new NodeModel()
				{
					Name = frameworkData.Name,
					Description = frameworkData.Description,
					Url = frameworkData.Url,
					Guid = frameworkGUID
				}
			};
			return result;
		}
		//

		public static void DeleteFrameworkData( string frameworkGUID )
		{
			if ( !string.IsNullOrWhiteSpace( frameworkGUID ) )
			{
				var url = cassRootUrl + "/framework/" + frameworkGUID;
				var rawData = new HttpClient().DeleteAsync( url ).Result.Content.ReadAsStringAsync().Result;
			}
		}
		//

		public class NodeModel
		{
			public string Guid { get; set; }
			public string Name { get; set; }
			public string NotationCode { get; set; }
			public string Url { get; set; }
			public string Description { get; set; }
		}
		//

		public class NodeRelation
		{
			public string Url { get; set; }
			public string SourceId { get; set; }
			public string TargetId { get; set; }
			public string RelationType { get; set; }
		}
		//

		public class FrameworkModel
		{
			public FrameworkModel()
			{
				FrameworkNode = new NodeModel();
				Nodes = new List<NodeModel>();
				Relations = new List<NodeRelation>();
			}
			public NodeModel FrameworkNode { get; set; }
			public List<NodeModel> Nodes { get; set; }
			public List<NodeRelation> Relations { get; set; }
		}
		//

		public class AlphaNumericContractResolver : DefaultContractResolver
		{
			protected override System.Collections.Generic.IList<JsonProperty> CreateProperties( System.Type type, MemberSerialization memberSerialization )
			{
				return base.CreateProperties( type, memberSerialization ).OrderBy( m => m.PropertyName ).ToList();
			}
		}
		//

	}
}
