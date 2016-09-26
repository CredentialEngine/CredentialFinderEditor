using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
using Utilities;
using DBentity = Data.Entity_CostProfile;
using ThisEntity = Models.ProfileModels.CostProfile;
//using DBentityChild = Data.Entity_CostProfileItem;
//using EntityChild = Models.ProfileModels.CostProfileItem;
namespace Factories
{
	public class CostProfileManager : BaseFactory
	{
		#region persistance ==================
		/// <summary>
		/// Persist Cost Profiles
		/// </summary>
		/// <param name="profiles"></param>
		/// <param name="parentUid"></param>
		/// <param name="parentTypeId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		//public bool CostProfileUpdate( List<ThisEntity> profiles, Guid parentUid, int parentTypeId, int userId, ref List<string> messages )
		//{
		//	bool isValid = true;
		//	int intialCount = messages.Count;

		//	if ( !IsValidGuid( parentUid ) )
		//	{
		//		messages.Add( "Error: the parent identifier was not provided." );
		//	}
		//	if ( parentTypeId == 0 )
		//	{
		//		messages.Add( "Error: the parent type was not provided." );
		//	}
		//	if ( messages.Count > intialCount )
		//		return false;

		//	//get parent entity
		//	Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
		//	if ( parent == null || parent.Id == 0 )
		//	{
		//		messages.Add( "Error - the parent entity was not found." );
		//		return false;
		//	}
		//	int count = 0;
		//	bool hasData = false;
		//	if ( profiles == null )
		//		profiles = new List<ThisEntity>();

		//	DBentity efEntity = new DBentity();

		//	using ( var context = new EM.CTIEntities() )
		//	{
		//		//check add/updates first
		//		if ( profiles.Count() > 0 )
		//		{
		//			hasData = true;
		//			bool isEmpty = false;
		//			foreach ( ThisEntity entity in profiles )
		//			{
		//				if ( ValidateCostProfile( entity, ref isEmpty, ref  messages ) == false )
		//				{
		//					//can't really scrub from here - too late?
		//					//at least add some identifer
		//					messages.Add( "Cost profile was invalid. " + SetCostProfileSummary( entity ) );
		//					continue;
		//				}
		//				if ( isEmpty ) //skip
		//					continue;

		//				//just in case
		//				entity.EntityId = parent.Id;
		//				entity.ParentUid = parentUid;
		//				entity.ParentTypeId = parentTypeId;

		//				if ( entity.Id == 0 )
		//				{
		//					//add
		//					efEntity = new DBentity();
		//					FromMap( entity, efEntity );
		//					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
		//					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
		//					efEntity.RowId = Guid.NewGuid();

		//					context.Entity_CostProfile.Add( efEntity );
		//					count = context.SaveChanges();
		//					//update profile record so doesn't get deleted
		//					entity.Id = efEntity.Id;
		//					entity.RowId = efEntity.RowId;
		//					if ( count == 0 )
		//					{
		//						ConsoleMessageHelper.SetConsoleErrorMessage( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.ProfileName ) ? "no description" : entity.ProfileName ) );
		//					}
		//					else
		//					{
		//						if ( !UpdateParts( entity, userId, ref messages ) )
		//							isValid = false;
		//					}
		//				}
		//				else
		//				{
		//					efEntity = context.Entity_CostProfile.SingleOrDefault( s => s.Id == entity.Id );
		//					if ( efEntity != null && efEntity.Id > 0 )
		//					{
		//						entity.RowId = efEntity.RowId;
		//						//update
		//						FromMap( entity, efEntity );
		//						//has changed?
		//						if ( HasStateChanged( context ) )
		//						{
		//							efEntity.LastUpdated = System.DateTime.Now;
		//							efEntity.LastUpdatedById = userId;

		//							count = context.SaveChanges();
		//						}
		//						//always check parts
		//						if ( !UpdateParts( entity, userId, ref messages ) )
		//							isValid = false;
		//					}

		//				}

		//			} //foreach

		//		}
		//		//===> now direct deletes
		//		//check for deletes ====================================
		//		//need to ensure ones just added don't get deleted

		//		//get existing 
		//		//List<DBentity> results = context.Entity_CostProfile
		//		//		.Where( s => s.ParentUid == parentUid )
		//		//		.OrderBy( s => s.Id )
		//		//		.ToList();

		//		////if profiles is null, need to delete all!!
		//		//if ( results.Count() > 0 && profiles.Count() == 0 )
		//		//{
		//		//	foreach ( var item in results )
		//		//		context.Entity_CostProfile.Remove( item );

		//		//	context.SaveChanges();
		//		//}
		//		//else
		//		//{
		//		//	//should only have existing ids, where not in current list, so should be deletes
		//		//	var deleteList = from existing in results
		//		//					 join item in profiles
		//		//							 on existing.Id equals item.Id
		//		//							 into joinTable
		//		//					 from result in joinTable.DefaultIfEmpty( new ThisEntity { Id = 0, ParentTypeId = 0 } )
		//		//					 select new { DeleteId = existing.Id, ParentTypeId = ( result.ParentTypeId ) };

		//		//	foreach ( var v in deleteList )
		//		//	{
		//		//		if ( v.ParentTypeId == 0 )
		//		//		{
		//		//			//delete item
		//		//			DBentity p = context.Entity_CostProfile.FirstOrDefault( s => s.Id == v.DeleteId );
		//		//			if ( p != null && p.Id > 0 )
		//		//			{
		//		//				context.Entity_CostProfile.Remove( p );
		//		//				count = context.SaveChanges();
		//		//			}
		//		//		}
		//		//	}
		//		//}

		//	}

		//	return isValid;
		//}
		/// <summary>
		/// Persist Cost Profile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool CostProfile_Save( ThisEntity entity, Guid parentUid, int userId, ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;

			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( messages.Count > intialCount )
				return false;

			//get parent entity
			Views.Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			int count = 0;

			DBentity efEntity = new DBentity();

			using ( var context = new EM.CTIEntities() )
			{
				bool isEmpty = false;

				if ( ValidateCostProfile( entity, ref isEmpty, ref  messages ) == false )
				{
					//can't really scrub from here - too late?
					//at least add some identifer
					messages.Add( "Cost profile was invalid. " + SetCostProfileSummary( entity ) );
					return false;
				}
				if ( isEmpty )
				{
					messages.Add( "Error - Cost profile is empty. " );
					return false;
				}
				//just in case
				entity.EntityId = parent.Id;
				entity.ParentUid = parentUid;
				entity.ParentTypeId = parent.EntityTypeId;

				if ( entity.Id == 0 )
				{
					//add
					efEntity = new DBentity();
					FromMap( entity, efEntity );
					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
					efEntity.CreatedById = efEntity.LastUpdatedById = userId;
					efEntity.RowId = Guid.NewGuid();

					context.Entity_CostProfile.Add( efEntity );
					count = context.SaveChanges();
					//update profile record so doesn't get deleted
					entity.Id = efEntity.Id;
					entity.RowId = efEntity.RowId;
					if ( count == 0 )
					{
						ConsoleMessageHelper.SetConsoleErrorMessage( string.Format( " Unable to add Profile: {0} <br\\> ", string.IsNullOrWhiteSpace( entity.ProfileName ) ? "no description" : entity.ProfileName ) );
					}
					else
					{
						if ( !UpdateParts( entity, userId, ref messages ) )
							isValid = false;
					}
				}
				else
				{
					efEntity = context.Entity_CostProfile.SingleOrDefault( s => s.Id == entity.Id );
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
						if ( !UpdateParts( entity, userId, ref messages ) )
							isValid = false;
					}
				}
			}

			return isValid;
		}

		public bool CostProfile_Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new EM.CTIEntities() )
			{
				DBentity p = context.Entity_CostProfile.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_CostProfile.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Cost Profile record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}

		#endregion

		#region  retrieval ==================

		/// <summary>
		/// Retrieve and fill cost profiles for parent entity
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<ThisEntity> CostProfile_GetAll( Guid parentUid )
		{
			ThisEntity row = new ThisEntity();
			DurationItem duration = new DurationItem();
			List<ThisEntity> profiles = new List<ThisEntity>();

			using ( var context = new EM.CTIEntities() )
			{
				List<DBentity> results = context.Entity_CostProfile
						.Where( s => s.ParentUid == parentUid )
						.OrderBy( s => s.Id )
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( DBentity item in results )
					{
						row = new ThisEntity();
						ToMap( item, row, true );


						profiles.Add( row );
					}
				}
				return profiles;
			}

		}//

		public static ThisEntity CostProfile_Get( int profileId, bool includingItems = true )
		{
			ThisEntity entity = new ThisEntity();

			using ( var context = new EM.CTIEntities() )
			{
				DBentity item = context.Entity_CostProfile
							.SingleOrDefault( s => s.Id == profileId );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, includingItems );
				}
			}
			return entity;
		}//
		public static ThisEntity CostProfile_Get( Guid profileUid, bool includingItems = true )
		{
			ThisEntity entity = new ThisEntity();

			using ( var context = new EM.CTIEntities() )
			{
				DBentity item = context.Entity_CostProfile
							.SingleOrDefault( s => s.RowId == profileUid );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, includingItems );
				}
			}
			return entity;
		}//
		public bool ValidateCostProfile( ThisEntity profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;

			isEmpty = false;
			//check if empty
			if ( string.IsNullOrWhiteSpace( profile.ProfileName )
				&& string.IsNullOrWhiteSpace( profile.Currency )
				&& string.IsNullOrWhiteSpace( profile.DateEffective )
				&& string.IsNullOrWhiteSpace( profile.ExpirationDate )
				&& string.IsNullOrWhiteSpace( profile.Description )
				&& ( profile.ReferenceUrl == null || profile.ReferenceUrl.Count == 0 )
				&& ( profile.Items == null || profile.Items.Count == 0 )
				&& ( profile.Jurisdiction == null || profile.Jurisdiction.Count == 0 )
				)
			{
				isEmpty = true;
				return isValid;
			}

			//currency?
			if ( string.IsNullOrWhiteSpace( profile.Currency ) == false )
			{
				//length
				if ( profile.Currency.Length != 3 || IsInteger( profile.Currency ) )
				{
					messages.Add( "The currency code must be a three-letter alphabetic code  " );
					isValid = false;
				}
			}

			return isValid;
		}

		public static void FromMap( ThisEntity from, DBentity to )
		{
			to.Id = from.Id;
			//make sure EntityId is not wiped out. Also can't actually chg
			if ( ( to.EntityId ?? 0 ) == 0 )
				to.EntityId = from.EntityId;

			//to.RowId = (Guid)from.RowId;
			if ( to.Id == 0 )
			{
				to.ParentUid = from.ParentUid;
				to.ParentTypeId = from.ParentTypeId;
			}

			to.ProfileName = from.ProfileName;
			to.Description = from.Description;

			if ( IsValidDate( from.ExpirationDate ) )
				to.ExpirationDate = DateTime.Parse( from.ExpirationDate );
			else
				to.ExpirationDate = null;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;

			//to.DetailsUrl = from.DetailsUrl;
			//if ( !string.IsNullOrWhiteSpace( from.Currency ) )
			//	to.Currency = from.Currency.ToUpper();
			//else
			//	to.Currency = "";
			if ( from.CurrencyTypeId > 0 )
				to.CurrencyTypeId = from.CurrencyTypeId;
			else
				to.CurrencyTypeId = null;

		}
		public static void ToMap( DBentity from, ThisEntity to, bool includingItems = true )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.EntityId = from.EntityId ?? 0;

			to.ParentUid = from.ParentUid;
			to.ParentTypeId = from.ParentTypeId;
			if ( from.ProfileName == "*** new profile ***" )
				to.ProfileName = "";
			else 
				to.ProfileName = from.ProfileName;
			to.Description = from.Description;

			if ( IsValidDate( from.ExpirationDate ) )
				to.ExpirationDate = ( ( DateTime ) from.ExpirationDate ).ToShortDateString();
			else
				to.ExpirationDate = "";

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = ( ( DateTime ) from.DateEffective ).ToShortDateString();
			else
				to.DateEffective = "";

			//obsolete, replaced by ReferenceUrl
			to.DetailsUrl = from.DetailsUrl;
			to.Currency = from.Currency;
			to.CurrencyTypeId = (int)(from.CurrencyTypeId ?? 0);
			to.ProfileSummary = SetCostProfileSummary( to );
			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

			if ( includingItems )
			{
				//TODO - the items should be part of the EF record
				to.Items = CostProfileItemManager.CostProfileItem_GetAll( to.Id );

				to.Jurisdiction = RegionsManager.Jurisdiction_GetAll( to.RowId );

				to.CurrencyTypes = CodesManager.GetCurrencies();

				to.ReferenceUrl = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS );
			}
		}
		static string SetCostProfileSummary( ThisEntity to )
		{
			string summary = "Cost Profile ";
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

		#region  cost items ==================
		private bool UpdateParts( ThisEntity entity, int userId, ref List<string> messages )
		{
			bool isAllValid = true;
			//if ( !entity.IsNewVersion )
			//{
			//	if ( !new CostProfileItemManager().UpdateItems( entity.Items, entity.Id, userId, ref messages ) )
			//		isAllValid = false;

			//	if ( new RegionsManager().JurisdictionProfile_Update( entity.Jurisdiction, entity.RowId, CodesManager.ENTITY_TYPE_COST_PROFILE, userId, RegionsManager.JURISDICTION_PURPOSE_SCOPE, ref messages ) == false )
			//		isAllValid = false;
			//}

			if ( new Entity_ReferenceManager().EntityUpdate( entity.ReferenceUrl, entity.RowId, CodesManager.ENTITY_TYPE_COST_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS ) == false )
				isAllValid = false;

			return isAllValid;
		}

		#endregion

	}
}
