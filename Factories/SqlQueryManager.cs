using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Helpers;

using Utilities;
using DBEntity = Data.SqlQuery;
using ThisEntity = Models.Helpers.SqlQuery;


namespace Factories
{
	public class SqlQueryManager : BaseFactory
	{
		static string thisClassName = "SqlQueryManager";

		/// <summary>
		/// select a single SqlQuery entity using the Id
		/// </summary>
		/// <param name="pid"></param>
		/// <returns></returns>
		public static SqlQuery Get( int pid )
		{
			ThisEntity entity = new ThisEntity();
			if ( pid < 1 )
				return entity;

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBEntity item = context.SqlQuery
							.SingleOrDefault( s => s.Id == pid );

					if ( item != null && item.Id > 0 )
					{
						MapFromDB( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get( int pid )" );
			}
			return entity;
		}//

		/// <summary>
		/// select a single SqlQuery entity using the query code
		/// </summary>
		/// <param name="pQueryCode"></param>
		/// <returns></returns>
		public static SqlQuery Get( string pQueryCode )
		{
			ThisEntity entity = new ThisEntity();
			if ( string.IsNullOrWhiteSpace( pQueryCode ) )
				return entity;

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBEntity item = context.SqlQuery
							.SingleOrDefault( s => s.QueryCode == pQueryCode );

					if ( item != null && item.Id > 0 )
					{
						MapFromDB( item, entity );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get( string pQueryCode )" );
			}
			return entity;
		}//

		public static void MapFromDB( DBEntity from, ThisEntity entity )
		{
			entity.Id = from.Id;
			entity.Title = from.Title;
			entity.Description = from.Description;
			entity.QueryCode = from.QueryCode;
			entity.Category = from.Category;
			entity.SQL = from.SQL;
			entity.OwnerId = (from.OwnerId ?? 0) ;
			entity.IsPublic = (from.IsPublic ?? true);
			entity.Created = from.Created != null ? (DateTime) from.Created : DateTime.Now;
			entity.CreatedBy = from.CreatedBy;
			entity.LastUpdated = from.LastUpdated != null ? ( DateTime ) from.LastUpdated : DateTime.Now;
			entity.LastUpdatedBy = from.LastUpdatedBy;

		}//

	}
}
