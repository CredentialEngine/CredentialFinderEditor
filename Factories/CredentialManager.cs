using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

using Models;
using CM = Models.Common;
//using ME = Models.Elastic;
using Models.ProfileModels;
using EM = Data;
using Utilities;
//using PropertyMgr = Factories.CredentialPropertyManager;
using ThisEntity = Models.Common.Credential;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
using CondProfileMgr = Factories.Entity_ConditionProfileManager;
//using CondProfileMgrOld = Factories.ConnectionProfileManager;

namespace Factories
{
	public class CredentialManager : BaseFactory
	{
		static string thisClassName = "Factories.CredentialManager";

		List<string> messages = new List<string>();
		#region Credential - presistance =======================

		/// <summary>
		/// add a credential
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public int Add( CM.Credential entity, ref string statusMessage )
		{
			EM.Credential efEntity = new EM.Credential();
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					if ( ValidateProfile( entity, ref  messages ) == false )
					{
						statusMessage = string.Join( "<br/>", messages.ToArray() );
						return 0;
					}

					FromMap( entity, efEntity );

					efEntity.RowId = Guid.NewGuid();
					efEntity.CTID = "ce-" + efEntity.RowId.ToString();
					efEntity.CreatedById = entity.CreatedById;
					efEntity.Created = System.DateTime.Now;
					efEntity.LastUpdatedById = entity.CreatedById;
					efEntity.LastUpdated = System.DateTime.Now;
					efEntity.StatusId = 1;

					context.Credential.Add( efEntity );

					// submit the change to database
					int count = context.SaveChanges();
					if ( count > 0 )
					{
						entity.RowId = efEntity.RowId;
						statusMessage = "successful";
						entity.Id = efEntity.Id;

						UpdateParts( entity, true, ref statusMessage );

						return efEntity.Id;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the add was not successful. ";
						string message = string.Format( "CredentialManager. Credential_Add Failed", "Attempted to add a credential. The process appeared to not work, but was not an exception, so we have no message, or no clue.Credential: {0}, createdById: {1}", entity.Name, entity.CreatedById );
						EmailManager.NotifyAdmin( "CredentialManager. Credential_Add Failed", message );
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					statusMessage = HandleDBValidationError( dbex, thisClassName + ".Add() ", "Credential" );
					LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Add(), Name: {0}, UserId: {1}", entity.Name, entity.CreatedById ) );

					//string message = thisClassName + string.Format( ".Credential_Add() DbEntityValidationException, Name: {0}", efEntity.Name );
					//foreach ( var eve in dbex.EntityValidationErrors )
					//{
					//	message += string.Format( "\rEntity of type \"{0}\" in state \"{1}\" has the following validation errors:",
					//		eve.Entry.Entity.GetType().Name, eve.Entry.State );
					//	foreach ( var ve in eve.ValidationErrors )
					//	{
					//		message += string.Format( "- Property: \"{0}\", Error: \"{1}\"",
					//			ve.PropertyName, ve.ErrorMessage );
					//	}

					//	LoggingHelper.LogError( message, true );
					//}

					//statusMessage = string.Join( ", ", dbex.EntityValidationErrors.SelectMany( m => m.ValidationErrors.Select( n => n.ErrorMessage ) ).ToList() );
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), Name: {0}", efEntity.Name ) );
					statusMessage = FormatExceptions( ex );
					
					//statusMessage = ex.Message + ( ex.InnerException != null && ex.InnerException.InnerException != null ? " - " + ex.InnerException.InnerException.Message : "" );
				}
			}

			return efEntity.Id;
		}
		/// <summary>
		/// Update a credential
		/// - base only, caller will handle parts?
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Update( CM.Credential entity, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					//context.Configuration.LazyLoadingEnabled = false;

					EM.Credential efEntity = context.Credential
								.SingleOrDefault( s => s.Id == entity.Id );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					if ( ValidateProfile( entity, ref  messages ) == false )
					{
						statusMessage = string.Join( "<br/>", messages.ToArray() );
						return false;
					}
					//**ensure rowId is passed down for use by profiles, etc
					entity.RowId = efEntity.RowId;
					FromMap( entity, efEntity );

					if (context.ChangeTracker.Entries().Any(e => e.State == EntityState.Added
                                              || e.State == EntityState.Modified
											  || e.State == EntityState.Deleted ) == true)
					{
						//note: testing - the latter may be true if the child has changed - but shouldn't as the mapping only updates the parent
						efEntity.LastUpdated = System.DateTime.Now;
						efEntity.LastUpdatedById = entity.LastUpdatedById;
						if ( efEntity.StatusId < CodesManager.ENTITY_STATUS_PUBLISHED )
							efEntity.StatusId = CodesManager.ENTITY_STATUS_IN_PROGRESS;
						count = context.SaveChanges();
					}
					
					//can be zero if no data changed
					if ( count >= 0 )
					{
						//TODO - handle first time owner roles here????
						isValid = true;

						if ( !UpdateParts( entity, false, ref statusMessage ) )
							isValid = false;
					}
					else
					{
						//?no info on error
						statusMessage = "Error - the update was not successful. ";
							string message = string.Format( "CredentialManager. Credential_Update Failed", "Attempted to update a credential. The process appeared to not work, but was not an exception, so we have no message, or no clue. Credential: {0}, Id: {1}, updatedById: {2}", entity.Name, entity.Id, entity.LastUpdatedById );
						EmailManager.NotifyAdmin( "CredentialManager. Credential_Update Failed", message );
					}
				}
				else
				{
					statusMessage = "Error - update failed, as record was not found.";
				}
			}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					//LoggingHelper.LogError( dbex, thisClassName + string.Format( ".ContentAdd() DbEntityValidationException, Type:{0}", entity.TypeId ) );
					string message = thisClassName + string.Format( ".Credential_Update() DbEntityValidationException, Name: {0}", entity.Name );
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

					statusMessage = string.Join( ", ", dbex.EntityValidationErrors.SelectMany( m => m.ValidationErrors.Select( n => n.ErrorMessage ) ).ToList() );
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Credential_Update(), Name: {0}", entity.Name ) );
					statusMessage = ex.Message + ( ex.InnerException != null && ex.InnerException.InnerException != null ? " - " + ex.InnerException.InnerException.Message : "" );
				}
			}

			return isValid;
	}

		public bool ValidateProfile( ThisEntity profile, ref List<string> messages )
		{
			bool isValid = true;
			int count = messages.Count;

			if ( string.IsNullOrWhiteSpace( profile.Name ) )
			{
				messages.Add( "A credential name must be entered" );
			}
			if ( profile.IsDescriptionRequired && string.IsNullOrWhiteSpace( profile.Description ) )
			{
				messages.Add( "A description must be entered" );
			}
			if ( !string.IsNullOrWhiteSpace( profile.DateEffective ) && !IsValidDate( profile.DateEffective ) )
			{
				messages.Add( "Please enter a valid effective date" );
			}


			//if ( !IsUrlWellFormed( profile.Url ) )
			//{
			//	messages.Add( "The value for Url is invalid" );
			//}
			if ( string.IsNullOrWhiteSpace( profile.SubjectWebpage ) )
				messages.Add( "A Subject Webpage name must be entered" );

			else if ( !IsUrlValid( profile.SubjectWebpage, ref commonStatusMessage ) )
			{
				messages.Add( "The Subject Webpage Url is invalid. " + commonStatusMessage );
			}

			if ( !IsUrlValid( profile.AvailabilityListing, ref commonStatusMessage ) )
			{
				messages.Add( "The 'Availability Listing' Url is invalid. " + commonStatusMessage );
			}
			if ( !IsUrlValid( profile.AvailableOnlineAt, ref commonStatusMessage ) )
			{
				messages.Add( "The 'Available Online At' URL format is invalid. " + commonStatusMessage );
				;
			}
			if ( !IsUrlValid( profile.LatestVersionUrl, ref commonStatusMessage ) )
			{
				messages.Add( "The 'Latest Version' URL format is invalid. " + commonStatusMessage );
			}
			if ( !IsUrlValid( profile.PreviousVersion, ref commonStatusMessage ) )
			{
				messages.Add( "The 'Replaces Version' URL format is invalid. " + commonStatusMessage );
			}
			if ( !IsUrlValid( profile.ImageUrl, ref commonStatusMessage ) )
			{
				messages.Add( "The Image Url is invalid. " + commonStatusMessage );
			}
		
			if ( messages.Count > count )
				isValid = false;

			return isValid;
		}

		/// <summary>
		/// Update credential registry id, and set status published
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="envelopeId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UpdateEnvelopeId( int credentialId, string envelopeId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			bool updatingStatus = UtilityManager.GetAppKeyValue( "onRegisterSetEntityToPublic", false );
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.Credential efEntity = context.Credential
									.SingleOrDefault( s => s.Id == credentialId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.CredentialRegistryId = envelopeId;
						if ( updatingStatus )
							efEntity.StatusId = CodesManager.ENTITY_STATUS_PUBLISHED;

						if ( HasStateChanged( context ) )
						{
							//don't set updated for this action
							//efEntity.LastUpdated = System.DateTime.Now;
							//efEntity.LastUpdatedById = userId;
							
							count = context.SaveChanges();
							//can be zero if no data changed
							if ( count >= 0 )
							{
								isValid = true;
							}
							else
							{
								//?no info on error
								statusMessage = "Error - the update was not successful. ";
								string message = string.Format( thisClassName + ". UpdateEnvelopeId Failed", "Attempted to update an EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. Credential: {0}, envelopeId: {1}, updatedById: {2}", credentialId, envelopeId, userId );
								EmailManager.NotifyAdmin( thisClassName + ".UpdateEnvelopeId Failed", message );
							}
						}

						
					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UpdateEnvelopeId(), Credential: {0}, envelopeId: {1}, updatedById: {2}", credentialId, envelopeId, userId ) );
					statusMessage = ex.Message + ( ex.InnerException != null && ex.InnerException.InnerException != null ? " - " + ex.InnerException.InnerException.Message : "" );
				}
			}

			return isValid;
		}

		/// <summary>
		/// Reset credential registry id, and set status to in process
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UnPublish( int credentialId, int userId, ref string statusMessage )
		{
			bool isValid = false;
			int count = 0;
			using ( var context = new Data.CTIEntities() )
			{
				EM.Credential efEntity = new EM.Credential();
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					efEntity = context.Credential
									.SingleOrDefault( s => s.Id == credentialId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.CredentialRegistryId = null;
						//may not know reason for unpublish
						efEntity.StatusId = CodesManager.ENTITY_STATUS_IN_PROGRESS;

						if ( HasStateChanged( context ) )
						{
							efEntity.LastUpdated = System.DateTime.Now;
							efEntity.LastUpdatedById = userId;

							count = context.SaveChanges();
						}

						//can be zero if no data changed
						if ( count >= 0 )
						{
							isValid = true;
						}
						else
						{
							//?no info on error
							statusMessage = "Error - the update was not successful. ";
							string message = string.Format( thisClassName + ".UnPublish Failed", "Attempted to unpublish the credential. The process appeared to not work, but was not an exception, so we have no message, or no clue. Credential: {0}, updatedById: {1}", credentialId, userId );
							EmailManager.NotifyAdmin( thisClassName + ".UnPublish Failed", message );
						}
					}
					else
					{
						statusMessage = "Error - update failed, as record was not found.";
					}
				}
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{
					string message = HandleDBValidationError( dbex, thisClassName + ".)() ", efEntity.Name );
					statusMessage = "Error - the unpublish was not successful. " + message;

				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".UnPublish(), Credential: {0}, updatedById: {1}", credentialId, userId ) );
					statusMessage = ex.Message + ( ex.InnerException != null && ex.InnerException.InnerException != null ? " - " + ex.InnerException.InnerException.Message : "" );
				}
			}

			return isValid;
		}
		public bool UpdateParts( CM.Credential entity, bool isAdd, ref string statusMessage )
		{
			bool isAllValid = true;
			statusMessage = "";
			int count = 0;
			List<string> messages = new List<string>();
			//OrganizationRoleManager orgMgr = new OrganizationRoleManager();
			if ( UpdateProperties( entity, ref messages ) == false )
			{
				isAllValid = false;
			}
			Entity_ReferenceManager erm = new
						Entity_ReferenceManager();

			if ( erm.Entity_Reference_Update( entity.Subject, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_SUBJECT, false ) == false )
				isAllValid = false;

			if ( erm.Entity_Reference_Update( entity.Keyword, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_KEYWORD, false ) == false )
				isAllValid = false;

			if ( erm.Entity_Reference_Update( entity.OtherIndustries, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_NAICS, false ) == false )
				isAllValid = false;

			if ( erm.Entity_Reference_Update( entity.OtherOccupations, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_SOC, false ) == false )
				isAllValid = false;

			if ( erm.Entity_Reference_Update( entity.DegreeConcentration, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_DEGREE_CONCENTRATION, false ) == false )
				isAllValid = false;

			if ( erm.Entity_Reference_Update( entity.DegreeMajor, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_DEGREE_MAJOR, false ) == false )
				isAllValid = false;

			if ( erm.Entity_Reference_Update( entity.DegreeMinor, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, entity.LastUpdatedById, ref messages, CodesManager.PROPERTY_CATEGORY_DEGREE_MINOR, false ) == false )
				isAllValid = false;



			//note this may only be necessary on add, if we use the agent role popup for updates!
			//17-02-08 mparsons - will always do, as now updatable from page
			//need to prevent removing owner

			if ( isAdd || ( entity.OwnerRoles != null && entity.OwnerRoles.Items.Count > 0 ) )
			{
				if ( entity.OwnerRoles == null || entity.OwnerRoles.Items.Count == 0 )
				{
					messages.Add( "Invalid request, please select one or more roles for the owing agent." );
					isAllValid = false;
				}
				//the owner role must be selected
				else if ( entity.OwnerRoles.GetFirstItemId() != Entity_AgentRelationshipManager.ROLE_TYPE_OWNER )
				{
					messages.Add( "Invalid request. The role \"Owned By\" must be one of the roles selected." );
					isAllValid = false;
				}
				else
				{
					OrganizationRoleProfile profile = new OrganizationRoleProfile();
					profile.ParentUid = entity.RowId;
					profile.ActingAgentUid = entity.OwningAgentUid;
					profile.AgentRole = entity.OwnerRoles;
					profile.CreatedById = entity.LastUpdatedById;
					profile.LastUpdatedById = entity.LastUpdatedById;

					if ( !new Entity_AgentRelationshipManager().Agent_EntityRoles_Save( profile, Entity_AgentRelationshipManager.VALID_ROLES_OWNER,  entity.LastUpdatedById, ref messages ) )
						isAllValid = false;
				}
			}
		
			statusMessage = string.Join( "<br/>", messages.ToArray() );
			return isAllValid;
		}
		public bool UpdateProperties( ThisEntity entity, ref List<string> messages )
		{
			bool isAllValid = true;
			//OLD
			//PropertyMgr cpm = new PropertyMgr();
			//if ( cpm.UpdateProperties( entity, ref messages ) == false )
			//	isAllValid = false;


			//============================
			EntityPropertyManager mgr = new EntityPropertyManager();

			//if ( mgr.UpdateProperties( entity.IntendedPurpose, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_PURPOSE, entity.LastUpdatedById, ref messages ) == false )
			//	isAllValid = false;

			if ( mgr.UpdateProperties( entity.CredentialType, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;

			if ( mgr.UpdateProperties( entity.AudienceLevelType, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;


			if ( mgr.UpdateProperties( entity.CredentialStatusType, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_STATUS_TYPE, entity.LastUpdatedById, ref messages ) == false )
				isAllValid = false;


			//if ( mgr.UpdateProperties( entity.EarningCredentialPrimaryMethodId, entity.RowId, CodesManager.ENTITY_TYPE_CREDENTIAL, CodesManager.PROPERTY_CATEGORY_PRIMARY_EARN_METHOD, entity.LastUpdatedById, ref messages ) == false )
			//	isAllValid = false;

			return isAllValid;
		}



		/// <summary>
		/// Delete a credential
		/// 16-04-27 mparsons - changed to a virual delete
		/// </summary>
		/// <param name="id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Credential_Delete( int id, int userId, ref string statusMessage )
		{
			bool isValid = false;
			if ( id == 0 )
				{
					statusMessage = "Error - missing an identifier for the Credential";
					return false;
				}
			using ( var context = new Data.CTIEntities() )
			{
				EM.Credential efEntity = context.Credential
							.SingleOrDefault( s => s.Id == id );

				if ( efEntity != null && efEntity.Id > 0 )
				{
					statusMessage = string.Format("Credential: {0}, Id:{1}", efEntity.Name, efEntity.Id);

					//context.Credential.Remove( efEntity );
					efEntity.LastUpdated = System.DateTime.Now;
					efEntity.LastUpdatedById = userId;
					efEntity.StatusId = CodesManager.ENTITY_STATUS_DELETED;

					int count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					} else
						statusMessage = "Error - delete failed, but no message was provided.";
				}
				else
				{
					statusMessage = "Error - delete failed, as record was not found.";
				}
			}

			return isValid;
		}


		#endregion

		#region credential - retrieval ===================
		public static CM.Credential GetForEdit( int id )
		{
			CM.Credential entity = new CM.Credential();
			CredentialRequest cr = new CredentialRequest();
			cr.IsEditRequest();

			using ( var context = new Data.CTIEntities() )
			{
				EM.Credential item = context.Credential
							.SingleOrDefault( s => s.Id == id
								&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
								);

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, cr );
				}
			}

			return entity;
		}
		public static CM.Credential GetForCompare( int id, CredentialRequest cr )
		{
			CM.Credential entity = new CM.Credential();
			if ( id < 1 )
				return entity;
			using ( var context = new Data.CTIEntities() )
			{
				//context.Configuration.LazyLoadingEnabled = false;
				EM.Credential item = context.Credential
							.SingleOrDefault( s => s.Id == id
								&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
								);

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, cr);
				}
			}

			return entity;
		}
		/// <summary>
		/// Get a credential
		/// ?should we allow get on a 'deleted' cred? Most people wouldn't remember the Id, although could be from a report
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static CM.Credential GetBasic( int id )
		{

			CM.Credential entity = new CM.Credential();
			CredentialRequest cr = new CredentialRequest();
			cr.IsForProfileLinks = true;
			if ( id < 1 )
				return entity;

			using ( var context = new Data.CTIEntities() )
			{
				if ( cr.IsForProfileLinks )
					context.Configuration.LazyLoadingEnabled = false;
				EM.Credential item = context.Credential
							.SingleOrDefault( s => s.Id == id
								&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
								);

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, cr );

					//Other parts
				}
			}

			return entity;
		}
		public static CM.Credential GetBasic( Guid rowId, bool isForEditView, bool isForLink = false )
		{
			CM.Credential entity = new CM.Credential();
			CredentialRequest cr = new CredentialRequest();
			if ( isForEditView || isForLink )
				cr.IsForProfileLinks = true;
			else
			{
				cr.IncludingProperties = true;
				cr.IncludingSubjectsKeywords = true;
				cr.BubblingUpSubjects = true;
				cr.IncludingFrameworkItems = true;
			}

			cr.IsForEditView = isForEditView;

			using ( var context = new Data.CTIEntities() )
			{
				if ( cr.IsForProfileLinks ) //get minimum
					context.Configuration.LazyLoadingEnabled = false;

				EM.Credential item = context.Credential
							.SingleOrDefault( s => s.RowId == rowId
								&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
								);

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, cr );
				}
			}

			return entity;
		}

		public static CM.Credential GetBasicWithConditions( Guid rowId)
		{
			CM.Credential entity = new CM.Credential();
			CredentialRequest cr = new CredentialRequest();
			cr.IsForProfileLinks = true;
			cr.IsForEditView = false;

			using ( var context = new Data.CTIEntities() )
			{
				if ( cr.IsForProfileLinks ) //get minimum
					context.Configuration.LazyLoadingEnabled = false;

				EM.Credential item = context.Credential
							.SingleOrDefault( s => s.RowId == rowId
								&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
								);

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity, cr );

					Entity_ConditionProfileManager.FillConditionProfilesForList( entity, cr.IsForEditView );
				}
			}

			return entity;
		}


		//public static CM.Credential Credential_GetBasic( Guid rowId,
		//		bool includingProperties,
		//		bool includingProfiles = false,
		//		bool forProfileLink = false )
		//{

		//	CM.Credential entity = new CM.Credential();
		//	bool forEditView = false;

		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		if ( forProfileLink )
		//			context.Configuration.LazyLoadingEnabled = false;
		//		EM.Credential item = context.Credential
		//					.SingleOrDefault( s => s.RowId == rowId
		//						&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
		//						);

		//		if ( item != null && item.Id > 0 )
		//		{
		//			ToMap( item, entity, includingProperties, includingProfiles, forEditView, forProfileLink );

		//			//Other parts
		//		}
		//	}

		//	return entity;
		//}
		public static CM.Credential GetForDetail( int id, CredentialRequest cr )
		{
			CM.Credential entity = new CM.Credential();

			using ( var context = new Data.CTIEntities() )
			{

				//context.Configuration.LazyLoadingEnabled = false;
				EM.Credential item = context.Credential
							.SingleOrDefault( s => s.Id == id
								&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
								);
				try
				{
					if ( item != null && item.Id > 0 )
					{
						ToMap( item, entity, cr );
						//get summary for some totals
						EM.Credential_SummaryCache cache = GetSummary( item.Id );
						if ( cache != null && cache.BadgeClaimsCount > 0 )
							entity.HasVerificationType_Badge = true;
					}
				} catch (Exception ex)
				{
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".GetForDetail(), Name: {0} ({1}", item.Name, item.Id ) );
					entity.StatusMessage = FormatExceptions( ex );
					entity.Id = 0;
				}
			}

			return entity;
		}

		/// <summary>
		/// Get summary view of a credential
		/// Useful for accessing counts
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static EM.Credential_SummaryCache GetSummary( int id )
		{

			EM.Credential_SummaryCache item = new Data.Credential_SummaryCache();
			using ( var context = new Data.CTIEntities() )
			{

				item = context.Credential_SummaryCache
							.SingleOrDefault( s => s.CredentialId == id );

				if ( item != null && item.CredentialId > 0 )
				{

				}
			}

			return item;
		}


		//public static CM.Credential Credential_GetForLink( Guid rowId )
		//{
		//	CM.Credential entity = new CM.Credential();
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		context.Configuration.LazyLoadingEnabled = false;

		//		EM.Credential item = context.Credential
		//					.SingleOrDefault( s => s.RowId == rowId
		//						&& s.StatusId <= CodesManager.ENTITY_STATUS_PUBLISHED
		//						);

		//		if ( item != null && item.Id > 0 )
		//		{
		//			ToMapForLink( item, entity );
		//		}
		//	}

		//	return entity;
		//}

		public static List<string> Autocomplete( string pFilter, int pageNumber, int pageSize, int userId, ref int pTotalRows )
		{
			bool autocomplete = true;
			List<string> results = new List<string>();

			List<CM.CredentialSummary> list = Search( pFilter, "", pageNumber, pageSize, ref pTotalRows, userId, autocomplete );

			string prevName = "";
			foreach ( CM.CredentialSummary item in list )
			{
				//note excluding duplicates may have an impact on selected max terms
				if ( item.Name.ToLower() != prevName)
					results.Add( item.Name );

				prevName = item.Name.ToLower();
			}

			return results;
		}

		
		public static List<CM.CredentialSummary> Search( string pFilter, string pOrderBy, int pageNumber, int pageSize, ref int pTotalRows, int userId = 0, bool autocomplete = false )
		{
			string connectionString = DBConnectionRO();
			CM.CredentialSummary item = new CM.CredentialSummary();
			List<CM.CredentialSummary> list = new List<CM.CredentialSummary>();
			var result = new DataTable();
			string creatorOrgs = "";
			string owningOrgs = "";
			int orgId = 0;
			bool includingHasPartIsPartWithConnections = UtilityManager.GetAppKeyValue( "includeHasPartIsPartWithConnections", false );

			//int avgMinutes = 0;
			//string orgName = "";
			//int totals = 0;

			using ( SqlConnection c = new SqlConnection( connectionString ) )
			{
				c.Open();

				if ( string.IsNullOrEmpty( pFilter ) )
				{
					pFilter = "";
				}

				using ( SqlCommand command = new SqlCommand( "[Credential.Search]", c ) )
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

				//Used for costs. Only need to get these once. See below. - NA 5/12/2017
				var currencies = CodesManager.GetCurrencies();
				var costTypes = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_ATTAINMENT_COST );

				foreach ( DataRow dr in result.Rows )
				{
					//avgMinutes = 0;
					item = new CM.CredentialSummary();
					item.Id = GetRowColumn( dr, "Id", 0 );
					
					//item.Name = GetRowColumn( dr, "Name", "missing" );
					item.Name = dr[ "Name"].ToString();
					item.FriendlyName = FormatFriendlyTitle( item.Name );

					//for autocomplete, only need name
					if ( autocomplete )
					{
						list.Add( item );
						continue;
					}
					//string rowId = GetRowColumn( dr, "RowId" );
					//string rowId = GetRowColumn( dr, "EntityUid" );
					string rowId = dr[ "EntityUid" ].ToString();
					//if ( IsGuidValid( rowId ) )
					item.RowId = new Guid( rowId );

					
					//item.Description = GetRowColumn( dr, "Description", "" );
					item.Description = dr[ "Description" ].ToString();
					//item.Url = GetRowColumn( dr, "Url", "" );
					item.Url = dr[ "Url" ].ToString();

					item.CanEditRecord = GetRowColumn( dr, "CanEditRecord", false );

					item.ManagingOrgId = GetRowPossibleColumn( dr, "ManagingOrgId", 0 );
					//item.ManagingOrganization = GetRowPossibleColumn( dr, "ManagingOrganization" );
					item.ManagingOrganization = dr[ "ManagingOrganization" ].ToString();
					//creatorOrgs = GetRowPossibleColumn( dr, "CreatorOrgs" );
					creatorOrgs = dr[ "CreatorOrgs" ].ToString();
		
					//owningOrgs = GetRowPossibleColumn( dr, "OwningOrgs" );
					owningOrgs = dr[ "OwningOrgs" ].ToString();
					
				
					item.OwnerOrganizationId = GetRowPossibleColumn( dr, "OwningOrganizationId", 0 );
					item.OwnerOrganizationName = GetRowPossibleColumn( dr, "owningOrganization" );

					item.CTID = GetRowColumn( dr, "CTID" );
					//item.CredentialRegistryId = GetRowPossibleColumn( dr, "CredentialRegistryId" );
					item.CredentialRegistryId = dr[ "CredentialRegistryId" ].ToString();

					string date = GetRowColumn( dr, "EffectiveDate", "" );
					if ( IsValidDate( date ) )
						item.DateEffective = ( DateTime.Parse(date ).ToShortDateString());
					else
						item.DateEffective = "";
					date = GetRowColumn( dr, "Created", "" );
					if ( IsValidDate( date ) )
						item.Created = DateTime.Parse(date );
					date = GetRowColumn( dr, "LastUpdated", "" );
					if ( IsValidDate( date ) )
						item.LastUpdated = DateTime.Parse(date );

					//item.Version = GetRowPossibleColumn( dr, "Version", "" );
					item.Version = dr[ "Version" ].ToString();
					//item.LatestVersionUrl = GetRowPossibleColumn( dr, "LatestVersionUrl", "" );
					item.LatestVersionUrl = dr[ "LatestVersionUrl" ].ToString();
					//item.PreviousVersion = GetRowPossibleColumn( dr, "PreviousVersion", "" );
					item.PreviousVersion = dr[ "ReplacesVersionUrl" ].ToString();
					//item.AvailableOnlineAt = GetRowPossibleColumn( dr, "AvailableOnlineAt", "" );

					//item.CredentialType = GetRowPossibleColumn( dr, "CredentialType", "" );
					item.CredentialType = dr[ "CredentialType" ].ToString();
					//item.CredentialTypeSchema = GetRowPossibleColumn( dr, "CredentialTypeSchema", "" );
					item.CredentialTypeSchema = dr[ "CredentialTypeSchema" ].ToString();
					item.TotalCost = GetRowPossibleColumn( dr, "TotalCost", 0m );
					//AverageMinutes is a rough approach to sorting. If present, get the duration profiles
					if ( GetRowPossibleColumn( dr, "AverageMinutes", 0 ) > 0 )
					{
						item.EstimatedTimeToEarn = DurationProfileManager.GetAll( item.RowId );
					}

					item.IsAQACredential = GetRowColumn( dr, "IsAQACredential", false );
					item.HasQualityAssurance = GetRowColumn( dr, "HasQualityAssurance", false );

					item.LearningOppsCompetenciesCount = GetRowColumn( dr, "LearningOppsCompetenciesCount", 0 );
					item.AssessmentsCompetenciesCount = GetRowColumn( dr, "AssessmentsCompetenciesCount", 0 );

					item.QARolesCount = GetRowColumn( dr, "QARolesCount", 0 );

					item.HasPartCount = GetRowColumn( dr, "HasPartCount", 0 );
					item.IsPartOfCount = GetRowColumn( dr, "IsPartOfCount", 0 );
					item.RequiresCount = GetRowColumn( dr, "RequiresCount", 0 );
					item.RecommendsCount = GetRowColumn( dr, "RecommendsCount", 0 );
					item.RequiredForCount = GetRowColumn( dr, "isRequiredForCount", 0 );
					item.IsRecommendedForCount = GetRowColumn( dr, "IsRecommendedForCount", 0 );
					item.RenewalCount = 0;// GetRowColumn( dr, "RenewalCount", 0 );
					item.IsAdvancedStandingForCount = GetRowColumn( dr, "IsAdvancedStandingForCount", 0 );
					item.AdvancedStandingFromCount = GetRowColumn( dr, "AdvancedStandingFromCount", 0 );
					item.PreparationForCount = GetRowColumn( dr, "isPreparationForCount", 0 );
					item.PreparationFromCount = GetRowColumn( dr, "isPreparationFromCount", 0 );

					//NAICS CSV
					//16-09-12 mp - changed to use pipe (|) rather than ; due to conflicts with actual embedded semicolons
					item.NaicsResults = Fill_CodeItemResults( dr, "NaicsList", CodesManager.PROPERTY_CATEGORY_NAICS, false, false );
					item.IndustryOtherResults = Fill_CodeItemResults( dr, "OtherIndustriesList", CodesManager.PROPERTY_CATEGORY_NAICS, false, false, false );

					//OccupationsCSV
					item.OccupationResults = Fill_CodeItemResults( dr, "OccupationsList", CodesManager.PROPERTY_CATEGORY_SOC, false, false );
					item.OccupationOtherResults = Fill_CodeItemResults( dr, "OtherOccupationsList", CodesManager.PROPERTY_CATEGORY_SOC, false, false, false );
					//education levels CSV
					//16-09-12 mp - changed to use pipe (|) rather than ; due to conflicts with actual embedded semicolons
					item.LevelsResults = Fill_CodeItemResults( dr, "LevelsList", CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL, false, false );

					item.QARolesResults =  Fill_CodeItemResults(dr, "QARolesList", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, true);

					item.QAOrgRolesResults = Fill_CodeItemResults( dr, "QAOrgRolesList", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, true );

					item.AgentAndRoles = Fill_AgentRelationship( dr, "AgentAndRoles", CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE, false, false, true );

					item.ConnectionsList = Fill_CodeItemResults( dr, "ConnectionsList", CodesManager.PROPERTY_CATEGORY_CONNECTION_PROFILE_TYPE, true, true );
					if ( includingHasPartIsPartWithConnections )
					{
						//manually add other connections
						if ( item.HasPartCount > 0 )
						{
							item.ConnectionsList.Results.Add( new CodeItem() { Id = 0, Title = "Includes", SchemaName = "hasPart", Totals = item.HasPartCount } );
						}
						if ( item.IsPartOfCount > 0 )
						{
							item.ConnectionsList.Results.Add( new CodeItem() { Id = 0, Title = "Included With", SchemaName = "isPartOf", Totals = item.IsPartOfCount } );
						}
					}
					

					item.CredentialsList = Fill_CredentialConnectionsResult( dr, "CredentialsList", CodesManager.PROPERTY_CATEGORY_CONNECTION_PROFILE_TYPE );
					//
					item.ListTitle = item.Name + " (" + item.OwnerOrganizationName + ")";

					string subjects = dr[ "SubjectsList" ].ToString();//GetRowPossibleColumn( dr, "", "" );
					
					if ( !string.IsNullOrWhiteSpace( subjects ) )
					{
						var codeGroup = subjects.Split( '|' );
						foreach ( string codeSet in codeGroup )
						{
							var codes = codeSet.Split( '~' );
							item.Subjects.Add( codes[ 0 ].Trim() );
						}
					}

					//addressess
					int addressess = GetRowPossibleColumn( dr, "AvailableAddresses", 0 );
					if ( addressess > 0 )
					{
						item.Addresses = AddressProfileManager.GetAll( item.RowId );
					}

					//Edit - Estimated Costs - needed for gray buttons in search results. Copied from ToMap method, then edited to move database calls outside of foreach loop. - NA 5/12/2017
					//this only gets for the credential, need to alter to get all - should change to an ajax call
					/*
					 * - cred
					 *		- conditions
					 *			- asmts
					 *				costs
					 *			- lopp
					 *				costs
					 */

					item.NumberOfCostProfileItems = GetRowColumn( dr, "NumberOfCostProfileItems", 0 );

					//item.EstimatedCost = CostProfileManager.GetAll( item.RowId, false );
					//foreach ( var cost in item.EstimatedCost )
					//{
					//	cost.CurrencyTypes = currencies;
					//	foreach ( var costItem in cost.Items )
					//	{
					//		costItem.CostType.Items.Add( costTypes.Items.FirstOrDefault( m => m.CodeId == costItem.CostTypeId ) );
					//	}
					//}
					//End edit
					//badgeClaimsCount
					if ( GetRowPossibleColumn( dr, "badgeClaimsCount", 0 ) > 0 )
					{
						//Edit - Has Badge Verification Service.  Needed in search results. - NA 6/1/2017
						item.HasVerificationType_Badge = true;  //Update this with appropriate source data
					}
					list.Add( item );
				}

				return list;

			}
		}
		

		/// <summary>
		/// Expect
		/// - relationshipId (RoleId)
		/// - Relationship
		/// - AgentId
		/// - Agent Name
		/// </summary>
		/// <param name="dr"></param>
		/// <param name="fieldName"></param>
		/// <param name="categoryId"></param>
		/// <param name="hasSchemaName"></param>
		/// <param name="hasTotals"></param>
		/// <param name="hasAnIdentifer"></param>
		/// <returns></returns>
		private static CM.AgentRelationshipResult Fill_AgentRelationship( DataRow dr, string fieldName, int categoryId, bool hasSchemaName, bool hasTotals, bool hasAnIdentifer = true )
		{
			string list = dr[ fieldName ].ToString();
			CM.AgentRelationshipResult item = new CM.AgentRelationshipResult() { CategoryId = categoryId };
			item.HasAnIdentifer = hasAnIdentifer;
			AgentRelationship code = new AgentRelationship();
			int totals = 0;
			int id = 0;

			if ( !string.IsNullOrWhiteSpace( list ) )
			{

				var codeGroup = list.Split( '|' );
				foreach ( string codeSet in codeGroup )
				{
					code = new AgentRelationship();

					var codes = codeSet.Split( '~' );
					//schema = "";
					totals = 0;
					id = 0;
					if ( hasAnIdentifer )
					{
						Int32.TryParse( codes[ 0 ].Trim(), out id );
						code.RelationshipId = id;
						code.Relationship = codes[ 1 ].Trim();

						Int32.TryParse( codes[ 2 ].Trim(), out id );
						code.AgentId = id;
						code.Agent = codes[ 3 ].Trim();
						
					}
					else
					{
						//currently if no Id, assume only text value
						//title = codes[ 0 ].Trim();
					}
					item.Results.Add( code );
				}
			}

			return item;
		}
		private static bool ExtractOrg( string data, ref int orgId, ref string orgName )
		{
			var org = data.Split( ',' );
			orgName = org[ 1 ].Trim();
			if (Int32.TryParse(org[ 0 ].Trim(), out orgId))
				return true;
			else
				return false;
			
		
		}

		/// <summary>
		/// Search for credential assets.
		/// At this time the number would seem to be small, so not including paging
		/// </summary>
		/// <param name="credentialId"></param>
		/// <returns></returns>
		public static List<CM.Entity> CredentialAssetsSearch( int credentialId )
		{
			CM.Entity result = new CM.Entity();
			List<CM.Entity> list = new List<CM.Entity>();
			using ( var context = new ViewContext() )
			{
				List<Views.Credential_Assets> results = context.Credential_Assets
					.Where( s => s.CredentialId == credentialId )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach (Views.Credential_Assets item in results ) 
					{
						result = new CM.Entity();
						result.Id = item.AssetEntityId;
						result.EntityBaseId = item.AssetId;
						result.EntityUid = item.AssetEntityUid;
						result.EntityTypeId = item.AssetTypeId;
						result.EntityType = item.AssetType;
						result.EntityBaseName = item.Name;

						list.Add( result );
					}
					
				}
			}

			return list;
		}
		public static List<CodeItem> CredentialAssetsSearch2( int credentialId )
		{
			CodeItem result = new CodeItem();
			List<CodeItem> list = new List<CodeItem>();
			using ( var context = new ViewContext() )
			{
				List<Views.Credential_Assets> results = context.Credential_Assets
					.Where( s => s.CredentialId == credentialId )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Views.Credential_Assets item in results )
					{
						result = new CodeItem();
						result.Id = item.AssetEntityId;
						result.Title = item.AssetType + " - " + item.Name;

						list.Add( result );
					}

				}
			}

			return list;
		}
		/// <summary>
		/// Map properties from the database to the class
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="cr"></param>
		public static void ToMap( EM.Credential from, CM.Credential to,
					CredentialRequest cr )
		{
			to.Id = from.Id;
			to.StatusId = from.StatusId ?? 1;
			to.RowId = from.RowId;

			to.Name = from.Name;
			to.Description = from.Description;

			to.SubjectWebpage = from.Url != null ? from.Url : "";

			to.ctid = from.CTID;
			to.CredentialRegistryId = from.CredentialRegistryId;
			// 16-06-15 mp - always include credential type
			to.CredentialType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE );

			to.ManagingOrgId = from.ManagingOrgId ?? 0;
			if ( from.ImageUrl != null && from.ImageUrl.Trim().Length > 0 )
				to.ImageUrl = from.ImageUrl;
			else
				to.ImageUrl = null;

			if ( IsGuidValid( from.OwningAgentUid ) )
			{
				to.OwningAgentUid = ( Guid ) from.OwningAgentUid;
				to.OwningOrganization = OrganizationManager.GetForSummary( to.OwningAgentUid );

				//get roles
				OrganizationRoleProfile orp = Entity_AgentRelationshipManager.AgentEntityRole_GetAsEnumerationFromCSV( to.RowId, to.OwningAgentUid );
				to.OwnerRoles = orp.AgentRole;
			}

			to.CredentialTypeDisplay = to.CredentialType.GetFirstItem().Name;
			to.CredentialTypeSchema = to.CredentialType.GetFirstItem().SchemaName;
			//
			to.OwningOrgDisplay = to.OwningOrganization.Name;

			to.AudienceLevelType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL );


			if ( cr.IsForProfileLinks ) //return minimum ===========
				return;
			//===================================================================

			if ( IsGuidValid( from.CopyrightHolder ) )
			{
				to.CopyrightHolder = ( Guid ) from.CopyrightHolder;
				//not sure if we need the org for display?
				to.CopyrightHolderOrganization = OrganizationManager.GetForSummary( to.CopyrightHolder );
			}

			to.AlternateName = from.AlternateName;
			to.CredentialId = from.CredentialId;
			to.CodedNotation = from.CodedNotation;
			to.AvailabilityListing = from.AvailabilityListing;

			to.VersionIdentifier = from.Version;
			if ( IsValidDate( from.EffectiveDate ) )
				to.DateEffective = ( ( DateTime ) from.EffectiveDate ).ToShortDateString();
			else
				to.DateEffective = "";

			to.LatestVersionUrl = from.LatestVersionUrl;
			to.PreviousVersion = from.ReplacesVersionUrl;

			to.AvailableOnlineAt = from.AvailableOnlineAt;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;
			to.LastUpdatedBy = SetLastUpdatedBy( to.LastUpdatedById, from.Account_Modifier );

			if ( from.InLanguageId != null )
			{
				to.InLanguageId = ( int ) from.InLanguageId;
				to.InLanguage = from.Codes_Language.LanguageName;
				to.InLanguageCode = from.Codes_Language.LangugeCode;
			}
			else
			{
				to.InLanguageId = 0;
				to.InLanguage = "";
				to.InLanguageCode = "";
			}

			to.EarningCredentialPrimaryMethodId = from.EarningCredentialPrimaryMethodId ?? 0;
			to.FeatureLearningOpportunities = ( bool ) (from.FeatureLearningOpportunities ?? false);
			to.FeatureAssessments = ( bool ) (from.FeatureAssessments ?? false);

			to.ProcessStandards = from.ProcessStandards ?? "";
			to.ProcessStandardsDescription = from.ProcessStandardsDescription ?? "";

			//properties ===========================================

			if ( cr.IncludingProperties )
			{
				

				//to.IntendedPurpose = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_PURPOSE );

				if ( cr.IncludingEstimatedCosts )
				{
					to.EstimatedCosts = CostProfileManager.GetAll( to.RowId, cr.IsForEditView );

					//Include currencies to fix null errors in various places (detail, compare, publishing) - NA 3/17/2017
					var currencies = CodesManager.GetCurrencies();
					//Include cost types to fix other null errors - NA 3/17/2017
					var costTypes = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_ATTAINMENT_COST );
					foreach( var cost in to.EstimatedCosts )
					{
						cost.CurrencyTypes = currencies;

						foreach( var costItem in cost.Items )
						{
							costItem.CostType.Items.Add( costTypes.Items.FirstOrDefault( m => m.CodeId == costItem.CostTypeId ) );
						}
					}
					//End edits - NA 3/17/2017
				}

				to.CredentialStatusType = EntityPropertyManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_STATUS_TYPE );
			}
			//just in case
			if ( to.EstimatedCosts == null )
				to.EstimatedCosts = new List<CostProfile>();

			//profiles ==========================================
			to.FinancialAssistance = Entity_FinancialAlignmentProfileManager.GetAll( to.RowId, cr.IsForEditView );

			if ( cr.IncludingAddesses )
				to.Addresses = AddressProfileManager.GetAll( to.RowId );

			if ( cr.IncludingDuration )
				to.EstimatedDuration = DurationProfileManager.GetAll( to.RowId );

			if ( cr.IncludingFrameworkItems )
			{
				to.Occupation = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );
				to.OtherOccupations = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SOC );

				to.Industry = Entity_FrameworkItemManager.FillEnumeration( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );
				to.OtherIndustries = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_NAICS );
			}
			 
			if ( cr.IncludingConnectionProfiles )
			{
				//get all associated top level learning opps, and assessments
				//will always be for profile lists - not expected any where else other than edit


				//assessment
				//for entity.condition(ec) - entity = ec.rowId
				//actually, should these only be for edit view. For detail, they will be drawn from conditions!!!
				if ( cr.IsForEditView )
				{
					to.TargetAssessment = Entity_AssessmentManager.GetAll( to.RowId, cr.IsForEditView );
					foreach ( AssessmentProfile ap in to.TargetAssessment )
					{
						if ( ap.EstimatedCost != null && ap.EstimatedCost.Count > 0 )
						{
							//to.AssessmentEstimatedCosts.AddRange( ap.EstimatedCost );
							//to.EstimatedCosts.AddRange( ap.EstimatedCost );
						}
					}

					to.TargetLearningOpportunity = Entity_LearningOpportunityManager.LearningOpps_GetAll( to.RowId, cr.IsForEditView, cr.IsForEditView );
					foreach ( LearningOpportunityProfile lp in to.TargetLearningOpportunity )
					{
						if ( lp.EstimatedCost != null && lp.EstimatedCost.Count > 0 )
						{
							//to.LearningOpportunityEstimatedCosts.AddRange( lp.EstimatedCost );
							//to.EstimatedCosts.AddRange( lp.EstimatedCost );
						}
					}

					//not sure if competencies are germain
					foreach ( LearningOpportunityProfile e in to.TargetLearningOpportunity )
					{
						if ( e.HasCompetencies || e.ChildHasCompetencies )
						{
							to.ChildHasCompetencies = true;
							break;
						}
					}
				}

				//TODO - need distinguish between embedded and those for a condition profile - maybe-defer use of this until certain
				//to.TargetCredential = Entity_CredentialManager.GetAll( to.RowId );

				//******************get all condition profiles *******************
				//TODO - have custom version of this to only get minimum!!
				//NOTE - the IsForEditView relates to cred, but probably don't want to sent true to the fill
				//re: commonConditions - consider checking if any exist, and if not, don't show
				if ( cr.IsForEditView )
				{
					Entity_ConditionProfileManager.FillConditionProfilesForList( to, cr.IsForEditView );

					to.CommonConditions = Entity_CommonConditionManager.GetAll( to.RowId, cr.IsForEditView );

					to.CommonCosts = Entity_CommonCostManager.GetAll( to.RowId, cr.IsForEditView );
				}
				else
				{
					//need to ensure competencies are bubbled up
					Entity_ConditionProfileManager.FillConditionProfilesForDetailDisplay( to );

					to.CommonConditions = Entity_CommonConditionManager.GetAll( to.RowId, cr.IsForEditView );
					to.CommonCosts = Entity_CommonCostManager.GetAll( to.RowId, cr.IsForEditView );
				}

			}

			if ( cr.IncludingRevocationProfiles )
			{
				to.Revocation = Entity_RevocationProfileManager.GetAll( to.RowId );
			}

			if ( cr.IncludingJurisdiction )
			{
				to.Jurisdiction = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId );
				to.Region = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_RESIDENT );
			}
			//TODO - CredentialProcess is used in the detail pages. Should be removed and use individual profiles
			//List<ProcessProfile>  credentialProcess =
			to.CredentialProcess = Entity_ProcessProfileManager.GetAll( to.RowId, cr.IsForEditView );
			foreach ( ProcessProfile item in to.CredentialProcess )
			{
				if ( item.ProcessTypeId == Entity_ProcessProfileManager.ADMIN_PROCESS_TYPE )
					to.AdministrationProcess.Add( item );
				else if ( item.ProcessTypeId == Entity_ProcessProfileManager.DEV_PROCESS_TYPE )
					to.DevelopmentProcess.Add( item );
				else if ( item.ProcessTypeId == Entity_ProcessProfileManager.MTCE_PROCESS_TYPE )
					to.MaintenanceProcess.Add( item );
				else
				{
					//unexpected
				}
			}

			if ( cr.IncludingEmbeddedCredentials )
			{
				to.EmbeddedCredentials = Entity_CredentialManager.GetAll( to.RowId );
			}


			//populate is part of - when??
			if ( from.Entity_Credential != null && from.Entity_Credential.Count > 0 )
			{
				foreach ( EM.Entity_Credential ec in from.Entity_Credential )
				{
					if ( ec.Entity != null )
					{
						//This method needs to be enhanced to get enumerations for the credential for display on the detail page - NA 6/2/2017
						//Need to determine is when non-edit, is actually for the detail reference
						//only get where parent is a credential, ex not a condition profile
						if (ec.Entity.EntityTypeId == CodesManager.ENTITY_TYPE_CREDENTIAL)
							to.IsPartOf.Add( GetBasic( ec.Entity.EntityUid, cr.IsForEditView, false) ); 
					}
				}
			}

			if ( cr.IncludingSubjectsKeywords )
			{
				if ( cr.BubblingUpSubjects )
					to.Subject = Entity_ReferenceManager.GetAllSubjects( to.RowId );
				else
					to.Subject = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_SUBJECT );

				to.Keyword = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_KEYWORD );
			}
			to.DegreeConcentration = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_DEGREE_CONCENTRATION );
			to.DegreeMajor = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_DEGREE_MAJOR );
			to.DegreeMinor = Entity_ReferenceManager.Entity_GetAll( to.RowId, CodesManager.PROPERTY_CATEGORY_DEGREE_MINOR );

			//---------------
			if ( cr.IncludingRolesAndActions )
			{

				to.JurisdictionAssertions = Entity_JurisdictionProfileManager.Jurisdiction_GetAll( to.RowId, Entity_JurisdictionProfileManager.JURISDICTION_PURPOSE_OFFERREDIN );

				if ( cr.IsForEditView )
				{
					//get all except owned by, and offered by
					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAllExceptOwnerSummary( to.RowId, to.OwningAgentUid, true, false );
					//to.OrganizationRole = Entity_AgentRelationshipManager.CredentialAssets_GetAllQARoles( to.Id );
					//to.OfferedByOrganizationRole = Entity_AgentRelationshipManager.CredentialAssets_GetAllOfferedByRoles( to.Id );

					to.OfferedByOrganization = Entity_AgentRelationshipManager.GetAllOfferingOrgs( to.RowId );
					//get owner roles
					to.OwnerOrganizationRoles = Entity_AgentRelationshipManager.AgentEntityRole_GetOwnerSummary( to.RowId, to.OwningAgentUid, false );

				}
				else
				{
					//get as ennumerations
					to.OrganizationRole = Entity_AgentRelationshipManager.AgentEntityRole_GetAll_ToEnumeration( to.RowId, true );
				}

				//to.QualityAssuranceAction = Entity_QualityAssuranceActionManager.QualityAssuranceActionProfile_GetAll( to.RowId );
			}


		}


		private static void FromMap( CM.Credential from, EM.Credential to )
		{
			to.Id = from.Id;
			if ( to.Id < 1 )
			{
				
				//to.CreatedById = ( int ) from.CreatedById;
				//to.LastUpdatedById = ( int ) from.CreatedById;
			}
			//don't map rowId, ctid, or dates as not on form
			//to.RowId = from.RowId;

			to.Name = GetData(from.Name);
			to.Description = GetData(from.Description);
			to.AlternateName = GetData(from.AlternateName);
			//to.CTID = from.ctid;
			to.CredentialId = string.IsNullOrWhiteSpace( from.CredentialId ) ? null : from.CredentialId;
			to.CodedNotation = GetData(from.CodedNotation);

			//generally the managing orgId should not be allowed to change in the interface - yet
			if ( from.ManagingOrgId > 0
				&& from.ManagingOrgId != ( to.ManagingOrgId ?? 0 ) )
				to.ManagingOrgId = from.ManagingOrgId;

			//handle old version setting to zero
			if ( IsGuidValid( from.OwningAgentUid ) )
			{
				if ( to.Id > 0 && to.OwningAgentUid != from.OwningAgentUid )
				{
					if ( IsGuidValid( to.OwningAgentUid ) )
					{
						//need to remove the owner role, or could have been others
						string statusMessage = "";
						new Entity_AgentRelationshipManager().Delete( to.RowId, to.OwningAgentUid, Entity_AgentRelationshipManager.ROLE_TYPE_OWNER, ref statusMessage );
					}
				}
				to.OwningAgentUid = from.OwningAgentUid;
			}
			else
			{
				//always have to have an owner
				//to.OwningAgentUid = null;
			}
				
			if ( from.OwnerOrganizationRoles != null && from.OwnerOrganizationRoles.Count > 0 )
			{
				//may need to do something in case was a change via the roles popup
			}

			to.Version = GetData(from.VersionIdentifier );
			if ( IsValidDate( from.DateEffective ) )
				to.EffectiveDate = DateTime.Parse(from.DateEffective);
			else //handle reset
				to.EffectiveDate = null;

			//to.Url = GetUrlData( from.Url, null );
			//to.SubjectWebpage = GetUrlData( from.SubjectWebpage, null );
			to.Url = GetUrlData( from.SubjectWebpage, null );

			to.LatestVersionUrl = GetUrlData( from.LatestVersionUrl, null );
			to.ReplacesVersionUrl = GetUrlData( from.PreviousVersion, null );
			to.AvailabilityListing = GetUrlData( from.AvailabilityListing, null );
			to.AvailableOnlineAt = GetUrlData( from.AvailableOnlineAt, null );
			to.ImageUrl = GetUrlData( from.ImageUrl, null );
			if ( from.InLanguageId > 0 )
				to.InLanguageId = from.InLanguageId;
			else
				to.InLanguageId = null;

			to.FeatureLearningOpportunities = from.FeatureLearningOpportunities;
			to.FeatureAssessments = from.FeatureAssessments;

			to.ProcessStandards = GetUrlData( from.ProcessStandards, null );
			to.ProcessStandardsDescription = from.ProcessStandardsDescription;
		
			

			if ( IsGuidValid( from.CopyrightHolder ) )
				to.CopyrightHolder = from.CopyrightHolder;
			else
				to.CopyrightHolder = null;
			
		}

		#endregion

	
	}
	public class CredentialRequest
	{
		public CredentialRequest()
		{
		}
		public void DoCompleteFill() {
			IncludingProperties = true;
		}
		public void IsDetailRequest()
		{
			IsForDetailView = true;
			IncludingProperties = true;
			IncludingEstimatedCosts = true;
			IncludingDuration = true;
			IncludingFrameworkItems = true;
			IncludingRolesAndActions = true;

			//add all conditions profiles for now - to get all costs
			IncludingConnectionProfiles = true;
			ConditionProfilesAsList = false;
			IncludingAddesses = true;
			IncludingSubjectsKeywords = true;
			BubblingUpSubjects = true;
			IncludingEmbeddedCredentials = true;

			IncludingJurisdiction = true;
			IncludingRevocationProfiles = true;
		}
		public void IsPublishRequest()
		{
			//check if this is valid for publishing
			IsForPublishRequest = true;
			IsForDetailView = true;
			IncludingProperties = true;
			IncludingEstimatedCosts = true;
			IncludingDuration = true;
			IncludingFrameworkItems = true;
			IncludingRolesAndActions = true;

			//add all conditions profiles for now - to get all costs
			IncludingConnectionProfiles = true;
			ConditionProfilesAsList = false;
			IncludingAddesses = true;
			IncludingSubjectsKeywords = true;
			BubblingUpSubjects = false;
			IncludingEmbeddedCredentials = true;

			IncludingJurisdiction = true;
			IncludingRevocationProfiles = true;
		}
		public void IsCompareRequest() {
			IncludingProperties = true;
			IncludingEstimatedCosts = true;
			IncludingDuration = true;
			IncludingFrameworkItems = true;
			IncludingRolesAndActions = true;

			//add all conditions profiles for now - to get all costs
			IncludingConnectionProfiles = true;
		}

		/// <summary>
		/// Indicate options for the edit view
		/// under construction
		/// - really only need details for the basic info view
		/// - other properties would need only the minimum content for profile links 
		/// </summary>
		public void IsEditRequest() {
			IsForEditView = true;
			IncludingProperties = true;
			
			IncludingDuration = true;
			IncludingAddesses = true;
			IncludingEstimatedCosts = true;
			IncludingJurisdiction = true;
			IncludingSubjectsKeywords = true;
			IncludingConnectionProfiles = true;
			ConditionProfilesAsList = true;
			IncludingFrameworkItems = true;
			IncludingEmbeddedCredentials = true;
			IncludingRolesAndActions = true;
			IncludingRevocationProfiles = true;

			//need handle only ProfileLink equivalent views for most
		}

        public bool IsForEditView { get; set; }
		public bool IsForDetailView { get; set; }
		public bool IsForPublishRequest{ get; set; }
		public bool IsForProfileLinks { get; set; }
        public bool AllowCaching { get; set; }

        public bool IncludingProperties { get; set; }
		
        public bool IncludingRolesAndActions { get; set; }
		public bool IncludingConnectionProfiles { get; set; }
		public bool ConditionProfilesAsList { get; set; }
		public bool IncludingRevocationProfiles { get; set; }
		public bool IncludingEstimatedCosts{ get; set; }
		public bool IncludingDuration{ get; set; }
		public bool IncludingAddesses { get; set; }
		public bool IncludingJurisdiction { get; set; }

        public bool IncludingSubjectsKeywords{ get; set; }
		public bool BubblingUpSubjects { get; set; }

        //public bool IncludingKeywords{ get; set; }
		//both occupations and industries, and others for latter
		public bool IncludingFrameworkItems{ get; set; }

		public bool IncludingEmbeddedCredentials { get; set; }
	}
		

}
