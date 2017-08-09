using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using CM = Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.Entity_FinancialAlignmentProfile;
using ThisEntity = Models.Common.FinancialAlignmentObject;

namespace Factories
{
	public class Entity_FinancialAlignmentProfileManager : BaseFactory
	{
		static string thisClassName = "Entity_FinancialAlignmentProfileManager";

		List<string> messages = new List<string>();


		#region === -Persistance ==================
		/// <summary>
		/// Persist FinancialAlignmentProfile
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

			DBentity efEntity = new DBentity();

			CM.Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				try
				{

					if ( ValidateProfile( entity, ref messages ) == false )
					{
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

						context.Entity_FinancialAlignmentProfile.Add( efEntity );
						count = context.SaveChanges();

						entity.Id = efEntity.Id;
						entity.RowId = efEntity.RowId;
						if ( count == 0 )
						{
							messages.Add( " Unable to add Financial Alignment Profile" );
						}
						else
						{
							// a trigger is used to create the entity Object. 
							UpdateParts( entity, userId, ref messages );
						}
					}
					else
					{

						efEntity = context.Entity_FinancialAlignmentProfile.SingleOrDefault( s => s.Id == entity.Id );
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
				catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
				{

					string message = HandleDBValidationError( dbex, thisClassName + ".Save() ", "FinancialAlignmentProfile" );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( dbex, thisClassName + string.Format( ".Save(), Parent: {0} ({1}), UserId: {2}", parent.EntityBaseName, parent.EntityBaseId, userId ) );
					isValid = false;
				}
				catch ( Exception ex )
				{
					string message = FormatExceptions( ex );
					messages.Add( "Error - the save was not successful. " + message );
					LoggingHelper.LogError( ex, thisClassName + string.Format( ".Save(), Parent: {0} ({1}), UserId: {2}", parent.EntityBaseName, parent.EntityBaseId, userId ) );
					isValid = false;
				}

			}

			return isValid;
		}


		/// <summary>
		/// Delete a Financial Alignment profile, and related Entity
		/// </summary>
		/// <param name="Id"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Delete( int Id, ref string statusMessage )
		{
			bool isValid = false;
			if ( Id == 0 )
			{
				statusMessage = "Error - missing an identifier for the FinancialAlignmentProfile";
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					DBentity efEntity = context.Entity_FinancialAlignmentProfile
								.SingleOrDefault( s => s.Id == Id );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						Guid rowId = efEntity.RowId;

						context.Entity_FinancialAlignmentProfile.Remove( efEntity );
						int count = context.SaveChanges();
						if ( count > 0 )
						{
							isValid = true;
							//do with trigger now
							//new EntityManager().Delete( rowId, ref statusMessage );
						}
					}
					else
					{
						statusMessage = "Error - delete failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, thisClassName + ".Delete()" );

					if ( ex.InnerException != null && ex.InnerException.Message != null )
					{
						statusMessage = ex.InnerException.Message;

						if ( ex.InnerException.InnerException != null && ex.InnerException.InnerException.Message != null )
							statusMessage = ex.InnerException.InnerException.Message;
					}
					if ( statusMessage.ToLower().IndexOf( "the delete statement conflicted with the reference constraint" ) > -1 )
					{
						statusMessage = "Error: this Financial Alignment cannot be deleted as it is being referenced by other items, such as roles or credentials. These associations must be removed before this Financial Alignment can be deleted.";
					}
				}
			}

			return isValid;
		}

		#region FinancialAlignmentProfile properties ===================
		public bool UpdateParts( ThisEntity entity, int userId, ref List<string> messages )
		{
			bool isAllValid = true;
			//NONE



			return isAllValid;
		} //


		#endregion

		public bool ValidateProfile( ThisEntity profile, ref List<string> messages )
		{
			bool isValid = true;
			int count = messages.Count;
			if ( profile.IsStarterProfile )
				return true;

			//check if empty
			if ( string.IsNullOrWhiteSpace( profile.TargetNode )
				&& string.IsNullOrWhiteSpace( profile.TargetNodeDescription )
				&& string.IsNullOrWhiteSpace( profile.TargetNodeName )
				&& string.IsNullOrWhiteSpace( profile.Framework )
				&& string.IsNullOrWhiteSpace( profile.FrameworkName )
				&& string.IsNullOrWhiteSpace( profile.CodedNotation )
				)
			{
				messages.Add( "Please provide a little more information, before attempting to save this profile" );
				return false;
			}
			//
			if ( string.IsNullOrWhiteSpace( profile.FrameworkName ) )
			{
				messages.Add( "A Framework name must be entered" );
			}

			if ( !IsUrlValid( profile.Framework, ref commonStatusMessage ) )
			{
				messages.Add( "The Framework Url is invalid " + commonStatusMessage );
			}

			if ( !IsUrlValid( profile.TargetNode, ref commonStatusMessage ) )
			{
				messages.Add( "The TargetNode Url is invalid " + commonStatusMessage );
			}
			if ( !string.IsNullOrWhiteSpace( profile.AlignmentDate ) 
			  && IsValidDate(profile.AlignmentDate) ==  false)
				messages.Add( "The Alignment Date is invalid " );

			if ( messages.Count > count )
				isValid = false;

			return isValid;
		}

		#endregion

		#region == Retrieval =======================
		public static ThisEntity Get( int id,
			bool forEditView = false )
		{
			ThisEntity entity = new ThisEntity();
			bool includingProfiles = false;
			if ( forEditView )
				includingProfiles = true;

			using ( var context = new Data.CTIEntities() )
			{

				DBentity item = context.Entity_FinancialAlignmentProfile
						.SingleOrDefault( s => s.Id == id );

				if ( item != null && item.Id > 0 )
				{
					ToMap( item, entity,
						true, //includingProperties
						includingProfiles,
						forEditView );
				}
			}

			return entity;
		}



		 /// <summary>
		 /// Get all the Financial Alignments for the parent entity (ex a credential)
		 /// </summary>
		 /// <param name="parentUid"></param>
		 /// <returns></returns>
		public static List<ThisEntity> GetAll( Guid parentUid, bool isForLinks )
		{
			ThisEntity to = new ThisEntity();
			List<ThisEntity> list = new List<ThisEntity>();
			CM.Entity parent = EntityManager.GetEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				return list;
			}

			try
			{
				using ( var context = new Data.CTIEntities() )
				{
					//context.Configuration.LazyLoadingEnabled = false;

					List<EM.Entity_FinancialAlignmentProfile> results = context.Entity_FinancialAlignmentProfile
							.Where( s => s.EntityId == parent.Id )
							.OrderBy( s => s.FrameworkName)
							.ThenBy( s => s.Created )
							.ToList();

					if ( results != null && results.Count > 0 )
					{
						foreach ( EM.Entity_FinancialAlignmentProfile from in results )
						{
							to = new ThisEntity();
							if ( isForLinks )
							{
								to.Id = from.Id;
								to.RowId = from.RowId;
								to.ProfileName = from.FrameworkName;
								to.Framework = from.Framework;
							}
							else
							{
								ToMap( from, to, true, true, false );
							}
							list.Add( to );
						}
					}
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, thisClassName + ".GetAll (Guid parentUid)" );
			}
			return list;
		}//
		 

		public static void FromMap( ThisEntity from, DBentity to )
		{

			//want to ensure fields from create are not wiped
			if ( to.Id == 0 )
			{
				
			}


			to.Id = from.Id;
			to.Framework = GetData( from.Framework );
			to.FrameworkName = GetData( from.FrameworkName );
			to.TargetNodeName = GetData( from.TargetNodeName );
			to.TargetNode = GetData( from.TargetNode );
			to.TargetNodeDescription = GetData( from.TargetNodeDescription );
			to.CodedNotation = from.CodedNotation;
			to.AlignmentDate = SetDate(from.AlignmentDate);
			//to.AlignmentType = from.AlignmentType;
			//if ( from.AlignmentTypeId > 0 )
			//	to.AlignmentTypeId = from.AlignmentTypeId;
			//else
				to.AlignmentTypeId = null;

			to.Weight = SetData( from.Weight, 0.01M );

		}
		public static void ToMap( DBentity from, ThisEntity to,
				bool includingProperties = false,
				bool includingProfiles = true,
				bool forEditView = true )
		{
			to.Id = from.Id;
			to.RowId = from.RowId;
			to.ParentId = from.EntityId;
			to.Framework = GetData( from.Framework );
			to.FrameworkName = GetData( from.FrameworkName );
			to.ProfileName = to.FrameworkName;
			to.TargetNodeName = GetData( from.TargetNodeName );
			to.TargetNode = GetData( from.TargetNode );
			to.TargetNodeDescription = GetData( from.TargetNodeDescription );
			to.CodedNotation = from.CodedNotation;
			
			if ( IsValidDate( from.AlignmentDate ) )
				to.AlignmentDate = ( ( DateTime ) from.AlignmentDate ).ToShortDateString();
			else
				to.AlignmentDate = "";

			to.AlignmentTypeId = from.AlignmentTypeId == null ? 0 : (int) from.AlignmentTypeId;
			if ( to.AlignmentTypeId > 0 && from.Codes_PropertyValue != null)
			{
				to.AlignmentType = from.Codes_PropertyValue.Title;
			}

			to.Weight = from.Weight != null ? (decimal) from.Weight : 0;

			if ( IsValidDate( from.Created ) )
				to.Created = ( DateTime ) from.Created;
			to.CreatedById = from.CreatedById == null ? 0 : ( int ) from.CreatedById;
			if ( IsValidDate( from.LastUpdated ) )
				to.LastUpdated = ( DateTime ) from.LastUpdated;
			to.LastUpdatedById = from.LastUpdatedById == null ? 0 : ( int ) from.LastUpdatedById;

			if ( from.Account_Modifier != null )
			{
				to.LastUpdatedBy = from.Account_Modifier.FirstName + " " + from.Account_Modifier.LastName;
			}
			else
			{
				AppUser user = AccountManager.AppUser_Get( to.LastUpdatedById );
				to.LastUpdatedBy = user.FullName();
			}


		}

		#endregion
	}
}
