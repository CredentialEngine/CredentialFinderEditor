using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Microsoft.IdentityModel.Tokens.JWT;
using Jose;

namespace Utilities
{
	public class JWTHandler
	{
		public static string SignRSA256(string document, string privateKey)
		{
			string encodedDoc = "";
			//JWTSecurityTokenHandler tokenHandler = new JWTSecurityTokenHandler();
			//// Set the expected properties of the JWT token in the TokenValidationParameters
			//okenValidationParameters validationParameters = 
			//new TokenValidationParameters()
			//{                   
			//	AllowedAudience =  UtilityManager.GetAppKeyValue("AllowedAudience"),
			//	ValidIssuer = UtilityManager.GetAppKeyValue("Issuer"),
   
			//	// Fetch the signing token from the FederationMetadata document of the tenant.
			//	SigningToken = new X509SecurityToken(new X509Certificate2(
			//	GetSigningCertificate(
			//		ConfigurationManager.AppSettings["FedMetadataEndpoint"])))
			//};                       
                    
			//Thread.CurrentPrincipal = tokenHandler.ValidateToken(token, 
			//								validationParameters);
			return encodedDoc;
		}


		public static string JoseEncode( object payload, string privateKey )
		{
			string encodedDoc = "";
			

			//var privateKey = new X509Certificate2( "my-key.p12", "password", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet ).PrivateKey as RSACryptoServiceProvider;
			//this fails on cannot find requested file
			var privateKey2 = new X509Certificate2("mykey-openssh", "ctiDevelopment", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet).PrivateKey as RSACryptoServiceProvider;

			//fails with:
			//RsaUsingSha alg expects key to be of AsymmetricAlgorithm type.
			encodedDoc = Jose.JWT.Encode( payload, privateKey, JwsAlgorithm.RS256 );

			return encodedDoc;
		}

		public static string JoseEncodeFromFile(object payload, string privateKeyFile, string password )
		{
			string encodedDoc = "";


			//var privateKey = new X509Certificate2( "my-key.p12", "password", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet ).PrivateKey as RSACryptoServiceProvider;

			//FAILS: The system cannot find the file specified
			var privateKey = new X509Certificate2(privateKeyFile, password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet).PrivateKey as RSACryptoServiceProvider;

			encodedDoc = Jose.JWT.Encode(payload, privateKey, JwsAlgorithm.RS256);

			return encodedDoc;
		}	
	}
}
