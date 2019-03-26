using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web;
using System.Xml.Linq;

using Models;
using Models.Common;
using Utilities;
using System.Globalization;

namespace Factories
{
    public class BaseFactory
    {
        protected static string DEFAULT_GUID = "00000000-0000-0000-0000-000000000000";
        public static string commonStatusMessage = "";
		public static int MinimumDescriptionLength = UtilityManager.GetAppKeyValue( "minDescriptionTextLength", 25 );

		public static bool IsDevEnv()
        {
            if ( UtilityManager.GetAppKeyValue( "envType", "no" ) == "development" )
                return true;
            else
                return false;
        }
        public static bool IsProduction()
        {

            if ( UtilityManager.GetAppKeyValue( "envType", "no" ) == "production" )
                return true;
            else
                return false;
        }
        #region Entity frameworks helpers
        public static bool HasStateChanged( Data.CTIEntities context )
        {
            if ( context.ChangeTracker.Entries().Any( e =>
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified ||
                    e.State == EntityState.Deleted ) == true )
                return true;
            else
                return false;
        }

        public static string SetLastUpdatedBy( int lastUpdatedById, Data.Account accountModifier )
        {
            string lastUpdatedBy = "";
            if ( accountModifier != null )
            {
                lastUpdatedBy = accountModifier.FirstName + " " + accountModifier.LastName;
            }
            else
            {
                if ( lastUpdatedById > 0 )
                {
                    AppUser user = AccountManager.AppUser_Get( lastUpdatedById );
                    lastUpdatedBy = user.FullName();
                }
            }
            return lastUpdatedBy;
        }
        #endregion
        #region Database connections
        /// <summary>
        /// Get the read only connection string for the main database
        /// </summary>
        /// <returns></returns>
        public static string DBConnectionRO()
        {

            string conn = WebConfigurationManager.ConnectionStrings["cti_RO"].ConnectionString;
            return conn;

        }
        public static string MainConnection()
        {
            string conn = WebConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            return conn;
        }
        #endregion

        #region data retrieval

        protected static CodeItemResult Fill_CodeItemResults( DataRow dr, string fieldName, int categoryId, bool hasSchemaName, bool hasTotals, bool hasAnIdentifer = true )
        {
            string list = GetRowPossibleColumn( dr, fieldName, "" );
            //string list = dr[ fieldName ].ToString();
            CodeItemResult item = new CodeItemResult() { CategoryId = categoryId };
            item.HasAnIdentifer = hasAnIdentifer;

            int totals = 0;
            int id = 0;
            string title = "";
            string schema = "";
            string codedNotation = "";

            if ( !string.IsNullOrWhiteSpace( list ) )
            {
                var codeGroup = list.Split( '|' );
                foreach ( string codeSet in codeGroup )
                {
                    var codes = codeSet.Split( '~' );
                    schema = "";
                    totals = 0;
                    id = 0;
                    if ( hasAnIdentifer )
                    {
                        Int32.TryParse( codes[0].Trim(), out id );
                        if ( codes.Length > 1 )
                            title = codes[1].Trim();
                        if ( hasSchemaName )
                        {
                            if ( codes.Length > 2 )
                                schema = codes[2];

                            if ( hasTotals && codes.Length > 3 )
                                totals = Int32.Parse( codes[3] );
                        }
                        else
                        {
                            if ( hasTotals && codes.Length > 2 )
                                totals = Int32.Parse( codes[2] );
                        }
                    }
                    else
                    {
                        //currently if no Id, assume only text value
                        title = codes[0].Trim();
                    }
                    if ( codes.Length > 4 )
                        codedNotation = codes[4];

                    item.Results.Add( new Models.CodeItem() { Id = id, Title = title, SchemaName = schema, Totals = totals, CodedNotation = codedNotation } );
                }
            }

            return item;
        }
        protected static CodeItemResult Fill_CodeItemResultsFromXml( DataRow dr, string fieldName, int categoryId, bool hasSchemaName, bool hasTotals, bool hasAnIdentifer = true )
        {
            string list = GetRowPossibleColumn( dr, fieldName, "" );
            //string list = dr[ fieldName ].ToString();
            CodeItemResult item = new CodeItemResult() { CategoryId = categoryId };
            item.HasAnIdentifer = hasAnIdentifer;
            if ( string.IsNullOrWhiteSpace( list ) )
                return item;

            if ( !string.IsNullOrWhiteSpace( list ) )
            {
                var xDoc = XDocument.Parse( list );
                foreach ( var child in xDoc.Root.Elements() )
                {
                    var prop = new CodeItem
                    {
                        CategoryId = int.Parse( child.Attribute( "CategoryId" ).Value ),
                        Id = int.Parse( child.Attribute( "PropertyValueId" ).Value ),
                        Title = ( string )child.Attribute( "Property" ) ?? "",
                        CodedNotation = ( string )child.Attribute( "CodedNotation" ) ?? "",
                        SchemaName = ( string )child.Attribute( "SchemaName" ) ?? "",
                        //OtherValue = ( string )child.Attribute( "OtherValue" ) ?? "",
                    };
					if ( prop.CategoryId == categoryId)
					{
						item.Results.Add( prop );
					}
                    
                    //item.Results.Add(new Models.CodeItem() { Id = id, Title = title, SchemaName = schema, Totals = totals });
                }

            }

            return item;
        }

        protected static AgentRelationshipResult Fill_AgentItemResultsFromXml( DataRow dr, string fieldName, int categoryId, bool hasSchemaName, bool hasTotals, bool hasAnIdentifer = true )
        {
            string list = GetRowPossibleColumn( dr, fieldName, "" );
            //string list = dr[ fieldName ].ToString();
            AgentRelationshipResult item = new AgentRelationshipResult() { CategoryId = categoryId };
            item.HasAnIdentifer = hasAnIdentifer;
            if ( string.IsNullOrWhiteSpace( list ) )
                return item;

            if ( !string.IsNullOrWhiteSpace( list ) )
            {
                var xDoc = XDocument.Parse( list );
                foreach ( var child in xDoc.Root.Elements() )
                {
                    var prop = new AgentRelationship
                    {
                        RelationshipId = int.Parse( child.Attribute( "RelationshipTypeId" ).Value ),
                        AgentId = int.Parse( child.Attribute( "OrgId" ).Value ),
                        Relationship = ( string )child.Attribute( "Relationship" ) ?? "",
                        Agent = ( string )child.Attribute( "Organization" ) ?? "",
                        AgentUrl = ( string )child.Attribute( "SubjectWebpage" ) ?? "",
                        IsThirdPartyOrganization = ( string )child.Attribute( "IsThirdPartyOrganization" ) ?? "",
                    };
                    if ( prop.IsThirdPartyOrganization == "1" && !IsProduction() && prop.Agent.IndexOf( "[reference]" ) == -1 )
                        prop.Agent += " [reference] ";
                    item.Results.Add( prop );
                    //item.Results.Add(new Models.CodeItem() { Id = id, Title = title, SchemaName = schema, Totals = totals });
                }

            }

            return item;
        }
        /// <summary>
        /// Helper method to retrieve a string column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>

        protected static CredentialConnectionsResult Fill_CredentialConnectionsResult( DataRow dr, string fieldName, int categoryId )
        {
            string list = GetRowPossibleColumn( dr, fieldName, "" );
            CredentialConnectionsResult result = new CredentialConnectionsResult() { CategoryId = categoryId };
            CredentialConnectionItem item = new CredentialConnectionItem();
            int id = 0;

            if ( !string.IsNullOrWhiteSpace( list ) )
            {
                var codeGroup = list.Split( '|' );
                foreach ( string codeSet in codeGroup )
                {
                    var codes = codeSet.Split( '~' );
                    item = new CredentialConnectionItem();

                    id = 0;
                    Int32.TryParse( codes[0].Trim(), out id );
                    item.ConnectionId = id;
                    if ( codes.Length > 1 )
                        item.Connection = codes[1].Trim();
                    if ( codes.Length > 2 )
                        Int32.TryParse( codes[2].Trim(), out id );
                    item.CredentialId = id;
                    if ( codes.Length > 3 )
                        item.Credential = codes[3].Trim();
                    if ( codes.Length > 4 )
                        Int32.TryParse( codes[4].Trim(), out id );
                    item.CredentialOwningOrgId = id;
                    if ( codes.Length > 5 )
                        item.CredentialOwningOrg = codes[5].Trim();

                    result.Results.Add( item );
                }
            }

            return result;
        }


        public static string GetRowColumn( DataRow row, string column, string defaultValue = "" )
        {
            string colValue = "";

            try
            {
                colValue = row[column].ToString();

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                if ( HasMessageBeenPreviouslySent( column ) == false )
                {
                    string queryString = GetWebUrl();
                    string exType = ex.GetType().ToString();
                    LoggingHelper.LogError( exType + " Exception in GetRowColumn( DataRow row, string column, string defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                }

                colValue = defaultValue;
            }
            return colValue;

        }

        public static string GetRowPossibleColumn( DataRow row, string column, string defaultValue = "" )
        {
            string colValue = "";

            try
            {
                colValue = row[column].ToString();

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {

                colValue = defaultValue;
            }
            return colValue;

        }


        /// <summary>
        /// Helper method to retrieve an int column from a row while handling invalid values
        /// </summary>
        /// <param name="row">DataRow</param>
        /// <param name="column">Column Name</param>
        /// <param name="defaultValue">Default value to return if column data is invalid</param>
        /// <returns></returns>
        public static int GetRowColumn( DataRow row, string column, int defaultValue )
        {
            int colValue = 0;

            try
            {
                colValue = Int32.Parse( row[column].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                if ( HasMessageBeenPreviouslySent( column ) == false )
                {
                    string queryString = GetWebUrl();
                    LoggingHelper.LogError( "Exception in GetRowColumn( DataRow row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                }


                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        }

        public static int GetRowPossibleColumn( DataRow row, string column, int defaultValue )
        {
            int colValue = 0;

            try
            {
                colValue = Int32.Parse( row[column].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        }
        public static decimal GetRowPossibleColumn( DataRow row, string column, decimal defaultValue )
        {
            decimal colValue = 0;

            try
            {
                colValue = decimal.Parse( row[column].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                //string queryString = GetWebUrl();

                //LoggingHelper.LogError( "Exception in GetRowColumn( DataRow row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        }
        public static bool GetRowColumn( DataRow row, string column, bool defaultValue )
        {
            bool colValue = false;

            try
            {
                //need to properly handle int values of 0,1, as bool
                string strValue = row[column].ToString();
                if ( !string.IsNullOrWhiteSpace( strValue ) && strValue.Trim().Length == 1 )
                {
                    strValue = strValue.Trim();
                    if ( strValue == "0" )
                        return false;
                    else if ( strValue == "1" )
                        return true;
                }
                colValue = bool.Parse( row[column].ToString() );

            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                if ( HasMessageBeenPreviouslySent( column ) == false )
                {
                    string queryString = GetWebUrl();
                    LoggingHelper.LogError( "Exception in GetRowColumn( DataRow row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                }

                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        }
        public static DateTime GetRowColumn( DataRow row, string column, DateTime defaultValue )
        {
            DateTime colValue;

            try
            {
                string strValue = row[column].ToString();
                if ( DateTime.TryParse( strValue, out colValue ) == false )
                    colValue = defaultValue;
            }
            catch ( System.FormatException fex )
            {
                //Assuming FormatException means null or invalid value, so can ignore
                colValue = defaultValue;

            }
            catch ( Exception ex )
            {
                if ( HasMessageBeenPreviouslySent( column ) == false )
                {
                    string queryString = GetWebUrl();
                    LoggingHelper.LogError( ex, " Exception in GetRowColumn( DataRow row, string column, string defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
                }
                colValue = defaultValue;
                //throw ex;
            }
            return colValue;

        }
        public static bool HasMessageBeenPreviouslySent( string keyName )
        {

            string key = "missingColumn_" + keyName;
            //check cache for keyName
            if ( HttpRuntime.Cache[key] != null )
            {
                return true;
            }
            else
            {
                //not really much to store
                HttpRuntime.Cache.Insert( key, keyName );
            }

            return false;
        }
        protected static int GetField( int? field, int defaultValue = 0 )
        {
            int value = field != null ? ( int )field : defaultValue;

            return value;
        } // end method
        protected static decimal GetField( decimal? field, decimal defaultValue = 0 )
        {
            decimal value = field != null ? ( decimal )field : defaultValue;

            return value;
        } // end method
        protected static Guid GetField( Guid? field, Guid defaultValue )
        {
            Guid value = field != null ? ( Guid )field : defaultValue;

            return value;
        } // end method

        protected static string GetMessages( List<string> messages )
        {
            if ( messages == null || messages.Count == 0 )
                return "";

            return string.Join( "<br/>", messages.ToArray() );

        }
        //protected static List<string> GetArray( string messages )
        //{
        //	List<string> list = new List<string>();
        //	if ( string.IsNullOrWhiteSpace( messages) )
        //		return list;
        //	string[] array = messages.Split( ',' );

        //	return list;
        //}
        /// <summary>
        /// Split a comma separated list into a list of strings
        /// </summary>
        /// <param name="csl"></param>
        /// <returns></returns>
        public static List<string> CommaSeparatedListToStringList( string csl )
        {
            if ( string.IsNullOrWhiteSpace( csl ) )
                return new List<string>();

            try
            {
                return csl.Trim().Split( new string[] { "," }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
        /// <summary>
        /// Get the current url for reporting purposes
        /// </summary>
        /// <returns></returns>
        public static string GetWebUrl()
        {
            string queryString = "n/a";
			try
			{
				if ( HttpContext.Current != null && HttpContext.Current.Request != null )
					queryString = HttpContext.Current.Request.RawUrl.ToString();
			} catch (Exception ex)
			{
				//ignore
			}
            return queryString;
        }


        #endregion
        #region Dynamic Sql
        public static DataTable ReadTable( string tableViewName, string orderBy = "" )
        {
            // Table to store the query results
            DataTable table = new DataTable();
            if ( string.IsNullOrWhiteSpace( tableViewName ) )
                return table;
            if ( tableViewName.IndexOf( "[" ) == -1 )
                tableViewName = "[" + tableViewName.Trim() + "]";
            string sql = string.Format( "SELECT * FROM {0} ", tableViewName );
            if ( !string.IsNullOrWhiteSpace( orderBy ) )
                sql += " Order by " + orderBy;

            string connectionString = DBConnectionRO();
            // Creates a SQL connection
            using ( var connection = new SqlConnection( DBConnectionRO() ) )
            {
                connection.Open();

                // Creates a SQL command
                using ( var command = new SqlCommand( sql, connection ) )
                {
                    // Loads the query results into the table
                    table.Load( command.ExecuteReader() );
                }

                connection.Close();
            }

            return table;
        }
        public static DataTable ReadSql( string sql )
        {
            // Table to store the query results
            DataTable table = new DataTable();
            if ( string.IsNullOrWhiteSpace( sql ) )
                return table;

            string connectionString = DBConnectionRO();
            // Creates a SQL connection
            using ( var connection = new SqlConnection( DBConnectionRO() ) )
            {
                connection.Open();

                // Creates a SQL command
                using ( var command = new SqlCommand( sql, connection ) )
                {
                    // Loads the query results into the table
                    table.Load( command.ExecuteReader() );
                }

                connection.Close();
            }

            return table;
        } //

        public static DataSet DoQuery( string sql )
        {
            DataSet ds = new DataSet();

            try
            {
                string connectionString = DBConnectionRO();
                //use default database connection for sql
               // ds = SqlHelper.ExecuteDataset( connectionString, System.Data.CommandType.Text, sql );

                if ( ds.Tables.Count == 0 )
                    ds = null;

                return ds;
            }

            catch ( Exception e )
            {

                LoggingHelper.LogError( e, string.Format( "DatabaseManager.DoQuery(): "
                    + "\r\nSQL:" + sql
                    + "\r\n" + e.ToString() ) );
                //return SetDataSetMessage( "DatabaseManager.DoQuery(): " + e.Message.ToString() );
                return null;
            }

        } //


        #endregion
        #region validations, etc
        public static bool IsValidDate( DateTime date )
        {
            if ( date != null && date > new DateTime( 1492, 1, 1 ) )
                return true;
            else
                return false;
        }

        public static bool IsValidDate( DateTime? date )
        {
            if ( date != null && date > new DateTime( 1492, 1, 1 ) )
                return true;
            else
                return false;
        }
        public static bool IsValidDate( string date )
        {
            DateTime validDate;
            if ( string.IsNullOrWhiteSpace( date ) || date.Length < 8 )
                return false;

            if ( !string.IsNullOrWhiteSpace( date )
                && DateTime.TryParse( date, out validDate )
                && date.Length >= 8
                && validDate > new DateTime( 1492, 1, 1 )
                )
                return true;
            else
                return false;
        }
        public static bool IsInteger( string nbr )
        {
            int validNbr = 0;
            if ( !string.IsNullOrWhiteSpace( nbr ) && int.TryParse( nbr, out validNbr ) )
                return true;
            else
                return false;
        }
        public static bool IsValid( string nbr )
        {
            int validNbr = 0;
            if ( !string.IsNullOrWhiteSpace( nbr ) && int.TryParse( nbr, out validNbr ) )
                return true;
            else
                return false;
        }

        protected bool IsValidGuid( Guid field )
        {
            if ( ( field == null || field.ToString() == DEFAULT_GUID ) )
                return false;
            else
                return true;
        }
        protected bool IsValidGuid( Guid? field )
        {
            if ( ( field == null || field.ToString() == DEFAULT_GUID ) )
                return false;
            else
                return true;
        }
        public static bool IsValidGuid( string field )
        {
            if ( string.IsNullOrWhiteSpace( field )
                || field.Trim() == DEFAULT_GUID
                || field.Length != 36
                )
                return false;
            else
                return true;
        }
        public static bool IsGuidValid( Guid field )
        {
            if ( ( field == null || field.ToString() == DEFAULT_GUID ) )
                return false;
            else
                return true;
        }

        public static bool IsGuidValid( Guid? field )
        {
            if ( ( field == null || field.ToString() == DEFAULT_GUID ) )
                return false;
            else
                return true;
        }
        public static string GetData( string text, string defaultValue = "" )
        {
            if ( string.IsNullOrWhiteSpace( text ) == false )
                return text.Trim();
            else
                return defaultValue;
        }
        public static int? SetData( int value, int minValue )
        {
            if ( value >= minValue )
                return value;
            else
                return null;
        }
        public static decimal? SetData( decimal value, decimal minValue )
        {
            if ( value >= minValue )
                return value;
            else
                return null;
        }
        public static DateTime? SetDate( string value )
        {
            DateTime output;
            if ( DateTime.TryParse( value, out output ) )
                return output;
            else
                return null;
        }
        public static string NormalizeUrlData( string text, string defaultValue = "" )
        {
            if ( string.IsNullOrWhiteSpace( text ) == false )
            {
                text = text.TrimEnd( '/' );
                return text.Trim();
            }
            else
                return defaultValue;
        }
        /// <summary>
        /// Validates the format of a Url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        //public static bool IsUrlWellFormed( string url )
        //{
        //	string responseStatus = "";

        //	if ( string.IsNullOrWhiteSpace( url ) )
        //		return true;
        //	if ( !Uri.IsWellFormedUriString( url, UriKind.Absolute ) )
        //	{
        //		responseStatus = "The URL is not in a proper format";
        //		return false;
        //	}

        //	//may need to allow ftp, and others - not likely for this context?
        //	if ( url.ToLower().StartsWith( "http" ) == false )
        //	{
        //		responseStatus = "A URL must begin with http or https";

        //		return false;
        //	}

        //	//NOTE - do NOT use the HEAD option, as many sites reject that type of request
        //	var isOk = DoesRemoteFileExists( url, ref responseStatus );
        //	//optionally try other methods, or again with GET
        //	if ( !isOk && responseStatus == "999" )
        //		isOk = true;

        //	return isOk;
        //}
        public static bool IsUrlValid( string url, ref string statusMessage, bool doingExistanceCheck = true )
        {
            statusMessage = "";
            if ( string.IsNullOrWhiteSpace( url ) )
                return true;

            if ( !Uri.IsWellFormedUriString( url, UriKind.Absolute ) )
            {
                statusMessage = "The URL is not in a proper format (for example, must begin with http or https).";
                return false;
            }

            //may need to allow ftp, and others - not likely for this context?
            if ( url.ToLower().StartsWith( "http" ) == false )
            {
                statusMessage = "A URL must begin with http or https";
                return false;
            }
            if ( !doingExistanceCheck )
                return true;

            bool isaImageUrl = false;
            var isOk = DoesRemoteFileExists( url, ref statusMessage, ref isaImageUrl );
            //optionally try other methods, or again with GET
            if ( !isOk && statusMessage == "999" )
                return true;
            //	isOk = DoesRemoteFileExists( url, ref responseStatus, "GET" );

            //if ( isOk )
            //{
            //    SafeBrowsing.Reputation rep = SafeBrowsing.CheckUrl( url );
            //    if ( rep != SafeBrowsing.Reputation.None )
            //    {
            //        statusMessage = string.Format( "Url ({0}) failed SafeBrowsing check.", url );
            //        return false;
            //    }
            //}
            if(isOk & isaImageUrl)
            {
                statusMessage = " This property should not contain an image URL ";
                return false;
            }

            return isOk;
        }

        public static bool IsImageUrlValid ( string url, ref string statusMessage, bool doingExistanceCheck = true )
        {
            bool isaImageUrl = false;

            if ( !Uri.IsWellFormedUriString( url, UriKind.Absolute ) )
            {
                statusMessage = "The URL is not in a proper format (for example, must begin with http or https).";
                return false;
            }

            //may need to allow ftp, and others - not likely for this context?
            //may want to allow just //. This is more likely with resources like images, stylesheets
                if ( url.ToLower().StartsWith( "http" ) == false && url.ToLower().StartsWith( "//" ) == false )
                {
                    statusMessage = "A URL must begin with http or https";
                    return false;
                }
            if ( !doingExistanceCheck )
                return true;

            if ( DoesRemoteFileExists( url, ref statusMessage, ref isaImageUrl ) )
             {
                if ( isaImageUrl == false )
                {
                    statusMessage = "This property is not a valid Image URL";
                    return false;
                }
                else return true;
            }
            else
            {
                return false;
            }
           
        }

        /// <summary>
        /// Checks the file exists or not.
        /// </summary>
        /// <param name="url">The URL of the remote file.</param>
        /// <returns>True : If the file exits, False if file not exists</returns>
        public static bool DoesRemoteFileExists( string url, ref string responseStatus,ref bool isaimageurl)
        {
            //the following is only used to handle certain errors. 
            //we may want a different property to allow publishing to skip completely (for speed, and already checked)
            bool doingLinkChecking = UtilityManager.GetAppKeyValue( "doingLinkChecking", true );
            //consider stripping off https?
            //or if not found and https, try http
            try
            {
                if ( SkippingValidation( url ) )
                    return true;

                SafeBrowsing.Reputation rep = SafeBrowsing.CheckUrl( url );
                if ( rep != SafeBrowsing.Reputation.None )
                {
                    responseStatus = string.Format( "Url ({0}) failed SafeBrowsing check.", url );
                    return false;
                }
                //Creating the HttpWebRequest
                HttpWebRequest request = WebRequest.Create( url ) as HttpWebRequest;
                //NOTE - do use the HEAD option, as many sites reject that type of request
                //request.Method = "GET";
                //var agent = HttpContext.Current.Request.AcceptTypes;

                //request.ContentType = "text/html;charset=\"utf-8\";image/*";

                request.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1309.0 Safari/537.17";

                //users may be providing urls to sites that have invalid ssl certs installed.You can ignore those cert problems if you put this line in before you make the actual web request:
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback( AcceptAllCertifications );

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;

                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if ( response != null )
                    if ( response.ContentType.ToLower( CultureInfo.InvariantCulture ).Contains( "image/" ) )
                    {
                        isaimageurl = true;
                    }
                //Returns TRUE if the Status code == 200
                response.Close();
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    if ( url.ToLower().StartsWith( "https:" ) )
                    {
                        url = url.ToLower().Replace( "https:", "http:" );
                        LoggingHelper.DoTrace( 5, string.Format( "_____________Failed for https, trying again using http: {0}", url ) );

                        return DoesRemoteFileExists( url, ref responseStatus, ref isaimageurl );
                    }
                    else
                    {
                        LoggingHelper.DoTrace( 5, string.Format( "Url validation failed for: {0}, using method: GET, with status of: {1}", url, response.StatusCode ) );
                    }
                }
                responseStatus = response.StatusCode.ToString();

                return ( response.StatusCode == HttpStatusCode.OK );
                //apparantly sites like Linked In have can be a  problem
                //http://stackoverflow.com/questions/27231113/999-error-code-on-head-request-to-linkedin
                //may add code to skip linked In?, or allow on fail - which the same.
                //or some update, refer to the latter link

                //
            }
            catch ( WebException wex )
            {
                responseStatus = wex.Message;
                //
                if ( wex.Message.IndexOf( "(404)" ) > 1 )
                    return false;
                else if ( wex.Message.IndexOf( "Too many automatic redirections were attempted" ) > -1 )
                    return false;
                else if ( wex.Message.IndexOf( "(999" ) > 1 )
                    return true;
                else if ( wex.Message.IndexOf( "(400) Bad Request" ) > 1 )
                    return true;
                else if ( wex.Message.IndexOf( "(401) Unauthorized" ) > 1 )
                    return true;
                else if ( wex.Message.IndexOf( "(406) Not Acceptable" ) > 1 )
                    return true;
                else if ( wex.Message.IndexOf( "(500) Internal Server Error" ) > 1 )
                    return true;
                else if ( wex.Message.IndexOf( "Could not create SSL/TLS secure channel" ) > 1 )
                {
                    //https://www.naahq.org/education-careers/credentials/certification-for-apartment-maintenance-technicians 
                    return true;

                }
                else if ( wex.Message.IndexOf( "Could not establish trust relationship for the SSL/TLS secure channel" ) > -1 )
                {
                    return true;
                }
                else if ( wex.Message.IndexOf( "The underlying connection was closed: An unexpected error occurred on a send" ) > -1 )
                {
                    return true;
                }
                else if ( wex.Message.IndexOf( "The connection was closed unexpectedly" ) > -1 )
                {
                    return true;
                }
                else if ( wex.Message.IndexOf( "Detail=CR must be followed by LF" ) > 1 )
                {
                    return true;
                }
                //var pageContent = new StreamReader( wex.Response.GetResponseStream() )
                //		 .ReadToEnd();
                if ( !doingLinkChecking )
                {
                    LoggingHelper.LogError( string.Format( "BaseFactory.DoesRemoteFileExists url: {0}. Exception Message:{1}; URL: {2}", url, wex.Message, GetWebUrl() ), true, "SKIPPING - Exception on URL Checking" );

                    return true;
                }

                LoggingHelper.LogError( string.Format( "BaseFactory.DoesRemoteFileExists url: {0}. Exception Message:{1}", url, wex.Message ), true, "Exception on URL Checking" );
                responseStatus = wex.Message;
                return false;
            }
            catch ( Exception ex )
            {

                if ( ex.Message.IndexOf( "(999" ) > -1 )
                {
                    //linked in scenario
                    responseStatus = "999";
                }
                else if ( ex.Message.IndexOf( "Could not create SSL/TLS secure channel" ) > 1 )
                {
                    //https://www.naahq.org/education-careers/credentials/certification-for-apartment-maintenance-technicians 
                    return true;

                }
                else if ( ex.Message.IndexOf( "(500) Internal Server Error" ) > 1 )
                {
                    return true;
                }
                else if ( ex.Message.IndexOf( "(401) Unauthorized" ) > 1 )
                {
                    return true;
                }
                else if ( ex.Message.IndexOf( "Could not establish trust relationship for the SSL/TLS secure channel" ) > 1 )
                {
                    return true;
                }
                else if ( ex.Message.IndexOf( "Detail=CR must be followed by LF" ) > 1 )
                {
                    return true;
                }
                if ( !doingLinkChecking )
                {
                    LoggingHelper.LogError( string.Format( "BaseFactory.DoesRemoteFileExists url: {0}. Exception Message:{1}", url, ex.Message ), true, "SKIPPING - Exception on URL Checking" );

                    return true;
                }

                LoggingHelper.LogError( string.Format( "BaseFactory.DoesRemoteFileExists url: {0}. Exception Message:{1}", url, ex.Message ), true, "Exception on URL Checking" );
                //Any exception will returns false.
                responseStatus = ex.Message;
                return false;
            }
        }
        public static bool AcceptAllCertifications( object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors )
        {
            return true;
        }
        private static bool SkippingValidation( string url )
        {


            Uri myUri = new Uri( url );
            string host = myUri.Host;

            string exceptions = UtilityManager.GetAppKeyValue( "urlExceptions" );
            //quick method to avoid loop
            if ( exceptions.IndexOf( host ) > -1 )
                return true;


            //string[] domains = exceptions.Split( ';' );
            //foreach ( string item in domains )
            //{
            //	if ( url.ToLower().IndexOf( item.Trim() ) > 5 )
            //		return true;
            //}

            return false;
        }

        public static bool IsPhoneValid( string phone, string type, ref List<string> messages )
        {
            bool isValid = true;

            string phoneNbr = PhoneNumber.StripPhone( GetData( phone ) );

            if ( !string.IsNullOrWhiteSpace( phoneNbr ) && phoneNbr.Length < 10 )
            {
                messages.Add( string.Format( "Error - A valid {0} ({1}) must have at least 10 numbers.", type, phone ) );
                return false;
            }


            return isValid;
        }
        #endregion
        public static string ConvertSpecialCharacters(string text)
        {
            bool hasChanged = false;

            return ConvertSpecialCharacters(text, ref hasChanged);
        }
        /// <summary>
        /// Convert characters often resulting from external programs like Word
        /// NOTE: keep in sync with the RegistryAssistant Api version
        /// </summary>
        /// <param name="text"></param>
        /// <param name="hasChanged"></param>
        /// <returns></returns>
        public static string ConvertSpecialCharacters( string text, ref bool hasChanged )
        {
            hasChanged = false;
            if ( string.IsNullOrWhiteSpace( text ) )
                return "";
            string orginal = text.Trim();
            if ( ContainsUnicodeCharacter(text) )
            {

                if ( text.IndexOf('\u2013') > -1 ) text = text.Replace('\u2013', '-'); // en dash
                if ( text.IndexOf('\u2014') > -1 ) text = text.Replace('\u2014', '-'); // em dash
                if ( text.IndexOf('\u2015') > -1 ) text = text.Replace('\u2015', '-'); // horizontal bar
                if ( text.IndexOf('\u2017') > -1 ) text = text.Replace('\u2017', '_'); // double low line
                if ( text.IndexOf('\u2018') > -1 ) text = text.Replace('\u2018', '\''); // left single quotation mark
                if ( text.IndexOf('\u2019') > -1 ) text = text.Replace('\u2019', '\''); // right single quotation mark
                if ( text.IndexOf('\u201a') > -1 ) text = text.Replace('\u201a', ','); // single low-9 quotation mark
                if ( text.IndexOf('\u201b') > -1 ) text = text.Replace('\u201b', '\''); // single high-reversed-9 quotation mark
                if ( text.IndexOf('\u201c') > -1 ) text = text.Replace('\u201c', '\"'); // left double quotation mark
                if ( text.IndexOf('\u201d') > -1 ) text = text.Replace('\u201d', '\"'); // right double quotation mark
                if ( text.IndexOf('\u201e') > -1 ) text = text.Replace('\u201e', '\"'); // double low-9 quotation mark
                if ( text.IndexOf('\u201f') > -1 ) text = text.Replace('\u201f', '\"'); // ???
                if ( text.IndexOf('\u2026') > -1 ) text = text.Replace("\u2026", "..."); // horizontal ellipsis
                if ( text.IndexOf('\u2032') > -1 ) text = text.Replace('\u2032', '\''); // prime
                if ( text.IndexOf('\u2033') > -1 ) text = text.Replace('\u2033', '\"'); // double prime
                if ( text.IndexOf('\u2036') > -1 ) text = text.Replace('\u2036', '\"'); // ??
                if ( text.IndexOf('\u0090') > -1 ) text = text.Replace('\u0090', 'ê'); // e circumflex
            }
			text = text.Replace( "â€™", "'" );
			text = text.Replace( "â€\"", "-" );
			text = text.Replace( "\"â€ú", "-" );
			//
			//don't do this as \r is valid
			text = text.Replace( "\\\\r", "" );
			
			text = text.Replace( "\u009d", " " ); //
			text = text.Replace( "Ã,Â", "" ); //

			text = Regex.Replace( text, "’", "'" );
			text = Regex.Replace( text, "“", "'" );
			text = Regex.Replace( text, "”", "'" );
			//BIZARRE
			text = Regex.Replace( text, "Ã¢â,¬â\"¢", "'" );
			text = Regex.Replace( text, "–", "-" );

			text = Regex.Replace(text, "[Õ]", "'");
            text = Regex.Replace(text, "[Ô]", "'");
            text = Regex.Replace(text, "[Ò]", "\"");
            text = Regex.Replace(text, "[Ó]", "\"");
            text = Regex.Replace(text, "[Ñ]", " -"); //Ñ
            text = Regex.Replace(text, "[Ž]", "é");
            text = Regex.Replace(text, "[ˆ]", "à");
            text = Regex.Replace(text, "[Ð]", "-");
            //
            text = text.Replace("‡", "á"); //Ã³

			text = text.Replace( "ÃƒÂ³", "ó" ); //
			text = text.Replace( "Ã³", "ó" ); //
			//é
			text = text.Replace( "ÃƒÂ©", "é" ); //
			text = text.Replace( "Ã©", "é" ); //

			text = text.Replace( "ÃƒÂ¡", "á" ); //
			text = text.Replace( "Ã¡", "á" ); //Ã¡
			text = text.Replace( "ÃƒÂ", "à" ); //
			//
			text = text.Replace( "ÃƒÂ±", "ñ" ); //
			text = text.Replace( "Â±", "ñ" ); //"Ã±"
											  //
			text = text.Replace( "ÃƒÂ-", "í" ); //???? same as à
			text = text.Replace( "ÃƒÂ­­", "í" ); //"Ã­as" "gÃ­a" "gÃ­as"
			text = text.Replace( "gÃ­as", "gías" ); //"Ã­as" "gÃ­a" "gÃ­as"
			text = text.Replace( "’", "í" ); //


			text = text.Replace( "ÃƒÂº", "ú" ); //"Ãº"
			text = text.Replace( "Âº", "ú" ); //"Ãº"
            text = text.Replace("œ", "ú"); //

            text = text.Replace("quÕˆ", "qu'à"); //
            text = text.Replace("qu'ˆ", "qu'à"); //
            text = text.Replace("ci—n ", "ción ");
			//"Â¨"
			text = text.Replace( "Â¨", "®" ); //

			text = text.Replace("teor'as", "teorías"); // 
            text = text.Replace("log'as", "logías"); //
            text = text.Replace("ense–anza", "enseñanza"); //
			//
			text = text.Replace( "Ã¢â,¬Ãº", "\"" ); //
			text = text.Replace( "Ã¢â,¬Â", "\"" ); //
													//

			//not sure if should do this arbitrarily here?
			if ( text.IndexOf( "Ã" ) > -1 || text.IndexOf( "Â" ) > -1 )
			{
				string queryString = GetWebUrl();
				LoggingHelper.DoTrace( 1, string.Format("@#@#@# found text containing Ã or Â, setting to blank. URL: {0}, Text:\r{1}", queryString, text ) );
				text = text.Replace( "Ã", "" ); //
				text = text.Replace( ",Â", "" ); //
				text = text.Replace( "Â", "" ); //
			}


			text = text.Replace("ou�ll", "ou'll"); //
            text = text.Replace("�s", "'s"); // 
			text = text.Replace( "�", "" ); // 
			text = Regex.Replace(text, "[—]", "-"); //

            text = Regex.Replace(text, "[�]", " "); //could be anything
            //covered above
            //text = Regex.Replace(text, "[«»\u201C\u201D\u201E\u201F\u2033\u2036]", "\"");
            //text = Regex.Replace(text, "[\u2026]", "...");

            //
            
            if ( orginal != text.Trim())
            {
                //should report any changes
                hasChanged = true;
                //text = orginal;
            }
            return text.Trim();
        } //

        public static bool ContainsUnicodeCharacter(string input)
        {
            const int MaxAnsiCode = 255;

            return input.Any(c => c > MaxAnsiCode);
        }
        public static void ListUnicodeCharacter(string input)
        {
            if ( !ContainsUnicodeCharacter(input) )
                return;

            //string chg = Regex.Match(input, @"[^\u0000-\u007F]", "");
            
        }
        /// <summary>
        /// Format a title (such as for a library) to be url friendly
        /// NOTE: there are other methods:
        /// ILPathways.Utilities.UtilityManager.UrlFriendlyTitle()
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string FormatFriendlyTitle( string text )
        {
            if ( text == null || text.Trim().Length == 0 )
                return "";

            string title = UrlFriendlyTitle( text );

            //encode just incase
            title = HttpUtility.HtmlEncode( title );
            return title;
        }
        /// <summary>
        /// Format a title (such as for a library) to be url friendly
        /// NOTE: there are other methods:
        /// ILPathways.Utilities.UtilityManager.UrlFriendlyTitle()
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private static string UrlFriendlyTitle( string title )
        {
            if ( title == null || title.Trim().Length == 0 )
                return "";

            title = title.Trim();

            string encodedTitle = title.Replace( " - ", "-" );
            encodedTitle = encodedTitle.Replace( " ", "_" );

            //for now allow embedded periods
            //encodedTitle = encodedTitle.Replace( ".", "-" );

            encodedTitle = encodedTitle.Replace( "'", "" );
            encodedTitle = encodedTitle.Replace( "&", "-" );
            encodedTitle = encodedTitle.Replace( "#", "" );
            encodedTitle = encodedTitle.Replace( "$", "S" );
            encodedTitle = encodedTitle.Replace( "%", "percent" );
            encodedTitle = encodedTitle.Replace( "^", "" );
            encodedTitle = encodedTitle.Replace( "*", "" );
            encodedTitle = encodedTitle.Replace( "+", "_" );
            encodedTitle = encodedTitle.Replace( "~", "_" );
            encodedTitle = encodedTitle.Replace( "`", "_" );
            encodedTitle = encodedTitle.Replace( "/", "_" );
            encodedTitle = encodedTitle.Replace( "://", "/" );
            encodedTitle = encodedTitle.Replace( ":", "" );
            encodedTitle = encodedTitle.Replace( ";", "" );
            encodedTitle = encodedTitle.Replace( "?", "" );
            encodedTitle = encodedTitle.Replace( "\"", "_" );
            encodedTitle = encodedTitle.Replace( "\\", "_" );
            encodedTitle = encodedTitle.Replace( "<", "_" );
            encodedTitle = encodedTitle.Replace( ">", "_" );
            encodedTitle = encodedTitle.Replace( "__", "_" );
            encodedTitle = encodedTitle.Replace( "__", "_" );
            encodedTitle = encodedTitle.Replace( "..", "_" );
            encodedTitle = encodedTitle.Replace( ".", "_" );

            if ( encodedTitle.EndsWith( "." ) )
                encodedTitle = encodedTitle.Substring( 0, encodedTitle.Length - 1 );

            return encodedTitle;
        } //
        public static string GenerateFriendlyName( string name )
        {
            if ( name == null || name.Trim().Length == 0 )
                return "";
            //another option could be use a pattern like the following?
            //string phrase = string.Format( "{0}-{1}", Id, name );

            string str = RemoveAccent( name ).ToLower();
            // invalid chars           
            str = Regex.Replace( str, @"[^a-z0-9\s-]", "" );
            // convert multiple spaces into one space   
            str = Regex.Replace( str, @"\s+", " " ).Trim();
            // cut and trim 
            str = str.Substring( 0, str.Length <= 45 ? str.Length : 45 ).Trim();
            str = Regex.Replace( str, @"\s", "-" ); // hyphens   
            return str;
        }
        private static string RemoveAccent( string text )
        {
            byte[] bytes = System.Text.Encoding.GetEncoding( "Cyrillic" ).GetBytes( text );
            return System.Text.Encoding.ASCII.GetString( bytes );
        }
        protected string HandleDBValidationError( System.Data.Entity.Validation.DbEntityValidationException dbex, string source, string title )
        {
            string message = string.Format( "{0} DbEntityValidationException, Name: {1}", source, title );

            foreach ( var eve in dbex.EntityValidationErrors )
            {
                message += string.Format( "\rEntity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    eve.Entry.Entity.GetType().Name, eve.Entry.State );
                foreach ( var ve in eve.ValidationErrors )
                {
                    message += string.Format( "- Property: \"{0}\", Error: \"{1}\"",
                        ve.PropertyName, ve.ErrorMessage );
                }

                LoggingHelper.LogError( message, true );
            }

            return message;
        }

        public static string FormatExceptions( Exception ex )
        {
            string message = ex.Message;

            if ( ex.InnerException != null )
            {
                message += FormatExceptions( ex );
            }

            return message;
        }

        /// <summary>
        /// Strip off text that is randomly added that starts with jquery
        /// Will need additional check for numbers - determine actual format
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string StripJqueryTag( string text )
        {
            int pos2 = text.ToLower().IndexOf( "jquery" );
            if ( pos2 > 1 )
            {
                text = text.Substring( 0, pos2 );
            }

            return text;
        }

    }
}
