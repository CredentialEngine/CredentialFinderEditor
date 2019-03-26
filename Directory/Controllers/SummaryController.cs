using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using CTIServices;
using Models;
using Models.Common;
using Models.ProfileModels;
using Utilities;

namespace CTI.Directory.Controllers
{
    public class SummaryController : BaseController
    {
        static AppUser user = new AppUser();
        //Ensure the user is logged in - otherwise reject
        public class LoggedInJsonFilterAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting( ActionExecutingContext filterContext )
            {
                if ( !filterContext.HttpContext.User.Identity.IsAuthenticated )
                {
					//extra check 
					if ( AccountServices.IsUserAuthenticated() )
					{
						user = AccountServices.GetUserFromSession();
					}
					else
					{
						filterContext.Result = JsonHelper.GetJsonWithWrapper( null, false, "You must be logged in to access this data.", null );
					}
                }
                else
                {
                    user = AccountServices.GetCurrentUser( filterContext.HttpContext.User.Identity.Name );
                    base.OnActionExecuting( filterContext );
                }
            }
        }
        //
        // GET: Summary

		[Route("summary/organization")]
		public ActionResult Organization()
		{
			user = AccountServices.GetUserFromSession();
			return Organization( user.Organizations.FirstOrDefault().Id );
		}

        [Route("summary/organization/{organizationID}")]
		public ActionResult Organization( int organizationID )
		{
			var vm = OrganizationServices.GetForSummary( organizationID );
			vm.IsApproved = vm.IsApproved || (vm.LastApproved != null && vm.LastApproved > DateTime.MinValue) || vm.IsEntityApproved() || (vm.EntityApproval != null && vm.EntityApproval.Id > 0); //This value is not being properly set by GetForSummary(), also how many different properties to track approval do we really /need/?
			return View( "~/Views/V2/Summary/OrganizationSummaryV1.cshtml", vm );
		}
		//

		//Get all departments/sub-organizations
		public JsonResult GetAll_Organization( string organizationGUID )
		{
            int pTotalRows = 0;
            var records = OrganizationServices.GetAllForOwningOrganization( organizationGUID, ref pTotalRows );
            var list = new List<OrganizationListItem>();
            foreach ( var item in records )
            {
                list.Add( new OrganizationListItem()
                {
                    Name = item.Name,
                    Type = SummaryListItem.ListItemType.Organization,
                    Id = item.Id,
                    Guid = item.RowId.ToString(),
                    LastUpdated = SummaryListItem.ValidateDate( item.EntityLastUpdated ),
                    LastApproved = SummaryListItem.ValidateDate( item.LastApproved ),
                    LastPublished = SummaryListItem.ValidateDate( item.LastPublished ),
                    HasRelatedItems_Organization = item.ChildOrganizationsList
                } );
            }

            return JsonResponse( list, true, "", null );
		}
		//

		public JsonResult GetAll_Credential( string organizationGUID )
		{
            int pTotalRows = 0;
            var records = CredentialServices.GetAllForOwningOrganization( organizationGUID, ref pTotalRows );
            

            var list = new List<CredentialListItem>();
            foreach (var item in records )
            {
                list.Add( new CredentialListItem()
                {
                    Name = item.Name,
                    Type = SummaryListItem.ListItemType.Credential,
                    Id = item.Id,
                    Guid = item.RowId.ToString(),
                    LastUpdated = SummaryListItem.ValidateDate( item.EntityLastUpdated ),
                    LastApproved = SummaryListItem.ValidateDate( item.LastApproved ),
                    LastPublished = SummaryListItem.ValidateDate( item.LastPublished ),
                    HasRelatedItems_Assessment = item.TargetAssessmentsList,
                    HasRelatedItems_LearningOpportunity = item.TargetLearningOppsList,
                    HasRelatedItems_Credential = item.TargetCredentialsList,
                    HasRelatedItems_CostManifest = item.CommonCostsList,
                    HasRelatedItems_ConditionManifest = item.CommonConditionsList
                } );
            }
            
			return JsonResponse( list, true, "", null );
		}
        //

        public JsonResult GetAll_Assessment( string organizationGUID )
		{
            int pTotalRows = 0;
			var records = AssessmentServices.GetAllForOwningOrganization( organizationGUID, ref pTotalRows );
            var list = new List<SummaryListItem>();
            foreach ( var item in records )
            {
                list.Add( new AssessmentLoppListItem()
                {
                    Name = item.Name,
                    Type = SummaryListItem.ListItemType.Assessment,
                    Id = item.Id,
                    Guid = item.RowId.ToString(),
					LastUpdated = SummaryListItem.ValidateDate( item.EntityLastUpdated ),
					LastApproved = SummaryListItem.ValidateDate( item.LastApproved ),
					LastPublished = SummaryListItem.ValidateDate( item.LastPublished ),
                    HasRelatedItems_CostManifest = item.CommonCostsList,
                    HasRelatedItems_ConditionManifest = item.CommonConditionsList
                } );
            }

            return JsonResponse( list, true, "", null );
        }
        //

        public JsonResult GetAll_LearningOpportunity( string organizationGUID )
		{
            int pTotalRows = 0;
            var records = LearningOpportunityServices.GetAllForOwningOrganization( organizationGUID, ref pTotalRows );
			var list = new List<SummaryListItem>();
			foreach ( var item in records )
            {
                list.Add( new AssessmentLoppListItem()
                {
                    Name = item.Name,
                    Type = SummaryListItem.ListItemType.LearningOpportunity,
                    Id = item.Id,
                    Guid = item.RowId.ToString(),
					LastUpdated = SummaryListItem.ValidateDate( item.EntityLastUpdated ),
					LastApproved = SummaryListItem.ValidateDate( item.LastApproved ),
					LastPublished = SummaryListItem.ValidateDate( item.LastPublished ),
                    HasRelatedItems_CostManifest = item.CommonCostsList,
                    HasRelatedItems_ConditionManifest = item.CommonConditionsList
                } );
            }

            return JsonResponse( list, true, "", null );
        }
        //

        public JsonResult GetAll_CostManifest( string organizationGUID )
        {
            int pTotalRows = 0;
            var records = CostManifestServices.GetAllForOwningOrganization( organizationGUID, ref pTotalRows );
			var list = new List<SummaryListItem>();
			foreach ( var item in records )
            {
                list.Add( new SummaryListItem()
                {
                    Name = item.Name,
                    Type = SummaryListItem.ListItemType.CostManifest,
                    Id = item.Id,
                    Guid = item.RowId.ToString(),
					LastUpdated = SummaryListItem.ValidateDate( item.EntityLastUpdated ),
					LastApproved = SummaryListItem.ValidateDate( item.LastApproved ),
					LastPublished = SummaryListItem.ValidateDate( item.LastPublished )
				} );
            }

            return JsonResponse( list, true, "", null );
        }
        //

        public JsonResult GetAll_ConditionManifest( string organizationGUID )
        {
            int pTotalRows = 0;
            var records = ConditionManifestServices.GetAllForOwningOrganization( organizationGUID, ref pTotalRows );
			var list = new List<SummaryListItem>();
			foreach ( var item in records )
            {
                list.Add( new ManifestListItem()
                {
                    Name = item.Name,
                    Type = SummaryListItem.ListItemType.ConditionManifest,
                    Id = item.Id,
                    Guid = item.RowId.ToString(),
					LastUpdated = SummaryListItem.ValidateDate( item.EntityLastUpdated ),
					LastApproved = SummaryListItem.ValidateDate( item.LastApproved ),
					LastPublished = SummaryListItem.ValidateDate( item.LastPublished ),
                    HasRelatedItems_Assessment = item.TargetAssessmentsList,
                    HasRelatedItems_LearningOpportunity = item.TargetLearningOppsList,
                    HasRelatedItems_Credential = item.TargetCredentialsList
                } );
            }

            return JsonResponse( list, true, "", null );
        }
        //        

        public JsonResult GetAll_CredentialTest( string organizationGUID )
        {

            var temporaryData = new List<CredentialListItem>()
            {
                new CredentialListItem() { Name = "Credential One", Type = SummaryListItem.ListItemType.Credential, Id = 99, Guid = "8b1be750-0517-4844-a5f4-220a9d38759e", LastUpdated = DateTime.Parse( "2018-5-12" ), LastApproved = DateTime.Parse( "2018-5-13" ), LastPublished = DateTime.Parse( "2018-5-14" ), HasRelatedItems_Assessment = new List<string>() { "8d09fc4f-5392-4877-86c8-8df85fa33cd0", "425d3404-795e-4712-8190-54ae08c29afc", "1bc0ec7d-56a4-47dd-a2ff-ba3834682ed8" }, HasRelatedItems_LearningOpportunity = new List<string>() { "dfa0e629-fc72-4fe7-bfb9-b41fc71f5c82" } },
                new CredentialListItem() { Name = "Credential Two", Type = SummaryListItem.ListItemType.Credential, Id = 11, Guid = "9ef9d6d7-1eed-4965-8046-4a51c9cab772", LastUpdated = DateTime.Parse( "2018-4-11" ), LastApproved = DateTime.Parse( "2018-5-13" ), HasRelatedItems_LearningOpportunity = new List<string>() { "bdfa09e2-0697-49b8-b1fa-7e97b54f5469", "a53050be-40c2-47e0-8669-0bfd0dacae9e" }, HasRelatedItems_Credential = new List<string>() { "753d370b-5a7e-4e96-914a-475009383ce6" } },
                new CredentialListItem() { Name = "Credential Three", Type = SummaryListItem.ListItemType.Credential, Id = 22, Guid = "753d370b-5a7e-4e96-914a-475009383ce6", LastUpdated = DateTime.Parse( "2018-5-13" ), HasRelatedItems_Assessment = new List<string>() { "1bc0ec7d-56a4-47dd-a2ff-ba3834682ed8" }, HasRelatedItems_CostManifest = new List<string>() { "3986e628-9942-4a19-8d98-b320b72d589a" }, HasRelatedItems_ConditionManifest = new List<string>() { "630ef366-2f24-46ea-8d4e-c2eb2ab4efd1", "1975ce42-66bc-43f8-84b1-860a814253d3" } }
            };
            return JsonResponse( temporaryData, true, "", null );
        }
        //
        public JsonResult GetAll_AssessmentTest( string organizationGUID )
        {
            var temporaryData = new List<SummaryListItem>()
            {
                new SummaryListItem() { Name = "Assessment One", Type = SummaryListItem.ListItemType.Assessment, Id = 193, Guid = "8d09fc4f-5392-4877-86c8-8df85fa33cd0", LastUpdated = DateTime.Parse( "2018-3-3" ), LastApproved = DateTime.Parse( "2018-2-3" ), LastPublished = DateTime.Parse( "2018-2-4" ) },
                new SummaryListItem() { Name = "Assessment Two", Type = SummaryListItem.ListItemType.Assessment, Id = 195, Guid = "425d3404-795e-4712-8190-54ae08c29afc", LastUpdated = DateTime.Parse( "2018-3-3" ), LastApproved = DateTime.Parse( "2018-2-3" ) },
                new SummaryListItem() { Name = "Assessment Three", Type = SummaryListItem.ListItemType.Assessment, Id = 123, Guid = "1bc0ec7d-56a4-47dd-a2ff-ba3834682ed8", LastUpdated = DateTime.Parse( "2018-3-3" ) },
                new SummaryListItem() { Name = "Assessment Four (Orphan)", Type = SummaryListItem.ListItemType.Assessment, Id = 8349, Guid = "e8f77882-9403-438f-a24a-da1a35b4c340", LastUpdated = DateTime.Parse( "2018-3-3" ) },
            };
            return JsonResponse( temporaryData, true, "", null );
        }
        //
        public JsonResult GetAll_LearningOpportunityTest( string organizationGUID )
        {
            var temporaryData = new List<SummaryListItem>()
            {
                new SummaryListItem() { Name = "Learning Opportunity One", Type = SummaryListItem.ListItemType.LearningOpportunity, Id = 193, Guid = "dfa0e629-fc72-4fe7-bfb9-b41fc71f5c82", LastUpdated = DateTime.Parse( "2018-3-3" ), LastApproved = DateTime.Parse( "2018-2-3" ), LastPublished = DateTime.Parse( "2018-2-4" ) },
                new SummaryListItem() { Name = "Learning Opportunity Two", Type = SummaryListItem.ListItemType.LearningOpportunity, Id = 195, Guid = "bdfa09e2-0697-49b8-b1fa-7e97b54f5469", LastUpdated = DateTime.Parse( "2018-3-3" ), LastApproved = DateTime.Parse( "2018-2-3" ) },
                new SummaryListItem() { Name = "Learning Opportunity Three", Type = SummaryListItem.ListItemType.LearningOpportunity, Id = 123, Guid = "a53050be-40c2-47e0-8669-0bfd0dacae9e", LastUpdated = DateTime.Parse( "2018-3-3" ) },
            };
            return JsonResponse( temporaryData, true, "", null );
        }
        //
        public JsonResult GetAll_CostManifestTest( string organizationGUID )
		{
			var temporaryData = new List<SummaryListItem>()
			{
				new SummaryListItem() { Name = "Cost Manifest One", Type = SummaryListItem.ListItemType.LearningOpportunity, Id = 193, Guid = "3986e628-9942-4a19-8d98-b320b72d589a", LastUpdated = DateTime.Parse( "2018-3-3" ), LastApproved = DateTime.Parse( "2018-2-3" ), LastPublished = DateTime.Parse( "2018-2-4" ) },
				new SummaryListItem() { Name = "Cost Manifest Two", Type = SummaryListItem.ListItemType.LearningOpportunity, Id = 123, Guid = "aaadf24c-9305-40fb-a743-e7d3152640bf", LastUpdated = DateTime.Parse( "2018-3-3" ) },
			};
			return JsonResponse( temporaryData, true, "", null );
		}
        //
        public JsonResult GetAll_ConditionManifestTest( string organizationGUID )
		{
			var temporaryData = new List<SummaryListItem>()
			{
				new SummaryListItem() { Name = "Condition Manifest One", Type = SummaryListItem.ListItemType.ConditionManifest, Id = 193, Guid = "630ef366-2f24-46ea-8d4e-c2eb2ab4efd1", LastUpdated = DateTime.Parse( "2018-3-3" ), LastApproved = DateTime.Parse( "2018-2-3" ), LastPublished = DateTime.Parse( "2018-2-4" ) },
				new SummaryListItem() { Name = "Condition Manifest Two", Type = SummaryListItem.ListItemType.Assessment, Id = 195, Guid = "1975ce42-66bc-43f8-84b1-860a814253d3", LastUpdated = DateTime.Parse( "2018-3-3" ), LastApproved = DateTime.Parse( "2018-2-3" ) },
			};
			return JsonResponse( temporaryData, true, "", null );
		}
        //
        [LoggedInJsonFilter]
        public JsonResult ApproveEntityByTypeAndId( string entityType, int id, bool testMode = false )
        {
            bool valid = true;
            bool isPublished = false;
            string status = "";
            bool sendEmailOnSuccess = false;

            if ( testMode )
			{
				var rand = new Random();
				valid = rand.Next( 10 ) > 1;
				status = valid ? "Test Message: Successful" : "Test Message: Error";
				System.Threading.Thread.Sleep( rand.Next( 500, 3000 ) );
			}
			else
			{
                //we don't want individual emails for other than org
                if ( entityType.ToLower() == "organization" )
                    sendEmailOnSuccess = true;
                valid = new ProfileServices().Entity_Approval_Save( entityType, id, user, ref isPublished, ref status, sendEmailOnSuccess );
			}

			//Return the result
			var date = DateTime.Now;
			return JsonResponse( new
			{
				IsApproved = valid,
				LastApproved = date,
				LastApprovedString = SummaryListItem.FormatDate( date ),
				LastApprovedSort = SummaryListItem.GetSortDate( date ),
				NeedsReapproval = false,
				StatusMessages = new List<string>()
			}, valid, status, null );
        }
        //
        [LoggedInJsonFilter]
        public JsonResult PublishEntityByTypeAndId( string entityType, int id )
        {
            bool valid = true;
            bool isPublished = false;
            string status = "";
            //need to inhibit this part
            List<SiteActivity> list = new List<SiteActivity>();
            switch ( entityType )
            {
                case "Credential":
                    valid = new RegistryServices().PublishCredential( id, user, ref status, ref list );
                    break;
                case "QACredential":
                    valid = new RegistryServices().PublishCredential( id, user, ref status, ref list );
                    break;
                case "Organization":
                case "QAOrganization":
                    //called method checks for authorization
                    //set to NOT publish manifests with the org
                    valid = new RegistryServices().PublishOrganization( id, user, ref status, ref list, false );
					//currently no publish notification and always single, so could notify here, if valid
					if (valid)
					{
						//the standard email may not be appropriate for just the org
						ProfileServices.SendPublishingSummaryEmail( entityType, id, user, ref status );
					}
                    break;
                case "AssessmentProfile":
                case "Assessment":
                    //called method checks for authorization
                    valid = new RegistryServices().PublishAssessment( id, user, ref status, ref list );
                    break;
                case "LearningOpportunityProfile":
                case "LearningOpportunity":
                    //called method checks for authorization
                    valid = new RegistryServices().PublishLearningOpportunity( id, user, ref status, ref list );
                    break;
                case "ConditionManifest":
                    valid = new RegistryServices().PublishConditionManifest( id, user, ref status, ref list );
                    break;
                case "CostManifest":
                    valid = new RegistryServices().PublishCostManifest( id, user, ref status, ref list );
                    break;
                default:
                    valid = false;
                    status = "Profile not handled";
                    break;
            }

			//Return the result
            var date = DateTime.Now;
            return JsonResponse( new
            {
                IsPublished = valid,
                LastPublished = date,
                LastPublishedString = SummaryListItem.FormatDate( date ),
				LastPublishedSort = SummaryListItem.GetSortDate( date ),
                NeedsRepublishing = false,
                StatusMessages = new List<string>()
            }, valid, status, null );
        }
        //
        [LoggedInJsonFilter]
        public JsonResult UnPublishEntityByTypeAndId( string entityType, int id )
        {
            bool valid = true;
            string status = "";

			//just in case - re: Scarlett issue
			user = AccountServices.GetUserFromSession();
			//need to inhibit this part
			List<SiteActivity> list = new List<SiteActivity>();
            switch ( entityType )
            {
                case "Credential":
                    valid = new RegistryServices().Unregister_Credential( id, user, ref status, ref list );
                    break;
                case "QACredential":
                    valid = new RegistryServices().Unregister_Credential( id, user, ref status, ref list );
                    break;
                case "Organization":
                case "QAOrganization":
                    //called method checks for authorization
                    valid = new RegistryServices().Unregister_Organization( id, user, ref status, ref list );
                    break;
                case "AssessmentProfile":
                case "Assessment":
                    //called method checks for authorization
                    valid = new RegistryServices().Unregister_Assessment( id, user, ref status, ref list );
                    break;
                case "LearningOpportunityProfile":
                case "LearningOpportunity":
                    //called method checks for authorization
                    valid = new RegistryServices().Unregister_LearningOpportunity( id, user, ref status, ref list );
                    break;
                case "ConditionManifest":
                    valid = new RegistryServices().Unregister_ConditionManifest( id, user, ref status, ref list );
                    break;
                case "CostManifest":
                    valid = new RegistryServices().Unregister_CostManifest( id, user, ref status, ref list );
                    break;
                default:
                    valid = false;
                    status = "Profile not handled";
                    break;
            }

            //Return the result - TBD
            var date = DateTime.Now;
            return JsonResponse( new
            {
                IsPublished = !valid, //if successfull, set unpublished
                LastPublishedString = "",
                LastPublishedSort = "",
                NeedsReapproval = false,
                StatusMessages = new List<string>()
            }, valid, status, null );
        }
		//

		public JsonResult SendSummaryEmail_Approve( string organizationGUID, string primaryType, List<SummaryActionResult> items )
		{
			var successfulItems = items.Where( m => m.Valid ).ToList();
			var failedItems = items.Where( m => !m.Valid ).ToList();
            string status = "";
            if ( successfulItems.Count() > 0 )
                ProfileServices.SendApprovalSummaryEmail( primaryType, successfulItems.Count(), organizationGUID, user, ref status );


            return JsonHelper.GetJsonWithWrapper( null, true, "okay", null );
		}
		//

		public JsonResult SendSummaryEmail_Publish( string organizationGUID, string primaryType, List<SummaryActionResult> items )
		{
			var successfulItems = items.Where( m => m.Valid ).ToList();
			var failedItems = items.Where( m => !m.Valid ).ToList();
            if (successfulItems.Count > 0)
            {
                foreach ( var item in successfulItems)
                {
                    //the names are not included.
                    //item.Id;
                }
            }
            

            string status = "";
            if ( successfulItems.Count() > 0 )
                ProfileServices.SendPublishingSummaryEmail( primaryType, successfulItems.Count, organizationGUID, user, ref status );

            return JsonHelper.GetJsonWithWrapper( null, true, "okay", null );
		}
        //
        public JsonResult SendSummaryEmail_unpublish( string organizationGUID, string primaryType, List<SummaryActionResult> items )
        {
            var successfulItems = items.Where( m => m.Valid ).ToList();
            var failedItems = items.Where( m => !m.Valid ).ToList();

            string status = "";
            if ( successfulItems.Count() > 0 )
                ProfileServices.SendUnpublishingSummaryEmail( primaryType, successfulItems.Count(), organizationGUID, user, ref status );

            return JsonHelper.GetJsonWithWrapper( null, true, "okay", null );
        }
        //
        //
        public JsonResult GetItemSummary( SummaryListItem.ListItemType dataType, string guid )
		{
			switch ( dataType )
			{
				case SummaryListItem.ListItemType.Organization:
					{
						var data = OrganizationServices.GetLightOrgByRowId( guid );
						return JsonHelper.GetJsonWithWrapper( new SummaryListItem() { Name = data.Name, Id = data.Id, Guid = data.RowId.ToString(), Type = SummaryListItem.ListItemType.Organization }, true, "", null );
					}
				case SummaryListItem.ListItemType.Credential:
					{
						var data = CredentialServices.GetBasicCredential( Guid.Parse( guid ) );
						return JsonHelper.GetJsonWithWrapper( new SummaryListItem() { Name = data.Name, Id = data.Id, Guid = data.RowId.ToString(), Type = SummaryListItem.ListItemType.Credential }, true, "", null );
					}
				case SummaryListItem.ListItemType.Assessment:
					{
						var data = AssessmentServices.GetLightAssessmentByRowId( guid );
						return JsonHelper.GetJsonWithWrapper( new SummaryListItem() { Name = data.Name, Id = data.Id, Guid = data.RowId.ToString(), Type = SummaryListItem.ListItemType.Assessment }, true, "", null );
					}
				case SummaryListItem.ListItemType.LearningOpportunity:
					{
						var data = LearningOpportunityServices.GetLightLearningOpportunityByRowId( guid );
						return JsonHelper.GetJsonWithWrapper( new SummaryListItem() { Name = data.Name, Id = data.Id, Guid = data.RowId.ToString(), Type = SummaryListItem.ListItemType.LearningOpportunity }, true, "", null );
					}
				case SummaryListItem.ListItemType.CostManifest:
					{
						var data = CostManifestServices.GetBasic( Guid.Parse( guid ) );
						return JsonHelper.GetJsonWithWrapper( new SummaryListItem() { Name = data.Name, Id = data.Id, Guid = data.RowId.ToString(), Type = SummaryListItem.ListItemType.CostManifest }, true, "", null );
					}
				case SummaryListItem.ListItemType.ConditionManifest:
					{
						var data = ConditionManifestServices.GetBasic( Guid.Parse( guid ) );
						return JsonHelper.GetJsonWithWrapper( new SummaryListItem() { Name = data.Name, Id = data.Id, Guid = data.RowId.ToString(), Type = SummaryListItem.ListItemType.ConditionManifest }, true, "", null );
					}
				default:
					return JsonHelper.GetJsonWithWrapper( null, false, "Unable to determine data type", null );
			}
		}
		//

		public class SummaryListItem
		{
			public SummaryListItem()
			{
				Properties = new Dictionary<string, object>();
			}

			public enum ListItemType { Credential, Assessment, LearningOpportunity, CostManifest, ConditionManifest, Organization }
			public string Name { get; set; }
			public ListItemType Type { get; set; }
			public string TypeName { get { return Type.ToString(); } set { Type = ( ListItemType ) Enum.Parse( typeof( ListItemType ), value, true ); } }
			public int Id { get; set; }
			public string Guid { get; set; }
			public DateTime? LastUpdated { get; set; }
			public DateTime? LastApproved { get; set; }
			public DateTime? LastPublished { get; set; }
			public string LastUpdatedString { get { return FormatDate( LastUpdated ); } }
			public string LastApprovedString { get { return FormatDate( LastApproved ); } }
			public string LastPublishedString { get { return FormatDate( LastPublished ); } }
			public long LastUpdatedSort { get { return GetSortDate( LastUpdated ); } }
			public long LastApprovedSort { get { return GetSortDate( LastApproved ); } }
			public long LastPublishedSort { get { return GetSortDate( LastPublished ); } }
			public Dictionary<string, object> Properties { get; set; }
			public bool IsPublished { get { return LastPublished != null; } }
			public bool IsApproved { get { return LastApproved != null; } }
			public bool NeedsReapproval { get { return
						(LastUpdated != null && LastApproved != null && LastUpdated > LastApproved) || //Updated after approval
						(LastApproved == null && LastPublished != null); //Published but not approved
				} }
			public bool NeedsRepublishing { get { return 
						(LastUpdated != null && LastPublished != null && LastUpdated > LastPublished) || //Updated after publish
						(LastApproved != null && LastPublished != null && LastApproved > LastPublished); //Approved after publish
				} }

			public static string FormatDate( DateTime? date )
			{
				return date == null ? null : date.Value.ToString( "yyyy-MM-dd hh:mm tt" );
			}
			public static long GetSortDate( DateTime? date )
			{
				return date == null ? -1 : long.Parse( date.Value.ToString( "yyyyMMddHHmmss" ) );
			}
			public static DateTime? ValidateDate( DateTime date )
			{
				return ValidateDate( ( DateTime? ) date );
			}
			public static DateTime? ValidateDate( DateTime? date )
			{
				return date > DateTime.MinValue ? date : null;
			}
		}
		//

		public class CredentialListItem : SummaryListItem
		{
			public CredentialListItem()
			{
				HasRelatedItems_Credential = new List<string>();
				HasRelatedItems_Assessment = new List<string>();
				HasRelatedItems_LearningOpportunity = new List<string>();
				HasRelatedItems_CostManifest = new List<string>();
				HasRelatedItems_ConditionManifest = new List<string>();
				Type = ListItemType.Credential;
			}
			public List<string> HasRelatedItems_Credential { get; set; }
			public List<string> HasRelatedItems_Assessment { get; set; }
			public List<string> HasRelatedItems_LearningOpportunity { get; set; }
			public List<string> HasRelatedItems_CostManifest { get; set; }
			public List<string> HasRelatedItems_ConditionManifest { get; set; }
		}
		//

        public class AssessmentLoppListItem : SummaryListItem
        {
            public AssessmentLoppListItem()
            {
                HasRelatedItems_CostManifest = new List<string>();
                HasRelatedItems_ConditionManifest = new List<string>();
            }

            public List<string> HasRelatedItems_CostManifest { get; set; }
            public List<string> HasRelatedItems_ConditionManifest { get; set; }
        }
		//

        public class ManifestListItem : SummaryListItem
        {
            public ManifestListItem()
            {
                HasRelatedItems_Credential = new List<string>();
                HasRelatedItems_Assessment = new List<string>();
                HasRelatedItems_LearningOpportunity = new List<string>();
                Type = ListItemType.ConditionManifest;
            }

            public List<string> HasRelatedItems_Credential { get; set; }
            public List<string> HasRelatedItems_Assessment { get; set; }
            public List<string> HasRelatedItems_LearningOpportunity { get; set; }
        }
		//

        public class OrganizationListItem : SummaryListItem
        {
            public OrganizationListItem()
            {
                HasRelatedItems_Organization = new List<string>();
            }

            public List<string> HasRelatedItems_Organization { get; set; }  
        }
		//

		public class SummaryActionResult
		{
			public SummaryListItem.ListItemType DataType { get; set; }
			public int Id { get; set; }
			public string Status { get; set; }
			public bool Valid { get; set; }
		}
		//
    }
}