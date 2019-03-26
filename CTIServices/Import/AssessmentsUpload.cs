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
using ImportMgr = Factories.Import.AssessmentImport;
using DBMgr = Factories.AssessmentManager;
using Models;
using Models.Common;
using ImportModel = Models.Import.AssessmentDTO;
using ThisEntity = Models.ProfileModels.AssessmentProfile;
using Models.Import;
using Models.ProfileModels;
using Utilities;

namespace CTIServices.Import
{
    public class AssessmentsUpload : BaseUpload
    {
        #region properties
        //identifier, org ctid, name, type, desc, swp
        static int RequiredNbrOfColumns = 6;

        public AssessmentImportRequest importHelper = new AssessmentImportRequest();
        // public AssessmentImportHelper asmtImportHelper = new AssessmentImportHelper();
        List<ImportModel> importList = new List<ImportModel>();
        //
        public ImportMgr importMgr = new ImportMgr();
        public string prevCipList = "";
        //
        string previousAssessmentMethodTypes = "";
        Enumeration prevAssessmentMethodType = new Enumeration();
        Enumeration thisAssessmentMethodType = new Enumeration();
        //
        string previousAssessmentUseTypes = "";
        Enumeration prevAssessmentUseType = new Enumeration();
        Enumeration thisAssessmentUseType = new Enumeration();
        //
        public string previousDeliveryTypes = "";
        public Enumeration prevDeliveryType = new Enumeration();
        public Enumeration thisDeliveryType = new Enumeration();
        //
        public string previousScoringMethodTypes = "";
        public Enumeration prevScoringMethodType = new Enumeration();
        public Enumeration thisScoringMethodType = new Enumeration();
        //
        string previousCreditUnitTypes = "";
        Enumeration prevCreditUnitType = new Enumeration();
        Enumeration thisCreditUnitType = new Enumeration();
        public string prevSocList = "";
        public string prevNaicsList = "";
        #endregion

        public bool UploadFromText(string inputText, string action, AppUser user, string owningOrganizationRowID, ref int owningOrgId, ref List<string> messages)
        {
            bool isOK = true;
            //string file = "C:\\Projects\\CTI\\AssessmentRegistry\\Testing\\Data\\AssessmentsExport.csv";
            AssessmentDTO import = new AssessmentDTO();

            DateTime start = DateTime.Now;

			messages = new List<string>();
			importList = new List<ImportModel>();
			ctidsList = new List<string>();
			externalIdentifiers = new List<string>();
			rowNbr = 1;
			//if owningOrganizationRowID is provided, then no owning org column is needed
			//NOTE: need to handle where could still exist in an older spreadsheet
			CheckForOwningOrg(owningOrganizationRowID, user, ref messages);

            if (messages.Count > 0)
                return false;

            //if (action == PARTIAL_UPDATE)
            //    IsPartialUpdate = true;

            try
            {
                using (CsvReader csv =
                       new CsvReader(new StringReader(inputText), true))
                {
                    int fieldCount = csv.FieldCount;

                    string[] headers = csv.GetFieldHeaders();
                    //validate headers
                    if (!ValidateHeaders(headers, true, ref messages))
                    {
                        return false;
                    }

                    csv.SkipEmptyLines = true;
                    while (csv.ReadNextRecord())
                    {
                        rowNbr++;
                        //entity = new Assessment();
                        if (IsEmpty(csv, RequiredNbrOfColumns))
                        {
                            //brake assumes at end
                            warnings.Add(string.Format("WARNING. Row {0} did not contain any data, skipped. ", rowNbr));
                            continue;
                        }

						//
						if ( csv[ 0 ] == "Instructions" || csv[ 0 ] == "Sample Data" )
						{
							continue;
						}
						LoggingHelper.DoTrace(7, string.Format("UploadFromText. Row: {0}", rowNbr));

						var duplicateCheck = new ImportModel();
						//validate org. Must exist. This assigns org info that is referenced in CheckIdentifiers!!
						AssignOrgStuff( rowNbr, csv, duplicateCheck, user, importHelper, ref messages );

						CheckIdentifiers( rowNbr, csv, entity: duplicateCheck, user: user, messages: ref messages );
						if ( duplicateCheck != null && duplicateCheck.IsPotentialPartialUpdate == false )
						{
							if ( rowNbr > 2 )
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
						}
						else
						{
							//will append to current output record
							import.IsPotentialPartialUpdate = true;
						}
						HandleRecord(rowNbr, csv, import, user, owningOrganizationRowID, ref messages);

                        //importList.Add(import);

                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("item with the same key has already been added") > -1)
                    messages.Add("Error: An item with the same key has already been added - check for a column name being used more than once. ");
                else
                    messages.Add("AssessmentUpload - Error unexpected exception was encountered: " + ex.Message);
            }

			//add last record
			if ( import != null && !string.IsNullOrWhiteSpace( import.Name ) )
			{
				importList.Add( import );
			}

			//reject all if any errors
			if (messages.Count > 0)
                return false;
            else if (importList.Count < 1)
            {
                messages.Add("No useable data was found in the input file. Hmmmm");
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
                string jsonList = JsonConvert.SerializeObject(importList, settings);
                LoggingHelper.WriteLogFile(5, "AssessmentsUpload.json", jsonList, "", false);
            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(ex, "AssessmentsUpload.UploadFromText Issue with serializing importList error.");
                messages.Add(ex.Message);
            }

            ImportStatus status = new ImportStatus();
            try
            {
                importMgr.Import(importList, user, ref status);

                TimeSpan timeDifference = DateTime.Now.Subtract(start);
                //note need better count to handle multiple rows per Assessment
				string stats = string.Format( "Upload Summary - Read: {0}, Unique Identifiers: {1} Added: {2}, Updated: {3}, Failed: {4}, elapsed: {5:N2} seconds ", rowNbr - 1, importList.Count(), status.RecordsAdded, status.RecordsUpdated, status.RecordsFailed, timeDifference.TotalSeconds );

				ActivityManager activityMgr = new ActivityManager();
                activityMgr.SiteActivityAdd(new SiteActivity()
                {
                    ActivityType = "Organization",
                    Activity = "Bulk Upload",
                    Event = "Assessments",
                    Comment = string.Format("Upload by {0}, of Organization: '{1}'. {2}.", user.FullName(), defaultOwningOrg.Name, stats),
                    ActionByUserId = user.Id,
                    TargetObjectId = defaultOwningOrg.Id,
                    ActivityObjectParentEntityUid = prevOwningAgentUid
                });

                if (status.HasErrors)
                    stats += " NOTE: some errors were encountered.";
                //notify administration
                string url = UtilityManager.FormatAbsoluteUrl(string.Format("~/summary/organization/{0}", defaultOwningOrg.Id));
                string message = string.Format("New Assessment Bulk Upload. <p>{5}</p><ul><li>Organization Id: {0}</li><li>Organization: {1}</a></li><li>{2} </li><li>Uploaded By: {3}</li><li><a href='{4}'>Organization Summary: {1}</a></li></ul>", defaultOwningOrg.Id, defaultOwningOrg.Name, stats, user.FullName(), url, DateTime.Now);
                owningOrgId = defaultOwningOrg.Id; //Useful for other methods

                if (status.RecordsAdded > 0 || status.RecordsUpdated > 0)
                    EmailServices.SendSiteEmail("New Assessment Bulk Upload", message);
                string summaryPage = string.Format("<a href='{0}' target='_summary'>Organization Summary</a>", url);

                LoggingHelper.DoTrace(5, "AssessmentsUpload.UploadAssessmentsFromText(). " + stats);
                messages.Add(summaryPage);
                messages.Add(stats);

				 if (warnings.Count > 0)
                    messages.AddRange(warnings);

                messages.AddRange(status.GetAllMessages());
                if (status.HasErrors)
                {
                    isOK = false;
                    //save errors
                    LoggingHelper.DoTrace(2, "Import. Errors encountered" + string.Join("\\r\\n", messages.ToArray()));
                }
                else
                {
                    if (status.Messages.Count > 0)
                        LoggingHelper.DoTrace(5, "Import" + string.Join("\\r\\n", messages.ToArray()));
                }



            }
            catch (Exception ex)
            {
                LoggingHelper.LogError(ex, "AssessmentsImport.UploadAssessments Unexpected error.");
                messages.Add(ex.Message);
            }


            return isOK;
        }

        public bool UploadFromFile(string file, AppUser user, string owningOrganizationRowID, ref int owningOrgId, ref List<string> messages)
        {
            bool isOK = true;

            ImportModel import = new ImportModel();
            DateTime start = DateTime.Now;

            messages = new List<string>();
            CheckForOwningOrg(owningOrganizationRowID, user, ref messages);

            if (messages.Count > 0)
                return false;
            try
            {

                using (CsvReader csv =
                   new CsvReader(new StreamReader(file), true))
                {
                    int fieldCount = csv.FieldCount;

                    string[] headers = csv.GetFieldHeaders();
                    //validate headers
                    if (!ValidateHeaders(headers, true, ref messages))
                    {
                        return false;
                    }
                    int rowNbr = 0;
                    csv.SkipEmptyLines = true;
                    while (csv.ReadNextRecord())
                    {
                        rowNbr++;
                        //entity = new Credential();
                        if (IsEmpty(csv, RequiredNbrOfColumns))
                        {
                            //brake assumes at end
                            break;
                        }
                        import = new ImportModel();
                        LoggingHelper.DoTrace(7, string.Format("UploadFromText. Row: {0}", rowNbr));
                        HandleRecord(rowNbr, csv, import, user, owningOrganizationRowID, ref messages);
                        //that latter may handle field level validations
                        importList.Add(import);

                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.IndexOf("item with the same key has already been added") > -1)
                    messages.Add("Error: An item with the same key has already been added - check for a column name being used more than once. ");
                else
                    messages.Add("Error unexpected exception was encountered: " + ex.Message);
            }

            return isOK;
        }
        public bool HandleRecord(int rowNbr, CsvReader csv, ImportModel entity, AppUser user, string owningOrganizationRowID, ref List<string> messages)
        {
            bool isValid = true;
            bool isPartialUpdate = false;
            entity.RowNumber = rowNbr;

            //validate org. Must exist
            //AssignOrgStuff(rowNbr, csv, entity, user, importHelper, ref messages);

            //NOTE: currently don't have a means for identifying partials. This method will attempt to set isPartialUpdate true if assessment exists.
            //CheckIdentifiers(rowNbr, csv, entity, user, ref messages, ref isPartialUpdate);

            entity.Name = Assign(rowNbr, csv, importHelper.NameHdr, "Assessment Name", ref messages, "", true);
			if ( entity.IsPotentialPartialUpdate )
			{
				//if set true, need to validate as an intended partial, or an actual error. 
				//more likely an error if for new credential
				//check for explicit append
				if ( entity.Action == "append" )
				{

				}
			}
			else
			{
                entity.Description = Assign(rowNbr, csv, importHelper.DescHdr, "Assessment Description", ref messages, "", (isPartialUpdate ? false : true), MinimumDescriptionLength);
                entity.SubjectWebpage = AssignUrl(rowNbr, csv, importHelper.SubjectWebpageHdr, "Assessment Subject webpage", ref messages, "", (isPartialUpdate ? false : true));

                //check for duplicates based on name/owner and SWP
                CheckForDuplicates(rowNbr, csv, entity, user, ref messages, ref isPartialUpdate);

                //check for credentials
                CheckTargetCredentials(rowNbr, csv, entity, user, ref messages, ref isPartialUpdate);

                //duration
                string durationDescr = "";
                if (importHelper.DurationDescHdr > -1)
                {
                    //NOTE: at this time, the duration is deleted, and then re-added, so suppress #DELETE
                    //WHOA  - if only description is entered, then need to allow delete value
                    //      - if have duration, and this is DELETE, THEN set blank
                    durationDescr = AssignProperty(rowNbr, csv, importHelper.DurationDescHdr, "DurationDescription", ref messages);
                    if (importHelper.DurationHdr == -1)
                    {
                        if (durationDescr == DELETE_ME)
                            entity.DeleteEstimatedDuration = true;
                        else
                            entity.EstimatedDuration.Description = durationDescr ?? "";
                    }
                }
                if (importHelper.DurationHdr > -1)
                {
                    string input = csv[importHelper.DurationHdr];
                    if (input == DELETE_ME)
                        entity.DeleteEstimatedDuration = true;
                    else
                    {
                        entity.EstimatedDuration = AssignDuration(rowNbr, csv, importHelper.DurationHdr, ref messages);
                        if (entity.HasEstimatedDuration)
                        {
                            entity.EstimatedDuration.Description = durationDescr == DELETE_ME ? "" : durationDescr;
                        }
                        else
                            entity.EstimatedDuration.Description = durationDescr ?? "";

                    }
                }

                //-------------------------
                if (importHelper.AudienceTypeHdr > -1)
                {
                    AssignAudienceTypes(rowNbr, csv, entity, user, importHelper.AudienceTypeHdr, ref messages);
                }
                entity.AssessmentExampleUrl = AssignUrl(rowNbr, csv, importHelper.AssessmentExampleUrlHdr, "AssessmentExampleUrl", ref messages);
                entity.AssessmentExampleDescription = Assign(rowNbr, csv, importHelper.AssessmentExampleDescriptionHdr, "Assessment Example Description", ref messages, "");
                //               
                entity.AssessmentMethodTypeList = AssignEnumeration(rowNbr, csv, user, importHelper.AssessmentMethodTypeHdr, "AssessmentMethodType", CodesManager.PROPERTY_CATEGORY_Assessment_Method_Type, ref prevAssessmentMethodType, ref thisAssessmentMethodType, ref previousAssessmentMethodTypes, ref messages);
                if (!string.IsNullOrWhiteSpace(entity.AssessmentMethodTypeList))
                {
                    if (entity.AssessmentMethodTypeList != DELETE_ME)
                        entity.AssessmentMethodType = thisAssessmentMethodType;
                }
                entity.AssessmentOutput = Assign(rowNbr, csv, importHelper.AssessmentOutputHdr, "Assessment Output", ref messages, "");
                //               
                entity.AssessmentUseTypeList = AssignEnumeration(rowNbr, csv, user, importHelper.AssessmentUseTypeHdr, "AssessmentUseType", CodesManager.PROPERTY_CATEGORY_ASSESSMENT_USE_TYPE, ref prevAssessmentUseType, ref thisAssessmentUseType, ref previousAssessmentUseTypes, ref messages);
                if (!string.IsNullOrWhiteSpace(entity.AssessmentUseTypeList))
                {
                    if (entity.AssessmentUseTypeList != DELETE_ME)
                        entity.AssessmentUseType = thisAssessmentUseType;
                }
				//
                entity.CodedNotation = AssignProperty(rowNbr, csv, importHelper.CodedNotationHdr, "CodedNotation", ref messages);
                //               
                entity.DeliveryTypeList = AssignEnumeration(rowNbr, csv, user, importHelper.DeliveryTypeHdr, "DeliveryType", CodesManager.PROPERTY_CATEGORY_DELIVERY_TYPE, ref prevDeliveryType, ref thisDeliveryType, ref previousDeliveryTypes, ref messages);
                if (!string.IsNullOrWhiteSpace(entity.DeliveryTypeList))
                {
                    if (entity.DeliveryTypeList != DELETE_ME)
                        entity.DeliveryType = thisDeliveryType;
                }
                entity.DeliveryTypeDescription = Assign(rowNbr, csv, importHelper.DeliveryTypeDescriptionHdr, "Delivery Type Description", ref messages, "");
                //     

                entity.ExternalResearch = AssignUrl(rowNbr, csv, importHelper.ExternalResearchHdr, "ExternalResearch", ref messages);
                entity.ProcessStandards = AssignUrl(rowNbr, csv, importHelper.ProcessStandardsHdr, "ProcessStandards", ref messages);
                entity.ProcessStandardsDescription = AssignProperty(rowNbr, csv, importHelper.ProcessStandardsDescHdr, "ProcessStandardsDescription", ref messages);
                //
                entity.ScoringMethodExample = AssignUrl(rowNbr, csv, importHelper.ScoringMethodExampleHdr, "ScoringMethodExample", ref messages);
                entity.ScoringMethodExampleDescription = Assign(rowNbr, csv, importHelper.ScoringMethodExampleDescriptionHdr, "ScoringMethodExample Description", ref messages, "");
                entity.ScoringMethodDescription = Assign(rowNbr, csv, importHelper.ScoringMethodDescriptionHdr, "Scoring Method Description", ref messages, "");
                entity.ScoringMethodTypeList = AssignEnumeration(rowNbr, csv, user, importHelper.ScoringMethodTypeHdr, "ScoringMethodType", CodesManager.PROPERTY_CATEGORY_Scoring_Method, ref prevScoringMethodType, ref thisScoringMethodType, ref previousScoringMethodTypes, ref messages);
                if (!string.IsNullOrWhiteSpace(entity.ScoringMethodTypeList))
                {
                    if (entity.ScoringMethodTypeList != DELETE_ME)
                        entity.ScoringMethodType = thisScoringMethodType;
                }

                entity.VerificationMethodDescription = Assign(rowNbr, csv, importHelper.VerificationMethodDescriptionHdr, "Verification Method Description", ref messages, "");
                entity.VersionIdentifier = Assign(rowNbr, csv, importHelper.VersionIdentifierHdr, "VersionIdentifier", ref messages, "");

                if (importHelper.DateEffectiveHdr > -1)
                {
                    entity.DateEffective = AssignDate(rowNbr, csv, importHelper.DateEffectiveHdr, "Date Effective", ref messages);
                }
                if (importHelper.AssessesCompetencyFrameworkHdr > -1)
                {
                    var competencyFramework = AssignProperty(rowNbr, csv, importHelper.AssessesCompetencyFrameworkHdr, "Assesses Competency Framework", ref messages);

                    if (competencyFramework != DELETE_ME)
                        AssignCompetencyFramework(rowNbr, csv, importHelper.AssessesCompetencyFrameworkHdr, entity, "Assesses Competency Framework", ref messages);
                    else
                    {
                        entity.DeleteFrameworks = true;
                    }
                }

                //entity.InLanguage = AssignProperty( rowNbr,csv,importHelper.InLanguageHdr,"InLanguage",ref messages,"English",false );
                //entity.InLanguage = AssignProperty(rowNbr, csv, importHelper.InLanguageSingleHdr, "LanguageSingle", ref messages, "English", false);
                entity.LanguageCodeList = AssignLanguages(rowNbr, csv, importHelper.InLanguageHdr, "Languages", ref messages, false);

                entity.Keywords = AssignList(rowNbr, csv, importHelper.KeywordsHdr, "Keywords", ref messages);

                entity.Subjects = AssignList(rowNbr, csv, importHelper.SubjectsHdr, "Subjects", ref messages);

				entity.NaicsCodesList = AssignNAICSList( rowNbr, csv, importHelper.NaicsListHdr, "NAICS", entity, ref messages );
				entity.Industries = AssignList( rowNbr, csv, importHelper.IndustriesHdr, "Industries", ref messages );

				entity.OnetCodesList = AssignSOCList( rowNbr, csv, importHelper.OnetListHdr, "SOC", entity, ref messages );
				entity.Occupations = AssignList( rowNbr, csv, importHelper.OccupationsHdr, "Occupations", ref messages );

				entity.CIPCodesList = AssignCIPList( rowNbr, csv, importHelper.CIPListHdr, "CIP", entity, ref messages );
				entity.Programs = AssignList( rowNbr, csv, importHelper.ProgramListHdr, "Programs", ref messages );
				//
				
				entity.IsProctored = AssignBool(rowNbr, csv, importHelper.IsProctoredHdr, "IsProctored", ref messages);
                entity.HasGroupEvaluation = AssignBool(rowNbr, csv, importHelper.HasGroupEvaluationHdr, "HasGroupEvaluation", ref messages);
                entity.HasGroupParticipation = AssignBool(rowNbr, csv, importHelper.HasGroupParticipationHdr, "HasGroupParticipation", ref messages);

                //roles
                AssignOfferedByList(rowNbr, csv, importHelper.OfferedByListHdr, entity, user, ref messages);
                AssignRecognizedByList(rowNbr, csv, importHelper.RecognizedByListHdr, entity, user, ref messages);

                //approved by
                if (importHelper.ApprovedByListHdr > -1)
                    AssignApprovedByList(rowNbr, csv, importHelper.ApprovedByListHdr, entity, user, ref messages);
                //accredited by
                if (importHelper.AccreditedByListHdr > -1)
                    AssignAccreditedByList(rowNbr, csv, importHelper.AccreditedByListHdr, entity, user, ref messages);
                AssignRegulatedByList(rowNbr, csv, importHelper.RegulatedByListHdr, entity, user, ref messages);

				//
				entity.AvailabilityListing = AssignUrl( rowNbr, csv, importHelper.AvailabilityListingHdr, "AvailabilityListing", ref messages );
				entity.AvailableOnlineAt = AssignUrl( rowNbr, csv, importHelper.AvailableOnlineAtHdr, "AvailableOnlineAt", ref messages );
				//addresses
				if (importHelper.AvailableAtHdr > 1 && importHelper.AvailableAtCodesHdr > 1)
                {
                    //error, only allow one type
                    messages.Add(string.Format("Row: {0}. You cannot use both Available At (column: {1}), and Available At Codes (column: {2}). Remove one of the columns and try again. ", rowNbr, importHelper.AvailableAtHdr, importHelper.AvailableAtCodesHdr));
                }
                else if (importHelper.AvailableAtHdr > 1)
                    AssignAddresses(rowNbr, csv, entity, user, importHelper, ref messages);
                else if (importHelper.AvailableAtCodesHdr > 1)
                    AssignAddressesViaCodes(rowNbr, csv, entity, user, importHelper, ref messages);

				if ( string.IsNullOrWhiteSpace( entity.AvailableOnlineAt ) && string.IsNullOrWhiteSpace( entity.AvailabilityListing )	 )
				{
					//addresses probably are not in the profile at this point
					if ( entity.AvailableAt == null || entity.AvailableAt.Count == 0 )
					{
						messages.Add( string.Format( "Row: {0}. At least one of: 'Available Online At', 'Availability Listing', or 'Available At' (address) must be entered",rowNbr) );
					}
				}

                //=======================================================================

                if (importHelper.CreditHourTypeHdr > -1)
                {
                    //Type of unit of time corresponding to type of credit such as semester hours, quarter hours, clock hours, or hours of participation.
                    entity.CreditHourType = Assign(rowNbr, csv, importHelper.CreditHourTypeHdr, "CreditHourType", ref messages);
                }
                if (importHelper.CreditHourValueHdr > -1)
                {
                    entity.CreditHourValue = AssignDecimal(rowNbr, csv, importHelper.CreditHourValueHdr, "CreditHourValue", ref messages, false);
                }

                if (importHelper.CreditUnitTypeHdr > -1)
                {
                    //Best practice is to use concepts from a controlled vocabulary such as ceterms:CreditUnit.
                    //actually only need the Id.
                    entity.CreditUnitType = Assign(rowNbr, csv, importHelper.CreditUnitTypeHdr, "CreditUnitType", ref messages);
                    if (!string.IsNullOrWhiteSpace(entity.CreditUnitType))
                    {
                        if (entity.CreditUnitType != DELETE_ME)
                        {
                            EnumeratedItem ei = CodesManager.GetCodeAsEnumerationItem(CodesManager.PROPERTY_CATEGORY_CREDIT_UNIT_TYPE, entity.CreditUnitType);
                            if (ei != null && ei.Id > 0)
                                entity.CreditUnitTypeId = ei.Id;
                            else
                                messages.Add(string.Format("Row: {0} Invalid {1} of {2}", rowNbr, "CreditUnitType", entity.CreditUnitType));
                        }
                    }
                } //
                if (importHelper.CreditUnitValueHdr > -1)
                {
                    entity.CreditUnitValue = AssignDecimal(rowNbr, csv, importHelper.CreditUnitValueHdr, "CreditUnitValue", ref messages, false);
                }

                if (importHelper.CreditUnitDescriptionHdr > -1)
                {
                    entity.CreditUnitTypeDescription = Assign(rowNbr, csv, importHelper.CreditUnitDescriptionHdr, "ConditionCreditUnitDescription", ref messages);
                }
                //
                if (importHelper.CommonConditionsHdr > 1)
                    AssignCommonConditions(rowNbr, csv, entity, user, importHelper, ref messages);
                if (importHelper.CommonCostsHdr > 1)
                    AssignCommonCosts(rowNbr, csv, entity, user, importHelper, ref messages);
            }


			//conditions
			if ( importHelper.HasConditionProfile )
				AssignConditionProfile( rowNbr, csv, entity, 3, user, importHelper, ref messages );

			//connections
			if ( importHelper.HasConnectionProfile )
				AssignConnectionProfile( rowNbr, csv, entity, 3, user, importHelper, ref messages );

			//AssignCosts
			if ( importHelper.HasCosts )
			{
				//AssignCosts(rowNbr, csv, entity, user, ref messages);
				AssignCosts( rowNbr, csv, entity, user, importHelper, entity.FoundExistingRecord, 3, entity.ExistingRecord.RowId, ref messages );

			}

			if (messages.Count > 0)
                isValid = false;
            return isValid;
        }

        private void CheckIdentifiers(int rowNbr, CsvReader csv, ImportModel entity, AppUser user, ref List<string> messages)
        {
            bool hasIdentifier = false;
            bool hasCtid = false;
            int startingCount = messages.Count;
			string entityType = "Assessment";
			entity.Action = "missing";
			if (importHelper.ActionHdr > -1)
            {
                entity.Action = Assign(rowNbr, csv, importHelper.ActionHdr, "Action", ref messages, "").ToLower();
                if ("new update append".IndexOf(entity.Action.ToLower()) == -1)
                {
                    messages.Add(string.Format("Row: {0} Invalid Action property: {1}. Valid values are: Add, Update, Append (used for additional rows for the primary assessment row).", rowNbr, entity.ExternalIdentifier));
                }
                else
                {
					//can only have append, if there is a preceding New/Update
					//how to check? - probably on return 
					if ( entity.Action == "append" )
						entity.IsPotentialPartialUpdate = true;
				}
            } //

			//get the name for use in comparisons
			entity.Name = Assign( rowNbr, csv, importHelper.NameHdr, "Assessment Name", ref messages, "", true );
			if (importHelper.ExternalIdentifierHdr > -1)
            {
                bool isRequired = true;
                if (importHelper.CtidHdr > -1)
                    isRequired = false;
                entity.ExternalIdentifier = Assign(rowNbr, csv, importHelper.ExternalIdentifierHdr, "Assessment Identifier", ref messages, "", isRequired);

                if (entity.ExternalIdentifier == DELETE_ME)
                {
                    //could allow for an existing record
                    messages.Add(string.Format("Row: {0} The external identifier cannot be deleted {1}", rowNbr, entity.ExternalIdentifier));
                }
                else if (entity.ExternalIdentifier.Length > 50)
                {
                    messages.Add(string.Format("Row: {0} The external identifier {1} must be less than or equal to 50 characters in length", rowNbr, entity.ExternalIdentifier));
                }
                else if (!string.IsNullOrWhiteSpace(entity.ExternalIdentifier))
                {
                    hasIdentifier = true;
                    //need a validation that external identifier is not used more than once
                    int index = externalIdentifiers.FindIndex(a => a == entity.ExternalIdentifier);
                    var exists = importList.FirstOrDefault(a => a.ExternalIdentifier == entity.ExternalIdentifier);
					if ( index > -1 || ( exists != null && exists.FoundExistingRecord ) )
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
                        if (entity.Action == "append")
                        {
                            //can only have append, if there is a preceding New/Update
                            messages.Add(string.Format("Row: {0} Error the current record has an Action of 'Append', but the External Identifier: {1}, has not been used in a previous row. Append can only be used with a previous row that is for a full assessment.", rowNbr, entity.ExternalIdentifier));
                        }
                        else
                        {
                            externalIdentifiers.Add(entity.ExternalIdentifier);
                            //may need to do a lookup, if no CTID? While CTID is in export, an update could be triggered from an external source
                        }

                    }
                }
            }
            //may want to allow both
            if (importHelper.CtidHdr > -1)
            {
                bool isRequired = true;
				//OR should it be required, if in the header?
				//if we also have an external identifier, then what
				if ( !string.IsNullOrWhiteSpace( entity.ExternalIdentifier ) )
					isRequired = false;
				entity.CTID = Assign(rowNbr, csv, importHelper.CtidHdr, "Assessment CTID", ref messages, "", isRequired);
                if (!string.IsNullOrWhiteSpace(entity.CTID))
                {
                    hasCtid = true;
                    //need a validation that a CTID is not used more than once
                    //18-05-07 - actually may be possible for updates - for example for multiple conditions
                    //          - could allow if the cred name is same for each
                    int index = ctidsList.FindIndex(a => a == entity.CTID);
                    var exists = importList.FirstOrDefault(a => a.CTID == entity.CTID);
					if ( index > -1 || ( exists != null && exists.FoundExistingRecord ) )
					{
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
                    else if (entity.Action == "append")
                    {
                        //can only have append, if there is a preceding New/Update
                        messages.Add(string.Format("Row: {0} Error the current record has an Action of 'Append', but the CTID: {1}, has not been used in a previous row. Append can only be used with a previous row that is for a full assessment.", rowNbr, entity.CTID));
                    }
                    else
                    {
                        ctidsList.Add(entity.CTID);
                    }
                }
            }
            if (!hasCtid && !hasIdentifier)
            {
                messages.Add(string.Format("Row: {0} Error either a CTID and/or a unique identifier must be provided.", rowNbr));
            }
            //if any new messages, exit
            if (startingCount < messages.Count)
                return;

            if (!string.IsNullOrWhiteSpace(entity.OwningOrganizationCtid))
            {
                if (hasCtid)
                {
                    entity.ExistingRecord = DBMgr.GetByCtid(entity.CTID);
                    if (entity.FoundExistingRecord)
                    {
                        entity.IsExistingEntity = true;
						entity.ExistingParentRowId = entity.ExistingRecord.RowId;
                        entity.ExistingParentId = entity.ExistingRecord.Id;
                        entity.ExistingParentTypeId = CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE;
                        if (entity.ExistingRecord.OwningAgentUid != entity.OwningAgentUid)
                        {
                            messages.Add(string.Format("Row: {0} Error a assessment CTID was provided for an existing assessment, and it has a different owning organization: '{1}' than that designated for this upload {2}.", rowNbr, entity.ExistingRecord.OrganizationName, entity.OrganizationName));
                        }
                    }
                }
                else //must have unique id to get here
                {
                    //do we have a check that identifier is unique to org
                    //Warning: if assessment is copied, then combination of unique Id, and org Uid will not exist, but may be invalid. This is more likely in the test environment. 
                    entity.ExistingRecord = DBMgr.GetBasicByUniqueId(entity.ExternalIdentifier, entity.OwningAgentUid);
                    if (entity.FoundExistingRecord)
                    {
                        entity.IsExistingEntity = true;
						entity.ExistingParentRowId = entity.ExistingRecord.RowId;
                        entity.ExistingParentId = entity.ExistingRecord.Id;
                        entity.ExistingParentTypeId = CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE;
                        entity.CTID = entity.ExistingRecord.CTID;
                    }

                }
            }
        }

        private void CheckTargetCredentials(int rowNbr, CsvReader csv, ImportModel entity, AppUser user, ref List<string> messages, ref bool isPartialUpdate)
        {
            if (importHelper.TargetCredentialListHdr == -1)
            {
                //this could be a warning, if allowing multiple approaches
                messages.Add(string.Format("Row: {0} One or more target credentials must be included with an assessment.", rowNbr));
                return;
            } //
            string conditionType = "";
            int credId = 0;
            if (importHelper.CredentialConditionTypeHdr == -1)
            {
                //default to requires
                conditionType = "requires";
                entity.CredentialConditionTypeId = Entity_ConditionProfileManager.GetConditionTypeId(conditionType);
            }
            else
            {
                conditionType = Assign(rowNbr, csv, importHelper.CredentialConditionTypeHdr, "ConditionType", ref messages, "", false);
                if (conditionType == DELETE_ME)
                {
                    messages.Add(string.Format("Row: {0}. Credential Condition Type : Invalid delete request for a condition profile. The condition profile for a credential cannot be deleted from an assessment: {1}", rowNbr, conditionType));
                }
                else
                {
                    if (conditionType.IndexOf("|") > -1)
                    {
                        //multiples can occur in an export. remove extras
                        conditionType = conditionType.Substring(0, conditionType.IndexOf("|"));
                    }
                    if ("requires recommends renewal".IndexOf(conditionType.ToLower()) == -1)
                    {
                        messages.Add(string.Format("Row: {0}. The provided Credential Condition Type ({1}) is invalid. It must be one of Requires, Recommends or Renewal.", rowNbr, conditionType));
                    }
                    else
                    {
                        //if identifier provided, the type must match actual value
                        entity.CredentialConditionTypeId = Entity_ConditionProfileManager.GetConditionTypeId(conditionType);
                    }
                }
            }

            List<string> credentialList = AssignList(rowNbr, csv, importHelper.TargetCredentialListHdr, "TargetCredentialList", ref messages, true);
            if (credentialList.Count == 0)
            {
                messages.Add(string.Format("Row: {0} One or more target credentials must be included with an assessment.", rowNbr));
                return;
            }
            else if (credentialList[0] == DELETE_ME)
            {
                messages.Add(string.Format("Row: {0} Invalid content for target credentials: {1}. You cannot delete all credential references from here.", rowNbr, credentialList[0]));
                return;
            }
            else if (credentialList[0].ToLower() == "#defer")
            {
                //interim means to not require credentials here
                return;
            } //

            //TODO - WARNING If referencing a credential for a different org, should not be able to create a condition profile. 
            //  if this condition is allowed within publisher, may require creating a reference object, and create an asmt condition profile, with an Is Required For type (or Is Recommended For?)
            int itemNbr = 0;
            foreach (var item in credentialList)
            {
                itemNbr++;
                credId = 0;
                if (item.StartsWith("*") && item.Length > 1)
                {
                    string itemId = item.Substring(1);
                    if (Int32.TryParse(itemId, out credId))
                    {
                        //action?
                    }
                }
                else if (ServiceHelper.IsInteger(item))
                {
                    if (Int32.TryParse(item, out credId))
                    {
                        //action?
                    }
                }
                //an integer could be an internal id, or external identifier
                //may have to do both lookups. The use of an external id, should fairly clearly imply the same org.
                if (credId > 0)
                {
                    //try first as external identifier and this org.
                    var ec = CredentialManager.GetBasicByUniqueId(credId.ToString(), entity.OwningAgentUid, true);
                    if (ec != null && ec.Id > 0)
                    {
                        entity.TargetCredentials.Add(ec);
                        continue;
                    }
                    else
                    {
                        var ic = CredentialManager.GetBasic(credId, true);
                        if (ic != null && ic.Id > 0)
                        {
                            //flag if not same org
                            if (ic.OwningAgentUid != entity.OwningAgentUid)
                            {
								//or add in import step, perhaps a warning
								if ( CanReferenceCredentialFromDifferentOrg )
									warnings.Add( string.Format( "Row: {0}. WARNING. The owner ('{1}') for the target credential ('{2}') is different than the owner selected for this assessment. This is not a fully supported scenario", rowNbr, ic.OwningOrganization.Name, ic.Name ) );
								else
									messages.Add( string.Format( "Row: {0}. ERROR. The owner ('{1}') for the target credential ('{2}') is different than the owner selected for this assessment. This is not a supported scenario at this time.", rowNbr, ic.OwningOrganization.Name, ic.Name ) );
							}
                            entity.TargetCredentials.Add(ic);
                            continue;
                        }
                    }
                }
				//allow ctids, or unique identifiers
				//if ( ServiceHelper.IsValidCtid( item, ref messages ) )
				if ( item.Length == 39 && item.ToLower().StartsWith("ce-"))
                {
                    var c = CredentialManager.GetByCtid(item.ToLower(), true);
                    //not sure if owning orgs need to match. maybe not
                    //cannot do reference creds, while this may make sense - actually all about creds
                    if (c.Id > 0)
                    {
                        entity.TargetCredentials.Add(c);
                    }
                    else
                    {
                        messages.Add(string.Format("Row: {0} Target credential with CTID: {1} was not found.", rowNbr, item));
                    }
                }

                else
                {
                    //a unique external identifier that must exist
                    var c = CredentialManager.GetBasicByUniqueId(item, entity.OwningAgentUid, true);
                    if (c.Id > 0)
                    {
                        entity.TargetCredentials.Add(c);
                    }
                    else
                    {
                        messages.Add(string.Format("Row: {0} Target credential with Unique Identifier: {1} was not found.", rowNbr, item));
                    }
                }
            }
        }

        private void CheckForDuplicates(int rowNbr, CsvReader csv, ImportModel entity, AppUser user, ref List<string> messages, ref bool isPartialUpdate)
        {
            //although could be changing SWP for an existing cred
            //|| entity.IsExistingCredential
            if (string.IsNullOrWhiteSpace(entity.SubjectWebpage))
                return;
            //skip if an example url
            if (entity.SubjectWebpage.ToLower().IndexOf("example.com") > -1)
                return;
            //normalize
            string url = CredentialManager.NormalizeUrlData(entity.SubjectWebpage);
            url = ServiceHelper.HandleApostrophes(url);

            int ptotalRows = 0;
            //search does a like (trims a trailing /), so need to check carefully
            //should actually trim off the protocol
            List<ThisEntity> exists = DBMgr.SearchByUrl(url, "", 1, 100, ref ptotalRows);
            foreach (var item in exists)
            {
                string resultUrl = CredentialManager.NormalizeUrlData(item.SubjectWebpage);
                if (resultUrl.ToLower() == url.ToLower())
                {
                    if (entity.FoundExistingRecord && entity.ExistingRecord.Id == item.Id)
                    {
                        //check if changed owning org
                        if (item.OwningAgentUid.ToString().ToLower() != entity.OwningAgentUid.ToString().ToLower())
                        {
                            //don't allow, at least until can do more stringent checks, like is user related to previous org
                            if (OrganizationServices.CanUserUpdateOrganization(user, entity.OwningAgentUid) == false)
                            {
                                messages.Add(string.Format("Row: {0}. Issue: Attempting to change the owning organization for an existing assessment. The entered assessment already exists with an owning organization of '{1}'. You do not have update rights for the latter organization and so the system cannot allow you to change the owning organization to: '{2}'. ", rowNbr, item.OwningOrganization.Name, defaultOwningOrg.Name));
                            }
                        }
                    }
                    else
                    {
                        string msg = "";
                        if (item.OwningAgentUid.ToString().ToLower() != entity.OwningAgentUid.ToString().ToLower())
                        {
                            if (isProduction)
                                messages.Add(string.Format("Row: {0}. The subject webpage for this assessment already exists for assessment: '{1}' (#{2}), with an owning organization of '{3}' . This appears to be an invalid scenario. ", rowNbr, item.Name, item.Id, item.OwningOrganization.Name));
                            else
                                warnings.Add(string.Format("Row: {0}. WARNING. The subject webpage for this assessment already exists for assessment: '{1}' (#{2}), with an owning organization of '{3}' . This appears to be an invalid scenario. ", rowNbr, item.Name, item.Id, item.OwningOrganization.Name));
                        }
                        else
                        {
                            if (entity.Name == item.Name)
                            {
                                if (isProduction)
                                    messages.Add(string.Format("Row: {0}. The subject webpage for this assessment already exists for assessment with the same name: '{1}' (#{2}). This appears to be a duplicate. ", rowNbr, item.Name, item.Id));
                                else
                                    warnings.Add(string.Format("Row: {0}. WARNING. The subject webpage for this assessment already exists for assessment with the same name: '{1}' (#{2}). This appears to be a duplicate. ", rowNbr, item.Name, item.Id));
                            }
                            //else
                            //    messages.Add( string.Format( "Row: {0}. The subject webpage for this assessment already exists for assessment: '{1}' (#{2}). This appears to be a duplicate. ", rowNbr, item.Name, item.Id ) );
                        }
                    }
                    break;
                }
                else if (item.SubjectWebpage.ToLower().IndexOf(url) > -1)
                {
                    //now what if close? some trailing parameter
                    LoggingHelper.DoTrace(2, string.Format("*** Bulk upload WARNING possible duplicates. upload.Name: {0}, upload.SubjectWebpage: {1}, existing.Name: {2}, existing.Id: {3}, existing.SubjectWebpage: {4}, organizationId: {5}", entity.Name, entity.SubjectWebpage, item.Name, item.Id, item.SubjectWebpage, defaultOwningOrg.Id));
                }



            }
        }
        #region Assignments
        //public void AssignAudienceTypes(int rowNbr, CsvReader csv, ImportModel entity, AppUser user, ref List<string> messages)
        //{
        //    entity.AudienceTypesList = AssignProperty(rowNbr, csv, importHelper.AudienceTypeHdr, "AudienceType", ref messages);
        //    if ( string.IsNullOrWhiteSpace(entity.AudienceTypesList) || entity.AudienceTypesList == DELETE_ME )
        //        return;

        //    if ( prevAudienceType == entity.AudienceTypesList )
        //    {
        //        entity.AudienceType = audienceType;
        //    }
        //    else
        //    {
        //        audienceType = new Enumeration() { Id = CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE };
        //        var itemList = entity.AudienceTypesList.Split('|');
        //        foreach ( var item in itemList )
        //        {
        //            EnumeratedItem ei = CodesManager.GetCodeAsEnumerationItem(CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE, item);
        //            if ( ei != null && ei.Id > 0 )
        //                audienceType.Items.Add(ei);
        //            else
        //                messages.Add(string.Format("Row: {0} Invalid audience type of {1}", rowNbr, item));
        //        }

        //        if ( audienceType != null && audienceType.Items.Count > 0 )
        //        {
        //            entity.AudienceType = audienceType;
        //            prevAudienceType = entity.AudienceTypesList;
        //        }

        //    }
        //}
        #endregion

        #region Validations
        public bool ValidateHeaders(string[] headers, bool doingMinimumRequiredChecks, ref List<string> messages)
        {
            /*string action, 
             * if ( headers == null 
                || (action != PARTIAL_UPDATE && headers.Count() < RequiredNbrOfColumns)
                || ( action == PARTIAL_UPDATE && headers.Count() < 4 )
                )
             * 
             */
            bool isValid = true;
            if (headers == null
                || (!doingMinimumRequiredChecks && headers.Count() < RequiredNbrOfColumns)
                || (doingMinimumRequiredChecks && headers.Count() < 4)
                )
            {
                messages.Add("Error - the input file must have a header row with at least the required columns");
                return false;
            }
            int cntr = -1;

            try
            {
                #region Check header columns
                foreach (var item in headers)
                {
                    cntr++;
                    string colname = item.ToLower().Replace(" ", "").Replace("*", "").Replace(":", ".");
                    if (colname.StartsWith("cost"))
                    {
                        #region cost profile related
                        switch (colname)
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
                                messages.Add("Error unknown column header encountered: " + item);
                                break;
                        }
                        #endregion
                    }
                    else if (colname.StartsWith("condition") || colname.StartsWith("requires"))
                    {
                        #region condition profile related
                        switch (colname)
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
                            case "requires.assessment_ctid":
                                importHelper.ConditionExistingAsmtHdr = cntr;
                                importHelper.HasConditionProfile = true;
                                break;
                            case "conditionprofile.assessment_identifier":
                            case "requires.assessment_identifier":
                                importHelper.ConditionAsmtIdentifierHdr = cntr;
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
                                messages.Add("Error unknown column header encountered: " + item);
                                break;
                        }
                        #endregion
                    }
                    else if (colname.StartsWith("connection"))
                    {
                        #region CONNECTION profile related
                        switch (colname)
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
                                messages.Add("Error unknown column header encountered: " + item);
                                break;
                        }
                        #endregion
                    }
                    else
                        switch (colname)
                        {
                            case "externalidentifier":
                            case "uniqueidentifier":
                            case "identifier":
                                importHelper.ExternalIdentifierHdr = cntr;
                                break;
							case "internalidentifier":
								importHelper.InternalIdentifierHdr = cntr;
								break;
							case "targetcredentials":
                            case "relatedcredentials":
                                importHelper.TargetCredentialListHdr = cntr;
                                break;
                            case "credentialconditiontype":
                                importHelper.CredentialConditionTypeHdr = cntr;
                                break;
                            case "action":
                                importHelper.ActionHdr = cntr;
                                break;
                            case "ctid":
                                //ctid will not be required if external identifier is present
                                importHelper.CtidHdr = cntr;
                                break;
                            case "name":
                            case "assessmentname":
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

                            case "availableonlineat":
                                importHelper.AvailableOnlineAtHdr = cntr;
                                break;
                            case "availabilitylisting":
                                importHelper.AvailabilityListingHdr = cntr;

                                break;
                            case "assessmentexampleurl":
                            case "assessmentexample":
                            case "exampleurl":
                                importHelper.AssessmentExampleUrlHdr = cntr;
                                break;
                            case "assessmentexampledescription":
                            case "exampledescription":
                                importHelper.AssessmentExampleDescriptionHdr = cntr;
                                break;
                            case "assessmentmethodtype":
                                importHelper.AssessmentMethodTypeHdr = cntr;
                                break;
                            case "assessmentoutput":
                                importHelper.AssessmentOutputHdr = cntr;
                                break;
                            case "assessmentusetype":
                                importHelper.AssessmentUseTypeHdr = cntr;
                                break;
                            case "codednotation":
                                importHelper.CodedNotationHdr = cntr;
                                break;
                            case "credithourvalue":
                                importHelper.CreditHourValueHdr = cntr;
                                break;
                            case "credithourtype":
                                importHelper.CreditHourTypeHdr = cntr;
                                break;
                            case "creditunittype":
                                importHelper.CreditUnitTypeHdr = cntr;
                                break;
                            case "creditunitvalue":
                                importHelper.CreditUnitValueHdr = cntr;
                                break;
                            case "creditunittypedescription":
                                importHelper.CreditUnitDescriptionHdr = cntr;
                                break;
                            case "deliverytype":
                                importHelper.DeliveryTypeHdr = cntr;
                                break;
                            case "deliverytypedescription":
                                importHelper.DeliveryTypeDescriptionHdr = cntr;
                                break;
                            case "externalresearch":
                                importHelper.ExternalResearchHdr = cntr;
                                break;
                            case "hasgroupevaluation":
                                importHelper.HasGroupEvaluationHdr = cntr;
                                break;
                            case "hasgroupparticipation":
                                importHelper.HasGroupParticipationHdr = cntr;
                                break;
                            case "isproctored":
                                importHelper.IsProctoredHdr = cntr;
                                break;

                            case "processstandards":
                                importHelper.ProcessStandardsHdr = cntr;
                                break;
                            case "processstandardsdescription":
                                importHelper.ProcessStandardsDescHdr = cntr;
                                break;
                            case "scoringmethodtype":
                                importHelper.ScoringMethodTypeHdr = cntr;
                                break;
                            case "scoringmethoddescription":
                                importHelper.ScoringMethodDescriptionHdr = cntr;
                                break;
                            case "scoringmethodexampledescription":
                                importHelper.ScoringMethodExampleDescriptionHdr = cntr;
                                break;
                            case "scoringmethodexample":
                                importHelper.ScoringMethodExampleHdr = cntr;
                                break;
                            case "verificationmethoddescription":
                                importHelper.VerificationMethodDescriptionHdr = cntr;
                                break;
                            case "versionidentifier":
                                importHelper.VersionIdentifierHdr = cntr;
                                break;
                            //not sure if will handle both options in one column
                            case "addresses":
                            case "availableat":
                                importHelper.AvailableAtHdr = cntr;
                                break;
                            case "availableatcodes":
                                importHelper.AvailableAtCodesHdr = cntr;
                                break;

                            case "language":
                                importHelper.InLanguageHdr = cntr;
                                break;
                            case "dateeffective":
                                importHelper.DateEffectiveHdr = cntr;
                                break;
                            case "audiencetype":
                                importHelper.AudienceTypeHdr = cntr;
                                break;

                            case "cip":
                            case "ciplist":
                                importHelper.CIPListHdr = cntr;
                                break;

                            case "programs":
                            case "instructionalprogramtype":
                                importHelper.ProgramListHdr = cntr;
                                break;
                            case "keywords":
                            case "keyword":
                                importHelper.KeywordsHdr = cntr;
                                break;
                            case "assessescompetencyframework":
                            case "assessescompetencyframeworks":
                                importHelper.AssessesCompetencyFrameworkHdr = cntr;
                                break;
                            case "subjects":
                            case "subject":
                                importHelper.SubjectsHdr = cntr;
                                break;
                            case "industries":
                            case "industrytype":
                                importHelper.IndustriesHdr = cntr;
                                break;
                            case "occupations":
                            case "occupationtype":
                                importHelper.OccupationsHdr = cntr;
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
                            //case "durationamount":
                            //    importHelper.DurationAmountHdr = cntr;
                            //    break;
                            //case "durationunit":
                            //    importHelper.DurationTypeHdr = cntr;
                            //    break;

                            case "commonconditions":
                                importHelper.CommonConditionsHdr = cntr;
                                break;
                            case "commoncosts":
                                importHelper.CommonCostsHdr = cntr;
                                break;

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
                            case "regulatedbylist":
                            case "regulatedby":
                                importHelper.RegulatedByListHdr = cntr;
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
                            case "targetcredentialname":
                                //IGNORE
                                break;
                            default:
                                //action?
                                if (colname.IndexOf("column") > -1)
                                    break;
                                messages.Add("Error unknown column header encountered: " + item);
                                break;
                        }
                }
                #endregion

                if (importHelper.CtidHdr == -1 && importHelper.ExternalIdentifierHdr == -1)
                    messages.Add("Error - Either an identifier from the source system or a ctid must be provided to uniquely identify an input record.");
                if (importHelper.OrganizationCtidHdr == -1
                && (defaultOwningOrg == null || defaultOwningOrg.Id == 0))
                    messages.Add("Error - An owning organization CTID column (Owned By) must be provided (or set in the interface).");

                //TBD - will name be required for an update - for now yes
                if (importHelper.NameHdr == -1)
                    messages.Add("Error - An assessment name column must be provided");

                //TBD: 
                if (importHelper.TargetCredentialListHdr == -1)
                {
                    messages.Add("Error - Target Credential(s) must be included with assessments.");
                }

                if (doingMinimumRequiredChecks)
                {
                    //will have to defer these checks - only if a new Assessment
                    if (importHelper.DescHdr == -1)
                        messages.Add("Error - A Assessment description column must be provided");

                    if (importHelper.SubjectWebpageHdr == -1)
                        messages.Add("Error - A Assessment subject webpage column must be provided");

                    //needs to have one of availableOnlineAt, availabilityListing, address
                    if (importHelper.AvailableOnlineAtHdr == -1 && importHelper.AvailabilityListingHdr == -1
                        && importHelper.AvailableAtHdr == -1 && importHelper.AvailableAtCodesHdr == -1)
                        messages.Add("Error - At least one of: Available OnlineAt, AvailabilityListing, or Available At (address reference) must be provided");
                }

                //if any cost data, then check for minimum properties
                if (importHelper.HasCosts)
                {
                    if (importHelper.CostDetailUrlHdr == -1)
                        messages.Add("Error - If any costs are entered, a cost detail url is required");

                    if (importHelper.CostDescriptionHdr == -1)
                        messages.Add("Error - If any costs are entered, a cost description is required");
                }

            }
            catch (Exception ex)
            {
                string msg = BaseFactory.FormatExceptions(ex);
                LoggingHelper.DoTrace(1, "Exception encountered will validating headers for Assessment upload: " + msg);
                messages.Add("Exception encountered will validating headers for Assessment upload: " + msg);
            }
            if (messages.Count > 0)
                isValid = false;

            return isValid;
        }//
        #endregion
    }
}
