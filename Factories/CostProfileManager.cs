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
using DBEntity = Data.Entity_CostProfile;
using ThisEntity = Models.ProfileModels.CostProfile;
//using DBentityChild = Data.Entity_CostProfileItem;
//using EntityChild = Models.ProfileModels.CostProfileItem;
namespace Factories
{
	public class CostProfileManager : BaseFactory
	{
		static string thisClassName = "CostProfileManager";
		#region persistance ==================

		/// <summary>
		/// Persist Cost Profile
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public bool Save( ThisEntity entity, Guid parentUid, int userId, ref List<string> messages )
		{
			bool isValid = true;
			int intialCount = messages.Count;

			//TODO - if asset cost (asmt, etc) edited from credential, then credential is passed as parent, instead of the asmt
			if ( !IsValidGuid( parentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( messages.Count > intialCount )
				return false;

			//get parent entity
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			int count = 0;

			DBEntity efEntity = new DBEntity();

			using ( var context = new EM.CTIEntities() )
			{
				bool isEmpty = false;

				if ( ValidateProfile( entity, ref isEmpty, ref  messages ) == false )
				{
					//can't really scrub from here - too late?
					//at least add some identifer
					//messages.Add( "Cost profile was invalid. " + SetCostProfileSummary( entity ) );
					return false;
				}
				if ( isEmpty )
				{
					messages.Add( "Error - Cost profile is empty. " );
					return false;
				}
				
				//entity.ParentUid = parentUid;
				//entity.ParentTypeId = parent.EntityTypeId;

				try
				{
					if ( entity.Id == 0 )
					{
						//just in case
						entity.EntityId = parent.Id;

						//add
						efEntity = new DBEntity();
						MapToDB( entity, efEntity );
						efEntity.Created = efEntity.LastUpdated = DateTime.Now;
						efEntity.CreatedById = efEntity.LastUpdatedById = userId;
                        //allow client (initially via bulk upload), to set identifer
                        if ( IsValidGuid( entity.RowId ) )
                            efEntity.RowId = entity.RowId;
                        else
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
						context.Configuration.LazyLoadingEnabled = false;

						efEntity = context.Entity_CostProfile.SingleOrDefault( s => s.Id == entity.Id );
						if ( efEntity != null && efEntity.Id > 0 )
						{
							entity.RowId = efEntity.RowId;
							//update
							MapToDB( entity, efEntity );
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
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, "CostProfileManager.Save()", entity.ProfileName );

					messages.Add( message );
					isValid = false;
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + ".CostProfile_Save" );
					messages.Add( ex.Message );
					isValid = false;
				}
			}

			return isValid;
		}

		/// <summary>
		/// Copy the source cost profile under the target parent
		/// </summary>
		/// <param name="sourceCostProfileUid"></param>
		/// <param name="targetParentUid"></param>
		/// <param name="userId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public int Copy( Guid sourceCostProfileUid, Guid targetParentUid, int userId, ref ThisEntity newCostProfile, ref List<string> messages )
		{
			int newId = 0;
			int intialCount = messages.Count;
			newCostProfile = new ThisEntity();

			if ( !IsValidGuid( sourceCostProfileUid ) )
			{
				messages.Add( "Error: the cost profile identifier was not provided." );
			}

			if ( !IsValidGuid( targetParentUid ) )
			{
				messages.Add( "Error: the parent identifier was not provided." );
			}

			if ( messages.Count > intialCount )
				return 0;

			//get parent entity
			Entity parent = EntityManager.GetEntity( targetParentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return 0;
			}

			DBEntity efEntity = new DBEntity();
			

			using ( var context = new EM.CTIEntities() )
			{
				//get the source
				DBEntity item = context.Entity_CostProfile
							.SingleOrDefault( s => s.RowId == sourceCostProfileUid );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, newCostProfile, true, true );
					//change parentage
					newCostProfile.Id = 0;
					newCostProfile.ProfileName = ( newCostProfile.ProfileName ?? "" ) + " (COPY)";
					newCostProfile.EntityId = parent.Id;
					newCostProfile.LastUpdatedById = newCostProfile.CreatedById = userId;
                    newCostProfile.RowId = Guid.NewGuid(); 
                    //save main
                    if ( Save( newCostProfile, targetParentUid, userId, ref messages ) )
					{
						newId = newCostProfile.Id;
						//get the newly created Entity
						Entity costEntity = EntityManager.GetEntity( newCostProfile.RowId );
						//save items
						foreach ( CostProfileItem cpi in newCostProfile.Items )
						{
							cpi.CostProfileId = newId;
							cpi.Id = 0;

							new CostProfileItemManager().Save( cpi, newId, userId, ref messages );
						}
						//jurisdiction

						foreach ( JurisdictionProfile jp in newCostProfile.Jurisdiction )
						{
							jp.ParentId = costEntity.Id;
							jp.Id = 0;
							//fix all geocoordinates
							if ( jp.MainJurisdiction != null && jp.MainJurisdiction.GeoNamesId > 0 )
							{
								jp.MainJurisdiction.Id = 0;
							}
							if ( jp.JurisdictionException != null && jp.JurisdictionException.Count > 0 )
							{
								foreach ( GeoCoordinates gc in jp.JurisdictionException )
								{
									//is this all that is necessary?
									gc.Id = 0;
								}
							}
							//do update
							new Entity_JurisdictionProfileManager().Add( jp, "", ref messages );
						}
					}
				}
				else
				{
					messages.Add( "Error - the cost profile was not found." );
					return 0;
				}

			}

			return newId;
		}
		public bool Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			statusMessage = "";
			using ( var context = new EM.CTIEntities() )
			{
				DBEntity p = context.Entity_CostProfile.FirstOrDefault( s => s.Id == recordId );
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
		public bool Delete( Guid recordId, ref string statusMessage )
		{
			bool isOK = true;
			statusMessage = "";
			using ( var context = new EM.CTIEntities() )
			{
				DBEntity p = context.Entity_CostProfile.FirstOrDefault( s => s.RowId == recordId );
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
		public static List<ThisEntity> GetAll( Guid parentUid, bool forEditView )
		{
			LoggingHelper.DoTrace( 8, string.Format( "CostProfileManager.GetAll(parentUid={0}, forEditView={1})", parentUid, forEditView ) );
			ThisEntity row = new ThisEntity();

			List<ThisEntity> profiles = new List<ThisEntity>();
			Entity parent = EntityManager.GetEntity( parentUid );

			using ( var context = new EM.CTIEntities() )
			{
				List<DBEntity> results = context.Entity_CostProfile
						.Where( s => s.EntityId == parent.Id )
						.OrderBy( s => s.Id )
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( DBEntity item in results )
					{
						row = new ThisEntity();
						MapFromDB( item, row, true, forEditView );


						profiles.Add( row );
					}
				}
				return profiles;
			}

		}//
		public static List<ThisEntity> GetAllForList( Guid parentUid, bool forEditView )
		{
			ThisEntity row = new ThisEntity();

			List<ThisEntity> profiles = new List<ThisEntity>();
			Entity parent = EntityManager.GetEntity( parentUid );

			using ( var context = new EM.CTIEntities() )
			{
				List<DBEntity> results = context.Entity_CostProfile
						.Where( s => s.EntityId == parent.Id )
						.OrderBy( s => s.Id )
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( DBEntity item in results )
					{
						row = new ThisEntity();
						MapFromDB( item, row, false, forEditView );


						profiles.Add( row );
					}
				}
				return profiles;
			}

		}//

		public static ThisEntity GetForEdit( int profileId )
		{
			ThisEntity entity = new ThisEntity();

			using ( var context = new EM.CTIEntities() )
			{
				DBEntity item = context.Entity_CostProfile
							.SingleOrDefault( s => s.Id == profileId );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity, true, true );
				}
			}
			return entity;
		}//
		public static ThisEntity GetBasicProfile( int profileId )
		{
			ThisEntity entity = new ThisEntity();

			using ( var context = new EM.CTIEntities() )
			{
				//context.Configuration.LazyLoadingEnabled = false;
				DBEntity item = context.Entity_CostProfile
							.SingleOrDefault( s => s.Id == profileId );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity, true, false );

					//entity.Id = item.Id;
					//entity.RowId = item.RowId;
					//entity.EntityId = item.EntityId ?? 0;
					//entity.ProfileName = item.ProfileName;
					//entity.Description = item.Description;
     //               if (item.Entity != null && item.Entity.Id > 0)
     //               {
     //                   entity.ParentUid = item.Entity.EntityUid;
     //                   entity.ParentId = (int)item.Entity.EntityBaseId;
     //               }
				}
			}
			return entity;
        }//
        public static ThisEntity GetBasicProfile( string profileUid )
        {
            Guid identifier = new Guid( profileUid );
            return GetBasicProfile( identifier ); ;
        }//
		public static ThisEntity GetBasicProfile( Guid profileUid )
		{
			ThisEntity entity = new ThisEntity();

			using ( var context = new EM.CTIEntities() )
			{
				//context.Configuration.LazyLoadingEnabled = false;
				DBEntity item = context.Entity_CostProfile
							.SingleOrDefault( s => s.RowId == profileUid );

				if ( item != null && item.Id > 0 )
				{
					MapFromDB( item, entity, true, false );
					//entity.Id = item.Id;
					//entity.RowId = item.RowId;
					//entity.EntityId = item.EntityId ?? 0;
					//entity.ProfileName = item.ProfileName;
					//entity.Description = item.Description;
				}
			}
			return entity;
		}//
		public static List<ThisEntity> SearchByOwningOrg( string parentRowId, int pageNumber, int pageSize, ref int pTotalRows )
		{
			ThisEntity row = new ThisEntity();
			List<ThisEntity> profiles = new List<ThisEntity>();
			if ( IsValidGuid( parentRowId ) )
			{
				profiles = SearchByOwningOrg( new Guid( parentRowId ) );
                pTotalRows = profiles.Count();
			}

			return profiles;
		}
		/// <summary>
		/// Search for cost profiles for owning org.
		/// Will include those for current parent entity  (ex credential), to allow copying for, perhaps different audiences.
		/// Will probably need paging at some point
		/// </summary>
		/// <param name="parentUid"></param>
		/// <returns></returns>
		public static List<ThisEntity> SearchByOwningOrg( Guid parentUid )
		{
			ThisEntity row = new ThisEntity();
			List<ThisEntity> profiles = new List<ThisEntity>();

			//get the parent, and then the owning org
			Entity parent = EntityManager.GetEntity( parentUid );
			Guid owningAgentUid = new Guid();
			//will initially only be used from a credential
			if ( parent.EntityTypeId == 1 )
			{
				Credential c = CredentialManager.GetBasic( parent.EntityBaseId );
				owningAgentUid = c.OwningAgentUid;
			}
			else if ( parent.EntityTypeId == 3 )
			{
				AssessmentProfile a = AssessmentManager.GetBasic( parent.EntityBaseId );
				owningAgentUid = a.OwningAgentUid;
			}
			else if ( parent.EntityTypeId == 7 )
			{
				LearningOpportunityProfile l = LearningOpportunityManager.GetBasic( parent.EntityBaseId );
				owningAgentUid = l.OwningAgentUid;
			}
			using ( var context = new Views.CTIEntities1() )
			{
				List<Views.CostProfile_SummaryForSearch> results = context.CostProfile_SummaryForSearch
						.Where( s => s.OwningAgentUid == owningAgentUid )
						.OrderBy( s => s.EntityTypeId ).ThenBy( s => s.ParentName ).ThenBy( s => s.CostProfileName )
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Views.CostProfile_SummaryForSearch item in results )
					{
						row = new ThisEntity();
						row.Id = item.Entity_CostProfileId;
						row.RowId = item.CostProfileRowId;
						row.ProfileName = "Cost: " +  (( item.CostProfileName ?? "" ).Length > 0 ? item.CostProfileName : "Cost Profile");
						
						row.ProfileSummary = "From: " + item.EntityType + ": " + item.ParentName;
						row.Description = item.CostDescription;
						profiles.Add( row );
					}
				}
				return profiles;
			}

		}//

		
		public bool ValidateProfile( ThisEntity profile, ref bool isEmpty, ref List<string> messages )
		{
			bool isValid = true;
			int count = messages.Count;
			isEmpty = false;

			if ( profile.IsStarterProfile )
				return true;
			//check if empty
			//				&& ( profile.ReferenceUrl == null || profile.ReferenceUrl.Count == 0 )
			//&& string.IsNullOrWhiteSpace( profile.Currency )
			if ( string.IsNullOrWhiteSpace( profile.ProfileName )
				&& string.IsNullOrWhiteSpace( profile.DateEffective )
				&& string.IsNullOrWhiteSpace( profile.ExpirationDate )
				&& string.IsNullOrWhiteSpace( profile.Description )
				&& ( profile.Items == null || profile.Items.Count == 0 )
				&& ( profile.Jurisdiction == null || profile.Jurisdiction.Count == 0 )
				)
			{
				isEmpty = true;
				return isValid;
			}

			if ( string.IsNullOrWhiteSpace( profile.Description ) )
			{
				messages.Add( "The Cost Profile Description is required" );
			}

			if ( string.IsNullOrWhiteSpace( profile.DetailsUrl ))
			{
				messages.Add( "The Cost Details Url is required"  );
			}
			else if ( !IsUrlValid( profile.DetailsUrl, ref commonStatusMessage ) )
			{
				messages.Add( "The Cost Details Url is invalid" + commonStatusMessage );
			}
			DateTime startDate = DateTime.Now;
			DateTime endDate = DateTime.Now;
			if ( !string.IsNullOrWhiteSpace( profile.DateEffective )  )
			{
				if ( !IsValidDate( profile.DateEffective ) )
					messages.Add( "Please enter a valid start date" );
				else
				{
					DateTime.TryParse( profile.DateEffective, out startDate );
				}
			}
			if ( !string.IsNullOrWhiteSpace( profile.ExpirationDate )  )
			{
				if ( !IsValidDate( profile.ExpirationDate ) )
					messages.Add( "Please enter a valid end date" );
				else
				{
					DateTime.TryParse( profile.ExpirationDate, out endDate );
					if ( IsValidDate( profile.DateEffective )
						&& startDate > endDate)
						messages.Add( "The end date must be greater than the start date." );
				}
			}
			//currency?
			//if ( string.IsNullOrWhiteSpace( profile.Currency ) == false )
			//{
			//	//length
			//	if ( profile.Currency.Length != 3 || IsInteger( profile.Currency ) )
			//	{
			//		messages.Add( "The currency code must be a three-letter alphabetic code  " );
			//		isValid = false;
			//	}
			//}

			if ( messages.Count > count )
				isValid = false;
			return isValid;
		}

		public static void MapToDB( ThisEntity from, DBEntity to )
		{
			to.Id = from.Id;
			

			//to.RowId = (Guid)from.RowId;
			if ( to.Id == 0 )
			{
				//make sure EntityId is not wiped out. Also can't actually chg
				if ( ( to.EntityId ?? 0 ) == 0 )
					to.EntityId = from.EntityId;
			}

			to.ProfileName = (from.ProfileName ?? "").Trim();
			to.Description = from.Description;

			if ( IsValidDate( from.ExpirationDate ) )
				to.ExpirationDate = DateTime.Parse( from.ExpirationDate );
			else
				to.ExpirationDate = null;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;

			to.DetailsUrl = from.DetailsUrl;

			if ( from.CurrencyTypeId > 0 )
				to.CurrencyTypeId = from.CurrencyTypeId;
			else
				to.CurrencyTypeId = null;

		}
		public static void MapFromDB( DBEntity from, ThisEntity to, bool includingItems, bool forEditView )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.EntityId = from.EntityId ?? 0;

			//to.ParentUid = from.ParentUid;
			//to.ParentTypeId = from.ParentTypeId;
			if ( forEditView && from.ProfileName == "*** new profile ***" )
				to.ProfileName = "";
			else
				to.ProfileName = from.ProfileName;
			to.Description = from.Description;
			//set viewHeading
			if ( from.Entity != null )
			{
				if ( from.Entity.Codes_EntityType != null )
					to.ViewHeading = from.Entity.Codes_EntityType.Title + " - ";
				//this name could be out of date
				to.ViewHeading += from.Entity.EntityBaseName;
				EntityManager.MapFromDB( from.Entity, to.RelatedEntity );
			}
			if ( IsValidDate( from.ExpirationDate ) )
				to.ExpirationDate = ( ( DateTime ) from.ExpirationDate ).ToShortDateString();
			else
				to.ExpirationDate = "";

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = ( ( DateTime ) from.DateEffective ).ToShortDateString();
			else
				to.DateEffective = "";

			to.DetailsUrl = from.DetailsUrl;
			
			to.CurrencyTypeId = (int)(from.CurrencyTypeId ?? 0);
			if ( from.Codes_Currency != null )
			{
				to.Currency = from.Codes_Currency.Currency;
				to.CurrencySymbol = from.Codes_Currency.HtmlCodes;
			}

			to.ProfileSummary = SetCostProfileSummary( to );
			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
			to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );

			to.Condition = Entity_ReferenceManager.GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM );

			if ( includingItems )
			{
				//TODO - the items should be part of the EF record
				if ( from.Entity_CostProfileItem != null && from.Entity_CostProfileItem.Count > 0 )
				{
					CostProfileItem row = new CostProfileItem();
					foreach ( EM.Entity_CostProfileItem item in from.Entity_CostProfileItem )
					{
						row = new CostProfileItem();
						CostProfileItemManager.MapFromDB( item, row, true, forEditView );
						to.Items.Add( row );
					}
				}

				//to.Items = CostProfileItemManager.CostProfileItem_GetAll( to.Id );

				to.Jurisdiction = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId );
				to.Region = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_RESIDENT );
				//TODO - IS this used?
				//to.CurrencyTypes = CodesManager.GetCurrencies();

				//to.ReferenceUrl = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS );
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

			//if ( new Entity_ReferenceManager().Entity_Reference_Update( entity.ReferenceUrl, entity.RowId, CodesManager.ENTITY_TYPE_COST_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_REFERENCE_URLS, false ) == false )
			//	isAllValid = false;

			if ( new Entity_ReferenceManager().Update( entity.Condition, entity.RowId, CodesManager.ENTITY_TYPE_CONDITION_PROFILE, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_CONDITION_ITEM, false ) == false )
				isAllValid = false;
			return isAllValid;
		}

		#endregion

	}
}
