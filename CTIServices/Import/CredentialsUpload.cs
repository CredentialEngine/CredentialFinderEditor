using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using LumenWorks.Framework.IO.Csv;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using CTIServices;
using Factories;
using ImportMgr = Factories.Import.CredentialImport;
using Models;
using Models.Common;
using Models.Import;
using ImportModel = Models.Import.CredentialDTO;
using Models.ProfileModels;
using Utilities;

namespace CTIServices.Import
{
    public class CredentialsUpload : BaseUpload
    {
        #region properties
        //identifier, org ctid, name, type, desc, swp
        static int RequiredNbrOfColumns = 6;
        
        public CredentialImportRequest importHelper = new CredentialImportRequest();
        public AssessmentImportRequest asmtImportHelper = new AssessmentImportRequest();

        public LoppImportRequest loppImportHelper = new LoppImportRequest();
        //
        public ImportMgr importMgr = new ImportMgr();
        
        List<ImportModel> importList = new List<ImportModel>();

        public string prevCredType = "";
        Enumeration credentialType = new Enumeration();
        public string prevCredStatus = "";
        Enumeration credentialStatus = new Enumeration();
		//
		public string previousDeliveryTypes = "";
		public Enumeration prevDeliveryType = new Enumeration();
		public Enumeration thisDeliveryType = new Enumeration();

		public Enumeration prevLoppDeliveryType = new Enumeration();
		public Enumeration thisLoppDeliveryType = new Enumeration();
		public string previousLoppDeliveryTypes = "";

		public string prevCopyrightCtid = "-1";
        public Guid prevCopyrightGuid = new Guid();

        //public string prevLanguage = "";
        //public string prevDuration = "";
        public string prevSocList = "";
        public string prevNaicsList = "";

        

        #endregion
		/// <summary>
		/// Upload a list of credentials
		/// </summary>
		/// <param name="inputText"></param>
		/// <param name="action">Currently will not be provided, deriving from content</param>
		/// <param name="user"></param>
		/// <param name="owningOrganizationRowID"></param>
		/// <param name="owningOrgId"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
        public bool UploadCredentialsFromText( string inputText, string action, AppUser user, string owningOrganizationRowID, ref int owningOrgId, ref List<string> messages )
        {
            bool isOK = true;
            //string file = "C:\\Projects\\CTI\\CredentialRegistry\\Testing\\Data\\credentialsExport.csv";
            ImportModel import = new ImportModel();

            DateTime start = DateTime.Now;
            
            messages = new List<string>();
			importList = new List<ImportModel>();

			//if owningOrganizationRowID is provided, then no owning org column is needed
			//NOTE: need to handle where could still exist in an older spreadsheet
			CheckForOwningOrg(owningOrganizationRowID, user, ref  messages);
            
            if ( messages.Count > 0 )
                return false;
			owningOrgId = defaultOwningOrg.Id; //Useful for other methods

			//if ( action == PARTIAL_UPDATE )
   //             IsPartialUpdate = true;

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
						if ( rowNbr % 50 == 0 )
						{
							LoggingHelper.DoTrace( 5, string.Format( "UploadCredentialsFromText. Processing Row: {0}", rowNbr ) );
						}

						if ( IsEmpty( csv, RequiredNbrOfColumns ) )
                        {
                            //brake assumes at end
                            warnings.Add( string.Format( "WARNING. Row: {0} did not contain any data, skipped. ", rowNbr) );
                            continue;
                        }
                        
                        if ( rowNbr == 57)
                        {

                        }
                        //
						if ( csv [ 0 ] == "Instructions" || csv [ 0 ] == "Sample Data" )
						{
							continue;
						}
						LoggingHelper.DoTrace( 7, string.Format( "UploadCredentialsFromText. Row: {0}", rowNbr ) );
						
						var duplicateCheck = new ImportModel();
						//validate org. Must exist. This assigns org info that is referenced in CheckIdentifiers!!
						AssignOrgStuff( rowNbr, csv, duplicateCheck, user, importHelper, ref messages );
						
						CheckIdentifiers( rowNbr, csv, entity: duplicateCheck, entityType: "Credential", user: user, messages: ref messages );

						if ( duplicateCheck != null && duplicateCheck.IsPotentialPartialUpdate == false)
						{
							if (rowNbr > 2)
								importList.Add( import );

							import = new ImportModel()
							{
								Action = duplicateCheck.Action,
								IsPotentialPartialUpdate = false,
								CTID = duplicateCheck.CTID,
								ExternalIdentifier = duplicateCheck.ExternalIdentifier,
								OwningOrganizationCtid = duplicateCheck.OwningOrganizationCtid,
								OwningAgentUid = duplicateCheck.OwningAgentUid,
								ExistingRecord = duplicateCheck.ExistingRecord,
								ExistingParentId = duplicateCheck.ExistingParentId,
								IsExistingEntity = duplicateCheck.IsExistingEntity,
								ExistingParentRowId = duplicateCheck.ExistingParentRowId,
								OrganizationName = duplicateCheck.OrganizationName,
							};
						} else
						{
							//will append to current output record
							import.IsPotentialPartialUpdate = true;
						}

						HandleRecord( rowNbr, csv, import, user, owningOrganizationRowID, ref messages );

						//importList.Add( import );

					}
                }
            } catch(Exception ex)
            {
				if ( ex.Message.IndexOf( "item with the same key has already been added" ) > -1 )
					messages.Add( string.Format( "Row: {0} Error: An item with the same key has already been added - check for a column name being used more than once. ", rowNbr ) );
				else
				{
					messages.Add( string.Format( "Row: {0} CredentialsUpload - Error unexpected exception was encountered. System administration will be notified. ", rowNbr ) + ex.Message );
					LoggingHelper.LogError( ex, "CredentialsUpload.UploadCredentialsFromText", true );
				}
            }
			//add last record
			if ( import != null && !string.IsNullOrWhiteSpace( import.Name ) )
			{
				importList.Add( import );
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

			LoggingHelper.DoTrace( 5, string.Format( "UploadCredentialsFromText. Completed validation, starting import of: {0} records.", importList.Count() ) );
			try
            {
                string jsonList = JsonConvert.SerializeObject( importList, settings );
                LoggingHelper.WriteLogFile( 5, "CredentialsUpload.json", jsonList, "", false );
            }
            catch ( Exception ex )
            {
                LoggingHelper.LogError( ex, "CredentialsImport.UploadCredentials Issue with serializing importList error." );
                messages.Add( ex.Message );
            }
            ImportStatus status = new ImportStatus();
            try
            {
                importMgr.Import( importHelper, importList, user, ref status );

                TimeSpan timeDifference = DateTime.Now.Subtract( start );
                //note need better count to handle multiple rows per credential
                string stats = string.Format( "Upload Summary - Read: {0}, Unique Identifiers: {1} Added: {2}, Updated: {3}, Failed: {4}, elapsed: {5:N2} seconds ", rowNbr - 1, importList.Count(), status.RecordsAdded, status.RecordsUpdated, status.RecordsFailed, timeDifference.TotalSeconds );
                
                
                ActivityManager activityMgr = new ActivityManager();
                activityMgr.SiteActivityAdd(new SiteActivity()
                {
                    ActivityType = "Organization",
                    Activity = "Bulk Upload",
                    Event = "Credentials",
                    Comment = string.Format("Upload by {0}, of Organization: '{1}'. {2}.", user.FullName(), defaultOwningOrg.Name, stats),
                    ActionByUserId = user.Id,
                    TargetObjectId = defaultOwningOrg.Id,
                    ActivityObjectParentEntityUid = prevOwningAgentUid
                });
              
                if ( status.HasErrors )
                    stats += " NOTE: some errors were encountered.";
                //notify administration
                string url = UtilityManager.FormatAbsoluteUrl( string.Format("~/summary/organization/{0}", defaultOwningOrg.Id));
                string message = string.Format( "New Credential Bulk Upload.<p>{5}</p> <ul><li>Organization Id: {0}</li><li>Organization: {1}</a></li><li>{2} </li><li>Uploaded By: {3}</li><li><a href='{4}'>Organization Summary: {1}</a></li></ul>", defaultOwningOrg.Id, defaultOwningOrg.Name, stats, user.FullName(), url, DateTime.Now );
				
                int pTotalRows = 0;
                
                if (status.RecordsAdded > 0 || status.RecordsUpdated > 0)
                {
					EmailServices.SendSiteEmail( "New Credential Bulk Upload", message);
                }
                string summaryPage = string.Format("<a href='{0}' target='_summary'>Organization Summary</a>",url);

                LoggingHelper.DoTrace( 5, "CredentialsUpload.UploadCredentialsFromText(). " + stats );
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
                LoggingHelper.LogError( ex, "CredentialsImport.UploadCredentials Unexpected error during Import step." );
                messages.Add( ex.Message );
            }


            return isOK;
        }
        
        /// <summary>
        /// Process a row
        /// </summary>
        /// <param name="rowNbr"></param>
        /// <param name="csv"></param>
        /// <param name="entity"></param>
        /// <param name="isPartialUpdate">IsPartialUpdate will be false for now, this method will have to set on a row by row basis</param>
        /// <param name="user"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public bool HandleRecord( int rowNbr, CsvReader csv, ImportModel entity, AppUser user, string owningOrganizationRowID, ref List<string> messages )
        {
            bool isValid = true;
            bool isPartialUpdate = false;
            entity.RowNumber = rowNbr;

			//validate org. Must exist. This assigns org info that is referenced in CheckIdentifiers!!
			//AssignOrgStuff( rowNbr, csv, entity, user, importHelper, ref messages );

			//NOTE: currently don't have a means for identifying partials. This method will attempt to set entity.IsPotentialPartialUpdate true if credential exists.
			//CheckIdentifiers( rowNbr, csv, entity, user, ref messages );
            if ( entity.IsPotentialPartialUpdate )
            {
                //if set true, need to validate as an intended partial, or an actual error. 
                //more likely an error if for new credential
                //check for explicit append
                if ( entity.Action == "append" )
                {

                }
            } else
			{
				//may add previous record to the list, and initialize
			}
			//**** isPartialUpdate is not really implemented, so always set false
			//entity.IsPotentialPartialUpdate = false;

            entity.Name = Assign( rowNbr, csv, importHelper.NameHdr, "Credential Name", ref messages, "", true );
            if ( entity.Action == "append" || entity.IsPotentialPartialUpdate )
            {
                //just skip columns that should not be included
                //identifier check will confirm that append can only be used if there is a preceding New or Update
                entity.CredentialType = credentialType;
            }
            else
            {
                entity.Description = Assign( rowNbr, csv, importHelper.DescHdr, "Credential Description", ref messages, "", ( entity.IsPotentialPartialUpdate ? false : true ), MinimumDescriptionLength );
                //just in case
                if ( entity.IsPotentialPartialUpdate&& entity.Description == DELETE_ME )
                    messages.Add(string.Format("Row: {0} The credential description cannot be deleted.", rowNbr));

                entity.SubjectWebpage = AssignUrl( rowNbr, csv, importHelper.SubjectWebpageHdr, "Credential Subject webpage", ref messages, "", ( entity.IsPotentialPartialUpdate ? false : true ) );
                if ( entity.IsPotentialPartialUpdate && entity.SubjectWebpage == DELETE_ME )
                    messages.Add(string.Format("Row: {0} The Credential Subject Webpage cannot be deleted.", rowNbr));

                //check for duplicates based on name/owner and SWP
				//
                CheckForDuplicates( rowNbr, csv, entity, user, ref messages, ref isPartialUpdate );

                //Type special handling
                AssignCredentialType( rowNbr, csv, entity, entity.IsPotentialPartialUpdate, user, ref messages );

               
                //status - if not found will default to active in update method
                if ( importHelper.StatusHdr > -1 )
                {
                    entity.CredentialStatusSchema = Assign( rowNbr, csv, importHelper.StatusHdr, "Credential Status", ref messages, "", false );
                    if ( entity.CredentialStatusSchema == DELETE_ME )
                    {
                        messages.Add( string.Format( "Row: {0} The credential status cannot be deleted.", rowNbr ) );

                    }
                    else if ( prevCredStatus == entity.CredentialStatusSchema )
                    {
                        entity.CredentialStatus = credentialStatus;
                    }
                    else
                    {
                        credentialStatus = CodesManager.GetCodeAsEnumeration( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_STATUS_TYPE, entity.CredentialStatusSchema );
                        if ( credentialStatus == null || credentialStatus.Items == null || credentialStatus.Items.Count == 0 )
                        {
                            messages.Add( string.Format( "Row: {0} Invalid credential status of {1}", rowNbr, entity.CredentialStatusSchema ) );
                        }
                        else
                        {
                            prevCredStatus = entity.CredentialStatusSchema;
                            entity.CredentialStatus = credentialStatus;
                        }
                    }
                }

                //duration
                string durationDescr = "";
                if ( importHelper.DurationDescHdr > -1 )
                {
                    //NOTE: at this time, the duration is deleted, and then re-added, so suppress #DELETE
                    //WHOA  - if only description is entered, then need to allow delete value
                    //      - if have duration, and this is DELETE, THEN set blank
                    durationDescr = AssignProperty( rowNbr, csv, importHelper.DurationDescHdr, "DurationDescription", ref messages );
                    if ( importHelper.DurationHdr == -1 )
                    {
                        if ( durationDescr == DELETE_ME )
                            entity.DeleteEstimatedDuration = true;
                        else 
                            entity.EstimatedDuration.Description = durationDescr ?? "";
                    }
                }
                if ( importHelper.DurationHdr > -1 )
                {
                    string input = csv[ importHelper.DurationHdr ];
                    if ( input == DELETE_ME )
                        entity.DeleteEstimatedDuration = true;
                    else
                    {
                        entity.EstimatedDuration = AssignDuration( rowNbr, csv, importHelper.DurationHdr, ref messages );
                        if ( entity.HasEstimatedDuration )
                        {
                            entity.EstimatedDuration.Description = durationDescr == DELETE_ME ? "" : durationDescr;
                        } else
                            entity.EstimatedDuration.Description = durationDescr ?? "";

                    }
                }
                if ( importHelper.RenewalFrequencyHdr > -1 )
                {
                    string input = csv[ importHelper.RenewalFrequencyHdr ];
                    if ( input == DELETE_ME )
                        entity.DeleteRenewalFrequency = true;
                    else
                    {
                        entity.RenewalFrequency = AssignDuration( rowNbr, csv, importHelper.RenewalFrequencyHdr, ref messages, "", false, true );
                        //no description yet
                        if ( entity.HasRenewalFrequency )
                        {
                            entity.RenewalFrequency.DurationProfileTypeId = 3;
                            //entity.RenewalFrequency.Description = durationDescr == DELETE_ME ? "" : durationDescr;
                        }
                        else
                        {
                            //entity.EstimatedDuration.Description = durationDescr ?? "";
                        }
                    }
                }
                entity.AlternateName = AssignProperty( rowNbr, csv, importHelper.AlternateNameHdr, "AlternateName", ref messages );
                //-------------------------
                if ( importHelper.AudienceLevelHdr > -1 )
                    AssignAudienceLevels( rowNbr, csv, entity, user, ref messages );
                if ( importHelper.AudienceTypeHdr > -1 )
                    AssignAudienceTypes(rowNbr, csv, entity, user, importHelper.AudienceTypeHdr, ref messages);

                entity.AvailabilityListing = AssignUrl( rowNbr, csv, importHelper.AvailabilityListingHdr, "AvailabilityListing", ref messages );
                entity.AvailableOnlineAt = AssignUrl( rowNbr, csv, importHelper.AvailableOnlineAtHdr, "AvailableOnlineAt", ref messages );
                entity.CodedNotation = AssignProperty( rowNbr, csv, importHelper.CodedNotationHdr, "CodedNotation", ref messages );

				//               
				entity.AssessmentDeliveryTypeList = AssignEnumeration( rowNbr, csv, user, importHelper.AssessmentDeliveryTypeHdr, "AssessmentDeliveryType", CodesManager.PROPERTY_CATEGORY_ASMT_DELIVERY_TYPE, ref prevDeliveryType, ref thisDeliveryType, ref previousDeliveryTypes, ref messages );
				if ( !string.IsNullOrWhiteSpace( entity.AssessmentDeliveryTypeList ) )
				{
					if ( entity.AssessmentDeliveryTypeList != DELETE_ME )
						entity.AssessmentDeliveryType = thisDeliveryType;
                    
                }
				//
				entity.LearningDeliveryTypeList = AssignEnumeration( rowNbr, csv, user, importHelper.LearningDeliveryTypeHdr, "LearningDeliveryType", CodesManager.PROPERTY_CATEGORY_DELIVERY_TYPE, ref prevLoppDeliveryType, ref thisLoppDeliveryType, ref previousLoppDeliveryTypes, ref messages );
				if ( !string.IsNullOrWhiteSpace( entity.LearningDeliveryTypeList ) )
				{
					if ( entity.LearningDeliveryTypeList != DELETE_ME )
						entity.LearningDeliveryType = thisLoppDeliveryType;
				}

				entity.CredentialId = AssignProperty( rowNbr, csv, importHelper.CredentialIdHdr, "CredentialId", ref messages );
                entity.VersionIdentifier = AssignProperty( rowNbr, csv, importHelper.VersionIdentifierHdr, "CredentialId", ref messages );
                if ( importHelper.DateEffectiveHdr > -1 )
                {
                    entity.DateEffective = AssignDate( rowNbr, csv, importHelper.DateEffectiveHdr, "Date Effective", ref messages );
                }

                entity.ImageUrl = AssignImageUrl( rowNbr, csv, importHelper.ImageUrlHdr, "ImageUrl", ref messages );
                //should be conditionally required
                //probably want to validate now, not later
                //entity.InLanguage = AssignProperty( rowNbr, csv, importHelper.InLanguageSingleHdr, "LanguageSingle", ref messages, "English", false );
                entity.LanguageCodeList = AssignLanguages(rowNbr, csv, importHelper.InLanguageHdr, "Languages", ref messages, false);

                entity.Keywords = AssignList( rowNbr, csv, importHelper.KeywordsHdr, "Keywords", ref messages );
                entity.Subjects = AssignList( rowNbr, csv, importHelper.SubjectsHdr, "Subjects", ref messages );

                entity.DegreeMajors = AssignList( rowNbr, csv, importHelper.DegreeMajorsHdr, "DegreeMajors", ref messages );
                entity.DegreeMinors = AssignList( rowNbr, csv, importHelper.DegreeMinorHdr, "DegreeMinor", ref messages );
                entity.DegreeConcentrations = AssignList(rowNbr, csv, importHelper.DegreeConcentrationHdr, "DegreeConcentration", ref messages);

				//frameworks =================
				entity.NaicsCodesList = AssignNAICSList( rowNbr, csv, importHelper.NaicsListHdr, "NAICS", entity, ref messages );
				entity.Industries = AssignList( rowNbr, csv, importHelper.IndustriesHdr, "Industries", ref messages );

				entity.OnetCodesList = AssignSOCList( rowNbr, csv, importHelper.OnetListHdr, "SOC", entity, ref messages );
				entity.Occupations = AssignList( rowNbr, csv, importHelper.OccupationsHdr, "Occupations", ref messages );

				//need to validate the codes
				entity.CIPCodesList = AssignCIPList( rowNbr, csv, importHelper.CIPListHdr, "CIP", entity, ref messages );
				entity.Programs = AssignList( rowNbr, csv, importHelper.ProgramListHdr, "Programs", ref messages );

				entity.LatestVersion = AssignUrl( rowNbr, csv, importHelper.LatestVersionHdr, "LatestVersion", ref messages );
                entity.PreviousVersion = AssignUrl( rowNbr, csv, importHelper.PreviousVersionHdr, "PreviousVersion", ref messages );
                entity.ProcessStandards = AssignUrl( rowNbr, csv, importHelper.ProcessStandardsHdr, "ProcessStandards", ref messages );
                entity.ProcessStandardsDescription = AssignProperty( rowNbr, csv, importHelper.ProcessStandardsDescHdr, "ProcessStandardsDescription", ref messages );
                HandleCopyrightHolder(rowNbr, csv, entity, user, ref messages);

                AssignOfferedByList( rowNbr,csv, importHelper.OfferedByListHdr, entity, user, ref messages );
                

                AssignRecognizedByList( rowNbr, csv, importHelper.RecognizedByListHdr, entity, user, ref messages );


                //approved by
                if ( importHelper.ApprovedByListHdr > -1 )
                    AssignApprovedByList( rowNbr, csv, importHelper.ApprovedByListHdr, entity, user, ref messages );
                //accredited by
                if ( importHelper.AccreditedByListHdr > -1 )
                    AssignAccreditedByList( rowNbr, csv, importHelper.AccreditedByListHdr, entity, user, ref messages );
                AssignRegulatedByList( rowNbr, csv, importHelper.RegulatedByListHdr, entity, user, ref messages );

                //addresses
                if ( importHelper.AvailableAtHdr > 1 && importHelper.AvailableAtCodesHdr > 1 )
                {
                    //error, only allow one type
                    messages.Add( string.Format( "Row: {0}. You cannot use both Available At (column: {1}), and Available At Codes (column: {2}). Remove one of the columns and try again. ", rowNbr, importHelper.AvailableAtHdr, importHelper.AvailableAtCodesHdr ) );
                }
                else if ( importHelper.AvailableAtHdr > 1 )
                    AssignAddresses( rowNbr, csv, entity, user, importHelper,ref messages );
                else if (importHelper.AvailableAtCodesHdr > 1)
                    AssignAddressesViaCodes( rowNbr, csv, entity, user, importHelper, ref messages );
            }

            //conditions
            if ( importHelper.HasConditionProfile )
                AssignConditionProfile( rowNbr, csv, entity, 1, user, importHelper, ref messages );

			//connections
			if ( importHelper.HasConnectionProfile )
				AssignConnectionProfile( rowNbr, csv, entity, 1, user, importHelper, ref messages );

			if (importHelper.HasAssessment )
            {

            }
            if ( loppImportHelper.HasLearningOpp )
            {

            }
            //AssignCosts
            if ( importHelper.HasCosts )
            {
                //AssignCosts(rowNbr, csv, entity, user, ref messages);
				AssignCosts( rowNbr, csv, entity, user, importHelper, entity.FoundExistingRecord, 1, entity.ExistingRecord.RowId, ref messages );

				//entity.CostProfile = ReturnCosts(rowNbr, csv, entity, user, importHelper, entity.IsExistingCredential, entity.ExistingCredential.RowId, ref messages);

			}
           
            //other validations
            if ( importHelper.CommonConditionsHdr > 1 )
                AssignCommonConditions( rowNbr, csv, entity, user, importHelper, ref messages );
            if ( importHelper.CommonCostsHdr > 1 )
                AssignCommonCosts( rowNbr, csv, entity, user, importHelper, ref messages );



            if ( messages.Count > 0 )
                isValid = false;

            return isValid;
        }


        private void CheckIdentifiers( int rowNbr, CsvReader csv, ImportModel entity, string entityType, AppUser user, ref List<string> messages)
        {
            bool hasIdentifier = false;
            bool hasCtid = false;
            int startingCount = messages.Count;
            entity.Action = "missing";
            if ( importHelper.ActionHdr > -1 )
            {
                entity.Action = Assign( rowNbr, csv, importHelper.ActionHdr, "Action", ref messages, "" ).ToLower();
                if ("new update append ".IndexOf(entity.Action.ToLower()) == -1)
                {
                    messages.Add( string.Format( "Row: {0} Invalid Action property: {1}. Valid values are: Add, Update, Append (used for additional rows for the {2} record).", rowNbr, entity.ExternalIdentifier, entityType ) );
                } else
                {
					//can only have append, if there is a preceding New/Update
					//how to check? - probably on return 
					if ( entity.Action == "append" )
						entity.IsPotentialPartialUpdate = true;

				}
            } //
			//get the name for use in comparisons
			entity.Name = Assign( rowNbr, csv, importHelper.NameHdr, entityType + " Name", ref messages, "", true );

			if ( importHelper.ExternalIdentifierHdr > -1 )
            {
                bool isRequired = true;
                if ( importHelper.CtidHdr > -1 )
                    isRequired = false;
                entity.ExternalIdentifier = Assign( rowNbr, csv, importHelper.ExternalIdentifierHdr, entityType + " ExternalIdentifier", ref messages, "", isRequired );

                if ( entity.ExternalIdentifier == DELETE_ME )
                {
                    //could allow for an existing record
                    //actually this would be used to delete a credentials, along with CTID
                    messages.Add( string.Format( "Row: {0} The external identifier cannot be deleted {1}", rowNbr, entity.ExternalIdentifier ) );
                }
                else if ( entity.ExternalIdentifier.Length > 50 )
                {
                    messages.Add( string.Format( "Row: {0} The external identifier {1} must be less than or equal to 50 characters in length",rowNbr,entity.ExternalIdentifier ) );
                }
                else if ( !string.IsNullOrWhiteSpace( entity.ExternalIdentifier ) )
                {
                    hasIdentifier = true;
                    //need a validation that external identifier is not used more than once
                    int index = externalIdentifiers.FindIndex( a => a == entity.ExternalIdentifier );
                    var exists = importList.FirstOrDefault( a => a.ExternalIdentifier == entity.ExternalIdentifier );
                    if ( index > -1 || (exists != null && exists.FoundExistingRecord ))
                    {
						//dups of existing are OK to allow for multiple child profiles
						//but need to also check for accidential duplicates
						if ( exists != null && exists.Name != entity.Name )
						{
							//flag external identifier as a duplicate
							messages.Add( string.Format( "Row: {0} The external identifier '{1}' has been previously used for a different {2} name '{3}'. This external identifier should probably be changed.", rowNbr, entity.ExternalIdentifier, entityType, exists.Name ) );
						}
						else
						{
							//or flag as duplicate and do a reasonableness check
							//can't set this until validated
							entity.Action = "append"; //???
													  //actually a potential partial that needs validating
							entity.IsPotentialPartialUpdate = true;
						}
                    }
                    else
                    {
                        if ( entity.Action == "append" )
                        {
                            //can only have append, if there is a preceding New/Update
                            messages.Add( string.Format( "Row: {0} Error the current record has an Action of 'Append', but the External Identifier: {1}, has not been used in a previous row. Append can only be used with a previous row that is for a full {2}.", rowNbr, entity.ExternalIdentifier, entityType ) );
                        }
                        else
                        {
                            externalIdentifiers.Add( entity.ExternalIdentifier );
                            //may need to do a lookup, if no CTID? While CTID is in export, an update could be triggered from an external source
                        }

                    }
                }
            }
            //may want to allow both
            if ( importHelper.CtidHdr > -1 )
            {
                bool isRequired = true;
                //OR should it be required, if in the header?
                //if we also have an external identifier, then what
                if ( !string.IsNullOrWhiteSpace( entity.ExternalIdentifier) )
                    isRequired = false;
                entity.CTID = Assign( rowNbr, csv, importHelper.CtidHdr, entityType + " CTID", ref messages, "", isRequired );
                if ( !string.IsNullOrWhiteSpace( entity.CTID ) )
                {
                    hasCtid = true;
                    //need a validation that a CTID is not used more than once
                    //18-05-07 - actually may be possible for updates - for example for multiple conditions
                    //          - could allow if the cred name is same for each
                    int index = ctidsList.FindIndex( a => a == entity.CTID );
                    var exists = importList.FirstOrDefault( a => a.CTID == entity.CTID );
					//Darn, primary record will not have been added yet!
                    if ( index > -1 || (exists != null && exists.FoundExistingRecord ))
                    {
						//dups of existing are OK
						//but need to also check for accidential duplicates
						//if this is the first duplicate record the previous has not been added to importList yet
						if ( exists != null && exists.Name != entity.Name )
						{
							//flag external identifier as a duplicate
							messages.Add( string.Format( "Row: {0} The CTID '{1}' has been previously used for a different {2} name '{3}'. This CTID should probably be changed.", rowNbr, entity.CTID, entityType, exists.Name ) );
						}
						else
						{
							entity.Action = "append"; 
							entity.IsPotentialPartialUpdate = true;
						}
					}
                    else if ( entity.Action == "append" )
                    {
                        //can only have append, if there is a preceding New/Update
                        messages.Add( string.Format( "Row: {0} Error the current record has an Action of 'Append', but the CTID: {1}, has not been used in a previous row. Append can only be used with a previous row that is for a full {2}.", rowNbr, entity.CTID, entityType ) );
                    }
                    else
                    {
                        ctidsList.Add( entity.CTID );
                    }
                }
            }
            if ( !hasCtid && !hasIdentifier )
            {
                messages.Add( string.Format( "Row: {0} Error either a CTID and/or a unique external identifier must be provided.", rowNbr ) );
            }
            //if any new messages, exit
            if ( startingCount < messages.Count )
                return;

            if ( !string.IsNullOrWhiteSpace( entity.OwningOrganizationCtid ) )
            {
                if ( hasCtid )
                {
					//this should not be necessary if doing an append. Could pass the previous record as well
                    entity.ExistingRecord = CredentialManager.GetByCtid( entity.CTID );
                    if ( entity.FoundExistingRecord )
                    {
                        entity.IsExistingEntity = true;
						entity.ExistingParentId = entity.ExistingRecord.Id;

						entity.ExistingParentRowId = entity.ExistingRecord.RowId;
                        //TODO - actually should only set true where exists and in previous row.
                        //isPartialUpdate = true;
                        if ( entity.ExistingRecord.OwningAgentUid != entity.OwningAgentUid )
                        {
                            messages.Add( string.Format( "Row: {0} Error a {3} CTID was provided for an existing {3}, and it has a different owning organization {1} than that designated for this upload {2}.", rowNbr, entity.ExistingRecord.OrganizationName, entity.OrganizationName, entityType ) );
                        }
                    }
                }
                else //must have unique id to get here
                {
                    //do we have a check that identifier is unique to org
                    //Warning: if credential is copied, then combination of unique Id, and org Uid will not exist, but may be invalid. This is more likely in the test environment. 
                    entity.ExistingRecord = CredentialManager.GetBasicByUniqueId( entity.ExternalIdentifier, entity.OwningAgentUid );
                    if ( entity.FoundExistingRecord )
                    {
                        entity.IsExistingEntity = true;
						entity.ExistingParentId = entity.ExistingRecord.Id;
						entity.ExistingParentRowId = entity.ExistingRecord.RowId;
                        entity.CTID = entity.ExistingRecord.CTID;
                        //TODO - actually should only set true where exists and in previous row.
                        //isPartialUpdate = true;
                    }

                }
            }
        }

        private void CheckForDuplicates( int rowNbr, CsvReader csv, ImportModel entity, AppUser user, ref List<string> messages, ref bool isPartialUpdate )
        {
            //although could be changing SWP for an existing cred
            //|| entity.IsExistingCredential
            if ( string.IsNullOrWhiteSpace( entity.SubjectWebpage ) )
                return;
            //skip if an example url
            if ( entity.SubjectWebpage.ToLower().IndexOf( "example.com" ) > -1 
                || entity.SubjectWebpage.ToLower().IndexOf("google.com") > -1 )
                return;
            //normalize
            string url = CredentialManager.NormalizeUrlData( entity.SubjectWebpage );
            url = ServiceHelper.HandleApostrophes( url );

            int ptotalRows = 0;
            //search does a like (trims a trailing /), so need to check carefully
            //should actually trim off the protocol
            List<CredentialSummary> exists = CredentialManager.SearchByUrl( url, "", 1, 100, ref ptotalRows );
            foreach ( var item in exists )
            {
                string resultUrl = CredentialManager.NormalizeUrlData( item.SubjectWebpage );
                if ( resultUrl.ToLower() == url.ToLower() )
                {
                    if ( entity.FoundExistingRecord && entity.ExistingRecord.Id == item.Id )
                    {
                        //check if changed owning org
                        if ( item.OwningAgentUid.ToLower() != entity.OwningAgentUid.ToString().ToLower() )
                        {
                            //don't allow, at least until can do more stringent checks, like is user related to previous org
                            if ( OrganizationServices.CanUserUpdateOrganization( user, entity.OwningAgentUid ) == false )
                            {
                                messages.Add( string.Format( "Row: {0}. Issue: Attempting to change the owning organization for an existing credential. The entered credential already exists with an owning organization of '{1}'. You do not have update rights for the latter organization and so the system cannot allow you to change the owning organization to: '{2}'. ", rowNbr, item.OwnerOrganizationName, defaultOwningOrg.Name ) );
                            }
                        }
                    }
                    else
                    {
                        string msg = "";
                        if ( item.OwningAgentUid.ToLower() != entity.OwningAgentUid.ToString().ToLower() )
                        {
                            //if ( isProduction )
                            //    messages.Add( string.Format( "Row: {0}. The subject webpage for this credential already exists for credential: '{1}' (#{2}), with a different owning organization of '{3}' . This appears to be an invalid scenario. ", rowNbr, item.Name, item.Id, item.OwnerOrganizationName ) );
                            //else
                                warnings.Add( string.Format( "Row: {0}. WARNING. The subject webpage for this credential already exists for credential: '{1}' (#{2}), with a different owning organization of '{3}' . Please make sure that this is not an invalid scenario. ", rowNbr, item.Name, item.Id, item.OwnerOrganizationName ) );
                        } else
                        {
                            if ( entity.Name == item.Name )
                            {
                                //if ( isProduction )
                                //    messages.Add( string.Format( "Row: {0}. The subject webpage for this credential already exists for credential with the same name: '{1}' (#{2}). This appears to be a duplicate. ", rowNbr, item.Name, item.Id ) );
                                //else
                                    warnings.Add( string.Format("Row: {0}. WARNING. The subject webpage for this credential already exists for credential with the same name: '{1}' (#{2}). Please make sure that this is not a duplicate. ", rowNbr, item.Name, item.Id ) );
                            }
                            //else
                            //    messages.Add( string.Format( "Row: {0}. The subject webpage for this credential already exists for credential: '{1}' (#{2}). This appears to be a duplicate. ", rowNbr, item.Name, item.Id ) );
                        }
                    }
                    break;
                }
                else if ( item.SubjectWebpage.ToLower().IndexOf( url ) > -1 )
                {
                    //now what if close? some trailing parameter
                    LoggingHelper.DoTrace( 2, string.Format( "*** Bulk upload WARNING possible duplicates. upload.Name: {0}, upload.SubjectWebpage: {1}, existing.Name: {2}, existing.Id: {3}, existing.SubjectWebpage: {4}, organizationId: {5}", entity.Name, entity.SubjectWebpage, item.Name, item.Id, item.SubjectWebpage, defaultOwningOrg.Id ));
                }



            }
        }
        #region Assignments
        public void AssignCredentialType( int rowNbr, CsvReader csv, ImportModel entity, bool isPartialUpdate, AppUser user, ref List<string> messages )
        {
            if ( importHelper.TypeHdr < 1 )
            {
                messages.Add( string.Format( "Row: {0} The credential type is a required property.", rowNbr ) );
                return;
            }
            entity.CredentialTypeSchema = Assign( rowNbr, csv, importHelper.TypeHdr, "Credential Type", ref messages, "", true );
            if ( entity.CredentialTypeSchema == DELETE_ME )
            {
                messages.Add( string.Format( "Row: {0} The credential type cannot be deleted.", rowNbr ) );

            }
            else if ( string.IsNullOrWhiteSpace( entity.CredentialTypeSchema )
                && ( isPartialUpdate || entity.Action == "append" ) )
            {
                //OK
                entity.CredentialType = credentialType;
            }
            else if ( prevCredType == entity.CredentialTypeSchema )
            {
                entity.CredentialType = credentialType;
            }
            else
            {
                credentialType = CodesManager.GetCodeAsEnumeration( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE, entity.CredentialTypeSchema );
                if ( credentialType == null || credentialType.Items == null || credentialType.Items.Count == 0 )
                {
                    messages.Add( string.Format( "Row: {0} Invalid credential type of {1}", rowNbr, entity.CredentialTypeSchema ) );
                }
                else
                {
                    prevCredType = entity.CredentialTypeSchema;
                    entity.CredentialType = credentialType;
                }
            }
        }

        public void AssignAudienceLevels(int rowNbr, CsvReader csv, ImportModel entity, AppUser user, ref List<string> messages)
        {
            entity.AudienceLevelTypesList = AssignProperty(rowNbr, csv, importHelper.AudienceLevelHdr, "AudienceLevelTypes", ref messages);
            if ( string.IsNullOrWhiteSpace(entity.AudienceLevelTypesList) || entity.AudienceLevelTypesList == DELETE_ME )
                return;

            if ( prevAudienceLevelType == entity.AudienceLevelTypesList )
            {
                entity.AudienceLevelType = audienceLevelType;
            }
            else
            {
                audienceLevelType = new Enumeration() { Id = CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL };
                var itemList = entity.AudienceLevelTypesList.Split('|');
                foreach ( var item in itemList )
                {
                    EnumeratedItem ei = CodesManager.GetCodeAsEnumerationItem(CodesManager.PROPERTY_CATEGORY_AUDIENCE_LEVEL, item);
                    if ( ei != null && ei.Id > 0 )
                        audienceLevelType.Items.Add(ei);
                    else
                        messages.Add(string.Format("Row: {0} Invalid audience level type of {1}", rowNbr, item));
                }

                if ( audienceLevelType != null && audienceLevelType.Items.Count > 0 )
                {
                    entity.AudienceLevelType = audienceLevelType;
                    prevAudienceLevelType = entity.AudienceLevelTypesList;
                }

            }
        }


        private void HandleCopyrightHolder( int rowNbr, CsvReader csv, ImportModel entity, AppUser user, ref List<string> messages )
        {
            if ( importHelper.Copyrightholder_CtidHdr > -1 )
            {
                Organization copyrighter = new Organization();
                //can be ctid or name~url pair
                string holder = Assign( rowNbr, csv, importHelper.Copyrightholder_CtidHdr, "Copyrightholder", ref messages, "", false );
                if ( string.IsNullOrWhiteSpace( holder ) )
                    return;
                //same as owner
                if ( holder == entity.OwningOrganizationCtid )
                    entity.CopyrightHolder = entity.OwningAgentUid;
                else if ( holder.ToLower() == SAME_AS_OWNER )
                    entity.CopyrightHolder = entity.OwningAgentUid;
                else
                {
                    if ( prevCopyrightCtid == holder )
                    {
                        entity.CopyrightHolder = prevCopyrightGuid;
                    }
                    else if ( holder == DELETE_ME )
                    {
                        entity.DeleteCopyrightHolder = true;
                    }
                    else
                    {
                        //if different org, then look up
                        prevCopyrightCtid = holder;
                        if ( ServiceHelper.IsValidCtid( holder, ref messages ) )
                        {
                            copyrighter = OrganizationManager.GetByCtid( holder );
                            if ( copyrighter == null || copyrighter.Id == 0 )
                            {
                                messages.Add( string.Format( "Row: {0}. An organization was not found for the provided CTID for copyright holder: {1}", rowNbr, holder ) );
                            }
                            else
                            {
                                entity.CopyrightHolder = copyrighter.RowId;
                                prevCopyrightGuid = copyrighter.RowId;
                            }

                        }
                        else
                        {
                            if ( holder.IndexOf( "~" ) == -1 )
                            {
                                messages.Add( string.Format( "Row: {0}. Invalid data found for copyright holder. Please provide the CTID of an existing organization or a single Organization~webpage pair: {1}", rowNbr, holder ) );
                            }
                            else if ( HandleOrganizationReference( rowNbr, 0, "Copyright Holder", holder.Trim(), entity, user, ref messages, ref copyrighter ) )
                            {
                                entity.CopyrightHolder = copyrighter.RowId;
                                prevCopyrightGuid = copyrighter.RowId;
                            }

                        }
                    }
                }
            }
        }//


		[Obsolete]
        public void AssignCosts(int rowNbr, CsvReader csv, CredentialDTO entity, AppUser user, ref List<string> messages)
        {
            string status = "";
            CostProfileDTO costProfile = new CostProfileDTO();
            CostProfile existingProfile = new CostProfile();
            string identifier = "";
            string externalIdentifier = "";
            int existingCostsCount = 0;
            Guid existingProfileUid = new Guid();
            int msgcnt = messages.Count;

            if ( entity.FoundExistingRecord )
            {
                //check if has costs
                List<CostProfile> costs = CostProfileManager.GetAll(entity.ExistingParentRowId, false);
                if ( costs != null && costs.Count > 0 )
                {
                    existingCostsCount = costs.Count;
                    if ( costs.Count == 1 )
                    {
                        existingProfileUid = costs[ 0 ].RowId;
                        identifier = existingProfileUid.ToString();
						//additional lookups are done for this, so why set?
						//existingProfile = existingCostProfiles[ 0 ];
						//costProfile.ExistingCostProfile = existingProfile;
						costProfile.Identifier = existingProfileUid;
                    }
                }
            }

            
            //note may need to add an identifier for multple profiles
            //- also as with others may need an external identifier
            //-180525 - should always require an identifier - but this puts extra onus on user, especially for one time
            if ( importHelper.CostInternalIdentifierHdr > -1 )
            {
                identifier = Assign(rowNbr, csv, importHelper.CostInternalIdentifierHdr, "Cost Internal Identifier", ref messages, "", false);
                if ( !string.IsNullOrWhiteSpace(identifier) )
                {
                    //maybe not ever allow delete here?
                    if ( identifier == DELETE_ME )
                    {
                        messages.Add(string.Format("Row: {0}. A cost profile identifier ({1}) cannot have a value of #DELETE. Deletes are handled by providing the identifier in this column, and {2} in the external identifier column.", rowNbr, identifier, DELETE_ME));
                        return;
                    }
                     else if ( identifier == NEW_ID ) //not likely as only for existing scenario
                    {
                        // Indicates this is a new record
                        //might be better to leave blank?
                        costProfile.Identifier = Guid.NewGuid();
                        //return;
                    } //how to do deletes?
					else if ( !ServiceHelper.IsValidGuid( identifier ) )
					{
						messages.Add( string.Format( "Row: {0}. The provided cost profile identifier ({1}) is invalid. It must be a valid UUID for a cost profile that actually exists in the database.", rowNbr, identifier ) );
					}
					else if ( entity.FoundExistingRecord == false )
                    {
                        //just ignore, could be from export in another env.
                        //messages.Add( string.Format( "Row: {0}. A cost profile identifier ({1}) cannot be entered with a new credential: {2}", rowNbr, identifier, entity.Name ) );
                    }

                    else
                    {
                        //will want to ignore if no other requires data
                        //do a lookup to see if references existing profile
                        existingProfile = CostProfileManager.GetBasicProfile(new Guid(identifier));
                        if ( existingProfile == null || existingProfile.Id == 0 )
                        {
                            messages.Add(string.Format("Row: {0}. A cost profile identifier ({1}) was provided, but the system could not find this record. It must be a valid UUID for a cost profile that actually exists in the database.", rowNbr, identifier));
                            return;
                        }
                        else
                        {
                            existingProfileUid = new Guid(identifier);
                            costProfile.Identifier = new Guid(identifier);
                            costProfile.ExistingCostProfile = existingProfile;
                            //what else?
                        }
                    }
                }
            }
            //externalIdentifier =====================================================
            if ( importHelper.CostExternalIdentifierHdr > -1 )
            {
                externalIdentifier = Assign(rowNbr, csv, importHelper.CostExternalIdentifierHdr, "Cost External Identifier", ref messages, "", false);
                if ( !string.IsNullOrWhiteSpace(externalIdentifier) )
                {
                    // check if in previous list - or better from list in credential
                    //format is up to user, just unique to the credential
                    if ( entity.FoundExistingRecord )
                    {
                        if ( externalIdentifier == DELETE_ME )
                        {
                            if ( existingCostsCount == 1 
                            || ( existingCostsCount > 1 && ServiceHelper.IsValidGuid(existingProfileUid) ) )
                            {
                                entity.CostProfile.DeletingProfile = true;
                                entity.CostProfile.Identifier = existingProfileUid;
                                return;
                            } else if ( existingCostsCount > 1 )
                            {
                                messages.Add(string.Format("Row: {0}. Cost Profile - You have entered {1} for External Identifier. However, there are more than 1 existing cost profiles. The system cannot determine which cost profile should be deleted. Please provide the internal identifier for the related cost profile. HINT: do an export of existing credentials, and the related internal identifers will be provided. ", rowNbr, externalIdentifier));
                                return;
                            }
                            else if ( existingCostsCount == 0 )
                            {
                                messages.Add(string.Format("Row: {0}. Cost Profile - You have entered {1} for External Identifier. However, there are no existing cost profiles. This is an inconsistent request, so the record is being rejected. ", rowNbr, externalIdentifier));
                                return;
                            }
                        } else
                        {
                            //should a look up be done here, if not already done?
                            if ( existingProfile == null || existingProfile.Id == 0 )
                            {
                                Guid targetRowId = Factories.Import.ImportHelpers.ExternalIdentifierXref_Get(1, entity.ExistingRecord.Id, CodesManager.ENTITY_TYPE_COST_PROFILE, externalIdentifier, ref status);
                                if ( !ServiceHelper.IsValidGuid(targetRowId) )
                                {
                                    //will not be found for first time, but should be there for existing credential
                                    if ( existingCostsCount > 1 )
                                    {
                                        //then an issue - or could interpret as an add!
                                        messages.Add(string.Format("Row: {0}. CostProfile. An external identifier ({1}) was provided that is not yet associated with an existing cost profile. There are existing cost profiles, so the system cannot determine if this cost is a new one, or meant to be an update.  Re: credential: {2}", rowNbr, externalIdentifier, entity.Name));
                                        return;
                                    }
                                } else
                                {
                                    costProfile.Identifier = targetRowId;
                                    existingProfile = CostProfileManager.GetBasicProfile(targetRowId);
                                    if ( existingProfile == null || existingProfile.Id == 0 )
                                    {
                                        messages.Add(string.Format("Row: {0}. In order for the system to update a cost profile, a cost identifier (either internal or external) must be entered. The entered external identifier: {1} is not valid:  for credential: {2} - that is a related cost profile is not associated with this external identifier.", rowNbr, externalIdentifier, entity.Name));
                                        return;
                                    }
                                    else
                                    {
                                        costProfile.ExistingCostProfile = existingProfile;
                                        identifier = existingProfile.RowId.ToString();
                                        costProfile.Identifier = existingProfileUid;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if ( externalIdentifier == DELETE_ME )
                        {
                            messages.Add(string.Format("Row: {0}. Cost Profile - Invalid value for External Identifier: {1}. You cannot use the value of #DELETEME with a new credential.", rowNbr, externalIdentifier));
                            return;
                        }
                    }
                }
            } //

            //will need an edit for this in the validate headers. 
            if ( importHelper.CostDetailUrlHdr > -1 )
            {
                //conditionally required
                costProfile.DetailsUrl = AssignUrl(rowNbr, csv, importHelper.CostDetailUrlHdr, "CostProfile.DetailsUrl", ref messages, "", false);
            }
            if ( importHelper.CostNameHdr > -1 )
            {
                //only an error if has data.
                costProfile.Name = Assign(rowNbr, csv, importHelper.CostNameHdr, "CostProfile.CostNameHdr", ref messages, "", false);
            }

            if ( importHelper.CostCurrencyTypeHdr > -1 )
            {
                //will want some sort of validation for this
                string currency = Assign(rowNbr, csv, importHelper.CostCurrencyTypeHdr, "CostProfile.CostCurrencyType", ref messages, "", false);
                if ( !string.IsNullOrWhiteSpace(currency) )
                {
                    costProfile.CurrencyType = currency;
                    if ( costProfile.CurrencyType.ToLower() == "usd" )
                    {
                        costProfile.CurrencyTypeId = 840;
                    }
                    else
                    {
                        EnumeratedItem ei = CodesManager.GetCurrencyItem(costProfile.CurrencyType);
                        if ( ei != null && ei.Id > 0 )
                        {
                            costProfile.CurrencyTypeId = ei.Id;
                        }
                        else
                        {
                            messages.Add(string.Format("Row: {0}. The currency type is not a known code: {1}", rowNbr, currency));
                        }
                    }
                }
            }
            //do desc last to check for generating a default
            if ( importHelper.CostDescriptionHdr > -1 )
            {
                //conditionally required
                costProfile.Description = Assign(rowNbr, csv, importHelper.CostDescriptionHdr, "CostProfile.Description", ref messages, "", false);
            }


            if ( importHelper.CostTypesListHdr > -1 )
            {
                string list = Assign(rowNbr, csv, importHelper.CostTypesListHdr, "Cost Types List", ref messages, "", false);
                if ( !string.IsNullOrWhiteSpace(list) )
                {
                    if ( list == DELETE_ME )
                    {
                        costProfile.DeleteCostItems = true;
                    }
                    else
                    {
                        //type1~price1~future1|type2~price2~future2
                        string[] array = list.Split('|');
                        if ( array.Count() > 0 )
                        {
                            int cntr = 0;
                            foreach ( var item in array )
                            {
                                cntr++;
                                if ( string.IsNullOrWhiteSpace(item.Trim()) )
                                    continue;

                                string[] parts = item.Split('~');
                                //for now expecting at lease type, and price
                                if ( parts.Count() < 2 )
                                {
                                    messages.Add(string.Format("Row: {0} Costs Entry Number: {1}. Cost types list must contain a cost type and a price. Entry: {2}", rowNbr, cntr, item));
                                    continue;
                                }

                                CostProfileItemDTO costItem = new CostProfileItemDTO();
                                //validate type
                                costItem.DirectCostType = parts[ 0 ];
                                EnumeratedItem ei = CodesManager.GetCodeAsEnumerationItem(CodesManager.PROPERTY_CATEGORY_CREDENTIAL_ATTAINMENT_COST, costItem.DirectCostType);
                                if ( ei == null || ei.Id == 0 )
                                    messages.Add(string.Format("Row: {0} Invalid direct cost type of {1}", rowNbr, costItem.DirectCostType));
                                else
                                {
                                    //not sure which will use
                                    costItem.DirectCostTypeId = ei.Id;
                                    costItem.CostItem = ei;
                                }

                                decimal price = 0;
                                if ( decimal.TryParse(parts[ 1 ], out price) )
                                {
                                    costItem.Price = price;
                                    costProfile.CostItems.Add(costItem);
                                }
                                else
                                {
                                    messages.Add(string.Format("Row: {0} Costs Entry Number: {1}. Invalid value for price. This must be a valid integer or decimal. Price. : {2}", rowNbr, cntr, parts[ 1 ]));
                                }

                            } //foreach

                        }
                    }
                }

            }
            //============================================================
            //if we have any cost data, verify we have the correct minimum data
            //unless a delete request!
            if ( costProfile != null && costProfile.IsNotEmpty )
            {
                if ( string.IsNullOrWhiteSpace( costProfile.Name ) )
                {
					costProfile.Name = "Cost Profile";

					//messages.Add( string.Format( "Row: {0}. Missing Cost Profile Name. If any cost data is included, a Cost Name must also be included. ", rowNbr ) );
                }
                if ( string.IsNullOrWhiteSpace(costProfile.Description) )
                {
                    messages.Add(string.Format("Row: {0}. Missing Cost Description. If any cost data is included, a Cost Description must also be included. ", rowNbr));
                } else if ( costProfile.Description == DELETE_ME )
                {
                    messages.Add(string.Format("Row: {0}. Invalid use of #DELETE with Cost Description. Cost Description is required when any cost information is provided, it can not be deleted. ", rowNbr));
                }
                if ( string.IsNullOrWhiteSpace(costProfile.DetailsUrl) )
                {
                    messages.Add(string.Format("Row: {0}. Missing Cost Details Url. If any cost data is included, a Cost Details Url must also be included. ", rowNbr));
                }
                else if ( costProfile.DetailsUrl == DELETE_ME )
                {
                    messages.Add(string.Format("Row: {0}. Invalid use of #DELETE with Cost Details Url. Cost Details Url is required when any cost information is provided, it can not be deleted. ", rowNbr));
                }

                if ( entity.FoundExistingRecord )
                {
                    if ( string.IsNullOrWhiteSpace(identifier)
                        && string.IsNullOrWhiteSpace(externalIdentifier)
                        && existingCostsCount > 0 )
                    {
                        if ( existingCostsCount > 1)
                            messages.Add(string.Format("Row: {0}. In order for the system to update a cost profile, a cost identifier (either internal or external) must be entered with a existing credential: {1}. It is recommended to always include an external identifier for a cost profile.", rowNbr, entity.Name));
                    }
                    else
                    {
                        costProfile.ExternalIdentifier = externalIdentifier;
                        //at this point we would have retrieved an existing profile by identifier, or ext identifier, or set to first item from list of all profiles. 
                        //  - Unless there is more than one profile, in which case don't have an identifier
                        if ( string.IsNullOrWhiteSpace(identifier) || !ServiceHelper.IsValidGuid(identifier) )
                        {
                            if ( existingCostsCount  > 1)
                            {
                                messages.Add(string.Format("Row: {0}. There are multiple existing cost profiles for this credential. In order for the system to update a cost profile, either an external cost identifier or an internal (as generated by exporting existing records) cost identifier must be entered. Credential: {1}", rowNbr, entity.Name));

                            }
                        }
                        //else
                        //{
                        //    // at this point we either have:
                        //    //  no extIdentifier with no costs or 
                        //    //  an extIdentifier with costs
                        //    //  an extIdentifier with no costs
                        //    if ( string.IsNullOrWhiteSpace(externalIdentifier) && existingCostsCount == 0 )
                        //    {
                        //        //OK - no extIdentifier with no costs or 
                        //    }
                        //    else if ( !string.IsNullOrWhiteSpace(externalIdentifier) && existingCostsCount == 0 )
                        //    {
                        //        //OK - an extIdentifier with no costs
                        //    }
                        //    else
                        //    {
                        //        //  a extIdentifier with costs

                        //        //also have to consider adding new cost to existing credential!
                        //        //may defer checking - if only one, then attempt to allow
                        //        //or maybe we should append these to the previous record. It will save handling appends in the update (jsut handle lists instead)
                                
                        //        Guid targetRowId = Factories.Import.ImportHelpers.ExternalIdentifierXref_Get(1, entity.ExistingRecord.Id, CodesManager.ENTITY_TYPE_COST_PROFILE, externalIdentifier, ref status);
                        //        if ( !ServiceHelper.IsValidGuid(targetRowId) )
                        //        {
                        //            //will not be found for first time, but should be there
                        //            if ( existingCostsCount > 0 )
                        //            {
                        //                //then an issue - or could interpret as an add!
                        //                //messages.Add( string.Format( "Row: {0}. CostProfile. An external identifier ({1}) was provided that is not yet associated with an existing cost profile. There are existing cost profiles, so the system cannot determine if this cost is a new one, or meant to be an update.  Re: credential: {2}", rowNbr, extIdentifier, entity.Name ) );
                        //            }
                        //        }
                        //        else
                        //        {
                        //            costProfile.Identifier = targetRowId;
                        //            existingProfile = CostProfileManager.GetBasicProfile(targetRowId);
                        //            if ( existingProfile == null || existingProfile.Id == 0 )
                        //            {
                        //                messages.Add(string.Format("Row: {0}. In order for the system to update a cost profile, a cost identifier (either internal or external) must be entered. The entered external identifier is not valid: {1}  for credential: {2}", rowNbr, externalIdentifier, entity.Name));
                        //            }
                        //            else
                        //            {
                        //                costProfile.ExistingCostProfile = existingProfile;
                        //            }
                        //        }
                        //    }
                        //}
                    }
                } //

                entity.CostProfile = costProfile;
                //get previous
                if ( entity.Action == "append" )
                {
                    var exists = importList.FirstOrDefault(a => a.ExternalIdentifier == entity.ExternalIdentifier);
                    if ( exists != null && !string.IsNullOrWhiteSpace(exists.ExternalIdentifier) )
                    {
                        //may need to remove from list and re-add
                        exists.CostProfiles.Add(costProfile);
                    }
                }
                else
                {
                    //may need to remove from list and re-add
                    entity.CostProfiles.Add(costProfile);
                }
            }
          

        }//
        public void AssignAssessment( int rowNbr, CsvReader csv, ImportModel entity, AppUser user, ref List<string> messages )
        {
            string identifier = "";
            string externalIdentifier = "";
            AssessmentProfile record = new AssessmentProfile();
            int msgcnt = messages.Count;

            if ( asmtImportHelper.ExternalIdentifierHdr > -1 )
            {
                externalIdentifier = Assign( rowNbr, csv, asmtImportHelper.ExternalIdentifierHdr, "ExternalIdentifier", ref messages, "", false );
                if ( !string.IsNullOrWhiteSpace( externalIdentifier ) )
                {
                    if ( externalIdentifier == DELETE_ME )
                    {
                        //hmmm this may not work in this field, will need the identifier to delete - in case of multiple
                        //or separate delete all?
                        messages.Add( string.Format( "Row: {0}. An assessment external Identifier ({1}) cannot have a value of #DELETE. Deletes are not currently handled.", rowNbr, identifier ) );
                        //entity.RequiresCondition.DeleteCondition = true;
                        return;
                    }
                    else if ( entity.FoundExistingRecord == false )
                    {
                        //or just ignore it? If new and existing are entered
                        //actually will need to create the proxy - see cost
                        //externalIdentifier = "";
                        //messages.Add( string.Format( "Row: {0}. A requires condition identifier ({1}) cannot be entered with a new credential: {2}", rowNbr, identifier, entity.Name ) );
                    }
                    else
                    {
                        //OR ????

                    }
                }
            }

            
            //note this is auto added to an export
            //if there is no other data, this should be ignored
            if ( asmtImportHelper.InternalIdentifierHdr > -1 )
            {
                identifier = Assign( rowNbr, csv, asmtImportHelper.InternalIdentifierHdr, "InternalIdentifier", ref messages, "", false );
                if ( !string.IsNullOrWhiteSpace( identifier ) )
                {
                    if ( identifier == DELETE_ME )
                    {
                        //hmmm this may not work in this field, will need the identifier to delete - in case of multiple
                        //or separate delete all?
                        messages.Add( string.Format( "Row: {0}. An assessment identifier ({1}) cannot have a value of #DELETE. Deletes are NOT currently handled.", rowNbr, identifier ) );
                        return;
                    }
                    else if ( entity.FoundExistingRecord == false )
                    {
                        //or just ignore it? If new and existing are entered
                        identifier = "";
                        //messages.Add( string.Format( "Row: {0}. A requires condition identifier ({1}) cannot be entered with a new credential: {2}", rowNbr, identifier, entity.Name ) );
                    }
                    else
                    {
                        //OR will we allow NEW
                        if ( ServiceHelper.IsValidGuid( identifier ) )
                        {
                            //will want to ignore if no other requires data, or look up now and handle validation later
                            //entity.RequiresCondition.Identifier
                            record = AssessmentManager.GetBasic( new Guid( identifier ) );
                            if ( record == null || record.Id == 0 )
                            {
                                messages.Add( string.Format( "Row: {0}. The provided assessment identifier ({1}) does not exist. A valid identifier must be provided in order to update and existing assessment. Re: credential: {2}", rowNbr, identifier, entity.Name ) );
                            }
                        }
                        else
                        {
                            messages.Add( string.Format( "Row: {0}. The provided identifier ({1}) is invalid. It must be a valid UUID for a condition that actually exists in the database.", rowNbr, identifier ) );
                        }

                    }
                }
            }


            if ( asmtImportHelper.NameHdr > -1 )
            {
                record.Name = Assign( rowNbr, csv, asmtImportHelper.NameHdr, "Assessment Name", ref messages, "", true );
            }
            //do desc last to check for generating a default
            if ( asmtImportHelper.DescHdr > -1 )
            {
                record.Description = Assign( rowNbr, csv, asmtImportHelper.DescHdr, "Assessment Description", ref messages, "", true );
            }
            if ( asmtImportHelper.SubjectWebpageHdr > -1 )
            {
                record.SubjectWebpage = AssignUrl( rowNbr, csv, asmtImportHelper.SubjectWebpageHdr, "Assessment SubjectWebpage", ref messages, "", true );
            }
            if ( asmtImportHelper.AvailableOnlineAtHdr > -1 )
            {
                record.AvailableOnlineAt = AssignUrl( rowNbr, csv, asmtImportHelper.AvailableOnlineAtHdr, "Assessment AvailableOnline", ref messages, "", false );
            }
            if ( asmtImportHelper.AvailabilityListingHdr > -1 )
            {
                record.AvailabilityListing = AssignUrl( rowNbr, csv, asmtImportHelper.AvailabilityListingHdr, "Assessment AvailabilityListing", ref messages, "", false );
            }
            if ( asmtImportHelper.AvailableAtHdr > -1 )
            {
                //NOT HANDLED 
                
            }
            if ( asmtImportHelper.AssessmentExampleUrlHdr > -1 )
            {
                record.AssessmentExample = AssignUrl( rowNbr, csv, asmtImportHelper.AssessmentExampleUrlHdr, "Assessment ExampleUrl", ref messages, "", false );
            }
            if ( asmtImportHelper.CodedNotationHdr > -1 )
            {
                record.CodedNotation = AssignUrl( rowNbr, csv, asmtImportHelper.CodedNotationHdr, "Assessment CodedNotation", ref messages, "", false );
            }

            //NOTE: has to be associated with the condition profile
            entity.Assessments.Add( record );

            //will always need a related condition profile
            if ( importHelper.HasConditionProfile == false )
            {
                messages.Add( string.Format( "Row: {0}. A condition profile must always be entered with an assessment, and no data was found. A valid condition profile must be provided in order to connect an assessment to a credential. Re: credential: {2}", rowNbr, identifier, entity.Name ) );
            } else 
            {
                //if present, will be ctid for existing asmt
                //must have other condition properties, especially on update
                //entity.ConditionProfile.AssessmentCtid = Assign( rowNbr, csv, importHelper.ConditionExistingAsmtHdr, "ConditionAssessmentCtid", ref messages, "", false );
                //if ( prevAsmtCtid == entity.ConditionProfile.AssessmentCtid )
                //{
                //    entity.ConditionProfile.TargetAssessment = lastAsmt;

                //}
                //else
                //{
                //    //get org and ensure can view
                //    lastAsmt = AssessmentManager.GetByCtid( entity.ConditionProfile.AssessmentCtid );
                //    if ( lastAsmt == null || lastAsmt.Id == 0 )
                //    {
                //        messages.Add( string.Format( "Row: {0}. An assessment was not found for the provided CTID: {1}", rowNbr, entity.ConditionProfile.AssessmentCtid ) );
                //    }
                //    else
                //    {
                //        //confirm has access - may not be necessary for an assessment, skip for now
                //        //if (AssessmentServices.CanUserUpdateAssessment( lastAsmt.Id, user, ref status ) == false)
                //        //{
                //        //    messages.Add( string.Format( "Row: {0}. You do not have update rights for the referenced assessment (via CTID): {1} ({2}). ", rowNbr, owningOrg.Name, owningOrg.Id ) );
                //        //}
                //        //else
                //        {
                //            entity.ConditionProfile.TargetAssessment = lastAsmt;
                //            prevAsmtCtid = entity.ConditionProfile.AssessmentCtid;
                //        }
                //    }

                //}
            }

            //if ( entity.ConditionProfile.IsNotEmpty )
            //{
            //    if ( string.IsNullOrWhiteSpace( entity.ConditionProfile.ConditionType ) )
            //    {
            //        if ( record != null && record.Id > 0 )
            //        {
            //            //dont' allow type chg at this time, and don't flag as error
            //            entity.ConditionProfile.ConditionTypeId = record.ConnectionProfileTypeId;
            //            entity.ConditionProfile.ConditionType = record.ConnectionProfileType;
            //        }
            //        else
            //        {
            //            entity.ConditionProfile.ConditionType = "Requires";
            //            entity.ConditionProfile.ConditionTypeId = 1;
            //        }
            //    }

            //    if ( entity.IsExistingCredential )
            //    {
            //        //if exists could allow new, without an identifier - may be dangerous to assume?
            //        if ( asmtImportHelper.ConditionIdentifierHdr > -1 )
            //        {
            //            if ( !ServiceHelper.IsValidGuid( identifier ) )
            //            {
            //                //ISSUE - will need to be able to add a new condition to an existing credential!!!
            //                messages.Add( string.Format( "Row: {0}. A condition identifier ({1}) cannot be entered with a new credential: {2}", rowNbr, identifier, entity.Name ) );
            //            }
            //            else
            //            {
            //                //condition must exist
            //                //var record = Entity_ConditionProfileManager.GetBasic( new Guid( identifier ) );
            //                if ( record == null || record.Id == 0 )
            //                {
            //                    messages.Add( string.Format( "Row: {0}. The provided condition identifier ({1}) does not exist. Only a valid identifier from the publisher database may be used for a assessment identifier for credential: {2}", rowNbr, identifier, entity.Name ) );
            //                }
            //                else
            //                {
            //                    //should we store and pass the existing record?
            //                    entity.ConditionProfile.Identifier = record.RowId;

            //                    // as well the condition should match. Unless we want a means to change the condition type???
            //                }
            //            }
            //        }
            //        else
            //        {
            //            //may be issue, as could have an existing credential, and didn't use the latest template with an identifier - 
            //        }
            //    } //ignore for a new credential (actually have error above)
            //}
            //else
            //{
            //    //no action, identifier will be ignored
            //}

        }//




        #endregion

        #region Validations
        public bool ValidateHeaders( string[] headers, bool doingMinimumRequiredChecks, ref List<string> messages )
        {
            /*string action, 
             * if ( headers == null 
                || (action != PARTIAL_UPDATE && headers.Count() < RequiredNbrOfColumns)
                || ( action == PARTIAL_UPDATE && headers.Count() < 4 )
                )
             * 
             */
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
                    if ( colname.StartsWith( "cost" ) )
                    {
                        #region cost profile related
                        switch ( colname )
                        {
                            //======= cost profile ================
                            //case "cost.identifier":
                            case "cost.internalidentifier":
                            //case "costidentifier":
                                importHelper.CostInternalIdentifierHdr = cntr;
                                //importHelper.HasCosts = true;
                                break;
                            case "cost.externalidentifier":
                            
                                importHelper.CostExternalIdentifierHdr = cntr;
                                //importHelper.HasCosts = true;
                                break;
                            case "cost.name":
                                importHelper.CostNameHdr = cntr;
                                importHelper.HasCosts = true;
                                break;
                            case "cost.description":
                                importHelper.CostDescriptionHdr = cntr;
                                importHelper.HasCosts = true;
                                break;
                            case "cost.detailsurl":
                                importHelper.CostDetailUrlHdr = cntr;
                                importHelper.HasCosts = true;
                                break;
                            case "cost.currencytype":
                                importHelper.CostCurrencyTypeHdr = cntr;
                                importHelper.HasCosts = true;
                                break;
                            case "cost.typeslist":
                                importHelper.CostTypesListHdr = cntr;
                                importHelper.HasCosts = true;
                                break;
                            default:
                                messages.Add( "Error unknown column header encountered: " + item );
                                break;
                        }
                        #endregion
                    }
                    else if ( colname.StartsWith( "condition" ) || colname.StartsWith( "requires" ) )
                    {
                        #region condition profile related
                        switch ( colname )
                        {
                            //======= condition profile ================
                            case "conditionprofile.identifier":
                            case "conditionprofile.internalidentifier":
                            case "conditionidentifier":
                                importHelper.ConditionIdentifierHdr = cntr;
                                //importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.externalidentifier":
                            case "conditionprofile.uniqueidentifier":
                                importHelper.ConditionExternalIdentifierHdr = cntr;
                                //importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.type":
                            case "conditionprofile.conditiontype":
                                importHelper.ConditionTypeHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.name":
                                importHelper.ConditionNameHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.description":
                            case "condition:description":
                            case "conditiondescription":
                                importHelper.ConditionDescHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.subjectwebpage":
                            case "requires.subjectwebpage":
                                importHelper.ConditionSubjectWebpageHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.submissions":
                            case "conditionprofile.submissionofitems":
                            case "requires.submissionofitems":
                            case "requires.submissions":
                                importHelper.ConditionSubmissionHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.conditionitems":
                            case "requires.conditionitems":
                            case "conditionconditions":
                                importHelper.ConditionConditionsHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.experience":
                            case "requires.experience":
                                importHelper.ConditionExperienceHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.yearsofexperience":
                            case "requires.yearsofexperience":
                                importHelper.ConditionYearsOfExperienceHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.assessment_ctid":
                            case "conditionprofile.assessmentctid":
                                importHelper.ConditionExistingAsmtHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.assessment_identifier":
                                importHelper.ConditionAsmtIdentifierHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.credentiallist":
                            case "conditionprofile.targetcredential":
                                importHelper.ConditionCredentialsListHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.assessmentList":
                            case "conditionprofile.targetassessment":
                                importHelper.ConditionAsmtsListHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.learningopportunitylist":
                            case "conditionprofile.targetlearningopportunity":
                                importHelper.ConditionLoppsListHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.credithourtype":
                                importHelper.ConditionCreditHourTypeHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.credithourvalue":
                                importHelper.ConditionCreditHourValueHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.creditunittype":
                                importHelper.ConditionCreditUnitTypeHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.creditunitvalue":
                                importHelper.ConditionCreditUnitValueHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.creditunittypedescription":
                                importHelper.ConditionCreditUnitDescriptionHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;

                            default:
                                messages.Add( "Error unknown column header encountered: " + item );
                                break;
                        }
                        #endregion
                    }

					else if ( colname.StartsWith( "connection" ) )
					{
						#region CONNECTION profile related
						switch ( colname )
						{
							//======= connection profile ================
							case "connectionprofile.identifier":
							case "connectionprofile.internalidentifier":
							case "connectionidentifier":
								importHelper.ConnectionIdentifierHdr = cntr;
								//importHelper.HasConnectionProfile = true;
								break;
							case "connectionprofile.externalidentifier":
							case "connectionprofile.uniqueidentifier":
								importHelper.ConnectionExternalIdentifierHdr = cntr;
								//importHelper.HasConnectionProfile = true;
								break;

							case "connectionprofile.connectiontype":
								importHelper.ConnectionTypeHdr = cntr;
								importHelper.HasConnectionProfile = true;
								break;

							case "connectionprofile.description":
							case "connection:description":
								importHelper.ConnectionDescHdr = cntr;
								importHelper.HasConnectionProfile = true;
								break;
							case "connectionprofile.credentiallist":
							case "connectionprofile.targetcredential":
								importHelper.ConnectionCredentialsListHdr = cntr;
								importHelper.HasConnectionProfile = true;
								break;
							case "connectionprofile.assessmentlist":
							case "connectionprofile.targetassessment":
								importHelper.ConnectionAsmtsListHdr = cntr;
								importHelper.HasConnectionProfile = true;
								break;
							case "connectionprofile.learningopportunitylist":
							case "connectionprofile.targetlearningopportunity":
								importHelper.ConnectionLoppsListHdr = cntr;
								importHelper.HasConnectionProfile = true;
								break;
							case "connectionprofile.credithourtype":
							case "connection.credithourtype":
								importHelper.ConnectionCreditHourTypeHdr = cntr;
								importHelper.HasConnectionProfile = true;
								break;
							case "connectionprofile.credithourvalue":
							case "connection.credithourvalue":
								importHelper.ConnectionCreditHourValueHdr = cntr;
								importHelper.HasConnectionProfile = true;
								break;
							case "connectionprofile.creditunittype":
							case "connection.creditunittype":
								importHelper.ConnectionCreditUnitTypeHdr = cntr;
								importHelper.HasConnectionProfile = true;
								break;
							case "connectionprofile.creditunitvalue":
							case "connection.creditunitvalue":
								importHelper.ConnectionCreditUnitValueHdr = cntr;
								importHelper.HasConnectionProfile = true;
								break;
							case "connectionprofile.creditunittypedescription":
							case "connection.creditunittypedescription":
								importHelper.ConnectionCreditUnitDescriptionHdr = cntr;
								importHelper.HasConnectionProfile = true;
								break;
							case "connectionprofile.weight":
							case "connection.weight":
								importHelper.ConnectionWeightHdr = cntr;
								break;
							default:
								messages.Add( "Error unknown column header encountered: " + item );
								break;
						}
						#endregion
					}
					else if ( colname.StartsWith( "assessment" ) )
                    {
                        #region assessment related
                        switch ( colname )
                        {
							case "assessmentdeliverytype": //had to add here due to assessment prefix
								importHelper.AssessmentDeliveryTypeHdr = cntr;
								break;

							//======= assessment ================                            
							case "assessment.identifier":
                            case "assessment.internalidentifier":
                                importHelper.AssessmentIdentifierHdr = cntr;
                                importHelper.HasAssessment = true;
                                break;
                            case "assessment.externalidentifier":
                            case "assessment.uniqueidentifier":
                                importHelper.AssessmentExternalIdentifierHdr = cntr;
                                importHelper.HasAssessment = true;
                                break;
                            case "assessment.name":
                                importHelper.AssessmentNameHdr = cntr;
                                importHelper.HasAssessment = true;
                                break;
                            case "assessment.description":
                                importHelper.AssessmentDescHdr = cntr;
                                importHelper.HasAssessment = true;
                                break;
                            case "assessment.subjectwebpage":
                                importHelper.AssessmentSubjectWebpageHdr = cntr;
                                importHelper.HasAssessment = true;
                                break;
                            case "assessment.availableonlineat":
                                importHelper.AssessmentAvailableAtHdr = cntr;
                                importHelper.HasAssessment = true;
                                break;
                            case "assessment.availabilitylisting":
                                importHelper.AssessmentAvailabilityListingHdr = cntr;
                                importHelper.HasAssessment = true;
                                break;
                            //case "assessment.exampleurl":
                            //    importHelper.AssessmentExampleUrlHdr = cntr;
                            //    importHelper.HasAssessment = true;
                            //    break;
                            //case "assessment.availableat":
                            //    importHelper.AssessmentAvailableAtHdr = cntr;
                            //    importHelper.HasAssessment = true;
                            //    break;
                            default:
                                messages.Add( "Error unknown assessment column header encountered: " + item );
                                break;
                        }
                        #endregion
                    }
                    else if ( colname.StartsWith( "learningopp" ) )
                    {
                        #region learningopp related
                        switch ( colname )
                        {
                            //======= learningopp ================
                            case "learningopp.identifier":
                            case "learningopp.internalidentifier":
                                loppImportHelper.InternalIdentifierHdr = cntr;
                                loppImportHelper.HasLearningOpp = true;
                                break;
                            case "learningopp.externalidentifier":
                            case "learningopp.uniqueidentifier":
                                loppImportHelper.ExternalIdentifierHdr = cntr;
                                loppImportHelper.HasLearningOpp = true;
                                break;
                            case "learningopp.name":
                                loppImportHelper.NameHdr = cntr;
                                loppImportHelper.HasLearningOpp = true;
                                break;
                            case "learningopp.description":
                                loppImportHelper.DescHdr = cntr;
                                loppImportHelper.HasLearningOpp = true;
                                break;
                            case "learningopp.subjectwebpage":
                                loppImportHelper.SubjectWebpageHdr = cntr;
                                loppImportHelper.HasLearningOpp = true;
                                break;
                            case "learningopp.availableonlineat":
                                loppImportHelper.AvailableOnlineAtHdr = cntr;
                                loppImportHelper.HasLearningOpp = true;
                                break;
                            case "learningopp.availabilitylisting":
                                loppImportHelper.AvailabilityListingHdr = cntr;
                                loppImportHelper.HasLearningOpp = true;
                                break;
                            case "learningopp.learningresourceurl":
                                loppImportHelper.LearningResourceUrlHdr = cntr;
                                loppImportHelper.HasLearningOpp = true;
                                break;
                            case "learningopp.availableat":
                                loppImportHelper.AvailableAtHdr = cntr;
                                loppImportHelper.HasLearningOpp = true;
                                break;
                            default:
                                messages.Add( "Error unknown column header encountered: " + item );
                                break;
                        }
                        #endregion
                    }
                    else
                        switch ( colname )
                        {
                            case "externalidentifier":
                            case "uniqueidentifier":
                            case "identifier":
                                importHelper.ExternalIdentifierHdr = cntr;
                                break;
							case "internalidentifier":
								importHelper.InternalIdentifierHdr = cntr;
								break;
							case "action":
                                importHelper.ActionHdr = cntr;
                                break;
                            case "ctid":
                                //ctid will not be required if external identifier is present
                                importHelper.CtidHdr = cntr;
                                break;
                            case "name":
                            case "credentialname":
                                importHelper.NameHdr = cntr;
                                break;
                            case "description":
                            case "credentialdescription":
                                importHelper.DescHdr = cntr;
                                break;
                            //case "partneridentifier": //not used
                            //    importHelper.OrganizationExternalIdHdr = cntr;
                            //    break;
                            case "existingorganizationctid": //always requiried, or if using unique identifier, currently a single value
                            case "orgctid":
                            case "ownedby":
                                importHelper.OrganizationCtidHdr = cntr;
                                break;

                            case "credentialtype":
                            case "credentialtypeschema":
                                //todo verification
                                importHelper.TypeHdr = cntr;
                                break;
                            case "subjectwebpage":
                            case "webpage":
                                importHelper.SubjectWebpageHdr = cntr;
                                break;

                            case "copyrightholder_ctid":
                            case "copyrightholderctid":
                            case "copyrightholder":
                                importHelper.Copyrightholder_CtidHdr = cntr;
                                break;
                            case "credentialstatus":
                            case "credentialstatustype":
                            case "status":
                                importHelper.StatusHdr = cntr;
                                break;

                            case "codednotation":
                                importHelper.CodedNotationHdr = cntr;
                                break;
                            case "credentialid":
                                importHelper.CredentialIdHdr = cntr;
                                break;
                            case "versionidentifier":
                                importHelper.VersionIdentifierHdr = cntr;
                                break;
                            case "imageurl":
                            case "credentialimage":
                                importHelper.ImageUrlHdr = cntr;
                                break;
                            case "alternatename":
                                importHelper.AlternateNameHdr = cntr;
                                break;
                            case "language":
                                importHelper.InLanguageHdr = cntr;
                                break;
                            case "inlanguage":
                                importHelper.InLanguageSingleHdr = cntr;
                                break;
                            case "dateeffective":
                                importHelper.DateEffectiveHdr = cntr;
                                break;
                            case "audienceleveltype":
                            case "audiencelevel":
                                importHelper.AudienceLevelHdr = cntr;
                                break;
                            case "audiencetype":
                                importHelper.AudienceTypeHdr = cntr;
                                break;
                            case "availabilitylisting":
                                importHelper.AvailabilityListingHdr = cntr;
                                break;
                            case "availableonlineat":
                                importHelper.AvailableOnlineAtHdr = cntr;
                                break;

                            case "keywords":
                            case "keyword":
                                importHelper.KeywordsHdr = cntr;
                                break;
                            //
                            case "renewalfrequency":
                                importHelper.RenewalFrequencyHdr = cntr;
                                break;
                            case "subjects":
                            case "subject":
                                importHelper.SubjectsHdr = cntr;
                                break;
							case "cip":
							case "ciplist":
								importHelper.CIPListHdr = cntr;
								break;
                            case "programs":
                            case "instructionalprogramtype":
                                importHelper.ProgramListHdr = cntr;
                                break;
                            case "occupations":
                            case "occupationtype":
                                importHelper.OccupationsHdr = cntr;
                                break;
                            case "industries":
                            case "industrytype":
                                importHelper.IndustriesHdr = cntr;
                                break;
                            case "naics":
                            case "naicslist":
                                importHelper.NaicsListHdr = cntr;
                                break;
                            case "soclist":
                            case "onetlist":
                                importHelper.OnetListHdr = cntr;
                                break;
                            case "duration":
                            case "estimatedduration":
                                importHelper.DurationHdr = cntr;
                                break;
                            case "estimateddurationdescription":
                                importHelper.DurationDescHdr = cntr;
                                break;
							case "assessmentdeliverytype": //note will not hit this due to assessment prefix
								importHelper.AssessmentDeliveryTypeHdr = cntr;
								break;
							case "learningdeliverytype": //note will not hit this due to assessment prefix
								importHelper.LearningDeliveryTypeHdr = cntr;
								break;
							case "degreemajor":
                            case "degreemajors":
                                importHelper.DegreeMajorsHdr = cntr;
                                break;
                            case "degreeminor":
                                importHelper.DegreeMinorHdr = cntr;
                                break;
                            case "degreeconcentration":
                                importHelper.DegreeConcentrationHdr = cntr;
                                break;
                            case "latestversion":
                                importHelper.LatestVersionHdr = cntr;
                                break;
                            case "previousversion":
                                importHelper.PreviousVersionHdr = cntr;
                                break;
                            case "processstandards":
                                importHelper.ProcessStandardsHdr = cntr;
                                break;
                            case "processstandardsdescription":
                                importHelper.ProcessStandardsDescHdr = cntr;
                                break;
                            //not sure if will handle both options in one column
                            case "addresses":
                            case "availableat":
                                importHelper.AvailableAtHdr = cntr;
                                break;
                            case "availableatcodes":
                                importHelper.AvailableAtCodesHdr = cntr;
                                break;

                            case "commonconditions":
                                importHelper.CommonConditionsHdr = cntr;
                                break;
                            case "commoncosts":
                                importHelper.CommonCostsHdr = cntr;
                                break;
                            
                            //QA connections 
                            case "offeredbylist":
                            case "offeredby":
                                importHelper.OfferedByListHdr = cntr;
                                break;
                            case "accreditedbylist":
                            case "accreditedby":
                                importHelper.AccreditedByListHdr = cntr;
                                break;

                            case "approvedbylist":
                            case "approvedby":
                                importHelper.ApprovedByListHdr = cntr;
                                break;
                            case "recognizedbylist":
                            case "recognizedby":
                                importHelper.RecognizedByListHdr = cntr;
                                break;
                            case "regulatedby":
                                importHelper.RegulatedByListHdr = cntr;
                                break;
                            //**** Ignore the following if included - a warning message would be helpful ****
                            case "assessments":
                            case "assessment.offeringorganization":
                            case "offeringorganization":
                            case "advancedstandingfor.assertedby.name":
                            case "advancedstandingfor.assertedby.subjectwebpage":
                            case "advancedstandingfor.description":
                            case "comment":
                            case "comments":
                            case "costprofilecreated":
                            case "conditionprofilecreated":
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

                //TBD - will name be required for an update - for now yes
                if ( importHelper.NameHdr == -1 )
                    messages.Add( "Error - A credential name column must be provided" );

                if ( !doingMinimumRequiredChecks )
                {
                    //will have to defer these checks - only if a new credential
                    if ( importHelper.DescHdr == -1 )
                        messages.Add( "Error - A credential description column must be provided" );

                    if ( importHelper.TypeHdr == -1 )
                        messages.Add( "Error - A credential type column must be provided" );

                    if ( importHelper.SubjectWebpageHdr == -1 )
                        messages.Add( "Error - A credential subject webpage column must be provided" );
                }

                //if any cost data, then check for minimum properties
                if ( importHelper.HasCosts )
                {
                    if ( importHelper.CostDetailUrlHdr == -1 )
                        messages.Add( "Error - If any costs are entered, a cost detail url is required" );

                    if ( importHelper.CostDescriptionHdr == -1 )
                        messages.Add( "Error - If any costs are entered, a cost description is required" );
                }

            }
            catch ( Exception ex )
            {
                string msg = BaseFactory.FormatExceptions( ex );
                LoggingHelper.DoTrace( 1, "Exception encountered will validating headers for credential upload: " + msg );
                messages.Add( "Exception encountered will validating headers for credential upload: " + msg );
            }
            if ( messages.Count > 0 )
                isValid = false;

            return isValid;
        }//
        #endregion
    }

}
