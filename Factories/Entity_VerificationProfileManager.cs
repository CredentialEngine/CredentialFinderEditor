using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.Entity_VerificationProfile;
using ThisEntity = Models.ProfileModels.AuthenticationProfile;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class Entity_VerificationProfileManager : BaseFactory
	{
		static string thisClassName = "Entity_VerificationProfileManager";
		#region Entity Persistance ===================
		/// <summary>
		/// Persist VerificationProfile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool VerificationProfile_Update( ThisEntity entity, Guid parentUid, int userId, ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;

			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( messages.Count > intialCount )
				return false;

			int count = 0;

			DBentity efEntity = new DBentity();

			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{

				bool isEmpty = false;

				if ( ValidateProfile( entity, ref isEmpty, ref  messages ) == false )
				{
					messages.Add( "Verification Profile was invalid. " + SetEntitySummary( entity ) );
					return false;
				}
				if ( isEmpty )
				{
					messages.Add( "The Verification Profile is empty. " + SetEntitySummary( entity ) );
					return false;
				}

				if ( entity.Id == 0 )
				{
					//add
					efEntity = new DBentity();
					Entity_FromMap( entity, efEntity );
					efEntity.EntityId = parent.Id;

					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
					efEntity.RowId = Guid.NewGuid();

					context.Entity_VerificationProfile.Add( efEntity );
					count = context.SaveChanges();
					//update profile record so doesn't get deleted
					entity.Id = efEntity.Id;
					entity.ParentId = parent.Id;
					entity.RowId = efEntity.RowId;
					if ( count == 0 )
					{
						messages.Add( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.ProfileName ) ? "no description" : entity.ProfileName ) );
					}
					else
					{
						//other entity components use a trigger to create the entity Object. If a trigger is not created, then child adds will fail (as typically use entity_summary to get the parent. As the latter is easy, make the direct call?
						//string statusMessage = "";
						//int entityId = new EntityManager().Add( efEntity.RowId, entity.Id, 	CodesManager.ENTITY_TYPE_VERIFICATION_PROFILE, ref  statusMessage );

						UpdateParts( entity, userId, ref messages );
					}
				}
				else
				{
					entity.ParentId = parent.Id;

					efEntity = context.Entity_VerificationProfile.SingleOrDefault( s => s.Id == entity.Id );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						entity.RowId = efEntity.RowId;
						//update
						Entity_FromMap( entity, efEntity );
						//has changed?
						if ( HasStateChanged( context ) )
						{
							efEntity.LastUpdated = System.DateTime.Now;
							efEntity.LastUpdatedById = userId;

							count = context.SaveChanges();
						}
						//always check parts
						UpdateParts( entity, userId, ref messages );
					}

				}


			}

			return isValid;
		}

		private bool UpdateParts( ThisEntity entity, int userId, ref List<string> messages )
		{
			bool isAllValid = true;

			//TODO - skip items not yet handled in new version
			//if ( !entity.IsNewVersion )
			//{
			//	if ( !new CostProfileManager().CostProfileUpdate( entity.EstimatedCost, entity.RowId, CodesManager.ENTITY_TYPE_TASK_PROFILE, userId, ref messages ) )
			//		isAllValid = false;
			

			//	if ( new RegionsManager().JurisdictionProfile_Update( entity.Jurisdiction, entity.RowId, CodesManager.ENTITY_TYPE_TASK_PROFILE, userId, RegionsManager.JURISDICTION_PURPOSE_SCOPE, ref messages ) == false )
			//		isAllValid = false;
			//}



			return isAllValid;
		}

		public bool VerificationProfile_Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity p = context.Entity_VerificationProfile.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_VerificationProfile.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Verification Profile record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}


		public bool ValidateProfile( ThisEntity profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;

			isEmpty = false;
			//check if empty
				//&& ( profile.EstimatedCost == null || profile.EstimatedCost.Count == 0 )
				//&& ( profile.Jurisdiction == null || profile.Jurisdiction.Count == 0 )

			if ( string.IsNullOrWhiteSpace( profile.ProfileName )
				&& string.IsNullOrWhiteSpace( profile.Description )
				&& string.IsNullOrWhiteSpace( profile.DateEffective )
				&& profile.TargetCredentialId == 0

				)
			{
				isEmpty = true;
				return isValid;
			}

			if ( string.IsNullOrWhiteSpace( profile.ProfileName ) )
			{
				messages.Add( "A profile name must be entered" );
				isValid = false;
			}
			//should be something else
			//if ( !IsValidGuid( profile.AffiliatedAgentUid )
			//	&& ( profile.EstimatedCost == null || profile.EstimatedCost.Count == 0 )
			//	&& ( profile.Jurisdiction == null || profile.Jurisdiction.Count == 0 )
			//	&& ( profile.EstimatedDuration == null || profile.EstimatedDuration.Count == 0 )
			//	)
			//{
			//	//messages.Add( "This profile does not seem to contain much of any data. Please enter something worth saving! " );
			//	//isValid = false;
			//}
			return isValid;
		}

		#endregion
		#region  retrieval ==================

		/// <summary>
		/// Get all VerificationProfile for the parent
		/// Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<ThisEntity> VerificationProfile_GetAll( Guid parentUid )
		{
			ThisEntity entity = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentity> results = context.Entity_VerificationProfile
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new ThisEntity();
							Entity_ToMap( item, entity, true );


							list.Add( entity );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".VerificationProfile_GetAll" );
			}
			return list;
		}//

		public static ThisEntity VerificationProfile_Get( int profileId )
		{
			ThisEntity entity = new ThisEntity();

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity item = context.Entity_VerificationProfile
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						Entity_ToMap( item, entity, true );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".VerificationProfile_Get" );
			}
			return entity;
		}//

		public static void Entity_FromMap( ThisEntity from, DBentity to )
		{
			//want to ensure fields from create are not wiped
			if ( to.Id == 0 )
			{
				if ( IsValidDate( from.Created ) )
					to.Created = from.Created;
				to.CreatedById = from.CreatedById;
			}
			to.Id = from.Id;
			to.ProfileName = from.ProfileName;
			to.Description = from.Description;
			to.HolderMustAuthorize = from.HolderMustAuthorize;
			if ( from.TargetCredentialId > 0 )
				to.CredentialId = from.TargetCredentialId;
			else
				to.CredentialId = null;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;


		}
		public static void Entity_ToMap( DBentity from, ThisEntity to, bool includingItems = true )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;

			to.ProfileName = from.ProfileName;
			to.Description = from.Description;
			to.HolderMustAuthorize = (bool) (from.HolderMustAuthorize ?? false);
			to.TargetCredentialId = (int) (from.CredentialId ?? 0);

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = ( ( DateTime ) from.DateEffective ).ToShortDateString();
			else
				to.DateEffective = "";

			to.ProfileSummary = SetEntitySummary( to );


			if ( includingItems )
			{
				to.EstimatedCost = CostProfileManager.CostProfile_GetAll( to.RowId );
				to.Jurisdiction = RegionsManager.Jurisdiction_GetAll( to.RowId );
			}


			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
		}
		static string SetEntitySummary( ThisEntity to )
		{
			string summary = "Verification Profile ";
			if ( !string.IsNullOrWhiteSpace( to.ProfileName ) )
			{
				return to.ProfileName;
			}

			if ( to.Id > 1 )
			{
				summary += to.Id.ToString();
			}
			return summary;

		}
		#endregion

	}
}
