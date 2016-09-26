using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CM = Models.Common;
using EM = Factories;
using Utilities;

namespace Factories
{
    class Class1
    {
		string thisClassName = "delete me";
		#region hold stuff being removed

		/// <summary>
		/// Add related region. Note the parent and related entities are created first, and then related using this method.
		/// OR - pass the GeoCoordinates with the parentId,
		/// </summary>
		/// <param name="parentId"></param>
		/// <param name="relatedId"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		//public int RelatedRegion_Add( Guid parentId, Guid relatedId, ref string statusMessage )
		//{
		//	int Id = 0;
		//	EM.RelatedRegion efEntity = new EM.RelatedRegion();
		//	efEntity.ParentId = parentId;
		//	efEntity.RelatedId = relatedId;

		//	Id = RelatedRegion_Add( efEntity, ref statusMessage );

		//	return Id;
		//}

		//public int RelatedRegion_Add( EM.RelatedRegion efEntity, ref string statusMessage )
		//{
		//	int Id = 0;
		//	using ( var context = new CTIEntities() )
		//	{
		//		try
		//		{

		//			efEntity.Created = System.DateTime.Now;

		//			context.RelatedRegions.Add( efEntity );

		//			// submit the change to database
		//			int count = context.SaveChanges();
		//			if ( count > 0 )
		//			{
		//				statusMessage = "successful";

		//				return efEntity.Id;
		//			}
		//			else
		//			{
		//				//?no info on error
		//			}
		//		}
		//		catch ( System.Data.Entity.Validation.DbEntityValidationException dbex )
		//		{

		//			string message = thisClassName + string.Format( ".RelatedRegion_Add() DbEntityValidationException,  Parent: {0}, related: {1)", efEntity.ParentId.ToString(), efEntity.RelatedId );
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
		//			LoggingHelper.LogError( ex, thisClassName + string.Format( ".RelatedRegion_Add(), Parent: {0}, related: {1)", efEntity.ParentId.ToString(), efEntity.RelatedId ) );
		//		}
		//	}

		//	return Id;
		//}



		/// <summary>
		/// get related region with GeoCoordinates
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		//public static CM.RelatedRegion RelatedRegion_Get( Guid parentId, Guid relatedId )
		//{
		//	CM.RelatedRegion entity = new CM.RelatedRegion();
		//	List<CM.RelatedRegion> list = new List<CM.RelatedRegion>();
		//	using ( var context = new CTIEntities() )
		//	{
		//		RelatedRegion_GeoCoordinateSummary item = context.RelatedRegion_GeoCoordinateSummary
		//					.SingleOrDefault( s => s.ParentId == parentId && s.RelatedId == relatedId );

		//		if ( item != null && item.PrimaryId > 0 )
		//		{
		//			RelatedRegion_ToMap( item, entity );
		//		}
		//	}

		//	return entity;
		//}

		/// <summary>
		/// get all related region with GeoCoordinates for the parent
		/// </summary>
		/// <param name="parentId"></param>
		/// <returns></returns>
		//public static List<CM.RelatedRegion> RelatedRegion_GetAll( Guid parentId )
		//{
		//	CM.RelatedRegion entity = new CM.RelatedRegion();
		//	List<CM.RelatedRegion> list = new List<CM.RelatedRegion>();
		//	using ( var context = new CTIEntities() )
		//	{
		//		RelatedRegion_GeoCoordinateSummary item = context.RelatedRegion_GeoCoordinateSummary
		//					.SingleOrDefault( s => s.ParentId == parentId );

		//		if ( item != null && item.PrimaryId > 0 )
		//		{
		//			RelatedRegion_ToMap( item, entity );
		//		}
		//	}

		//	return list;
		//}

		/// <summary>
		/// Probably want to combine with region to have access to keys
		/// </summary>
		/// <param name="efEntity"></param>
		/// <param name="statusMessage"></param>
		/// <returns></returns>
		//public int GeoCoordinate_Add( EM.GeoCoordinate efEntity, ref string statusMessage )
		//{

		//	using ( var context = new CTIEntities() )
		//	{
		//		try
		//		{
		//			if ( efEntity.ParentId.ToString() == DEFAULT_GUID )
		//			{
		//				statusMessage = "Error - missing a parent idenifier";
		//			}

		//			efEntity.Created = System.DateTime.Now;

		//			context.GeoCoordinates.Add( efEntity );

		//			// submit the change to database
		//			int count = context.SaveChanges();
		//			if ( count > 0 )
		//			{
		//				statusMessage = "successful";

		//				return efEntity.Id;
		//			}
		//			else
		//			{
		//				//?no info on error
		//			}
		//		}

		//		catch ( Exception ex )
		//		{
		//			LoggingHelper.LogError( ex, thisClassName + string.Format( ".RelatedRegion_Add(), Name: {0}, ParentId: {1)", efEntity.Name, efEntity.ParentId ) );
		//		}
		//	}

		//	return 0;
		//}

		//private static void RelatedRegion_FromMap( CM.RelatedRegion fromEntity, EM.RelatedRegion to )
		//{
		//	to.Id = fromEntity.Id;
		//	to.ParentId = fromEntity.ParentId;
		//	to.RelatedId = fromEntity.RelatedId;
		//	//to.RegionContextId = fromEntity.RegionContextId;

		//	if ( fromEntity.Created != null )
		//		to.Created = fromEntity.Created;

		//}
		//private static void RelatedRegion_ToMap( EM.RelatedRegion_GeoCoordinateSummary fromEntity, CM.RelatedRegion to )
		//{
		//	to.Id = fromEntity.PrimaryId;
		//	to.ParentId = fromEntity.ParentId;
		//	to.RelatedId = fromEntity.RelatedId;
		//	to.RegionContextId = fromEntity.RegionContextId == null ? 0 : ( int ) fromEntity.RegionContextId;
		//	to.RegionContext = fromEntity.RegionContext;

		//	to.Coordinates = new CM.GeoCoordinates();
		//	to.Coordinates.Id = fromEntity.GeoCoordinateId;
		//	to.Coordinates.Name = fromEntity.Name;
		//	to.Coordinates.Latitude = fromEntity.Latitude;
		//	to.Coordinates.Longitude = fromEntity.Longitude;
		//	to.Coordinates.Url = fromEntity.Url;
		//	if ( fromEntity.Created != null )
		//		to.Coordinates.Created = ( DateTime ) fromEntity.Created;
		//	to.Coordinates.CreatedById = fromEntity.CreatedById == null ? 0 : ( int ) fromEntity.CreatedById;

		//	if ( fromEntity.Created != null )
		//		to.Created = ( DateTime ) fromEntity.Created;


		//}
		#endregion
    }
}
