using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Common;
using MPM = Models.ProfileModels;
using ViewContext = Data.Views.CTIEntities1;
using EM = Data;
using Utilities;
using DBEntity = Data.Entity_Assertion;
using Views = Data.Views;
using ViewCsv = Data.Views.Entity_AssertionCSV;
using ThisEntity = Models.ProfileModels.OrganizationAssertion;

namespace Factories
{
    public class Entity_AssertionManager : BaseFactory
    {
        static string thisClassName = "Entity_AssertionManager";

        #region Persistance


        public bool Update( Guid parentUid, MPM.OrganizationAssertion profile,
             int userId,
             ref List<string> messages )
        {
            bool isValid = true;
            int intialCount = messages.Count;
            if ( !IsValidGuid( parentUid ) )
            {
                messages.Add( "Error: the parent identifier was not provided." );
                return false;
            }

            int count = 0;
            int msgCount = messages.Count;
            var efEntity = new DBEntity();

            Entity parent = EntityManager.GetEntity( parentUid );
            if ( parent == null || parent.Id == 0 )
            {
                messages.Add( "Error - the parent entity was not found." );
                return false;
            }
            if ( parentUid == profile.TargetUid )
            {
                messages.Add( "Error: You cannot assert QA performed on the same organization as the parent organization" );
                return false;
            }

            //validate and get all roles
            List<MPM.OrganizationAssertion> list = FillAssertions( profile, ref messages, ref isValid );
            if ( messages.Count > msgCount )
                return false;
            try
            {
                using ( var context = new Data.CTIEntities() )
                {
                    //need to be careful here - as required, can't delete all
                    //if ( !profile )
                    //{
                    //    //delete all assertions?
                    //    var existingAssertions = context.Entity_Assertion.Where( s => s.EntityId == parent.Id );

                    //}
                    {
                        //NOTE: will not find anything if the target was changed!!
                        var existingAssertions = context.Entity_Assertion.Where( s => s.EntityId == parent.Id && s.TargetEntityUid == profile.TargetUid ).ToList();
                        var assertionTypeIds = list.Select( x => x.AssertionTypeId ).ToList();

                        //delete Assertions which are not selected 
                        var notExistAssertions = existingAssertions.Where( x => !assertionTypeIds.Contains( x.AssertionTypeId ) ).ToList();
                        foreach ( var assertion in notExistAssertions )
                        {
                            context.Entity_Assertion.Remove( assertion );
                            context.SaveChanges();
                        }

                        foreach ( var entity in list )
                        {
                            efEntity = context.Entity_Assertion.FirstOrDefault( s => s.EntityId == parent.Id && s.TargetEntityUid == entity.TargetUid && s.AssertionTypeId == entity.AssertionTypeId );

                            if ( efEntity == null || efEntity.Id == 0 )
                            {

                                efEntity = new DBEntity
                                {
                                    EntityId = parent.Id,
                                    AssertionTypeId = entity.AssertionTypeId,
                                    TargetEntityTypeId = profile.TargetEntityTypeId,
                                    TargetEntityUid = profile.TargetUid,
                                    Created = DateTime.Now,
                                    CreatedById = userId
                                };
                                context.Entity_Assertion.Add( efEntity );
                                count = context.SaveChanges();
                            }

                        } //foreach
                    }
                }
            }
            catch ( Exception ex )
            {
                string message = FormatExceptions( ex );
                messages.Add( message );
                LoggingHelper.LogError( ex, "Entity_AssertionManager.Update()" );
            }
            return isValid;
        }
     
        private List<MPM.OrganizationAssertion> FillAssertions( MPM.OrganizationAssertion profile,
            ref List<string> messages,
            ref bool isValid )
        {
            isValid = false;

            MPM.OrganizationAssertion entity = new MPM.OrganizationAssertion();
            List<MPM.OrganizationAssertion> list = new List<MPM.OrganizationAssertion>();

            if ( !IsGuidValid( profile.TargetUid ) )
            {
                //roles, no agent
                messages.Add( "Invalid request, please select a target for selected roles." );
                return list;
            }
            if ( profile.AgentAssertion == null || profile.AgentAssertion.Items.Count == 0 )
            {
                messages.Add( "Invalid request, please select one or more roles for this selected target." );
                return list;
            }

            //loop thru the roles
            foreach ( EnumeratedItem e in profile.AgentAssertion.Items )
            {
                entity = new MPM.OrganizationAssertion();
                entity.ParentId = profile.ParentId;
                entity.AgentUid = profile.AgentUid;
                entity.TargetUid = profile.TargetUid;
                entity.AssertionTypeId = e.Id;

                list.Add( entity );
            }


            isValid = true;
            return list;
        }
        private static bool AgentEntityRoleExists( int entityId, Guid targetEntityUid, int roleId )
        {
            MPM.EntityAgentRelationship item = new MPM.EntityAgentRelationship();
            using ( var context = new EM.CTIEntities() )
            {
                EM.Entity_Assertion entity = context.Entity_Assertion.FirstOrDefault( s => s.EntityId == entityId
                        && s.TargetEntityUid == targetEntityUid
                        && s.AssertionTypeId == roleId );
                if ( entity != null && entity.Id > 0 )
                {
                    return true;
                }
            }
            return false;
        }
        public static bool Delete( int parentId, Guid targetUid )
        {
            bool isValid = false;

            using ( var context = new Data.CTIEntities() )
            {
                context.Entity_Assertion.RemoveRange( context.Entity_Assertion.Where( s => s.EntityId == parentId && s.TargetEntityUid == targetUid ) );
                int count = context.SaveChanges();
                if ( count > 0 )
                {
                    isValid = true;
                }
                else
                {
                    //if doing a delete on spec, may not have been any roles, so ignore, or maybe log
                    //messages.Add( string.Format( "Warning Delete failed, Agent role record(s) were not found for: entityId: {0}, agentUid: {1}", parent.Id, roleId));
                    //isValid = false;
                }
            }

            return isValid;
        }
        #endregion
        public static List<MPM.OrganizationRoleProfile> GetAllCombined( Guid agentUid )
        {
            MPM.OrganizationRoleProfile orp = new MPM.OrganizationRoleProfile();
            List<MPM.OrganizationRoleProfile> list = new List<MPM.OrganizationRoleProfile>();
            EnumeratedItem eitem = new EnumeratedItem();

            Guid prevTargetUid = new Guid();
            string prevRoleSource = "";
            int prevRoleTypeId = 0;
            Entity agentEntity = EntityManager.GetEntity( agentUid );

            using ( var context = new ViewContext() )
            {
                List<Views.Organization_CombinedQAPerformed> agentRoles = context.Organization_CombinedQAPerformed
                    .Where( s => s.OrgUid == agentUid
                         && s.IsQARole == true)
                         .OrderBy( s => s.TargetEntityTypeId )
                         .ThenBy( s => s.TargetEntityBaseId )
                         .ThenBy( s => s.TargetEntityName )
                         .ThenBy( s => s.AgentToSourceRelationship )
                         .ThenBy( s => s.roleSource )
                    .ToList();

                foreach ( Views.Organization_CombinedQAPerformed entity in agentRoles )
                {
                    //loop until change in entity type?
                    if ( prevTargetUid != entity.TargetEntityUid )
                    {
                        //handle previous fill
                        if ( IsGuidValid( prevTargetUid ) && prevRoleTypeId > 0 )
                        {
                            orp.AgentRole.Items.Add( eitem );
                            list.Add( orp );
                        }

                        prevTargetUid = entity.TargetEntityUid;
                        prevRoleSource = entity.roleSource;
                        prevRoleTypeId = entity.RelationshipTypeId;//maybe zero

                        orp = new MPM.OrganizationRoleProfile
                        {
                            Id = 0,
                            ParentId = agentEntity.Id,
                            ParentTypeId = agentEntity.EntityTypeId,
                            ProfileSummary = entity.TargetEntityName,

                            AgentRole = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_ENTITY_AGENT_ROLE )
                        };
                        orp.AgentRole.ParentId = entity.OrgId;

                        orp.AgentRole.Items = new List<EnumeratedItem>();
                        orp.TargetEntityType = entity.TargetEntityType;

                        if ( entity.TargetEntityTypeId == CodesManager.ENTITY_TYPE_CREDENTIAL )
                        {
                            //17-08-27 mp - just get the basic for each entity!
                            orp.TargetCredential = CredentialManager.GetBasic( entity.TargetEntityBaseId ?? 0 );
                            //orp.TargetCredential.Id = entity.TargetEntityBaseId ?? 0;
                            //orp.TargetCredential.RowId = entity.TargetEntityUid;
                            //orp.TargetCredential.Name = entity.TargetEntityName;
                            //orp.TargetCredential.Description = entity.TargetEntityDescription;
                            //orp.TargetCredential.SubjectWebpage = entity.TargetEntitySubjectWebpage;
                            //orp.TargetCredential.ImageUrl = entity.TargetEntityImageUrl;

                        }
                        else if ( entity.TargetEntityTypeId == CodesManager.ENTITY_TYPE_ORGANIZATION )
                        {
                            orp.TargetOrganization.Id = entity.TargetEntityBaseId ?? 0;
                            orp.TargetOrganization.RowId = entity.TargetEntityUid;
                            orp.TargetOrganization.Name = entity.TargetEntityName;
                            orp.TargetOrganization.Description = entity.TargetEntityDescription;
                            orp.TargetOrganization.SubjectWebpage = entity.TargetEntitySubjectWebpage;
                            orp.TargetOrganization.ImageUrl = entity.TargetEntityImageUrl;
                        }
                        else if ( entity.TargetEntityTypeId == CodesManager.ENTITY_TYPE_ASSESSMENT_PROFILE )
                        {
                            //orp.TargetAssessment = AssessmentManager.GetBasic( entity.TargetEntityBaseId ?? 0 );
                            orp.TargetAssessment.Id = entity.TargetEntityBaseId ?? 0;
                            orp.TargetAssessment.RowId = entity.TargetEntityUid;
                            orp.TargetAssessment.Name = entity.TargetEntityName;
                            orp.TargetAssessment.Description = entity.TargetEntityDescription;
                            orp.TargetAssessment.SubjectWebpage = entity.TargetEntitySubjectWebpage;
                        }
                        else if ( entity.TargetEntityTypeId == CodesManager.ENTITY_TYPE_LEARNING_OPP_PROFILE )
                        {
                            //orp.TargetLearningOpportunity = LearningOpportunityManager.GetBasic( entity.TargetEntityBaseId ?? 0 );
                            orp.TargetLearningOpportunity.Id = entity.TargetEntityBaseId ?? 0;
                            orp.TargetLearningOpportunity.RowId = entity.TargetEntityUid;
                            orp.TargetLearningOpportunity.Name = entity.TargetEntityName;
                            orp.TargetLearningOpportunity.Description = entity.TargetEntityDescription;
                            orp.TargetLearningOpportunity.SubjectWebpage = entity.TargetEntitySubjectWebpage;
                        }
                    }
                    /* either first one for new target
                     * or change in relationship
                     * or change in role source
                     */

                    if ( prevRoleTypeId == entity.RelationshipTypeId)
                    {
                        if ( prevRoleSource != entity.roleSource )
                        {
                            if ( entity.roleSource == "DirectAssertion" )
                                eitem.IsDirectAssertion = true;
                            else
                                eitem.IsIndirectAssertion = true;

                            //add previous
                            //could get a dup if there is an immediate chg in target, 
                            //orp.AgentRole.Items.Add( eitem );
                            prevRoleSource = entity.roleSource;
                            
                            continue;
                        }
                        
                    } else
                    {
                        //if not equal, add previous, and initialize next one (fall thru)
                        orp.AgentRole.Items.Add( eitem );
                    }
                    //new relationship
                    eitem = new EnumeratedItem
                    {
                        Id = entity.RelationshipTypeId,
                        Name = entity.AgentToSourceRelationship,
                        SchemaName = entity.ReverseSchemaTag
                    };


                    prevRoleTypeId = entity.RelationshipTypeId;
                    prevRoleSource = entity.roleSource;
                    if ( entity.roleSource == "DirectAssertion" )
                        eitem.IsDirectAssertion = true;
                    else
                        eitem.IsIndirectAssertion = true;

                }
                //check for remaining
                if ( IsGuidValid( prevTargetUid ) && orp.AgentRole.Items.Count > 0 )
                {
                    orp.AgentRole.Items.Add( eitem );
                    list.Add( orp );
                }

            }
            return list;

        } //
        /// <summary>
        /// TODO - do we need this method?
        /// Maybe for publishing?
        /// </summary>
        /// <param name="agentUid"></param>
        /// <returns></returns>
        public static List<MPM.OrganizationAssertion> GetAllDirectAssertions( Guid agentUid, int entityTypeId = 0, int assertionTypeId = 0 )
        {
            MPM.OrganizationAssertion oa = new MPM.OrganizationAssertion();
            List<MPM.OrganizationAssertion> list = new List<MPM.OrganizationAssertion>();
            Organization org = OrganizationManager.GetForSummary( agentUid, true );

            using ( var context = new ViewContext() )
            {
                var agentRoles = context.Entity_Assertion_Summary
                    .Where( s => s.AgentUid == agentUid
                        && ( entityTypeId == 0 || s.TargetEntityTypeId == entityTypeId )
                        && ( assertionTypeId == 0 || s.AssertionTypeId == assertionTypeId )
                        )
                    .Distinct()
                    .OrderBy( s => s.TargetEntityTypeId )
                    .ThenBy( s => s.TargetEntityName )
                    .ThenBy( s => s.AgentToTargetRelationship )
                         //.Select( x => new EM.Views.Entity_Assertion_Summary { EntityAssertionId = x.targetEntityBaseId.Value, AgentUid = x.AgentUid, EntityId = x.EntityId, Name = x.targetEntityName, RelationshipDescription = x.RelationshipDescription, AgentDescription = x.targetEntityDescription } ).Distinct()
                         .ToList();
                foreach ( var entity in agentRoles )
                {
                    oa = new MPM.OrganizationAssertion
                    {
                        //warning for purposes of the editor, need to set the agent/object id to the orgId, and rowId from the org
                        Id = entity.EntityAssertionId,
                        OrganizationId = entity.OrgId,
                        TargetOrganization = org,       //just in case
                        RowId = ( Guid )entity.AgentUid,

                        //parent, ex credential, assessment, or org in org-to-org
                        //Hmm should this be the entityId - need to be consistant
                        ParentId = entity.EntityId,

                        Relationship = entity.AgentToTargetRelationship,
                        AssertionTypeId = entity.AssertionTypeId,

                        //useful for compare when doing deletes, and New checks
                        TargetUid = entity.AgentUid,
                        TargetEntityTypeId = entity.TargetEntityTypeId,
                        TargetEntityType = entity.TargetEntityType,
                        TargetEntityBaseId = ( int )entity.TargetEntityBaseId,
                        TargetEntityName = entity.TargetEntityName,
                        TargetEntityDescription = entity.TargetEntityDescription,
                        TargetEntitySubjectWebpage = entity.TargetEntitySubjectWebpage,
                        TargetEntityImageUrl = entity.TargetEntityImageUrl,
                        TargetCTID = entity.TargetCTID,
                        CtdlType = entity.CtdlType
                    };

                    list.Add( oa );
                }
            }
            return list;

        }
        public static List<MPM.OrganizationAssertion> GetAllDirectAssertions( int orgId, int maxRecords = 10 )
        {
            MPM.OrganizationAssertion oa = new MPM.OrganizationAssertion();
            List<MPM.OrganizationAssertion> list = new List<MPM.OrganizationAssertion>();
            Organization org = OrganizationManager.GetForSummary( orgId, true );

            using ( var context = new ViewContext() )
            {
                var agentRoles = context.Entity_Assertion_Summary
                    .Where( s => s.OrgId == orgId )
                    .Distinct()
                    .OrderBy( s => s.TargetEntityTypeId )
                    .ThenBy( s => s.TargetEntityName )
                    .ThenBy( s => s.AgentToTargetRelationship )
                         //.Select( x => new EM.Views.Entity_Assertion_Summary { EntityAssertionId = x.targetEntityBaseId.Value, AgentUid = x.AgentUid, EntityId = x.EntityId, Name = x.targetEntityName, RelationshipDescription = x.RelationshipDescription, AgentDescription = x.targetEntityDescription } ).Distinct()
                         .Take(maxRecords)
                         .ToList();

                foreach ( var entity in agentRoles )
                {
                    oa = new MPM.OrganizationAssertion
                    {
                        //warning for purposes of the editor, need to set the agent/object id to the orgId, and rowId from the org
                        Id = entity.EntityAssertionId,
                        OrganizationId = entity.OrgId,
                        TargetOrganization = org,       //just in case
                        RowId = ( Guid ) entity.AgentUid,

                        //parent, ex credential, assessment, or org in org-to-org
                        //Hmm should this be the entityId - need to be consistant
                        ParentId = entity.EntityId,

                        Relationship = entity.AgentToTargetRelationship,
                        AssertionTypeId = entity.AssertionTypeId,

                        //useful for compare when doing deletes, and New checks
                        TargetUid = entity.AgentUid,
                        TargetEntityTypeId = entity.TargetEntityTypeId,
                        TargetEntityType = entity.TargetEntityType,
                        TargetEntityBaseId = ( int ) entity.TargetEntityBaseId,
                        TargetEntityName = entity.TargetEntityName,
                        TargetEntityDescription = entity.TargetEntityDescription,
                        TargetEntitySubjectWebpage = entity.TargetEntitySubjectWebpage,
                        TargetEntityImageUrl = entity.TargetEntityImageUrl,
                        TargetCTID = entity.TargetCTID,
                        CtdlType = entity.CtdlType
                    };

                    list.Add( oa );
                }
            }
            return list;

        }
        /// <summary>
        /// Get all assertions for detail view
        /// </summary>
        /// <param name="agentUid"></param>
        /// <returns></returns>
        public static List<MPM.OrganizationAssertion> GetAllAssertionsForDetailView( Guid agentUid )
        {
            MPM.OrganizationAssertion oa = new MPM.OrganizationAssertion();
            List<MPM.OrganizationAssertion> list = new List<MPM.OrganizationAssertion>();
            Organization org = OrganizationManager.GetForSummary( agentUid, true );

            using ( var context = new ViewContext() )
            {
                var assertions = context.Entity_AssertionCSV
                    .Where( s => s.AgentUid == agentUid )
                    .OrderBy( s => s.TargetEntityName)
                    .ToList();

                foreach ( var assertion in assertions )
                {
                    list.Add( DBEntity_Fill( assertion, true ) );
                }

            }
            return list;

        }

        /// <summary>
        /// Get all applicable assertions
        /// The assertions will be formatted as enumerations
        /// </summary>
        /// <param name="agentUid"></param>
        /// <param name="entityTypeId"></param>
        /// <returns></returns>
        public static List<MPM.OrganizationAssertion> GetAllAssertionAsEnumerations( Guid agentUid, int entityTypeId = 0 )
        {
            ThisEntity entity = new ThisEntity();
            List<MPM.OrganizationAssertion> list = new List<ThisEntity>();

            using ( var context = new ViewContext() )
            {
                var assertions = context.Entity_AssertionCSV
                    .Where( s => s.AgentUid == agentUid )
                    .ToList();

                foreach (var item in assertions)
                {
                    entity = new ThisEntity();
                    DBEntity_Fill2( entity, assertions, true );
                    list.Add( entity );
                }
               
            }

            return list;
        }

        public static ThisEntity GetAsEnumerationFromCSV( Guid pParentUid, Guid targetUid )
        {
            ThisEntity entity = new ThisEntity();

            Entity parent = EntityManager.GetEntity( pParentUid );

            using ( var context = new ViewContext() )
            {
                //there can be inconsistancies, resulting in more than one.
                //Appears we will need something like Entity.AssertionCSV
                var assertion = context.Entity_AssertionCSV
                    .Where( s => s.AgentUid == pParentUid
                         && s.TargetEntityUid == targetUid )
                    .SingleOrDefault();

                entity = DBEntity_Fill( assertion, true );
                //really should only be one row
                //if ( assertions.Count > 1 )
                //{
                //    //log an exception
                //    //==>NO, there can be multiples with the new format, until stabalized. ex. Owned by, offered by, a QA role
                //    LoggingHelper.LogError( string.Format( "{0}.GetAsEnumerationFromCSV. Multiple records found where one expected. org.RowId: {1}, targetUid: {2}", thisClassName, pParentUid, targetUid ), true );
                //}

            }
            return entity;

        } //
        private static ThisEntity DBEntity_Fill( ViewCsv assertion, bool fillingEnumerations = true )
        {
            //List<ThisEntity> list = new List<ThisEntity>();
            //EnumeratedItem eitem = new EnumeratedItem();
            var entity = new ThisEntity();
            //don't need much for org, as it is the parent, and display should be handled.
            entity.Id = assertion.EntityId;
            entity.OrganizationId = assertion.OrgId;
            entity.RowId = assertion.TargetEntityUid;
            //assertion types is a csv string 
            //entity.AssertionTypeId = assertion.RoleIds;

            entity.TargetEntityType = assertion.TargetEntityType;
            entity.TargetEntityTypeId = assertion.TargetEntityTypeId;
            entity.TargetEntityName = assertion.TargetEntityName;
            entity.ProfileName = assertion.TargetEntityName;
            entity.ProfileSummary = assertion.TargetEntityName;
            entity.TargetUid = assertion.TargetEntityUid;
            entity.TargetEntityDescription = assertion.TargetEntityDescription;
            entity.TargetEntitySubjectWebpage = assertion.TargetEntitySubjectWebpage;
            entity.TargetEntityBaseId = ( int ) assertion.TargetEntityBaseId;
            entity.TargetCTID = assertion.TargetCTID;

            entity.CtdlType = assertion.CtdlType;
            entity.TargetEntityImageUrl = assertion.TargetEntityImageUrl;
            //maybe useful  - OR NOT
            entity.ActedUponEntity = new Entity()
            {
                Id = assertion.EntityId,
                EntityBaseId = ( int )assertion.TargetEntityBaseId,
                EntityUid = assertion.TargetEntityUid,
                RowId = assertion.TargetEntityUid, //??????
                EntityBaseName = assertion.TargetEntityName

            };
            entity.Recipient = new MPM.TargetEntity()
            {
                Id = entity.TargetEntityBaseId,
                Name = entity.TargetEntityName,
                RowId = entity.TargetUid,
                TypeName = entity.CtdlType
            };

            entity.AgentAssertion.Items = new List<EnumeratedItem>();
            if ( fillingEnumerations )
            {
                string[] roles = assertion.RoleIds.Split( ',' );
                string[] rolenames = assertion.Roles.Split( ',' );

                for ( var i = 0; i < roles.Length; i++ )
                {
                    var eitem = new EnumeratedItem();
                    eitem.Id = int.Parse( roles[ i ] );
                    eitem.Name = rolenames[ i ];
                    entity.AgentAssertion.Items.Add( eitem );
                }
            }

            return entity;
        }
        private static void DBEntity_Fill2( ThisEntity entity, List<ViewCsv> assertions, bool fillingEnumerations = true )
        {
            List<ThisEntity> list = new List<ThisEntity>();
            EnumeratedItem eitem = new EnumeratedItem();
            foreach ( ViewCsv item in assertions )
            {

                //don't need much for org, as it is the parent, and display should be handled.
                entity.OrganizationId = item.OrgId;
                entity.RowId = item.AgentUid;
                
                entity.TargetEntityName = item.TargetEntityName;
                entity.TargetEntityDescription = item.TargetEntityDescription;
                entity.TargetEntitySubjectWebpage = item.TargetEntitySubjectWebpage;
                entity.TargetEntityImageUrl = item.TargetEntityImageUrl;
                entity.TargetEntityBaseId = ( int )item.TargetEntityBaseId;
                entity.TargetCTID = item.TargetCTID;
                entity.CtdlType = item.CtdlType;
                //maybe useful  - OR NOT
                entity.ActedUponEntity = new Entity()
                {
                    Id = item.EntityId,
                    EntityBaseId = (int)item.TargetEntityBaseId,
                    EntityUid = item.TargetEntityUid,
                    RowId = item.TargetEntityUid, //??????
                    EntityBaseName = item.TargetEntityName

                };

                entity.AgentAssertion.Items = new List<EnumeratedItem>();
                if ( fillingEnumerations )
                {
                    string[] roles = item.RoleIds.Split( ',' );
                    string[] rolenames = item.Roles.Split( ',' );

                    for ( var i = 0; i < roles.Length; i++ )
                    {
                        eitem = new EnumeratedItem();
                        //??
                        eitem.Id = int.Parse( roles[ i ] );
                        //not used here
                        eitem.Name = rolenames[ i ];
                        eitem.RecordId = int.Parse( roles[ i ] );
                        eitem.CodeId = int.Parse( roles[ i ] );
                        eitem.Value = roles[ i ].Trim();

                        eitem.Selected = true;
                        entity.AgentAssertion.Items.Add( eitem );
                    }
                }

                break;
            }
        }

        #region probably will not use
        /// <summary>
        /// Or use the CSV view?
        /// </summary>
        /// <param name="agentUid"></param>
        /// <param name="entityTypeId"></param>
        /// <returns></returns>
        public static List<MPM.OrganizationAssertion> GetAllAssertionAsEnumerationsOLD( Guid agentUid, int entityTypeId = 0 )
        {
            MPM.OrganizationAssertion oa = new MPM.OrganizationAssertion();
            List<MPM.OrganizationAssertion> list = new List<MPM.OrganizationAssertion>();
            EnumeratedItem eitem = new EnumeratedItem();
            int prevEntityId = 0;
            int prevEntityTypeId = 0;

            using ( var context = new ViewContext() )
            {
                var agentRoles = context.Entity_Assertion_Summary
                    .Where( s => s.AgentUid == agentUid && ( entityTypeId == 0 || s.TargetEntityTypeId == entityTypeId ) )
                    .Distinct()
                    .OrderBy( s => s.TargetEntityTypeId )
                    .ThenBy( s => s.TargetEntityName )
                    .ThenBy( s => s.AgentToTargetRelationship )
                    .ToList();

                foreach ( var entity in agentRoles )
                {
                    //loop until change in agent
                    //watch for chance where same baseId occurs across
                    if ( prevEntityId != entity.TargetEntityBaseId
                        || prevEntityTypeId != entity.TargetEntityTypeId )
                    {
                        if ( prevEntityId > 0 )
                            list.Add( oa );

                        prevEntityId = ( int ) entity.TargetEntityBaseId;
                        prevEntityTypeId = entity.TargetEntityTypeId;

                        oa = new MPM.OrganizationAssertion
                        {
                            //warning for purposes of the editor, need to set the agent/object id to the orgId, and rowId from the org
                            Id = entity.EntityAssertionId,
                            OrganizationId = entity.OrgId,
                            RowId = ( Guid ) entity.AgentUid,

                            //parent, ex credential, assessment, or org in org-to-org
                            //Hmm should this be the entityId - need to be consistant
                            ParentId = entity.EntityId,

                            Relationship = entity.AgentToTargetRelationship,
                            AssertionTypeId = entity.AssertionTypeId,

                            //useful for compare when doing deletes, and New checks
                            TargetUid = entity.AgentUid,
                            TargetEntityTypeId = entity.TargetEntityTypeId,
                            TargetEntityType = entity.TargetEntityType,
                            TargetEntityBaseId = ( int ) entity.TargetEntityBaseId,
                            TargetEntityName = entity.TargetEntityName,
                            TargetEntityDescription = entity.TargetEntityDescription,
                            TargetEntitySubjectWebpage = entity.TargetEntitySubjectWebpage,
                            TargetCTID = entity.TargetCTID,
                            CtdlType = entity.CtdlType,

                            AgentAssertion = CodesManager.GetEnumeration( CodesManager.PROPERTY_CATEGORY_ENTITY_AGENT_ROLE )
                        };

                    }
                    //add assertions
                    eitem = new EnumeratedItem();
                    eitem.Id = entity.AssertionTypeId;
                    eitem.RowId = entity.TargetEntityUid.ToString();
                    eitem.Name = entity.AgentToTargetRelationship;
                    eitem.SchemaName = entity.ReverseSchemaTag;

                    oa.AgentAssertion.Items.Add( eitem );
                }

                //check for remaining
                if ( prevEntityId > 0 )
                    list.Add( oa );
            }
            return list;

        }
        #endregion

     
    }
}
