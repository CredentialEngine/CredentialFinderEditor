using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CTIServices;
using Utilities;
using Models.Common;
using Models.Search;

namespace CTI.Directory.Controllers
{
  public class AjaxController : Controller
  {

		/// <summary>
		/// Search GeoNames.org for a location and return similar results
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		[ AcceptVerbs ( HttpVerbs.Get | HttpVerbs.Post ) ]
		public JsonResult GeoNamesSearch( string text )
		{
			var result = new ThirdPartyApiServices().GeoNamesSearch( text );
			return JsonHelper.GetJsonWithWrapper( result );
		}
		//

		/// <summary>
		/// Take a MicroSearchInput and return the results
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult MicroSearch( MicroSearchInput data )
		{
			var valid = true;
			var status = "";
			var result = new MicroSearchServices().MicroSearch( data, ref valid, ref status );

			return JsonHelper.GetJsonWithWrapper( result, valid, status, null );
		
		}
		//

		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult SelectMicroSearchResult( MicroSearchSelection data )
		{
			var valid = true;
			var status = "";
			var result = new MicroSearchServices().SelectResult( data, ref valid, ref status );

			return JsonHelper.GetJsonWithWrapper( result, valid, status, null );

		}
		//

		[AcceptVerbs( HttpVerbs.Post )]
		public JsonResult DeleteMicroSearchResult( MicroSearchSelection data )
		{
			var valid = true;
			var status = "";
			new MicroSearchServices().DeleteResult( data, ref valid, ref status );

			return JsonHelper.GetJsonWithWrapper( null, valid, status, null );

		}
		//

	}
}