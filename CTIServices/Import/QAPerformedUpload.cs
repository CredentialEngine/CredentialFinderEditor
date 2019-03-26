using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json;
using Factories;
using ImportMgr = Factories.Import.LearningOpportunityImport;
using DBMgr = Factories.Entity_AssertionManager;
using Models;
using Models.Common;
using ImportModel = Models.Import.QAPerformedDTO;
using ThisEntity = Models.ProfileModels.LearningOpportunityProfile;
using Models.Import;
using Utilities;

namespace CTIServices.Import
{
    public class QAPerformedUpload : BaseUpload
    {
        public string thisClassName = "QAPerformedUpload";
        #region properties
        //identifier, org ctid, name, type, desc, swp
        static int RequiredNbrOfColumns = 6;

        public QAPerformedRequest importHelper = new QAPerformedRequest();
        List<ImportModel> importList = new List<ImportModel>();
        //
        public ImportMgr importMgr = new ImportMgr();
        //
        public string prevAssertionsList = "";
        public Enumeration assertions = new Enumeration();
        //

        #endregion

        public bool UploadFromText( string inputText, string action, AppUser user, string owningOrganizationRowID, ref int owningOrgId, ref List<string> messages )
        {
            bool isOK = true;
            ImportModel import = new ImportModel();

            DateTime start = DateTime.Now;

            messages = new List<string>();
            //if owningOrganizationRowID is provided, then no owning org column is needed
            //NOTE: need to handle where could still exist in an older spreadsheet
            CheckForOwningOrg( owningOrganizationRowID, user, ref messages );

            if ( messages.Count > 0 )
                return false;

            if ( action == PARTIAL_UPDATE )
                IsPartialUpdate = true;

            int startingMsgCount = 0;
            try
            {
                using ( CsvReader csv =
                       new CsvReader( new StringReader( inputText ), true ) )
                {
                    int fieldCount = csv.FieldCount;

                    string[] headers = csv.GetFieldHeaders();
                    //validate headers
                    if ( !ValidateHeaders( headers, true, ref messages ) )
                    {
                        return false;
                    }

                    csv.SkipEmptyLines = true;
                    while ( csv.ReadNextRecord() )
                    {
                        rowNbr++;
                        if ( IsEmpty( csv, RequiredNbrOfColumns ) )
                        {
                            //brake assumes at end
                            warnings.Add( string.Format( "WARNING. Row {0} did not contain any data, skipped. ", rowNbr ) );
                            continue;
                        }
                        import = new ImportModel();
                        //
                        LoggingHelper.DoTrace( 7, string.Format( "UploadFromText. Row: {0}", rowNbr ) );
                        //HandleRecord( rowNbr, csv, import, user, owningOrganizationRowID, ref messages );

                        //that latter may handle field level validations
                        importList.Add( import );

                    }
                }
            }
            catch ( Exception ex )
            {
                if ( ex.Message.IndexOf( "item with the same key has already been added" ) > -1 )
                    messages.Add( "Error: An item with the same key has already been added - check for a column name being used more than once. " );
                else
                    messages.Add( "LOPP Upload - Error unexpected exception was encountered: " + ex.Message );
            }

            //reject all if any errors
            if ( messages.Count > 0 )
                return false;
            else if ( importList.Count < 1 )
            {
                messages.Add( "No useable data was found in the input file. Hmmmm" );
                return false;
            }
            //process
            var settings = new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented
            };

            try
            {
                string jsonList = JsonConvert.SerializeObject( importList, settings );
                LoggingHelper.WriteLogFile( 5, thisClassName + ".json", jsonList, "", false );
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".UploadFromText Issue with serializing importList error." );
                messages.Add( ex.Message );
            }

            ImportStatus status = new ImportStatus();
            try
            {
                //importMgr.Import( importList, user, ref status );

                TimeSpan timeDifference = DateTime.Now.Subtract( start );
                //note need better count to handle multiple rows per learning opportunity
                string stats = string.Format( "Upload Summary - Attempted: {0}, Added: {1}, Updated: {2}, Failed: {3}, elapsed: {4:N2} seconds ", rowNbr - 1, status.RecordsAdded, status.RecordsUpdated, status.RecordsFailed, timeDifference.TotalSeconds );

                ActivityManager activityMgr = new ActivityManager();
                activityMgr.SiteActivityAdd( new SiteActivity()
                {
                    ActivityType = "Organization",
                    Activity = "Bulk Upload",
                    Event = "LearningOpportunities",
                    Comment = string.Format( "Upload by {0}, of Organization: '{1}'. {2}.", user.FullName(), defaultOwningOrg.Name, stats ),
                    ActionByUserId = user.Id,
                    TargetObjectId = defaultOwningOrg.Id,
                    ActivityObjectParentEntityUid = prevOwningAgentUid
                } );

                if ( status.HasErrors )
                    stats += " NOTE: some errors were encountered.";
                //notify administration
                string url = UtilityManager.FormatAbsoluteUrl( string.Format( "~/summary/organization/{0}", defaultOwningOrg.Id ) );
                string message = string.Format( "New learning opportunity Bulk Upload. <p>{5}</p><ul><li>Organization Id: {0}</li><li>Organization: {1}</a></li><li>{2} </li><li>Uploaded By: {3}</li><li><a href='{4}'>Organization Summary: {1}</a></li></ul>", defaultOwningOrg.Id, defaultOwningOrg.Name, stats, user.FullName(), url, DateTime.Now );
                owningOrgId = defaultOwningOrg.Id; //Useful for other methods

                if ( status.RecordsAdded > 0 || status.RecordsUpdated > 0 )
					EmailServices.SendSiteEmail( "New learning opportunity Bulk Upload", message );
                string summaryPage = string.Format( "<a href='{0}' target='_summary'>Organization Summary</a>", url );

                LoggingHelper.DoTrace( 5, thisClassName + ".UploadFromText(). " + stats );
                messages.Add( summaryPage );
                messages.Add( stats );

                if ( warnings.Count > 0 )
                    messages.AddRange( warnings );

                messages.AddRange( status.GetAllMessages() );
                if ( status.HasErrors )
                {
                    isOK = false;
                    //save errors
                    LoggingHelper.DoTrace( 2, "Import. Errors encountered" + string.Join( "\\r\\n", messages.ToArray() ) );
                }
                else
                {
                    if ( status.Messages.Count > 0 )
                        LoggingHelper.DoTrace( 5, "Import" + string.Join( "\\r\\n", messages.ToArray() ) );
                }



            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, thisClassName + ".Upload Unexpected error." );
                messages.Add( ex.Message );
            }


            return isOK;
        }

        #region Validations
        public bool ValidateHeaders( string[] headers, bool doingMinimumRequiredChecks, ref List<string> messages )
        {
            bool isValid = true;
            if ( headers == null
                || ( !doingMinimumRequiredChecks && headers.Count() < RequiredNbrOfColumns )
                || ( doingMinimumRequiredChecks && headers.Count() < 4 )
                )
            {
                messages.Add( "Error - the input file must have a header row with at least the required columns" );
                return false;
            }
            int cntr = -1;

            try
            {
                #region Check header columns
                foreach ( var item in headers )
                {
                    cntr++;
                    string colname = item.ToLower().Replace( " ", "" ).Replace( "*", "" ).Replace( ":", "." );

                   
                    switch ( colname )
                    {
                        case "externalidentifier":
                        case "uniqueidentifier":
                        case "identifier":
                            importHelper.ExternalIdentifierHdr = cntr;
                            break;

                        case "artifacttype":
                            importHelper.ArtifactTypeHdr = cntr;
                            break;

                        case "action":
                            importHelper.ActionHdr = cntr;
                            break;
                        case "ctid":
                            //ctid will not be required if external identifier is present
                            importHelper.CtidHdr = cntr;
                            break;
                        case "name":
                            importHelper.NameHdr = cntr;
                            break;
                        case "description":
                            importHelper.DescHdr = cntr;
                            break;
                        case "existingorganizationctid": //always requiried, or if using unique identifier, currently a single value
                        case "orgctid":
                        case "ownedby":
                            importHelper.OrganizationCtidHdr = cntr;
                            break;
                        case "subjectwebpage":
                        case "webpage":
                            importHelper.SubjectWebpageHdr = cntr;
                            break;

                        case "assertionslist":
                        case "assertions":
                            importHelper.AssertionsHdr = cntr;
                            break;

                        //**** Ignore the following if included - a warning message would be helpful ****

                        case "advancedstandingfor.assertedby.name":
                        case "advancedstandingfor.assertedby.subjectwebpage":
                        case "advancedstandingfor.description":
                        case "comment":
                        case "comments":
                        case "costprofilecreated":
                        case "conditionprofilecreated":
                        case "targetcredentialnames":
                            //IGNORE
                            break;
                        default:
                            //action?
                            if ( colname.IndexOf( "column" ) > -1 )
                                break;
                            messages.Add( "Error unknown column header encountered: " + item );
                            break;
                    }
                }
                #endregion

                if ( importHelper.CtidHdr == -1 && importHelper.ExternalIdentifierHdr == -1 )
                    messages.Add( "Error - Either an identifier from the source system or a ctid must be provided to uniquely identify an input record." );
                if ( importHelper.OrganizationCtidHdr == -1
                && ( defaultOwningOrg == null || defaultOwningOrg.Id == 0 ) )
                    messages.Add( "Error - An owning organization CTID column (Owned By) must be provided (or set in the interface)." );

                //TBD - only if no ctid?
                if ( importHelper.NameHdr == -1 )
                    messages.Add( "Error - a target artifact name column must be provided" );

                if ( importHelper.ArtifactTypeHdr == -1 )
                    messages.Add( "Error - The Artifact type column/data must be provided." );
                if ( importHelper.AssertionsHdr == -1 )
                    messages.Add( "Error - The Assertions column/data must be provided." );
            }
            catch ( Exception ex )
            {
                string msg = BaseFactory.FormatExceptions( ex );
                LoggingHelper.DoTrace( 1, "Exception encountered will validating headers for learning opportunity upload: " + msg );
                messages.Add( "Exception encountered will validating headers for learning opportunity upload: " + msg );
            }
            if ( messages.Count > 0 )
                isValid = false;

            return isValid;
        }//
        #endregion
    }
}
