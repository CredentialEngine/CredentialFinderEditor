using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.AspNet.Identity.Core;
//using Microsoft.AspNet.Identity;
//using Microsoft.AspNet.Identity.Owin;
//using Microsoft.Owin.Security;

using Models;
using EM = Data;
using Utilities;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;

namespace Factories
{
	public class AccountManager : BaseFactory
	{
		static string thisClassName = "AccountManager";

		static int Administrator = 1;
		static int SiteManager = 2;
		static int SiteStaff = 3;
		static int SitePartner = 4;
		static int SiteReader = 5;

		#region persistance 
		public int Account_Add( AppUser entity, ref string statusMessage )
		{
			EM.Account efEntity = new EM.Account();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{

					AppUser_FromMap( entity, efEntity );
					efEntity.RowId = Guid.NewGuid();

					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Account.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = string.Format( "AccountManager. Account_Add Failed", "Attempted to add a Account. The process appeared to not work, but was not an exception, so we have no message, or no clue.Email: {0}", entity.Email );
						EmailManager.NotifyAdmin( "	Manager. Account_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".Account_Add() DbEntityValidationException, Email: {0}", efEntity.Email );
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
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_Add(), Email: {0}", efEntity.Email ) );
				}
			}

			return efEntity.Id;
		}

		public int Account_AddFromAspNetUser( string aspNetId, AppUser entity, ref string statusMessage )
		{
			EM.Account efEntity = new EM.Account();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					EM.AspNetUsers user = AspNetUser_Get( entity.Email );

					AppUser_FromMap( entity, efEntity );

					efEntity.RowId = Guid.NewGuid();

					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdated = System.DateTime.Now;

					context.Account.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						entity.Id = efEntity.Id;

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = string.Format( "AccountManager. Account_Add Failed", "Attempted to add a Account. The process appeared to not work, but was not an exception, so we have no message, or no clue.Email: {0}", entity.Email );
						EmailManager.NotifyAdmin( "	Manager. Account_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".Account_Add() DbEntityValidationException, Email: {0}", efEntity.Email );
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
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_Add(), Email: {0}", efEntity.Email ) );
				}
			}

			return efEntity.Id;
		}

		public bool Account_Update( AppUser entity, ref string statusMessage )
		{
			using ( var context = new EM.CTIEntities() )
			{
				try
				{
					EM.Account efEntity = context.Account
							.SingleOrDefault( s => s.Id == entity.Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						AppUser_FromMap( entity, efEntity );
						efEntity.LastUpdated = System.DateTime.Now;

						// submit the change to database
						int count = context.SaveChanges();
						if ( count > 0 )
						{
							statusMessage = "successful";
							//arbitrarily update AspNetUsers???
							AspNetUsers_Update( entity, ref statusMessage );

							return true;
						}
						else
						{
							//?no info on error
							statusMessage = "Error - the update was not successful. ";
							string message = string.Format( thisClassName + ".Account_Update Failed", "Attempted to uddate a Account. The process appeared to not work, but was not an exception, so we have no message, or no clue.Email: {0}", entity.Email );
							EmailManager.NotifyAdmin( thisClassName +  ". Account_Update Failed", message );
						}
					}

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_Update(), Email: {0}", entity.Email ) );
				}
			}

			return false;
		}

		public bool AspNetUsers_Update( AppUser entity, ref string statusMessage )
		{
			using ( var context = new EM.CTIEntities() )
			{
				try
				{
					EM.AspNetUsers efEntity = context.AspNetUsers
							.SingleOrDefault( s => s.Id == entity.AspNetUserId );

					if ( efEntity != null && efEntity.UserId > 0 )
					{
						efEntity.FirstName = entity.FirstName;
						efEntity.LastName = entity.LastName;
						efEntity.Email = entity.Email;
						//could be dangerous, as hidden??
						//efEntity.UserName = entity.UserName;

						if ( HasStateChanged( context ) )
						{
							// submit the change to database
							int count = context.SaveChanges();
							if ( count > 0 )
							{
								statusMessage = "successful";
								return true;
							}
							else
							{
								//?no info on error
								statusMessage = "Error - the update was not successful. ";
								string message = string.Format( thisClassName + ".AspNetUsers_Update Failed", "Attempted to update AspNetUsers (sync with Account). The process appeared to not work, but was not an exception, so we have no message, or no clue.Email: {0}", entity.Email );
								EmailManager.NotifyAdmin( thisClassName + "AspNetUsers_Update Failed", message );
							}
						}
						
					}

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".AspNetUsers_Update(), Email: {0}", entity.Email ) );
				}
			}

			return false;
		}

		#endregion 

		#region retrieval
		public static AppUser AppUser_Get( int Id )
		{
			AppUser entity = new AppUser();
			using ( var context = new ViewContext() )
			{
				Views.Account_Summary item = context.Account_Summary
							.SingleOrDefault( s => s.Id == Id );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity );
				}
			}

			return entity;
		}
		public static AppUser AppUser_GetByEmail( string email )
		{
			AppUser entity = new AppUser();
			using ( var context = new ViewContext() )
			{
				Views.Account_Summary efEntity = context.Account_Summary
							.SingleOrDefault( s => s.Email.ToLower() == email.ToLower() );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					ToMap( efEntity, entity );
				}
			}

			return entity;
		}
		public static AppUser GetUserByUserName( string username )
		{
			AppUser entity = new AppUser();
			using ( var context = new ViewContext() )
			{
				Views.Account_Summary efEntity = context.Account_Summary
							.SingleOrDefault( s => s.UserName.ToLower() == username.ToLower() );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					ToMap( efEntity, entity );
				}
			}

			return entity;
		}

		public static AppUser AppUser_GetByKey( string aspNetId )
		{
			AppUser entity = new AppUser();
			using ( var context = new ViewContext() )
			{
				Views.Account_Summary efEntity = context.Account_Summary
							.SingleOrDefault( s => s.AspNetId == aspNetId );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					ToMap( efEntity, entity );
				}
			}

			return entity;
		}
		public static AppUser AppUser_GetFromAspUser( string email )
		{
			AppUser entity = new AppUser();
			using ( var context = new Data.CTIEntities() )
			{
				EM.AspNetUsers efEntity = context.AspNetUsers
							.SingleOrDefault( s => s.Email.ToLower() == email.ToLower() );

				if ( efEntity != null && efEntity.Email != null && efEntity.Email.Length > 5 )
				{
					entity = AppUser_GetByEmail( efEntity.Email );
				}
			}

			return entity;
		}

		public static EM.AspNetUsers AspNetUser_Get( string email )
		{
			EM.AspNetUsers entity = new EM.AspNetUsers();
			using ( var context = new Data.CTIEntities() )
			{
				entity = context.AspNetUsers
							.SingleOrDefault( s => s.Email.ToLower() == email.ToLower() );

				if ( entity != null && entity.Email != null && entity.Email.Length > 5 )
				{
					
				}
			}

			return entity;
		}

		public static List<AppUser> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows, int userId = 0 )
		{
			string connectionString = DBConnectionRO();
			AppUser item = new AppUser();
			List<AppUser> list = new List<AppUser>();
			var result = new DataTable();
			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}

				using ( SqlCommand command = new SqlCommand( "AccountSearch", c ) )
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add( new SqlParameter( "@Filter", pFilter ) );
					command.Parameters.Add( new SqlParameter( "@SortOrder", pOrderBy ) );
					command.Parameters.Add( new SqlParameter( "@StartPageIndex", pageNumber ) );
					command.Parameters.Add( new SqlParameter( "@PageSize", pageSize ) );
					command.Parameters.Add( new SqlParameter( "@CurrentUserId", userId ) );

					SqlParameter totalRows = new SqlParameter( "@TotalRows", pTotalRows );
					totalRows.Direction = ParameterDirection.Output;
					command.Parameters.Add( totalRows );

					using ( SqlDataAdapter adapter = new SqlDataAdapter() )
					{
						adapter.SelectCommand = command;
						adapter.Fill( result );
					}
					string rows = command.Parameters[ 5 ].Value.ToString();
					try
					{
						pTotalRows = Int32.Parse( rows );
					}
					catch
					{
						pTotalRows = 0;
					}
				}

				foreach ( DataRow dr in result.Rows )
				{
					item = new AppUser();
					item.Id = GetRowColumn( dr, "Id", 0 );
					item.FirstName = GetRowColumn( dr, "FirstName", "missing" );
					item.LastName = GetRowColumn( dr, "LastName", "" );
					string rowId = GetRowColumn( dr, "RowId" );
					item.RowId = new Guid( rowId );

					item.Email = GetRowColumn( dr, "Email", "" );

					list.Add( item );
				}

				return list;

			}
		}
		public static List<AppUser> ImportUsers_GetAll( int maxRecords = 0 )
		{
			if ( maxRecords == 0 )
				maxRecords = 50;
			int pTotalRows = 0;
			AppUser item = new AppUser();
			List<AppUser> list = new List<AppUser>();

			using ( var context = new Data.CTIEntities() )
			{
				var Query = from Results in context.AspNetUser_Import
						.Where( s => s.IsImported == false )
						.OrderBy( s => s.Id	 )
							select Results;

				pTotalRows = Query.Count();
				var results = Query.Take( maxRecords )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( EM.AspNetUser_Import row in results )
					{
						item = new AppUser();
						item.Id = row.Id;
						item.FirstName = row.FirstName;
						item.LastName = row.LastName;
						item.Email = row.Email;
						item.PrimaryOrgId = ( int ) ( row.OrgId ?? 0 );
						item.DefaultRoleId = ( int ) ( row.DefaultRoleId ?? 0 );
						
						list.Add( item );

					}

				}
			}
			return list;
		}
		public bool ImportUsers_Update( int importId, int userId, ref string statusMessage )
		{
			using ( var context = new EM.CTIEntities() )
			{
				try
				{
					EM.AspNetUser_Import efEntity = context.AspNetUser_Import
							.SingleOrDefault( s => s.Id == importId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.IsImported = true;
						efEntity.UserId = userId;

						// submit the change to database
						int count = context.SaveChanges();
						if ( count > 0 )
						{
							statusMessage = "successful";
							return true;
						}
						else
						{
							//?no info on error
							statusMessage = "Error - the update was not successful. ";
							string message = string.Format( "AccountManager. ImportUsers_Update Failed", "Attempted to uddate a ImportUsers. The process appeared to not work, but was not an exception, so we have no message, or no clue. importId: {0}", importId );
							EmailManager.NotifyAdmin( "	Manager. ImportUsers_Update Failed", message );
						}
					}

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".ImportUsers_Update(), importId: {0}", importId ) );
				}
			}

			return false;
		}

		//public void GetAllUsersInRole( string role )
		//{
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		var customers = context.AspNetUsers
		//			  .Where( u => u.AspNetUserRoles.Any( r => r..Name == role )  )
		//			  .ToList();
		//	}
		//}
		public static void ToMap( Views.Account_Summary fromEntity, AppUser to )
		{
			to.Id = fromEntity.Id;
			//to.RowId = fromEntity.RowId;
			to.UserName = fromEntity.UserName;

			to.AspNetUserId = fromEntity.AspNetId;
			//to.Password = fromEntity.Password;
			to.IsActive = fromEntity.IsActive == null ? false : ( bool ) fromEntity.IsActive;
			to.FirstName = fromEntity.FirstName;
			to.LastName = fromEntity.LastName;
			to.SortName = fromEntity.SortName;

			to.Email = fromEntity.Email;
			to.Created = fromEntity.Created;
			to.LastUpdated = fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById == null ? 0 : ( int ) fromEntity.LastUpdatedById;

			to.Roles = new List<string>();
			if ( string.IsNullOrWhiteSpace( fromEntity.Roles ) == false )
			{
				var roles = fromEntity.Roles.Split( ',' );
				foreach ( string role in roles )
				{
					to.Roles.Add( role );
				}
			}

		} //
		public static void ToMap( EM.Account fromEntity, AppUser to )
		{
			to.Id = fromEntity.Id;
			//to.RowId = fromEntity.RowId;
			to.UserName = fromEntity.UserName;

			to.AspNetUserId = fromEntity.AspNetId;
			//to.Password = fromEntity.Password;
			to.IsActive = fromEntity.IsActive == null ? false : ( bool ) fromEntity.IsActive;
			to.FirstName = fromEntity.FirstName;
			to.LastName = fromEntity.LastName;

			to.Email = fromEntity.Email;
			to.Created = fromEntity.Created;
			to.LastUpdated = fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById == null ? 0 : ( int ) fromEntity.LastUpdatedById;

			to.Roles = new List<string>();
			if ( fromEntity.AspNetUsers != null )
			{
				foreach ( EM.AspNetUserRoles role in fromEntity.AspNetUsers.AspNetUserRoles )
				{
					to.Roles.Add( role.AspNetRoles.Name );
				}
			}

		} //
		//NOTE: AspNetRoles is to be a guid, so not likely to use this version
		//public void GetAllUsersInRole( int roleId )
		//{
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		var customers = context.AspNetUsers
		//			  .Where( u => u.AspNetRoles.Any( r => r.Id == roleId ) )
		//			  .ToList();
		//	}
		//}
		private static void AppUser_FromMap( AppUser fromEntity, EM.Account to )
		{
			to.Id = fromEntity.Id;
			to.RowId = fromEntity.RowId;
			to.UserName = fromEntity.UserName;
			to.AspNetId = fromEntity.AspNetUserId;
			to.Password = fromEntity.Password;
			to.IsActive = fromEntity.IsActive;

			to.FirstName = fromEntity.FirstName;
			to.LastName = fromEntity.LastName;
			to.Email = fromEntity.Email;

			to.Created = fromEntity.Created;
			to.LastUpdated = fromEntity.LastUpdated;
			to.LastUpdatedById = fromEntity.LastUpdatedById;

		}

		#endregion 

		#region Roles
		public bool Account_AddRole( int userId, int roleId, int createdByUserId, ref string statusMessage )
		{
			bool isValid = true;
			string aspNetUserId = "";
			if ( userId == 0 )
			{
				statusMessage = "Error - please provide a valid user";
				return false;
			}
			if ( roleId < 1 || roleId > SiteReader )
			{
				statusMessage = "Error - please provide a valid role identifier";
				return false;
			}

			AppUser user = AppUser_Get( userId );
			if ( user != null && user.Id > 0 )
				aspNetUserId = user.AspNetUserId;

			if ( !IsValidGuid( aspNetUserId ) )
			{
				statusMessage = "Error - please provide a valid user identifier";
				return false;
			}
		
			EM.AspNetUserRoles efEntity = new EM.AspNetUserRoles();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					efEntity.UserId = aspNetUserId;
					efEntity.RoleId = roleId.ToString();
					efEntity.Created = System.DateTime.Now;

					context.AspNetUserRoles.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						statusMessage = "successful";
						//other, maybe notification
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the Account_AddRole was not successful. ";
						string message = string.Format( "AccountManager. Account_AddRole Failed", "Attempted to add an Account_AddRole. The process appeared to not work, but was not an exception, so we have no message, or no clue. Email: {0}, roleId {1}, requestedBy: {2}", user.Email, roleId, createdByUserId );
						EmailManager.NotifyAdmin( "	Manager. Account_AddRole Failed", message );
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_AddRole(), Email: {0}", user.Email ) );
					statusMessage = ex.Message;
					isValid = false;
				}
			}

			return isValid;
		}

		public bool Account_DeleteRole( AppUser entity, int roleId, int createdByUserId, ref string statusMessage )
		{
			bool isValid = true;

			if ( entity == null || !IsValidGuid( entity.AspNetUserId ) )
			{
				statusMessage = "Error - please provide a value user identifier";
				return false;
			}
			if ( roleId < 1 || roleId > SiteReader )
			{
				statusMessage = "Error - please provide a value role identifier";
				return false;
			}

			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					EM.AspNetUserRoles efEntity = context.AspNetUserRoles
							.SingleOrDefault( s => s.UserId == entity.AspNetUserId && s.RoleId == roleId.ToString() );

					if ( efEntity != null && !string.IsNullOrWhiteSpace( efEntity.RoleId ) )
					{
						context.AspNetUserRoles.Remove( efEntity );
						int count = context.SaveChanges();
						if ( count > 0 )
						{
							isValid = true;
						}
					}
					else
					{
						statusMessage = "Error - delete failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Account_DeleteRole(), Email: {0}", entity.Email ) );
					statusMessage = ex.Message;
					isValid = false;
				}
			}

			return isValid;
		}

		#endregion 
	}
}
