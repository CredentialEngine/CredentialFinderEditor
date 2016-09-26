using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.ProfileModels;
using Models.Common;
using EM = Data;
using Utilities;

using DBentity = Data.Credential_ConnectionProfile;
using Entity = Models.ProfileModels.ConditionProfile;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;


namespace Factories
{
	public class ConnectionProfile_PartsManager : BaseFactory
	{
		static string thisClassName = "ConnectionProfile_PartsManager";

		#region ConnectionProfile properties ===================
		public bool UpdatePropertiesDELETE( Entity entity, ref List<string> messages )
		{
			bool isAllValid = true;
			int count = 0;
			int updatedCount = 0;

			if ( entity.Id == 0 )
			{
				messages.Add( string.Format("A valid connection profile identifier was not provided to the {0} UpdateProperties method.", thisClassName) );
				return false;
			}

			//For efficiency, just roll all properties together and then process

			using ( var context = new Data.CTIEntities() )
			{
				EM.ConnectionProfile_Property op = new EM.ConnectionProfile_Property();

				//get all existing
				var results = context.ConnectionProfile_Property
							.Where( s => s.ConnectionProfileId == entity.Id )
							.OrderBy( s => s.PropertyValueId )
							.ToList();

				List<EntityProperty> properties = new List<EntityProperty>();
				EntityProperty prop = new EntityProperty();
				if ( entity.CredentialType != null && entity.CredentialType.Items.Count > 0 )
				{
					foreach ( EnumeratedItem item in entity.CredentialType.Items )
					{
						prop = new EntityProperty();
						prop.ParentId = entity.Id;
						prop.PropertyValueId = item.Id;
						properties.Add( prop );
					}
				}
				if ( entity.ApplicableAudienceType != null && entity.ApplicableAudienceType.Items.Count > 0 )
				{
					foreach ( EnumeratedItem item in entity.ApplicableAudienceType.Items )
					{
						prop = new EntityProperty();
						prop.ParentId = entity.Id;
						prop.PropertyValueId = item.Id;
						properties.Add( prop );
					}
				}
				

				#region == deletes
				//should only existing ids, where not in current list, so should be deletes
				var deleteList = from existing in results
								 join item in properties
										 on existing.PropertyValueId equals item.PropertyValueId
										 into joinTable
								 from result in joinTable.DefaultIfEmpty( new EntityProperty { ParentId = 0, PropertyValueId = 0 } )
								 select new { DeleteId = existing.PropertyValueId, ParentId = ( result.ParentId ) };

				foreach ( var v in deleteList )
				{
					//Console.WriteLine( "existing: {0} input: {1}, delete? {2}", v.DeleteId, v.ItemId, v.ItemId == 0 );
					if ( v.ParentId == 0 )
					{
						//delete item
						EM.ConnectionProfile_Property p = context.ConnectionProfile_Property.FirstOrDefault( s => s.ConnectionProfileId == entity.Id && s.PropertyValueId == v.DeleteId );
						if ( p != null && p.Id > 0 )
						{
							context.ConnectionProfile_Property.Remove( p );
							count = context.SaveChanges();
						}
					}
				}
				#endregion

				#region adds
				//should only show entry ids, where not in current list, so should be adds
				var newList = from item in properties
							  join existing in results
									on item.PropertyValueId equals existing.PropertyValueId
									into joinTable
							  from addList in joinTable.DefaultIfEmpty( new EM.ConnectionProfile_Property { Id = 0, PropertyValueId = 0 } )
							  select new { AddId = item.PropertyValueId, ExistingId = addList.PropertyValueId };
				foreach ( var v in newList )
				{
					//Console.WriteLine( "input: {0} existing: {1}, Add? {2}", v.AddId, v.ExistingId, v.ExistingId == 0 );
					if ( v.ExistingId == 0 )
					{

						op = new EM.ConnectionProfile_Property();
						op.ConnectionProfileId = entity.Id;
						op.PropertyValueId = v.AddId;
						op.Created = System.DateTime.Now;
						op.CreatedById = entity.LastUpdatedById;

						context.ConnectionProfile_Property.Add( op );
						count = context.SaveChanges();
						if ( count == 0 )
						{
							messages.Add( string.Format( " Unable to add property value Id of: {0} <br\\> ", v.AddId ) );
							isAllValid = false;
						}
						else
							updatedCount++;
					}
				}
				#endregion

			}
			return isAllValid;
		}


		#endregion

		//OBSOLETE - REPLACE BY Entity_AssessmentManager CODE
		#region ConnectionProfile assessments ===================
		//public int Assessment_Add( int connectionProfileId, int assessmentId, int userId, ref List<string> messages )
		//{
		//	int id = 0;

		//	if ( connectionProfileId == 0 )
		//	{
		//		messages.Add( string.Format( "A valid connection profile identifier was not provided to the {0}.Assessment_Add method.", thisClassName ) );
		//		return 0;
		//	}
		//	if ( assessmentId == 0 )
		//	{
		//		messages.Add( string.Format( "A valid Assessment identifier was not provided to the {0}.Assessment_Add method.", thisClassName ) );
		//		return 0;
		//	}

		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		EM.ConnectionProfile_Assessment efEntity = new EM.ConnectionProfile_Assessment();
		//		try
		//		{
		//			//first check for duplicates
		//			efEntity = context.ConnectionProfile_Assessment
		//					.SingleOrDefault( s => s.ConnectionProfileId == connectionProfileId && s.AssessmentId == assessmentId );

		//			if ( efEntity != null && efEntity.Id > 0 ) 
		//			{
		//				messages.Add( string.Format( "Error - this assessment has already been added to this profile.", thisClassName ) );
		//				return 0;
		//			}

		//			efEntity = new EM.ConnectionProfile_Assessment();
		//			efEntity.ConnectionProfileId = connectionProfileId;
		//			efEntity.AssessmentId = assessmentId;

		//			efEntity.CreatedById = userId;
		//			efEntity.Created = System.DateTime.Now;

		//			context.ConnectionProfile_Assessment.Add( efEntity );

		//			// submit the change to database
		//			int count = context.SaveChanges();
		//			if ( count > 0 )
		//			{
		//				messages.Add( "Successful" );
		//				id = efEntity.Id;
		//				return efEntity.Id;
		//			}
		//			else
		//			{
		//				//?no info on error
		//				messages.Add( "Error - the add was not successful." );
		//				string message = thisClassName + string.Format( ".Assessment_Add Failed", "Attempted to add a Assessment for a connection profile. The process appeared to not work, but there was no exception, so we have no message, or no clue. ConnectionProfile: {0}, AssessmentId: {1}, createdById: {2}", connectionProfileId, assessmentId, userId);
		//				EmailManager.NotifyAdmin( thisClassName + ".Assessment_Add Failed", message );
		//			}
		//		}
		//		catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
		//		{
		//			string message = thisClassName + string.Format( ".Assessment_Add() DbEntityValidationException, ConnectionProfile: {0}, AssessmentId: {1}, createdById: {2}", connectionProfileId, assessmentId, userId );
		//			messages.Add( "Error - missing fields." );
		//			foreach ( var eve in dbex.EntityValidationErrors )
		//			{
		//				message += string.Format( "\rEntity of type \"{0}\" in state \"{1}\" has the following validation errors:",
		//					eve.Entry.Entity.GetType().Name, eve.Entry.State );
		//				foreach ( var ve in eve.ValidationErrors )
		//				{
		//					message += string.Format( "- Property: \"{0}\", Error: \"{1}\"",
		//						ve.PropertyName, ve.ErrorMessage );
		//				}

		//				LoggingHelper.LogError( message, true );
		//			}
		//		}
		//		catch ( Exception ex )
		//		{
		//			LoggingHelper.LogError( ex, thisClassName + string.Format( ".Assessment_Add(), ConnectionProfile: {0}, AssessmentId: {1}, createdById: {2}", connectionProfileId, assessmentId, userId) );
		//		}


		//	}
		//	return id;
		//}

		//public bool Assessment_Delete( int connectionProfileId, int assessmentId, ref string statusMessage )
		//{
		//	bool isValid = false;
		//	if ( connectionProfileId == 0 )
		//	{
		//		statusMessage = "Error - missing an identifier for the connection profile to remove the assessment.";
		//		return false;
		//	}
		//	if ( assessmentId == 0 )
		//	{
		//		statusMessage = "Error - missing an identifier for the Assessment to remove";
		//		return false;
		//	}
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		EM.ConnectionProfile_Assessment efEntity = context.ConnectionProfile_Assessment
		//					.SingleOrDefault( s => s.ConnectionProfileId == connectionProfileId && s.AssessmentId == assessmentId );

		//		if ( efEntity != null && efEntity.Id > 0 )
		//		{
		//			context.ConnectionProfile_Assessment.Remove( efEntity );
		//			int count = context.SaveChanges();
		//			if ( count > 0 )
		//			{
		//				isValid = true;
		//			}
		//		}
		//		else
		//		{
		//			statusMessage = "Error - delete failed, as record was not found.";
		//		}
		//	}

		//	return isValid;
		//}

		//public bool Assessment_Delete( int Id, ref string statusMessage )
		//{
		//	bool isValid = false;
		//	if ( Id == 0 )
		//	{
		//		statusMessage = "Error - missing an identifier for the Assessment to remove";
		//		return false;
		//	}
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		EM.ConnectionProfile_Assessment efEntity = context.ConnectionProfile_Assessment
		//					.SingleOrDefault( s => s.Id == Id );

		//		if ( efEntity != null && efEntity.Id > 0 )
		//		{
		//			context.ConnectionProfile_Assessment.Remove( efEntity );
		//			int count = context.SaveChanges();
		//			if ( count > 0 )
		//			{
		//				isValid = true;
		//			}
		//		}
		//		else
		//		{
		//			statusMessage = "Error - delete failed, as record was not found.";
		//		}
		//	}

		//	return isValid;
		//}

		#endregion


		//OBSOLETE - REPLACE BY Entity_LearningOpportunityManager CODE
		#region ConnectionProfile LearningOpp ===================
		//public int ConditionLearningOpp_Add( int connectionProfileId, int learningOppId, int userId, ref List<string> messages )
		//{
		//	int id = 0;

		//	if ( connectionProfileId == 0 )
		//	{
		//		messages.Add( string.Format( "A valid connection profile identifier was not provided to the {0}.ConditionLearningOpp_Add method.", thisClassName ) );
		//		return 0;
		//	}
		//	if ( learningOppId == 0 )
		//	{
		//		messages.Add( string.Format( "A valid Learning Opportunity identifier was not provided to the {0}.ConditionLearningOpp_Add method.", thisClassName ) );
		//		return 0;
		//	}

		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		EM.Entity_LearningOpportunity efEntity = new EM.Entity_LearningOpportunity();
		//		try
		//		{
		//			//first check for duplicates
		//			efEntity = context.Entity_LearningOpportunity
		//					.SingleOrDefault( s => s.EntityId == connectionProfileId && s.LearningOpportunityId == learningOppId );

		//			if ( efEntity != null && efEntity.Id > 0 )
		//			{
		//				messages.Add( string.Format( "Error - this LearningOpp has already been added to this profile.", thisClassName ) );
		//				return 0;
		//			}

		//			efEntity = new EM.Entity_LearningOpportunity();
		//			efEntity.EntityId = connectionProfileId;
		//			efEntity.LearningOpportunityId = learningOppId;

		//			efEntity.CreatedById = userId;
		//			efEntity.Created = System.DateTime.Now;

		//			context.Entity_LearningOpportunity.Add( efEntity );

		//			// submit the change to database
		//			int count = context.SaveChanges();
		//			if ( count > 0 )
		//			{
		//				messages.Add( "Successful" );
		//				id = efEntity.Id;
		//				return efEntity.Id;
		//			}
		//			else
		//			{
		//				//?no info on error
		//				messages.Add( "Error - the add was not successful." );
		//				string message = thisClassName + string.Format( ".ConditionLearningOpp_Add Failed", "Attempted to add a LearningOpp for a connection profile. The process appeared to not work, but there was no exception, so we have no message, or no clue. ConnectionProfile: {0}, learningOppId: {1}, createdById: {2}", connectionProfileId, learningOppId, userId );
		//				EmailManager.NotifyAdmin( thisClassName + ".ConditionLearningOpp_Add Failed", message );
		//			}
		//		}
		//		catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
		//		{
		//			string message = thisClassName + string.Format( ".ConditionLearningOpp_Add() DbEntityValidationException, ConnectionProfile: {0}, learningOppId: {1}, createdById: {2}", connectionProfileId, learningOppId, userId );
		//			messages.Add( "Error - missing fields." );
		//			foreach ( var eve in dbex.EntityValidationErrors )
		//			{
		//				message += string.Format( "\rEntity of type \"{0}\" in state \"{1}\" has the following validation errors:",
		//					eve.Entry.Entity.GetType().Name, eve.Entry.State );
		//				foreach ( var ve in eve.ValidationErrors )
		//				{
		//					message += string.Format( "- Property: \"{0}\", Error: \"{1}\"",
		//						ve.PropertyName, ve.ErrorMessage );
		//				}

		//				LoggingHelper.LogError( message, true );
		//			}
		//		}
		//		catch ( Exception ex )
		//		{
		//			LoggingHelper.LogError( ex, thisClassName + string.Format( ".ConditionLearningOpp_Add(), ConnectionProfile: {0}, learningOppId: {1}, createdById: {2}", connectionProfileId, learningOppId, userId ) );
		//		}


		//	}
		//	return id;
		//}

		//public bool ConditionLearningOpp_Delete( int connectionProfileId, int learningOppId, ref string statusMessage )
		//{
		//	bool isValid = false;
		//	if ( connectionProfileId == 0 )
		//	{
		//		statusMessage = "Error - missing an identifier for the connection profile to remove the LearningOpp.";
		//		return false;
		//	}
		//	if ( learningOppId == 0 )
		//	{
		//		statusMessage = "Error - missing an identifier for the LearningOpp to remove";
		//		return false;
		//	}
		//	//need to get Entity.Id 
		//	//using ConnectionProfile using connectionProfileId, 
		//	//		then Entity using ConnectionProfile.RowId
		//	Entity entity = ConnectionProfileManager.ConditionProfile_Get( connectionProfileId, false, false );
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		EM.Entity_LearningOpportunity efEntity = context.Entity_LearningOpportunity
		//					.SingleOrDefault( s => s. == connectionProfileId && s.learningOppId == learningOppId );

		//		if ( efEntity != null && efEntity.Id > 0 )
		//		{
		//			context.Entity_LearningOpportunity.Remove( efEntity );
		//			int count = context.SaveChanges();
		//			if ( count > 0 )
		//			{
		//				isValid = true;
		//			}
		//		}
		//		else
		//		{
		//			statusMessage = "Error - delete failed, as record was not found.";
		//		}
		//	}

		//	return isValid;
		//}

		//public bool ConditionLearningOpp_Delete( int Id, ref string statusMessage )
		//{
		//	bool isValid = false;
		//	if ( Id == 0 )
		//	{
		//		statusMessage = "Error - missing an identifier for the LearningOpp to remove";
		//		return false;
		//	}
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		//EM.Entity_LearningOpportunity efEntity = context.Entity_LearningOpportunity
		//		//			.SingleOrDefault( s => s.Id == Id );

		//		//if ( efEntity != null && efEntity.Id > 0 )
		//		//{
		//		//	context.Entity_LearningOpportunity.Remove( efEntity );
		//		//	int count = context.SaveChanges();
		//		//	if ( count > 0 )
		//		//	{
		//		//		isValid = true;
		//		//	}
		//		//}
		//		//else
		//		//{
		//		//	statusMessage = "Error - delete failed, as record was not found.";
		//		//}
		//	}

		//	return isValid;
		//}

		#endregion
	}
}
