using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Factories;

namespace CTIServices
{
	public class SiteServices
	{
		public void ImportMicrosoftCredentials( ref List<string> summary, int maxRecords )
		{
			//List<string> summary
			MicrosoftImport mgr = new MicrosoftImport();
			//mgr.ImportMicrosoftCredentials( ref summary, maxRecords );

			//List<AppUser> users = AccountManager.ImportUsers_GetAll( maxRecords );

			//return users;

		}
	}
}
