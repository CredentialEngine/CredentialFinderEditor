using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using Utilities;
using DBEntity = Data.RegistryPublishingHistory;


namespace Factories
{
	public class RegistryPublishManager : BaseFactory
    {
        static string thisClassName = "RegistryPublishManager";
        List<string> messages = new List<string>();

        #region === Persistance ==================


        /// <summary>
        /// Add Publish
        /// </summary>
        /// <param name="dataOwnerCTID"></param>
        /// <param name="payloadJSON"></param>
        /// <param name="publishMethodURI"></param>
        /// <param name="publishingEntityType"></param>
        /// <param name="ctdlType"></param>
        /// <param name="entityCtid"></param>
        /// <param name="payloadInput"></param>
        /// <param name="statusMessage"></param>
        /// <returns></returns>
        public int Add( string environment, string dataOwnerCTID, string payloadJSON, string publishMethodURI, string publishingEntityType, string ctdlType, string entityCtid, string payloadInput, string crEnvelopeId, ref string statusMessage )
        {
            DBEntity efEntity = new DBEntity();

            using (var context = new Data.CTIEntities())
            {
                try
                {

                    efEntity.Created = System.DateTime.Now;
                    efEntity.Environment = environment;
                    efEntity.DataOwnerCTID = dataOwnerCTID;
                    efEntity.PublishPayload = payloadJSON;
                    efEntity.PublishMethodURI = publishMethodURI;
                    efEntity.PublishingEntityType = publishingEntityType;
                    efEntity.CtdlType = ctdlType;
                    efEntity.EntityCtid = entityCtid;
                    efEntity.EnvelopeId = crEnvelopeId;
                    efEntity.PublishInput = payloadInput;

                    context.RegistryPublishingHistory.Add( efEntity );

                    // submit the change to database
                    int count = context.SaveChanges();
                    if (count > 0)
                    {
                        statusMessage = "successful";

                        return efEntity.Id;
                    }
                    else
                    {
                        //?no info on error
                        statusMessage = "Error - the add was not successful. ";
                        string message = string.Format( thisClassName + string.Format( ".Add() Failed", "The process appeared to not work, but was not an exception, so we have no message, or no clue. dataOwnerCTID: {0}; publishingEntityType: {1}", dataOwnerCTID, publishingEntityType ) );
                        EmailManager.NotifyAdmin( thisClassName + ".Add() Failed", message );
                    }
                }

                catch (Exception ex)
                {
                    LoggingHelper.LogError( ex, thisClassName + string.Format( ".Add(), dataOwnerCTID: {0}; publishingEntityType: {1}", dataOwnerCTID, publishingEntityType ) );
                }
            }

            return efEntity.Id;
        }

        #endregion

        #region == Retrieval =======================
        public static DBEntity GetBasic( int id )
        {
            DBEntity efentity = new DBEntity();
            using (var context = new Data.CTIEntities())
            {
                efentity = context.RegistryPublishingHistory
                        .FirstOrDefault( s => s.Id == id );

                if (efentity != null && efentity.Id > 0)
                {
                    //MapFromDB_Basic( item, entity, false, false );
                }
            }
            return efentity;
        }
		public static string GetMostRecentHistory( string ctid, string environment )
		{
			DBEntity efentity = new DBEntity();
			using ( var context = new Data.CTIEntities() )
			{
				List<DBEntity> list = context.RegistryPublishingHistory
						.Where( s => s.EntityCtid.ToLower() == ctid.ToLower()
						&& ( s.Environment == environment ) )
						.OrderByDescending( s => s.Created )
						.Take( 1 )
						.ToList();

				if ( list != null && list.Count > 0 )
				{
					efentity = list[ 0 ];
					var result = JsonConvert.SerializeObject( efentity );
					return result;
				}
				
			}
			return "";
		}

		/// <summary>
		/// Get all history entries for an org, entity type, and environment
		/// NOTE: would likely want to be able to more finely tune this request. 
		/// Of especial concern would be to only get the latest publishing record
		/// - could order descending, track CTIDs and exclude duplicates
		/// - or order by ctid, and skip duplicates
		/// </summary>
		/// <param name="orgCtid"></param>
		/// <param name="environment"></param>
		/// <param name="entityType"></param>
		/// <returns></returns>
		public static List<DBEntity> GetAllForOrg( string orgCtid, string environment, string entityType, string createdFilter = "" )
        {
            DBEntity entity = new DBEntity();
            List<DBEntity> output = new List<DBEntity>();
			List<string> ctids = new List<string>();
			DateTime createdDate = new DateTime( 2017, 1, 1 );
			if (IsValidDate(createdFilter))
			{
				createdDate = DateTime.Parse( createdFilter );
			}
            using (var context = new Data.CTIEntities())
            {
                List<DBEntity> list = context.RegistryPublishingHistory
                        .Where( s => s.DataOwnerCTID == orgCtid 
                            && s.Environment == environment
                            && ( entityType == "" || s.PublishingEntityType == entityType)
							&& s.Created >= createdDate
                        )
                        .OrderBy( s =>  s.EntityCtid )
						.ThenByDescending( s => s.Created)
                        .ToList();
				string prevCtid = "";
                if (list != null && list.Count > 0)
                {
					foreach ( var item in list )
					{
						if ( prevCtid != item.EntityCtid )
							output.Add( item );
						else
						{
							
						}

						prevCtid = item.EntityCtid;
					}
                    
                }
                return output;
            }
        }

        public static string GetMostRecentPublishedPayload( string ctid, string environment = "")
        {
            string payload = "";
            using (var context = new Data.CTIEntities())
            {
                List<DBEntity> list = context.RegistryPublishingHistory
                        .Where( s => s.EntityCtid == ctid
                        && ( environment == "" || s.Environment == environment ))
                        .Take(1)
                        .OrderByDescending(s => s.Created)
                        .ToList();

                if (list != null && list.Count > 0)
                {
                    payload = list[ 0 ].PublishPayload;
                }
                return payload;
            }
        }
        #endregion
    }
}
