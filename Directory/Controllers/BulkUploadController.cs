using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Net.Http;
using System.Runtime.Caching;
using System.IO;
using Microsoft.VisualBasic.FileIO;

using Models;
using Models.Search;
using CTIServices.Import;
using CTIServices;
using Utilities;

namespace CTI.Directory.Controllers
{
    public class BulkUploadController : BaseController
    {
        AppUser user = AccountServices.GetCurrentUser();

        // GET: BulkUpload
        public ActionResult Index( int organizationId = 0 )
        {
            return Simple( organizationId );
        }
        //

        public ActionResult BulkUploadPage( int organizationId, string bulkUploadType )
        {
            if ( user == null || user.Id == 0 )
            {
                SetSystemMessage( "Unauthorized Action", "You must be logged in to perform import tasks." );
                return RedirectToAction( "Index", "Message" );
            }

			var tpOrgs = OrganizationServices.GetBulkUploadOrganizationsForUser( user );

			if ( organizationId > 0 && AccountServices.IsUserSiteStaff( user ) )
            {
                int index = user.Organizations.FindIndex( a => a.Id == organizationId );
                if ( index == -1 )
                {
                    var org = OrganizationServices.GetForSummary( organizationId );
                    if ( org != null && org.Id > 0 )
                    {
                        user.Organizations.Insert( 0, org );
                    }
                }
            }
            else
            {
                user.Organizations = tpOrgs;
            }

            ViewBag.BulkUploadType = bulkUploadType;
            if ( Request.Params[ "usev3" ] == "false" )
            {
                return View( "~/Views/V2/BulkUpload/SimpleV2.cshtml" );
            }
            if ( Request.Params[ "usev4" ] == "true" )
            {
                return View( "~/Views/V2/BulkUpload/SimpleV4.cshtml" );
            }

            AccountServices.AddUserToSession( System.Web.HttpContext.Current.Session, user );

            return View( "~/Views/V2/BulkUpload/SimpleV3.cshtml" );
        }
        //
        #region Credentials
        public ActionResult Simple( int organizationId = 0 )
        {
            return Credentials( organizationId );
        }
        //
        public ActionResult Credentials( int organizationId = 0 )
        {
            return BulkUploadPage( organizationId, "Credential" );
        }
        //
        //Uses raw CSV Data
        public JsonResult UploadRawCredentialCSV( string rawCSVData, string action, string owningOrganizationRowID )
        {
            var user = AccountServices.GetCurrentUser();
            var messages = new List<string>();
            var importer = new CredentialsUpload();
            //CredentialsUpload.doingExistanceCheck = false;
            var owningOrgID = 0;
            //no longer to be used, going to by guess and by golly
            action = ( action ?? "" ).Trim().ToLower();

            if ( importer.UploadCredentialsFromText( rawCSVData, action, user, owningOrganizationRowID, ref owningOrgID, ref messages ) == false )
            {
                LoggingHelper.WriteLogFile( 1, string.Format( "Credentials_upload_errors_{0}_{1}.csv", owningOrgID, DateTime.Now.ToString("hhmmss")), rawCSVData, "", false );
                return JsonResponse( null, false, "Error encountered during import", new { messages = messages } );
            }
            else
            {
                LoggingHelper.WriteLogFile( 1, string.Format( "Credentials_upload_success_{0}_{1}.csv", owningOrgID, DateTime.Now.ToString( "hhmmss" ) ), rawCSVData, "", false );
                //there will be summary messages as well
                return JsonResponse( null, true, "Credentials were uploaded successfully.", new { messages = messages, owningOrgID = owningOrgID } );
            }
        }
        //
        public JsonResult ExportCredentialsForOrganization( string organizationRowID )
        {
            var rawData = CredentialServices.GetAllForExport_DictionaryList( organizationRowID );
            return JsonHelper.GetJsonWithWrapper( rawData, true, "okay", null );
        }
        //
        public JsonResult ClearAllCredentials( string organizationRowID )
        {
            List<string> messages = new List<string>();
            var user = AccountServices.GetUserFromSession();

            //Delete credentials
            if ( new CredentialServices().DeleteAllForOrganization( user, organizationRowID, ref messages ) )
            {
                //Return result
                return JsonResponse( null, true, "okay", null );
            }
            else
            {
                return JsonResponse( null, false, "Error(s) encountered attempting to clear credentials", new { messages = messages } );
            }

        }
        #endregion

        #region Assessments
        public ActionResult Assessments( int organizationId = 0 )
        {
            return BulkUploadPage( organizationId, "Assessment" );
        }
        //Uses raw CSV Data
        public JsonResult UploadRawAssessmentCSV( string rawCSVData, string action, string owningOrganizationRowID )
        {
            var user = AccountServices.GetUserFromSession();
            var messages = new List<string>();
            var importer = new CredentialsUpload();
            var owningOrgID = 0;
            //no longer to be used, going to by guess and by golly
            action = ( action ?? "" ).Trim().ToLower();

            if ( new AssessmentsUpload().UploadFromText( rawCSVData, action, user, owningOrganizationRowID, ref owningOrgID, ref messages ) == false )
            {
                LoggingHelper.WriteLogFile( 1, string.Format( "Assessments_upload_errors_{0}_{1}.csv", owningOrgID, DateTime.Now.ToString("hhmmss") ), rawCSVData, "", false );
                return JsonResponse( null, false, "Error encountered during import", new { messages = messages } );
            }
            else
            {
                //save input file
                LoggingHelper.WriteLogFile( 1, string.Format( "Assessments_upload_success_{0}_{1}.csv", owningOrgID, DateTime.Now.ToString("hhmmss") ), rawCSVData, "", false );

                //there will be summary messages as well
                return JsonResponse( null, true, "Assessments were uploaded successfully.", new { messages = messages, owningOrgID = owningOrgID } );
            }
        }
        //

        public JsonResult ExportAssessmentsForOrganization( string organizationRowID )
        {
            var rawData = AssessmentServices.GetAllForExport_DictionaryList( organizationRowID );
            return JsonHelper.GetJsonWithWrapper( rawData, true, "okay", null );
        }
        //


        public JsonResult ClearAllAssessments( string organizationRowID )
        {
            List<string> messages = new List<string>();
            var user = AccountServices.GetUserFromSession();

            //Delete all for org
            if ( new AssessmentServices().DeleteAllForOrganization( user, organizationRowID, ref messages ) )
            {
                //Return result
                return JsonResponse( null, true, "okay", null );
            }
            else
            {
                return JsonResponse( null, false, "Error(s) encountered attempting to clear assessments", new { messages = messages } );
            }

        }
        //
        #endregion

        #region Learning Opps
        public ActionResult LearningOpportunities( int organizationId = 0 )
        {
            return BulkUploadPage( organizationId, "LearningOpportunity" );
        }
        public JsonResult UploadRawLoppCSV( string rawCSVData, string action, string owningOrganizationRowID )
        {
            var user = AccountServices.GetUserFromSession();
            var messages = new List<string>();
            var importer = new CredentialsUpload();
            var owningOrgID = 0;
            //no longer to be used, going to by guess and by golly
            action = ( action ?? "" ).Trim().ToLower();

            if ( new LearningOppsUpload().UploadFromText( rawCSVData, action, user, owningOrganizationRowID, ref owningOrgID, ref messages ) == false )
            {
                LoggingHelper.WriteLogFile( 1, string.Format( "Lopps_upload_errors_{0}_{1}.csv", owningOrgID, DateTime.Now.ToString("hhmmss") ), rawCSVData, "", false );
                return JsonResponse( null, false, "Error encountered during import", new { messages = messages } );
            }
            else
            {
                //save input file
                LoggingHelper.WriteLogFile( 1, string.Format( "Lopps_upload_success_{0}_{1}.csv", owningOrgID, DateTime.Now.ToString("hhmmss") ), rawCSVData, "", false );

                //there will be summary messages as well
                return JsonResponse( null, true, "Learning Opportunites were uploaded successfully.", new { messages = messages, owningOrgID = owningOrgID } );
            }
        }
        //
        public JsonResult ExportLoppsForOrganization( string organizationRowID )
        {
            var rawData = LearningOpportunityServices.GetAllForExport_DictionaryList( organizationRowID );
            return JsonHelper.GetJsonWithWrapper( rawData, true, "okay", null );
        }
        //


        public JsonResult ClearAllLopps( string organizationRowID )
        {
            List<string> messages = new List<string>();
            var user = AccountServices.GetUserFromSession();

            //Delete all for organization
            if ( new LearningOpportunityServices().DeleteAllForOrganization( user, organizationRowID, ref messages ) )
            {
                //Return result
                return JsonResponse( null, true, "okay", null );
            }
            else
            {
                return JsonResponse( null, false, "Error(s) encountered attempting to clear learning opportunities", new { messages = messages } );
            }

        }
        //
        #endregion



        public ActionResult Advanced()
        {
            return View( "~/Views/V2/BulkUpload/AdvancedV1.cshtml" );
        }
        //



        public JsonResult ParseCSV( string csv )
        {
            var result = new List<string[]>();
            using ( var reader = new StringReader( csv ) )
            //no overload for StringReader and Encoding: System.Text.Encoding.UTF7 
            using ( var parser = new TextFieldParser( reader ) )
            {
                parser.HasFieldsEnclosedInQuotes = true;
                parser.SetDelimiters( "," );
                while ( !parser.EndOfData )
                {
                    result.Add( parser.ReadFields() );
                }
            }

            return JsonResponse( result, true, "okay", null );
        }
        //
        public JsonResult ReadAndParseCSV( string filename )
        {
            var result = new List<string[]>();
            result = new CredentialsUpload().ReadFileToList( filename );
            return JsonResponse( result, true, "okay", null );
        }
        //
        public JsonResult GenerateCTID()
        {
            return JsonResponse( "ce-" + Guid.NewGuid(), true, "okay", null );
        }
        //

        //For some reason, HttpClient and MemoryCache don't show up in their respective namespaces when referenced in cshtml file, but they show up here
        public static string GetSchema( string url )
        {
            var cache = MemoryCache.Default;
            var schema = ( string ) cache.Get( url );
            if ( schema == null )
            {
                schema = new HttpClient().GetAsync( url ).Result.Content.ReadAsStringAsync().Result;
                cache.Remove( url );
                cache.Add( url, schema, new CacheItemPolicy() { AbsoluteExpiration = DateTime.Now.AddHours( 1 ) } );
            }
            return schema;
        }
        //

        //Uses Pre-parsed headers and rows - This method is not used right now
        public JsonResult UploadCSVData( List<string> headers, List<List<string>> rows )
        {
            //Each row is a List<string> where each value is a cell
            //Values have not been parsed here - for instance, you will still have things like "keyword 1|keyword 2|keyword 3"

            //Map to a List<Dictionary<string, string>> for now
            var holder = new List<Dictionary<string, string>>();
            foreach ( var row in rows )
            {
                var item = new Dictionary<string, string>();
                for ( var i = 0; i < headers.Count(); i++ )
                {
                    item.Add( headers[ i ], row[ i ] );
                }
                holder.Add( item );
            }


            return JsonResponse( null, true, "", null );
        }
        //




        public JsonResult UploadRawAddressCSV( string rawCSVData, string action, string owningOrganizationRowID )
        {
            var user = AccountServices.GetUserFromSession();
            var messages = new List<string>();
            var importer = new LocationsUpload();
            action = ( action ?? "" ).Trim().ToLower();

            if ( importer.UploadLocationsFromText( rawCSVData, action, user, ref messages ) )
            {
                return JsonResponse( null, true, "Addresses were uploaded successfully.", new { messages = messages } );
            }
            else
            {
                return JsonResponse( null, false, "One or more errors encountered uploading addresses.", new { messages = messages } );
            }
        }
        //

		public JsonResult LookupQAOrganizations( QALookupFilters filters, int pageNumber = 1 )
		{
			var total = 0;
			var valid = true;
			var status = "";
			var microQuery = new MicroSearchInputV2()
			{
				SearchType = "QAOrganizationLookup",
				PageNumber = pageNumber,
				PageSize = 10,
				Filters = new List<MicroSearchFilter>()
				{
					new MicroSearchFilter() { Name = "Name", Value = filters.Name },
					new MicroSearchFilter() { Name = "SubjectWebpage", Value = filters.SubjectWebpage }
				}
			};
			var results = MicroSearchServicesV2.DoMicroSearch( microQuery, ref total, ref valid, ref status );
			return JsonResponse( results, valid, status, total );
		}
        public JsonResult CredentialsSearch( BulkUploadFilters filters, int pageNumber, int pageSize = 20)
        {
            //
            var total = 0;
            var valid = true;
            var status = "";
            var microQuery = new MicroSearchInputV2()
            {
                SearchType = "CredentialSearch",
                PageNumber = pageNumber,
                PageSize = pageSize,
                Filters = new List<MicroSearchFilter>()
                {
                    new MicroSearchFilter() { Name = "Keywords", Value = filters.Name }
                }
            };
            if (filters.UseMyAssociatedOrgs)
            {
                microQuery.Filters.Add( new MicroSearchFilter()
                {
                    Name = "OrgFilters",
                    Value = "myOrgs"
                } );
            } else if ( filters.OrgId != Guid.Empty )
            {
                microQuery.Filters.Add( new MicroSearchFilter()
                {
                    Name = "OwningOrg",
                    Value = filters.OrgId.ToString()
                } );
            }
            var results = MicroSearchServicesV2.DoMicroSearch( microQuery, ref total, ref valid, ref status );
            return JsonResponse( results, valid, status, total );
        }
        public class QALookupFilters
		{
			public string Name { get; set; }
			public string SubjectWebpage { get; set; }
		}
        //
        public class BulkUploadFilters
        {
            public string Name { get; set; }
            public bool UseMyAssociatedOrgs { get; set; }
            public Guid OrgId { get; set; }
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
        }
        //

    }
}