using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Common;
using Models.ProfileModels;
using EM = Data;
using Utilities;
using DBEntity = Data.Import_Microsoft;

namespace Factories
{
	public class MicrosoftImport : BaseFactory
	{
		CredentialManager cmgr = new CredentialManager();
		AssessmentManager amgr = new AssessmentManager();
		Entity_AssessmentManager eaMgr = new Entity_AssessmentManager();
		Entity_ConditionProfileManager cpMgr = new Entity_ConditionProfileManager();
		int defaultUserId = 10;
		int english = 40;
		int managingOrgId = 1128;
		CodeItem credStatus = new CodeItem();
		CodeItem credCertificationTypeCode = new CodeItem();
        CodeItem credTypeCode = new CodeItem();

        CodeItem examCode = new CodeItem();
		CodeItem inPersonCode = new CodeItem();
		CodeItem onlineCode = new CodeItem();
		CodeItem blendedCode = new CodeItem();
		public void ImportMicrosoftCredentials( ref List<string> summary, int maxRecords = 0 )
		{
			if ( maxRecords == 0 )
				maxRecords = 100;
			int pTotalRows = 0;
			int credId = 0;
			int asmtId = 0;
			int condProfileId = 0;
			//replace with current user

			List<string> messages = new List<string>();
			//			List<string> summary = new List<string>();
			string statusMessage = "";

			Credential cred = new Credential();
			AssessmentProfile asmt = new AssessmentProfile();
			ConditionProfile cp = new ConditionProfile();
			string orgGuid = "8f1a526b-3e18-4a4e-829f-05b85197772c";
			Guid owningOrgUid = new Guid( orgGuid );
			Enumeration eProperty = new Enumeration();

			//get codes
			credStatus = CodesManager.GetPropertyBySchema( "ceterms:CredentialStatus", "credentialStat:Active" );
			credCertificationTypeCode = CodesManager.GetPropertyBySchema( "ceterms:credentialType", "ceterms:Certification" );

			//assessMethod(56) - exam
			int asmtMethodCatId = CodesManager.PROPERTY_CATEGORY_Assessment_Method_Type;
			examCode = CodesManager.GetPropertyBySchema( "ceterms:AssessmentMethod", "assessMethod:Exam" );
			//deliveryType (21) - in-person, online
			int deliveryTypeCatId = CodesManager.PROPERTY_CATEGORY_DELIVERY_TYPE;
			inPersonCode = CodesManager.GetPropertyBySchema( "ceterms:Delivery", "deliveryType:InPerson" );
			onlineCode = CodesManager.GetPropertyBySchema( "ceterms:Delivery", "deliveryType:OnlineOnly" );
			blendedCode = CodesManager.GetPropertyBySchema( "ceterms:Delivery", "deliveryType:BlendedDelivery" );
			//
			List<string> asmts = new List<string>();
			//use same Guid in both env
			//Organization org = OrganizationManager.get
			using ( var context = new Data.CTIEntities() )
			{
				//
				List<DBEntity> results = context.Import_Microsoft
						.Where( s => s.IsImported == false )
						.OrderBy( s => s.CertProgramCode )
						.ThenBy( s => s.CertSpecialtyId )
						.ThenBy( s => s.IsRequiredOrOptional ) //this is fine if only one combination of require, and alternate. If multiple groups, this would mess up the processing. 
						.ThenBy( s => s.ExamNumber )
						.ToList();

				if ( results != null && results.Count > 0 )
				{
					int prevCertId = 0;
					int icntr = 0;
					int acntr = 0;
					bool hasRequiredCP = false;
					//true if any alternate conditions have been created
					bool hasAlternateCondtions = false;

					bool skippingConditionTargets = false;
					bool credentialExists = false;
					bool asmtExists = false;
					foreach ( DBEntity item in results )
					{
						icntr++;

						statusMessage = "";
						messages = new List<string>();

						if ( item.CertSpecialtyId != prevCertId )
						{
							if ( prevCertId > 0 )
							{
								if ( asmts.Count > 1 )
								{
									UpdateConditionDescription( asmts, cp, cpMgr, cred.RowId );
								}
							}

							//note can't stop in middle of a multiple asmt, so do after a break (maxRecord won't be accurate)
							if ( maxRecords > 0 && icntr > maxRecords )
							{
								break;
							}

							//handle credential
							credentialExists = false;
							//create credential
							cred = new Credential();
							HandleCredential( item, ref cred, owningOrgUid, defaultUserId,
				statusMessage, ref credentialExists, ref summary );

							cp = new ConditionProfile();
							hasRequiredCP = false;
							skippingConditionTargets = false;
							hasAlternateCondtions = false;

							//create starting condition profile
							if ( credentialExists )
							{
								//for existing cred, not sure we can create a condition, plus risky adding to existing CP
								//may be useful to have a merge conditions process????
								//could try to get all and see if only one, and possibly if a required one
								Entity_ConditionProfileManager.FillConditionProfilesForList( cred, false );
								if ( cred.Requires == null || cred.Requires.Count == 0 )
								{
									AddConditionProfile( cred, cp, cpMgr, defaultUserId, item.ExamNameWithNumber, ref messages );
									summary.Add( string.Format( "Credential exists and doesn't have a requires condition profile, so created one. Condition Profile Name: {0}", cp.ProfileName ) );
								}
								else
								{
									//a requires cp exists, so either abandon all asmts or create asmts as orphans
									//cp = cred.Requires[ 0 ];
									summary.Add( string.Format( "Credential exists and has at least one Requires condition profile. To avoid issues and incorrect guesses, the system will not be adding assesments to any condition. <br>____These asmts will need to be manually added to the appropriate Condition Profile Name: {0}", cp.ProfileName ) );
									skippingConditionTargets = true;
								}
							}
							else
							{
								AddConditionProfile( cred, cp, cpMgr, defaultUserId, item.ExamNameWithNumber, ref messages );
							}

							//==========================================
							prevCertId = ( int ) item.CertSpecialtyId;

							asmts = new List<string>();
							acntr = 0;
						}
						acntr++;
						asmtExists = false;
						//create assessment
						asmt = new AssessmentProfile();
						HandleAssessment( item, ref asmt, cred, owningOrgUid, defaultUserId,
				statusMessage, ref asmtExists, ref asmts, ref summary );

						messages = new List<string>();
						if ( skippingConditionTargets )
						{
							summary.Add( string.Format( "___ Will need to add this Assessment to a condition profile. Name: {0}, Id: {1}", asmt.Name, asmt.Id ) );
							continue;
						}
						//add asmt under cred
						if ( eaMgr.Add( cred.RowId, asmt.Id, defaultUserId, true, ref messages ) == 0 )
							summary.AddRange( messages );

						//add target assesment
						//action depends on 
						if ( ( ( int ) item.IsRequiredOrOptional ) == 1 )
						{
							//required, add to cp
							if ( !hasRequiredCP )
							{
								//may need to check if top, could have a variety of rraaraa
								hasRequiredCP = true;
								if ( eaMgr.Add( cp.RowId, asmt.Id, defaultUserId, true, ref messages ) == 0 )
									summary.AddRange( messages );
							}
							else
							{
								//may need to use a read ahead and collect all the asmts for the current cred
								//if sorted, should be an indication of starting a new set (potential)
								//actual would check for a previous alternate. If found might suggest a new top condition profile
								if ( hasAlternateCondtions )
								{
									//may want to update previous cp description, and reset the asmts list
									//actually this will be too late, the current asmt probably shou
									UpdateConditionDescription( asmts, cp, cpMgr, cred.RowId );

									//create new cp
									cp = new ConditionProfile();
									AddConditionProfile( cred, cp, cpMgr, defaultUserId, item.ExamNameWithNumber, ref messages, true );
									summary.Add( string.Format( "___ Added an additional condition profile based on encountering a required asmt, after alternate conditions. Name: {0}, Id: {1}", asmt.Name, asmt.Id ) );
								}

								//add
								if ( eaMgr.Add( cp.RowId, asmt.Id, defaultUserId, true, ref messages ) == 0 )
									summary.AddRange( messages );
								//so reset alternates
								hasAlternateCondtions = false;
							}

						}
						else if ( ( ( int ) item.IsRequiredOrOptional ) == 2 )
						{
							//always add alternate, and target will be added to the latter
							hasAlternateCondtions = true;
							//one of. Need a top level CP, that references the alternates, track the asmt item nbr
							if ( acntr == 1 )
							{
								/*the top level cp already has been created. 
								 * If acntr = 1, then all are optional or until next required?
								 * Create the alternate cp, but mark up the top cp to indicate multiple alternate conditions
								 */
								AddAlterateConditionProfile( cred, cp, asmt, ref messages );
							}
							else if ( acntr == 2 )
							{
								/*the top level cp already has been created. 
								 * If acntr = 2, want to determine if had previous required conditions, and now this an alternate for the last required CP
								 */
								//not sure
								AddAlterateConditionProfile( cred, cp, asmt, ref messages );
							}
							else
							{
								AddAlterateConditionProfile( cred, cp, asmt, ref messages );
							}
							//eaMgr.Add( cp.RowId, asmt.Id, defaultUserId, true, ref messages );
						}
						else if ( ( ( int ) item.IsRequiredOrOptional ) == 3 )
						{
							//any 2 of - not sure?
							//may just handle with text, and all as required
							eaMgr.Add( cp.RowId, asmt.Id, defaultUserId, true, ref messages );
						}

						//set record as imported
						//may want a note regarding anything skipped?
						SetRecordAsImported( item.Id );
					} //foreach

					//handle last??
					if ( prevCertId > 0 )
					{
						//handle cp update with multiple asmts
						UpdateConditionDescription( asmts, cp, cpMgr, cred.RowId );
					}
				}
			}
			//return;
		}

		private int HandleCredential( DBEntity item, ref Credential cred, Guid owningOrgUid, int defaultUserId,
				string statusMessage, ref bool entityExists, ref List<string> summary )
		{
			int newId = 0;
			int ptotalRows = 0;
			//check if cred exists based on name and subject webpage
			if ( DoesCredentialExist( item, ref cred, ref statusMessage ) )
			{
				summary.Add( string.Format( "Credential already exists. Name: {0}, Id: {1}", item.CertificationName, cred.Id ) );
				entityExists = true;
				return cred.Id;
			}


			cred.Name = item.CertificationName;
			cred.Description = item.CertSpecialtyName + " certification";
			cred.SubjectWebpage = item.CredentialUrl;
			cred.OwningAgentUid = owningOrgUid;
			cred.ManagingOrgId = managingOrgId;
			//add owner role
			Enumeration eProperty = new Enumeration();
			eProperty.Id = 6;
			eProperty.Items.Add( new EnumeratedItem()
			{
				CodeId = 6,
				Id = 6
			} );
			cred.OwnerRoles = eProperty;

			cred.CreatedById = defaultUserId;
			//cred.InLanguageId = english;
			cred.CodedNotation = item.CertSpecialtyId.ToString();
			cred.AlternateName = item.CertSpecialtyName;
			cred.Keyword.Add( new TextValueProfile() { TextValue = item.CertProgramCode } );
			cred.Keyword.Add( new TextValueProfile() { TextValue = item.CertTrackCode } );
			cred.Keyword.Add( new TextValueProfile() { TextValue = item.CertTrackName } );


			eProperty = new Enumeration();
			//cred type of certification
			eProperty.Id = 2;
			eProperty.Items.Add( new EnumeratedItem()
			{
				CodeId = 14,
				Id = 14
			} );
			cred.CredentialType = eProperty;
			//set credential status active
			eProperty = new Enumeration();
            //get property, and remember
            eProperty.Id = CodesManager.PROPERTY_CATEGORY_CREDENTIAL_STATUS_TYPE;
			eProperty.Items.Add( new EnumeratedItem()
			{
				CodeId = credStatus.Id,
				Id = credStatus.Id
			} );
			cred.CredentialStatusType = eProperty;


			newId = cmgr.Add( cred, ref statusMessage );

			summary.Add( string.Format( "Added credential: '{0}', id: {1}", cred.Name, cred.Id ) );

			return newId;
		}

		private bool DoesCredentialExist( DBEntity item, ref Credential cred, ref string message )
		{
			int ptotalRows = 0;
			bool isFound = false;
			//check if cred exists based on name and subject webpage
			//swp only may be enough, clearly can be spelling differences.
			string filter = string.Format( " ( base.Id in (Select Id from Credential where name = '{0}' AND Url = '{1}') )", item.CertificationName, item.CredentialUrl );
			List<CredentialSummary> exists = CredentialManager.Search( filter, "", 1, 25, ref ptotalRows );
			if ( exists != null && exists.Count > 0 )
			{
				//note if multiple, but return first
				cred = new Credential();
				//really only need the id?
				CredentialManager.MapFromSummary( exists[ 0 ], cred );
				isFound = true;
			}

			return isFound;
		}

		private int HandleAssessment( DBEntity item, ref AssessmentProfile asmt, Credential cred, Guid owningOrgUid, int defaultUserId,
				string statusMessage, ref bool entityExists, ref List<string> asmts, ref List<string> summary )
		{
			int newId = 0;
			int ptotalRows = 0;
			//check if cred exists based on name and subject webpage
			if ( DoesAssessmentExist( item, ref asmt, ref statusMessage ) )
			{
				summary.Add( string.Format( "Assessment already exists. Name: {0}, Id: {1}", item.ExamNameWithNumber, asmt.Id ) );
				entityExists = true;
				return asmt.Id;
			}

			asmt.Name = item.ExamNameWithNumber;
			asmt.Description = string.Format( "This exam is required to earn the credential:  '{0}'.", cred.Name );
			asmt.CreatedById = defaultUserId;
			asmt.CodedNotation = item.ExamNumber;
			asmt.SubjectWebpage = item.AsmtUrl;
			asmt.OwningAgentUid = owningOrgUid;
			asmt.ManagingOrgId = managingOrgId;

			//add owner role
			Enumeration eProperty = new Enumeration();
			eProperty.Id = 6;
			eProperty.Items.Add( new EnumeratedItem()
			{
				CodeId = 6,
				Id = 6
			} );
			asmt.OwnerRoles = eProperty;

			//asmt.InLanguageId = english;
			asmts.Add( asmt.Name );

			//only assign if not null
			//actually both 
			if ( item.IsProctored != null )
			{
				//if ( item.IsProctored == true )
				asmt.IsProctored = item.IsProctored;
			}
			asmt.HasGroupEvaluation = item.IsGroupEvaluation;
			asmt.HasGroupParticipation = item.IsGroupParticipation;
			//
			if ( item.Methods == "Exam" )
			{
				eProperty = new Enumeration();
				eProperty.Id = examCode.CategoryId;
				eProperty.Items.Add( new EnumeratedItem()
				{
					CodeId = examCode.Id,
					Id = examCode.Id
				} );
				asmt.AssessmentMethodType = eProperty;
			}

			if ( !string.IsNullOrWhiteSpace( item.DeliveryType ) )
			{
				if ( item.DeliveryType.IndexOf( "and" ) > 0 )
					item.DeliveryType = item.DeliveryType.Replace( "and", "," );

				var deliverTypes = item.DeliveryType.Split( ',' );

				eProperty = new Enumeration();
				eProperty.Id = inPersonCode.CategoryId;
				foreach ( string dt in deliverTypes )
				{
					if ( dt.Trim().ToLower().IndexOf( "in-person" ) > -1 )
					{
						eProperty.Items.Add( new EnumeratedItem()
						{
							CodeId = inPersonCode.Id,
							Id = inPersonCode.Id
						} );
					}
					else if ( dt.Trim().ToLower().IndexOf( "online" ) > -1 )
					{
						eProperty.Items.Add( new EnumeratedItem()
						{
							CodeId = onlineCode.Id,
							Id = onlineCode.Id
						} );
					}
					else if ( dt.Trim().ToLower().IndexOf( "blended" ) > -1 )
					{
						eProperty.Items.Add( new EnumeratedItem()
						{
							CodeId = blendedCode.Id,
							Id = blendedCode.Id
						} );
					}
				}

				asmt.DeliveryType = eProperty;
			}
			asmt.Keyword.Add( new TextValueProfile() { TextValue = item.CertProgramCode } );
			asmt.Keyword.Add( new TextValueProfile() { TextValue = item.CertTrackCode } );
			asmt.Keyword.Add( new TextValueProfile() { TextValue = item.CertTrackName } );

			amgr.Add( asmt, ref statusMessage );
			summary.Add( string.Format( "Added assessment: '{0}', id: {1}", asmt.Name, asmt.Id ) );
			return newId;
		} //
		private bool DoesAssessmentExist( DBEntity item, ref AssessmentProfile entity, ref string message )
		{
			int ptotalRows = 0;
			bool isFound = false;
			//check if entity exists based on name and subject webpage
			string filter = string.Format( " ( base.Id in (Select Id from Assessment where name = '{0}' AND Url = '{1}') )", item.ExamNameWithNumber, item.AsmtUrl );
			List<AssessmentProfile> exists = AssessmentManager.Search( filter, "", 1, 25, 0, ref ptotalRows );
			if ( exists != null && exists.Count > 0 )
			{
				//note if multiple, but return first
				entity = exists[ 0 ];
				//really only need the id?

				isFound = true;
			}

			return isFound;
		}
		private void AddConditionProfile( Credential cred, ConditionProfile cp, Entity_ConditionProfileManager cpMgr, int defaultUserId, string assessmentName, ref List<string> messages, bool isAdditionalCondition = false )
		{
			//cp = new ConditionProfile();
			cp.ProfileName = "Requirements for " + cred.Name;
			if ( isAdditionalCondition )
				cp.ProfileName = "Additional " + cp.ProfileName;

			cp.Description = string.Format( "To earn this credential, candidates must complete the {0} assessment.", assessmentName );
			cp.ConnectionProfileTypeId = 1;
			cp.AssertedByAgentUid = cred.OwningAgentUid;
			cp.CreatedById = defaultUserId;
			cpMgr.Save( cp, cred.RowId, defaultUserId, ref messages );
		}

		private void AddAlterateConditionProfile( Credential cred, ConditionProfile cp, AssessmentProfile asmt, ref List<string> messages )
		{
			ConditionProfile altcp = new ConditionProfile();
			altcp.ProfileName = "Alternate Assessment: " + asmt.Name;
			altcp.Description = string.Format( "To earn this credential, candidates must choose to complete the assessment: {0}.", asmt.Name );
			altcp.ConnectionProfileTypeId = 1; //convention is to leave as required
			altcp.ConditionSubTypeId = Entity_ConditionProfileManager.ConditionSubType_Alternative;
			altcp.AssertedByAgentUid = cred.OwningAgentUid;
			altcp.CreatedById = cred.CreatedById;
			//top cp is parent
			cpMgr.Save( altcp, cp.RowId, cred.CreatedById, ref messages );

			eaMgr.Add( altcp.RowId, asmt.Id, cred.CreatedById, true, ref messages );
		}

		private void UpdateConditionDescription( List<string> asmts, ConditionProfile cp, Entity_ConditionProfileManager cpMgr, Guid credUid )
		{
			if ( asmts.Count < 2 )
				return;

			List<string> messages = new List<string>();

			//update the description
			string desc = "To earn this credential, candidates must complete the following exams: ";
			int cntr = 0;
			foreach ( string aname in asmts )
			{
				cntr++;
				if ( cntr < asmts.Count )
					desc += aname + ", ";
				else
					desc += " and " + aname + ".";
			}

			cp.Description = desc;

			cpMgr.Save( cp, credUid, cp.CreatedById, ref messages );
		}

		private void SetRecordAsImported( int recordId )
		{
			//TODO - add actual record ids created
			using ( var context = new Data.CTIEntities() )
			{
				try
				{
					context.Configuration.LazyLoadingEnabled = false;

					EM.Import_Microsoft efEntity = context.Import_Microsoft
									.FirstOrDefault( s => s.Id == recordId );

					if ( efEntity != null && efEntity.Id > 0 )
					{
						efEntity.IsImported = true;
						if ( HasStateChanged( context ) )
						{
							int count = context.SaveChanges();
							//can be zero if no data changed
							if ( count == 0 )
							{
								//?no info on error
								//statusMessage = "Error - the update was not successful. ";
								//string message = string.Format( thisClassName + ". UpdateEnvelopeId Failed", "Attempted to update an EnvelopeId. The process appeared to not work, but was not an exception, so we have no message, or no clue. assessment: {0}, envelopeId: {1}, updatedById: {2}", assessmentId, envelopeId, userId );
								//EmailManager.NotifyAdmin( thisClassName + ".UpdateEnvelopeId Failed", message );
							}
						}


					}
					else
					{
						//statusMessage = "Error - update failed, as record was not found.";
					}
				}
				catch ( Exception ex )
				{
					LoggingHelper.LogError( ex, string.Format( "SiteManager.SetRecordAsImported(), recordId: {0}", recordId ) );
					//statusMessage = FormatExceptions(ex);
				}
			}

		}

	}
}
