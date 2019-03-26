using System;
using System.IO;
using System.Web;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;


namespace Utilities
{
    public class SecurityHelper
    {
        const string thisClassName = "SecurityHelper";

        public SecurityHelper() { }


        public static string GeneratePrivateKeyPair()
        {

			string privateKey = "";         

            try
            {
				RsaKeyPairGenerator r = new RsaKeyPairGenerator();
            r.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
            AsymmetricCipherKeyPair keys = r.GenerateKeyPair();
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(keys.Private);
            pemWriter.Writer.Flush();

            privateKey = textWriter.ToString();


            }
            catch
            {
                //eat any additional exception
            }

				return privateKey;

        } //

		
    }
}
