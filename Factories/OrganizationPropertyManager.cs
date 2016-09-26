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
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
namespace Factories
{
	public class OrganizationPropertyManager : BaseFactory
	{


		#region Organization property persistance ===================

		//public bool UpdateProperties( Organization entity, ref List<string> messages )
		//{
		//	bool isAllValid = true;
		//	//isAllValid = OrgTypeProperty_Update( entity, ref messages );

		//	if ( SocialMediaPropery_Update( entity, ref messages ) == false )
		//		isAllValid = false;


		//	if ( IdentifiersPropery_Update( entity, ref messages ) == false )
		//		isAllValid = false;
			
	
		//	return isAllValid;
		//}

		//public bool OrgTypeProperty_Update( Organization entity, ref List<string> messages )
		//{
		//	bool isAllValid = true;
		//	int updatedCount = 0;
		//	int count = 0;

		//	if ( entity.Id == 0 )
		//	{
		//		messages.Add( "A valid organization identifier was not provided to the Organization_UpdateParts method.");
		//		return false;
		//	}
		//	if ( entity.OrganizationType == null )
		//		entity.OrganizationType = new Enumeration();

		//	//need to handle deletes
		//	//else if ( entity.OrganizationType == null )
		//	//{
		//	//	return true;
		//	//}
		//	//for an update, we need to check for deleted Items, or just delete all and readd
		//	//==> then interface would have to always return everything
		//	//TODO - need to add code for handling the separate other data
		//	using ( var context = new ViewContext() )
		//	{
		//		EM.Organization_Property op = new EM.Organization_Property();

		//		//get all existing for org type
		//		var results = context.OrganizationProperty_Summary
		//					.Where( s => s.OrganizationId == entity.Id && s.CategoryId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_TYPE )
		//					.OrderBy( s => s.PropertyValueId )
		//					.ToList();
		//		#region testing
		//		//var results2 = context.Organization_Property
		//		//			.Where( s => s.OrganizationId == entity.Id )
		//		//			.OrderBy( s => s.PropertyValueId )
		//		//			.ToList();

		//		//var deleteList2 = from existing in results2
		//		//				 join item in entity.OrganizationType.Items
		//		//						 on existing.PropertyValueId equals item.Id
		//		//						 into joinTable
		//		//				 from result in joinTable.DefaultIfEmpty( new EnumeratedItem { SchemaName = "missing", Id = 0 } )
		//		//				 select new { DeleteId = existing.PropertyValueId, ItemId = ( result.Id ) };



		//		//var currentTypes = from prop in context.Organization_Property
		//		//			join codes in context.Codes_PropertyValue
		//		//			on prop.PropertyValueId equals codes.Id
		//		//			  where prop.OrganizationId == entity.Id && codes.CategoryId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_TYPE
		//		//			  select prop;

		//		//List<EntityProperty> properties = new List<EntityProperty>();
		//		//EntityProperty ep = new EntityProperty();
		//		//if ( entity.OrganizationType == null )
		//		//	entity.OrganizationType = new Enumeration();

		//		//if ( entity.OrganizationType != null && entity.OrganizationType.Items.Count > 0 )
		//		//{
		//		//	//transform to EntityProperty
		//		//	foreach ( EnumeratedItem item in entity.OrganizationType.Items )
		//		//	{
		//		//		ep = new EntityProperty();
		//		//		ep.ParentId = entity.Id;
		//		//		ep.PropertyValueId = item.Id;
		//		//		properties.Add( ep );
		//		//	}
		//		//}
		//		////should only existing ids, where not in current list, so should be deletes
		//		//var deleteList4a = from existing in results
		//		//				 join item in properties
		//		//					on existing.PropertyValueId equals item.PropertyValueId
		//		//					into joinTable
		//		//				 from result in joinTable.DefaultIfEmpty( new EntityProperty { ParentId = 0, PropertyValueId = 0 } )
		//		//				 select new { DeleteId = existing.PropertyValueId, ParentId = ( result.ParentId ) };
		//		//var deleteList4b = deleteList4a.ToList();
		//		#endregion 
		//		#region deletes/updates check
		//		var deleteList = from existing in results
		//						  join item in entity.OrganizationType.Items
		//								  on existing.PropertyValueId equals item.Id
		//								  into joinTable
		//						  from result in joinTable.DefaultIfEmpty( new EnumeratedItem { SchemaName = "missing", Id = 0 } )
		//						  select new { DeleteId = existing.Id, ItemId = ( result.Id ) };

		//		foreach ( var v in deleteList )
		//		{

		//			if ( v.ItemId == 0 )
		//			{
		//				//delete item
		//				Organization_PropertyRemove( v.DeleteId, ref messages );
		//				//EM.Organization_Property p = context.Organization_Property.FirstOrDefault( s => s.OrganizationId == entity.Id && s.PropertyValueId == v.DeleteId );
		//				//if ( p != null && p.Id > 0 )
		//				//{
		//				//	context.Organization_Property.Remove( p );
		//				//	count = context.SaveChanges();
		//				//}
		//			}
		//		}
		//		#endregion

		//		#region new items
		//		//should only empty ids, where not in current list, so should be adds
		//		var newList = from item in entity.OrganizationType.Items
		//					  join existing in results
		//							on item.Id equals existing.PropertyValueId
		//							into joinTable
		//					  from addList in joinTable.DefaultIfEmpty( new Views.OrganizationProperty_Summary { Id = 0, PropertyValueId = 0 } )
		//					  select new { AddId = item.Id, ExistingId = addList.PropertyValueId };
		//		foreach ( var v in newList )
		//		{
		//			if ( v.ExistingId == 0 )
		//			{
		//				using ( var context2 = new Data.CTIEntities() )
		//				{
		//					op = new EM.Organization_Property();
		//					op.OrganizationId = entity.Id;
		//					op.PropertyValueId = v.AddId;
		//					op.Created = System.DateTime.Now;
		//					op.CreatedById = entity.LastUpdatedById;
		//					op.LastUpdated = System.DateTime.Now;
		//					op.LastUpdatedById = entity.LastUpdatedById;

		//					context2.Organization_Property.Add( op );
		//					count = context.SaveChanges();
		//					if ( count == 0 )
		//					{
		//						messages.Add( string.Format( " Unable to add property value Id of: {0} <br\\> ", v.AddId ) );
		//						isAllValid = false;
		//					}
		//					else
		//						updatedCount++;
		//				}
		//			}
		//		}
		//		#endregion

		//		if ( entity.OrganizationType != null )
		//		{
		//			entity.OrganizationType.ParentId = entity.Id;
		//			//HACK ALERT!!! - only way to handle
		//			if ( entity.OrganizationType.Id == 0 )
		//				entity.OrganizationType.Id = CodesManager.PROPERTY_CATEGORY_ORGANIZATION_TYPE;

		//			Organization_PropertyOther_UpdateOrgType( entity.OrganizationType, entity.LastUpdatedById, ref messages );
		//		}
		//	}


		//	return isAllValid;
		//}

		//public bool IdentifiersPropery_Update( Organization entity, ref List<string> messages )
		//{
		//	bool isAllValid = true;
		//	int updatedCount = 0;
		//	int count = 0;
		
		//	if ( entity.Id == 0 )
		//	{
		//		messages.Add( "A valid organization identifier was not provided to the Organization_UpdateParts method.");
		//		return false;
		//	}
		//	if ( entity.Identifiers == null )
		//		entity.Identifiers = new Enumeration();

		//	//for an update, we need to check for deleted Items, or just delete all and readd
		//	//==> then interface would have to always return everything
		//	using ( var context = new ViewContext() )
		//	{
		//		EM.Organization_Property op = new EM.Organization_Property();

		//		//get all existing for org type
		//		var results = context.OrganizationProperty_Summary
		//					.Where( s => s.OrganizationId == entity.Id && s.CategoryId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS )
		//					.OrderBy( s => s.PropertyValueId )
		//					.ToList();
				
		//		#region deletes/updates check
		//		var deleteList = from existing in results
		//					join item in entity.Identifiers.Items
		//					on existing.Id equals item.Id
		//					into joinTable
		//					from result in joinTable.DefaultIfEmpty( new EnumeratedItem { SchemaName = "missing", Id = 0 } )
		//					select new { DeleteId = existing.Id, 
		//						ItemId = ( result.Id ), 
		//						TextValue = result.Value, 
		//						DBValue = existing.OtherValue,
		//								 CodeId = result.CodeId,
		//								DBPropertyValueId = existing.PropertyValueId
		//					};

		//		//TODO - fix  -will be replace though
		//		using ( var context2 = new Data.CTIEntities() )
		//		{
		//			foreach ( var v in deleteList )
		//			{

		//				if ( v.ItemId == 0 )
		//				{
		//					//delete item
		//					Organization_PropertyRemove( v.DeleteId, ref messages );
		//				}
		//				else
		//				{
		//					//updates

		//					if ( string.IsNullOrWhiteSpace( v.TextValue ) )
		//					{
		//						//if value has been set to blank, delete it
		//						Organization_PropertyRemove( v.DeleteId, ref messages );

		//					}
		//					else if ( v.DBValue != v.TextValue
		//						|| v.DBPropertyValueId != v.CodeId )
		//					{
		//						//update item
		//						EM.Organization_Property p = context2.Organization_Property.FirstOrDefault( s => s.Id == v.ItemId );
		//						if ( p != null && p.Id > 0 )
		//						{
		//							p.OtherValue = v.TextValue;
		//							p.PropertyValueId = v.CodeId;
		//							p.LastUpdated = System.DateTime.Now;
		//							p.LastUpdatedById = entity.LastUpdatedById;
		//							count = context2.SaveChanges();
		//						}


		//					}
		//				}
		//			}//foreach
		//		}
		//		#endregion

		//		#region new items
		//		//should only empty ids, where not in current list, so should be adds
		//		var newList = from item in entity.Identifiers.Items
		//					  join existing in results
		//							on item.Id equals existing.Id
		//							into joinTable
		//					  from addList in joinTable.DefaultIfEmpty( new Views.OrganizationProperty_Summary { Id = 0, PropertyValueId = 0 } )
		//					  select new { PropertyValueId = item.CodeId, TextValue = item.Value, ExistingId = addList.Id, SchemaUrl = item.SchemaUrl, FrameworkName = item.Name };
				
		//		foreach ( var v in newList )
		//		{
		//			if ( v.ExistingId == 0 && v.PropertyValueId > 0)
		//			{
		//				if ( string.IsNullOrWhiteSpace( v.TextValue ) )
		//				{
		//					//in absense of edits, ignore if no code
		//				}
		//				else
		//				{
		//					int id = Organization_PropertyAdd( entity.Id, v.PropertyValueId, v.TextValue,
		//	v.SchemaUrl, v.FrameworkName, entity.LastUpdatedById, ref messages );
		//					//op = new EM.Organization_Property();
		//					//op.OrganizationId = entity.Id;
		//					//op.PropertyValueId = v.PropertyValueId;
		//					//op.OtherValue = v.TextValue;
		//					////check for other
		//					//if ( string.IsNullOrWhiteSpace( v.SchemaUrl ) == false )
		//					//{
		//					//	//will want to create a property value, and send email
		//					//	//could append to text for now
		//					//	op.OtherValue += "{" + ( v.FrameworkName ?? "missing framework name" ) + "; " + v.SchemaUrl + "}";
		//					//	LoggingHelper.DoTrace( 2, "A new organization identifier of other has been added:" + op.OtherValue );
		//					//	SendNewOtherIdentityNotice( op, v.FrameworkName, v.SchemaUrl, v.TextValue, entity.LastUpdatedById );
		//					//}
		//					//op.Created = System.DateTime.Now;
		//					//op.CreatedById = entity.LastUpdatedById;
		//					//op.LastUpdated = System.DateTime.Now;
		//					//op.LastUpdatedById = entity.LastUpdatedById;

		//					//context.Organization_Property.Add( op );
		//					//count = context.SaveChanges();
		//					if ( id == 0 )
		//					{
		//						messages.Add( string.Format( " Unable to add property value Id of: {0} <br\\> ", v.PropertyValueId ));
		//						isAllValid = false;
		//					}
		//					else
		//						updatedCount++;
		//				}
		//			}
		//		}

		//		#endregion

		//	}


		//	return isAllValid;
		//}

		//private void SendNewOtherIdentityNotice(EM.Organization_Property entity, string name, string url, string value, int userId)
		//{
		//	string message = string.Format( "New identity. <ul><li>OrganizationId: {0}</li><li>PersonId: {1}</li><li>Framework: {2}</li><li>URL: {3}</li><li>Value: {4}</li></ul>", entity.OrganizationId, userId, name, url, entity.OtherValue );
		//	Utilities.EmailManager.NotifyAdmin( "New Organization Identity has been created", message );
		//}


//		public bool Organization_PropertyDelete( int recordId, ref List<string> messages )
//		{
//			bool isOK = true;
//			using ( var context = new Data.CTIEntities() )
//			{
				
//				//delete item
//				EM.Organization_Property p = context.Organization_Property.FirstOrDefault( s => s.Id == recordId );
//				if ( p != null && p.Id > 0 )
//				{
//					context.Organization_Property.Remove( p );
//					int count = context.SaveChanges();
//				}
//				else
//				{
//					messages.Add( string.Format( "Organization_Property record was not found: {0}", recordId ));
//					isOK = false;
//				}
//			}

//			return isOK;
//}

//		private int Organization_PropertyAdd( int entityId, int propertyValueId, string textValue, 
//			string schemaUrl, 
//			string frameworkName,
//			int userId,
//			ref List<string> messages )
//		{
//			int id = 0;
//			using ( var context = new Data.CTIEntities() )
//			{
//				EM.Organization_Property op = new EM.Organization_Property();
//				op.OrganizationId = entityId;
//				op.PropertyValueId = propertyValueId;
//				op.OtherValue = textValue;
//				//check for other
//				if ( string.IsNullOrWhiteSpace( schemaUrl ) == false )
//				{
//					//will want to create a property value, and send email
//					//could append to text for now
//					op.OtherValue += "{" + ( frameworkName ?? "missing framework name" ) + "; " + schemaUrl + "}";
//					LoggingHelper.DoTrace( 2, "A new organization identifier of other has been added:" + op.OtherValue );
//					SendNewOtherIdentityNotice( op, frameworkName, schemaUrl, textValue, userId );
//				}
//				op.Created = System.DateTime.Now;
//				op.CreatedById = userId;
//				op.LastUpdated = System.DateTime.Now;
//				op.LastUpdatedById = userId;

//				context.Organization_Property.Add( op );
//				int count = context.SaveChanges();
//				id = op.Id;

//			}

//			return id;
//		}

		//private bool Organization_PropertyRemove( int recordId, ref List<string> messages )
		//{
		//	bool isOK = true;
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		//delete item
		//		EM.Organization_Property p = context.Organization_Property.FirstOrDefault( s => s.Id == recordId );
		//		if ( p != null && p.Id > 0 )
		//		{
		//			context.Organization_Property.Remove( p );
		//			int count = context.SaveChanges();
		//		}
		//		else
		//		{
		//			messages.Add( string.Format( "Organization_Property record was not found: {0}", recordId ) );
		//			isOK = false;
		//		}
		//	}
		//	return isOK;

		//}
		//private bool SocialMediaPropery_Update( Organization entity, ref List<string> messages )
		//{
		//	bool isValid = true;
		//	int count = 0;
		//	int updatedCount = 0;

		//	EM.Organization_PropertyOther opo = new EM.Organization_PropertyOther();
		//	if ( entity.SocialMedia == null )
		//		entity.SocialMedia = new List<TextValueProfile>();
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		//get all existing for social media
		//		var currentTypes = context.Organization_PropertyOther
		//					.Where( s => s.OrganizationId == entity.Id && s.CategoryId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA )
		//					.OrderBy( s => s.OtherValue )
		//					.ToList();

		//		var deleteList = from existing in currentTypes
		//						 join item in entity.SocialMedia
		//								 on existing.Id equals item.Id
		//								 into joinTable
		//						 from result in joinTable.DefaultIfEmpty( new TextValueProfile { CategoryId = 0, Id = 0 } )
		//						 select new { DeleteId = existing.Id, ItemId = ( result.Id ), DBValue = existing.OtherValue, InputValue = result.TextValue };

		//		//handle deletes and updates
		//		foreach ( var v in deleteList )
		//		{
		//			if ( v.ItemId == 0 )
		//			{
		//				//delete item
		//				Organization_PropertyOtherRemove( context, v.DeleteId, ref messages );
		//			}
		//			else
		//			{
		//				//check for update
		//				if ( v.DBValue != v.InputValue )
		//				{
		//					if ( v.InputValue.Trim().Length < 10 )
		//					{
		//						//delete item
		//						Organization_PropertyOtherRemove( context, v.ItemId, ref messages );
		//					}
		//					else
		//					{
		//						//update item
		//						EM.Organization_PropertyOther p = context.Organization_PropertyOther.FirstOrDefault( s => s.Id == v.ItemId );
		//						if ( p != null && p.Id > 0 )
		//						{
		//							p.OtherValue = v.InputValue;
		//							p.LastUpdated = DateTime.Now;
		//							p.LastUpdatedById = entity.LastUpdatedById;
		//							count = context.SaveChanges();
		//						}
		//					}
		//				}
		//			}
		//		}
		//		//should only empty ids, where not in current list, so should be adds
		//		var newList = from item in entity.SocialMedia
		//					  join existing in currentTypes
		//							on item.Id equals existing.Id
		//							into joinTable
		//					  from addList in joinTable.DefaultIfEmpty( new EM.Organization_PropertyOther { Id = 0, CategoryId = 0 } )
		//					  select new { AddId = item.Id, ExistingId = addList.Id, InputValue = item.TextValue };

		//		foreach ( var v in newList )
		//		{
		//			if ( v.ExistingId == 0 && v.InputValue != null && v.InputValue.Length > 10 )
		//			{

		//				opo = new EM.Organization_PropertyOther();
		//				opo.OrganizationId = entity.Id;
		//				opo.CategoryId = CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA;
		//				opo.OtherValue = v.InputValue;
		//				opo.Created = System.DateTime.Now;
		//				opo.CreatedById = entity.LastUpdatedById;
		//				opo.LastUpdated = opo.Created;
		//				opo.LastUpdatedById = opo.CreatedById;

		//				context.Organization_PropertyOther.Add( opo );
		//				count = context.SaveChanges();
		//				if ( count == 0 )
		//				{
		//					messages.Add( string.Format( " Unable to add org social media property value of: {0} <br\\> ", v.InputValue ));
		//					isValid = false;
		//				}
		//				else
		//					updatedCount++;
		//			}
		//		}
			
		//	}

		//	return isValid;
		//}
		/// <summary>
		/// check if other property exists. Then determine actions
		/// </summary>
		/// <param name="entity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public bool PropertyOther_Update( Enumeration entity, int lastUpdatedById, ref string statusMessage )
		{
			bool isValid = true;
			bool hasOtherValue = false;
			int count = 0;
			if ( string.IsNullOrWhiteSpace( entity.OtherValue ) == false )
				hasOtherValue = true;

			using ( var context = new Data.CTIEntities() )
			{
				//only one value allowed for org type
				EM.Organization_PropertyOther opo = context.Organization_PropertyOther.SingleOrDefault( s => s.OrganizationId == entity.ParentId && s.CategoryId == entity.Id );

				if ( opo == null || opo.OrganizationId == 0 )
				{
					if ( hasOtherValue == false )
						return true; //not found, and no value, no action
					//else add new
					opo = new EM.Organization_PropertyOther();
					opo.OrganizationId = entity.ParentId;
					opo.CategoryId = entity.Id; //categoryId
					opo.OtherValue = entity.OtherValue;
					opo.Created = System.DateTime.Now;
					opo.CreatedById = lastUpdatedById;
					opo.LastUpdated = System.DateTime.Now;
					opo.LastUpdatedById = lastUpdatedById;

					context.Organization_PropertyOther.Add( opo );

					// submit the change to database
					count = context.SaveChanges();
				}
				else if ( hasOtherValue )
				{
					//update if has changed
					if ( opo.OtherValue.ToLower() != entity.OtherValue.ToLower() )
					{
						opo.OtherValue = entity.OtherValue;
						count = context.SaveChanges();
						if ( count > 0 )
						{
							isValid = true;
						}
					}
				}
				else
				{
					//no longer a value, delete
					context.Organization_PropertyOther.Remove( opo );
					count = context.SaveChanges();
					if ( count > 0 )
					{
						isValid = true;
					}
				}
			}

			return isValid;
		}
		//public bool Organization_PropertyOtherDelete( int recordId, ref List<string> messages )
		//{
		//	bool isOK = true;
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		EM.Organization_PropertyOther p = context.Organization_PropertyOther.FirstOrDefault( s => s.Id == recordId );
		//		if ( p != null && p.Id > 0 )
		//		{
		//			context.Organization_PropertyOther.Remove( p );
		//			int count = context.SaveChanges();
		//		}
		//		else
		//		{
		//			messages.Add( string.Format( "Organization_PropertyOther record was not found: {0}", recordId ));
		//			isOK = false;
		//		}
		//	}
		//	return isOK;

		//}
		//private bool Organization_PropertyOtherRemove( Data.CTIEntities context, int recordId, ref List<string> messages )
		//{
		//	bool isOK = true;
		//	EM.Organization_PropertyOther p = context.Organization_PropertyOther.FirstOrDefault( s => s.Id == recordId );
		//	if ( p != null && p.Id > 0 )
		//	{
		//		context.Organization_PropertyOther.Remove( p );
		//		int count = context.SaveChanges();
		//	}
		//	else
		//	{
		//		messages.Add( string.Format( "Organization_PropertyOther record was not found: {0}", recordId ) );
		//		isOK = false;
		//	}

		//	return isOK;

		//}
		#endregion


		//public static void OrganizationPropertyFill_ToMap( EM.Organization fromEntity, Organization to )
		//{
			
		//	//properties
		//	FillOrganizationType( fromEntity, to );
		//	//FillOrganizationService( fromEntity, to );

		//	FillSocialMedia( fromEntity, to );
		//	FillOrganizationIdentities( fromEntity, to );
			
		//}
		//public static void FillOrganizationType( EM.Organization fromEntity, Organization to )
		//{
		//	to.OrganizationType = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_ORGANIZATION_TYPE );
		
		//	to.OrganizationType.ParentId = to.Id;

		//	//Need to handle alternate Other now??
		//	if ( fromEntity.Organization_PropertyOther != null && fromEntity.Organization_PropertyOther.Count > 0 )
		//	{
		//		//for now only handle one value
		//		foreach ( EM.Organization_PropertyOther opo in fromEntity.Organization_PropertyOther )
		//		{
		//			if ( opo.CategoryId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_TYPE )
		//			{
		//				to.OrganizationType.OtherValue = opo.OtherValue;
		//				break;
		//			}
		//		}
		//	}

		//	to.OrganizationType.Items = new List<EnumeratedItem>();
		//	EnumeratedItem item = new EnumeratedItem();

		//	foreach ( EM.Organization_Property prop in fromEntity.Organization_Property )
		//	{
		//		if ( prop.Codes_PropertyValue.CategoryId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_TYPE )
		//		{
		//			item = new EnumeratedItem();
		//			item.Id = prop.PropertyValueId;
		//			item.Name = prop.Codes_PropertyValue.Title;
		//			item.Value = prop.PropertyValueId.ToString();

		//			item.Selected = true;
		//			if ( !string.IsNullOrWhiteSpace( prop.OtherValue ) )
		//			{
		//				to.OrganizationType.OtherValue = prop.OtherValue;
		//			}
		//			to.OrganizationType.Items.Add( item );
		//		}

		//	}
		//}

		
		//public static void FillSocialMedia( EM.Organization fromEntity, Organization to )
		//{
		//	//like an enumeration, but handled differently?
		//	//will only use PropertyOther

		//	to.SocialMedia = new List<TextValueProfile>();
		//	TextValueProfile tvp = new TextValueProfile();
		//	if ( fromEntity.Organization_PropertyOther != null && fromEntity.Organization_PropertyOther.Count > 0 )
		//	{
		//		//for now only handle one value
		//		foreach ( EM.Organization_PropertyOther opo in fromEntity.Organization_PropertyOther )
		//		{
		//			if ( opo.CategoryId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA )
		//			{
		//				tvp = new TextValueProfile();
		//				tvp.Id = opo.Id;
		//				tvp.CategoryId = CodesManager.PROPERTY_CATEGORY_ORGANIZATION_SOCIAL_MEDIA;
		//				tvp.TextValue = opo.OtherValue;
		//				tvp.ProfileSummary = opo.OtherValue;

		//				to.SocialMedia.Add( tvp );
		//			}
		//		}
		//	}

		//}

		//public static void FillOrganizationIdentities( EM.Organization fromEntity, Organization to )
		//{
		//	if ( fromEntity.Organization_Property == null )
		//		return;

		//	to.Identifiers = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS );
		//	to.Identifiers.ParentId = to.Id;

		//	//Should have a notify when an other is entered
		//	//not handling other yet
		//	if ( fromEntity.Organization_PropertyOther != null && fromEntity.Organization_PropertyOther.Count > 0 )
		//	{
		//		//for now only handle one value
		//		foreach ( EM.Organization_PropertyOther opo in fromEntity.Organization_PropertyOther )
		//		{
		//			if ( opo.CategoryId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS )
		//			{
		//				LoggingHelper.DoTrace( "@@@@@ found Organization_PropertyOther for identifiers" );
		//				to.Identifiers.OtherValue = opo.OtherValue;
		//				break;
		//			}
		//		}
		//	}

		//	to.Identifiers.Items = new List<EnumeratedItem>();
		//	EnumeratedItem item = new EnumeratedItem();

		//	foreach ( EM.Organization_Property prop in fromEntity.Organization_Property )
		//	{
		//		if ( prop.Codes_PropertyValue.CategoryId == CodesManager.PROPERTY_CATEGORY_ORGANIZATION_IDENTIFIERS )
		//		{
		//			item = new EnumeratedItem();
		//			item.Id = prop.Id;
		//			item.ParentId = prop.OrganizationId;
		//			item.CodeId = prop.PropertyValueId;
		//			item.Name = prop.Codes_PropertyValue.Title;
		//			item.Value = prop.OtherValue;
		//			item.SchemaName = prop.Codes_PropertyValue.SchemaName;
		//			item.SchemaUrl = prop.Codes_PropertyValue.SchemaUrl;
		//			item.ItemSummary = prop.Codes_PropertyValue.Title + " - " + item.Value ?? "";

		//			item.Selected = true;
					
		//			to.Identifiers.Items.Add( item );
		//		}

		//	}
		//}	//
	}
}
