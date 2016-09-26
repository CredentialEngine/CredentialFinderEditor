using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Common;
using EM = Data;
using Utilities;

namespace Factories
{
	/// <summary>
	/// Credential property manager
	/// ==> OBSOLETE - REPLACED BY ENTITY.PROPERTY
	/// </summary>
	[Obsolete]
	public class CredentialPropertyManager : BaseFactory
	{
	
		#region Credential property ===================
		//public bool UpdateProperties( Credential entity, ref List<string> messages )
		//{
		//	bool isAllValid = true;
		//	int count= 0;
		//	int updatedCount= 0;

		//	if ( entity.Id == 0 )
		//	{
		//		messages.Add("A valid credential identifier was not provided to the Credential_UpdateProperties method.");
		//		return false;
		//	}

		//	//For efficiency, just roll all properties together and then process

		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		EM.Credential_Property op = new EM.Credential_Property();

		//		//get all existing
		//		//List<EM.Credential_Property> results = context.Credential_Property
		//		var results = context.Credential_Property
		//					.Where( s => s.CredentialId == entity.Id )
		//					.OrderBy( s => s.PropertyValueId )
		//					.ToList();

		//		List<EntityProperty> properties = new List<EntityProperty>();
		//		EntityProperty prop = new EntityProperty();
		//		if ( entity.Purpose != null && entity.Purpose.Items.Count > 0 )
		//		{
		//			foreach ( EnumeratedItem item in entity.Purpose.Items )
		//			{
		//				prop = new EntityProperty();
		//				prop.ParentId = entity.Id;
		//				prop.PropertyValueId = item.Id;
		//				properties.Add( prop );
		//			}
		//		}
		//		if ( entity.CredentialType != null && entity.CredentialType.Items.Count > 0 )
		//		{
		//			foreach ( EnumeratedItem item in entity.CredentialType.Items )
		//			{
		//				prop = new EntityProperty();
		//				prop.ParentId = entity.Id;
		//				prop.PropertyValueId = item.Id;
		//				properties.Add( prop );
		//			}
		//		}
		//		if ( entity.CredentialLevel != null && entity.CredentialLevel.Items.Count > 0 )
		//		{
		//			foreach ( EnumeratedItem item in entity.CredentialLevel.Items )
		//			{
		//				prop = new EntityProperty();
		//				prop.ParentId = entity.Id;
		//				prop.PropertyValueId = item.Id;
		//				properties.Add( prop );
		//			}
		//		}

		//		#region == deletes
		//		//should only existing ids, where not in current list, so should be deletes
		//		var deleteList = from existing in results
		//						 join item in properties
		//								 on existing.PropertyValueId equals item.PropertyValueId
		//								 into joinTable
		//						 from result in joinTable.DefaultIfEmpty( new EntityProperty { ParentId = 0, PropertyValueId = 0 } )
		//						 select new { DeleteId = existing.PropertyValueId, ParentId = ( result.ParentId ) };

		//		foreach ( var v in deleteList )
		//		{
		//			//Console.WriteLine( "existing: {0} input: {1}, delete? {2}", v.DeleteId, v.ItemId, v.ItemId == 0 );
		//			if ( v.ParentId == 0 )
		//			{
		//				//delete item
		//				EM.Credential_Property p = context.Credential_Property.FirstOrDefault( s => s.CredentialId == entity.Id && s.PropertyValueId == v.DeleteId );
		//				if ( p != null && p.Id > 0 )
		//				{
		//					context.Credential_Property.Remove( p );
		//					count = context.SaveChanges();
		//				}
		//			}
		//		}
		//		#endregion 
		//		#region adds 
		//		//should only show entry ids, where not in current list, so should be adds
		//		var newList = from item in properties
		//					  join existing in results
		//							on item.PropertyValueId equals existing.PropertyValueId
		//							into joinTable
		//					  from addList in joinTable.DefaultIfEmpty( new EM.Credential_Property { Id = 0, PropertyValueId = 0 } )
		//					  select new { AddId = item.PropertyValueId, ExistingId = addList.PropertyValueId };
		//		foreach ( var v in newList )
		//		{
		//			//Console.WriteLine( "input: {0} existing: {1}, Add? {2}", v.AddId, v.ExistingId, v.ExistingId == 0 );
		//			if ( v.ExistingId == 0 )
		//			{

		//				op = new EM.Credential_Property();
		//				op.CredentialId = entity.Id;
		//				op.PropertyValueId = v.AddId;
		//				op.Created = System.DateTime.Now;
		//				op.CreatedById = entity.LastUpdatedById;

		//				context.Credential_Property.Add( op );
		//				count = context.SaveChanges();
		//				if ( count == 0 )
		//				{
		//					messages.Add(string.Format( " Unable to add property value Id of: {0} <br\\> ", v.AddId ));
		//					isAllValid = false;
		//				}
		//				else
		//					updatedCount++;
		//			}
		//		}
		//		#endregion

		//		#region PropertyOther
		//		//the other values will have to be handled separately
		//		Credential_PropertyOther_Update( entity.Id, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE, entity.CredentialType, entity.LastUpdatedById, ref messages );

		//		Credential_PropertyOther_Update( entity.Id, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_PURPOSE, entity.Purpose, entity.LastUpdatedById, ref messages );

		//		Credential_PropertyOther_Update( entity.Id, CodesManager.PROPERTY_CATEGORY_CREDENTIAL_LEVEL, entity.CredentialLevel, entity.LastUpdatedById, ref messages );
		//		#endregion
		//	}
		//	return isAllValid;
		//}

		///// <summary>
		///// check if other property exists. Then determine actions
		///// </summary>
		///// <param name="entity"></param>
		///// <param name="statusMessage"></param>
		///// <returns></returns>
		//private bool Credential_PropertyOther_Update( int credId, int categoryId, Enumeration entity, int lastUpdatedById, ref List<string> messages )
		//{
		//	bool isValid = false;
		//	bool hasOtherValue = false;
		//	int count = 0;
		//	if ( string.IsNullOrWhiteSpace( entity.OtherValue ) == false )
		//		hasOtherValue = true;

		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		//how to determine if item deleted?
		//		//may have to always do a read@@@@
		//		EM.Credential_PropertyOther opo = context.Credential_PropertyOther.SingleOrDefault( s => s.CredentialId == credId && s.CategoryId == categoryId );

		//		if ( opo == null || opo.CredentialId == 0 )
		//		{
		//			if ( hasOtherValue == false )
		//				return true; //not found, and no value, no action
		//			//else add new
		//			opo = new EM.Credential_PropertyOther();
		//			opo.CredentialId = credId;
		//			opo.CategoryId = categoryId;
		//			opo.OtherValue = entity.OtherValue;
		//			opo.Created = System.DateTime.Now;
		//			opo.CreatedById = lastUpdatedById;

		//			context.Credential_PropertyOther.Add( opo );

		//			// submit the change to database
		//			count = context.SaveChanges();
		//		}
		//		else if ( hasOtherValue )
		//		{
		//			//update if has changed
		//			if ( opo.OtherValue.ToLower() != entity.OtherValue.ToLower() )
		//			{
		//				opo.OtherValue = entity.OtherValue;
		//				count = context.SaveChanges();
		//				if ( count > 0 )
		//				{
		//					isValid = true;
		//				}
		//			}
		//		}
		//		else
		//		{
		//			//no longer a value, delete
		//			context.Credential_PropertyOther.Remove( opo );
		//			count = context.SaveChanges();
		//			if ( count > 0 )
		//			{
		//				isValid = true;
		//			}
		//		}
		//	}

		//	return isValid;
		//}

		//public bool Credential_PropertyDelete( int recordId, ref List<string> messages )
		//{
		//	bool isOK = true;
		//	using ( var context = new Data.CTIEntities() )
		//	{

		//		//delete item
		//		EM.Credential_Property p = context.Credential_Property.FirstOrDefault( s => s.Id == recordId );
		//		if ( p != null && p.Id > 0 )
		//		{
		//			context.Credential_Property.Remove( p );
		//			int count = context.SaveChanges();
		//		}
		//		else
		//		{
		//			messages.Add(string.Format( "Credential_Property record was not found: {0}", recordId ));
		//			isOK = false;
		//		}
		//	}

		//	return isOK;
		//}
		//public bool Credential_PropertyOtherDelete( int recordId, ref List<string> messages )
		//{
		//	bool isOK = true;
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		EM.Credential_PropertyOther p = context.Credential_PropertyOther.FirstOrDefault( s => s.Id == recordId );
		//		if ( p != null && p.Id > 0 )
		//		{
		//			context.Credential_PropertyOther.Remove( p );
		//			int count = context.SaveChanges();
		//		}
		//		else
		//		{
		//			messages.Add(string.Format( "Credential_PropertyOther record was not found: {0}", recordId ));
		//			isOK = false;
		//		}
		//	}
		//	return isOK;

		//}
		
		#endregion

	}
}
