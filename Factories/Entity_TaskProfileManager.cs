using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.Entity_TaskProfile;
using ThisEntity = Models.ProfileModels.TaskProfile;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class Entity_TaskProfileManager : BaseFactory
	{
		static string thisClassName = "Entity_TaskProfileManager";
		#region Entity Persistance ===================
		/// <summary>
		/// Persist Task Profiles
		/// </summary>
		/// <param name="profiles"></param>
		/// <param name="parentUid"></param>
		/// <param name="parentTypeId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Update( ThisEntity entity, Guid parentUid, int userId, ref List<string> messages )
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

			//Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			Entity parent = EntityManager.GetEntity( parentUid );
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
					//messages.Add( "Task Profile was invalid. " + SetEntitySummary( entity ) );
					return false;
				}
				if ( isEmpty )
				{
					messages.Add( "The Task Profile is empty. " + SetEntitySummary( entity ) );
					return false;
				}

				if ( entity.Id == 0 )
				{
					//add
					efEntity = new DBentity();
					FromMap( entity, efEntity );
					efEntity.EntityId = parent.Id;

					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
					efEntity.RowId = Guid.NewGuid();

					context.Entity_TaskProfile.Add( efEntity );
					count = context.SaveChanges();
					//update profile record so doesn't get deleted
					entity.Id = efEntity.Id;
					entity.ParentId = parent.Id;
					entity.RowId = efEntity.RowId;
					if ( count == 0 )
					{
						messages.Add( string.Format(" Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.ProfileName ) ? "no description" : entity.ProfileName ));
					}
					else
					{
						UpdateParts( entity, userId, ref messages );
					}
				}
				else
				{
					entity.ParentId = parent.Id;

					efEntity = context.Entity_TaskProfile.SingleOrDefault( s => s.Id == entity.Id );
					if ( efEntity != null && efEntity.Id > 0 )
					{
						entity.RowId = efEntity.RowId;
						//update
						FromMap( entity, efEntity );
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

			Entity_ReferenceManager erm = new Entity_ReferenceManager();

			if ( erm.Entity_Reference_Update( entity.ResourceUrl, entity.RowId, CodesManager.ENTITY_TYPE_TASK_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS, false ) == false )
				isAllValid = false;


			return isAllValid;
		}

		public bool TaskProfile_Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity p = context.Entity_TaskProfile.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_TaskProfile.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Task Profile record was not found: {0}", recordId );
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
			if ( string.IsNullOrWhiteSpace( profile.ProfileName )
				&& string.IsNullOrWhiteSpace( profile.Description )
				&& string.IsNullOrWhiteSpace( profile.AvailableOnlineAt )
				&& string.IsNullOrWhiteSpace( profile.DateEffective )
				&& !IsValidGuid( profile.AffiliatedAgentUid )
				&& ( profile.EstimatedCost == null || profile.EstimatedCost.Count == 0 )
				&& ( profile.Jurisdiction == null || profile.Jurisdiction.Count == 0 )
				&& ( profile.EstimatedDuration == null || profile.EstimatedDuration.Count == 0 )
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
			if ( !string.IsNullOrWhiteSpace( profile.DateEffective ) && !IsValidDate( profile.DateEffective ) )
			{
				messages.Add( "Please enter a valid effective date" );
				isValid = false;
			}
			if ( !IsUrlWellFormed( profile.AvailabilityListing ) )
			{
				messages.Add( "The Availability Listing Url is invalid" );
			}
			if ( !IsUrlWellFormed( profile.AvailableOnlineAt ) )
			{
				messages.Add( "The Available Online At Url is invalid" );
			}
			//should be something else
			if ( !IsValidGuid( profile.AffiliatedAgentUid )
				&& ( profile.EstimatedCost == null || profile.EstimatedCost.Count == 0 )
				&& ( profile.Jurisdiction == null || profile.Jurisdiction.Count == 0 )
				&& ( profile.EstimatedDuration == null || profile.EstimatedDuration.Count == 0 )
				)
			{
				//messages.Add( "This profile does not seem to contain much of any data. Please enter something worth saving! " );
				//isValid = false;
			}
			return isValid;
		}

		#endregion
		#region  retrieval ==================

		/// <summary>
		/// Get all Task profiles for the parent
		/// Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<ThisEntity> TaskProfile_GetAll( Guid parentUid )
		{
			ThisEntity entity = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBentity> results = context.Entity_TaskProfile
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBentity item in results )
						{
							entity = new ThisEntity();
							ToMap( item, entity, true );


							list.Add( entity );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".TaskProfile_GetAll" );
			}
			return list;
		}//

		public static ThisEntity TaskProfile_Get( int profileId )
		{
			ThisEntity entity = new ThisEntity();
			
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBentity item = context.Entity_TaskProfile
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						ToMap( item, entity, true );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".TaskProfile_Get" );
			}
			return entity;
		}//

		public static void FromMap( ThisEntity from, DBentity to )
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
			to.AvailableOnlineAt = GetUrlData( from.AvailableOnlineAt, null );
			to.AvailabilityListing = GetUrlData( from.AvailabilityListing, null );

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;
			if ( IsGuidValid( from.AffiliatedAgentUid ) )
				to.AffiliatedAgentUid = from.AffiliatedAgentUid;
			else
				to.AffiliatedAgentUid = null;

		}
		public static void ToMap( DBentity from, ThisEntity to, bool includingItems = true )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;

			if ( from.ProfileName == "*** new profile ***" )
				from.ProfileName = "** PROVIDE PROFILE NAME **";
			else	
				to.ProfileName = from.ProfileName;
			to.Description = from.Description;
			to.AvailabilityListing = from.AvailabilityListing;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = ( ( DateTime ) from.DateEffective ).ToShortDateString();
			else
				to.DateEffective = "";

			//to.ResourceUrl = from.Url;
			to.AvailableOnlineAt = from.AvailableOnlineAt;
			
			if ( from.AffiliatedAgentUid != null)
				to.AffiliatedAgentUid = (Guid)from.AffiliatedAgentUid;

			to.ProfileSummary = SetEntitySummary( to );

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
			to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );

			if ( includingItems )
			{
				to.Addresses = AddressProfileManager.GetAll( to.RowId );

				to.EstimatedCost = CostProfileManager.CostProfile_GetAll( to.RowId );

				to.EstimatedDuration = DurationProfileManager.GetAll( to.RowId );
				to.Jurisdiction = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId );

				to.ResourceUrl = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS );
			}
		}
		static string SetEntitySummary( ThisEntity to )
		{
			string summary = "Task Profile ";
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
