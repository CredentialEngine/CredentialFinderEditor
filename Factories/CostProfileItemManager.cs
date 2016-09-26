using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.Entity_CostProfileItem;
using ThisEntity = Models.ProfileModels.CostProfileItem;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class CostProfileItemManager : BaseFactory
	{

		#region persistance ==================
		/// <summary>
		/// Persist cost profile items
		/// </summary>
		/// <param name="profiles"></param>
		/// <param name="parentUid"></param>
		/// <param name="parentTypeId"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool UpdateItems( List<ThisEntity> profiles, int parentId, int userId, ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;
			if ( parentId == 0 )
			{
				messages.Add( "Error: the parent cost profile id was not provided." );
			}
			if ( messages.Count > intialCount )
				return false;

			int count = 0;
			if ( profiles == null )
				profiles = new List<ThisEntity>();

			DBentity efEntity = new DBentity();

			using ( var context = new Data.CTIEntities() )
			{
				//check add/updates first
				if ( profiles.Count() > 0 )
				{
					bool isEmpty = false;
					foreach ( ThisEntity entity in profiles )
					{
						if ( ValidateItem( entity, ref isEmpty, ref  messages ) == false )
						{
							//can't really scrub from here - too late?
							//at least add some identifer
							messages.Add( "Cost profile item was invalid. " + SetProfileSummary( entity ) );
							continue;
						}
						if ( isEmpty ) //skip
							continue;

						//just in case
						entity.CostProfileId = parentId;

						if ( entity.Id == 0 )
						{
							//add
							efEntity = new DBentity();
							FromMap( entity, efEntity );

							efEntity.RowId = Guid.NewGuid();
							efEntity.Created = efEntity.LastUpdated = DateTime.Now;
							efEntity.CreatedById = efEntity.LastUpdatedById = userId;
							

							context.Entity_CostProfileItem.Add( efEntity );
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
								UpdateParts( entity, userId, ref messages );
							}
							
						}
						else
						{
							efEntity = context.Entity_CostProfileItem.SingleOrDefault( s => s.Id == entity.Id );
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
								//always check parts
								entity.RowId = efEntity.RowId;
								UpdateParts(entity, userId, ref messages);
							}

						}

					} //foreach

				}

				#region deletes - obsolete, doing direct deletes
				//check for deletes ====================================
				//need to ensure ones just added don't get deleted

				//get existing 
				//List<DBentity> results = context.Entity_CostProfileItem
				//		.Where( s => s.CostProfileId == parentId )
				//		.OrderBy( s => s.Id )
				//		.ToList();

				////if profiles is null, need to delete all!!
				//if ( results.Count() > 0 && profiles.Count() == 0 )
				//{
				//	foreach ( var item in results )
				//		context.Entity_CostProfileItem.Remove( item );

				//	context.SaveChanges();
				//}
				//else
				//{
				//	//should only have existing ids, where not in current list, so should be deletes
				//	var deleteList = from existing in results
				//					 join item in profiles
				//							 on existing.Id equals item.Id
				//							 into joinTable
				//					 from result in joinTable.DefaultIfEmpty( new ThisEntity { Id = 0, ParentId = 0 } )
				//					 select new { DeleteId = existing.Id, ParentId = ( result.ParentId ) };

				//	foreach ( var v in deleteList )
				//	{
				//		if ( v.ParentId == 0 )
				//		{
				//			//delete item
				//			DBentity p = context.Entity_CostProfileItem.FirstOrDefault( s => s.Id == v.DeleteId );
				//			if ( p != null && p.Id > 0 )
				//			{
				//				LoggingHelper.DoTrace( 2, string.Format( "@@@@ Deleting a costProfileItem, for CostProfileId:{0}, ProfileName:{1}, CostTypeId: {2}, ParentUid: {3}", p.CostProfileId, p.ProfileName, p.CostTypeId, p.Entity_CostProfile.ParentUid ) );

				//				context.Entity_CostProfileItem.Remove( p );
				//				count = context.SaveChanges();
				//			}
				//		}
				//	}
				//}
				#endregion 
			}

			return isValid;
		}
		public bool CostProfileItem_Save( ThisEntity entity, int parentId, int userId, ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;
			if ( parentId == 0 )
			{
				messages.Add( "Error: the parent cost profile id was not provided." );
			}
			if ( messages.Count > intialCount )
				return false;

			int count = 0;

			DBentity efEntity = new DBentity();

			using ( var context = new Data.CTIEntities() )
			{
				bool isEmpty = false;
				if ( ValidateItem( entity, ref isEmpty, ref  messages ) == false )
				{
					messages.Add( "Cost profile item was invalid. " + SetProfileSummary( entity ) );
					return false;
				}
				if ( isEmpty ) 
				{
					messages.Add( "Error - profile item was empty. " );
					return false;
				}

				//just in case
				entity.CostProfileId = parentId;

				if ( entity.Id == 0 )
				{
					//add
					efEntity = new DBentity();
					FromMap( entity, efEntity );

					efEntity.RowId = Guid.NewGuid();
					efEntity.Created = efEntity.LastUpdated = DateTime.Now;
					efEntity.CreatedById = efEntity.LastUpdatedById = userId;


					context.Entity_CostProfileItem.Add( efEntity );
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
						UpdateParts( entity, userId, ref messages );
					}

				}
				else
				{
					efEntity = context.Entity_CostProfileItem.SingleOrDefault( s => s.Id == entity.Id );
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
						//always check parts
						entity.RowId = efEntity.RowId;
						UpdateParts( entity, userId, ref messages );
					}
				}
			}

			return isValid;
		}
		public bool CostProfileItem_Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBentity p = context.Entity_CostProfileItem.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_CostProfileItem.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "CostProfileItem record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}

		private bool UpdateParts( ThisEntity entity, int userId, ref List<string> messages )
		{
			bool isAllValid = true;
			string statusMessage = "";
			int count = 0;

			EntityPropertyManager mgr = new EntityPropertyManager();

			if ( mgr.UpdateProperties( entity.ApplicableAudienceType, entity.RowId, CodesManager.ENTITY_TYPE_COST_PROFILE_ITEM, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE, userId, ref messages ) == false )
			{
				isAllValid = false;
			}

			if ( mgr.UpdateProperties( entity.ResidencyType, entity.RowId, CodesManager.ENTITY_TYPE_COST_PROFILE_ITEM, CodesManager.PROPERTY_CATEGORY_RESIDENCY_TYPE, userId, ref messages ) == false )
			{
				isAllValid = false;
			}

			if ( mgr.UpdateProperties( entity.EnrollmentType, entity.RowId, CodesManager.ENTITY_TYPE_COST_PROFILE_ITEM, CodesManager.PROPERTY_CATEGORY_ENROLLMENT_TYPE, userId, ref messages ) == false )
			{
				isAllValid = false;
			}
			return isAllValid;
		}
		#endregion



		#region  retrieval ==================

		/// <summary>
		/// Retrieve and fill cost profile items for parent entity
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<ThisEntity> CostProfileItem_GetAll( int parentId )
		{
			ThisEntity row = new ThisEntity();
			List<ThisEntity> profiles = new List<ThisEntity>();

			using ( var context = new Data.CTIEntities() )
			{
				List<DBentity> results = context.Entity_CostProfileItem
						.Where( s => s.CostProfileId == parentId )
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
		public static ThisEntity CostProfileItem_Get( int profileId, bool includingProperties = true )
		{
			ThisEntity entity = new ThisEntity();

			using ( var context = new Data.CTIEntities() )
			{
				DBentity item = context.Entity_CostProfileItem
							.SingleOrDefault( s => s.Id == profileId );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, includingProperties );
				}
				return entity;
			}

		}//
		public bool ValidateItem( ThisEntity profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;
			int count = messages.Count;

			isEmpty = false;
			//check if empty
			if ( string.IsNullOrWhiteSpace( profile.ProfileName )
				&& profile.CostTypeId == 0
				&& string.IsNullOrWhiteSpace( profile.CostTypeOther )
				&& string.IsNullOrWhiteSpace( profile.DateEffective )
				&& string.IsNullOrWhiteSpace( profile.OtherResidencyType )
				&& string.IsNullOrWhiteSpace( profile.OtherEnrollmentType )
				&& string.IsNullOrWhiteSpace( profile.OtherApplicableAudienceType )
				&& string.IsNullOrWhiteSpace( profile.Description )
				&& ( profile.EnrollmentType == null || profile.EnrollmentType.Items.Count == 0 )
				&& ( profile.ResidencyType == null || profile.ResidencyType.Items.Count == 0 )
				&& ( profile.ApplicableAudienceType == null || profile.ApplicableAudienceType.Items.Count == 0 )
				)
			{
				isEmpty = true;
				return isValid;
			}
			if ( profile.CostTypeId == 0 && profile.CostType.hasItems() )
				profile.CostTypeId = CodesManager.GetEnumerationSelection( profile.CostType );

			//&& string.IsNullOrWhiteSpace( profile.CostTypeOther ) 
			if ( profile.CostTypeId == 0 )
			{
				messages.Add( "A cost type must be selected " );
			}
			//
			//if ( ( profile.ApplicableAudienceType == null || profile.ApplicableAudienceType.Items.Count == 0 ) && string.IsNullOrWhiteSpace( profile.OtherApplicableAudienceType ) )
			//	messages.Add( "An applicable audience must be selected " );
			////
			//if ( ( profile.EnrollmentType == null || profile.EnrollmentType.Items.Count == 0 ) && string.IsNullOrWhiteSpace( profile.OtherEnrollmentType ) )
			//	messages.Add( "An enrollment type must be selected " );
			////
			//if ( ( profile.ResidencyType == null || profile.ResidencyType.Items.Count == 0 ) && string.IsNullOrWhiteSpace( profile.OtherResidencyType ) )
			//	messages.Add( "A residency type must be selected " );
			//
			if ( profile.Price < 1)
				messages.Add( "A cost must be entered" );


			if ( messages.Count > count)
				isValid = false;

			return isValid;
		}

		public static void ToMap( DBentity from, ThisEntity to, bool includingProperties = false )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.CostProfileId = from.CostProfileId;
			to.CostTypeId = from.CostTypeId;
			to.ProfileName = from.ProfileName;
			if ( to.ProfileName.Length == 0 )
			{
				if ( from.Codes_PropertyValue != null )
				{
					to.ProfileName = from.Codes_PropertyValue.Title;
					if ( !string.IsNullOrEmpty( from.CostTypeOther ) )
						to.ProfileName += " - " + from.CostTypeOther;
				}
			}
			to.CostTypeOther = from.CostTypeOther;
			to.Price = from.Price == null ? 0 : ( decimal ) from.Price;
			if ( to.Price > 0 )
				to.ProfileName += " - " + to.Price.ToString();
			to.Description = from.Description;

			to.PaymentPattern = from.PaymentPattern;
			if ( from.PayeeUid != null )
				to.PayeeUid = ( Guid ) from.PayeeUid;

			to.OtherResidencyType = from.OtherResidencyType;
			to.OtherEnrollmentType = from.OtherEnrollmentType;
			to.OtherApplicableAudienceType = from.OtherApplicableAudienceType;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;


			//properties
			if ( includingProperties )
			{
				to.ApplicableAudienceType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE );

				to.ResidencyType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_RESIDENCY_TYPE );

				to.EnrollmentType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_ENROLLMENT_TYPE );

			}

		}
		public static void FromMap( ThisEntity from, DBentity to )
		{
			to.Id = from.Id;
			to.CostProfileId = from.CostProfileId;
			to.CostTypeId = from.CostTypeId;
			to.ProfileName = from.ProfileName;
			to.CostTypeOther = from.CostTypeOther;
			to.Price = from.Price;

			to.Description = from.Description;

			to.PaymentPattern = from.PaymentPattern;
			if ( from.PayeeUid != null && from.PayeeUid .ToString().IndexOf("0000-") == -1)
				to.PayeeUid = ( Guid ) from.PayeeUid;

			to.OtherResidencyType = from.OtherResidencyType;
			to.OtherEnrollmentType = from.OtherEnrollmentType;
			to.OtherApplicableAudienceType = from.OtherApplicableAudienceType;

			//public entity will prob not have this data
			//if ( IsValidDate( from.Created ) )
			//	to.Created = ( DateTime ) from.Created;
			//to.CreatedById = from.CreatedById;
			//if ( IsValidDate( from.LastUpdated ) )
			//	to.LastUpdated = ( DateTime ) from.LastUpdated;
			//to.LastUpdatedById = from.LastUpdatedById;

		}
		//private static void FillAudienceType( DBentity from, ThisEntity to )
		//{
		//	to.ApplicableAudienceType = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE );

		//	to.ApplicableAudienceType.ParentId = to.Id;
		//	to.ApplicableAudienceType.Items = new List<EnumeratedItem>();
		//	EnumeratedItem item = new EnumeratedItem();

		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		List<EM.Entity_Property> results = context.Entity_Property
		//			.Where( s => s.ParentUid == from.RowId && s.Codes_PropertyValue.CategoryId == CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE )
		//							.OrderBy( s => s.Codes_PropertyValue.SortOrder ).ThenBy( s => s.Codes_PropertyValue.Title )
		//							.ToList();

		//		if ( results != null && results.Count > 0 )
		//		{
		//			foreach ( EM.Entity_Property prop in results )
		//			{

		//				item = new EnumeratedItem();
		//				item.Id = prop.PropertyValueId;
		//				item.Value = prop.PropertyValueId.ToString();
		//				item.Selected = true;

		//				item.Name = prop.Codes_PropertyValue.Title;

		//				to.ApplicableAudienceType.Items.Add( item );

		//			}
		//		}

		//	}

		//}
		static string SetProfileSummary( ThisEntity to )
		{
			string summary = "Cost Profile Item ";
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
