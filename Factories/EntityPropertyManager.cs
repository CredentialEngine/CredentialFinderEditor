using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;

using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBentity = Data.Entity_Property;
using ThisEntity = Models.Common.Enumeration;

using Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class EntityPropertyManager : BaseFactory
	{
		string thisClassName = "EntityPropertyManager";
		#region Entity property persistance ===================


		/// <summary>
		/// Update Entity properies
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="parentUid"></param>
		/// <param name="parentTypeId"></param>
		/// <param name="categoryId">This could be part of the entity, just need to confirm</param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool UpdateProperties( Enumeration entity, Guid parentUid, int parentTypeId, int categoryId, int userId, ref List<string> messages )
		{
			bool isAllValid = true;
			int updatedCount = 0;
			int count = 0;

			if ( !IsGuidValid(parentUid) )
			{
				messages.Add("A valid identifier was not provided to the Update method.");
				return false;
			}
			if ( entity == null )
			{
				entity = new Enumeration();
				return true;
			}
			//get parent entity
			Entity_Summary parent = EntityManager.GetDBEntity( parentUid );
			if ( parent == null || parent.Id == 0 )
			{
				messages.Add( "Error - the parent entity was not found." );
				return false;
			}

			bool hasOtherValue = false;
			if ( !string.IsNullOrWhiteSpace( entity.OtherValue ) )
				hasOtherValue = true;

			//for an update, we need to check for deleted Items, or just delete all and re-add
			//==> then interface would have to always return everything
			using ( var context = new Data.CTIEntities() )
			{
				DBentity op = new DBentity();

				//get all existing for the category
				var results = context.Entity_Property
							.Where( s => s.ParentUid == parentUid
								&& s.Codes_PropertyValue.CategoryId == categoryId )
							.OrderBy( s => s.PropertyValueId )
							.ToList();

				#region deletes check
				var deleteList = from existing in results
								 join item in entity.Items
										 on existing.PropertyValueId equals item.Id
										 into joinTable
								 from result in joinTable.DefaultIfEmpty( new EnumeratedItem { SchemaName = "missing", Id = 0 } )
								 select new { DeleteId = existing.Id, ItemId = ( result.Id ) };

				foreach ( var v in deleteList )
				{
					if ( v.ItemId == 0 )
					{
						//delete item
						PropertyRemove( context, v.DeleteId, ref messages );
					}
				}
				#endregion

				#region new items
				//should only empty ids, where not in current list, so should be adds
				var newList = from item in entity.Items
							  join existing in results
									on item.Id equals existing.PropertyValueId
									into joinTable
							  from addList in joinTable.DefaultIfEmpty( new DBentity { Id = 0, PropertyValueId = 0 } )
							  select new { AddId = item.Id, ExistingId = addList.PropertyValueId };

				foreach ( var v in newList )
				{
					if ( v.ExistingId == 0 && v.AddId > 0)
					{
						op = new DBentity();
						//TODO switch to all EntityId
						op.EntityId = parent.Id;

						op.ParentUid = parentUid;
						op.ParentTypeId = parentTypeId;
						op.PropertyValueId = v.AddId;

						op.Created = System.DateTime.Now;
						op.CreatedById = userId;

						context.Entity_Property.Add( op );
						count = context.SaveChanges();
						if ( count == 0 )
						{
							messages.Add( string.Format(" Unable to add property value Id of: {0} <br\\> ", v.AddId ));
							isAllValid = false;
						}
						else
							updatedCount++;
					}
					else
					{
						//may need to check that schema of other has a value, or if not other, the value is ignored. Actually only for a dropdown. For multi select, need to record whether other was checked anywhere (but prob has to be the last entry!)
					}
				}
				#endregion
			}

			

			//check for other checked && entity.ShowOtherValue == true
			if ( (entity.InterfaceType == EnumerationType.SINGLE_SELECT
				|| ((entity.SchemaName ?? "").ToLower() == "credentialtype" && parentTypeId  == 1))
				&& entity.Items != null && entity.Items.Count > 0)
			{
				EnumeratedItem item = entity.Items[ entity.Items.Count - 1 ];
				if ( hasOtherValue && item.SchemaName.ToLower().IndexOf( "other" ) == -1 )
				{
					//have value, but other not check - ignore with message?
					isAllValid = false;
					messages.Add("Note: You did not select a value of Other and provided an 'Other Value'.  The 'Other Value' was ignored.");
					//actually wipe it out and continue
					entity.OtherValue = "";
					//return false;
				}
			}

			if ( !PropertyOther_Update( parentUid, categoryId, entity, userId, ref messages ) )
				isAllValid = false;

			return isAllValid;
		}


		private void SendNewOtherIdentityNotice( ThisEntity entity, string value, int userId )
		{
			//string message = string.Format( "New Other Value. <ul><li>OrganizationId: {0}</li><li>PersonId: {1}</li><li>Framework: {2}</li><li>URL: {3}</li><li>Value: {4}</li></ul>", entity.OrganizationId, userId, name, url, entity.OtherValue );

			//Utilities.EmailManager.NotifyAdmin( "New Other Value has been created", message );
		}

		public bool PropertyDelete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				//delete item
				DBentity p = context.Entity_Property.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_Property.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Property record was not found: {0}", recordId );
					isOK = false;
				}
			}

			return isOK;
		}
		/// <summary>
		/// Delete all records for a parent (typically due to delete of parent)
		/// </summary>
		/// <param name="parentUid"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool Property_DeleteAll( Guid parentUid, ref string statusMessage )
		{
			bool isValid = false;

			using ( var context = new Data.CTIEntities() )
			{
				if ( parentUid.ToString().IndexOf( "0000" ) == 0 )
				{
					statusMessage = "Error - missing an identifier for the Parent Entity";
					return false;
				}

				context.Entity_Property.RemoveRange( context.Entity_Property.Where( s => s.ParentUid == parentUid ) );
				int count = context.SaveChanges();
				if ( count > 0 )
				{
					isValid = true;
				}


			}

			return isValid;
		}
		private bool PropertyRemove( Data.CTIEntities context, int recordId, ref List<string> messages )
		{
			bool isOK = true;
			//delete item
			DBentity p = context.Entity_Property.FirstOrDefault( s => s.Id == recordId );
			if ( p != null && p.Id > 0 )
			{
				context.Entity_Property.Remove( p );
				int count = context.SaveChanges();
			}
			else
			{
				messages.Add(string.Format( "Property record was not found: {0}", recordId ));
				isOK = false;
			}

			return isOK;

		}
		#endregion
		#region Entity property read ===================
		public static Enumeration FillEnumeration( Guid parentUid, int categoryId )
		{
			Enumeration entity = new ThisEntity();
			entity = CodesManager.GetEnumeration( categoryId );

			entity.Items = new List<EnumeratedItem>();
			EnumeratedItem item = new EnumeratedItem();

			using ( var context = new ViewContext() )
			{
				List<EntityProperty_Summary> results = context.EntityProperty_Summary
					.Where( s => s.ParentUid == parentUid 
						&& s.CategoryId == categoryId )
					.OrderBy( s => s.SortOrder ).ThenBy( s => s.Property )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( EntityProperty_Summary prop in results )
					{

						item = new EnumeratedItem();
						item.Id = prop.PropertyValueId;
						item.Value = prop.PropertyValueId.ToString();
						item.Selected = true;

						item.Name = prop.Property;
						item.SchemaName = prop.PropertySchemaName;
						entity.Items.Add( item );

					}
				}
				entity.OtherValue = EntityOtherProperty_Get( parentUid, categoryId );
				return entity;
			}
		}
		#endregion

		#region ProperyOther 
		
		public static string EntityOtherProperty_Get( Guid parentUid, int categoryId )
		{
			string other = "";
			using ( var context = new ViewContext() )
			{
				Entity_PropertyOtherSummary item = context.Entity_PropertyOtherSummary
					.SingleOrDefault( s => s.EntityUid == parentUid && s.CategoryId == categoryId );

				if ( item != null && item.Id > 0 )
				{
					return item.OtherValue;
				}
			}
			return other;
		}

		private static string EntityOtherProperty_Get( int entityId, int categoryId )
		{
			string other = "";
			using ( var context = new Data.CTIEntities() )
			{
				EM.Entity_PropertyOther item = context.Entity_PropertyOther
					.SingleOrDefault( s => s.EntityId == entityId && s.CategoryId == categoryId );

				if ( item != null && item.Id > 0 )
				{
					return item.OtherValue;
				}
			}
			return other;
		}
		/// <summary>
		/// Get all other values for an entity
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public static List<EntityPropertyOther> EntityOtherProperty_GetAll( int entityId )
		{

			List<EntityPropertyOther> list = new List<EntityPropertyOther>();
			EntityPropertyOther other = new EntityPropertyOther();
			using ( var context = new ViewContext() )
			{

				List<Entity_PropertyOtherSummary> results = context.Entity_PropertyOtherSummary
					.Where( s => s.EntityId == entityId  )
					.OrderBy( s => s.CategoryId)
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Entity_PropertyOtherSummary item in results )
					{
						other = new EntityPropertyOther();
						other.Id = item.Id;
						other.CategoryId = item.CategoryId;
						other.OtherValue = item.OtherValue;
						list.Add( other );
					}
				}
			}
			return list;
		}
		public static List<EntityPropertyOther> EntityOtherProperty_GetAll( Guid entityUid )
		{

			List<EntityPropertyOther> list = new List<EntityPropertyOther>();
			EntityPropertyOther other = new EntityPropertyOther();
			using ( var context = new ViewContext() )
			{

				List<Entity_PropertyOtherSummary> results = context.Entity_PropertyOtherSummary
					.Where( s => s.EntityUid == entityUid )
					.OrderBy( s => s.CategoryId )
					.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Entity_PropertyOtherSummary item in results )
					{
						other = new EntityPropertyOther();
						other.Id = item.Id;
						other.CategoryId = item.CategoryId;
						other.OtherValue = item.OtherValue;
						list.Add( other );
					}
				}
			}
			return list;
		}

		private bool PropertyOther_Update( Guid parentUid, int categoryId, Enumeration entity, int lastUpdatedById, ref List<string> messages )
		{
			bool isValid = true;
			bool hasOtherValue = false;
			int count = 0;
			if ( string.IsNullOrWhiteSpace( entity.OtherValue ) == false )
				hasOtherValue = true;
			int entityId = EntityManager.GetEntityId( parentUid );
			if ( entityId == 0 )
			{
				messages.Add( "The parent entity was not found - system administration has been notified." );
				EmailManager.NotifyAdmin( thisClassName + ".PropertyOther_Update", string.Format("The parent entity was not found - user: {0}, category: {1}, parentUid: {2}",lastUpdatedById) );
				return false;
			}
			using ( var context = new Data.CTIEntities() )
			{
				//how to determine if item deleted?
				//may have to always do a read@@@@
				EM.Entity_PropertyOther item = context.Entity_PropertyOther
					.SingleOrDefault( s => s.EntityId == entityId && s.CategoryId == categoryId );

				if ( item == null || item.EntityId == 0 )
				{
					if ( hasOtherValue == false )
						return true; //not found, and no value, no action
					//else add new
					item = new EM.Entity_PropertyOther();
					item.EntityId = entityId;
					item.CategoryId = categoryId;
					item.OtherValue = entity.OtherValue;
					item.Created = System.DateTime.Now;
					item.CreatedById = lastUpdatedById;

					context.Entity_PropertyOther.Add( item );

					// submit the change to database
					count = context.SaveChanges();

					//not sure if we want to handle other values here
					//would lead to a new entity.PropertyOtherValue table
					if ( hasOtherValue )
					{

						SendNewOtherIdentityNotice( entity, entity.OtherValue, lastUpdatedById );
					}
				}
				else if ( hasOtherValue )
				{
					//update if has changed
					if ( item.OtherValue.ToLower() != entity.OtherValue.ToLower() )
					{
						item.OtherValue = entity.OtherValue;
						count = context.SaveChanges();
						if ( count > 0 )
						{
							isValid = true;
							//notify on change?
						}
					}
				}
				else
				{
					//no longer a value, delete
					context.Entity_PropertyOther.Remove( item );
					count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
			}

			return isValid;
		}
		public bool EntityOtherProperty_Delete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				EM.Entity_PropertyOther p = context.Entity_PropertyOther.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					context.Entity_PropertyOther.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Entity_PropertyOther record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}
		
		#endregion

	}
}