using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web;

using Utilities;

namespace Factories
{
	public class BaseFactory
	{
		protected static string DEFAULT_GUID = "00000000-0000-0000-0000-000000000000";
		public static bool IsDevEnv()
		{
			if ( UtilityManager.GetAppKeyValue( "envType", "no" ) == "dev" )
				return true;
			else
				return false;
		}
		#region Entity frameworks helpers
		public bool HasStateChanged( Data.CTIEntities context  )
		{
			if ( context.ChangeTracker.Entries().Any( e => 
					e.State == EntityState.Added  || 
					e.State == EntityState.Modified || 
					e.State == EntityState.Deleted ) == true )
				return true;
			else
				return false;
		}
		#endregion
		#region Database connections
		/// <summary>
		/// Get the read only connection string for the main database
		/// </summary>
		/// <returns></returns>
		public static string DBConnectionRO()
		{

			string conn = WebConfigurationManager.ConnectionStrings[ "cti_RO" ].ConnectionString;
			return conn;

		}

		#endregion

		#region data retrieval
		/// <summary>
		/// Helper method to retrieve a string column from a row while handling invalid values
		/// </summary>
		/// <param name="row">DataRow</param>
		/// <param name="column">Column Name</param>
		/// <param name="defaultValue">Default value to return if column data is invalid</param>
		/// <returns></returns>
		public static string GetRowColumn( DataRow row, string column, string defaultValue = "" )
		{
			string colValue = "";

			try
			{
				colValue = row[ column ].ToString();

			}
			catch ( System.FormatException fex )
			{
				//Assuming FormatException means null or invalid value, so can ignore
				colValue = defaultValue;

			}
			catch ( Exception ex )
			{
				string queryString = GetWebUrl();
				string exType = ex.GetType().ToString();
				LoggingHelper.LogError( exType + " Exception in GetRowColumn( DataRow row, string column, string defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
				
				colValue = defaultValue;
			}
			return colValue;

		}

		public static string GetRowPossibleColumn( DataRow row, string column, string defaultValue = "" )
		{
			string colValue = "";

			try
			{
				colValue = row[ column ].ToString();

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
				colValue = Int32.Parse( row[ column ].ToString() );

			}
			catch ( System.FormatException fex )
			{
				//Assuming FormatException means null or invalid value, so can ignore
				colValue = defaultValue;

			}
			catch ( Exception ex )
			{
				string queryString = GetWebUrl();

				LoggingHelper.LogError( "Exception in GetRowColumn( DataRow row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
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
				colValue = Int32.Parse( row[ column ].ToString() );

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
		public static double GetRowColumn( DataRow row, string column, double defaultValue )
		{
			double colValue = 0;

			try
			{
				colValue = double.Parse( row[ column ].ToString() );

			}
			catch ( System.FormatException fex )
			{
				//Assuming FormatException means null or invalid value, so can ignore
				colValue = defaultValue;

			}
			catch ( Exception ex )
			{
				string queryString = GetWebUrl();

				LoggingHelper.LogError( "Exception in GetRowColumn( DataRow row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
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
				string strValue = row[ column ].ToString();
				if ( !string.IsNullOrWhiteSpace( strValue ) && strValue.Trim().Length == 1 )
				{
					strValue = strValue.Trim();
					if ( strValue == "0" )
						return false;
					else if ( strValue == "1" )
						return true;
				}
				colValue = bool.Parse( row[ column ].ToString() );

			}
			catch ( System.FormatException fex )
			{
				//Assuming FormatException means null or invalid value, so can ignore
				colValue = defaultValue;

			}
			catch ( Exception ex )
			{
				string queryString = GetWebUrl();

				LoggingHelper.LogError( "Exception in GetRowColumn( DataRow row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
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
				string strValue = row[ column ].ToString();
				if (DateTime.TryParse(strValue, out colValue) == false)
					colValue = defaultValue;
			}
			catch ( System.FormatException fex )
			{
				//Assuming FormatException means null or invalid value, so can ignore
				colValue = defaultValue;

			}
			catch ( Exception ex )
			{
				string queryString = GetWebUrl();

				LoggingHelper.LogError( "Exception in GetRowColumn( DataRow row, string column, int defaultValue ) for column: " + column + ". \r\n" + ex.Message.ToString() + "\r\nLocation: " + queryString, true );
				colValue = defaultValue;
				//throw ex;
			}
			return colValue;

		} 
		protected static int GetField( int? field, int defaultValue = 0 )
		{
			int value = field != null ? ( int ) field : defaultValue;

			return value;
		} // end method
		protected static Guid GetField( Guid? field, Guid defaultValue )
		{
			Guid value = field != null ? ( Guid ) field : defaultValue;

			return value;
		} // end method

		protected static string GetMessages(List<string> messages)
		{
			if ( messages == null || messages.Count == 0 )
				return "";

			return string.Join( ",", messages.ToArray() );

		}
		//protected static List<string> GetArray( string messages )
		//{
		//	List<string> list = new List<string>();
		//	if ( string.IsNullOrWhiteSpace( messages) )
		//		return list;
		//	string[] array = messages.Split( ',' );

		//	return list;
		//}
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
		private static string GetWebUrl()
		{
			string queryString = "n/a";

			if ( HttpContext.Current != null && HttpContext.Current.Request != null )
				queryString = HttpContext.Current.Request.RawUrl.ToString();

			return queryString;
		}
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
			if ( !string.IsNullOrWhiteSpace( date ) && DateTime.TryParse( date, out validDate ) && validDate > new DateTime( 1492, 1, 1 ) )
				return true;
			else
				return false;
		}
		public static bool IsInteger( string nbr )
		{
			int validNbr=0;
			if ( !string.IsNullOrWhiteSpace( nbr ) && int.TryParse( nbr, out validNbr )  )
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

		protected  bool IsValidGuid( Guid field )
		{
			if ( ( field == null || field.ToString() == DEFAULT_GUID )  )
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
			if ( string.IsNullOrWhiteSpace(text) == false)
				return text;
			else
				return defaultValue;
		}

		#endregion
		
	}
}
