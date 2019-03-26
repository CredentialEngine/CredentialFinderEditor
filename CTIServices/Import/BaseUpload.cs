using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LumenWorks.Framework.IO.Csv;
using Newtonsoft.Json;

using CTIServices;
using Factories;
using Models;
using Models.Common;
using Models.Import;
using MN = Models.Node;
using Models.ProfileModels;
using Utilities;

namespace CTIServices.Import
{
	public class BaseUpload
	{
		#region properties - general
		public int rowNbr = 1;
		public bool isProduction = UtilityManager.GetAppKeyValue( "envType" ) == "production";
		public static bool doingExistanceCheck = true;
		public string messageTemplate = "Row: {0} Name: {1}";
		public string DELETE_ME = "#DELETE";
		public string SAMPLE = "SAMPLE:";
		public string NEW_ID = "#NEW";
		public string SAME_AS_OWNER = "same as owner";
		public string PARTIAL_UPDATE = "partial";
		public bool IsPartialUpdate = false;
		public static int MinimumDescriptionLength = UtilityManager.GetAppKeyValue( "minDescriptionTextLength",25);
		public static bool CanReferenceCredentialFromDifferentOrg = UtilityManager.GetAppKeyValue( "canReferenceCredentialFromDifferentOrg", false );
		public List<string> warnings = new List<string>();
		public List<string> previousUrls = new List<string>();

		//
		string previousCreditUnitTypes = "";
		Enumeration prevCreditUnitType = new Enumeration();
		Enumeration thisCreditUnitType = new Enumeration();

		#endregion

		#region properties - organization
		public List<string> externalIdentifiers { get; set; } = new List<string>();

		public List<string> ctidsList { get; set; } = new List<string>();

		public OrganizationManager orgMgr = new OrganizationManager();
		public Organization defaultOwningOrg = new Organization();
		public string prevOrgCtid = "-1";
		public string prevOrgIdentifier = "-1";
		public string prevOrgName = "";
		public string prevOrgRoles = "";
		public Guid prevOwningAgentUid = Guid.NewGuid();
		public Enumeration ownerRoles = new Enumeration();
		public List<Organization> approvedByList = new List<Organization>();
		public string prevApprovedByList = "-1";

		public List<Organization> accreditedByList = new List<Organization>();
		public string prevAccreditedByList = "-1";

		public List<Organization> offeredByList = new List<Organization>();
		public string prevOfferedByList = "-1";

        public List<Credential> targetCredentialList = new List<Credential>();
        public string prevTargetCredentialList = "-1";
        public List<Credential> prevRefCreds = new List<Credential>();
        public List<string> prevRefCredPairs = new List<string>();
        public CredentialManager credMgr = new CredentialManager();

        public List<AssessmentProfile> targetAssessmentList = new List<AssessmentProfile>();
        public string prevTargetAssessmentList = "-1";
        public List<AssessmentProfile> prevRefAssmts = new List<AssessmentProfile>();
        public List<string> prevRefAssmtPairs = new List<string>();
        public AssessmentManager assmMgr = new AssessmentManager();

        public List<LearningOpportunityProfile> targetLearningOppList = new List<LearningOpportunityProfile>();
        public string prevTargetLearningOppList = "-1";
        public List<LearningOpportunityProfile> prevRefLopps = new List<LearningOpportunityProfile>();
        public List<string> prevRefLoppPairs = new List<string>();
        public LearningOpportunityManager loppMgr = new LearningOpportunityManager();

        public List<Organization> recognizedByList = new List<Organization>();
		public Organization recognizedByOrg = new Organization();
		public string prevRecognizedByList = "-1";

		public List<Organization> regulatedByList = new List<Organization>();
		public Organization regulatedByOrg = new Organization();
		public string prevRegulatedByList = "-1";

		public List<Organization> prevRefOrgs = new List<Organization>();
        
		public List<string> prevRefOrgPairs = new List<string>();
        public string prevAddressIdentifiers = "-1";
		public List<Address> orgAddresses { get; set; } = new List<Address>();
		public List<Address> prevOrgAddresses { get; set; } = new List<Address>();

		public List<CostManifest> orgCostManifests { get; set; } = new List<CostManifest>();
		public List<ConditionManifest> orgConditionManifests { get; set; } = new List<ConditionManifest>();
		public string prevCommonCostIdentifiers = "-1";

		public List<int> prevCommonCosts { get; set; } = new List<int>();

		public string prevCommonConditionsIdentifiers = "-1";

		public List<int> prevCommonConditions { get; set; } = new List<int>();
		//
		public string prevAudienceLevelType = "";
		public Enumeration audienceLevelType = new Enumeration();
		//
		public string prevAudienceTypeList = "";
		public Enumeration prevAudienceType = new Enumeration();
		//
		public string prevLanguage = "";
		public string prevDuration = "";
		public string prevTargetCtid = "-1";
		//not sure if needed, less likely for reuse
		public Credential lastCredential = new Credential();
		public AssessmentProfile lastAsmt = new AssessmentProfile();
		public LearningOpportunityProfile lastLopp = new LearningOpportunityProfile();
		#endregion


		protected void CheckForOwningOrg( string owningOrganizationRowID, AppUser user, ref List<string> messages )
		{
			//if owningOrganizationRowID is provided, then no owning org column is needed
			//NOTE: need to handle where could still exist in an older spreadsheet
			if ( !string.IsNullOrWhiteSpace( owningOrganizationRowID ) )
			{
				if ( ServiceHelper.IsValidGuid( owningOrganizationRowID ) )
				{
					defaultOwningOrg = OrganizationManager.GetForSummary( new Guid( owningOrganizationRowID ) );
					if ( defaultOwningOrg == null || defaultOwningOrg.Id == 0 )
					{
						messages.Add( string.Format( " An organization was not found for the provided Owning Organization unique identifier: {0}", owningOrganizationRowID ) );
					}
					else
					{
						if ( OrganizationServices.CanUserUpdateOrganization( user, defaultOwningOrg.RowId ) == false )
						{
							messages.Add( string.Format( "You do not have update rights for the provided Owning organization (selected in interface): {0} ({1}). ", defaultOwningOrg.Name, defaultOwningOrg.Id ) );
						}
						else
						{
							SetOwnedByRole( ref messages );
							orgCostManifests = CostManifestManager.GetAll( defaultOwningOrg.RowId, true );
							orgConditionManifests = ConditionManifestManager.GetAll( defaultOwningOrg.RowId, true );
							prevOrgCtid = defaultOwningOrg.CTID;
							prevOwningAgentUid = defaultOwningOrg.RowId;
						}
					}
				}
				else
				{
					messages.Add( string.Format( " The identifier provided for the Owning Organization is not a valid unique identifer: {0}", owningOrganizationRowID ) );
				}
			}
		}

		public void AssignOrgStuff( int rowNbr, CsvReader csv, BaseDTO entity, AppUser user, CommonImportRequest importHelper, ref List<string> messages )
		{
			int msgcnt = messages.Count;
			#region  one of following for org
			if ( importHelper.OrganizationCtidHdr == -1 )
			{
				//to get this far, must have a valid owning org
				entity.OwningAgentUid = defaultOwningOrg.RowId;
				entity.OrganizationId = defaultOwningOrg.Id;
				entity.OrganizationName = defaultOwningOrg.Name;
				entity.OwningOrganizationCtid = defaultOwningOrg.ctid;
				entity.OwnerRoles = ownerRoles;
			}
			else
			{
				//if column include, must have data - unless??
				//FUTURE ISSUE: org a owns credential, and this is org b that offers the credential. How to signal that this condition is valid.
				entity.OwningOrganizationCtid = Assign( rowNbr, csv, importHelper.OrganizationCtidHdr, "OrganizationCtid", ref messages, "", false );
				if ( entity.OwningOrganizationCtid.ToLower() == "me" )
					entity.OwningOrganizationCtid = defaultOwningOrg.CTID;
				else if ( string.IsNullOrWhiteSpace( entity.OwningOrganizationCtid ) )
				{
					if ( defaultOwningOrg != null && defaultOwningOrg.Id > 0 )
					{
						// just set CTID, and fall thru in case need to reset addresses, etc.
						entity.OwningOrganizationCtid = defaultOwningOrg.ctid;
						entity.OwningAgentUid = defaultOwningOrg.RowId;
						entity.OrganizationId = defaultOwningOrg.Id;
						entity.OrganizationName = defaultOwningOrg.Name;
						entity.OwnerRoles = ownerRoles;
					}
				}

				if ( prevOrgCtid == entity.OwningOrganizationCtid )
				{
					entity.OwningAgentUid = prevOwningAgentUid;
					entity.OrganizationId = defaultOwningOrg.Id;
					entity.OrganizationName = defaultOwningOrg.Name;
					//handles bug, should be able to remove
					if ( ownerRoles == null || ownerRoles.Items.Count == 0 )
					{
						EnumeratedItem item = Entity_AgentRelationshipManager.GetAgentRole( "Owned By" );
						if ( item == null || item.Id == 0 )
						{
							messages.Add( string.Format( "Row: {0} The organization role: {1} is not valid", rowNbr, "OwnedBy" ) );
						}
						else
						{
							ownerRoles.Items.Add( item );
							entity.OwnerRoles = ownerRoles;
						}
					}
					entity.OwnerRoles = ownerRoles;
				}
				else if ( !string.IsNullOrWhiteSpace( entity.OwningOrganizationCtid ) )
				{
					//reset addresses and other related lists, in case we have multiple owning orgs in file
					orgAddresses = new List<Address>();
					orgCostManifests = new List<CostManifest>();
					orgConditionManifests = new List<ConditionManifest>();
					//get org and ensure can view
					defaultOwningOrg = OrganizationManager.GetByCtid( entity.OwningOrganizationCtid );
					if ( defaultOwningOrg == null || defaultOwningOrg.Id == 0 )
					{
						messages.Add( string.Format( "Row: {0}. An organization was not found for the provided CTID: {1}", rowNbr, entity.OwningOrganizationCtid ) );
					}
					else
					{
						//confirm has access
						if ( OrganizationServices.CanUserUpdateOrganization( user, defaultOwningOrg.RowId ) == false )
						{
							messages.Add( string.Format( "Row: {0}. You do not have update rights for the referenced organization (via CTID): {1} ({2}). ", rowNbr, defaultOwningOrg.Name, defaultOwningOrg.Id ) );
						}
						else
						{
							entity.OwningAgentUid = defaultOwningOrg.RowId;
							entity.OrganizationId = defaultOwningOrg.Id;
							entity.OrganizationName = defaultOwningOrg.Name;
							prevOwningAgentUid = entity.OwningAgentUid;
							prevOrgCtid = entity.OwningOrganizationCtid;
							//add owned by role
							ownerRoles = new Enumeration();
							EnumeratedItem item = Entity_AgentRelationshipManager.GetAgentRole( "Owned By" );
							if ( item == null || item.Id == 0 )
							{
								messages.Add( string.Format( "Row: {0} The organization role: {1} is not valid", rowNbr, "OwnedBy" ) );
							}
							else
							{
								ownerRoles.Items.Add( item );
								entity.OwnerRoles = ownerRoles;
							}

							if ( importHelper.CommonCostsHdr > -1 )
							{
								orgCostManifests = CostManifestManager.GetAll( entity.OwningAgentUid, true );
							}

							if ( importHelper.CommonConditionsHdr > -1 )
							{
								orgConditionManifests = ConditionManifestManager.GetAll( entity.OwningAgentUid, true );
							}
						}
					}
				}
			}

			if ( !ServiceHelper.IsValidGuid( entity.OwningAgentUid ) )
			{
				//means one not found, should probably be a previous message
				if ( msgcnt == messages.Count )
				{
					messages.Add( string.Format( "Row: {0}. An owning organization must be entered by referencing an existing organization Id from the publisher, or the CTID of the organization in the publisher, or a unique name of an organization in the publisher", rowNbr ) );
				}
			}
			#endregion



		}//

		protected void SetOwnedByRole( ref List<string> messages )
		{

			EnumeratedItem item = Entity_AgentRelationshipManager.GetAgentRole( "Owned By" );
			//should never happen
			if ( item == null || item.Id == 0 )
			{
				messages.Add( string.Format( "The organization role: {0} is not valid", "OwnedBy" ) );
			}
			else
			{
				ownerRoles.Items.Add( item );
			}
		} //

		public bool IsEmpty( CsvReader csv, int columnsToCheck )
		{
			bool isEmpty = true;
			for ( int i = 0; i < columnsToCheck; i++ )
			{
				if ( !string.IsNullOrWhiteSpace( csv[ i ] ) )
				{
					isEmpty = false;
					break;
				}
			}

			return isEmpty;
		}

		#region Common Assignments
		public string AssignProperty( int rowNbr, CsvReader csv, int indexNbr, string property, ref List<string> messages, string defaultValue = "", bool allowingDeletes = true )
		{
			if ( indexNbr < 0 )
				return defaultValue;
			//handle special cases like ` to "
			string text = csv[ indexNbr ].Trim().Replace( "`", "\"" );
			if ( !allowingDeletes && text == DELETE_ME )
			{
				messages.Add( string.Format( "Row: {0} Error The property: {1} is a required field and the contents cannot be deleted.", rowNbr, property ) );
				return defaultValue;
			}
			if ( text.IndexOf( SAMPLE ) == 0 )
			{
				messages.Add( string.Format( "Row: {1}: column: {0} starts with the word SAMPLE. This is likely invalid data left from the sample data in the template spreadsheet.", property, rowNbr ) );
				return defaultValue;
			}
			return text.Trim();
		}
		public string AssignDate( int rowNbr, CsvReader csv, int indexNbr, string propertyName, ref List<string> messages, string defaultValue = "" )
		{
			if ( indexNbr < 0 )
				return defaultValue;
			DateTime validDate;
			string data = csv[ indexNbr ];
			if ( string.IsNullOrWhiteSpace( data ) )
				return defaultValue;
			if ( data == DELETE_ME )
			{
				return data;
			}
			if ( DateTime.TryParse( csv[ indexNbr ], out validDate ) )
			{
				return validDate.ToString( "yyyy-MM-dd" );
			}
			else
			{
				messages.Add( string.Format( "Row: {0} {1} has invalid date of: {2}", rowNbr, propertyName, data ) );
				return "";
			}

		}

        //common method for list of ctid and url'
        public void AssignCompetencyFramework( int rowNbr, CsvReader csv, int indexNbr, BaseDTO entity, string colName, ref List<string> messages )
        {
            string status = "";
            string input = Assign( rowNbr, csv, indexNbr, "Competency FrameWork", ref messages );
            string[] list = input.Split( '|' );
            if ( list.Count() > 0 )
            {
                int cntr = 0;               
                foreach ( var item in list )
                {
                    cntr++;
                    if ( string.IsNullOrWhiteSpace( item.Trim() ) )
                        continue;
                    //TODO: seems unlikely the same framework would be targeted by the same amsts or lopps in a single import.
                    if ( ServiceHelper.IsValidCtid( item, ref messages, false ) )
                    {
                        //TODO - generalize handling
                        MN.CassInput framework = new CASS_CompetencyFrameworkServices().ImportComptencyFramework( item, ref messages );
                       if(framework == null || string.IsNullOrWhiteSpace(framework.Framework.Name))
                        {
                            messages.Add( string.Format( "Row: {1}: column: {0} Unable to find the framework using Ctid: {2}.", colName, rowNbr, item ) );
                        }
                        else
                        {
                            entity.Frameworks.Add( framework );
                        }
                    }
                    else if ( AssessmentManager.IsUrlValid( item, ref status, doingExistanceCheck = false ) )
                    {
                        //TODO - generalize handling
                        MN.CassInput framework = new CASS_CompetencyFrameworkServices().ImportComptencyFramework( item, ref messages );
                        if ( framework == null || string.IsNullOrWhiteSpace( framework.Framework.Name ) )
                        {
                            messages.Add( string.Format( "Row: {1}: column: {0} Unable to find the framework using Url: {2}.", colName, rowNbr, item ) );
                        }
                        else
                        {
                            entity.Frameworks.Add( framework );
                        }
                    }    
                    else
                    {
                        messages.Add( string.Format( "Row: {1}: column: {0} Error invalid must be a valid Ctid or CaSS Url: {2}.", colName, rowNbr, item ) );
                    }

                }
            }
        }

        public string AssignImageUrl( int rowNbr, CsvReader csv, int indexNbr, string colName, ref List<string> messages,
				string defaultValue = "", bool isRequired = false )
		{
			if ( indexNbr < 0 )
			{
				if ( isRequired )
					if ( !IsPartialUpdate )
						messages.Add( string.Format( "Row: {1}: column: {0} is a required field (or must be provided with another field)", colName, rowNbr ) );
				return defaultValue;
			}
			if ( isRequired && string.IsNullOrWhiteSpace( csv[ indexNbr ] ) )
				if ( !IsPartialUpdate )
					messages.Add( string.Format( "Row: {1}: column: {0} is a required field", colName, rowNbr ) );

			string text = csv[ indexNbr ].Trim();
			if ( isRequired && text == DELETE_ME )
			{
				messages.Add( string.Format( "Row: {1}: column: {0} is a required field and the contents cannot be deleted.", colName, rowNbr ) );
				return defaultValue;
			}
			if ( text == DELETE_ME || text == "" )
			{
				return text;
			}
			//do url check?
			string status = "";
			if ( !OrganizationManager.IsImageUrlValid( text, ref status, doingExistanceCheck ) )
			{
				messages.Add( string.Format( "Row: {1}: column: {0} Error on Url check of image: {2} - {3}.", colName, rowNbr, text, status ) );
				return defaultValue;
			}

			return text;
		}
		public string AssignUrl( int rowNbr, CsvReader csv, int indexNbr, string colName, ref List<string> messages,
				string defaultValue = "", bool isRequired = false )
		{
			if ( indexNbr < 0 )
			{
				if ( isRequired )
					if ( !IsPartialUpdate )
						messages.Add( string.Format( "Row: {1}: column: {0} is a required field (or must be provided with another field)", colName, rowNbr ) );
				return defaultValue;
			}
			if ( isRequired && string.IsNullOrWhiteSpace( csv[ indexNbr ] ) )
				if ( !IsPartialUpdate )
					messages.Add( string.Format( "Row: {1}: column: {0} is a required field", colName, rowNbr ) );

			string text = csv[ indexNbr ].Trim();
			if ( isRequired && text == DELETE_ME )
			{
				messages.Add( string.Format( "Row: {1}: column: {0} is a required field and the contents cannot be deleted.", colName, rowNbr ) );
				return defaultValue;
			}
			if ( text == DELETE_ME || text == "" )
			{
				return text;
			}
			//do url check
			//first check if in list of urls already checked
			int index = previousUrls.FindIndex( a => a == text.ToLower() );
			if ( index > -1 )
				return text;

			string status = "";
			if ( !OrganizationManager.IsUrlValid( text, ref status, doingExistanceCheck ) )
			{
				messages.Add( string.Format( "Row: {1}: column: {0} Error on Url check of: {2} - {3}.", colName, rowNbr, text, status ) );
			}
			previousUrls.Add( text.ToLower() );
			return text;
		}


		public DurationProfile AssignDuration( int rowNbr, CsvReader csv, int indexNbr, ref List<string> messages, string defaultValue = "", bool isRequired = false, bool exactDurationOnly = false )
		{
			DurationProfile profile = new DurationProfile();
			if ( indexNbr < 0 )
				return profile;
			string input = csv[ indexNbr ];
			if ( string.IsNullOrWhiteSpace( input ) )
				return profile;
			if ( input.IndexOf( SAMPLE ) == 0 )
			{
				messages.Add( string.Format( "Row: {1}, column: {0} starts with the word SAMPLE. This is likely invalid data left from the sample data in the template spreadsheet.", "Duration", rowNbr ) );
				return profile;
			}

			int amt = 0;
			string unit = "";
			//should only be two, but could allow a range:
			//99 xxxxx~99 yyyyy
			if ( input.IndexOf( "~" ) == -1 )
			{
				string[] parts = input.Split( ' ' );
				//expecting amt first
				if ( parts.Count() > 0 )
				{
					if ( !Int32.TryParse( parts[ 0 ], out amt ) )
					{
						messages.Add( string.Format( "Row: {2} Invalid amount (integer) value of {0} for column: {1}. Must be a format like 12 hours, or 2 years", parts[ 0 ], "Duration", rowNbr ) );
						return profile;
					}
					if ( parts.Count() > 1 )
					{
						unit = parts[ 1 ];
						if ( string.IsNullOrWhiteSpace( unit ) )
						{
							messages.Add( string.Format( "Row: {2} Invalid duration unit of {0} for column: {1}. Must be a format years, months, weeks, or hours.", parts[ 0 ], "Duration", rowNbr ) );
							return profile;
						}
					}
					profile.ExactDuration = PopulateDuration( rowNbr, unit, amt, ref messages );
				}
			}
			else
			{
				string[] ranges = input.Split( '~' );
				if ( ranges.Count() > 0 )
				{
					string[] parts = ranges[ 0 ].Split( ' ' );
					//expecting amt first
					if ( parts.Count() > 0 )
					{
						if ( !Int32.TryParse( parts[ 0 ], out amt ) )
						{
							messages.Add( string.Format( "Row: {2} Invalid amount (integer) value of {0} for column: {1}. Must be a format like 12 hours, or 2 years", parts[ 0 ], "Duration", rowNbr ) );
							return profile;
						}
						if ( parts.Count() > 1 )
						{
							unit = parts[ 1 ];
							if ( string.IsNullOrWhiteSpace( unit ) )
							{
								messages.Add( string.Format( "Row: {2} Invalid duration unit of {0} for column: {1}. Must be a format years, months, weeks, or hours.", parts[ 0 ], "Duration", rowNbr ) );
								return profile;
							}
						}
						profile.MinimumDuration = PopulateDuration( rowNbr, unit, amt, ref messages );
					}
				}
				if ( ranges.Count() > 1 )
				{
					string[] parts = ranges[ 1 ].Split( ' ' );
					//expecting amt first
					if ( parts.Count() > 0 )
					{
						if ( !Int32.TryParse( parts[ 0 ], out amt ) )
						{
							messages.Add( string.Format( "Row: {2} Invalid amount (integer) value of {0} for column: {1}. Must be a format like 12 hours, or 2 years", parts[ 0 ], "Duration", rowNbr ) );
							return profile;
						}
						if ( parts.Count() > 1 )
						{
							unit = parts[ 1 ];
							if ( string.IsNullOrWhiteSpace( unit ) )
							{
								messages.Add( string.Format( "Row: {2} Invalid duration unit of {0} for column: {1}. Must be a format years, months, weeks, or hours.", parts[ 0 ], "Duration", rowNbr ) );
								return profile;
							}
						}
						profile.MaximumDuration = PopulateDuration( rowNbr, unit, amt, ref messages );
					}
				}
			}

			return profile;
		}
		public DurationItem PopulateDuration( int rowNbr, string input, int amt, ref List<string> messages )
		{
			DurationItem profile = new DurationItem();
			string durationUnit = input.ToLower();
			switch ( durationUnit )
			{
				case "years":
				case "year":
					profile.Years = amt;
					break;
				case "months":
				case "month":
					profile.Months = amt;
					break;
				case "weeks":
				case "week":
					profile.Weeks = amt;
					break;
				case "days":
				case "day":
					profile.Days = amt;
					break;
				case "hours":
				case "hour":
					profile.Hours = amt;
					break;
				case "minutes":
				case "minute":
					profile.Minutes = amt;
					break;
				default:

					messages.Add( string.Format( "Error - Invalid unit was entered for duration. Row: {0}, Unit: {1}", rowNbr, input ) );
					break;
			}

            return profile;
        }
        public int AssignInteger(int rowNbr, CsvReader csv, int indexNbr, string colName, ref List<string> messages,
                bool isRequired = false, int defaultValue = 0)
        {
            if ( indexNbr < 0 )
                return defaultValue;
            //need to handle delete if used!
            int value = 0;
            string text = csv[ indexNbr ].Trim();
            if ( string.IsNullOrWhiteSpace( text ) )
                return 0;
            if ( !Int32.TryParse(csv[ indexNbr ], out value) )
            {
                messages.Add(string.Format( "Row: {2} Invalid integer value of {0} for column: {1}", csv[ indexNbr ], colName, rowNbr));
                value = defaultValue;
            }

            return value;
        }
        public decimal AssignDecimal(int rowNbr, CsvReader csv, int indexNbr, string colName, ref List<string> messages, bool isRequired = false, decimal defaultValue = -100)
        {
            if ( indexNbr < 0 )
                return defaultValue; //may want something different to designate ignore!
            string text = csv[ indexNbr ].Trim();
            if ( string.IsNullOrWhiteSpace( text ) )
                return defaultValue;
            if ( text == DELETE_ME )
                return -99;

            decimal value = 0;
            int integerValue = 0;
            if(int.TryParse(csv[ indexNbr ], out integerValue ) )
            {
                value = integerValue;
            }
          else if ( !decimal.TryParse(csv[ indexNbr ], out value) )
            {
                messages.Add(string.Format( "Row: {2} Invalid decimal value of {0} for column: {1}, value: {3}", csv[ indexNbr ], colName, rowNbr, text ) );
                value = defaultValue;
            }

			return value;
		}
		/// <summary>
		/// Handle bool with integer values. Return
		/// 0 - if entered false
		/// 1 - if entered true
		/// 2 - if entered #delete
		/// 3 - no entry
		/// </summary>
		/// <param name="rowNbr"></param>
		/// <param name="csv"></param>
		/// <param name="indexNbr"></param>
		/// <param name="colName"></param>
		/// <param name="messages"></param>
		/// <param name="isRequired"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		public int AssignBool( int rowNbr, CsvReader csv, int indexNbr, string colName, ref List<string> messages )
		{
			if ( indexNbr < 0 )
				return 3;
			string text = csv[ indexNbr ].Trim();
			if ( text == DELETE_ME )
				return 2;
			else if ( text == "" )
				return 3;
			bool value = false;
			if ( !bool.TryParse( csv[ indexNbr ], out value ) )
			{
				messages.Add( string.Format( "Row: {2} Invalid value of {0} for column: {1}", csv[ indexNbr ], colName, rowNbr ) );
				return 3;
			}
			if ( value )
				return 1;
			else
				return 0;
		}
		public List<int> AssignLanguages( int rowNbr, CsvReader csv, int indexNbr, string colName, ref List<string> messages, bool isRequired = false )
		{
			List<int> list = new List<int>();
			if ( indexNbr < 0 )
			{
				if ( isRequired )
				{
					if ( !IsPartialUpdate )
						messages.Add( string.Format( "Row: {0} Required value missing for column name: {1}", rowNbr, colName ) );
				}

				return list;
			}

			string input = csv[ indexNbr ].Trim();
			if ( string.IsNullOrWhiteSpace( input.Trim() ) )
				return list;
			if ( input == DELETE_ME )
			{
				messages.Add( string.Format( "Row: {0}, column: {1} Language is a required property, and cannot be deleted.", rowNbr, colName ) );
				return list;
			}
			else if ( input.IndexOf( SAMPLE ) == 0 )
			{
				messages.Add( string.Format( "Row: {0}, column: {1} starts with the word SAMPLE. This is likely invalid data left from the sample data in the template spreadsheet.", rowNbr, colName ) );
				return list;
			}

			string[] parts = input.Split( '|' );
			foreach ( var item in parts )
			{
				if ( !string.IsNullOrWhiteSpace( item.Trim() ) )
				{
					int languageId = CodesManager.GetLanguageId( item );
					if ( languageId > 0 )
						list.Add( languageId );
					else
					{
						messages.Add( string.Format( "Row: {0}, column: {1} The entered Language: '{2}' is not recognized as a valid value.", rowNbr, colName, item ) );
					}
				}
			}
			return list;
		}
		public List<CodeItem> AssignNAICSList( int rowNbr, CsvReader csv, int indexNbr, string colName, BaseDTO entity, ref List<string> messages )
		{
			List<CodeItem> output = new List<CodeItem>();
			List<string> list = AssignList( rowNbr, csv, indexNbr, colName, ref messages );
			foreach ( var code in list )
			{
				if ( code == DELETE_ME )
				{
					entity.DeletingNaicsCodes = true;
					break;
				}
				List<CodeItem> codes = CodesManager.NAICS_Search( code );
				if ( codes == null || codes.Count == 0 )
				{
					messages.Add( string.Format( "Row: {0}. The entered NAICS code ({1}) is not valid. ", rowNbr, code ) );
				}
				else
					output.AddRange( codes );
			}


			return output;
		}

		public List<CodeItem> AssignSOCList( int rowNbr, CsvReader csv, int indexNbr, string colName, BaseDTO entity, ref List<string> messages )
		{
			List<CodeItem> output = new List<CodeItem>();
			List<string> list = AssignList( rowNbr, csv, indexNbr, colName, ref messages );
			foreach ( var code in list )
			{
				if ( code == DELETE_ME )
				{
					entity.DeletingSOCCodes = true;
					break;
				}
				List<CodeItem> codes = CodesManager.SOC_Search( code );
				if ( codes == null || codes.Count == 0 )
				{
					messages.Add( string.Format( "Row: {0}. The entered SOC code ({1}) is not valid. ", rowNbr, code ) );
				}
				else
					output.AddRange( codes );
			}


			return output;
		}
		public List<CodeItem> AssignCIPList( int rowNbr, CsvReader csv, int indexNbr, string colName, BaseDTO entity, ref List<string> messages )
		{
			List<CodeItem> output = new List<CodeItem>();
			List<string> searchCodes = new List<string>();
			List<CodeItem> codes = new List<CodeItem>();
			List<string> list = AssignList( rowNbr, csv, indexNbr, colName, ref messages );
			foreach ( var code in list )
			{
				if ( code == DELETE_ME )
				{
					entity.DeletingCIPCodes = true;
					break;
				}
				codes = new List<CodeItem>();
				//the leading zero for a CIP code will be truncated by Excel, do a check for only one digit before the period
				if ( code.IndexOf( "." ) == 1 || code.Length == 1 )
				{
					searchCodes = new List<string>
					{
						code,
						"0" + code,
						"0" + code + "0" //just in case
					};
					codes = CodesManager.CIP_Search( searchCodes );
				}
				else if ( code.Length - code.IndexOf( "." ) == 4 )
				{
					//handling missing trailinng zero
					searchCodes = new List<string>
					{
						code,
						code + "0"
					};
					codes = CodesManager.CIP_Search( searchCodes );
				}
				else
				{
					codes = CodesManager.CIP_Search( code );
				}
				if ( codes == null || codes.Count == 0 )
				{
					messages.Add( string.Format( "Row: {0}. The entered CIP code ({1}) is not valid. ", rowNbr, code ) );
				}
				else
					output.AddRange( codes );
			}


			return output;
		}
		public List<string> AssignList( int rowNbr, CsvReader csv, int indexNbr, string colName, ref List<string> messages, bool isRequired = false )
		{
			List<string> list = new List<string>();
			if ( indexNbr < 0 )
			{
				if ( isRequired )
				{
					if ( !IsPartialUpdate )
						messages.Add( string.Format( "Row: {0} Error: required value missing for column name: {1}", rowNbr, colName ) );
				}

				return new List<string>();
			}
			string input = csv[ indexNbr ].Trim();
			if ( string.IsNullOrWhiteSpace( input.Trim() ) )
				return list;
			if ( input == DELETE_ME )
			{
				if ( isRequired )
				{
					messages.Add( string.Format( "Row: {0}, column: {1} is a required property, it cannot be deleted.", rowNbr, colName ) );
					return list;
				}
				else
				{
					list.Add( DELETE_ME );
					return list;
				}
			}
			else if ( input.IndexOf( SAMPLE ) == 0 )
			{
				messages.Add( string.Format( "Row: {0}, column: {1} starts with the word SAMPLE. This is likely invalid data left from the sample data in the template spreadsheet.", rowNbr, colName ) );
				return list;
			}

			string[] parts = input.Split( '|' );

			foreach ( var item in parts )
			{
				if ( !string.IsNullOrWhiteSpace( item.Trim() ) )
					list.Add( item.Trim() );
			}
			return list;
		}
		public string Assign( int rowNbr, CsvReader csv, int indexNbr, string colName, ref List<string> messages, string defaultValue = "", bool isRequired = false, int minimumLength = 0 )
		{
			if ( indexNbr < 0 )
			{
				if ( isRequired )
					if ( !IsPartialUpdate )
						messages.Add( string.Format( "Row: {1}: column: {0} is a required field (or must be provided with another field)", colName, rowNbr ) );
				return defaultValue;
			}
			if ( isRequired && string.IsNullOrWhiteSpace( csv[ indexNbr ] ) )
			{
				if ( !IsPartialUpdate )
					messages.Add( string.Format( "Row: {1}: column: {0} is a required field", colName, rowNbr ) );
				return defaultValue;
			}
			string text = csv[ indexNbr ].Trim().Replace( "`", "\"" ).Replace( "NULL", "" );
			if ( isRequired && text == DELETE_ME )
			{
				messages.Add( string.Format( "Row: {1}: column: {0} is a required field and the contents cannot be deleted.", colName, rowNbr ) );
				return defaultValue;
			}
			else if ( text.IndexOf( SAMPLE ) == 0 )
			{
				messages.Add( string.Format( "Row: {1}: column: {0} starts with the word SAMPLE. This is likely invalid data left from the sample data in the template spreadsheet.", colName, rowNbr ) );
				return defaultValue;
			}
			if ( minimumLength > 0 && text.Length > 0 && text.Length < minimumLength)
			{
				messages.Add( string.Format( "Row: {1}: column: {0} must be a minimum length of {2} characters.", colName, rowNbr, minimumLength ) );
				return defaultValue;
			}
			return text;
		}

		public void AssignAudienceTypes( int rowNbr, CsvReader csv, BaseDTO entity, AppUser user, int audienceTypeHdr, ref List<string> messages )
		{
			entity.AudienceTypesList = AssignProperty( rowNbr, csv, audienceTypeHdr, "Audience Types", ref messages );
			if ( string.IsNullOrWhiteSpace( entity.AudienceTypesList ) || entity.AudienceTypesList == DELETE_ME )
				return;

			if ( prevAudienceTypeList == entity.AudienceTypesList )
			{
				entity.AudienceType = prevAudienceType;
			}
			else
			{
				prevAudienceType = new Enumeration() { Id = CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE };
				var itemList = entity.AudienceTypesList.Split( '|' );
				foreach ( var item in itemList )
				{
					EnumeratedItem ei = CodesManager.GetCodeAsEnumerationItem( CodesManager.PROPERTY_CATEGORY_AUDIENCE_TYPE, item );
					if ( ei != null && ei.Id > 0 )
						prevAudienceType.Items.Add( ei );
					else
						messages.Add( string.Format( "Row: {0} Invalid audience type of {1}", rowNbr, item ) );
				}

				if ( prevAudienceType != null && prevAudienceType.Items.Count > 0 )
				{
					entity.AudienceType = prevAudienceType;
					prevAudienceTypeList = entity.AudienceTypesList;
				}

			}
		}


		public string AssignEnumeration( int rowNbr, CsvReader csv, AppUser user, int enumerationTypeHdr, string label, int categoryId, ref Enumeration lastProperty, ref Enumeration thisProperty, ref string previousList, ref List<string> messages )
		{
			string propertyList = AssignProperty( rowNbr, csv, enumerationTypeHdr, label, ref messages );
			if ( string.IsNullOrWhiteSpace( propertyList ) || propertyList == DELETE_ME )
				return propertyList;

			if ( previousList == propertyList )
			{
				thisProperty = lastProperty;
			}
			else
			{
				lastProperty = new Enumeration() { Id = categoryId };
				var itemList = propertyList.Split( '|' );
				foreach ( var item in itemList )
				{
					EnumeratedItem ei = CodesManager.GetCodeAsEnumerationItem( categoryId, item );
					if ( ei != null && ei.Id > 0 )
						lastProperty.Items.Add( ei );
					else
						messages.Add( string.Format( "Row: {0} Invalid {1} of {2}", rowNbr, label, item ) );
				}

				if ( lastProperty != null && lastProperty.Items.Count > 0 )
				{
					thisProperty = lastProperty;
					previousList = propertyList;
				}

			}
			return propertyList;
		}
		#endregion


		#region less common assignments
		public bool HandleOrganizationReference( int rowNbr, int cntr, string propertyType, string orgReference, BaseDTO entity, AppUser user, ref List<string> messages, ref Organization refOrg )
		{
			bool isValid = true;
			//check against existing pairs
			//......
			int index = prevRefOrgPairs.FindIndex( s => s == orgReference.ToLower() );
			if ( index > -1 )
			{
				refOrg = prevRefOrgs[ index ];
				return true;
			}
			string[] parts = orgReference.Split( '~' );
			//for now expecting just name and swp
			if ( parts.Count() != 2 )
			{
				messages.Add( string.Format( "Row: {0} {1} Entry Number: {2} List must contain an organization name and organization webpage. Entry: {3}", rowNbr, propertyType, cntr, orgReference ) );
				return false;
			}
			string orgname = parts[ 0 ].Trim();
			string swp = parts[ 1 ].Trim();
			if ( orgname == "" && swp == "" )
				return false;
			//if one of these are blank, then error
			if ( orgname == "" || swp == "" )
			{
				messages.Add( string.Format( "Row: {0}. {1} Entry Number: {2}. Both name of the referenced organization and the subject webpage must be provided: {3}", rowNbr, propertyType, cntr, orgReference ) );
				return false;
			}
			string status = "";


			//get org by name and swp, or check latter on return
			refOrg = OrganizationManager.GetByNameAndUrl( orgname, swp, ref status );
			if ( refOrg == null || refOrg.Id == 0 )
			{
				if ( !string.IsNullOrWhiteSpace( status ) ) //error likely duplicates 
				{
					messages.Add( string.Format( "Row: {0}. {1} Error on look up for {2}: {3}", rowNbr, propertyType, orgname, status ) );
					return false;
				}
				else
				{
					//need to create a reference org
					refOrg.Name = orgname;
					refOrg.SubjectWebpage = swp;
					refOrg.IsAQAOrg = true;
					refOrg.IsReferenceVersion = true;
					refOrg.CreatedById = user.Id;
					int newID = orgMgr.Add( refOrg, false, ref status );
					if ( newID > 0 )
					{
						prevRefOrgs.Add( refOrg );
						prevRefOrgPairs.Add( orgReference.ToLower() );
					}
					else
					{
						messages.Add( status );
						return false;
					}
				}
			}
			else
			{
				//perhaps ane extra check, to prevent dups 
				//int index = prevRefOrgPairs.FindIndex( s => s == orgReference.ToLower() );
				prevRefOrgs.Add( refOrg );
				prevRefOrgPairs.Add( orgReference.ToLower() );
				return true;
			}

			return isValid;
		}//
        public bool HandleCredentialReference( int rowNbr, int cntr, string propertyType, string credReference, AppUser user, ref List<string> messages, ref Credential refEntity )
        {
            bool isValid = true;
            //check against existing pairs
            //......
            int index = prevRefCredPairs.FindIndex( s => s == credReference.ToLower() );
            if ( index > -1 )
            {
                refEntity = prevRefCreds[ index ];
                return true;
            }
            string[] parts = credReference.Split( '~' );
            //for now expecting just name and swp
            if ( parts.Count() != 3 )
            {
                messages.Add( string.Format( "Row: {0} {1} Entry Number: {2} List must contain an credential name, credential type  and credential webpage. Entry: {3}", rowNbr, propertyType, cntr, credReference ) );
                return false;
            }
            string credname = parts[ 0 ].Trim();
            string credtype = parts[ 1 ].Trim();
            string swp = parts[ 2 ].Trim();
            if ( credname == ""&& credtype == "" && swp == "" )
                return false;
            //if one of these are blank, then error
            if ( credname == "" || credtype == "" || swp == "" )
            {
                messages.Add( string.Format( "Row: {0}. {1} Entry Number: {2}. credential name,type and the subject webpage must be provided: {3}", rowNbr, propertyType, cntr, credReference ) );
                return false;
            }
            string status = "";
           var credentialType = CodesManager.GetCodeAsEnumeration( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_TYPE, credtype );
            if ( credentialType == null || credentialType.Items == null || credentialType.Items.Count == 0 )
            {
                messages.Add( string.Format( "Row: {0} Invalid credential type of {1}", rowNbr, credtype ) );
            }
           

            //get cred by name and swp, or check latter on return
            refEntity =CredentialManager.GetByNameAndUrl( credname, swp, ref status );
            if ( refEntity == null || refEntity.Id == 0 )
            {
                if ( !string.IsNullOrWhiteSpace( status ) ) //error likely duplicates 
                {
                    messages.Add( string.Format( "Row: {0}. {1} Error on look up for {2}: {3}", rowNbr, propertyType, credname, status ) );
                    return false;
                }
                else
                {
                    //need to create a reference org
                    refEntity.Name = credname;
                    refEntity.CredentialType = credentialType;
                    refEntity.SubjectWebpage = swp;
                  
                    refEntity.IsReferenceVersion = true;
                    refEntity.CreatedById = user.Id;
                    int newID = credMgr.Add( refEntity, ref status, false );
                    if ( newID > 0 )
                    {
                        prevRefCreds.Add( refEntity );
                        prevRefCredPairs.Add( credReference.ToLower() );
                    }
                    else
                    {
                        messages.Add( status );
                        return false;
                    }
                }
            }
            else
            {
                //perhaps ane extra check, to prevent dups 
                //int index = prevRefCredPairs.FindIndex( s => s == credReference.ToLower() );
                prevRefCreds.Add( refEntity );
                prevRefCredPairs.Add( credReference.ToLower() );
                return true;
            }

            return isValid;
        }
        public bool HandleAssessmentReference( int rowNbr, int cntr, string propertyType, string assmtReference, AppUser user, ref List<string> messages, ref AssessmentProfile refEntity )
        {
            bool isValid = true;
            //check against existing pairs
            //......
            int index = prevRefAssmtPairs.FindIndex( s => s == assmtReference.ToLower() );
            if ( index > -1 )
            {
                refEntity = prevRefAssmts[ index ];
                return true;
            }
            string[] parts = assmtReference.Split( '~' );
            //for now expecting just name and swp
            if ( parts.Count() != 2)
            {
                messages.Add( string.Format( "Row: {0} {1} Entry Number: {2} List must contain an assessment name and assessment webpage. Entry: {3}", rowNbr, propertyType, cntr, assmtReference ) );
                return false;
            }
            string assmtname = parts[ 0 ].Trim();
            //string assmttype = parts[ 1 ].Trim();
            string swp = parts[ 1 ].Trim();
            if ( assmtname == "" && swp == "" )
                return false;
            //if one of these are blank, then error
            if ( assmtname == "" || swp == "" )
            {
                messages.Add( string.Format( "Row: {0}. {1} Entry Number: {2}. assessment nameand the subject webpage must be provided: {3}", rowNbr, propertyType, cntr, assmtReference ) );
                return false;
            }
            string status = "";
            //var assessmentType = CodesManager.GetCodeAsEnumeration( CodesManager.PROPERTY_CATEGORY_ASSESSMENT_TYPE /*assmttype */);
            //if ( assessmentType == null || assessmentType.Items == null || assessmentType.Items.Count == 0 )
            //{
            //    messages.Add( string.Format( "Row: {0} Invalid assessment type of {1}", rowNbr/*, assmttype */) );
            //}


            //get cred by name and swp, or check latter on return
            refEntity = AssessmentManager.GetByNameAndUrl( assmtname, swp, ref status );
            if ( refEntity == null || refEntity.Id == 0 )
            {
                if ( !string.IsNullOrWhiteSpace( status ) ) //error likely duplicates 
                {
                    messages.Add( string.Format( "Row: {0}. {1} Error on look up for {2}: {3}", rowNbr, propertyType, assmtname, status ) );
                    return false;
                }
                else
                {
                    //need to create a reference org
                    refEntity.Name = assmtname;
                    refEntity.SubjectWebpage = swp;

                    refEntity.IsReferenceVersion = true;
                    refEntity.CreatedById = user.Id;
                    int newID = assmMgr.Add( refEntity, ref status );
                    if ( newID > 0 )
                    {
                        prevRefAssmts.Add( refEntity );
                        prevRefAssmtPairs.Add( assmtReference.ToLower() );
                    }
                    else
                    {
                        messages.Add( status );
                        return false;
                    }
                }
            }
            else
            {
                //perhaps ane extra check, to prevent dups 
                //int index = prevRefCredPairs.FindIndex( s => s == credReference.ToLower() );
                prevRefAssmts.Add( refEntity );
                prevRefAssmtPairs.Add( assmtReference.ToLower() );
                return true;
            }

            return isValid;
        }
        public bool HandleLearningOppReference( int rowNbr, int cntr, string propertyType, string loppReference, AppUser user, ref List<string> messages, ref LearningOpportunityProfile refEntity )
        {
            bool isValid = true;
            //check against existing pairs
            //......
            int index = prevRefLoppPairs.FindIndex( s => s == loppReference.ToLower() );
            if ( index > -1 )
            {
                refEntity = prevRefLopps[ index ];
                return true;
            }
            string[] parts = loppReference.Split( '~' );
            //for now expecting just name and swp
            if ( parts.Count() != 2 )
            {
                messages.Add( string.Format( "Row: {0} {1} Entry Number: {2} List must contain an learningOpp name and learningOpp webpage. Entry: {3}", rowNbr, propertyType, cntr, loppReference ) );
                return false;
            }
            string loppname = parts[ 0 ].Trim();
            //string assmttype = parts[ 1 ].Trim();
            string swp = parts[ 1 ].Trim();
            if ( loppname == "" && swp == "" )
                return false;
            //if one of these are blank, then error
            if ( loppname == "" || swp == "" )
            {
                messages.Add( string.Format( "Row: {0}. {1} Entry Number: {2}. learningOpp name and the subject webpage must be provided: {3}", rowNbr, propertyType, cntr, loppReference ) );
                return false;
            }
            string status = "";
            //var assessmentType = CodesManager.GetCodeAsEnumeration( CodesManager.PROPERTY_CATEGORY_ASSESSMENT_TYPE /*assmttype */);
            //if ( assessmentType == null || assessmentType.Items == null || assessmentType.Items.Count == 0 )
            //{
            //    messages.Add( string.Format( "Row: {0} Invalid assessment type of {1}", rowNbr/*, assmttype */) );
            //}


            //get cred by name and swp, or check latter on return
            refEntity = LearningOpportunityManager.GetByNameAndUrl( loppname, swp, ref status );
            if ( refEntity == null || refEntity.Id == 0 )
            {
                if ( !string.IsNullOrWhiteSpace( status ) ) //error likely duplicates 
                {
                    messages.Add( string.Format( "Row: {0}. {1} Error on look up for {2}: {3}", rowNbr, propertyType, loppname, status ) );
                    return false;
                }
                else
                {
                    //need to create a reference org
                    refEntity.Name = loppname;
                    refEntity.SubjectWebpage = swp;

                    refEntity.IsReferenceVersion = true;
                    refEntity.CreatedById = user.Id;
                    int newID = loppMgr.Add( refEntity, ref status );
                    if ( newID > 0 )
                    {
                        prevRefLopps.Add( refEntity );
                        prevRefLoppPairs.Add( loppReference.ToLower() );
                    }
                    else
                    {
                        messages.Add( status );
                        return false;
                    }
                }
            }
            else
            {
                //perhaps ane extra check, to prevent dups 
                //int index = prevRefCredPairs.FindIndex( s => s == credReference.ToLower() );
                prevRefLopps.Add( refEntity );
                prevRefLoppPairs.Add( loppReference.ToLower() );
                return true;
            }

            return isValid;
        }
        public void AssignAddressesViaCodes( int rowNbr, CsvReader csv, BaseDTO entity, AppUser user, CommonImportRequest importHelper, ref List<string> messages )
		{
			//need to handle via ids or via external codes
			int msgcnt = messages.Count;

			//we will either have a code to indicate use all of the org addresses, or a delimited list
			//do we check for valid ids now or not until ready to save?
			//perhaps for the first time, get all addresses for the org - should only have one org
			if ( orgAddresses.Count == 0 )
			{
				orgAddresses = AddressProfileManager.GetAll( prevOwningAgentUid );
			}

			string addressIdentifiers = Assign( rowNbr, csv, importHelper.AvailableAtCodesHdr, "Available At Codes", ref messages, "", false );
			if ( string.IsNullOrWhiteSpace( addressIdentifiers ) )
				return;

			//could have some variance with each cred, 
			if ( addressIdentifiers.ToLower() == "all" )
			{
				entity.AvailableAt = orgAddresses;

			}
			else if ( prevAddressIdentifiers == addressIdentifiers )
			{
				entity.AvailableAt = prevOrgAddresses;

			}
			else
			{
				prevOrgAddresses = new List<Address>();
				prevAddressIdentifiers = addressIdentifiers;
				//loop thru address ids and assign
				string[] parts = addressIdentifiers.Split( '|' );
				int addresssId = 0;
				foreach ( var item in parts )
				{
					//get address from org addresses.
					Address address = orgAddresses.FirstOrDefault( s => s.ParentRowId == prevOwningAgentUid && s.ExternalIdentifier == item );
					if ( address == null || address.Id == 0 )
					{
						messages.Add( string.Format( "Row: {0}. The External Address identifier is not valid for the owning organization: {1}", rowNbr, defaultOwningOrg.Name ) );
						continue;
					}
					prevOrgAddresses.Add( address );
				}
				entity.AvailableAt = prevOrgAddresses;
			}
		}//
		public void AssignAddresses( int rowNbr, CsvReader csv, BaseDTO entity, AppUser user, CommonImportRequest importHelper, ref List<string> messages )
		{
			//need to handle via ids or via external codes
			int msgcnt = messages.Count;
			List<Address> entityAddresses = new List<Address>();
			//we will either have a code to indicate use all of the org addresses, or a delimited list
			//do we check for valid ids now or not until ready to save?
			//perhaps for the first time, get all addresses for the org - should only have one org
			if ( orgAddresses.Count == 0 )
			{
				orgAddresses = AddressProfileManager.GetAll( prevOwningAgentUid );
			}
			if ( entity.IsExistingEntity )
			{
				entityAddresses = AddressProfileManager.GetAll( entity.ExistingParentRowId );
			}
			string addressIdentifiers = Assign( rowNbr, csv, importHelper.AvailableAtHdr, "Available At", ref messages, "", false );
			string addressExternalCodes = Assign( rowNbr, csv, importHelper.AvailableAtCodesHdr, "Available At Codes", ref messages, "", false );
			if ( string.IsNullOrWhiteSpace( addressIdentifiers ) && string.IsNullOrWhiteSpace( addressExternalCodes ) )
				return;

			//could have some variance with each cred, 
			if ( addressIdentifiers.ToLower() == "all" )
			{
				entity.AvailableAt = orgAddresses;

			}
			else if ( prevAddressIdentifiers == addressIdentifiers )
			{
				entity.AvailableAt = prevOrgAddresses;

			}
			else
			{
				prevOrgAddresses = new List<Address>();
				prevAddressIdentifiers = addressIdentifiers;
				//loop thru address ids and assign
				string[] parts = addressIdentifiers.Split( '|' );
				//18-04-05 - initially will expect integers representing Id for entity.address
				int addresssId = 0;
				foreach ( var item in parts )
				{
					if ( !Int32.TryParse( item.Trim(), out addresssId ) )
					{
						messages.Add( string.Format( "Row: {0}. Address identifier must be an integer that relates to an address stored for the organization: {1}", rowNbr, item.Trim() ) );
						continue;
					}
					//get address from org addresses.
					Address address = orgAddresses.FirstOrDefault( s => s.Id == addresssId );
					if ( address == null || address.Id == 0 )
					{
						//check addresses for target
						address = entityAddresses.FirstOrDefault( s => s.Id == addresssId );
						if ( address == null || address.Id == 0 )
						{
							messages.Add( string.Format( "Row: {0}. The Address identifier is not valid for the owning organization: {1}", rowNbr, addresssId ) );
							continue;
						}
						else
						{
							//probably want to ignore
						}
					}
					else
					{
						//need to confirm the address is from the selected org
						prevOrgAddresses.Add( address );
					}
				}
				entity.AvailableAt = prevOrgAddresses;
			}
		}//

		public void AssignConditionProfile( int rowNbr, CsvReader csv, BaseDTO entity, int parentEntityTypeId, AppUser user, CommonImportRequest importHelper, ref List<string> messages )
		{
			bool hasConditionProperties = false;
			string profileType = "Condition";
			string identifier = "";
			string status = "";
			string externalIdentifier = "";
			entity.ConditionProfile = new ConditionProfileDTO();
			int existingCount = 0;
			Guid existingProfileUid = new Guid();

			ConditionProfile existingProfile = new ConditionProfile();
			int msgcnt = messages.Count;

			if ( entity.IsExistingEntity )
			{
				//check if has conditions - not Conditions
				List<ConditionProfile> conditions = Entity_ConditionProfileManager.GetAll( entity.ExistingParentRowId , 1);
				if ( conditions != null && conditions.Count > 0 )
				{
					existingCount = conditions.Count;
					//existingProfileUid = conditions[ 0 ].RowId;
					if ( conditions.Count == 1 )
					{
						//these can be overridden if provided in upload
						existingProfileUid = conditions[ 0 ].RowId;
						identifier = existingProfileUid.ToString();
						existingProfile = conditions[ 0 ];
						entity.ConnectionProfile.RowId = existingProfileUid;
					}
				}
			}

			//internaldentifier =====================================================
			//note this is auto added to an export - which may be confusing. 
			//	- not all users will do an export before updates.
			//	- valuable where other conditions have been added manually.
			//if there is no other data, this should be ignored
			//tbd
			if ( importHelper.ConditionIdentifierHdr > -1 )
			{
				identifier = Assign( rowNbr, csv, importHelper.ConditionIdentifierHdr, "ConditionIdentifier", ref messages, "", false );
				if ( !string.IsNullOrWhiteSpace( identifier ) )
				{
					if ( identifier == DELETE_ME )
					{
						//hmmm this may not work in this field, will need the identifier to delete - in case of multiple
						//or could use condition type for deletes?
						messages.Add( string.Format( "Row: {0}. A {3} internal identifier ({1}) cannot have a value of #DELETE. Deletes are handled by providing the identifier in this column, and {2} in the Condition type column.", rowNbr, identifier, DELETE_ME, profileType ) );
						return;
					}
					else if ( entity.IsExistingEntity == false )
					{
						//or just ignore it? If new an existing are entered
						identifier = "";
						//messages.Add( string.Format( "Row: {0}. A requires Condition identifier ({1}) cannot be entered with a new credential: {2}", rowNbr, identifier, entity.Name ) );
					}
					else if ( !ServiceHelper.IsValidGuid( identifier ) )
					{
						messages.Add( string.Format( "Row: {0}. The provided identifier ({1}) is invalid. It must be a valid UUID for a record that actually exists in the database.", rowNbr, identifier ) );
					}
					else
					{
						//will want to ignore if no other requires data, or look up now and handle validation later
						//entity.RequiresCondition.Identifier
						if (entity.ConditionProfiles.Count > 0)
						{
							var exists = entity.ConditionProfiles.FirstOrDefault( a => a.RowId == new Guid( identifier ) );
							if ( exists != null && exists.IsNotEmpty )
							{
								//previously handled, so skip
								return;
							}
						}
						//TODO - should be for the current artifact
						existingProfile = Entity_ConditionProfileManager.GetBasic( new Guid( identifier ) );
						if ( existingProfile == null || existingProfile.Id == 0 )
						{
							messages.Add( string.Format( "Row: {0}. The provided {3} identifier ({1}) does not exist. A valid identifier must be provided in order to update an existing {3} profile. Re: artifact: {2}", rowNbr, identifier, entity.Name, profileType ) );
							return;
						}
						else if ( existingProfile.ParentEntity.EntityBaseId != entity.ExistingParentId || existingProfile.ParentEntity.EntityTypeId != parentEntityTypeId )
						{
							messages.Add( string.Format( "Row: {0}. The provided {3} identifier ({1}) is for a record that is not part of this artifact. Please provide a valid identifier for an existing {3} previously entered for artifact: {2}", rowNbr, identifier, entity.Name, profileType ) );
							return;
						}
						else
						{
							existingProfileUid = new Guid( identifier );
							entity.ConditionProfile.RowId = existingProfileUid;
						}
					}
				}
			}

			//externalIdentifier =====================================================
			if ( importHelper.ConditionExternalIdentifierHdr > -1 )
			{
				externalIdentifier = Assign( rowNbr, csv, importHelper.ConditionExternalIdentifierHdr, "Condition External Identifier", ref messages, "", false );
				if ( !string.IsNullOrWhiteSpace( externalIdentifier ) )
				{
					if ( entity.IsExistingEntity )
					{
						if ( externalIdentifier == DELETE_ME )
						{
							messages.Add( string.Format( "Row: {0}. A {3} External identifier ({1}) cannot have a value of #DELETE. Deletes are handled by providing the previously entered external identifier in this column, and {2} in the Condition Type column.", rowNbr, identifier, DELETE_ME, profileType ) );
							return;
						}
						else
						{
							//should a look up be done here, if not already done?
							//could happen if both internal an external identifiers were provided. 
							//note, in the latter case have to make sure the internal and external identifiers refer to the same record. 
							//if ( existingProfile == null || existingProfile.Id == 0 )
							//{ }
							Guid targetRowId = Factories.Import.ImportHelpers.ExternalIdentifierXref_Get( entity.ExistingParentTypeId, entity.ExistingParentId, CodesManager.ENTITY_TYPE_CONDITION_PROFILE, externalIdentifier, ref status );
							if ( !ServiceHelper.IsValidGuid( targetRowId ) )
							{
								//will not be found for first time, but should be there for existing credential
								if ( existingCount > 1 )
								{
									//then an issue - or could interpret as an add!
									messages.Add( string.Format( "Row: {0}. {3} Profile. An external identifier ({1}) was provided that is not yet associated with an existing {3} profile. There are multiple existing {3} profiles, so the system cannot determine if this {3} is a new one, or meant to be an update.  Re: artifact: {2}", rowNbr, externalIdentifier, entity.Name, profileType ) );
									return;
								}
							}
							else
							{
								var existingProfile2 = Entity_ConditionProfileManager.GetBasic( targetRowId );
								if ( existingProfile2 == null || existingProfile2.Id == 0 )
								{
									messages.Add( string.Format( "Row: {0}. In order for the system to update a {3} profile, a {3} identifier (either internal or external) must be entered. The entered external identifier: {1} is not valid:  for artifact: {2} - that is a related {3} profile is not associated with this external identifier.", rowNbr, externalIdentifier, entity.Name, profileType ) );
									return;
								}
								//this may not be possible given how external identifiers are stored, however:
								else if ( existingProfile2.ParentEntity.EntityBaseId != entity.ExistingParentId || existingProfile2.ParentEntity.EntityTypeId != parentEntityTypeId )
								{
									messages.Add( string.Format( "Row: {0}. The provided {3} external identifier ({1}) is for a record that is not part of this artifact. Please provide a valid external identifier for an existing {3} previously entered for artifact: {2}", rowNbr, identifier, entity.Name, profileType ) );
									return;
								}
								else
								{
									if ( existingProfile != null && existingProfile.Id > 0 )
									{
										if ( existingProfile.Id != existingProfile2.Id )
										{
											messages.Add( string.Format( "Row: {0}. Both an internal identifer ({1}) and an external identifier ({2}) were entered but reference two different {4} profiles. Please provide either a valid internal or external identifier previously entered for artifact: {3}", rowNbr, identifier, externalIdentifier, entity.Name, profileType ) );
											return;
										}
									}
									identifier = existingProfile.RowId.ToString();
									entity.ConditionProfile.RowId = existingProfile.RowId;
								}
							}
						}
					}
					else
					{
						if ( externalIdentifier == DELETE_ME )
						{
							messages.Add( string.Format( "Row: {0}. {2} Profile - Invalid value for External Identifier. You cannot use the value of {1} with a new entity.", rowNbr, DELETE_ME, profileType ) );
							return;
						}
					}
				}
			}

			//conditionally required
			if ( importHelper.ConditionTypeHdr > -1 )
			{
				entity.ConditionProfile.ConditionType = Assign( rowNbr, csv, importHelper.ConditionTypeHdr, "ConditionType", ref messages, "", false );
				if ( !string.IsNullOrWhiteSpace( entity.ConditionProfile.ConditionType ) )
				{
					if ( entity.ConditionProfile.ConditionType == DELETE_ME )
					{
						//this will be valid if only one condition, and previously retrieved
						//or entered
						if ( entity.IsExistingEntity == false )
						{
							messages.Add( string.Format( "Row: {0}. Invalid delete request for a {2} profile. As the related artifact does not yet exist, it is not possible to delete a Condition profile. Re: artifact: {1}", rowNbr, entity.Name, profileType ) );
						}
						else if ( !ServiceHelper.IsValidGuid( identifier ) )
						{
							messages.Add( string.Format( "Row: {0}. Invalid delete request for a {2} profile. A valid {2} external or internal identifier must provided in order to delete an existing {2} for artifact: {1}", rowNbr, entity.Name, profileType ) );
						}
						else
						{
							//if we have a valid identifier, the existing record should have been retrieved by now!
							existingProfile = Entity_ConditionProfileManager.GetBasic( new Guid( identifier ) );
							if ( existingProfile == null || existingProfile.Id == 0 )
							{
								messages.Add( string.Format( "Row: {0}. Invalid delete request for a {3} profile. The provided {3} identifier ({1}) does not exist - so cannot be deleted. Re: artifact: {2}", rowNbr, identifier, entity.Name, profileType ) );
							}
							else
							{
								entity.ConditionProfile.DeletingProfile = true;
								entity.ConditionProfile.RowId = existingProfile.RowId;
								return;
							}
						}
					}
					else
					{
						//OR will we allow NEW
						if ( "requires recommends renewal corequisite".IndexOf( entity.ConditionProfile.ConditionType.ToLower() ) == -1 )
						{
							messages.Add( string.Format( "Row: {0}. The provided condition type ({1}) is invalid. It must be one of Requires, Recommends or Renewal.", rowNbr, entity.ConditionProfile.ConditionType ) );
						}
						else
						{
							//if identifier provided, the type must match actual value
							entity.ConditionProfile.ConditionTypeId = Entity_ConditionProfileManager.GetConditionTypeId( entity.ConditionProfile.ConditionType );
						}
					}
				}
				else
				{
					//validate at end, default to requires if missing
				}
			}
			if ( importHelper.ConditionNameHdr > -1 )
			{
				entity.ConditionProfile.Name = Assign( rowNbr, csv, importHelper.ConditionNameHdr, "Condition.Name", ref messages, "", false );
			}
			if ( importHelper.ConditionSubjectWebpageHdr > -1 )
			{
				entity.ConditionProfile.SubjectWebpage = AssignUrl( rowNbr, csv, importHelper.ConditionSubjectWebpageHdr, "ConditionSubjectWebpageHdr", ref messages, "", false );
			}

			if ( importHelper.ConditionSubmissionHdr > -1 )
			{
				entity.ConditionProfile.SubmissionItems = AssignList( rowNbr, csv, importHelper.ConditionSubmissionHdr, "ConditionSubmissions", ref messages, true );
			}

			//do desc last to check for generating a default
			if ( importHelper.ConditionDescHdr > -1 )
			{
				//conditionally required
				entity.ConditionProfile.Description = Assign( rowNbr, csv, importHelper.ConditionDescHdr, "ConditionDescription", ref messages, "", false );
				//a check is done later for required desc
			}

			// condition target credential,Assessment and Lopp
			string targetCtid = "";
            int targetId = 0;
            if ( importHelper.ConditionCredentialsListHdr > -1 )
            {
                AssignTargetCredential( rowNbr, csv, importHelper.ConditionCredentialsListHdr, entity.ConditionProfile, user, ref messages );
                    
            }
            if ( importHelper.ConditionAsmtsListHdr > -1 )
            {
                AssignTargetAssessment( rowNbr, csv, importHelper.ConditionAsmtsListHdr, entity.ConditionProfile, user, ref messages );
               
            }
            if ( importHelper.ConditionLoppsListHdr > -1 )
            {
                AssignTargetLearningOpp( rowNbr, csv, importHelper.ConditionLoppsListHdr, entity.ConditionProfile, user, ref messages );

            }

            if ( importHelper.ConditionExperienceHdr > -1 )
			{
				entity.ConditionProfile.Experience = Assign( rowNbr, csv, importHelper.ConditionExperienceHdr, "ConditionExperience", ref messages, "", false );
			}

			if ( importHelper.ConditionYearsOfExperienceHdr > -1 )
			{
				entity.ConditionProfile.YearsOfExperience = AssignDecimal( rowNbr, csv, importHelper.ConditionYearsOfExperienceHdr, "ConditionYearsOfExperience", ref messages, false );
			}
			//=======================================================================
			#region Credit hours and units

			if ( importHelper.ConditionCreditHourTypeHdr > -1 )
			{
				//Type of unit of time corresponding to type of credit such as semester hours, quarter hours, clock hours, or hours of participation.
				entity.ConditionProfile.CreditHourType = Assign( rowNbr, csv, importHelper.ConditionCreditHourTypeHdr, "CreditHourType", ref messages, "", false );
			}
			if ( importHelper.ConditionCreditHourValueHdr > -1 )
			{
				entity.ConditionProfile.CreditHourValue = AssignDecimal( rowNbr, csv, importHelper.ConditionCreditHourValueHdr, "CreditHourValue", ref messages, false );
			}
			if ( importHelper.ConditionCreditUnitTypeHdr > -1 )
			{
				//Best practice is to use concepts from a controlled vocabulary such as ceterms:CreditUnit.
				//actually only need the Id.
				entity.ConditionProfile.CreditUnitType = Assign( rowNbr, csv, importHelper.ConditionCreditUnitTypeHdr, "CreditUnitType", ref messages );
				if ( !string.IsNullOrWhiteSpace( entity.ConditionProfile.CreditUnitType ) )
				{
					if ( entity.ConditionProfile.CreditUnitType != DELETE_ME )
					{
						EnumeratedItem ei = CodesManager.GetCodeAsEnumerationItem( CodesManager.PROPERTY_CATEGORY_CREDIT_UNIT_TYPE, entity.ConditionProfile.CreditUnitType );
						if ( ei != null && ei.Id > 0 )
							entity.ConditionProfile.CreditUnitTypeId = ei.Id;
						else
							messages.Add( string.Format( "Row: {0} Invalid {1} of {2}", rowNbr, "CreditUnitType", entity.ConditionProfile.CreditUnitType ) );
					}
				}
			} //

			if ( importHelper.ConditionCreditUnitValueHdr > -1 )
			{
				entity.ConditionProfile.CreditUnitValue = AssignDecimal( rowNbr, csv, importHelper.ConditionCreditUnitValueHdr, "CreditUnitValue", ref messages, false );
			}
			if ( importHelper.ConditionCreditUnitDescriptionHdr > -1 )
			{
				entity.ConditionProfile.CreditUnitTypeDescription = Assign( rowNbr, csv, importHelper.ConditionCreditUnitDescriptionHdr, "ConditionCreditUnitDescription", ref messages );
			}
            //can only have credit hours properties, or credit unit properties, not both
            bool hasCreditHourData = false;
            bool hasCreditUnitData = false;
            if ( entity.ConditionProfile.CreditHourValue > 0 || ( entity.ConditionProfile.CreditHourType ?? "" ).Length > 0 )
                hasCreditHourData = true;
            if ( entity.ConditionProfile.CreditUnitTypeId > 0
                || ( entity.ConditionProfile.CreditUnitTypeDescription ?? "" ).Length > 0
                || entity.ConditionProfile.CreditUnitValue > 0 )
                hasCreditUnitData = true;

            if ( hasCreditHourData && hasCreditUnitData )
                messages.Add( "Error: Data can be entered for Credit Hour related properties or Credit Unit related properties, but not for both." );
			#endregion

			//====================================================================
			//         string targetCtid = "";
			//int targetId = 0;
			if ( importHelper.ConditionExistingAsmtHdr > -1 )
			{
				//if present, will be ctid for existing asmt, or may need to be flexible
				//      - an external id, an internal id?
				//      - allow a list
				//must have other condition properties, especially on update
				targetCtid = Assign( rowNbr, csv, importHelper.ConditionExistingAsmtHdr, "ConditionAssessmentCtid", ref messages, "", false );
				if ( prevTargetCtid == targetCtid )
				{
					entity.ConditionProfile.TargetAssessmentList.Add(lastAsmt);
					if ( string.IsNullOrWhiteSpace( entity.ConditionProfile.Description ) )
					{
						entity.ConditionProfile.Description = string.Format( "To earn this credential, candidates must complete the '{0}' assessment.", lastAsmt.Name );
					}

				}
				else
				{
					//get org and ensure can view
					lastAsmt = AssessmentManager.GetByCtid( targetCtid );
					if ( lastAsmt == null || lastAsmt.Id == 0 )
					{
						messages.Add( string.Format( "Row: {0}. An assessment was not found for the provided CTID: {1}", rowNbr, targetCtid ) );
					}
					else
					{
						//confirm has access - may not be necessary for an assessment, skip for now
						//if (AssessmentServices.CanUserUpdateAssessment( lastAsmt.Id, user, ref status ) == false)
						//{
						//    messages.Add( string.Format( "Row: {0}. You do not have update rights for the referenced assessment (via CTID): {1} ({2}). ", rowNbr, owningOrg.Name, owningOrg.Id ) );
						//}
						//else
						{
							entity.ConditionProfile.TargetAssessmentList.Add( lastAsmt );
							prevTargetCtid = targetCtid;
							if ( string.IsNullOrWhiteSpace( entity.ConditionProfile.Description ) )
							{
								entity.ConditionProfile.Description = string.Format( "To earn this credential, candidates must complete the '{0}' assessment.", lastAsmt.Name );
							}
						}
					}

				}
			}

			if ( entity.ConditionProfile.IsNotEmpty )
			{
				if ( string.IsNullOrWhiteSpace( entity.ConditionProfile.ConditionType ) )
				{
					if ( existingProfile != null && existingProfile.Id > 0 )
					{
						//dont' allow type chg at this time, and don't flag as error
						entity.ConditionProfile.ConditionTypeId = existingProfile.ConnectionProfileTypeId;
						entity.ConditionProfile.ConditionType = existingProfile.ConnectionProfileType;
					}
					else
					{
						entity.ConditionProfile.ConditionType = "Requires";
						entity.ConditionProfile.ConditionTypeId = 1;
					}
				}
				if ( string.IsNullOrWhiteSpace( entity.ConditionProfile.Description ) )
				{
					messages.Add( string.Format( "Row: {0}. Missing Condition Description. If any condition data is included, a condition Description must also be included. ", rowNbr ) );
				}
				else if ( entity.ConditionProfile.Description == DELETE_ME )
				{
					messages.Add( string.Format( "Row: {0}. Invalid use of #DELETE with Condition Description. Condition Description is required when any Condition information is provided, it can not be deleted. ", rowNbr ) );
				}

				if ( entity.IsExistingEntity )
				{
					if ( string.IsNullOrWhiteSpace( identifier )
						&& string.IsNullOrWhiteSpace( externalIdentifier )
						&& existingCount > 0 )
					{
						if ( existingCount > 1 )
						{
							messages.Add( string.Format( "Row: {0}. In order for the system to update a condition profile, a condition identifier (either internal or external) must be entered with a existing credential: {1}. It is recommended to always include an external identifier for a cost profile.", rowNbr, entity.Name ) );
							return;
						}
					}
					else
					{
						entity.ConditionProfile.ExternalIdentifier = externalIdentifier;
						//at this point we would have retrieved an existing profile by identifier, or ext identifier, or set to first item from list of all profiles. 
						//  - Unless there is more than one profile, in which case don't have an identifier
						if ( string.IsNullOrWhiteSpace( identifier ) || !ServiceHelper.IsValidGuid( identifier ) )
						{
							if ( existingCount > 1 )
							{
								messages.Add( string.Format( "Row: {0}. There are multiple existing condition profiles for this credential. In order for the system to update a condition profile, either an external condition identifier or an internal (as generated by exporting existing records) condition identifier must be entered. Credential: {1}", rowNbr, entity.Name ) );

							}
						}

					}
				}
			}
			else
			{
				//no action, identifier will be ignored
			}
			if ( entity.ConditionProfile.IsNotEmpty )
				entity.ConditionProfiles.Add( entity.ConditionProfile );
		}//

		/// <summary>
		/// TBD - just cloned, needs to be made operational
		/// Validation
		/// - requires connection type, description and one or more of target credential, target asmt, or target lopp
		/// </summary>
		/// <param name="rowNbr"></param>
		/// <param name="csv"></param>
		/// <param name="entity"></param>
		/// <param name="user"></param>
		/// <param name="importHelper"></param>
		/// <param name="messages"></param>
		public void AssignConnectionProfile( int rowNbr, CsvReader csv, BaseDTO entity, int parentEntityTypeId, AppUser user, CommonImportRequest importHelper, ref List<string> messages )
		{
			string profileType = "Connection";
			bool hasIdentifier = false;
			string identifier = "";
			string status = "";
			string externalIdentifier = "";
			entity.ConnectionProfile = new ConditionProfileDTO();
			int existingCount = 0;
			Guid existingProfileUid = new Guid();
            if ( parentEntityTypeId == 1 )
            {
                entity.ConnectionProfile.ConditionSubTypeId = 2;
            }
           else if ( parentEntityTypeId == 3 )
            {
                entity.ConnectionProfile.ConditionSubTypeId = 3;
            }
            else if ( parentEntityTypeId == 7 )
            {
                entity.ConnectionProfile.ConditionSubTypeId = 4;
            }
            else 
            {
                messages.Add( string.Format( "Row: {0}. The provided entityType Id ({1}) is invalid.", rowNbr, parentEntityTypeId ) );
                return;
            }
			//ConditionProfile profile = new ConditionProfile();
			ConditionProfile existingProfile = new ConditionProfile();
			int msgcnt = messages.Count;

			if ( entity.IsExistingEntity )
			{
				//check if has connections i.e. subconditions
				List<ConditionProfile> conditions = Entity_ConditionProfileManager.GetAll( entity.ExistingParentRowId, entity.ConnectionProfile.ConditionSubTypeId );
				if ( conditions != null && conditions.Count > 0 )
				{
					existingCount = conditions.Count;
					//this doesn't seem correct
					//existingProfileUid = conditions[ 0 ].RowId;
					if ( conditions.Count == 1 )
					{
						//these can be overridden if provided in upload
						existingProfileUid = conditions[ 0 ].RowId;
						identifier = existingProfileUid.ToString();
						existingProfile = conditions[ 0 ];
						entity.ConnectionProfile.RowId = existingProfileUid;
					}
				}
			}

			//internaldentifier =====================================================
			//note this is auto added to an export - which may be confusing. 
			//	- not all users will do an export before updates.
			//	- valuable where other conditions have been added manually.
			//if there is no other data, this should be ignored
			//tbd
			if ( importHelper.ConnectionIdentifierHdr > -1 )
			{
				identifier = Assign( rowNbr, csv, importHelper.ConnectionIdentifierHdr, "ConnectionIdentifier", ref messages, "", false );
				if ( !string.IsNullOrWhiteSpace( identifier ) )
				{
					if ( identifier == DELETE_ME )
					{
						//hmmm this may not work in this field, will need the identifier to delete - in case of multiple
						//or could use condition type for deletes?
						messages.Add( string.Format( "Row: {0}. A {3} internal identifier ({1}) cannot have a value of #DELETE. Deletes are handled by providing the identifier in this column, and {2} in the connection type column.", rowNbr, identifier, DELETE_ME, profileType ) );
						return;
					}
					else if ( entity.IsExistingEntity == false )
					{
						//or just ignore it? If new an existing are entered
						identifier = "";
						//messages.Add( string.Format( "Row: {0}. A requires connection identifier ({1}) cannot be entered with a new credential: {2}", rowNbr, identifier, entity.Name ) );
					}
					else if ( !ServiceHelper.IsValidGuid( identifier ) )
					{
						messages.Add( string.Format( "Row: {0}. The provided identifier ({1}) is invalid. It must be a valid UUID for a record that actually exists in the database.", rowNbr, identifier ) );
					}
					else
					{
						hasIdentifier = true;
						//will want to ignore if no other requires data, or look up now and handle validation later
						if ( entity.ConnectionProfiles.Count > 0 )
						{
							var exists = entity.ConnectionProfiles.FirstOrDefault( a => a.RowId == new Guid( identifier ) );
							if ( exists != null && exists.IsNotEmpty )
							{
								//previously handled, so skip
								return;
							}
						}
						//TODO - should be for the current artifact
						existingProfile = Entity_ConditionProfileManager.GetBasic( new Guid( identifier ) );
						if ( existingProfile == null || existingProfile.Id == 0 )
						{
							messages.Add( string.Format( "Row: {0}. The provided {3} identifier ({1}) does not exist. A valid identifier must be provided in order to update an existing {3} profile. Re: artifact: {2}", rowNbr, identifier, entity.Name, profileType ) );
							return;
						} else if (existingProfile.ParentEntity.EntityBaseId != entity.ExistingParentId || existingProfile.ParentEntity.EntityTypeId != parentEntityTypeId )
						{
							messages.Add( string.Format( "Row: {0}. The provided {3} identifier ({1}) is for a record that is not part of this artifact. Please provide a valid identifier for an existing {3} previously entered for artifact: {2}", rowNbr, identifier, entity.Name, profileType ) );
							return;
						}
						else
						{
							existingProfileUid = new Guid( identifier );
							entity.ConnectionProfile.RowId = existingProfileUid;
						}
					}
				}
			}
			//externalIdentifier =====================================================
			if ( importHelper.ConnectionExternalIdentifierHdr > -1 )
			{
				externalIdentifier = Assign( rowNbr, csv, importHelper.ConnectionExternalIdentifierHdr, "ConnectionExternalIdentifier", ref messages, "", false );
				if ( !string.IsNullOrWhiteSpace( externalIdentifier ) )
				{
					if ( entity.IsExistingEntity )
					{
						if ( externalIdentifier == DELETE_ME )
						{
							messages.Add( string.Format( "Row: {0}. A {3} External identifier ({1}) cannot have a value of #DELETE. Deletes are handled by providing the previously entered external identifier in this column, and {2} in the Connection Type column.", rowNbr, identifier, DELETE_ME, profileType ) );
							return;
						}
						else
						{
							//should a look up be done here, if not already done?
							//could happen if both internal an external identifiers were provided. 
							//note, in the latter case have to make sure the internal and external identifiers refer to the same record. 
							//if ( existingProfile == null || existingProfile.Id == 0 )
							//{ }
							Guid targetRowId = Factories.Import.ImportHelpers.ExternalIdentifierXref_Get( entity.ExistingParentTypeId, entity.ExistingParentId, CodesManager.ENTITY_TYPE_CONDITION_PROFILE, externalIdentifier, ref status );
							if ( !ServiceHelper.IsValidGuid( targetRowId ) )
							{
								//will not be found for first time, but should be there for existing credential
								if ( existingCount > 1 )
								{
									//then an issue - or could interpret as an add!
									messages.Add( string.Format( "Row: {0}. {3} Profile. An external identifier ({1}) was provided that is not yet associated with an existing {3} profile. There are multiple existing {3} profiles, so the system cannot determine if this {3} is a new one, or meant to be an update.  Re: artifact: {2}", rowNbr, externalIdentifier, entity.Name, profileType ) );
									return;
								}
							}
							else
							{
								var existingProfile2 = Entity_ConditionProfileManager.GetBasic( targetRowId );
								if ( existingProfile2 == null || existingProfile2.Id == 0 )
								{
									messages.Add( string.Format( "Row: {0}. In order for the system to update a {3} profile, a {3} identifier (either internal or external) must be entered. The entered external identifier: {1} is not valid:  for artifact: {2} - that is a related {3} profile is not associated with this external identifier.", rowNbr, externalIdentifier, entity.Name, profileType ) );
									return;
								}
								//this may not be possible given how external identifiers are stored, however:
								else if ( existingProfile2.ParentEntity.EntityBaseId != entity.ExistingParentId || existingProfile2.ParentEntity.EntityTypeId != parentEntityTypeId )
								{
									messages.Add( string.Format( "Row: {0}. The provided {3} external identifier ({1}) is for a record that is not part of this artifact. Please provide a valid external identifier for an existing {3} previously entered for artifact: {2}", rowNbr, identifier, entity.Name, profileType ) );
									return;
								}
								else
								{
									if ( existingProfile != null && existingProfile.Id > 0 )
									{
										if (existingProfile.Id != existingProfile2.Id)
										{
											messages.Add( string.Format( "Row: {0}. Both an internal identifer ({1}) and an external identifier ({2}) were entered but reference two different {4} profiles. Please provide either a valid internal or external identifier previously entered for artifact: {3}", rowNbr, identifier, externalIdentifier, entity.Name, profileType ) );
											return;
										}
									}
									identifier = existingProfile.RowId.ToString();
									entity.ConnectionProfile.RowId = existingProfile.RowId;
								}
							}
						}
					}
					else
					{
						if ( externalIdentifier == DELETE_ME )
						{
							messages.Add( string.Format( "Row: {0}. {2} Profile - Invalid value for External Identifier. You cannot use the value of {1} with a new entity.", rowNbr, DELETE_ME, profileType ) );
							return;
						}
					}
				}
			}

			//conditionally required
			if ( importHelper.ConnectionTypeHdr > -1 )
			{
				entity.ConnectionProfile.ConditionType = Assign( rowNbr, csv, importHelper.ConnectionTypeHdr, "ConnectionType", ref messages, "", false );
				if ( !string.IsNullOrWhiteSpace( entity.ConnectionProfile.ConditionType ) )
				{
					if ( entity.ConnectionProfile.ConditionType == DELETE_ME )
					{
						//this will be valid if only one condition, and previously retrieved
						//or entered
						if ( entity.IsExistingEntity == false )
						{
							messages.Add( string.Format( "Row: {0}. Invalid delete request for a {2} profile. As the related artifact does not yet exist, it is not possible to delete a connection profile. Re: artifact: {1}", rowNbr, entity.Name, profileType ) );
						}
						else if( !ServiceHelper.IsValidGuid( identifier ) )
						{
							messages.Add( string.Format( "Row: {0}. Invalid delete request for a {2} profile. A valid {2} external or internal identifier must provided in order to delete an existing {2} for artifact: {1}", rowNbr, entity.Name, profileType ) );
						}
						else 
						{
							//if we have a valid identifier, the existing record should have been retrieved by now!
							existingProfile = Entity_ConditionProfileManager.GetBasic( new Guid( identifier ) );
							if ( existingProfile == null || existingProfile.Id == 0 )
							{
								messages.Add( string.Format( "Row: {0}. Invalid delete request for a {3} profile. The provided {3} identifier ({1}) does not exist - so cannot be deleted. Re: artifact: {2}", rowNbr, identifier, entity.Name, profileType ) );
							}
							else
							{
								entity.ConnectionProfile.DeletingProfile = true;
								entity.ConnectionProfile.RowId = existingProfile.RowId;
								return;
							}
						}
					}
					else
					{
						//TODO - add validation using code table lookup
						//**************
						EnumeratedItem ei = CodesManager.ConditionProfileTypesCodeAsEnumerationItem( entity.ConnectionProfile.ConditionType );
						if ( ei != null && ei.Id > 0 )
							entity.ConnectionProfile.ConditionTypeId = ei.Id;
						else
							messages.Add( string.Format( "Row: {0}. The provided {2} type ({1}) is invalid. It must be one of the valid connection types.", rowNbr, entity.ConnectionProfile.ConditionType, profileType ) );
					}
				}
				else
				{
                   // messages.Add( string.Format( "Row: {0}. The {1} type is required and is invalid. It must be one of the valid {1} types.", rowNbr, profileType ) );
                    //validate at end, default to requires if missing
                }
			}


			//do desc last to check for generating a default
			if ( importHelper.ConnectionDescHdr > -1 )
			{
				//Connectionally required
				entity.ConnectionProfile.Description = Assign( rowNbr, csv, importHelper.ConnectionDescHdr, "ConditionDescription", ref messages, "", false );
				//a check is done later for required desc
			} 
			

			//====================================================================
			/* todo
			 * - allow a list
			 * - could contain ctid, integer (internal identifier?), or name~subject webpage
			 * - use something like AssignApprovedByList as a model.
			 * - must have one of credentials, asmts, or lopps, and can have more than one
			 * - probably should use a separate method rather than inline here.
			 */

			if ( importHelper.ConnectionCredentialsListHdr > -1 )
			{
                //if present, will be ctid for existing artifact, or may need to be flexible
                //      - an external id, an internal id?
                //      - allow a list
                //must have other connection properties, especially on update
                AssignTargetCredential( rowNbr, csv, importHelper.ConnectionCredentialsListHdr, entity.ConnectionProfile, user, ref messages );
			}
			if ( importHelper.ConnectionAsmtsListHdr > -1 )
			{
                AssignTargetAssessment( rowNbr, csv, importHelper.ConnectionAsmtsListHdr, entity.ConnectionProfile, user, ref messages );
            }
			if ( importHelper.ConnectionLoppsListHdr > -1 )
			{
                AssignTargetLearningOpp( rowNbr, csv, importHelper.ConnectionLoppsListHdr, entity.ConnectionProfile, user, ref messages );
			}
			//=======================================================================
			#region Credit hours and units
			if ( importHelper.ConnectionCreditHourTypeHdr > -1 )
			{
				//Type of unit of time corresponding to type of credit such as semester hours, quarter hours, clock hours, or hours of participation.
				entity.ConnectionProfile.CreditHourType = Assign( rowNbr, csv, importHelper.ConnectionCreditHourTypeHdr, "CreditHourType", ref messages, "", false );
			}
			if ( importHelper.ConnectionCreditHourValueHdr > -1 )
			{
				entity.ConnectionProfile.CreditHourValue = AssignDecimal( rowNbr, csv, importHelper.ConnectionCreditHourValueHdr, "CreditHourValue", ref messages, false );
			}
			if ( importHelper.ConnectionCreditUnitTypeHdr > -1 )
			{
				//Best practice is to use concepts from a controlled vocabulary such as ceterms:CreditUnit.
				//actually only need the Id.
				entity.ConnectionProfile.CreditUnitType = Assign( rowNbr, csv, importHelper.ConnectionCreditUnitTypeHdr, "CreditUnitType", ref messages );
				if ( !string.IsNullOrWhiteSpace( entity.ConnectionProfile.CreditUnitType ) )
				{
					if ( entity.ConnectionProfile.CreditUnitType != DELETE_ME )
					{
						EnumeratedItem ei = CodesManager.GetCodeAsEnumerationItem( CodesManager.PROPERTY_CATEGORY_CREDIT_UNIT_TYPE, entity.ConnectionProfile.CreditUnitType );
						if ( ei != null && ei.Id > 0 )
							entity.ConnectionProfile.CreditUnitTypeId = ei.Id;
						else
							messages.Add( string.Format( "Row: {0} Invalid {1} of {2}", rowNbr, "CreditUnitType", entity.ConnectionProfile.CreditUnitType ) );
					}
				}
			} //

			if ( importHelper.ConnectionCreditUnitValueHdr > -1 )
			{
				entity.ConnectionProfile.CreditUnitValue = AssignDecimal( rowNbr, csv, importHelper.ConnectionCreditUnitValueHdr, "CreditUnitValue", ref messages, false );
			}
			if ( importHelper.ConnectionCreditUnitDescriptionHdr > -1 )
			{
				entity.ConnectionProfile.CreditUnitTypeDescription = Assign( rowNbr, csv, importHelper.ConnectionCreditUnitDescriptionHdr, "ConnectionCreditUnitDescription", ref messages );
			}

			if ( importHelper.ConnectionWeightHdr > -1 )
			{
				entity.ConnectionProfile.Weight = AssignDecimal( rowNbr, csv, importHelper.ConnectionWeightHdr, "ConnectionWeight", ref messages, false );
			}
			#endregion

			// =========================================================================
			if ( entity.ConnectionProfile.IsNotEmpty )
			{
				if ( string.IsNullOrWhiteSpace( entity.ConnectionProfile.ConditionType ) )
				{
					if ( existingProfile != null && existingProfile.Id > 0 )
					{
						//dont' allow type chg at this time, and don't flag as error
						entity.ConnectionProfile.ConditionTypeId = existingProfile.ConnectionProfileTypeId;
						entity.ConnectionProfile.ConditionType = existingProfile.ConnectionProfileType;
					}
					else
					{
						messages.Add( string.Format( "Row: {0}. Missing {1} Connection Type. If any {1} data is included, a Connection Type must also be included. ", rowNbr, profileType ) );
					}
				}
				if ( string.IsNullOrWhiteSpace( entity.ConnectionProfile.Description ) )
				{
					messages.Add( string.Format( "Row: {0}. Missing {1} Description. If any {1} data is included, a {1} Description must also be included. ", rowNbr, profileType ) );
				}
				else if ( entity.ConnectionProfile.Description == DELETE_ME )
				{
					messages.Add( string.Format( "Row: {0}. Invalid use of #DELETE with Connection Description. Connection Description is required when any Connection information is provided, it can not be deleted. ", rowNbr ) );
				}

				if ( entity.IsExistingEntity )
				{
					if ( string.IsNullOrWhiteSpace( identifier )
						&& string.IsNullOrWhiteSpace( externalIdentifier )
						&& existingCount > 0 )
					{
						if ( existingCount > 1 )
						{
							messages.Add( string.Format( "Row: {0}. In order for the system to update a {2} profile, a {2} identifier (either internal or external) must be entered with a existing artifact: {1}. It is recommended to always include an external identifier for a {2} profile.", rowNbr, entity.Name, profileType ) );
							return;
						}
					}
					else
					{
						entity.ConnectionProfile.ExternalIdentifier = externalIdentifier;
						//at this point we would have retrieved an existing profile by identifier, or ext identifier, or set to first item from list of all profiles. 
						//  - Unless there is more than one profile, in which case don't have an identifier
						if ( string.IsNullOrWhiteSpace( identifier ) || !ServiceHelper.IsValidGuid( identifier ) )
						{
							if ( existingCount > 1 )
							{
								messages.Add( string.Format( "Row: {0}. There are multiple existing {2} profiles for this artifact. In order for the system to update a {2} profile, either an external identifier or an internal (as generated by exporting existing records) identifier must be entered. Artifact: {1}", rowNbr, entity.Name, profileType ) );

							}
						}

					}
				}
			}
			else
			{
				//no action, identifier will be ignored
			}
			if ( entity.ConnectionProfile.IsNotEmpty )
				entity.ConnectionProfiles.Add( entity.ConnectionProfile );

		}//

		/// <summary>
		/// MP - Not sure if this should be implemented or removed
		/// </summary>
		/// <param name="rowNbr"></param>
		/// <param name="csv"></param>
		/// <param name="entity"></param>
		/// <param name="user"></param>
		/// <param name="importHelper"></param>
		/// <param name="isExistingEntity"></param>
		/// <param name="parentEntityUid"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public void AssignCosts( int rowNbr, CsvReader csv, BaseDTO entity, AppUser user, CommonImportRequest importHelper, bool isExistingEntity, int parentEntityTypeId, Guid parentEntityUid, ref List<string> messages )
		{
			CostProfileDTO costProfile = new CostProfileDTO();
			CostProfile existingProfile = new CostProfile();
			CostProfile existingProfileByExternalId = new CostProfile();
			Entity parentEntity = new Entity();
			if ( isExistingEntity )
				parentEntity = EntityManager.GetEntity( parentEntityUid, false );

			string identifier = "";
			string externalIdentifier = "";
			int existingCostsCount = 0;
			Guid existingProfileUid = new Guid();
			int msgcnt = messages.Count;

			//new - verify
			if ( isExistingEntity || entity.FoundExistingRecord )
			{
				//check if has costs
				var existingCostProfiles = CostProfileManager.GetAll( entity.ExistingParentRowId, false );
				if ( existingCostProfiles != null && existingCostProfiles.Count > 0 )
				{
					existingCostsCount = existingCostProfiles.Count;
					if ( existingCostProfiles.Count == 1 )
					{
						existingProfileUid = existingCostProfiles[ 0 ].RowId;
						//not sure why setting this, as expect it as input?
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
				identifier = Assign( rowNbr, csv, importHelper.CostInternalIdentifierHdr, "Cost Internal Identifier", ref messages, "", false );
				if ( !string.IsNullOrWhiteSpace( identifier ) )
				{
					//maybe not ever allow delete here?
					if ( identifier == DELETE_ME )
					{
						messages.Add( string.Format( "Row: {0}. A cost profile identifier ({1}) cannot have a value of #DELETE. Deletes are handled by providing the identifier in this column, and {2} in the external identifier column.", rowNbr, identifier, DELETE_ME ) );
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
					else if ( isExistingEntity == false )
					{
						//just ignore, could be from export in another env.
						//messages.Add( string.Format( "Row: {0}. A cost profile identifier ({1}) cannot be entered with a new credential: {2}", rowNbr, identifier, entity.Name ) );
					}
					else
					{
						//check if this is a duplicate row
						//entity.RequiresCondition.Identifier
						if ( entity.CostProfiles.Count() > 0 )
						{
							var exists = entity.CostProfiles.FirstOrDefault( a => a.Identifier == new Guid( identifier ) );
							if ( exists != null && exists.IsNotEmpty )
							{
								//previously handled, so skip
								return;
							}
						}
						costProfile.Identifier = new Guid( identifier );
						//will want to ignore if no other requires data
						//do a lookup to see if references existing profile
						existingProfile = CostProfileManager.GetBasicProfile( costProfile.Identifier );
						if ( existingProfile == null || existingProfile.Id == 0 )
						{
							//may not be an error - unless only allow for existing!
							messages.Add( string.Format( "Row: {0}. A cost profile identifier ({1}) was provided, but the system could not find this record. It must be a valid UUID for a cost profile that actually exists in the database.", rowNbr, identifier ) );
							return;
						}
						else 
						if ( existingProfile.RelatedEntity != null && ( existingProfile.RelatedEntity.EntityBaseId != entity.ExistingParentId || existingProfile.RelatedEntity.EntityTypeId != parentEntityTypeId )
							)
						{
							messages.Add( string.Format( "Row: {0}. The provided {3} identifier ({1}) is for a record that is not part of this artifact. Please provide a valid identifier for an existing {3} previously entered for artifact: {2}", rowNbr, identifier, entity.Name, "CostProfile" ) );
							return;
						}
						else
						{
							existingProfileUid = new Guid( identifier );
							costProfile.Identifier = new Guid( identifier );
							costProfile.ExistingCostProfile = existingProfile;
							//what else?
						}
					}
				}
			}
			//external 
			if ( importHelper.CostExternalIdentifierHdr > -1 )
			{
				externalIdentifier = Assign( rowNbr, csv, importHelper.CostExternalIdentifierHdr, "Cost External Identifier", ref messages, "", false );
				if ( !string.IsNullOrWhiteSpace( externalIdentifier ) )
				{
					// check if in previous list - or better from list in parent
					//format is up to user, just unique to the credential
					if ( isExistingEntity == true )
					{
						//action?
						//see Credential.AssignCosts
						if ( externalIdentifier == DELETE_ME )
						{
							if ( existingCostsCount == 1
							|| ( existingCostsCount > 1 && ServiceHelper.IsValidGuid( existingProfileUid ) ) )
							{
								costProfile.DeletingProfile = true;
								costProfile.Identifier = existingProfileUid;
								//or
								costProfile.DeletingProfile = true;
								costProfile.Identifier = existingProfileUid;
								return;
							}
							else if ( existingCostsCount > 1 )
							{
								messages.Add( string.Format( "Row: {0}. Cost Profile - You have entered {1} for External Identifier. However, there are more than 1 existing cost profiles. The system cannot determine which cost profile should be deleted. Please provide the internal identifier for the related cost profile. HINT: do an export of existing credentials, and the related internal identifers will be provided. ", rowNbr, externalIdentifier ) );
								return;
							}
							else if ( existingCostsCount == 0 )
							{
								messages.Add( string.Format( "Row: {0}. Cost Profile - You have entered {1} for External Identifier. However, there are no existing cost profiles. This is an inconsistent request, so the record is being rejected. ", rowNbr, externalIdentifier ) );
								return;
							}
						}
						else
						{
							//should a look up be done here, if not already done?
							//could have done look up by identifier, could point to different entities?
							if ( existingProfile == null || existingProfile.Id == 0 )
							{
								string status = "";
								Guid targetRowId = Factories.Import.ImportHelpers.ExternalIdentifierXref_Get( parentEntityTypeId, entity.ExistingRecord.Id, CodesManager.ENTITY_TYPE_COST_PROFILE, externalIdentifier, ref status );
								if ( !ServiceHelper.IsValidGuid( targetRowId ) )
								{
									//will not be found for first time, but should be there for existing credential
									if ( existingCostsCount > 1 )
									{
										//then an issue - or could interpret as an add!
										messages.Add( string.Format( "Row: {0}. CostProfile. An external identifier ({1}) was provided that is not yet associated with an existing cost profile. There are existing cost profiles, so the system cannot determine if this cost is a new one, or meant to be an update.  Re: entity: {2}", rowNbr, externalIdentifier, entity.Name ) );
										return;
									}
								}
								else
								{
									costProfile.Identifier = targetRowId;
									existingProfileByExternalId = CostProfileManager.GetBasicProfile( targetRowId );
									if ( existingProfileByExternalId == null || existingProfileByExternalId.Id == 0 )
									{
										messages.Add( string.Format( "Row: {0}. In order for the system to update a cost profile, a cost identifier (either internal or external) must be entered. The entered external identifier: {1} is not valid:  for record: {2} - that is a related cost profile is not associated with this external identifier.", rowNbr, externalIdentifier, entity.Name ) );
										return;
									}
									//this may not be possible given how external identifiers are stored, however:
									else if ( existingProfileByExternalId.RelatedEntity != null && ( existingProfileByExternalId.RelatedEntity.EntityBaseId != entity.ExistingParentId || existingProfileByExternalId.RelatedEntity.EntityTypeId != parentEntityTypeId )
										)
									{
										messages.Add( string.Format( "Row: {0}. The provided {3} external identifier ({1}) is for a record that is not part of this artifact. Please provide a valid external identifier for an existing {3} previously entered for artifact: {2}", rowNbr, identifier, entity.Name, "CostProfile" ) );
										return;
									}
									else
									{
										costProfile.ExistingCostProfile = existingProfileByExternalId;
										identifier = existingProfileByExternalId.RowId.ToString();
										costProfile.Identifier = existingProfileUid;
										if ( existingProfile != null || existingProfile.Id > 0 )
										{
											if ( existingProfile.Id != existingProfileByExternalId.Id )
											{
												messages.Add( string.Format( "Row: {0}. In order for the system to update a cost profile, a cost identifier (either internal or external) must be entered. Both internal {1} and external identifier: {2} have been entered, but refer to different cost profiles. Please only enter one of these, or at least ensure both relate to the same cost profile.", rowNbr, externalIdentifier, entity.Name ) );
												return;
											}

										}
									}
								}
							}
							
						}
					}
					else
					{
						//may just defer to end

						//will want to ignore if no other requires data
						//NOTE: ensure ExternalIdentifierXref gets created ==> OK
						//do a lookup to see if references existing profile
						if ( externalIdentifier == DELETE_ME )
						{
							messages.Add( string.Format( "Row: {0}. Cost Profile - Invalid value for External Identifier: {1}. You cannot use the value of #DELETEME with a new record.", rowNbr, externalIdentifier ) );
							return;
						}

					}
				}
			} //

			//will need an edit for this in the validate headers. 
			if ( importHelper.CostDetailUrlHdr > -1 )
			{
				costProfile.DetailsUrl = AssignUrl( rowNbr, csv, importHelper.CostDetailUrlHdr, "CostProfile.DetailsUrl", ref messages, "", false );
			}
			if ( importHelper.CostNameHdr > -1 )
			{
				costProfile.Name = Assign( rowNbr, csv, importHelper.CostNameHdr, "CostProfile.CostNameHdr", ref messages, "", true );
			}

			if ( importHelper.CostCurrencyTypeHdr > -1 )
			{
				//will want some sort of validation for this
				string currency = Assign( rowNbr, csv, importHelper.CostCurrencyTypeHdr, "CostProfile.CostCurrencyType", ref messages, "", false );
				if ( !string.IsNullOrWhiteSpace( currency ) )
				{
					costProfile.CurrencyType = currency;
					if ( costProfile.CurrencyType.ToLower() == "usd" )
					{
						costProfile.CurrencyTypeId = 840;
					}
					else
					{
						EnumeratedItem ei = CodesManager.GetCurrencyItem( costProfile.CurrencyType );
						if ( ei != null && ei.Id > 0 )
						{
							costProfile.CurrencyTypeId = ei.Id;
						}
						else
						{
							messages.Add( string.Format( "Row: {0}. The currency type is not a known code: {1}", rowNbr, currency ) );
						}
					}
				}
			}
			//do desc last to check for generating a default
			if ( importHelper.CostDescriptionHdr > -1 )
			{
				costProfile.Description = Assign( rowNbr, csv, importHelper.CostDescriptionHdr, "CostProfile.Description", ref messages, "", false );
			}


			if ( importHelper.CostTypesListHdr > -1 )
			{
				string list = Assign( rowNbr, csv, importHelper.CostTypesListHdr, "Cost Types List", ref messages, "", false );
				if ( !string.IsNullOrWhiteSpace( list ) )
				{
					if ( list == DELETE_ME )
					{
						costProfile.DeleteCostItems = true;
					}
					else
					{
						//type1~price1~future1|type2~price2~future2
						string[] array = list.Split( '|' );
						if ( array.Count() > 0 )
						{
							int cntr = 0;
							foreach ( var item in array )
							{
								cntr++;
								if ( string.IsNullOrWhiteSpace( item.Trim() ) )
									continue;

								string[] parts = item.Split( '~' );
								//for now expecting at lease type, and price
								if ( parts.Count() < 2 )
								{
									messages.Add( string.Format( "Row: {0} Costs Entry Number: {1}. Cost types list must contain a cost type and a price. Entry: {2}", rowNbr, cntr, item ) );
									continue;
								}

								CostProfileItemDTO costItem = new CostProfileItemDTO();
								//validate type
								costItem.DirectCostType = parts[ 0 ];
								EnumeratedItem ei = CodesManager.GetCodeAsEnumerationItem( CodesManager.PROPERTY_CATEGORY_CREDENTIAL_ATTAINMENT_COST, costItem.DirectCostType );
								if ( ei == null || ei.Id == 0 )
									messages.Add( string.Format( "Row: {0} Invalid direct cost type of {1}", rowNbr, costItem.DirectCostType ) );
								else
								{
									//not sure which will use
									costItem.DirectCostTypeId = ei.Id;
									costItem.CostItem = ei;
								}

								decimal price = 0;
								if ( decimal.TryParse( parts[ 1 ], out price ) )
								{
									costItem.Price = price;
									costProfile.CostItems.Add( costItem );
								}
								else
								{
									messages.Add( string.Format( "Row: {0} Costs Entry Number: {1}. Invalid value for price. This must be a valid integer or decimal. Price. : {2}", rowNbr, cntr, parts[ 1 ] ) );
								}

							} //foreach

						}
					}
				}

			}

			//============================================================
			//if we have any cost data, verify we have the correct minimum data
			if ( costProfile != null && costProfile.IsNotEmpty )
			{
				if ( string.IsNullOrWhiteSpace( costProfile.Name ) )
				{
					costProfile.Name = "Cost Profile";
					//messages.Add( string.Format( "Row: {0}. Missing Cost Profile Name. If any cost data is included, a Cost Name must also be included. ", rowNbr ) );
				}
				if ( string.IsNullOrWhiteSpace( costProfile.Description ) )
				{
					messages.Add( string.Format( "Row: {0}. Missing Cost Description. If any cost data is included, a Cost Description must also be included. ", rowNbr ) );
				}
				else if ( costProfile.Description == DELETE_ME )
				{
					messages.Add( string.Format( "Row: {0}. Invalid use of #DELETE with Cost Description. Cost Description is required when any cost information is provided, it can not be deleted. ", rowNbr ) );
				}
				if ( string.IsNullOrWhiteSpace( costProfile.DetailsUrl ) )
				{
					messages.Add( string.Format( "Row: {0}. Missing Cost Details Url. If any cost data is included, a Cost Details Url must also be included. ", rowNbr ) );
				}
				else if ( costProfile.DetailsUrl == DELETE_ME )
				{
					messages.Add( string.Format( "Row: {0}. Invalid use of #DELETE with Cost Details Url. Cost Details Url is required when any cost information is provided, it can not be deleted. ", rowNbr ) );
				}

				if ( isExistingEntity )
				{
					//check if has costs
					//bool hasCosts = false;
					//List<CostProfile> costs = CostProfileManager.GetAll( parentEntityUid, false );
					//if ( costs != null && costs.Count > 0 )
					//	hasCosts = true;

					if ( string.IsNullOrWhiteSpace( identifier )
						&& string.IsNullOrWhiteSpace( externalIdentifier )
						&& existingCostsCount > 0 )
					{
						messages.Add( string.Format( "Row: {0}. In order for the system to update a cost profile, a cost identifier (either internal or external) must be entered with a existing credential: {1}. It is recommended to always include an external identifier for a cost profile.", rowNbr, entity.Name ) );
					}
					else
					{
						costProfile.ExternalIdentifier = externalIdentifier;

						if ( !string.IsNullOrWhiteSpace( identifier ) && ServiceHelper.IsValidGuid( identifier ) )
						{
							//why doing this again. If we get here, identifier was valid, and existingProfile exists

							//costProfile.Identifier = new Guid( identifier );
							//existingProfile = CostProfileManager.GetBasicProfile( costProfile.Identifier );
							//if ( existingProfile == null || existingProfile.Id == 0 )
							//{
							//	messages.Add( string.Format( "Row: {0}. In order for the system to update a cost profile, a cost identifier (either internal or external) must be entered. The entered internal identifier is not valid: {1} for credential: {2}", rowNbr, identifier, entity.Name ) );
							//}
							//else
							//{
							//	//what to do with the existing profile? 
							//	costProfile.ExistingCostProfile = existingProfile;
							//}
						}
						else
						{
							// at this point we either have:
							//  no externalIdentifier with no costs or 
							//  an externalIdentifier with costs
							//  an externalIdentifier with no costs
							//Also, if we had an identifier, would not get here, so use existingProfileByExternalId
							if ( string.IsNullOrWhiteSpace( externalIdentifier ) && existingCostsCount == 0 )
							{
								//OK - no externalIdentifier with no costs or 
							}
							else if ( !string.IsNullOrWhiteSpace( externalIdentifier ) && existingCostsCount == 0 )
							{
								//OK - an externalIdentifier with no costs
							}
							else
							{
								//  a externalIdentifier with costs

								//also have to consider adding new cost to existing credential!
								//may defer checking - if only one, then attempt to allow
								//or maybe we should append these to the previous record. It will save handling appends in the update (just handle lists instead)
								string status = "";
								//we already did this - existingProfileByExternalId
								//Guid targetRowId = Factories.Import.ImportHelpers.ExternalIdentifierXref_Get( parentEntity.EntityTypeId, parentEntity.EntityBaseId, CodesManager.ENTITY_TYPE_COST_PROFILE, externalIdentifier, ref status );
								//if ( !ServiceHelper.IsValidGuid( targetRowId ) )
								//{
								//	//will not be found for first time, but should be there
								//	if ( existingCostsCount > 0 )
								//	{
								//		//then an issue - or could interpret as an add!
								//		//messages.Add( string.Format( "Row: {0}. CostProfile. An external identifier ({1}) was provided that is not yet associated with an existing cost profile. There are existing cost profiles, so the system cannot determine if this cost is a new one, or meant to be an update.  Re: credential: {2}", rowNbr, externalIdentifier, entity.Name ) );
								//	}
								//}
								//else
								//{
								//	//costProfile.Identifier = targetRowId;
								//	//existingProfile = CostProfileManager.GetBasicProfile( targetRowId );
								//	//if ( existingProfile == null || existingProfile.Id == 0 )
								//	//{
								//	//	messages.Add( string.Format( "Row: {0}. In order for the system to update a cost profile, a cost identifier (either internal or external) must be entered. The entered external identifier is not valid: {1}  for credential: {2}", rowNbr, externalIdentifier, entity.Name ) );
								//	//}
								//	//else
								//	//{
								//	//	costProfile.ExistingCostProfile = existingProfile;
								//	//}
								//}
							}
						}
					}
				} //
				//retain this for now
				entity.CostProfile = costProfile;
				//check have data to add
				if ( costProfile.IsNotEmpty )
				{
					//may need to remove from list and re-add
					entity.CostProfiles.Add( costProfile );
				}
			}
			
		}//
		public void AssignCommonConditions( int rowNbr, CsvReader csv, BaseDTO entity, AppUser user, CommonImportRequest importHelper, ref List<string> messages )
		{

			int msgcnt = messages.Count;
			//perhaps for the first time, get all manifests for the org - should only have one org
			//don't want to call repetively if there are none - perhaps should do these on a chg of org?
			//See AssignOrgStuff

			//if present, will be ctid for existing asmt
			string identifiers = Assign( rowNbr, csv, importHelper.CommonConditionsHdr, "CommonConditions", ref messages, "", false );
			if ( string.IsNullOrWhiteSpace( identifiers ) )
				return;
			else if ( identifiers.Trim() == DELETE_ME )
			{
				entity.DeleteCommonConditions = true;
				return;
			}

			//could have some variance with each cred, 
			if ( prevCommonConditionsIdentifiers == identifiers )
			{
				//no check necessary - unless previous had errors?
				entity.CommonConditionsIdentifiers = prevCommonConditions;

			}
			else
			{
				prevCommonConditions = new List<int>();
				prevCommonConditionsIdentifiers = identifiers;
				//loop thru ids and assign
				string[] parts = identifiers.Split( '|' );
				//18-04-05 - initially will expect integers representing Id for entity.commonCondition
				int recordId = 0;
				foreach ( var item in parts )
				{
					if ( !Int32.TryParse( item.Trim(), out recordId ) )
					{
						messages.Add( string.Format( "Row: {0}. Common Condition identifier must be an integer: {1}", rowNbr, item.Trim() ) );
						continue;
					}
					//get record from org manifest.
					var cm = orgConditionManifests.FirstOrDefault( s => s.Id == recordId );
					if ( cm == null || cm.Id == 0 )
					{
						messages.Add( string.Format( "Row: {0}. The Common Condition identifier is not valid for the owning organization: {1}", rowNbr, recordId ) );
						continue;
					}
					prevCommonConditions.Add( recordId );
				}
			}
		}//
		public void AssignCommonCosts( int rowNbr, CsvReader csv, BaseDTO entity, AppUser user, CommonImportRequest importHelper, ref List<string> messages )
		{

			int msgcnt = messages.Count;
			//perhaps for the first time, get all manifests for the org - should only have one org
			//don't want to call repetively if there are none - perhaps should do these on a chg of org?
			//See AssignOrgStuff
			//if ( orgCostManifests.Count == 0 )
			//{
			//    orgCostManifests = CostManifestManager.GetAll( entity.OwningAgentUid, false );
			//}

			//if present, will be ctid for existing manifest
			string identifiers = Assign( rowNbr, csv, importHelper.CommonCostsHdr, "CommonCosts", ref messages, "", false );
			if ( string.IsNullOrWhiteSpace( identifiers ) )
				return;
			else if ( identifiers.Trim() == DELETE_ME )
			{
				entity.DeleteCommonCosts = true;
				return;
			}

			//could have some variance with each cred, 
			if ( prevCommonCostIdentifiers == identifiers )
			{
				//no check necessary - unless previous had errors?
				entity.CommonCostsIdentifiers = prevCommonCosts;

			}
			else
			{
				prevCommonCosts = new List<int>();
				prevCommonCostIdentifiers = identifiers;
				//loop thru ids and assign
				string[] parts = identifiers.Split( '|' );
				//18-04-05 - initially will expect integers representing Id for entity.commonCost
				int recordId = 0;
				foreach ( var item in parts )
				{
					if ( !Int32.TryParse( item.Trim(), out recordId ) )
					{
						messages.Add( string.Format( "Row: {0}. Common Cost identifier must be an integer: {1}", rowNbr, item.Trim() ) );
						continue;
					}
					//get record from org manifest.
					var cm = orgCostManifests.FirstOrDefault( s => s.Id == recordId );
					if ( cm == null || cm.Id == 0 )
					{
						messages.Add( string.Format( "Row: {0}. The Common Cost identifier is not valid for the owning organization: {1}", rowNbr, recordId ) );
						continue;
					}
					prevCommonCosts.Add( recordId );
				}
			}
		}//

		public void AssignApprovedByList( int rowNbr, CsvReader csv, int listHdrId, BaseDTO entity, AppUser user, ref List<string> messages )
		{
			int msgcnt = messages.Count;
			Organization approvedByOrg = new Organization();

			if ( listHdrId > -1 )
			{
				string orglist = Assign( rowNbr, csv, listHdrId, "ApprovedBy Organization List", ref messages, "", false );
				if ( string.IsNullOrWhiteSpace( orglist ) )
					return;
				if ( prevApprovedByList == orglist )
				{
					entity.ApprovedByList = approvedByList;
				}
				else if ( orglist == DELETE_ME )
				{
					entity.DeleteApprovedBy = true;
				}
				else
				{
					if ( string.IsNullOrWhiteSpace( orglist ) )
						return;
					string[] list = orglist.Split( '|' );
					if ( list.Count() > 0 )
					{
						int cntr = 0;
						bool errorsFound = false;
						foreach ( var item in list )
						{
							cntr++;
							string ctid = "";
							if ( string.IsNullOrWhiteSpace( item.Trim() ) )
								continue;
							//TODO: actually should check for a ctid???
							if ( ServiceHelper.IsValidCtid( item, ref messages, false ) )
							{
								ctid = item.ToLower().Trim();
								//TODO - generalize handling
								if ( ctid == entity.OwningOrganizationCtid )
								{
									//error, can't approve your own data
									messages.Add( string.Format( "Row: {0} Approved By Entry Number: {1} An organization cannot 'Approve' its own artifacts. Entry: {2}", rowNbr, cntr, ctid ) );
									continue;
									//entity.ApprovedByList.Add(owningOrg);
								}
								else
								{
									//check existing
									approvedByOrg = prevRefOrgs.FirstOrDefault( s => s.CTID != null && s.CTID.ToLower() == ctid.ToLower() );
									if ( approvedByOrg == null || approvedByOrg.Id == 0 )
									{
										approvedByOrg = OrganizationManager.GetByCtid( ctid );
										if ( approvedByOrg == null || approvedByOrg.Id == 0 )
										{
											messages.Add( string.Format( "Row: {0} Approved By Entry Number: {1} An organization was not found with the entered CTID. Entry: {2}", rowNbr, cntr, ctid ) );
											continue;
										}
									}
									entity.ApprovedByList.Add( approvedByOrg );
									prevRefOrgs.Add( approvedByOrg );
									//OK, a cheat
									prevRefOrgPairs.Add( ctid );
								}
							}
							else
							{
								string[] parts = item.Split( '~' );
								//for now expecting just name and swp
								if ( parts.Count() != 2 )
								{
									messages.Add( string.Format( "Row: {0} Approved By Entry Number: {1} Approved By list must contain an organization name and organization webpage OR CTID of an organization from the publisher. Entry: {2}", rowNbr, cntr, item ) );
									continue;
								}

								if ( HandleOrganizationReference( rowNbr, cntr, "ApprovedBy ", item.Trim(), entity, user, ref messages, ref approvedByOrg ) )
								{
									if ( approvedByOrg.Id == entity.OrganizationId )
									{
										//error, can't approve your own data
										messages.Add( string.Format( "Row: {0} Approved By Entry Number: {1} An organization cannot 'Approve' its own artifacts. Entry: {2}", rowNbr, cntr, item ) );
										continue;
										//entity.ApprovedByList.Add(owningOrg);
									}
									else
									{
										entity.ApprovedByList.Add( approvedByOrg );
									}

								}
								else
									errorsFound = true;
							}

						} //foreach
						  //don't want to do this if had an error
						if ( !errorsFound )
						{
							prevApprovedByList = orglist;
							approvedByList = entity.ApprovedByList;
						}
						else
							prevApprovedByList = "";
					}
				}
			}

		}//

		/// <summary>
		/// todo - Generalize
		/// </summary>
		/// <param name="rowNbr"></param>
		/// <param name="csv"></param>
		/// <param name="entity"></param>
		/// <param name="user"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public void AssignAccreditedByList( int rowNbr, CsvReader csv, int listHdrId, BaseDTO entity, AppUser user, ref List<string> messages )
		{
			int msgcnt = messages.Count;
			Organization accreditedByOrg = new Organization();
			if ( listHdrId > -1 )
			{
				string list = Assign( rowNbr, csv, listHdrId, "AccreditedBy Organization List", ref messages, "", false );
				if ( string.IsNullOrWhiteSpace( list ) )
					return;

				if ( prevAccreditedByList == list )
				{
					entity.AccreditedByList = accreditedByList;
				}
				else if ( list == DELETE_ME )
				{
					entity.DeleteAccreditedBy = true;
				}
				else
				{
					if ( string.IsNullOrWhiteSpace( list ) )
						return;
					string[] array = list.Split( '|' );
					if ( array.Count() > 0 )
					{
						int cntr = 0;
						bool errorsFound = false;
						foreach ( var item in array )
						{
							cntr++;
							string ctid = "";
							if ( string.IsNullOrWhiteSpace( item.Trim() ) )
								continue;
							//TODO: actually should check for a ctid???
							if ( ServiceHelper.IsValidCtid( item, ref messages, false ) )
							{
								ctid = item.ToLower().Trim();
								//TODO - generalize handling
								if ( ctid == entity.OwningOrganizationCtid )
								{
									//entity.AccreditedByList.Add(owningOrg);
									//error, can't approve your own data
									messages.Add( string.Format( "Row: {0} Approved By Entry Number: {1} An organization cannot 'Accredit' its own artifacts. Entry: {2}", rowNbr, cntr, ctid ) );
									continue;
								}
								else
								{
									//check existing
									accreditedByOrg = prevRefOrgs.FirstOrDefault( s => s.CTID != null && s.CTID.ToLower() == ctid.ToLower() );
									if ( accreditedByOrg == null || accreditedByOrg.Id == 0 )
									{
										accreditedByOrg = OrganizationManager.GetByCtid( ctid );
										if ( accreditedByOrg == null || accreditedByOrg.Id == 0 )
										{
											messages.Add( string.Format( "Row: {0} Accredited By Entry Number: {1} An organization was not found with the entered CTID. Entry: {2}", rowNbr, cntr, ctid ) );
											continue;
										}
									}
									entity.AccreditedByList.Add( accreditedByOrg );
									prevRefOrgs.Add( accreditedByOrg );
									//OK, a cheat
									prevRefOrgPairs.Add( ctid );
								}
							}
							else
							{
								string[] parts = item.Split( '~' );
								//for now expecting just name and swp
								if ( parts.Count() != 2 )
								{
									messages.Add( string.Format( "Row: {0} Accredited By Entry Number: {1} Accredited By list must contain an organization name and organization webpage OR CTID of an organization from the publisher. Entry: {2}", rowNbr, cntr, item ) );
									continue;
								}

								if ( HandleOrganizationReference( rowNbr, cntr, "AccreditedBy ", item.Trim(), entity, user, ref messages, ref accreditedByOrg ) )
								{
									if ( accreditedByOrg.Id == entity.OrganizationId )
									{
										messages.Add( string.Format( "Row: {0} Accredited By Entry Number: {1} An organization cannot 'Accredit' its own artifacts. Entry: {2}", rowNbr, cntr, item ) );
										continue;
									}
									else
									{
										entity.AccreditedByList.Add( accreditedByOrg );
									}
								}
								else
									errorsFound = true;
							}

						} //foreach
						  //don't want to do this if had an error
						if ( !errorsFound )
						{
							prevAccreditedByList = list;
							accreditedByList = entity.AccreditedByList;
						}
						else
							prevAccreditedByList = "";
					}
				}
			}

		}//
        //connection target and condition target for credential
        public void AssignTargetCredential( int rowNbr, CsvReader csv, int listHdrId, ConditionProfileDTO entity, AppUser user, ref List<string> messages /*, int entityTypeId*/)
        {
            int msgcnt = messages.Count;
            Credential target = new Credential();
			
            if ( listHdrId == -1 )
            {
                return;
            }
            else
            {
                string targets = Assign( rowNbr, csv, listHdrId, "Target Credential List" /*string.Format( "target {0}", colname ),*/ , ref messages, "", false );
                if ( string.IsNullOrWhiteSpace( targets ) )
                    return;
                if ( prevTargetCredentialList == targets )
                {
                    entity.TargetCredentialList = targetCredentialList;
                }
                else if ( targets == DELETE_ME )
                {
                    entity.DeleteTargetCredentials = true;
                }
                else
                {
                    if ( string.IsNullOrWhiteSpace( targets ) )
                        return;
                    string[] list = targets.Split( '|' );
                    if ( list.Count() > 0 )
                    {
                        int cntr = 0;
						int intId = 0;
						bool errorsFound = false;
                        foreach ( var item in list )
                        {
                            cntr++;
                            if ( string.IsNullOrWhiteSpace( item.Trim() ) )
                                continue;
                            var ctid = item.Trim();
                            //TODO: actually should check for a ctid???
                            if ( ServiceHelper.IsValidCtid( ctid, ref messages, false ) )
                            {
                                    //check existing
                                    target = prevRefCreds.FirstOrDefault( s => s.CTID != null && s.CTID.ToLower() == ctid.ToLower() );
                                    if ( target == null || target.Id == 0 )
                                    {
                                        target = CredentialManager.GetByCtid( ctid );
                                        if ( target == null || target.Id == 0 )
                                        {
                                            messages.Add( string.Format( "Row: {0} Target Credential: {1} A credential was not found with the entered CTID. Value: {2}", rowNbr, cntr, ctid ) );
                                            continue;
                                        }
                                    }
                                    entity.TargetCredentialList.Add( target );
                                    prevRefCreds.Add( target );
                                //OK, a cheat
                                prevRefCredPairs.Add( item );

                            }
                            else if ( ServiceHelper.IsInteger( ctid) &&  int.TryParse( ctid, out intId ) )
							{

								target = CredentialManager.GetBasic( intId );
								if ( target == null || target.Id == 0 )
								{
									messages.Add( string.Format( "Row: {0} Target Credential#: {1} A credential was not found with the entered identifier. Value: {2}", rowNbr, cntr, ctid ) );
									continue;
								}
								entity.TargetCredentialList.Add( target );
								prevRefCreds.Add( target );
								//OK, a cheat
								prevRefCredPairs.Add( item );
							}
							else
							{
                                string[] parts = item.Split( '~' );
                                //for now expecting just name, type and swp
                                if ( parts.Count() != 3 )
                                {
                                    messages.Add( string.Format( "Row: {0} Target Credential#: {1} Target By list must contain an credential name , credential type and credential webpage OR CTID of an credential. Value: {2}", rowNbr, cntr, item ) );
                                    continue;
                                }

                                if ( HandleCredentialReference( rowNbr, cntr, "TargetBy ", item.Trim(),  user, ref messages, ref target ) )
                                {
                                    entity.TargetCredentialList.Add( target );
                                }
                                else
                                    errorsFound = true;
                            }

                        } //foreach
                          //don't want to do this if had an error
						  //the targets can be a mix of ctids, integers, and references
                        if ( !errorsFound )
                        {
                            prevTargetCredentialList = targets;
                            targetCredentialList = entity.TargetCredentialList;
                        }
                        else
                            prevTargetCredentialList = "";
                    }
                }
            }

        }
        // connection target and condition target for assessment
        public void AssignTargetAssessment( int rowNbr, CsvReader csv, int listHdrId, ConditionProfileDTO entity, AppUser user, ref List<string> messages)
        {
            int msgcnt = messages.Count;
           AssessmentProfile target = new AssessmentProfile();

            if ( listHdrId == -1 )
            {
                return;
            }
            else
            {
                string targets = Assign( rowNbr, csv, listHdrId, "Target Assessment List", ref messages, "", false );
                if ( string.IsNullOrWhiteSpace( targets ) )
                    return;
                if ( prevTargetAssessmentList == targets )
                {
                    entity.TargetAssessmentList = targetAssessmentList;
                }
                else if ( targets == DELETE_ME )
                {
                    entity.DeleteTargetAssessments = true;
                }
                else
                {
                    if ( string.IsNullOrWhiteSpace( targets ) )
                        return;
                    string[] list = targets.Split( '|' );
                    if ( list.Count() > 0 )
                    {
                        int cntr = 0;
						int intId = 0;
                        bool errorsFound = false;
                        foreach ( var item in list )
                        {
                            cntr++;
                            if ( string.IsNullOrWhiteSpace( item.Trim() ) )
                                continue;
                            var ctid = item.Trim();
                            //TODO: actually should check for a ctid???
                            if ( ServiceHelper.IsValidCtid( ctid, ref messages, false ) )
                            {
                                //check existing
                                target = prevRefAssmts.FirstOrDefault( s => s.CTID != null && s.CTID.ToLower() == ctid.ToLower() );
                                if ( target == null || target.Id == 0 )
                                {

                                    target = AssessmentManager.GetByCtid( ctid );
                                    if ( target == null || target.Id == 0 )
                                    {
                                        messages.Add( string.Format( "Row: {0} Target Assessment: {1} An assessment was not found with the entered CTID. Entry: {2}", rowNbr, cntr, ctid ) );
                                        continue;
                                    }
                                }
                                entity.TargetAssessmentList.Add( target );
                                prevRefAssmts.Add( target );
                                //OK, a cheat
                                prevRefAssmtPairs.Add( item );
                            }
							else if ( ServiceHelper.IsInteger( ctid ) && int.TryParse( ctid, out intId ) )
							{

								target = AssessmentManager.GetBasic( intId );
								if ( target == null || target.Id == 0 )
								{
									messages.Add( string.Format( "Row: {0} Target Assessment#: {1} An assessment was not found with the entered identifier. Value: {2}", rowNbr, cntr, ctid ) );
									continue;
								}
								entity.TargetAssessmentList.Add( target );
								prevRefAssmts.Add( target );
								//OK, a cheat
								prevRefAssmtPairs.Add( item );
							}
							else
                            {
                                string[] parts = item.Split( '~' );
                                //for now expecting just name, type and swp
                                if ( parts.Count() != 2 )
                                {
                                    messages.Add( string.Format( "Row: {0} Target Assessment: {1} Target By list must contain an assessment name and assessment webpage OR CTID of an assessment. Value: {2}", rowNbr, cntr, item ) );
                                    continue;
                                }

                                if ( HandleAssessmentReference( rowNbr, cntr, "TargetBy ", item.Trim(), user, ref messages, ref target ) )
                                {
                                    entity.TargetAssessmentList.Add( target );
                                }
                                else
                                    errorsFound = true;
                            }

                        } //foreach
                          //don't want to do this if had an error
                        if ( !errorsFound )
                        {
                            prevTargetAssessmentList = targets;
                            targetAssessmentList = entity.TargetAssessmentList;
                        }
                        else
                            prevTargetAssessmentList = "";
                    }
                }
            }

        }
        //connection target and condition target for learningOpportunity
        public void AssignTargetLearningOpp( int rowNbr, CsvReader csv, int listHdrId, ConditionProfileDTO entity, AppUser user, ref List<string> messages )
        {
            int msgcnt = messages.Count;
            LearningOpportunityProfile target = new LearningOpportunityProfile();

            if ( listHdrId == -1 )
            {
                return;
            }
            else
            {
                string targets = Assign( rowNbr, csv, listHdrId, "Target LearningOpportunity List", ref messages, "", false );
                if ( string.IsNullOrWhiteSpace( targets ) )
                    return;
                if ( prevTargetLearningOppList == targets )
                {
                    entity.TargetLearningOpportunityList = targetLearningOppList;
                }
                else if ( targets == DELETE_ME )
                {
                    entity.DeleteTargetLearningOpportunities = true;
                }
                else
                {
                    if ( string.IsNullOrWhiteSpace( targets ) )
                        return;
                    string[] list = targets.Split( '|' );
                    if ( list.Count() > 0 )
                    {
                        int cntr = 0;
						int intId = 0;
                        bool errorsFound = false;
                        foreach ( var item in list )
                        {
                            cntr++;
                            if ( string.IsNullOrWhiteSpace( item.Trim() ) )
                                continue;
                            var ctid = item.Trim();
                            //TODO: actually should check for a ctid???
                            if ( ServiceHelper.IsValidCtid( ctid, ref messages, false ) )
                            {
                                //check existing
                                target = prevRefLopps.FirstOrDefault( s => s.CTID != null && s.CTID.ToLower() == ctid.ToLower() );
                                if ( target == null || target.Id == 0 )
                                {

                                    target = LearningOpportunityManager.GetByCtid( ctid );
                                    if ( target == null || target.Id == 0 )
                                    {
                                        messages.Add( string.Format( "Row: {0} Target LearningOpportunity: {1} An learningOpportunity was not found with the entered CTID. Entry: {2}", rowNbr, cntr, ctid ) );
                                        continue;
                                    }
                                }
                                entity.TargetLearningOpportunityList.Add( target );
                                prevRefLopps.Add( target );
                                //OK, a cheat
                                prevRefLoppPairs.Add( item );


                            }
							else if ( ServiceHelper.IsInteger( ctid ) && int.TryParse( ctid, out intId ) )
							{

								target = LearningOpportunityManager.GetBasic( intId );
								if ( target == null || target.Id == 0 )
								{
									messages.Add( string.Format( "Row: {0} Target LearningOpportunity#: {1} A Learning Opportunity was not found with the entered identifier. Value: {2}", rowNbr, cntr, ctid ) );
									continue;
								}
								entity.TargetLearningOpportunityList.Add( target );
								prevRefLopps.Add( target );
								//OK, a cheat
								prevRefLoppPairs.Add( item );
							}
							else
                            {
                                string[] parts = item.Split( '~' );
                                //for now expecting just name, type and swp
                                if ( parts.Count() != 2 )
                                {
                                    messages.Add( string.Format( "Row: {0} Target LearningOpportunity: {1} Target By list must contain an learningOpp name and learningOpp webpage OR CTID of an learningOpp. Entry: {2}", rowNbr, cntr, item ) );
                                    continue;
                                }

                                if ( HandleLearningOppReference( rowNbr, cntr, "TargetBy ", item.Trim(), user, ref messages, ref target ) )
                                {
                                    entity.TargetLearningOpportunityList.Add( target );
                                }
                                else
                                    errorsFound = true;
                            }

                        } //foreach
                          //don't want to do this if had an error
                        if ( !errorsFound )
                        {
                            prevTargetLearningOppList = targets;
                            targetLearningOppList = entity.TargetLearningOpportunityList;
                        }
                        else
                            prevTargetLearningOppList = "";
                    }
                }
            }

        }
        public void AssignOfferedByList( int rowNbr, CsvReader csv, int listHdrId, BaseDTO entity, AppUser user, ref List<string> messages )
		{
			int msgcnt = messages.Count;
			Organization offeredByOrg = new Organization();

			if ( listHdrId == -1 )
			{
				//TBD - decide if to add owning org by default if not provided
				entity.OfferedByList.Add( defaultOwningOrg );

			}
			else
			{
				string orglist = Assign( rowNbr, csv, listHdrId, "OfferedBy Organization List", ref messages, "", false );
				if ( string.IsNullOrWhiteSpace( orglist ) )
					return;
				if ( prevOfferedByList == orglist )
				{
					entity.OfferedByList = offeredByList;
				}
				else if ( orglist == DELETE_ME )
				{
					entity.DeleteOfferedBy = true;
				}
				else if ( orglist.ToLower() == SAME_AS_OWNER )
				{
					entity.OfferedByList.Add( defaultOwningOrg );
				}
				else
				{
					if ( string.IsNullOrWhiteSpace( orglist ) )
						return;
					string[] list = orglist.Split( '|' );
					if ( list.Count() > 0 )
					{
						int cntr = 0;
						bool errorsFound = false;
						foreach ( var item in list )
						{
							cntr++;
							string ctid = "";
							if ( string.IsNullOrWhiteSpace( item.Trim() ) )
								continue;
							
							if ( ServiceHelper.IsValidCtid( item, ref messages, false ) )
							{
								ctid = item.ToLower().Trim();
								//TODO - generalize handling
								if ( ctid == entity.OwningOrganizationCtid )
								{
									entity.OfferedByList.Add( defaultOwningOrg );
								}
								else
								{
									//check existing
									offeredByOrg = prevRefOrgs.FirstOrDefault( s => s.CTID != null && s.CTID.ToLower() == ctid.ToLower() );
									if ( offeredByOrg == null || offeredByOrg.Id == 0 )
									{
										offeredByOrg = OrganizationManager.GetByCtid( ctid );
										if ( offeredByOrg == null || offeredByOrg.Id == 0 )
										{
											messages.Add( string.Format( "Row: {0} Offered By Entry Number: {1} An organization was not found with the entered CTID. Entry: {2}", rowNbr, cntr, ctid ) );
											continue;
										}
									}
									entity.OfferedByList.Add( offeredByOrg );
									prevRefOrgs.Add( offeredByOrg );
									//OK, a cheat
									prevRefOrgPairs.Add( ctid );
								}
							}
							else if ( item.ToLower() == SAME_AS_OWNER )
							{
								entity.OfferedByList.Add( defaultOwningOrg );
							}
							else
							{
								string[] parts = item.Split( '~' );
								//for now expecting just name and swp
								if ( parts.Count() != 2 )
								{
									messages.Add( string.Format( "Row: {0} Offered By Entry Number: {1} Offered By list must contain an organization name and organization webpage OR CTID of an organization from the publisher. Entry: {2}", rowNbr, cntr, item ) );
									continue;
								}

								if ( HandleOrganizationReference( rowNbr, cntr, "OfferedBy ", item.Trim(), entity, user, ref messages, ref offeredByOrg ) )
								{
									entity.OfferedByList.Add( offeredByOrg );
								}
								else
									errorsFound = true;
							}

						} //foreach
						  //don't want to do this if had an error
						if ( !errorsFound )
						{
							prevOfferedByList = orglist;
							offeredByList = entity.OfferedByList;
						}
						else
							prevOfferedByList = "";
					}
				}
			}

		}//
		public void AssignRecognizedByList( int rowNbr, CsvReader csv, int listHdrId, BaseDTO entity, AppUser user, ref List<string> messages )
		{
			int msgcnt = messages.Count;

			if ( listHdrId > -1 )
			{
				string orglist = Assign( rowNbr, csv, listHdrId, "RecognizedBy Organization List", ref messages, "", false );
				if ( string.IsNullOrWhiteSpace( orglist ) )
					return;
				if ( prevRecognizedByList == orglist )
				{
					entity.RecognizedByList = recognizedByList;
				}
				else if ( orglist == DELETE_ME )
				{
					entity.DeleteRecognizedBy = true;
				}
				else
				{
					if ( string.IsNullOrWhiteSpace( orglist ) )
						return;
					string[] list = orglist.Split( '|' );
					if ( list.Count() > 0 )
					{
						int cntr = 0;
						bool errorsFound = false;
						foreach ( var item in list )
						{
							cntr++;
							string ctid = "";
							if ( string.IsNullOrWhiteSpace( item.Trim() ) )
								continue;
							if ( ServiceHelper.IsValidCtid( item, ref messages, false ) )
							{
								ctid = item.ToLower().Trim();
								//TODO - generalize handling
								if ( ctid == entity.OwningOrganizationCtid )
								{
									//entity.RecognizedByList.Add(owningOrg);
									//error, can't approve your own data
									messages.Add( string.Format( "Row: {0} Recognized By Entry Number: {1} An organization cannot 'Recognize' its own credentials. Entry: {2}", rowNbr, cntr, ctid ) );
									continue;
								}
								else
								{
									//check existing
									recognizedByOrg = prevRefOrgs.FirstOrDefault( s => s.CTID != null && s.CTID.ToLower() == ctid.ToLower() );
									if ( recognizedByOrg == null || recognizedByOrg.Id == 0 )
									{
										recognizedByOrg = OrganizationManager.GetByCtid( ctid );
										if ( recognizedByOrg == null || recognizedByOrg.Id == 0 )
										{
											messages.Add( string.Format( "Row: {0} Recognized By Entry Number: {1} An organization was not found with the entered CTID. Entry: {2}", rowNbr, cntr, ctid ) );
											continue;
										}
									}
									entity.RecognizedByList.Add( recognizedByOrg );
									prevRefOrgs.Add( recognizedByOrg );
									//OK, a cheat
									prevRefOrgPairs.Add( ctid );
								}
							}
							else
							{
								string[] parts = item.Split( '~' );
								//for now expecting just name and swp
								if ( parts.Count() != 2 )
								{
									messages.Add( string.Format( "Row: {0} Regulated By Entry Number: {1} Offered By list must contain an organization name and organization webpage OR CTID of an organization from the publisher. Entry: {2}", rowNbr, cntr, item ) );
									continue;
								}

								if ( HandleOrganizationReference( rowNbr, cntr, "RecognizedBy ", item.Trim(), entity, user, ref messages, ref recognizedByOrg ) )
								{

									if ( recognizedByOrg.Id == entity.OrganizationId )
									{
										messages.Add( string.Format( "Row: {0} Recognized By Entry Number: {1} An organization cannot 'Recognize' its own artifacts. Entry: {2}", rowNbr, cntr, item ) );
										continue;
									}
									else
									{
										entity.RecognizedByList.Add( recognizedByOrg );
									}
								}
								else
									errorsFound = true;
							}

						} //foreach
						  //don't want to do this if had an error
						if ( !errorsFound )
						{
							prevRecognizedByList = orglist;
							recognizedByList = entity.RecognizedByList;
						}
						else
							prevRecognizedByList = "";
					}
				}
			}

		}//

		public void AssignRegulatedByList( int rowNbr, CsvReader csv, int listHdrId, BaseDTO entity, AppUser user, ref List<string> messages )
		{
			int msgcnt = messages.Count;

			if ( listHdrId > -1 )
			{
				string orglist = Assign( rowNbr, csv, listHdrId, "RegulatedBy Organization List", ref messages, "", false );
				if ( string.IsNullOrWhiteSpace( orglist ) )
					return;
				if ( prevRegulatedByList == orglist )
				{
					entity.RegulatedByList = regulatedByList;
				}
				else if ( orglist == DELETE_ME )
				{
					entity.DeleteRegulatedBy = true;
				}
				else
				{
					if ( string.IsNullOrWhiteSpace( orglist ) )
						return;
					string[] list = orglist.Split( '|' );
					if ( list.Count() > 0 )
					{
						int cntr = 0;
						bool errorsFound = false;
						foreach ( var item in list )
						{
							cntr++;
							string ctid = "";
							if ( string.IsNullOrWhiteSpace( item.Trim() ) )
								continue;
							if ( ServiceHelper.IsValidCtid( item, ref messages, false ) )
							{
								ctid = item.ToLower().Trim();
								//TODO - generalize handling
								if ( ctid == entity.OwningOrganizationCtid )
								{
									//entity.RegulatedByList.Add( owningOrg );
									//error, can't approve your own data
									messages.Add( string.Format( "Row: {0} Regulated By Entry Number: {1} An organization cannot 'Regulate' its own artifacts. Entry: {2}", rowNbr, cntr, ctid ) );
									continue;
								}
								else
								{
									//check existing
									regulatedByOrg = prevRefOrgs.FirstOrDefault( s => s.CTID != null && s.CTID.ToLower() == ctid.ToLower() );
									if ( regulatedByOrg == null || regulatedByOrg.Id == 0 )
									{
										regulatedByOrg = OrganizationManager.GetByCtid( ctid );
										if ( regulatedByOrg == null || regulatedByOrg.Id == 0 )
										{
											messages.Add( string.Format( "Row: {0} Regulated By Entry Number: {1} An organization was not found with the entered CTID. Entry: {2}", rowNbr, cntr, ctid ) );
											continue;
										}
									}
									entity.RegulatedByList.Add( regulatedByOrg );
									prevRefOrgs.Add( regulatedByOrg );
									//OK, a cheat
									prevRefOrgPairs.Add( ctid );
								}
							}
							else
							{
								string[] parts = item.Split( '~' );
								//for now expecting just name and swp
								if ( parts.Count() != 2 )
								{
									messages.Add( string.Format( "Row: {0} Regulated By Entry Number: {1} Offered By list must contain an organization name and organization webpage OR CTID of an organization from the publisher. Entry: {2}", rowNbr, cntr, item ) );
									continue;
								}

								if ( HandleOrganizationReference( rowNbr, cntr, "RegulatedBy ", item.Trim(), entity, user, ref messages, ref regulatedByOrg ) )
								{
									if ( regulatedByOrg.Id == entity.OrganizationId )
									{
										messages.Add( string.Format( "Row: {0} Regulated By Entry Number: {1} An organization cannot 'Regulate' its own artifacts. Entry: {2}", rowNbr, cntr, item ) );
										continue;
									}
									else
									{
										entity.RegulatedByList.Add( regulatedByOrg );
									}
								}
								else
									errorsFound = true;
							}

						} //foreach
						  //don't want to do this if had an error
						if ( !errorsFound )
						{
							prevRegulatedByList = orglist;
							regulatedByList = entity.RegulatedByList;
						}
						else
							prevRegulatedByList = "";
					}
				}
			}

		}//

		#endregion

		public List<string[]> ReadFileToList( string fileName )
		{
			var result = new List<string[]>();


			System.Text.Encoding utf = System.Text.Encoding.UTF7;

			using ( CsvReader csv =
				   new CsvReader( new StreamReader( fileName, utf ), true ) )
			{
				int fieldCount = csv.FieldCount;
				string[] headers = csv.GetFieldHeaders();
				string[] array = new string[ fieldCount ];
				result.Add( headers );
				csv.SkipEmptyLines = true;
				while ( csv.ReadNextRecord() )
				{

					csv.CopyCurrentRecordTo( array );
					result.Add( array );
				}
			}

			return result;
		}
	}
}
