using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CTIServices;
using Models.Helpers.Cass;

namespace CTI.Directory.Controllers
{
    public class CassController : Controller
    {
		//Get the raw data for a framework
		public JsonResult GetFrameworkData( string frameworkURL )
		{
			var data = ThirdPartyApiServices.GetCassObject<CassFramework>( frameworkURL );
			var result = new CassServices.FrameworkModel()
			{
				FrameworkNode = new CassServices.NodeModel()
				{
					Name = data.Name,
					Description = data.Description,
					Url = data.Url,
					NotationCode = ""
				},
				Nodes = data.Competencies.ConvertAll( m => new CassServices.NodeModel() { Name = m.Name, Description = m.Description, Url = m.Url, NotationCode = m.CodedNotation } ),
				Relations = data.Relations.ConvertAll( m => new CassServices.NodeRelation() { SourceId = m.Source, TargetId = m.Target, RelationType = m.RelationType } )
			};
			return Utilities.JsonHelper.GetJsonWithWrapper( result, true, "", null );
		}
		//

		//Save the basic data for a framework
		//Apparently have to delete first, then create (again) to do an update
		public JsonResult SaveFrameworkData( CassServices.FrameworkModel data )
		{
			var result = CassServices.SaveFrameworkData( data );
			return Utilities.JsonHelper.GetJsonWithWrapper( result, true, "", null );
		}
		//

		//Save the basic data for a node
		//Apparently have to delete first, then create (again) to do an update
		public JsonResult SaveNodeData( CassServices.NodeModel data )
		{

			return Utilities.JsonHelper.GetJsonWithWrapper( data, true, "", null );
		}
		//

	}
}