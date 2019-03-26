using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace Utilities
{
    public class SafeBrowsing
    {
        public static Reputation CheckUrl( string Url )
        {
            List<string> Urls = new List<string> { Url };
            List<Reputation> results = CheckUrl( Urls );
            if ( results != null && results.Count > 0 )
                return results[ 0 ];
            else
                return Reputation.None;
        }

        /// <summary>
        /// reference
        /// https://developers.google.com/safe-browsing/v4/lookup-api
        /// </summary>
        /// <param name="Urls"></param>
        /// <returns></returns>
        public static List<Reputation> CheckUrl( List<string> Urls )
        {
            List<Reputation> reputationResults = new List<Reputation>();
            SafeBrowsingRequest request = new SafeBrowsingRequest();
            var googleSafeBrowsingApiKey = UtilityManager.GetAppKeyValue( "googleSafeBrowsingApiKey" );
            //set apikey to blank to skip checks
            if (string.IsNullOrWhiteSpace(googleSafeBrowsingApiKey))
            {
                reputationResults.Add( Reputation.None );
                return reputationResults;
            }
            var endpoint = UtilityManager.GetAppKeyValue( "googleSafeBrowsingApi" ) + googleSafeBrowsingApiKey;

            string message = "";
            for ( int index = 0; index < Urls.Count; index++ )
            {
                request.threatInfo.threatEntries.Add( new ThreatUrl() { url = Urls[index] } );
            }
            var postBody = JsonConvert.SerializeObject( request, Formatting.Indented );

            try
            {
                using ( var client = new HttpClient() )
                {
                    client.DefaultRequestHeaders.
                        Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );

                    var task = client.PostAsync( endpoint,
                        new StringContent( postBody, Encoding.UTF8, "application/json" ) );
                    task.Wait();
                    var response = task.Result;
                    string contents = task.Result.Content.ReadAsStringAsync().Result;
                    if ( !response.IsSuccessStatusCode || contents.Length > 0 )
                    {
                        SafeBrowsingResponse sbr = JsonConvert.DeserializeObject<SafeBrowsingResponse>( contents );
                        var json = JsonConvert.SerializeObject( sbr, Formatting.Indented );

                        if ( sbr.matches.Count == 0 && sbr.error == null )
                            reputationResults.Add( Reputation.None );
                        else
                        {
                            if ( sbr.error != null )
                                if ( sbr.error.code == "400" )
                                    reputationResults.Add( Reputation.Error );

                            sbr.matches.ForEach( x => {
                                if(x.threatType == "MALWARE" )
                                    reputationResults.Add( Reputation.MalwareBlackList );

                            } );                            
                        }
                    }
                    else
                    {
                        reputationResults.Add( Reputation.None );
                    }
                    //return response.IsSuccessStatusCode;

                }
            }
            catch ( Exception exc )
            {
                LoggingHelper.LogError( exc, "SafeBrowsing.CheckUrl" );
                reputationResults.Add( Reputation.Error );

            }


            return reputationResults;
        }

        [Flags]
        public enum Reputation
        {
            None = 0x0,
            MalwareBlackList = 0x1 << 0,
            PhishBlackList = 0x1 << 1,
            Error = 0x1 << 2
            // This is stored as an 8 bit value, the maximum value is 0x1 << 7
        }

        public class SafeBrowsingRequest
        {
            public SafeBrowsingClient client { get; set; } = new SafeBrowsingClient();
            public ThreatInfo threatInfo { get; set; } = new ThreatInfo();
        }
        public class SafeBrowsingClient
        {
            public string clientId { get; set; } = "credentialengine";
            public string clientVersion { get; set; } = "1.0";
        }

        public class ThreatInfo
        {
            public ThreatInfo()
            {
                threatTypes = new List<string>() { "MALWARE", "SOCIAL_ENGINEERING" };
                platformTypes = new List<string>() { "ANY_PLATFORM" };
                threatEntryTypes = new List<string>() { "URL" };
            }
            public List<string> threatTypes { get; set; }
            public List<string> platformTypes { get; set; }
            public List<string> threatEntryTypes { get; set; }
            public List<ThreatUrl> threatEntries { get; set; } = new List<ThreatUrl>();
        }
        public class ThreatUrl
        {
            public string url { get; set; }
        }
        public class SafeBrowsingResponse
        {
            public List<ThreatType> matches { get; set; } = new List<ThreatType>();
            public ErrorResponse error { get; set; }
        }

        public class ThreatType
        {
            public ThreatType()
            {
            }
            public string threatType { get; set; }
            public string platformType { get; set; }
            public string threatEntryType { get; set; }
            public ThreatUrl threat { get; set; } = new ThreatUrl();
            public object threatEntryMetadata { get; set; }
            public string cacheDuration { get; set; }
        }
        public class ErrorResponse
        {
            public string code { get; set; }
            public string message { get; set; }
            public string status { get; set; }
        }
    }
}
