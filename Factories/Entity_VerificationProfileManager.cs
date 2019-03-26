using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBEntity = Data.Entity_VerificationProfile;
using ThisEntity = Models.ProfileModels.VerificationServiceProfile;
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
		public bool Save( ThisEntity entity, Guid parentUid, int userId, ref List<string> messages )
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

			DBEntity efEntity = new DBEntity();

			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					bool isEmpty = false;

					if ( ValidateProfile( entity, ref isEmpty, ref messages ) == false )
					{
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
						efEntity = new DBEntity();
						MapToDB( entity, efEntity );
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
							messages.Add( " Unable to add Verification Service Profile"  );
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
							MapToDB( entity, efEntity );
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
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{

					string message = HandleDBValidationError( dbex, thisClassName + ".Entity_Add() ", "Verification Service" );
					messages.Add("Error - the save was not successful. " + message);
					LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Save(), Parent: {0} ({1})", parent.EntityBaseName, parent.EntityBaseId ) );
				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex);
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save(), Parent: {0} ({1})", parent.EntityBaseName, parent.EntityBaseId) );
				}

			}

			return isValid;
		}

		private bool UpdateParts( ThisEntity entity, int userId, ref List<string> messages )
		{
			bool isAllValid = true;

			EntityPropertyManager mgr = new EntityPropertyManager();

			if ( mgr.UpdateProperties( entity.ClaimType, entity.RowId, CodesManager.ENTITY_TYPE_PROCESS_PROFILE, CodesManager.PROPERTY_CATEGORY_CLAIM_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;
			


			return isAllValid;
		}

		public bool Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				DBEntity p = context.Entity_VerificationProfile.FirstOrDefault( s => s.Id == recordId );
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
			int count = messages.Count;	

			isEmpty = false;
			if ( profile.IsStarterProfile )
				return true;

			if ( string.IsNullOrWhiteSpace( profile.Description ) || profile.Description == "please add a meaningful description" )
			{
				profile.Description = "";
				messages.Add( "A meaningfull profile description must be entered" );
			}
			
				if ( !IsUrlValid( profile.SubjectWebpage, ref commonStatusMessage ) )
			{
				messages.Add( "The Subject Webpage Url is invalid. " + commonStatusMessage );
			}
			if ( !IsUrlValid( profile.VerificationServiceUrl, ref commonStatusMessage ) )
			{
				messages.Add( "The Verification Service Url is invalid. " + commonStatusMessage );
			}
			if ( !IsUrlValid( profile.VerificationDirectory, ref commonStatusMessage ) )
			{
				messages.Add( "The Verification Directory Url is invalid. " + commonStatusMessage );
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

			if ( messages.Count > count )
				isValid = false;
			return isValid;
		}

		#endregion
		#region  retrieval ==================

		/// <summary>
		/// Get all VerificationProfile for the parent
		/// Uses the parent Guid to retrieve the related Entity, then uses the EntityId to retrieve the child objects.
		/// </summary>
		/// <param name="parentUid"></param>
		public static List<ThisEntity> GetAll( Guid parentUid, bool isForEditView )
		{
			ThisEntity entity = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}
			bool includingItems = true;
			if ( isForEditView )
				includingItems = false;
			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					List<DBEntity> results = context.Entity_VerificationProfile
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.Id )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( DBEntity item in results )
						{
							entity = new ThisEntity();
							MapFromDB( item, entity, includingItems, isForEditView );


							list.Add( entity );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetAll" );
			}
			return list;
		}//

		public static ThisEntity Get( int profileId, bool isEditView )
		{
			ThisEntity entity = new ThisEntity();

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					DBEntity item = context.Entity_VerificationProfile
							.SingleOrDefault( s => s.Id == profileId );

					if ( item != null && item.Id > 0 )
					{
						MapFromDB( item, entity, true, isEditView );
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".Get" );
			}
			return entity;
		}//

		public static void MapToDB( ThisEntity from, DBEntity to )
		{
			//want to ensure fields from create are not wiped
			if ( to.Id == 0 )
			{
				if ( IsValidDate( from.Created ) )
					to.Created = from.Created;
				to.CreatedById = from.CreatedById;
			}
			to.Id = from.Id;
			//to.ProfileName = from.ProfileName;
			to.Description = from.Description;
			to.HolderMustAuthorize = from.HolderMustAuthorize;

			//TODO - removed this
			//need to first convert any existing ones to entity.credential
			//if ( from.TargetCredentialId > 0 )
			//	to.CredentialId = from.TargetCredentialId;
			//else
			//	to.CredentialId = null;

			to.SubjectWebpage = NormalizeUrlData( from.SubjectWebpage );

			to.VerificationServiceUrl = from.VerificationServiceUrl;
			to.VerificationDirectory = from.VerificationDirectory;
			to.VerificationMethodDescription = from.VerificationMethodDescription;

			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = DateTime.Parse( from.DateEffective );
			else
				to.DateEffective = null;
			
			if ( IsGuidValid( from.OfferedByAgentUid ) )
			{
				to.OfferedByAgentUid = from.OfferedByAgentUid;
			}
			else
			{
				to.OfferedByAgentUid = null;
			}

		}
		public static void MapFromDB( DBEntity from, ThisEntity to, 
				bool includingItems, 
				bool isEditView, 
				bool asList = false //better option than includingItems
			)
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
            to.ParentId = from.EntityId;

			to.Description = ( from.Description ?? "" );
			if ( isEditView && to.Description == "please add a meaningful description" )
				to.Description = "";
			//ProfileName is for display purposes
			to.ProfileName = to.Description.Length < 80 ? to.Description : to.Description.Substring(0, 79) + " ...";
			//to.ProfileSummary = SetEntitySummary( to );

			if ( from.HolderMustAuthorize != null )
				to.HolderMustAuthorize = ( bool ) from.HolderMustAuthorize;
			
			if ( IsValidDate( from.DateEffective ) )
				to.DateEffective = ( ( DateTime ) from.DateEffective ).ToShortDateString();
			else
				to.DateEffective = "";

			to.SubjectWebpage = from.SubjectWebpage;
			to.VerificationServiceUrl = from.VerificationServiceUrl;
			to.VerificationDirectory = from.VerificationDirectory;
			to.VerificationMethodDescription = from.VerificationMethodDescription;
			

			if ( IsGuidValid( from.OfferedByAgentUid ) )
			{
				to.OfferedByAgentUid = ( Guid ) from.OfferedByAgentUid;
				to.OfferedBy = OrganizationManager.GetForSummary( to.OfferedByAgentUid );
			}

			if ( includingItems )
			{
				//TODO 170803- need to chg to a list
				//only get if:
				//edit - get profile list
				//detail - get basic
				bool isForDetailPageCredential = true;
				if ( asList )
					isForDetailPageCredential = false;
				to.TargetCredential = Entity_CredentialManager.GetAll( to.RowId, isForDetailPageCredential );

				to.EstimatedCost = CostProfileManager.GetAll( to.RowId, isEditView );

				to.ClaimType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CLAIM_TYPE );

				to.Jurisdiction = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_SCOPE );

				to.Region = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_RESIDENT );
				to.JurisdictionAssertions = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_OFFERREDIN );

				//to.VerificationStatus = Entity_VerificationStatusManager.GetAll( to.Id );
			}


			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById;
			to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );
		}
		static string SetEntitySummary( ThisEntity to )
		{
			string summary = "Verification Profile ";
			if ( !string.IsNullOrWhiteSpace( to.Description ) )
			{
				return to.Description.Length < 80 ? to.Description : to.Description.Substring( 0, 79 ) + " ...";
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
