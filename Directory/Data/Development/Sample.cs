using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;

namespace Data.Development
{
	public class Sample
	{
		//Get a sample credential
		public Credential GetSampleCredential( int id, string name )
		{
			var result = new Credential();
			result.Id = id;
			result.Name = name;
			return result;
		}
		public Credential GetSampleCredential( int id )
		{
			return GetSampleCredential( id, "Sample Credential" );
		}
		public Credential GetSampleCredential()
		{
			return GetSampleCredential( 99, "Sample Credential" );
		}

		//Get a sample organization
		public Organization GetSampleOrganization( int id )
		{
			return GetSampleOrganization( id, "Sample Organization" );
		}
		public Organization GetSampleOrganization( int id, string name )
		{
			var result = new Organization();
			result.Id = id;
			result.Name = name;
			return result;
		}
	
		public Organization GetSampleOrganization()
		{
			return GetSampleOrganization( 99, "Sample  Organization" );
		}

		//Get a sample enumeration
		public Enumeration GetSampleEnumeration( string dataSource, string schemaName, EnumerationType interfaceType )
		{
			var result = TemporaryEnumerationData.GetEnumeration( dataSource, schemaName );
			result.InterfaceType = interfaceType;

			return result;
		}
		public Enumeration GetSampleEnumeration( string dataSource, EnumerationType interfaceType )
		{
			return GetSampleEnumeration( dataSource, dataSource, interfaceType );
		}
		public Enumeration GetSampleEnumeration( string dataSource )
		{
			return GetSampleEnumeration( dataSource, EnumerationType.MULTI_SELECT );
		}

	}
}
