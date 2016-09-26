using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Linq;

using Factories;
using Nest;
using Newtonsoft.Json;
using Models;
using Models.Common;
using ES = Models.Elastic;

namespace CTIServices.elasticSearch
{
	public class ElasticIndexService
	{
		private readonly IElasticClient client;

		public ElasticIndexService()
		{
			client = ElasticConfig.GetClient();
		}

		#region  credentials
		public void SaveCredential( int credentialId, AppUser user, ref string status )
		{
			CredentialSummary entity = CredentialServices.GetLightCredentialById(credentialId);

			//var response = client.Index(entity);
			Credential fullcred = CredentialServices.GetCredentialDetail(1);
			var response2 = client.Index(fullcred);

			ES.Credential cred = new ES.Credential();
			cred.Id = entity.Id;
			cred.RowId = entity.RowId;
			cred.Name = entity.Name;
			cred.Description = entity.Description;
			cred.Url = entity.Url;
			//TODO - ImageUrl is not returned by search
			cred.ImageUrl = entity.ImageUrl;
			cred.ManagingOrgId = entity.ManagingOrgId;
			//??
			cred.StatusId = entity.StatusId;
			cred.CTID = entity.CTID;
			cred.Created = entity.Created;
			cred.LastUpdated = entity.LastUpdated;

			SaveCredential(cred, ref status);
		}
		public bool SaveCredential(ES.Credential entity, ref string status)
		{
			var response = client.Index(entity);
	
			//var index = client.Index(entity, i => i
			//	.Index(ElasticConfig.IndexName)
			//	.Type("credential")
			//	.Id(entity.Id)
			//	.Refresh()
			//	.Ttl("1m")
			//);

			if ( response.Created == false && response.ServerError != null )
			{
				status = response.ServerError.Error.Reason;
				Utilities.LoggingHelper.DoTrace( 4, string.Format( "ElasticIndexService.SaveCredential. Failed for org: {0}, Reason: {1}", entity.Id, status ) );
				//throw new Exception(response.ServerError.Error.Reason);
				return false;
			}

			else
				return true;
		}

		/// <summary>
		/// Load all credentials
		/// -- need a purge first, or do an purge all NEST command
		/// </summary>
		public void LoadAllCredentials(ref string status)
		{
			string where = "";
			int pTotalRows = 0;
			int cntr = 0;
			int loaded = 0;
			int failed = 0;
			status = "";
			//AppUser user = AccountServices.GetCurrentUser();
			//if (user != null && user.Id > 0)
			//	userId = user.Id;
			try
			{
				List<ES.Credential> orgs = CredentialManager.GetAllForElastic( where, 1, 500, ref pTotalRows );
				foreach ( ES.Credential item in orgs )
				{
					cntr++;
					if ( !SaveCredential( item, ref status ) )
					{
						failed++;
					}
					else
						loaded++;
				}
			}
			catch (Exception ex)
			{
				Utilities.LoggingHelper.LogError( ex, "ElasticIndexService.LoadAllCredentials()" );

				status = string.Format( "Exception encountered.{3} <br/>Read: {0}, Loaded: {1}, Failed: {2}", cntr, loaded, failed , ex.Message);
				return;
			}
			status = string.Format( "Read: {0}, Loaded: {1}, Failed: {2}", cntr, loaded, failed );
		}
		#endregion 

		#region  organizations
		/// <summary>
		/// Load all orgs
		/// -- need a purge first, or do an purge all NEST command
		/// </summary>
		public void LoadAllOrganizations( ref string status )
		{
			string where = "";
			int pTotalRows = 0;
			int cntr = 0;
			int loaded = 0;
			int failed = 0;
			status = "";
			//AppUser user = AccountServices.GetCurrentUser();
			//if (user != null && user.Id > 0)
			//	userId = user.Id;
			
			try
			{
				List<ES.Organization> orgs = OrganizationManager.GetAllForElastic( where, 1, 500, ref pTotalRows );
				foreach ( ES.Organization org in orgs )
				{
					cntr++;
					if ( !SaveOrganization( org, ref status ) )
					{
						failed++;
					}
					else
						loaded++;
				}
			}
			catch ( Exception ex )
			{
				Utilities.LoggingHelper.LogError( ex, "ElasticIndexService.LoadAllOrganizations()" );

				status = string.Format( "Exception encountered.{3} <br/>Read: {0}, Loaded: {1}, Failed: {2}", cntr, loaded, failed, ex.Message );
				return;
			}
			status = string.Format( "Read: {0}, Loaded: {1}, Failed: {2}", cntr, loaded, failed );
		}
		public bool SaveOrganization(int organizationId, ref string status)
		{
			ES.Organization entity = OrganizationManager.Organization_GetForElastic(organizationId);

			return SaveOrganization(entity, ref status);
		}

		public bool SaveOrganization(ES.Organization entity, ref string status)
		{

			var response = client.Index(entity);

			if (response.Created == false && response.ServerError != null)
			{
				status = response.ServerError.Error.Reason;
				Utilities.LoggingHelper.DoTrace(4, string.Format("ElasticIndexService.SaveOrganization. Failed for org: {0}, Reason: {1}", entity.Id, status));
				//throw new Exception(response.ServerError.Error.Reason);
				return false;
			}

			else
				return true;
		}

		#endregion 
		public void CreateIndex()
		{
			if (!client.IndexExists(ElasticConfig.IndexName).Exists)
			{
				var indexDescriptor = new CreateIndexDescriptor(ElasticConfig.IndexName)
					.Mappings(ms => ms
						.Map<ES.Credential>(m => m.AutoMap())
						.Map<ES.Organization>(m => m.AutoMap())
						.Map<ES.LearningOpportunity>(m => m.AutoMap())
						.Map<ES.Assessment>(m => m.AutoMap())
						);

				client.CreateIndex(ElasticConfig.IndexName, i => indexDescriptor);

				//client.DeleteIndex()
			}

			
		}
		public void DeleteIndexIfExists()
		{
			if (client.IndexExists(ElasticConfig.IndexName).Exists)
				client.DeleteIndex(ElasticConfig.IndexName);
		}
		#region sample using Post
		public void CreatePostIndex(string fileName, int maxItems)
		{
			if (!client.IndexExists(ElasticConfig.IndexName).Exists)
			{
				var indexDescriptor = new CreateIndexDescriptor(ElasticConfig.IndexName)
					.Mappings(ms => ms
						.Map<Post>(m => m.AutoMap()));

				client.CreateIndex(ElasticConfig.IndexName, i => indexDescriptor);
			}

			BulkIndex(HostingEnvironment.MapPath("~/data/" + fileName), maxItems);
		}

		private IEnumerable<Post> LoadPostsFromFile(string inputUrl)
		{
			using (XmlReader reader = XmlReader.Create(inputUrl))
			{
				reader.MoveToContent();
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element && reader.Name == "row")
					{
						if (String.Equals(reader.GetAttribute("PostTypeId"), "1"))
						{
							XElement el = XNode.ReadFrom(reader) as XElement;

							if (el != null)
							{
								Post post = new Post
								{
									Id = el.Attribute("Id").Value,
									Title = el.Attribute("Title") != null ? el.Attribute("Title").Value : "",
									CreationDate = DateTime.Parse(el.Attribute("CreationDate").Value),
									Score = int.Parse(el.Attribute("Score").Value),
									Body = HtmlRemoval.StripTagsRegex(el.Attribute("Body").Value),
									Tags =
										el.Attribute("Tags") != null
											? el.Attribute("Tags")
												.Value.Replace("><", "|")
												.Replace("<", "")
												.Replace(">", "")
												.Replace("&gt;&lt;", "|")
												.Replace("&lt;", "")
												.Replace("&gt;", "")
												.Split('|')
											: null,
									AnswerCount =
										el.Attribute("AnswerCount") != null
											? int.Parse(el.Attribute("AnswerCount").Value)
											: 0
								};
								post.Suggest = post.Tags;
								yield return post;
							}
						}
					}
				}
			}
		}

		private void BulkIndex(string path, int maxItems)
		{
			int i = 0;
			int take = maxItems;
			int batch = 1000;
			string index = ElasticConfig.IndexName;
			foreach (var batches in LoadPostsFromFile(path).Take(take).Batch(batch))
			{
				i++;
				var result = client.IndexMany<Post>(batches, index);
			}
		}

		#endregion
	}
}
