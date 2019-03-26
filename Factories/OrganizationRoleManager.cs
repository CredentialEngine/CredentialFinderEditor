using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.ProfileModels;
using Models.Common;
using DBEntity = Data.Views.Credential_AgentRoleIdCSV;
using ThisEntity = Models.ProfileModels.OrganizationRoleProfile;
using EM = Data;
using Views = Data.Views;
using ViewContext = Data.Views.CTIEntities1;
using Utilities;

namespace Factories
{
	public class OrganizationRoleManager : BaseFactory
	{
		public static int CredentialToOrgRole_AccreditedBy = 1;
		public static int CredentialToOrgRole_ApprovedBy = 2;
		public static int CredentialToOrgRole_QualityAssuredBy = 3;
		public static int CredentialToOrgRole_ConferredBy = 4;
		public static int CredentialToOrgRole_CreatedBy = 5;
		public static int CredentialToOrgRole_OwnedBy = 6;
		public static int CredentialToOrgRole_OfferedBy = 7;
		public static int CredentialToOrgRole_EndorsedBy = 8;
		public static int CredentialToOrgRole_AssessedBy = 9;
		public static int CredentialToOrgRole_RecognizedBy = 10;
		public static int CredentialToOrgRole_RevokedBy = 11;
		public static int CredentialToOrgRole_RegulatedBy = 12;
		public static int CredentialToOrgRole_RenewalsBy = 13;
		public static int CredentialToOrgRole_UpdatedVersionBy = 14;


		public static int CredentialToOrgRole_MonitoredBy = 15;
		public static int CredentialToOrgRole_VerifiedBy = 16;
		public static int CredentialToOrgRole_ValidatedBy = 17;
		public static int CredentialToOrgRole_Contributor = 18;
		public static int CredentialToOrgRole_WIOAApproved = 19;

        #region role codes retrieval ==================
        public static CodeItem Codes_CredentialAgentRelationship_Get(int roleId)
        {
            CodeItem code = new CodeItem();

            using (var context = new EM.CTIEntities())
            {
                EM.Codes_CredentialAgentRelationship role = context.Codes_CredentialAgentRelationship
                            .FirstOrDefault(s => s.Id == roleId && s.IsActive == true);

                if (role != null && role.Id > 0)
                {
                    code = new CodeItem();
                    code.Id = role.Id;
                    code.Title = role.Title;
                    code.Description = role.Description;
                    code.ReverseTitle = role.ReverseRelation;
                    code.SchemaName = role.SchemaTag;
                }
            }
            return code;
        }
        //public static Enumeration GetEntityAgentQAActionFilters( bool isOrgToCredentialRole, bool getAll, string entityType )
        //{
        //	return GetEntityToOrgRolesCodes( isOrgToCredentialRole, 1, getAll, entityType );

        //}
        public static Enumeration GetEntityQARoles()
		{
			Enumeration entity = new Enumeration();

			using ( var context = new EM.CTIEntities() )
			{
				EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
							.SingleOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					EnumeratedItem val = new EnumeratedItem();
					var results = context.Codes_CredentialAgentRelationship
							.Where( s => s.IsActive == true && s.IsQARole == true )
							.OrderBy( p => p.Title )
							.ToList();

					foreach ( EM.Codes_CredentialAgentRelationship item in results )
					{
						val = new EnumeratedItem();
						val.Id = item.Id;
						val.CodeId = item.Id;
						val.Value = item.Id.ToString();//????
						val.Description = item.Description;

						val.Name = item.Title;
						val.SchemaName = item.SchemaTag;
						if ( ( bool ) item.IsQARole )
						{
							val.IsSpecialValue = true;
						}

						entity.Items.Add( val );
					}

				}
			}

			return entity;

		}
		public static Enumeration GetEntityOfferedByRoles()
		{
			Enumeration entity = new Enumeration();

			using ( var context = new EM.CTIEntities() )
			{
				EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
							.SingleOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					EnumeratedItem val = new EnumeratedItem();
					var results = context.Codes_CredentialAgentRelationship
							.Where( s => s.IsActive == true && s.SchemaTag == "ceterms:offeredBy" )
							.OrderBy( p => p.Title )
							.ToList();

					foreach ( EM.Codes_CredentialAgentRelationship item in results )
					{
						val = new EnumeratedItem();
						val.Id = item.Id;
						val.CodeId = item.Id;
						val.Value = item.Id.ToString();//????
						val.Description = item.Description;

						val.Name = item.Title;
						val.SchemaName = item.SchemaTag;
						if ( ( bool ) item.IsQARole )
						{
							val.IsSpecialValue = true;
						}

						entity.Items.Add( val );
					}

				}
			}

			return entity;

		}

		public static Enumeration GetEntityAgentQAActions( bool isOrgToCredentialRole, bool getAll = true, string entityType = "Credential" )
		{
			return GetEntityToOrgRolesCodes( isOrgToCredentialRole, 1, getAll, entityType );

		}

        public static Enumeration GetCredentialOrg_NonQARoles( bool isOrgToCredentialRole = true, string entityType = "Credential" )
		{
			return GetEntityToOrgRolesCodes( isOrgToCredentialRole, 2, true, entityType );
		}

		/// <summary>
		/// Get roles as enumeration for edit view
		/// </summary>
		/// <param name="isOrgToCredentialRole"></param>
		/// <param name="entityType"></param>
		/// <returns></returns>
		public static Enumeration GetCredentialOrg_AllRoles( bool isInverseRole = true, string entityType = "Credential" )
		{
			return GetEntityToOrgRolesCodes( isInverseRole, 0, true, entityType );
		}
		private static Enumeration GetEntityToOrgRolesCodes( bool isInverseRole, 
					int qaRoleState, 
					bool getAll,
					string entityType)
		{
			Enumeration entity = new Enumeration();

			using ( var context = new EM.CTIEntities() )
			{
				EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
							.SingleOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					EnumeratedItem val = new EnumeratedItem();
					//var sortedList = context.Codes_CredentialAgentRelationship
					//		.Where( s => s.IsActive == true && ( qaOnlyRoles == false || s.IsQARole == true) )
					//		.OrderBy( x => x.Title )
					//		.ToList();

					var Query = from P in context.Codes_CredentialAgentRelationship
							.Where( s => s.IsActive == true )
								select P;
					if ( qaRoleState == 1 ) //qa only
					{
						Query = Query.Where( p => p.IsQARole == true );
					}
					else if ( qaRoleState == 2 )
					{
						//this is state is for showning org roles for a credential.
						//16-06-01 mp - for now show qa and no qa, just skip agent to agent which for now is dept and Subsidiary
						if ( entityType.ToLower() == "credential" )
							Query = Query.Where( p => p.IsEntityToAgentRole == true );
						else
							Query = Query.Where( p => p.IsQARole == false && p.IsEntityToAgentRole == true );
					}
					else //all
					{

					}
					Query = Query.OrderBy( p => p.Title );
					var results = Query.ToList();

					//add Select option
					//need to only do if for a dropdown, not a checkbox list
					if ( qaRoleState == 1 )
					{
						//val = new EnumeratedItem();
						//val.Id = 0;
						//val.CodeId = val.Id;
						//val.Name = "Select an Action";
						//val.Description = "";
						//val.SortOrder = 0;
						//val.Value = val.Id.ToString();
						//entity.Items.Add( val );
					}
					

					//foreach ( Codes_PropertyValue item in category.Codes_PropertyValue )
					foreach ( EM.Codes_CredentialAgentRelationship item in results )
					{
						val = new EnumeratedItem
						{
							Id = item.Id,
							CodeId = item.Id,
							Value = item.Id.ToString(),//????
							Description = item.Description,
							SchemaName = item.SchemaTag,
							Totals = item.Totals ?? 0
						};

						if ( isInverseRole )
						{
							val.Name = item.ReverseRelation;
							//if ( string.IsNullOrWhiteSpace( entityType ) )
							//{
							//	//may not matter
							//	val.Description = string.Format( "Organization has {0} service.", item.ReverseRelation );
							//}
							//else
							//{
							//	val.Description = string.Format( "Organization {0} this {1}", item.ReverseRelation, entityType );
							//}
						}
						else
						{
							val.Name = item.Title;
							//val.Description = string.Format( "{0} is {1} by this Organization ", entityType, item.Title );
						}

						if ( ( bool ) item.IsQARole )
						{
							val.IsSpecialValue = true;
							//if ( IsDevEnv() )
							//	val.Name += " (QA)";
						}
						int totals = (int)item.Totals;
						if ( entityType == "Organization" && isInverseRole )
						{
							totals = ( int )item.QAPerformedTotals;
							val.Totals = ( int )item.QAPerformedTotals;
						}
						if ( IsDevEnv() )
							val.Name += string.Format( " ({0})", totals );

						if (getAll || (int)item.QAPerformedTotals  > 0 || IsDevEnv() )
							entity.Items.Add( val );
					}

				}
			}

			return entity;
		}

		public static Enumeration GetOwnerAgentRoles( bool getAll, bool isNonCredential = false )
		{
			Enumeration entity = new Enumeration();

			using ( var context = new EM.CTIEntities() )
			{
				EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
							.SingleOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					EnumeratedItem val = new EnumeratedItem();

					var results = context.Codes_CredentialAgentRelationship
							.Where( s => s.IsActive == true && s.IsOwnerAgentRole == true )
							.OrderBy( p => p.Title )
							.ToList();
					//Query = Query.OrderBy( p => p.Title );
					//var results = Query.ToList();

					foreach ( EM.Codes_CredentialAgentRelationship item in results )
					{
						val = new EnumeratedItem();
						val.Id = item.Id;
						val.CodeId = item.Id;
						val.Value = item.Id.ToString();//????
						val.Description = item.Description;
						val.Name = item.Title;
						val.SchemaName = item.SchemaTag;
						if ( item.Id == 6 )
						{
							//place owner first
							val.Selected = true;
							val.IsSpecialValue = true;
							entity.Items.Insert( 0, val );
						}
						else
						{
							//revoked only for credentials
							if ( item.Id == 11 && isNonCredential )
							{

							}
							else
								entity.Items.Add( val );
						}
					}
				}
			}
			return entity;
		}


		public static Enumeration GetAgentToAgentRolesCodes( bool isInverseRole = true )
		{
			Enumeration entity = new Enumeration();

			using ( var context = new EM.CTIEntities() )
			{
				EM.Codes_PropertyCategory category = context.Codes_PropertyCategory
							.SingleOrDefault( s => s.Id == CodesManager.PROPERTY_CATEGORY_CREDENTIAL_AGENT_ROLE );

				if ( category != null && category.Id > 0 )
				{
					entity.Id = category.Id;
					entity.Name = category.Title;
					entity.SchemaName = category.SchemaName;
					entity.Url = category.SchemaUrl;
					entity.Items = new List<EnumeratedItem>();

					EnumeratedItem val = new EnumeratedItem();
					//var sortedList = context.Codes_CredentialAgentRelationship
					//		.Where( s => s.IsActive == true && ( qaOnlyRoles == false || s.IsQARole == true) )
					//		.OrderBy( x => x.Title )
					//		.ToList();

					var Query = from P in context.Codes_CredentialAgentRelationship
							.Where( s => s.IsActive == true && s.IsAgentToAgentRole == true )
								select P;

					Query = Query.OrderBy( p => p.Title );
					var results = Query.ToList();

					foreach ( EM.Codes_CredentialAgentRelationship item in results )
					{
						val = new EnumeratedItem();
						val.Id = item.Id;
						val.CodeId = item.Id;
						val.Value = item.Id.ToString();//????
						val.Description = item.Description;
						val.SchemaName = item.SchemaTag;
						if ( isInverseRole )
						{
							val.Name = item.ReverseRelation;
							//if ( string.IsNullOrWhiteSpace( entityType ) )
							//{
							//	//may not matter
							//	val.Description = string.Format( "Organization has {0} service.", item.ReverseRelation );
							//}
							//else
							//{
							//	val.Description = string.Format( "Organization {0} this {1}", item.ReverseRelation, entityType );
							//}
						}
						else
						{
							val.Name = item.Title;
							//val.Description = string.Format( "{0} is {1} by this Organization ", entityType, item.Title );
						}

						if ( ( bool ) item.IsQARole )
						{
							val.IsSpecialValue = true;
							if ( IsDevEnv() )
								val.Name += " (QA)";
						}

						entity.Items.Add( val );
					}

				}
			}

			return entity;
		}

		
		#endregion

	}
}
