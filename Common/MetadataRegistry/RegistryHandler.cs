using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Jose;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.IO;
using Utilities;

namespace MetadataRegistry
{
	public class RegistryHandler
	{

		/// <summary>
		/// Creates a MetadataRegistry envelope from an RSA key pair.
		/// </summary>
		/// <param name="publicKeyPath">Path to the public key file in the PEM format.</param>
		/// <param name="secretKeyPath">Path to the private key file in the PEM format.</param>
		/// <param name="contents">Envelope payload.</param>
		/// <returns>An Envelope that can be serialized and POST'ed to a MetadataRegistry server.</returns>
		public static Envelope CreateEnvelope( string publicKeyPath, string secretKeyPath, string contents )
		{
			RsaPrivateCrtKeyParameters privateKey;

			using ( var reader = File.OpenText( secretKeyPath ) )
			{
				privateKey = ( RsaPrivateCrtKeyParameters ) ( ( AsymmetricCipherKeyPair ) new PemReader( reader ).ReadObject() ).Private;
			}

			string publicKey = File.ReadAllText( publicKeyPath );

			string encoded = JWT.Encode( contents, DotNetUtilities.ToRSA( privateKey ), JwsAlgorithm.RS256 );

			return new Envelope
			{
				EnvelopeType = "resource_data",
				EnvelopeVersion = "1.0.0",
				EnvelopeCommunity = "credential_registry",
				Resource = encoded,
				ResourceFormat = "json",
				ResourceEncoding = "jwt",
				ResourcePublicKey = publicKey
			};
		}

		public static bool CreateEnvelope( string publicKeyPath, string secretKeyPath, string contents, Envelope envelope )
		{
			bool isValid = true;
			RsaPrivateCrtKeyParameters privateKey;

			LoggingHelper.DoTrace( 4, string.Format( "====Reading private key: {0}", secretKeyPath ) );
			using ( var reader = File.OpenText( secretKeyPath ) )
			{
				privateKey = ( RsaPrivateCrtKeyParameters ) ( ( AsymmetricCipherKeyPair ) new PemReader( reader ).ReadObject() ).Private;
			}

			LoggingHelper.DoTrace( 4, string.Format( "====Reading public key: {0}", publicKeyPath ) );
			string publicKey = File.ReadAllText( publicKeyPath );

			LoggingHelper.DoTrace( 4, "====Doing JWT Encode" );
			string encoded = JWT.Encode( contents, DotNetUtilities.ToRSA( privateKey ), JwsAlgorithm.RS256 );

			LoggingHelper.DoTrace( 4, "==== populating envelope ====" );
			envelope.EnvelopeType = "resource_data";
			envelope.EnvelopeVersion = "1.0.0";
			envelope.EnvelopeCommunity = "credential_registry";
			envelope.Resource = encoded;
			envelope.ResourceFormat = "json";
			envelope.ResourceEncoding = "jwt";

			LoggingHelper.DoTrace( 4, "==== adding public key ====" );
			envelope.ResourcePublicKey = publicKey;
			return isValid;
		}

		/// <summary>
		/// Create an envelope for deleting a metadata document
		/// </summary>
		/// <param name="publicKeyPath"></param>
		/// <param name="secretKeyPath"></param>
		/// <param name="envelopeIdentifier"></param>
		/// <param name="contents"></param>
		/// <returns></returns>
		public static DeleteEnvelope CreateDeleteEnvelope( string publicKeyPath, string secretKeyPath, string ctid, string userName )
		{
			RsaPrivateCrtKeyParameters privateKey;
			//string envelopeIdentifier, 
			using ( var reader = File.OpenText( secretKeyPath ) )
			{
				privateKey = ( RsaPrivateCrtKeyParameters ) ( ( AsymmetricCipherKeyPair ) new PemReader( reader ).ReadObject() ).Private;
			}
			DeleteObject del = new DeleteObject();
			del.Ctid = ctid;
			del.Actor = userName;
			string contents = JsonConvert.SerializeObject( del );

			//string contents = string.Format("{\"delete\": true, \"ctld:ctid\":\"{0}\", \"deletedBy\":\"{1}\"}", ctid, userName) ;
			string publicKey = File.ReadAllText( publicKeyPath );

			string encoded = JWT.Encode( contents, DotNetUtilities.ToRSA( privateKey ), JwsAlgorithm.RS256 );
				//			EnvelopeCommunity = "credential_registry",
				//EnvelopeIdentifier = envelopeIdentifier,
			return new DeleteEnvelope
			{
				DeleteToken = encoded,
				ResourceFormat = "json",
				ResourceEncoding = "jwt",
				ResourcePublicKey = publicKey
			};
		}
	}
}
