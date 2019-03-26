using System;
using System.IO;
using Jose;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

using Utilities;

namespace CredentialRegistry
{
	/// <summary>
	/// Requires Nuget packages for:
	/// Jose-Jwt: https://github.com/dvsekhvalnov/jose-jwt
	/// Bouncy Castle: http://www.bouncycastle.org/csharp/
	/// </summary>
	public class RegistryHandler
	{
		/*************************************************************
		 * BE SURE TO UPDATE THE APPLICATION POOL in IIS
		 *	- Go to IIS Manager
		 *	- Go to the application pool instance
		 *	- Click advanced settings
		 *	- Under Process model, set Load User Profile to true
		 * 
		 * 
		 *************************************************************/


		/// <summary>
		/// Creates a Registry envelope from an RSA key pair.
		/// </summary>
		/// <param name="publicKeyPath">Path to the public key file in the PEM format.</param>
		/// <param name="secretKeyPath">Path to the private key file in the PEM format.</param>
		/// <param name="contents">Envelope payload.</param>
		/// <param name="envelope">A envelope object that will be populated</param>
		/// <returns>An Envelope that can be serialized and POST'ed to a credentialRegistry server.</returns>
		public static bool CreateEnvelope( string publicKeyPath, string secretKeyPath, string contents, Envelope envelope )
		{
			bool isValid = true;
			RsaPrivateCrtKeyParameters privateKey;

			LoggingHelper.DoTrace( 7, string.Format( "====Reading private key: {0}", secretKeyPath ) );
			using ( var reader = File.OpenText( secretKeyPath ) )
			{
				privateKey = ( RsaPrivateCrtKeyParameters ) ( ( AsymmetricCipherKeyPair ) new PemReader( reader ).ReadObject() ).Private;
			}

			LoggingHelper.DoTrace( 7, string.Format( "====Reading public key: {0}", publicKeyPath ) );
			string publicKey = File.ReadAllText( publicKeyPath );

			//LoggingHelper.DoTrace( 7, "==== contents: " + (contents != null ? contents : "missing content") );

			LoggingHelper.DoTrace( 7, "==== do the JWT encoding (note to RS256) using privateKey, using BouncyCastle for the ToRSA ====" );

			//try
			//{
			//	if ( privateKey != null )
			//	{
			//		LoggingHelper.DoTrace( 7, "==== privateKey: " + privateKey.ToString() );
			//	}
			//} catch (Exception ex)
			//{
			//	LoggingHelper.DoTrace( 4, "==== Exception on private key tracing. " + ex.Message );
			//}
			//do the JWT encoding (note to RS256) using privateKey, using BouncyCastle for the ToRSA
			//NOTE: Must update related IIS application pool, or following will result in a file not found.
			string encoded = JWT.Encode( contents, DotNetUtilities.ToRSA( privateKey ), JwsAlgorithm.RS256 );

			LoggingHelper.DoTrace( 7, "==== populating envelope ====" );
			envelope.EnvelopeType = "resource_data";
			envelope.EnvelopeVersion = "1.0.0";
			envelope.EnvelopeCommunity = UtilityManager.GetAppKeyValue( "envelopeCommunity", "ce_registry");
			envelope.Resource = encoded;
			envelope.ResourceFormat = "json";
			envelope.ResourceEncoding = "jwt";
            //if (DateTime.Now.ToString( "yyyy-MM-dd" ) == "2018-04-17")
            //{
            //    NodeVersion nv = new NodeVersion() { Actor = "michael parsons", EventType = "Update" };
            //    envelope.NodeHeader.NodeVersions = new System.Collections.Generic.List<NodeVersion>();
            //    envelope.NodeHeader.NodeVersions.Add( nv );
            //}

            LoggingHelper.DoTrace( 7, "==== adding public key ====" );
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
				//			EnvelopeCommunity = "ce_registry",
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
