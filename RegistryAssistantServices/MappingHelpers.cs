using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization;

using MC = Models.Common;
using MPM = Models.ProfileModels;
using RA.Models;
using RA.Models.Input;
using MIPlace = Models.Common.Address;
//using MOPlace = RA.Models.Input.PostalAddress;
using MOPlace = RA.Models.Input.Place;
//using RA.Services;
using RMI = RA.Models.Input;
using Utilities;

namespace RegistryAssistantServices
{
    public class MappingHelpers
    {
        public static string credRegistryResourceUrl = UtilityManager.GetAppKeyValue( "credRegistryResourceUrl" );
        public static bool usingGraphDocuments = UtilityManager.GetAppKeyValue( "usingGraphDocuments", false );
        public static string credRegistryGraphUrl = UtilityManager.GetAppKeyValue( "credRegistryGraphUrl" );
        public static bool outputingFlattenedCosts = UtilityManager.GetAppKeyValue( "mapEditorCostsFlattenedCosts", false );
        public static bool includingMinDataWithReferenceId = UtilityManager.GetAppKeyValue( "includeMinDataWithReferenceId", false );

        public static AssistantMonitor globalMonitor = new AssistantMonitor();

        //public static List<string> messages = new List<string>();
        static string DEFAULT_GUID = "00000000-0000-0000-0000-000000000000";
        public static int PROPERTY_CATEGORY_PHONE_TYPE = 31;

        #region role type constants
        public static int ROLE_TYPE_AccreditedBy = 1;
        public static int ROLE_TYPE_ApprovedBy = 2;
        public static int ROLE_TYPE_AssertedBy = 3;
        public static int ROLE_TYPE_OWNER = 6;
        public static int ROLE_TYPE_OFFERED_BY = 7;
        public static int ROLE_TYPE_RecognizedBy = 10;
        public static int ROLE_TYPE_RevokedBy = 11;
        public static int ROLE_TYPE_RegulatedBy = 12;
        public static int ROLE_TYPE_RenewedBy = 13;
        public static int ROLE_TYPE_DEPARTMENT = 20;
        public static int ROLE_TYPE_SUBSIDIARY = 21;

        #endregion

        public static string GetTopLevelUrl
        {  get
            {
                if ( usingGraphDocuments )
                    return credRegistryGraphUrl;
                else
                    return credRegistryResourceUrl;
            }
        }
		#region Mapping From Enumerations		
		public static List<string> MapEnumermationToStringList( MC.Enumeration input )
		{
			List<string> output = new List<string>();
			if ( input != null && input.Items != null )

				foreach ( MC.EnumeratedItem item in input.Items )
				{
					if ( item != null )
					{
						//get from schemaName, and remove prefix
						//or maybe better if kept?
						if ( !string.IsNullOrWhiteSpace( item.SchemaName ))
						{
							if ( item.SchemaName.IndexOf( ":" ) > 1 )
							{
								string prop = item.SchemaName.Substring( item.SchemaName.IndexOf( ":" ) + 1 );
								output.Add( prop );
							}
							else
								output.Add( item.SchemaName );
						}
					}
				}
			return output;
		}
		public static List<string> MapNaicsToStringList( MC.Enumeration input )
		{
			List<string> output = new List<string>();
			if ( input != null && input.Items != null )
				foreach ( MC.EnumeratedItem item in input.Items )
				{
					if ( item != null && !string.IsNullOrWhiteSpace( item.Value) )
					{
						output.Add( item.Value );
					}
				}
			return output;
		}

		/// <summary>
		/// Map enumeration with only a single value allowed to a string
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string MapSingleEnumermationToString( MC.Enumeration input )
		{
			string output = "";
			if ( input != null && input.Items != null )
				foreach ( MC.EnumeratedItem item in input.Items )
				{
					if ( item != null )
					{
						//get from schemaName, and remove prefix
						if ( !string.IsNullOrWhiteSpace( item.SchemaName ))
						{
							if ( item.SchemaName.IndexOf( ":" ) > 1 )
							{
								string prop = item.SchemaName.Substring( item.SchemaName.IndexOf( ":" ) + 1 );
								output = prop;
							}
							else
								output = item.SchemaName;
						}
					}

				}
			return output;
		}
		public static List<CredentialAlignmentObject> MapEnumermationToCAO( MC.Enumeration input, string frameworkName, string framework = "" )
		{
			List<CredentialAlignmentObject> output = new List<CredentialAlignmentObject>();
			CredentialAlignmentObject cao = new CredentialAlignmentObject();
			if ( input != null && input.Items != null )
			{
				foreach ( MC.EnumeratedItem item in input.Items )
				{
					cao = new CredentialAlignmentObject();
					cao.TargetNodeName = item.Name;
					cao.TargetNodeDescription = item.Description ?? "";
					cao.TargetNode = item.SchemaName ?? "";

					cao.FrameworkName = frameworkName;
					cao.Framework = framework;
					//not likely
					cao.Framework = item.ParentSchemaName ?? "";

					output.Add( cao );

				}
			}
			return output;
		}

		/// <summary>
		/// for use with a enumerations with a framework such as Naics, SOC, etc.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static List<FrameworkItem> MapEnumermationToFrameworkItem( MC.Enumeration input, string frameworkName, string framework = "" )
		{
			List<FrameworkItem> output = new List<FrameworkItem>();
			FrameworkItem fi = new FrameworkItem();
			if ( input != null && input.Items != null )
			{
				foreach ( MC.EnumeratedItem item in input.Items )
				{
					fi = new FrameworkItem();
					//Value was populated from FrameworkCode
					fi.CodedNotation = item.Value;
					fi.Name = item.Name;
					fi.Description = item.Description ?? "";
					fi.FrameworkName = frameworkName;
					fi.Framework = framework;
					fi.URL = item.URL;

					output.Add( fi );

				}
			}
			return output;
		}
		public static List<FrameworkItem> MapTextValueProfileToFrameworkItem( List<MPM.TextValueProfile> input )
		{
			List<FrameworkItem> output = new List<FrameworkItem>();
			if ( input == null || input.Count == 0 )
				return output;
			FrameworkItem fi = new FrameworkItem();

			foreach ( MPM.TextValueProfile item in input )
			{
				fi = new FrameworkItem();
				//fi.Description = item.TextValue;
				fi.Name = item.TextValue;
				output.Add( fi );
			}
			return output;
		}
		public static List<string> MapTextValueProfileToStringList( List<MPM.TextValueProfile> input )
		{
			List<string> output = new List<string>();
			if ( input == null || input.Count == 0 )
				return output;


			foreach ( MPM.TextValueProfile item in input )
			{
				if (!string.IsNullOrWhiteSpace( item.TextValue ) )
					output.Add( item.TextValue );
			}
			return output;
		}
		#endregion

		#region Mapping durations, etc	
		public static List<DurationProfile> MapToEstimatedDuration( List<MPM.DurationProfile> input )
		{
			if ( input == null || input.Count == 0 )
				return null;

			List<RA.Models.Input.DurationProfile> list = new List<RA.Models.Input.DurationProfile>();
			var cp = new DurationProfile();

			foreach ( MPM.DurationProfile item in input )
			{
				cp = new DurationProfile
				{
					Description = item.Description,
					ExactDuration = MapDurationItem( item.ExactDuration ),
					MaximumDuration = MapDurationItem( item.MaximumDuration ),
					MinimumDuration = MapDurationItem( item.MinimumDuration )
				};

				list.Add( cp );
			}

			return list;
		}
		public static List<DurationItem> MapListToDurationItem( List<MPM.DurationItem> input )
		{
			if ( input == null || input.Count == 0 )
				return null;

			List<RA.Models.Input.DurationItem> list = new List<RA.Models.Input.DurationItem>();
			var cp = new DurationItem();

			foreach ( MPM.DurationItem item in input )
			{
				cp = new DurationItem();
				cp = MapDurationItem( item );

				list.Add( cp );
			}

			return list;
		}
		public static RA.Models.Input.DurationItem MapDurationItem( MPM.DurationItem duration )
		{
			if ( duration == null )
				return null;

			var output = new RA.Models.Input.DurationItem
			{
				Days = duration.Days,
				Hours = duration.Hours,
				Minutes = duration.Minutes,
				Months = duration.Months,
				Weeks = duration.Weeks,
				Years = duration.Years
			};
			return output;
		}

		//public static List<CostProfile> MapToEstimatedCosts( List<MPM.CostProfile> input )
		//{
		//	if ( outputingFlattenedCosts )
		//		return MapToEstimatedCostsCombined( input );
		//	else
		//		return MapToEstimatedCostsAsHierarchy( input );
		//}
		public static List<CostProfile> MapToEstimatedCosts( List<MPM.CostProfile> input )
		{
			List<string> messages = new List<string>();
			//make configurable to either flatten, or use imbedded cost items
			//bool outputingFlattenedCosts = true;

			var output = new List<CostProfile>();
			if ( input == null || input.Count == 0 )
				return output;

			foreach ( var item in input )
			{
				var cp = new CostProfile();
				cp.CostDetails = item.DetailsUrl;
				cp.Currency = item.Currency;
				cp.Description = item.Description;
				cp.Name = item.ProfileName;
				if ( !string.IsNullOrEmpty( item.EndDate ) )
					cp.EndDate = Convert.ToDateTime( item.EndDate ).ToString( "yyyy-MM-dd" );
				else
					cp.EndDate = null;

				if ( !string.IsNullOrEmpty( item.StartDate ) )
					cp.StartDate = Convert.ToDateTime( item.StartDate ).ToString( "yyyy-MM-dd" );
				else
					cp.StartDate = null;

				cp.Condition = MapToStringList( item.Condition, false );

				cp.Jurisdiction = MapJurisdictions( item.Jurisdiction, ref messages );
				//TODO - hide regions for now?
				//		need to change all references from Jurisdiction to GeoC
				//cp.Region = MapRegions( item.Region, ref messages );

				//there will be one complete record output for each CostProfileItem
				//but these need not be any
				int costItems = 0;
				var costProfileItem = new CostProfileItem();
                if (item.Items.Count > 0)
                {
                    foreach (MPM.CostProfileItem cpi in item.Items)
                    {

                        //The assistant will be handling hierarchical - OR could offer either option
                        //if ( outputingFlattenedCosts )
                        //{
                        //	cp.DirectCostType = MapSingleEnumermationToString( cpi.CostType );
                        //	cp.ResidencyType = MapEnumermationToString( cpi.ResidencyType );
                        //	cp.AudienceType = MapEnumermationToString( cpi.ApplicableAudienceType );

                        //	cp.Price = cpi.Price;
                        //	cp.PaymentPattern = cpi.PaymentPattern;

                        //	output.Add( cp );
                        //} else
                        //{
                        costProfileItem = new CostProfileItem();

                        //this must be single, even though from an enumeration
                        costProfileItem.DirectCostType = cpi.CostTypeSchema; // MapSingleEnumermationToString( cpi.CostType );
                        costProfileItem.ResidencyType = MapEnumermationToStringList( cpi.ResidencyType );
                        costProfileItem.AudienceType = MapEnumermationToStringList( cpi.ApplicableAudienceType );

                        costProfileItem.Price = cpi.Price;
                        if (!string.IsNullOrEmpty( cpi.PaymentPattern ))
                            costProfileItem.PaymentPattern = cpi.PaymentPattern;
                        else
                            costProfileItem.PaymentPattern = null;

                        cp.CostItems.Add( costProfileItem );

                        //}

                        costItems++;
                    }
                }
                else
                    cp.CostItems = null;

                if ( !outputingFlattenedCosts )
					output.Add( cp );
			}

			return output;
		} //
		#endregion
		
		public static List<string> MapToStringList( List<MPM.TextValueProfile> list, bool usingTextTitle = false )
		{
			List<string> output = new List<string>();
			if ( list == null || list.Count == 0 )
				return output;

			foreach ( MPM.TextValueProfile item in list )
			{
				if ( usingTextTitle )
				{
					output.Add( item.TextTitle );
				}
				else
				{
					if ( item.CategoryId == PROPERTY_CATEGORY_PHONE_TYPE )
					{
						if (!string.IsNullOrWhiteSpace(item.TextTitle))
						{
							//output.Add( item.TextValue + string.Format(" ({0})", item.TextTitle) );
							output.Add( item.TextValue );
						} else
							output.Add( item.TextValue );
					}
					else 
						output.Add( item.TextValue );
				}
			}
			return output;
		}
		public static List<string> MapToPhoneString( List<MPM.TextValueProfile> list)
		{
			List<string> output = new List<string>();
			if ( list == null || list.Count == 0 )
				return output;

			foreach ( MPM.TextValueProfile item in list )
			{
				if ( !string.IsNullOrWhiteSpace( item.CodeTitle ) )
				{
					output.Add( item.TextValue  );
					//output.Add( item.TextValue + string.Format( " ({0})", item.CodeTitle ) );
				}
				else
					output.Add( item.TextValue );
			}
			return output;
		}
		#region Mapping organization references
		
		public static List<OrganizationReference> MapToOrgReferences( MC.Organization org )
		{
			List<OrganizationReference> list = new List<OrganizationReference>();
			if ( org == null || org.Id == 0 )
				return list;
            OrganizationReference refOut = MapToOrgRef( org );
            list.Add( refOut );
            return list;

        }
        public static List<OrganizationReference> MapToOrgRef( List<MPM.OrganizationRoleProfile> input, int roleTypeId )
        {
            List<OrganizationReference> output = new List<OrganizationReference>();
            OrganizationReference or = new OrganizationReference();
            foreach ( var role in input )
            {
                if ( role.AgentRole != null && role.AgentRole.Items != null )
                {
                    //should only be one item
                    foreach ( var item in role.AgentRole.Items )
                    {
                        if ( item.CodeId == roleTypeId )
                            output.Add( MapToOrgRef( role.ActingAgent ) );
                    }
                }
            }

            return output;
        }
        public static OrganizationReference MapToOrgRef( MC.Organization org )
        {
            OrganizationReference refOut = new OrganizationReference();
            if ( org == null || org.Id == 0 )
                return refOut;

            //??these should just be set to @Id???
            if ( string.IsNullOrWhiteSpace( org.ctid ) )
            {
                refOut.Name = org.Name;
                refOut.Description = org.Description;
                refOut.SubjectWebpage = org.SubjectWebpage;
                //set the type. 
                if ( org.ISQAOrganization )
                    refOut.Type = "QACredentialOrganization";
                else
                    refOut.Type = "CredentialOrganization";

                if ( org.SocialMediaPages != null && org.SocialMediaPages.Count > 0 )
                {
                    //not sure we need to handle for test purposes
                    refOut.SocialMedia = new List<string>();
                    foreach ( var item in org.SocialMediaPages )
                    {
                        refOut.SocialMedia.Add( item.TextValue );
                    }
                }
            }
            else
            {
                UpdateMonitorList( org );
                refOut.Id = credRegistryGraphUrl + org.ctid;
                if ( org.ISQAOrganization )
                    refOut.Type = "QACredentialOrganization";
                else
                    refOut.Type = "CredentialOrganization";
                if ( includingMinDataWithReferenceId )
                {
                    //as means of minimizing impact of pending imports, imnclude basic data
                    refOut.Name = org.Name;
                    refOut.Description = org.Description;
                    refOut.SubjectWebpage = org.SubjectWebpage;
                }
            }


            return refOut;
        }
        /// <summary>
        /// Format list of ctids (usually) from TextValueProfiles as OrganizationReferences with Id only.
        /// TODO - replace with something more concrete
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static List<OrganizationReference> FormatOrganizationReferenceIds( List<MPM.TextValueProfile> items )
		{
			List<OrganizationReference> list = new List<OrganizationReference>();
			foreach ( MPM.TextValueProfile item in items )
			{
				if ( !string.IsNullOrWhiteSpace( item.TextValue ) )
				{
					list.Add( new OrganizationReference() { Id = credRegistryGraphUrl + item.TextValue } );
				}
			}
			return list;
		}

	
		#endregion

		#region Mapping entity references
		//17-10-19 mp - common costs and conditions are now just strings
		public static bool FormatRegistryId( string ctid, ref string outputUrl )
		{

			if ( string.IsNullOrWhiteSpace( ctid ) )
				return false;
			
			outputUrl = GetTopLevelUrl + ctid;
			return true;
		}
		public static List<string> MapToStringList( List<MC.ConditionManifest> input )
		{
			List<string> output = new List<string>();
			if ( input == null || input.Count == 0 )
				return output;
			foreach ( var item in input )
			{
				//??these should just be set to @Id, can't have a third party manifest
				if ( !string.IsNullOrWhiteSpace( item.CTID ) )
				{
					output.Add( GetTopLevelUrl + item.CTID );
				}
			}
			return output;
		}
		public static List<string> MapToStringList( List<MC.CostManifest> input )
		{
			List<string> output = new List<string>();
			if ( input == null || input.Count == 0 )
				return output;
			foreach ( var item in input )
			{
				//??these should just be set to @Id, can't have a third party manifest
				if ( !string.IsNullOrWhiteSpace( item.CTID ) )
				{
					output.Add( GetTopLevelUrl + item.CTID );
				}
			}
			return output;
		}

		public static EntityReference MapToEntityRef( MC.Credential entity )
		{
			EntityReference refOut = new EntityReference();
			if ( string.IsNullOrWhiteSpace( entity.ctid ) )
			{
				refOut.Name = entity.Name;
				refOut.Description = entity.Description;
				refOut.SubjectWebpage = entity.SubjectWebpage;
				//set the type. 
				refOut.Type = entity.CredentialTypeSchema;

			}
			else
			{
				UpdateMonitorList( entity );
				refOut.Id = GetTopLevelUrl + entity.ctid;
				//include type? - will need to ensure this gets output, but may not matter
				refOut.Type = entity.CredentialTypeSchema;
                if ( includingMinDataWithReferenceId )
                {
                    //as means of minimizing impact of pending imports, imnclude basic data
                    refOut.Name = entity.Name;
                    refOut.Description = entity.Description;
                    refOut.SubjectWebpage = entity.SubjectWebpage;
                }

            }
			return refOut;
		}

		public static EntityReference MapToEntityRef( MPM.AssessmentProfile entity )
		{
			EntityReference refOut = new EntityReference();
			if ( string.IsNullOrWhiteSpace( entity.ctid ) )
			{
				refOut.Name = entity.Name;
				refOut.Description = entity.Description;
				refOut.SubjectWebpage = entity.SubjectWebpage;
				//set the type. 
				refOut.Type = "ceterms:AssessmentProfile";
			}
			else
			{
				UpdateMonitorList( entity );
				refOut.Id = GetTopLevelUrl + entity.ctid;
				//include type? - will need to ensure this gets output, but may not matter
				refOut.Type = "ceterms:AssessmentProfile";
                if ( includingMinDataWithReferenceId )
                {
                    //as means of minimizing impact of pending imports, imnclude basic data
                    refOut.Name = entity.Name;
                    refOut.Description = entity.Description;
                    refOut.SubjectWebpage = entity.SubjectWebpage;
                }
            }
			return refOut;
		}

		public static EntityReference MapToEntityRef( MPM.LearningOpportunityProfile entity )
		{
			EntityReference refOut = new EntityReference();
			if ( string.IsNullOrWhiteSpace( entity.ctid ) )
			{
				refOut.Name = entity.Name;
				refOut.Description = entity.Description;
				refOut.SubjectWebpage = entity.SubjectWebpage;
				//set the type. 
				refOut.Type = "ceterms:LearningOpportunityProfile";
			}
			else
			{
				UpdateMonitorList( entity );
				refOut.Id = GetTopLevelUrl + entity.ctid;
				//include type? - will need to ensure this gets output, but may not matter
				refOut.Type = "ceterms:LearningOpportunityProfile";
                if ( includingMinDataWithReferenceId )
                {
                    //as means of minimizing impact of pending imports, imnclude basic data
                    refOut.Name = entity.Name;
                    refOut.Description = entity.Description;
                    refOut.SubjectWebpage = entity.SubjectWebpage;
                }
            }
			return refOut;
		}


		public static EntityReference MapToEntityRef( MC.OrganizationThirdPartyAssertion entity )
		{
			EntityReference refOut = new EntityReference();
			if ( string.IsNullOrWhiteSpace( entity.CTID ) )
			{
				if ( string.IsNullOrWhiteSpace( entity.Name )
				|| string.IsNullOrWhiteSpace( entity.SubjectWebpage ) )
					return null;

				refOut.Name = entity.Name;
				refOut.Description = entity.Description;
				refOut.SubjectWebpage = entity.SubjectWebpage;
				//set the type. 
				refOut.Type = entity.CtdlType;
			}
			else if ( entity.CTID.Length == 39 )
			{
				//UpdateMonitorList( entity );
				refOut.Id = GetTopLevelUrl + entity.CTID;
				//include type? - will need to ensure this gets output, but may not matter - won't be used later, but useful for tracing
				refOut.Type = entity.CtdlType;
				if ( includingMinDataWithReferenceId )
                {
                    //as means of minimizing impact of pending imports, imnclude basic data
                    refOut.Name = entity.Name;
                    refOut.Description = entity.Description;
                    refOut.SubjectWebpage = entity.SubjectWebpage;
                }
            }
			else
				return null;

			return refOut;
		}
        public static EntityReference MapToEntityRef( MPM.OrganizationAssertion entity )
        {
            EntityReference refOut = new EntityReference();
            if ( string.IsNullOrWhiteSpace( entity.TargetCTID ) )
            {
                if ( string.IsNullOrWhiteSpace( entity.TargetEntityName )
                || string.IsNullOrWhiteSpace( entity.TargetEntitySubjectWebpage ) )
                    return null;

                refOut.Name = entity.TargetEntityName;
                refOut.Description = entity.Description;
                refOut.SubjectWebpage = entity.TargetEntitySubjectWebpage;
                //set the type. 
                refOut.Type = entity.CtdlType;
            }
            else if ( entity.TargetCTID.Length == 39 )
            {
                //UpdateMonitorList( entity );
                refOut.Id = GetTopLevelUrl + entity.TargetCTID;
                //include type? - will need to ensure this gets output, but may not matter - won't be used later, but useful for tracing
                refOut.Type = entity.CtdlType;
                if ( includingMinDataWithReferenceId )
                {
                    //as means of minimizing impact of pending imports, imnclude basic data
                    refOut.Name = entity.TargetEntityName;
                    refOut.Description = entity.Description;
                    refOut.SubjectWebpage = entity.TargetEntitySubjectWebpage;
                }
            }
            else
                return null;

            return refOut;
        }
        public static EntityReference MapToEntityRef( MC.CredentialAlignmentObjectFrameworkProfile entity )
        {
            EntityReference refOut = new EntityReference();
            if ( string.IsNullOrWhiteSpace( entity.CTID ) )
            {
                if ( string.IsNullOrWhiteSpace( entity.EducationalFrameworkName )
                || string.IsNullOrWhiteSpace( entity.Description )
                || string.IsNullOrWhiteSpace( entity.EducationalFrameworkUrl ) )
                    return null;

                refOut.Name = entity.EducationalFrameworkName;
                refOut.Description = entity.Description;
                refOut.SubjectWebpage = entity.EducationalFrameworkUrl;
                //set the type. 
                refOut.Type = "ceterms:targetCompetencyFramework";
            }
            else if ( entity.CTID.Length == 39 )
            {
                refOut.Id = GetTopLevelUrl + entity.CTID;
                //include type? - will need to ensure this gets output, but may not matter - won't be used later, but useful for tracing
                refOut.Type = "ceterms:targetCompetencyFramework";
            }
            else
                return null;

            return refOut;
        }
        #endregion

        #region Mapping CredentialAlignmentObject
  //      public static CredentialAlignmentObject MapToCredentialAlignmentObject( MC.CredentialAlignmentObject entity )
		//{
		//	CredentialAlignmentObject refOut = new CredentialAlignmentObject();
		//	refOut.FrameworkName = entity.FrameworkName;
		//	refOut.Framework = entity.FrameworkUrl;
		//	refOut.TargetNodeName = entity.TargetNodeName;
		//	refOut.TargetNodeDescription = entity.TargetNodeDescription;
		//	//refOut.TargetNode = entity.TargetNode;
		//	refOut.CodedNotation = entity.CodedNotation;

  //          if ( string.IsNullOrWhiteSpace( entity.TargetNode ) && !string.IsNullOrWhiteSpace( entity.CTID ) )
  //          {
  //              refOut.TargetNode = credRegistryResourceUrl + entity.CTID;
  //          }
  //          else
  //              refOut.TargetNode = entity.TargetUrl;
  //          return refOut;
		//}
		public static CredentialAlignmentObject MapCompetencyToCredentialAlignmentObject( MC.CredentialAlignmentObjectProfile entity )
		{
			CredentialAlignmentObject refOut = new CredentialAlignmentObject();
            //may want an additional appkey to separate use for publishing and use in editor
            if ( UtilityManager.GetAppKeyValue( "publishingUsingCassBasedCompetencies", false ) == false )
            {
                refOut.FrameworkName = entity.FrameworkName;
                refOut.Framework = entity.FrameworkUrl;
                refOut.TargetNodeName = entity.TargetNodeName;
                refOut.TargetNodeDescription = entity.TargetNodeDescription;

                refOut.CodedNotation = entity.CodedNotation;
                refOut.Weight = entity.Weight;
                if ( string.IsNullOrWhiteSpace( entity.TargetNode ) && !string.IsNullOrWhiteSpace( entity.CTID ) )
                {
                    refOut.TargetNode = credRegistryResourceUrl + entity.CTID;
                }
                else
                    refOut.TargetNode = entity.TargetNode;
            } else
            {
                refOut.TargetNode = entity.TargetNode;
                //just include everything
                refOut.FrameworkName = entity.FrameworkName;
                refOut.Framework = entity.FrameworkUrl;
                refOut.TargetNodeName = entity.TargetNodeName;
                refOut.TargetNodeDescription = entity.TargetNodeDescription;
                refOut.CodedNotation = entity.CodedNotation;
                refOut.Weight = entity.Weight;
            }
                

            return refOut;
		}
		public static List<CredentialAlignmentObject> MapCompetenciesToCredentialAlignmentObject( List<MC.CredentialAlignmentObjectFrameworkProfile> list )
		{
			List<CredentialAlignmentObject> competencies = new List<CredentialAlignmentObject>();
			if ( list == null || list.Count == 0 )
				return competencies;

			CredentialAlignmentObject cao = new CredentialAlignmentObject();

			foreach ( var entity in list )
			{
				foreach ( var item in entity.Items )
				{
					cao = new CredentialAlignmentObject();
                    if ( UtilityManager.GetAppKeyValue( "publishingUsingCassBasedCompetencies", false ) == false )
                    {
                        cao.FrameworkName = entity.EducationalFrameworkName;
                        cao.Framework = entity.EducationalFrameworkUrl;

                        cao.TargetNodeName = item.TargetNodeName;
                        cao.TargetNodeDescription = item.Description;

                        cao.CodedNotation = item.CodedNotation;
                        if (string.IsNullOrWhiteSpace( item.TargetNode ) && !string.IsNullOrWhiteSpace( item.CTID ))
                        {
                            cao.TargetNode = credRegistryResourceUrl + item.CTID;
                        }
                        else
                        {
                            cao.TargetNode = item.TargetNode;
                            //just include everything
                            cao.FrameworkName = entity.EducationalFrameworkName;
                            cao.Framework = entity.EducationalFrameworkUrl;
                            cao.TargetNodeName = item.TargetNodeName;
                            cao.TargetNodeDescription = item.TargetNodeDescription;
                            cao.CodedNotation = item.CodedNotation;
                            //cao.Weight = item.Weight;
                        }
                        competencies.Add( cao );
                    } else
                    {

                        if ( !string.IsNullOrWhiteSpace( item.RepositoryUri ) )
                        {
                            cao.TargetNode = item.RepositoryUri;
                            competencies.Add( cao );
                        }
                    }

                    
				}
				
			}

			return competencies;
		}


		#endregion

		#region IdentifierValue

		public static List<RMI.IdentifierValue> AssignIdentifierValueToList( string value )
		{
			if ( string.IsNullOrWhiteSpace( value ) )
				return null;

			List<RMI.IdentifierValue> list = new List<RMI.IdentifierValue>();
			list.Add( new RMI.IdentifierValue()
			{
				IdentifierValueCode = value
			} );

			return list;
		}
		public static List<RMI.IdentifierValue> AssignTextValueProfileListToList( List<MPM.TextValueProfile> input )
		{
			if ( input == null || input.Count > 0 )
				return null;

			List<RMI.IdentifierValue> list = new List<RMI.IdentifierValue>();
			foreach ( var item in input )
			{
				list.Add( new RMI.IdentifierValue()
				{
					IdentifierValueCode = item.TextValue,
					IdentifierType = item.TextTitle,
					Description = item.Description
				} );
			}
			return list;
		}

		#endregion 

		#region Mapping addresses, juridictions, etc


		public static List<MOPlace> FormatAvailableAt( List<MC.Address> input )
		{
			List<string> messages = new List<string>();
			return FormatAvailableAtList( input, ref messages );

			
		}
		/// <summary>
		/// Format AvailableAt
		/// 17-10-20 - essentially an address now
		/// 17-11-02 - essentially a Place now
		/// </summary>
		/// <param name="input"></param>
		/// <param name="messages"></param>
		/// <returns></returns>
		public static List<MOPlace> FormatAvailableAtList( List<MIPlace> input, ref List<string> messages )
		{
			//Available At should require an address, not just contact points
			return FormatPlacesList( input, ref messages, true );
			
		}

		public static List<MOPlace> FormatPlacesList( List<MC.Address> input )
		{
			List<string> messages = new List<string>();
			return FormatPlacesList( input, ref messages );
		}
		public static List<MOPlace> FormatPlacesList( List<MIPlace> input, ref List<string> messages, bool addressExpected = false )
		{

			List<MOPlace> list = new List<MOPlace>();
			if ( input == null || input.Count == 0 )
				return list;
			MOPlace output = new MOPlace();

			foreach ( var item in input )
			{
				output = new MOPlace();

				output = FormatPlace( item, addressExpected, ref messages );

				list.Add( output );
			}

			if ( messages.Count > 0 )
				globalMonitor.Messages.AddRange( messages );

			return list;
		}

		public static MOPlace FormatPlace( MIPlace input, bool isAddressExpected, ref List<string> messages )
		{
			MOPlace output = new MOPlace();
			if ( input == null || input.HasAddress() == false )
				return output;

			//need to handle null or incomplete
			RMI.ContactPoint cpo = new RMI.ContactPoint();

			output = ( new MOPlace
			{
				Name = input.Name,
				Address1 = input.Address1,
				Address2 = input.Address2,
				City = input.City,
				Country = input.Country,
				AddressRegion = input.AddressRegion,
				PostalCode = input.PostalCode
			} );

			//always include lat/lng
			output.Latitude = input.Latitude;
			output.Longitude = input.Longitude;

			output.Description = input.Description;

			bool hasContactPoints = false;
			if ( input.ContactPoint != null && input.ContactPoint.Count > 0 )
			{
				foreach ( var cpi in input.ContactPoint )
				{
					cpo = new RMI.ContactPoint()
					{
						Name = cpi.Name,
						ContactType = cpi.ContactType
					};
					//if (!string.IsNullOrWhiteSpace( cpi.ContactOption ) )
					//	cpo.ContactOption.Add(cpi.ContactOption);
					cpo.PhoneNumbers = MapToStringList( cpi.PhoneNumbers );
					cpo.Emails = MapToStringList( cpi.Emails );
					cpo.SocialMediaPages = MapToStringList( cpi.SocialMediaPages );

					output.ContactPoint.Add( cpo );
					hasContactPoints = true;
				}
			}
			else
				output.ContactPoint = null;

			bool hasAddress = false;
			if ( ( string.IsNullOrWhiteSpace( output.Address1 )
					&& string.IsNullOrWhiteSpace( output.PostOfficeBoxNumber ) )
					|| string.IsNullOrWhiteSpace( input.City )
					|| string.IsNullOrWhiteSpace( input.AddressRegion )
					|| string.IsNullOrWhiteSpace( input.PostalCode ) )
			{
				if ( isAddressExpected )
				{
					messages.Add( "Error - A valid address expected. Please provide a proper address, along with any contact points." );
					return null;
				}
			}
			else
				hasAddress = true;

			//check for at an address or contact point
			if ( !hasContactPoints && !hasAddress
			)
			{
				messages.Add( "Error - incomplete place/address. Please provide a proper address and/or one or more proper contact points." );
				output = null;
			}

			if ( messages.Count > 0 )
				globalMonitor.Messages.AddRange( messages );

			return output;
		}
		public static List<RMI.Jurisdiction> MapJurisdictions( List<MC.JurisdictionProfile> list, ref List<string> messages )
		{
			List<RMI.Jurisdiction> output = new List<RMI.Jurisdiction>();
			if ( list == null || list.Count == 0 )
				return null;
			RMI.Jurisdiction jp = new RMI.Jurisdiction();
			foreach ( var j in list )
			{
				jp = new RMI.Jurisdiction();
				jp = MapToJurisdiction( j, ref messages );
				output.Add( jp );
			}


			return output;
		}

		public static List<RMI.JurisdictionAssertedInProfile> MapJurisdictionAssertions( List<MC.JurisdictionProfile> list, ref List<string> messages )
		{
			List<RMI.JurisdictionAssertedInProfile> output = new List<RMI.JurisdictionAssertedInProfile>();
			if ( list == null || list.Count == 0 )
				return null;
			RMI.JurisdictionAssertedInProfile jp = new RMI.JurisdictionAssertedInProfile();
			foreach ( var j in list )
			{
				jp = new RMI.JurisdictionAssertedInProfile();
				jp.Jurisdiction = MapToJurisdiction( j, ref messages );
				//additional check for asserted by org, and list of assertion types
				jp.AssertedBy = MapToOrgRef( j.AssertedByOrganization );
				foreach ( var item in j.JurisdictionAssertion.Items )
				{
					if ( item.SchemaName == "ceterms:accreditedIn" )
					{
						jp.AssertsAccreditedIn = true;
					}
					else if ( item.SchemaName == "ceterms:approvedIn" )
					{
						jp.AssertsApprovedIn = true;
					}
					else if ( item.SchemaName == "ceterms:offeredIn" )
					{
						jp.AssertsOfferedIn = true;
					}
					else if ( item.SchemaName == "ceterms:recognizedIn" )
					{
						jp.AssertsRecognizedIn = true;
					}
					else if ( item.SchemaName == "ceterms:regulatedIn" )
					{
						jp.AssertsRegulatedIn = true;
					}
					else if ( item.SchemaName == "ceterms:renewedIn" )
					{
						jp.AssertsRenewedIn = true;
					}
					else if ( item.SchemaName == "ceterms:revokedIn" )
					{
						jp.AssertsRevokedIn = true;
					}
				}
				output.Add( jp );
			}


			return output;
		}
		//public static List<RMI.GeoCoordinates> MapRegions( List<MC.GeoCoordinates> list, ref List<string> messages )
		//{
		//	List<RMI.GeoCoordinates> output = new List<RMI.GeoCoordinates>();
		//	if ( list == null || list.Count == 0 )
		//		return null;
		//	RMI.GeoCoordinates jp = new RMI.GeoCoordinates();
		//	foreach ( var j in list )
		//	{
		//		jp = new RMI.GeoCoordinates();
		//		//jp = MapGeoCoordinates( j, ref messages );
		//	}


		//	return output;
		//}
		public static RMI.Jurisdiction MapToJurisdiction( MC.JurisdictionProfile profile, ref List<string> messages )
		{
			Jurisdiction entity = new Jurisdiction();

			entity.Description = profile.Description;
			if ( profile.MainJurisdiction != null && profile.MainJurisdiction.GeoNamesId > 0 )
			{
				//entity.MainJurisdiction = profile.MainJurisdiction.Name;
				entity.GlobalJurisdiction = false;
				entity.MainJurisdiction = MapGeoCoordinatesToPlace( profile.MainJurisdiction );
			}
			else
			{
				entity.GlobalJurisdiction = profile.GlobalJurisdiction;
			}

			//handle exceptions
			if ( profile.JurisdictionException != null && profile.JurisdictionException.Count > 0 )
			{
				foreach ( MC.GeoCoordinates item in profile.JurisdictionException )
				{
					entity.JurisdictionException.Add( MapGeoCoordinatesToPlace( item ) );
				}
			}


			return entity;
		}
		public static MOPlace MapGeoCoordinatesToPlace( MC.GeoCoordinates profile )
		{
			MOPlace entity = new MOPlace();
			entity.Name = profile.Name;
			//entity.GeoNamesId = profile.GeoNamesId;
			entity.Latitude = profile.Latitude;
			entity.Longitude = profile.Longitude;
			entity.GeoURI = profile.Url;
			entity.AddressRegion = profile.Region;
			entity.Country = profile.Country;

			return entity;
		}
		//public static GeoCoordinates MapGeoCoordinates( MC.GeoCoordinates profile )
		//{
		//	GeoCoordinates entity = new GeoCoordinates();
		//	entity.Name = profile.Name;
		//	entity.GeoNamesId = profile.GeoNamesId;
		//	entity.Latitude = profile.Latitude;
		//	entity.Longitude = profile.Longitude;
		//	entity.GeoUri = profile.Url;
		//	entity.Region = profile.Region;
		//	entity.Country = profile.Country;

		//	return entity;
		//}

		#endregion

		#region conditions, process, connections
		public static List<ConditionProfile> MapConditionProfiles( List<MPM.ConditionProfile> input )
		{
			var output = new List<ConditionProfile>();
			List<string> messages = new List<string>();
			if ( input == null || input.Count == 0 )
				return output;

			foreach ( var item in input )
			{
				var cp = new ConditionProfile();

				cp.Name = item.Name;
                if (string.IsNullOrWhiteSpace( item.Description ) )
                {
                    globalMonitor.Messages.Add( string.Format("A description must be provided for a condition profile. Name: {0}, Type: {1}", cp.Name, item.ConnectionProfileType) );
                } else 
				    cp.Description = item.Description;
				if ( !string.IsNullOrWhiteSpace( item.SubjectWebpage ) )
					cp.SubjectWebpage = item.SubjectWebpage;
				cp.AudienceLevelType = MapEnumermationToStringList( item.AudienceLevelType );
				cp.AudienceType = MapEnumermationToStringList( item.AudienceType );

				cp.DateEffective = item.DateEffective;
				cp.Condition = MapToStringList( item.Condition );
				cp.SubmissionOf = MapToStringList( item.SubmissionOf );
				cp.AssertedBy = MapToOrgRef( item.AssertedBy );
				cp.Experience = item.Experience;
				cp.MinimumAge = item.MinimumAge;
				cp.YearsOfExperience = item.YearsOfExperience;
				cp.Weight = item.Weight;

				//
				cp.CreditHourType = item.CreditHourType;
				cp.CreditHourValue = item.CreditHourValue;
				cp.CreditUnitType = MapSingleEnumermationToString( item.CreditUnitType );
				cp.CreditUnitTypeDescription = item.CreditUnitTypeDescription;
				cp.CreditUnitValue = item.CreditUnitValue;

				//costs
				//flattened version - not used
				//cp.EstimatedCost = MapToEstimatedCostsCombined( item.EstimatedCost );
				cp.EstimatedCost = MapToEstimatedCosts( item.EstimatedCost );

				//jurisdictions
				foreach ( MC.JurisdictionProfile jp in item.Jurisdiction )
				{
					cp.Jurisdiction.Add( MapToJurisdiction( jp, ref messages ) );
				}
				foreach ( MC.JurisdictionProfile jp in item.ResidentOf )
				{
					cp.ResidentOf.Add( MapToJurisdiction( jp, ref messages ) );
				}

				//targets
				foreach ( var ta in item.TargetCredential )
				{
					cp.TargetCredential.Add( MapToEntityRef( ta ) );
				}

				foreach ( var ta in item.TargetAssessment )
				{
					cp.TargetAssessment.Add( MapToEntityRef( ta ) );
				}
				foreach ( var ta in item.TargetLearningOpportunity )
				{
					cp.TargetLearningOpportunity.Add( MapToEntityRef( ta ) );
				}

				foreach ( var ta in item.TargetCompetency )
				{
					cp.RequiresCompetency.Add( MapCompetencyToCredentialAlignmentObject( ta ) );

				}
				cp.AlternativeCondition = MapConditionProfiles( item.AlternativeCondition );

				if ( messages.Count > 0 )
					globalMonitor.Messages.AddRange( messages );

				output.Add( cp );
			}
			return output;
		}

		public static List<ProcessProfile> MapProcessProfiles( List<MPM.ProcessProfile> input, ref List<string> messages )
		{
			var output = new List<ProcessProfile>();
			if ( input == null || input.Count == 0 )
				return output;

			foreach ( var item in input )
			{
				var profile = new ProcessProfile
				{
					DateEffective = item.DateEffective,
					Description = item.Description,
					ExternalInputType = MapEnumermationToStringList( item.ExternalInput ),
					ProcessFrequency = item.ProcessFrequency,
					ProcessingAgent = MapToOrgReferences( item.ProcessingAgent ),
					SubjectWebpage = item.SubjectWebpage,
					ProcessMethod = item.ProcessMethod,
					ProcessMethodDescription = item.ProcessMethodDescription,
					ProcessStandards = item.ProcessStandards,
					ProcessStandardsDescription = item.ProcessStandardsDescription,
					ScoringMethodDescription = item.ScoringMethodDescription,
					ScoringMethodExample = item.ScoringMethodExample,
					ScoringMethodExampleDescription = item.ScoringMethodExampleDescription,
					VerificationMethodDescription = item.VerificationMethodDescription
				};
				//ExternalInputType = MapEnumermationToCAO( item.ExternalInput, "External Input Type" ),

				foreach ( var ta in item.TargetCredential )
				{
					profile.TargetCredential.Add( MapToEntityRef( ta ) );
				}

				foreach ( var ta in item.TargetAssessment )
				{
					profile.TargetAssessment.Add( MapToEntityRef( ta ) );
				}
				foreach ( var ta in item.TargetLearningOpportunity )
				{
					profile.TargetLearningOpportunity.Add( MapToEntityRef( ta ) );
				}
                foreach ( var ta in item.RequiresCompetenciesFrameworks )
                {
                    profile.TargetCompetencyFramework.Add( MapToEntityRef( ta ) );
                }

                profile.Jurisdiction = MapJurisdictions( item.Jurisdiction, ref messages );
				//profile.Region = MapRegions( item.Region, ref messages );

				//foreach ( var j in item.Jurisdiction )
				//                profile.Jurisdiction.Add( MapToJurisdiction( j ) );

				//            foreach ( var r in item.Region )
				//                profile.Region.Add( MapToJurisdiction( r ) );

				output.Add( profile );
			}

			if ( messages.Count > 0 )
				globalMonitor.Messages.AddRange( messages );

			return output;
		}

		public static List<RMI.Connections> FormatCredentialConnections( List<MPM.ConditionProfile> requires )
		{
			if ( requires == null || requires.Count == 0 )
				return null;

			var list = new List<Connections>();
			foreach ( var item in requires )
			{
				var cc = new Connections();
				cc.Name = item.Name;
				cc.Description = item.Description;
                if ( string.IsNullOrWhiteSpace( item.Description ) || item.Description.Length < 20 )
                    globalMonitor.Messages.Add( "Error - Enter a meaningful description for a Connection (at least 20 characters)." );
                    cc.Weight = item.Weight;
				cc.CreditHourValue = item.CreditHourValue;
				cc.CreditHourType = item.CreditHourType;
				cc.CreditUnitType = MapSingleEnumermationToString( item.CreditUnitType );
				cc.CreditUnitTypeDescription = item.CreditUnitTypeDescription;
				cc.CreditUnitValue = item.CreditUnitValue;
				cc.AssertedBy = MapToOrgRef( item.AssertedBy );

                //must have at least one target to be valid
                int targetCount = 0;
				foreach ( var ta in item.TargetCredential )
				{
                    targetCount++;

                    cc.TargetCredential.Add( MapToEntityRef( ta ) );
				}

				foreach ( var ta in item.TargetAssessment )
				{
                    targetCount++;
                    cc.TargetAssessment.Add( MapToEntityRef( ta ) );
				}
				foreach ( var ta in item.TargetLearningOpportunity )
				{
                    targetCount++;
                    cc.TargetLearningOpportunity.Add( MapToEntityRef( ta ) );
				}
                if ( targetCount == 0 )
                    globalMonitor.Messages.Add( "Error: At least one credential, assessment or learning opportunity must be selected as a target for a connection." );

                list.Add( cc );
			}
			return list;
		}

		#endregion

		public static void UpdateMonitorList( MC.Organization entity)
		{
			//, ref AssistantMonitor monitor 
			if ( entity != null && entity.Id > 0 )
			{
				//todo first search for existance
                int index = globalMonitor.PendingEntities.FindIndex( a => a.EntityTypeId == 2 && a.Id == entity.Id );
                if ( index == -1 )
                {
                    //do a check if published, and when 
                    //globalMonitor.PendingEntities.Add( new MC.EntityReference()
                    //{
                    //    Id = entity.Id,
                    //    EntityBaseName = entity.Name,
                    //    EntityTypeId = 2,
                    //    CTID = entity.CTID
                    //} );
                }
            }
		}


		public static void UpdateMonitorList( MC.Credential credential )
		{
			if ( credential != null && credential.Id > 0 )
			{
                //todo first search for existance
                //or sort later 
                int index = globalMonitor.PendingEntities.FindIndex( a => a.EntityTypeId == 1 && a.Id == credential.Id );
                if ( index == -1 )
                {
                    //globalMonitor.PendingEntities.Add( new MC.EntityReference()
                    //{
                    //    Id = credential.Id,
                    //    EntityBaseName = credential.Name,
                    //    EntityTypeId = 1,
                    //    CTID = credential.CTID
                    //} );
                }
			}
		}
		public static void UpdateMonitorList( MC.ConditionManifest entity )
		{
			if ( entity != null && entity.Id > 0 )
			{
                //todo first search for existance
                int index = globalMonitor.PendingEntities.FindIndex( a => a.EntityTypeId == 19 && a.Id == entity.Id );
                if ( index == -1 )
                {
                    //globalMonitor.PendingEntities.Add( new MC.EntityReference()
                    //{
                    //    Id = entity.Id,
                    //    EntityBaseName = entity.Name,
                    //    EntityTypeId = 19,
                    //    CTID = entity.CTID
                    //} );
                }
            }
		}
		public static void UpdateMonitorList( MC.CostManifest entity )
		{
			if ( entity != null && entity.Id > 0 )
			{
                //todo first search for existance
                int index = globalMonitor.PendingEntities.FindIndex( a => a.EntityTypeId == 20 && a.Id == entity.Id );
                if ( index == -1 )
                {
                    //globalMonitor.PendingEntities.Add( new MC.EntityReference()
                    //{
                    //    Id = entity.Id,
                    //    EntityBaseName = entity.Name,
                    //    EntityTypeId = 20,
                    //    CTID = entity.CTID
                    //} );
                }
            }
		}
		public static void UpdateMonitorList( MPM.AssessmentProfile entity )
		{
			if ( entity != null && entity.Id > 0 )
			{
                //todo first search for existance
                int index = globalMonitor.PendingEntities.FindIndex( a => a.EntityTypeId == 3 && a.Id == entity.Id );
                if ( index == -1 )
                {
                    //globalMonitor.PendingEntities.Add( new MC.EntityReference()
                    //{
                    //    Id = entity.Id,
                    //    EntityBaseName = entity.Name,
                    //    EntityTypeId = 3,
                    //    CTID = entity.CTID
                    //} );
                }
            }
		}
		public static void UpdateMonitorList( MPM.LearningOpportunityProfile entity )
		{
			if ( entity != null && entity.Id > 0 )
			{
                //todo first search for existance
                int index = globalMonitor.PendingEntities.FindIndex( a => a.EntityTypeId == 7 && a.Id == entity.Id );
                if ( index == -1 )
                {
                    //globalMonitor.PendingEntities.Add( new MC.EntityReference()
                    //{
                    //    Id = entity.Id,
                    //    EntityBaseName = entity.Name,
                    //    EntityTypeId = 7,
                    //    CTID = entity.CTID
                    //} );
                }
            }
		}
		public static void UpdateMonitorList( List<OrganizationReference> list, ref AssistantMonitor monitor )
		{
			if (list != null && list.Count > 0)
			{

			}
		}

		public static void ReportRelatedEntitiesToBePublished(ref List<string> messages)
		{
            if ( UtilityManager.GetAppKeyValue( "displayingAdditionalEntitiesToPublish", false ) )
            {
                if ( globalMonitor != null && globalMonitor.PendingEntities.Count > 0 )
                {
                    string msg = "";
                    string template = "<a href='/publisher/{0}/{1}' target='pendingItem'>{0} # {1} - {2} ( CTID: {3})</a>";
                    messages.Add( string.Format( "=== {0} Request.  Additional Entities to publish encountered ===", globalMonitor.RequestType ) );
                    messages.Add( "The following entities were encountered, these must also be published. First step, this process will improve over time" );
                    LoggingHelper.DoTrace( 4, string.Format( "=== {0} Request.  Additional Entities to publish encountered ===\r\nThe following entities were encountered, these must also be published. First step, this process will improve over time", globalMonitor.RequestType ) );
                    foreach ( var item in globalMonitor.PendingEntities )
                    {
                        msg = string.Format( template, item.EntityType, item.Id, item.EntityBaseName, item.CTID );
                        messages.Add( msg );
                        LoggingHelper.DoTrace( 4, msg );
                    }
                }
            }
		}
		public static JsonSerializerSettings GetJsonSettings()
		{
			var settings = new JsonSerializerSettings()
			{
				NullValueHandling = NullValueHandling.Ignore,
				DefaultValueHandling = DefaultValueHandling.Ignore,
				ContractResolver = new AlphaNumericContractResolver(),
                Formatting = Formatting.Indented,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

			return settings;
        }
		//Force properties to be serialized in alphanumeric order
		public class AlphaNumericContractResolver : DefaultContractResolver
		{
			protected override System.Collections.Generic.IList<JsonProperty> CreateProperties( System.Type type, MemberSerialization memberSerialization )
			{
				return base.CreateProperties( type, memberSerialization ).OrderBy( m => m.PropertyName ).ToList();
			}
		}
		
		public static bool IsValidGuid( Guid field )
		{
			if ( ( field == null || field == Guid.Empty ) )
				return false;
			else
				return true;
		}
		public static bool IsValidGuid( string field )
		{
			Guid guidOutput;
			if ( ( field == null || field.ToString() == DEFAULT_GUID ) )
				return false;
			else if ( !Guid.TryParse( field, out guidOutput ) )
				return false;
			else
				return true;
		}

	}

	public class AssistantMonitor
	{
		public AssistantMonitor()
		{
			Messages = new List<string>();
			PendingEntities = new List<MC.EntityReference>();
		}

		public string RequestType { get; set; }
		public List<string> Messages { get; set; }

		public List<MC.EntityReference> PendingEntities { get; set; }

		public string EnvelopeId { get; set; }
		public string Payload { get; set; }
	}
}
