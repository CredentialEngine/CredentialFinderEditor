using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBEntity = Data.Organization_VerificationStatus;
using ThisEntity = Models.ProfileModels.VerificationStatus;
//

namespace Factories
{
	public class Organization_VerificationStatusManager : BaseFactory
	{

		#region persistance ==================

		public bool Save( ThisEntity entity, int parentId, int userId, ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;
			if ( parentId == 0 )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}
			if ( messages.Count > intialCount )
				return false;

			int count = 0;

			DBEntity efEntity = new DBEntity();

			using ( var context = new Data.CTIEntities() )
			{
				bool isEmpty = false;
				if ( ValidateProfile( entity, ref isEmpty, ref  messages ) == false )
				{
					return false;
				}
				if ( isEmpty )
				{
					messages.Add( "Error - profile item was empty. " );
					return false;
				}
				try
				{
					//just in case
					entity.ParentId = parentId;

					if ( entity.Id == 0 )
					{
						//add
						efEntity = new DBEntity();
						FromMap( entity, efEntity );

						efEntity.RowId = Guid.NewGuid();
						efEntity.Created = efEntity.LastUpdated = DateTime.Now;
						efEntity.CreatedById = efEntity.LastUpdatedById = userId;


						context.Organization_VerificationStatus.Add( efEntity );
						count = context.SaveChanges();
						entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;
						if ( count == 0 )
						{
							ConsoleMessageHelper.SetConsoleErrorMessage( string.Format( " Unable to add Verification Status: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.ProfileName ) ? "no description" : entity.ProfileName ) );
						}

					}
					else
					{
						context.Configuration.LazyLoadingEnabled = false;

						efEntity = context.Organization_VerificationStatus.SingleOrDefault( s => s.Id == entity.Id );
						if ( efEntity != null && efEntity.Id > 0 )
						{
							//update
							FromMap( entity, efEntity );
							//has changed?
							if ( HasStateChanged( context ) )
							{
								efEntity.LastUpdated = System.DateTime.Now;
								efEntity.LastUpdatedById = userId;

								count = context.SaveChanges();
							}
							entity.RowId = efEntity.RowId;
						}
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, "Organization_VerificationStatusManager.Save()", entity.ProfileName );
					messages.Add( message );
					isValid = false;
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, string.Format( "Organization_VerificationStatusManager.Save(), Name: {0}", entity.ProfileName ) );
					isValid = false;
				}
			}

			return isValid;
		}
		public bool Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity p = context.Organization_VerificationStatus.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Organization_VerificationStatus.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Verification Status Profile record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}

		#endregion

		#region  retrieval ==================

		/// <summary>
		/// Retrieve and fill Verification Profile items for parent entity
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<ThisEntity> GetAll( int parentId )
		{
			ThisEntity row = new ThisEntity();
			List<ThisEntity> profiles = new List<ThisEntity>();

			using ( var context = new Data.CTIEntities() )
			{
				List<DBEntity> results = context.Organization_VerificationStatus
						.Where( s => s.OrgId == parentId )
						.OrderBy( s => s.Name )
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( DBEntity item in results )
					{
						row = new ThisEntity();
						MapFromDB( item, row, true );
						profiles.Add( row );
					}
				}
				return profiles;
			}

		}//
		public static ThisEntity Get( int profileId, bool includingProperties = true )
		{
			ThisEntity entity = new ThisEntity();

			using ( var context = new Data.CTIEntities() )
			{
				DBEntity item = context.Organization_VerificationStatus
							.SingleOrDefault( s => s.Id == profileId );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity, includingProperties );
				}
				return entity;
			}

		}//
		public bool ValidateProfile( ThisEntity profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;
			int count = messages.Count;

			isEmpty = false;


			//check if empty
			if ( string.IsNullOrWhiteSpace( profile.Name )
				&& string.IsNullOrWhiteSpace( profile.Description )
				)
			{
				isEmpty = true;
				return isValid;
			}


			if ( messages.Count > count )
				isValid = false;

			return isValid;
		}

		public static void MapFromDB( DBEntity from, ThisEntity to, bool includingProperties = false )
		{
			to.Id = from.Id;
			to.ParentId = from.OrgId;
			to.RowId = from.RowId;
			to.Name = from.Name;
			to.Description = from.Description;
			to.URL = from.URL;
		
			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
			to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );


		}
		public static void FromMap( ThisEntity from, DBEntity to )
		{
			//to.Id = from.Id;
			//should not be necessary?
			to.OrgId = from.ParentId;

			to.Name = from.Name;
			to.Description = from.Description;
			to.URL = from.URL;

		}

		static string SetProfileSummary( ThisEntity to )
		{
			string summary = "Verification Status Item ";
			if ( !string.IsNullOrWhiteSpace( to.ProfileName ) )
			{
				summary = to.ProfileName;
				return summary;
			}

			if ( to.Id > 1 )
			{
				summary += to.Id.ToString();
				return summary;
			}
			return summary;

		}
		#endregion
	}
}
