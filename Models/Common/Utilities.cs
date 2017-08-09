using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Common
{
	public class Utilities
	{
		public static string GetWebConfigValue( string key, string defaultValue = "")
		{
			try
			{
				return string.IsNullOrWhiteSpace( System.Configuration.ConfigurationManager.AppSettings[ key ] ) ? defaultValue : System.Configuration.ConfigurationManager.AppSettings[ key ];
			}
			catch
			{
				return defaultValue;
			}
		}
		//
	}
}
