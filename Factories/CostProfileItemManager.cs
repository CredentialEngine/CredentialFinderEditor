using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
	
		public bool Save( ThisEntity entity, int parentId, int userId, ref List<string> messages )
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
							messages.Add( string.Format( " Unable to add Cost Item for CostProfileId: {0}, CostTypeId: {1}  ", parentId, entity.CostTypeId ));
							isValid = false;
						}
						else
						{
							UpdateParts( entity, userId, ref messages );
						}

					}
					else
					{
						context.Configuration.LazyLoadingEnabled = false;

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
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, "CostProfileItemManager.Save()", string.Format( "CostProfileId: 0 , CostTypeId: {1}  ", parentId, entity.CostTypeId ));

					messages.Add( message );
					isValid = false;
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, string.Format( "CostProfileItemManager.Save(), CostProfileId: 0 , CostTypeId: {1}  ", parentId, entity.CostTypeId ) );
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

			EntityPropertyManager mgr = new EntityPropertyManager();

			if ( mgr.UpdateProperties( entity.ApplicableAudienceType, entity.RowId, CodesManager.ENTITY_TYPE_COST_PROFILE_ITEM, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE, userId, ref messages ) == false )
			{
				isAllValid = false;
			}

			if ( mgr.UpdateProperties( entity.ResidencyType, entity.RowId, CodesManager.ENTITY_TYPE_COST_PROFILE_ITEM, CodesManager.PROPERTY_CATEGORY_RESIDENCY_TYPE, userId, ref messages ) == false )
			{
				isAllValid = false;
			}

			//if ( mgr.UpdateProperties( entity.EnrollmentType, entity.RowId, CodesManager.ENTITY_TYPE_COST_PROFILE_ITEM, CodesManager.PROPERTY_CATEGORY_ENROLLMENT_TYPE, userId, ref messages ) == false )
			//{
			//	isAllValid = false;
			//}
			return isAllValid;
		}
		#endregion

		#region  retrieval ==================

		
		public static ThisEntity Get( int profileId, bool includingProperties, bool forEditView )
		{
			ThisEntity entity = new ThisEntity();

			using ( var context = new Data.CTIEntities() )
			{
				DBentity item = context.Entity_CostProfileItem
							.SingleOrDefault( s => s.Id == profileId );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, includingProperties, forEditView );
				}
				return entity;
			}

		}//
		public bool ValidateProfile( ThisEntity profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;
			int count = messages.Count;

			isEmpty = false;
			//&& ( profile.EnrollmentType == null || profile.EnrollmentType.Items.Count == 0 )
			/*
			 * 				&& string.IsNullOrWhiteSpace( profile.OtherResidencyType )
				&& string.IsNullOrWhiteSpace( profile.OtherEnrollmentType )
				&& string.IsNullOrWhiteSpace( profile.OtherApplicableAudienceType )
								&& string.IsNullOrWhiteSpace( profile.Description )
			 */

			//check if empty
			if ( profile.CostTypeId == 0
				&& string.IsNullOrWhiteSpace( profile.CostTypeOther )
				&& string.IsNullOrWhiteSpace( profile.DateEffective )
				&& ( profile.ResidencyType == null || profile.ResidencyType.Items.Count == 0 )
				&& ( profile.ApplicableAudienceType == null || profile.ApplicableAudienceType.Items.Count == 0 )
				)
			{
				//isEmpty = true;
				//return isValid;
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

		public static void ToMap( DBentity from, ThisEntity to, bool includingProperties, bool forEditView )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.CostProfileId = from.CostProfileId;
			to.CostTypeId = from.CostTypeId;
			to.CostTypeOther = from.CostTypeOther;

			//profile name will no longer be visible, but we could still persist with the cost type
			//to.ProfileName = from.ProfileName;
			//if ( to.ProfileName.Length == 0 )
			//{
				if ( from.Codes_PropertyValue != null )
				{
					to.CostTypeName = from.Codes_PropertyValue.Title;

					to.ProfileName = from.Codes_PropertyValue.Title;
					if ( !string.IsNullOrEmpty( from.CostTypeOther ) )
						to.ProfileName += " - " + from.CostTypeOther;
				}
				else
				{
					to.ProfileName = (to.CostTypeOther ?? "Cost");
				}
			//}
			
			//NA 3/17/2017 - Need this to fix null errors in publishing and detail page, but it isn't working: no item is selected, and it's not clear why. 
			to.CostType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_ATTAINMENT_COST ); 

			to.Price = from.Price == null ? 0 : ( decimal ) from.Price;
			if ( forEditView && to.Price > 0 )
				to.ProfileName += " ( " + to.Price.ToString() + " )";
			//to.Description = from.Description;

			to.PaymentPattern = from.PaymentPattern;
			//if ( ( to.PaymentPattern ?? "").Length > 0 )
			//	to.ProfileName += " - " + to.PaymentPattern;

			//if ( from.PayeeUid != null )
			//	to.PayeeUid = ( Guid ) from.PayeeUid;

			//to.OtherResidencyType = from.OtherResidencyType;
			//to.OtherEnrollmentType = from.OtherEnrollmentType;
			//to.OtherApplicableAudienceType = from.OtherApplicableAudienceType;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
			to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );

			//properties
			if ( includingProperties )
			{
				to.ApplicableAudienceType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE );

				to.ResidencyType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_RESIDENCY_TYPE );

				//to.EnrollmentType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_ENROLLMENT_TYPE );

			}

		}
		public static void FromMap( ThisEntity from, DBentity to )
		{
			to.Id = from.Id;
			to.CostProfileId = from.CostProfileId;
			if ( to.CostTypeId != from.CostTypeId )
			{
				//get the profile name from the code table
				//Models.CodeItem item = CodesManager.Codes_PropertyValue_Get( from.CostTypeId );
				//to.ProfileName = item.Title;
			}
			to.CostTypeId = from.CostTypeId;
			to.ProfileName = null;
			//if ( to.ProfileName.IndexOf( "jQuery" ) > 5 )
			//{
			//	int len = to.ProfileName.Length - to.ProfileName.IndexOf( "jQuery" );
			//	to.ProfileName = to.ProfileName.Substring( 0, len );
			//}
		
			to.Price = from.Price;
			to.PaymentPattern = from.PaymentPattern;

			to.CostTypeOther = null; //from.CostTypeOther;
			to.Description = null;// 			from.Description;

			
			//if ( from.PayeeUid != null && IsGuidValid( from.PayeeUid ) )
			//	to.PayeeUid = ( Guid ) from.PayeeUid;
			//else
			//	to.PayeeUid = null;

			//to.OtherResidencyType = from.OtherResidencyType;
			//to.OtherEnrollmentType = from.OtherEnrollmentType;
			//to.OtherApplicableAudienceType = from.OtherApplicableAudienceType;

			//public entity will prob not have this data
			//if ( IsValidDate( from.Created ) )
			//	to.Created = ( DateTime ) from.Created;
			//to.CreatedById = from.CreatedById;
			//if ( IsValidDate( from.LastUpdated ) )
			//	to.LastUpdated = ( DateTime ) from.LastUpdated;
			//to.LastUpdatedById = from.LastUpdatedById;

		}

		public static List<ThisEntity> Search( int topParentTypeId, int topParentEntityBaseId, string pFilter, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows )
		{
			string connectionString = DBConnectionRO();
			ThisEntity item = new ThisEntity();
			CostProfile cp = new CostProfile();
			List<ThisEntity> list = new List<ThisEntity>();
			var result = new DataTable();

			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}

				using ( SqlCommand command = new SqlCommand( "[CostProfileItems_search]", c ) )
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add( new SqlParameter( "@condProfParentEntityTypeId", topParentTypeId ) );
					command.Parameters.Add( new SqlParameter( "@condProfParentEntityBaseId", topParentEntityBaseId ) );
					command.Parameters.Add( new SqlParameter( "@Filter", pFilter ) );
					command.Parameters.Add( new SqlParameter( "@SortOrder", pOrderBy ) );
					command.Parameters.Add( new SqlParameter( "@StartPageIndex", pageNumber ) );
					command.Parameters.Add( new SqlParameter( "@PageSize", pageSize ) );

					SqlParameter totalRows = new SqlParameter( "@TotalRows", pTotalRows );
					totalRows.Direction = ParameterDirection.Output;
					command.Parameters.Add( totalRows );

					using ( SqlDataAdapter adapter = new SqlDataAdapter() )
					{
						adapter.SelectCommand = command;
						adapter.Fill( result );
					}
					string rows = command.Parameters[ 4 ].Value.ToString();
					try
					{
						pTotalRows = Int32.Parse( rows );
					}
					catch
					{
						pTotalRows = 0;
					}
				}
				//determine if we want to return data as a list of costprofiles or costProfileItems
				//
				int prevCostProfileId = 0;
				foreach ( DataRow dr in result.Rows )
				{
					//cp = new CostProfile();
					item = new ThisEntity();
					item.Id = GetRowColumn( dr, "Entity_CostProfileId", 0 );
					//include parent entity type somewhere
					item.ParentEntityType = GetRowColumn( dr, "EntityType", "" );

					item.ProfileName = GetRowColumn( dr, "CostProfileName", "Cost Profile" );

					item.CostTypeName = GetRowColumn( dr, "CostType", "" );
					
					item.Currency = GetRowColumn( dr, "Currency", "" );
					item.CurrencySymbol = GetRowColumn( dr, "CurrencySymbol", "" );
					item.Price = GetRowPossibleColumn( dr, "Price", 0M );

					item.Description = string.Format( "{0} {1} ({2})", item.CurrencySymbol, item.Price, item.ParentEntityType );
					list.Add( item );
				}

				return list;

			}
		} //


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
		//static string SetProfileSummary( ThisEntity to )
		//{
		//	string summary = "Cost Profile Item ";
		//	if ( !string.IsNullOrWhiteSpace( to.ProfileName ) )
		//	{
		//		summary = to.ProfileName;
		//		return summary;
		//	}

		//	if ( to.Id > 1 )
		//	{
		//		summary += to.Id.ToString();
		//		return summary;
		//	}
		//	return summary;

		//}
		#endregion
	}
}
