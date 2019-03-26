using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;
using Models.Common;
using System.Linq.Expressions;
using LumenWorks.Framework.IO.Csv;


namespace CTIServices.Import
{
	public class ImportUtilities
    {
		public class UploadAttempt<T> where T : new()
		{
			public UploadAttempt()
			{
				Metadata = new Dictionary<string, object>();
				OwningOrganization = new Organization();
				UploadItem = new T();
			}
			public T UploadItem { get; set; }
			public Organization OwningOrganization { get; set; }
			public Dictionary<string, object> Metadata { get; set; }
			public bool WasSuccessful { get; set; }
			public string Message { get; set; }
		}
		//

	}
}
