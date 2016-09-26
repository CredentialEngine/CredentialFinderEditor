using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Utilities;
using Models.Helpers;
using CTIServices;

namespace CTI.Directory.Controllers
{
	public class CompareController : Controller
	{
		string sessionKey = "compare";

		//Load Compare page
		public ActionResult Index()
		{
			return V2();
		}
		//

		public ActionResult V2()
		{
			var vm = new CompareItemSummary();

			var lists = GetSessionItems();
			foreach ( var item in lists )
			{
				switch ( item.Type )
				{
					case "credential":
						vm.Credentials.Add( CredentialServices.GetCredential( item.Id, true ) );
						break;
					case "organization":
						vm.Organizations.Add( OrganizationServices.GetOrganizationDetail( item.Id ) );
						break;
					default:
						break;
				}
			}

			return View( "~/Views/V2/Compare/Index.cshtml", vm );
		}

		//Store a compare item
		public JsonResult AddItem( CompareItem input )
		{
			var items = GetSessionItems();
			var existing = items.FirstOrDefault( m => m.Id == input.Id && m.Type == input.Type );
			if ( existing == null )
			{
				//Don't allow too many items to be compared
				if ( items.Count() >= 10 )
				{
					return JsonHelper.GetJsonWithWrapper( null, false, "You can only compare up to 10 items. Please remove one or more items and try again.", null );
				}

				//Add the item
				items.Add( new CompareItem()
				{
					Id = input.Id,
					Type = input.Type.ToLower(),
					Title = input.Title
				} );

				UpdateSessionItems( items );

				return JsonHelper.GetJsonWithWrapper( items );
			}
			else
			{
				return JsonHelper.GetJsonWithWrapper( null, false, "That item is already in the list of items to compare!", null );
			}
		}
		//

		//Remove a compare item
		public JsonResult RemoveItem( CompareItem input )
		{
			var items = GetSessionItems();
			var existing = items.FirstOrDefault( m => m.Id == input.Id && m.Type == input.Type );
			if ( existing != null )
			{
				items.Remove( existing );
				UpdateSessionItems( items );
				return JsonHelper.GetJsonWithWrapper( items );
			}
			else
			{
				return JsonHelper.GetJsonWithWrapper( null, false, "Item not found!", null );
			}
		}
		//

		//Get compare items
		public JsonResult GetItems()
		{
			return JsonHelper.GetJsonWithWrapper( GetSessionItems() );
		}
		//

		//Dump all current compare items
		public void DumpItems( string type, int id )
		{
			UpdateSessionItems( new List<CompareItem>() );
		}
		//

		//Update session items
		private void UpdateSessionItems( List<CompareItem> items )
		{
			try
			{
				new HttpSessionStateWrapper( System.Web.HttpContext.Current.Session ).Contents[ sessionKey ] = items;
			}
			catch
			{
				//
			}
		}
		//

		//Get session items
		private List<CompareItem> GetSessionItems()
		{
			try
			{
				var items = new HttpSessionStateWrapper( System.Web.HttpContext.Current.Session ).Contents[ sessionKey ] as List<CompareItem>;
				if ( items == null )
				{
					return new List<CompareItem>();
				}
				else
				{
					return items;
				}
			}
			catch
			{
				return new List<CompareItem>();
			}
		}
		//

	}
}