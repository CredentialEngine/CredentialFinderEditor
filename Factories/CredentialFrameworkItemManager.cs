using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Common;
using Models.ProfileModels;
using Data;
using Utilities;
using Data.Views;
using ViewContext = Data.Views.CTIEntities1;

namespace Factories
{
	public class CredentialFrameworkItemManager
	{
		static string thisClassName = "CredentialFrameworkItemManager";
			
		/// <summary>
		/// Add a credential framework Item
		/// </summary>
		/// <param name="credentialId"></param>
		/// <param name="categoryId"></param>
		/// <param name="codeID"></param>
		/// <param name="userId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		//public int ItemAdd( int credentialId, int categoryId, int codeID, int userId, ref string statusMessage )
		//{

		//	Credential_FrameworkItem efEntity = new Credential_FrameworkItem();
		//	using ( var context = new Data.CTIEntities() )
		//	{
		//		try
		//		{
		//			//first ensure not a duplicate (until interface/search prevents dups)
		//			EnumeratedItem entity = ItemGet( credentialId, categoryId, codeID );
		//			if ( entity != null && entity.Id > 0 )
		//			{
		//				statusMessage = "Error: the selected code already exists!";
		//				return 0;
		//			}
		//			efEntity.CredentialId = credentialId;
		//			efEntity.CategoryId = categoryId;
		//			efEntity.CodeId = codeID;
		//			efEntity.CreatedById = userId;
					
		//			efEntity.Created = System.DateTime.Now;

		//			context.Credential_FrameworkItem.Add( efEntity );

		//			// submit the change to database
		//			int count = context.SaveChanges();
		//			if ( count > 0 )
		//			{
		//				statusMessage = "successful";
		//				//TODO could get the entity for use in activity logging, etc?
		//				return efEntity.Id;
		//			}
		//			else
		//			{
		//				//?no info on error
		//				statusMessage = "Error - the add was not successful. ";
		//				string message = string.Format( thisClassName + ".ItemAdd Failed", "Attempted to add a credential framework item. The process appeared to not work, but was not an exception, so we have no message, or no clue. CredentialId: {0}, createdById: {1}, CategoryId: {2}, CodeId: {3}", credentialId, userId, categoryId, codeID );
		//				EmailManager.NotifyAdmin( thisClassName + ".ItemAdd Failed", message );
		//			}
		//		}
		//		catch ( Exception ex )
		//		{
		//			LoggingHelper.LogError( ex, thisClassName + string.Format( ".ItemAdd(), CredentialId: {0}, createdById: {1}, CategoryId: {2}, CodeId: {3}", credentialId, userId, categoryId, codeID ) );
		//			statusMessage = ex.Message + ( ex.InnerException != null && ex.InnerException.InnerException != null ? " - " + ex.InnerException.InnerException.Message : "" );
		//		}
		//	}

		//	return efEntity.Id;
		//}

		public bool ItemDelete( int recordId, ref string statusMessage )
		{
			bool isOK = true;
			using ( var context = new Data.CTIEntities() )
			{
				Credential_FrameworkItem p = context.Credential_FrameworkItem.FirstOrDefault( s => s.Id == recordId );
				if ( p != null && p.Id > 0 )
				{
					statusMessage = string.Format( "CredentialFrameworkItem. Deleting codeID: {0} of categoryID: {1} from CredentialID: {2}", p.CodeId, p.CategoryId, p.CredentialId );
					context.Credential_FrameworkItem.Remove( p );
					int count = context.SaveChanges();
				}
				else
				{
					statusMessage = string.Format( "Credential_FrameworkItem record was not found: {0}", recordId );
					isOK = false;
				}
			}
			return isOK;

		}

		/// <summary>
		/// Get a generic version of a framework item
		/// </summary>
		/// <param name="recordId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		public static EnumeratedItem ItemGet( int recordId )
		{
			EnumeratedItem item = new EnumeratedItem();
			using ( var context = new ViewContext() )
			{
				Credential_FrameworkItemSummary entity = context.Credential_FrameworkItemSummary.FirstOrDefault( s => s.Id == recordId );
				if ( entity != null && entity.Id > 0 )
				{
					item.Id = entity.Id;
					item.ParentId = entity.CredentialId;
					item.CodeId = entity.CodeId;
					item.URL = entity.URL;
					item.Value = entity.FrameworkCode;
					item.Name = entity.Title;
					item.Description = entity.Description;
					item.SchemaName = entity.SchemaName;

					item.ItemSummary = entity.FrameworkCode + " - " + item.Name;
				}
				else
				{
					item.Name = "Not Found";
				}
				
			}
			return item;

		}
		//

		/// <summary>
		/// Get a list of framework items generically
		/// </summary>
		/// <param name="recordIds"></param>
		/// <returns></returns>
		public static List<EnumeratedItem> ItemsGet( List<int> recordIds )
		{
			var items = new List<EnumeratedItem>();
			using ( var context = new ViewContext() )
			{
				List<Credential_FrameworkItemSummary> entities = context.Credential_FrameworkItemSummary.Where( s => recordIds.Contains( s.Id ) ).ToList();
				foreach ( var entity in entities )
				{
					items.Add( new EnumeratedItem()
					{
						Id = entity.Id,
						ParentId = entity.CredentialId,
						CodeId = entity.CodeId,
						URL = entity.URL,
						Value = entity.FrameworkCode,
						Name = entity.Title,
						Description = entity.Description,
						SchemaName = entity.SchemaName,
						ItemSummary = entity.FrameworkCode + " - " + entity.Title
					} );
				}
			}
			return items;
		}
		//

		public static EnumeratedItem ItemGet( int credentialId, int categoryId, int codeID )
		{
			EnumeratedItem item = new EnumeratedItem();
			using ( var context = new ViewContext() )
			{
				Credential_FrameworkItemSummary entity = context.Credential_FrameworkItemSummary.FirstOrDefault( s => s.CredentialId == credentialId && s.CategoryId == categoryId && s.CodeId == codeID );
				if ( entity != null && entity.Id > 0 )
				{
					item.Id = entity.Id;
					item.ParentId = entity.CredentialId;
					item.CodeId = entity.CodeId;
					item.URL = entity.URL;
					item.Value = entity.FrameworkCode;
					item.Name = entity.Title;
					item.Description = entity.Description;
					item.SchemaName = entity.SchemaName;

					item.ItemSummary = entity.FrameworkCode + " - " + item.Name;
				}
				else
				{
					item.Name = "Not Found";
				}

			}
			return item;

		}

		public static void Item_FillOccupations( Models.Common.Credential to )
		{
			to.Occupation = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_SOC );

			to.Occupation.ParentId = to.Id;


			to.Occupation.Items = new List<EnumeratedItem>();
			EnumeratedItem item = new EnumeratedItem();

			using ( var context = new ViewContext() )
			{
				List<Credential_OccupationCodesSummary> results = context.Credential_OccupationCodesSummary
					.Where( s => s.CredentialId == to.Id && s.CategoryId == CodesManager.PROPERTY_CATEGORY_SOC )
									.OrderBy( s => s.OnetSocCode )
									.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Credential_OccupationCodesSummary entity in results )
					{
						item = new EnumeratedItem();
						item.Id = entity.Id;
						item.ParentId = entity.CredentialId;
						item.CodeId = entity.CodeId;
						item.URL = entity.URL;
						item.Value = entity.OnetSocCode;
						item.Name = entity.Title;
						item.Description = entity.Description;
						item.SchemaName = entity.SchemaName;

						item.Selected = true;

						item.ItemSummary = item.Name + " (" + entity.OnetSocCode + ")";
						to.Occupation.Items.Add( item );

					}
				}

				//Other parts
			}

		}

		public static void Item_FillNaics( Models.Common.Credential to )
		{
			to.Industry = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_NAICS );

			to.Industry.ParentId = to.Id;
			to.Industry.Items = new List<EnumeratedItem>();
			EnumeratedItem item = new EnumeratedItem();

			using ( var context = new ViewContext() )
			{
				List<Credential_NAICSCodesSummary> results = context.Credential_NAICSCodesSummary
					.Where( s => s.CredentialId == to.Id && s.CategoryId == CodesManager.PROPERTY_CATEGORY_NAICS )
									.OrderBy( s => s.NaicsGroup )
									.ThenBy(s => s.Title)
									.ToList();

				if ( results != null && results.Count > 0 )
				{
					foreach ( Credential_NAICSCodesSummary entity in results )
					{
						item = new EnumeratedItem();
						//entity.Id=Credential.FrameworkItem
						item.Id = entity.Id;
						item.ParentId = entity.CredentialId;
						//CodeId = record from Naics
						item.CodeId = entity.CodeId;
						item.URL = entity.URL;
						item.Value = entity.NaicsCode;
						item.Name = entity.Title;
						item.Description = entity.Description;
						item.SchemaName = entity.SchemaName;

						item.Selected = true;

						item.ItemSummary = item.Name + " (" + entity.NaicsCode + ")";
						to.Industry.Items.Add( item );

					}
				}

				//Other parts
			}

		}

	}
}
