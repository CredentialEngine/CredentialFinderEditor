using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Models.Common;
using Models.ProfileModels;
using Utilities;

namespace Factories
{
	public class CacheManager : BaseFactory
	{
		#region cache tables
		public void PopulateEntityRelatedCaches( Guid entityUid, bool doingEntityCache = true, bool doingCredentialCache = true, bool doingOrgCache = true )
		{
			Entity entity = EntityManager.GetEntity( entityUid );
			if ( entity == null || entity.Id == 0 )
				return;


			string connectionString = MainConnection();
			try
			{
				if ( doingEntityCache )
				{
					using ( SqlConnection c = new SqlConnection( connectionString ) )
					{
						using ( SqlCommand command = new SqlCommand( "[Entity_Cache_Populate]", c ) )
						{
							c.Open();
							command.CommandType = CommandType.StoredProcedure;
							command.Parameters.Add( new SqlParameter( "@EntityId", entity.Id ) );
							//command.Parameters.Add( new SqlParameter( "@ClearTable", 0 ) );
							command.CommandTimeout = 300;
							command.ExecuteNonQuery();
							command.Dispose();
							c.Close();

						}

						using ( SqlCommand command = new SqlCommand( "[Populate_Entity_SearchIndex]", c ) )
						{
							c.Open();
							command.CommandType = CommandType.StoredProcedure;
							command.Parameters.Add( new SqlParameter( "@EntityId", entity.Id ) );
							command.CommandTimeout = 300;
							command.ExecuteNonQuery();
							command.Dispose();
							c.Close();

						}
					}
				}


				if ( doingCredentialCache && entity.EntityTypeId == 1 )
				{
					PopulateCredentialRelatedCaches( entity.EntityBaseId );
				}

				if ( doingOrgCache && entity.EntityTypeId == 2 )
				{
					PopulateOrgRelatedCaches( entity.EntityBaseId );
				}
			}
			catch ( Exception ex )
			{
				LoggingHelper.LogError( ex, string.Format("PopulateEntityRelatedCaches. entityUid: {0}, doingCredentialCache: {1}, doingCredentialCache: {2}, doingOrgCache: {3}", entityUid, doingCredentialCache, doingCredentialCache, doingOrgCache), false );

			}
		}//

		/// <summary>
		/// Call with orgId = 0 to update all
		/// Could do after completing an import
		/// </summary>
		/// <param name="credentialId"></param>
		public void PopulateCredentialRelatedCaches( int credentialId )
		{
			string connectionString = MainConnection();
			try
			{
				using ( SqlConnection c = new SqlConnection( connectionString ) )
				{
					c.Open();

					using ( SqlCommand command = new SqlCommand( "[Populate_Credential_SummaryCache]", c ) )
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.Add( new SqlParameter( "@CredentialId", credentialId ) );
						command.CommandTimeout = 300;
						command.ExecuteNonQuery();
						command.Dispose();
						c.Close();

					}
		

				//	using ( SqlCommand command = new SqlCommand( "[Populate_Competencies_cache]", c ) )
				//	{
				//		command.CommandType = CommandType.StoredProcedure;
				//		command.Parameters.Add( new SqlParameter( "@CredentialId", connectionString ) );

				//		command.ExecuteNonQuery();
				//		command.Dispose();
				//		c.Close();
				//}
				}
			}
			catch ( Exception ex )
			{
                LoggingHelper.LogError(ex, string.Format("PopulateCredentialRelatedCaches. credentialId: {0}", credentialId), false);

            }
		}//

		/// <summary>
		/// Call with orgId = 0 to update all
		/// Could do after completing an import
		/// </summary>
		/// <param name="orgId"></param>
		public void PopulateOrgRelatedCaches( int orgId )
		{
			string connectionString = MainConnection();
			try
			{
				//running this after add/update of an org will only be partly complete. 
				//would need to run it after any of cred, asmt, and lopp
				using ( SqlConnection c = new SqlConnection( connectionString ) )
				{
					c.Open();

					using ( SqlCommand command = new SqlCommand( "[Cache.Organization_ActorRoles_Populate]", c ) )
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.Add( new SqlParameter( "@OrgId", orgId ) );

						command.ExecuteNonQuery();
						command.Dispose();
						c.Close();


					}
				}
			}
			catch ( Exception ex )
			{
                LoggingHelper.LogError(ex, string.Format("PopulateOrgRelatedCaches. orgId: {0}", orgId), false);

            }

		}

		#endregion


		#region credentials 
		/// <summary>
		/// 
		/// </summary>
		/// <param name="id"></param>
		/// <param name="key">The key could vary if for detail, compare, etc</param>
		/// <param name="?"></param>
		/// <returns></returns>
		public static bool IsCredentialAvailableFromCache( int id, string key, ref Credential credential )
		{
			
			int cacheMinutes = UtilityManager.GetAppKeyValue( "credentialCacheMinutes", 60 );
			DateTime maxTime = DateTime.Now.AddMinutes( cacheMinutes * -1 );

			//string key = "credential_" + id.ToString();

			if ( HttpRuntime.Cache[ key ] != null && cacheMinutes > 0 )
			{
				var cache = ( CachedCredential ) HttpRuntime.Cache[ key ];
				try
				{
					if ( cache.lastUpdated > maxTime )
					{
						LoggingHelper.DoTrace( 7, string.Format( "===CacheManager.IsCredentialAvailableFromCache === Using cached version of Credential, Id: {0}, {1}, key: {2}", cache.Item.Id, cache.Item.Name, key ) );

						//check if user can update the object
						//or move these checks to the manager
						//string status = "";
						//if ( !CanUserUpdateCredential( id, user, ref status ) )
						//	cache.Item.CanEditRecord = false;

						credential = cache.Item;
						return true;
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.DoTrace( 6, string.Format("===CacheManager.IsCredentialAvailableFromCache. id: {0}, key: {1},  exception ", id, key) + ex.Message );
				}
			}
			
			return false;
		}
		public static void AddCredentialToCache( Credential entity, string key )
		{
			int cacheMinutes = UtilityManager.GetAppKeyValue( "credentialCacheMinutes", 60 );

			//string key = "credential_" + entity.Id.ToString();

			if ( cacheMinutes > 0  )
			{
				var newCache = new CachedCredential()
				{
					Item = entity,
					lastUpdated = DateTime.Now
				};
				if ( HttpContext.Current != null )
				{
					if ( HttpContext.Current.Cache[ key ] != null )
					{
						HttpRuntime.Cache.Remove( key );
						HttpRuntime.Cache.Insert( key, newCache );

						LoggingHelper.DoTrace( 7, string.Format( "===CacheManager.AddCredentialToCache $$$ Updating cached version of credential, Id: {0}, {1}, key: {2}", entity.Id, entity.Name, key ) );

					}
					else
					{
						LoggingHelper.DoTrace( 6, string.Format( "===CacheManager.AddCredentialToCache ****** Inserting new cached version of credential, Id: {0}, {1}, key: {2}", entity.Id, entity.Name, key ) );

						System.Web.HttpRuntime.Cache.Insert( key, newCache, null, DateTime.Now.AddHours( cacheMinutes ), TimeSpan.Zero );
					}
				}
			}

			//return entity;
		}
        #endregion
        #region Organization
        public static bool IsOrganizationAvailableFromCache(int id, ref Organization entity)
        {
            string key = "Organization_" + id.ToString();
            return IsOrganizationAvailableFromCache(key, ref entity);
        }
        public static bool IsOrganizationAvailableFromCache(string key, ref Organization entity)
        {
            int cacheMinutes = UtilityManager.GetAppKeyValue("organizationSummaryCacheMinutes", 0);
            DateTime maxTime = DateTime.Now.AddMinutes(cacheMinutes * -1);
            if ( HttpRuntime.Cache[ key ] != null && cacheMinutes > 0 )
            {
                var cache = ( CachedOrganization )HttpRuntime.Cache[ key ];
                try
                {
                    if ( cache.lastUpdated > maxTime )
                    {
                        LoggingHelper.DoTrace(7, string.Format("%%%CacheManager.IsOrganizationAvailableFromCache === Using cached version of Organization, Id: {0}, {1}", cache.Item.Id, cache.Item.Name));
                        entity = cache.Item;
                        return true;
                    }
                }
                catch ( Exception ex )
                {
                    LoggingHelper.DoTrace(6, "%%%CacheManager.IsOrganizationAvailableFromCache === exception " + ex.Message);
                }
            }

            return false;
        }
        public static void AddOrganizationToCache(Organization entity)
        {
            string key = "Organization_" + entity.Id.ToString();
        }
        public static void AddOrganizationToCache(Organization entity, string key)
        {
            int cacheMinutes = UtilityManager.GetAppKeyValue("organizationSummaryCacheMinutes", 0);

            if ( cacheMinutes > 0 )
            {
                var newCache = new CachedOrganization()
                {
                    Item = entity,
                    lastUpdated = DateTime.Now
                };
                if ( HttpContext.Current != null )
                {
                    if ( HttpContext.Current.Cache[ key ] != null )
                    {
                        HttpRuntime.Cache.Remove(key);
                        HttpRuntime.Cache.Insert(key, newCache);

                        LoggingHelper.DoTrace(7, string.Format("%%%CacheManager.AddOrganizationToCache $$$ Updating cached version of Organization, Id: {0}, {1}", entity.Id, entity.Name));

                    }
                    else
                    {
                        LoggingHelper.DoTrace(6, string.Format("%%%CacheManager.AddOrganizationToCache ****** Inserting new cached version of Organization, Id: {0}, {1}", entity.Id, entity.Name));

                        System.Web.HttpRuntime.Cache.Insert(key, newCache, null, DateTime.Now.AddHours(cacheMinutes), TimeSpan.Zero);
                    }
                }
            }

            //return entity;
        }
        #endregion
        #region LearningOpportunities
        public static bool IsLearningOpportunityAvailableFromCache( int id, ref LearningOpportunityProfile entity )
		{
			int cacheMinutes = UtilityManager.GetAppKeyValue( "learningOppCacheMinutes", 0 );
			DateTime maxTime = DateTime.Now.AddMinutes( cacheMinutes * -1 );

			string key = "LearningOpportunity_" + id.ToString();

			if ( HttpRuntime.Cache[ key ] != null && cacheMinutes > 0 )
			{
				var cache = ( CachedLearningOpportunity ) HttpRuntime.Cache[ key ];
				try
				{
					if ( cache.lastUpdated > maxTime )
					{
						LoggingHelper.DoTrace( 7, string.Format( "%%%CacheManager.IsLearningOpportunityAvailableFromCache === Using cached version of LearningOpportunity, Id: {0}, {1}", cache.Item.Id, cache.Item.Name ) );

						//check if user can update the object
						//string status = "";
						//if ( !CanUserUpdateCredential( id, user, ref status ) )
						//	cache.Item.CanEditRecord = false;

						entity = cache.Item;
						return true;
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.DoTrace( 6, "%%%CacheManager.IsLearningOpportunityAvailableFromCache === exception " + ex.Message );
				}
			}

			return false;
		}
		public static void AddLearningOpportunityToCache( LearningOpportunityProfile entity )
		{
			int cacheMinutes = UtilityManager.GetAppKeyValue( "learningOppCacheMinutes", 0 );

			string key = "LearningOpportunity_" + entity.Id.ToString();

			if ( cacheMinutes > 0 )
			{
				var newCache = new CachedLearningOpportunity()
				{
					Item = entity,
					lastUpdated = DateTime.Now
				};
				if ( HttpContext.Current != null )
				{
					if ( HttpContext.Current.Cache[ key ] != null )
					{
						HttpRuntime.Cache.Remove( key );
						HttpRuntime.Cache.Insert( key, newCache );

						LoggingHelper.DoTrace( 7, string.Format( "%%%CacheManager.AddLearningOpportunityToCache $$$ Updating cached version of LearningOpportunity, Id: {0}, {1}", entity.Id, entity.Name ) );

					}
					else
					{
						LoggingHelper.DoTrace( 6, string.Format( "%%%CacheManager.AddLearningOpportunityToCache ****** Inserting new cached version of LearningOpportunity, Id: {0}, {1}", entity.Id, entity.Name ) );

						System.Web.HttpRuntime.Cache.Insert( key, newCache, null, DateTime.Now.AddHours( cacheMinutes ), TimeSpan.Zero );
					}
				}
			}

			//return entity;
		}
		#endregion 
		#region ConditionProfiles
		public static bool IsConditionProfileAvailableFromCache( int id, ref ConditionProfile entity )
		{
			//use same as lopp for now
			int cacheMinutes = UtilityManager.GetAppKeyValue( "learningOppCacheMinutes", 0 );
			DateTime maxTime = DateTime.Now.AddMinutes( cacheMinutes * -1 );

			string key = "ConditionProfile_" + id.ToString();

			if ( HttpRuntime.Cache[ key ] != null && cacheMinutes > 0 )
			{
				var cache = ( CachedConditionProfile ) HttpRuntime.Cache[ key ];
				try
				{
					if ( cache.lastUpdated > maxTime )
					{
						LoggingHelper.DoTrace( 7, string.Format( "===CacheManager.IsConditionProfileAvailableFromCache === Using cached version of ConditionProfile, Id: {0}, {1}", cache.Item.Id, cache.Item.ProfileName ) );

						//check if user can update the object
						//string status = "";
						//if ( !CanUserUpdateCredential( id, user, ref status ) )
						//	cache.Item.CanEditRecord = false;

						entity = cache.Item;
						return true;
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.DoTrace( 6, "===CacheManager.IsConditionProfileAvailableFromCache === exception " + ex.Message );
				}
			}

			return false;
		}
		public static void AddConditionProfileToCache( ConditionProfile entity )
		{
			int cacheMinutes = UtilityManager.GetAppKeyValue( "learningOppCacheMinutes", 0 );

			string key = "ConditionProfile_" + entity.Id.ToString();

			if ( cacheMinutes > 0 )
			{
				var newCache = new CachedConditionProfile()
				{
					Item = entity,
					lastUpdated = DateTime.Now
				};
				if ( HttpContext.Current != null )
				{
					if ( HttpContext.Current.Cache[ key ] != null )
					{
						HttpRuntime.Cache.Remove( key );
						HttpRuntime.Cache.Insert( key, newCache );

						LoggingHelper.DoTrace( 7, string.Format( "===CacheManager.AddConditionProfileToCache $$$ Updating cached version of ConditionProfile, Id: {0}, {1}", entity.Id, entity.ProfileName ) );

					}
					else
					{
						LoggingHelper.DoTrace( 6, string.Format( "===CacheManager.AddConditionProfileToCache ****** Inserting new cached version of ConditionProfile, Id: {0}, {1}", entity.Id, entity.ProfileName ) );

						System.Web.HttpRuntime.Cache.Insert( key, newCache, null, DateTime.Now.AddHours( cacheMinutes ), TimeSpan.Zero );
					}
				}
			}

			//return entity;
		}
		#endregion 

		public static void RemoveItemFromCache( string type, int id )
		{
			string key = string.Format( "{0}_{1}", type, id );
			if ( HttpContext.Current != null
				&& HttpContext.Current.Cache[ key ] != null )
			{
				HttpRuntime.Cache.Remove( key );

				LoggingHelper.DoTrace( 8, string.Format( "===CacheManager.RemoveFromCache $$$ Removed cached version of a {0}, Id: {1}", type, id ) );

			}
		}
	}

	public class CachedItem
	{
		public CachedItem()
		{
			lastUpdated = DateTime.Now;
		}
		public DateTime lastUpdated { get; set; }

	}
	public class CachedCredential : CachedItem
	{
		public Credential Item { get; set; }

	}
    public class CachedOrganization : CachedItem
    {
        public Organization Item { get; set; }

    }
    public class CachedLearningOpportunity : CachedItem
	{
		public LearningOpportunityProfile Item { get; set; }

	}
	public class CachedConditionProfile : CachedItem
	{
		public ConditionProfile Item { get; set; }

	}
}
