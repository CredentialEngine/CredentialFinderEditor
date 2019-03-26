using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models;
using Models.Node;
using Models.Node.Interface;
using MC = Models.Common;
using PM = Models.ProfileModels;
using Utilities;
using Factories;

namespace CTIServices
{
    public class EditorServices
    {
        #region Get Methods

        /// <summary>
        /// Get a clientProfile
        /// </summary>
        /// <param name="context"></param>
        /// <param name="skipNewCheck"></param>
        /// <param name="valid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static BaseProfile GetProfile( ProfileContext context, bool skipNewCheck, ref bool valid, ref string status )
        {
            //If new profile, save as necessary
            if ( !skipNewCheck && context.Profile.Id == 0 && context.Profile.RowId == Guid.Empty )
            {
                //default owing agent to that of parent
                if ( context != null && context.Parent != null )
                {
                    //TBD - check the profile to determine if owningOrg is applicable?
                    //test with lopp parts
                    if ( ServiceHelper.IsValidGuid( context.Parent.OwningAgentUid ) )
                        context.Profile.OwningAgentUid = context.Parent.OwningAgentUid;
                    else
                    {
                        if ( context.Parent.TypeName == "Credential" )
                        {
                            //implement a minimal credential:
                            //GetMinimalCredential
                            MC.Credential c = CredentialServices.GetBasicCredential( context.Parent.Id );
                            context.Profile.OwningAgentUid = c.OwningAgentUid;
                        }
                        else if ( context.Parent.TypeName == "Organization"
                            || context.Parent.TypeName == "QAOrganization" )
                        {
                            context.Profile.OwningAgentUid = context.Parent.RowId;
                        }
                        else if ( context.Parent.TypeName == "LearningOpportunity" )
                        {
                            PM.LearningOpportunityProfile l = LearningOpportunityServices.GetForMicroProfile( context.Parent.Id );
                            context.Profile.OwningAgentUid = l.OwningAgentUid;
                        }
                        else if ( context.Parent.TypeName == "Assessment" )
                        {
                            PM.AssessmentProfile a = AssessmentServices.GetBasic( context.Parent.Id );
                            context.Profile.OwningAgentUid = a.OwningAgentUid;
                        }
                        else if ( context.Parent.TypeName == "ConditionManifest" )
                        {
                            MC.ConditionManifest a = ConditionManifestServices.GetForEdit( context.Parent.Id );
                            context.Profile.OwningAgentUid = a.OwningAgentUid;
                        }
                    }
                }

                return SaveNewProfile( context, ref valid, ref status );
            }

            //Otherwise, get existing profile
            var profile = ( BaseProfile ) Activator.CreateInstance( context.Profile.Type );
            object data = new { };

            //TODO check for entity access 
            //How to return errors
            switch ( context.Profile.TypeName )
            {
                case "Credential":
                    data = CredentialServices.GetForEdit( context.Profile.Id, skipNewCheck, ref status );
                    break;
                case "QACredential":
                    data = CredentialServices.GetForEdit( context.Profile.Id, skipNewCheck, ref status );
                    break;
                case "Organization":
                    data = OrganizationServices.GetOrganizationForEdit( context.Profile.Id, skipNewCheck, ref status );
                    break;
                case "QAOrganization":
                    data = OrganizationServices.Get_QAOrganization( context.Profile.Id, skipNewCheck, ref status );
                    break;
                case "Agent_QAPerformed_Credential":
                case "Agent_QAPerformed_Organization":
                case "Agent_QAPerformed_Assessment":
                case "Agent_QAPerformed_Lopp":
                    data = OrganizationServices.GetQARoles_AsEnumeration( context.Parent.RowId, context.Profile.RowId );
                   
                    //model after GetEntityAgentRoles_AsEnumeration, like
                    //data = OrganizationServices.QAPerformed_AsEnumeration( context.Parent.RowId, context.Profile.RowId );
                    break;
                case "Assessment":

                    PM.AssessmentProfile ap = AssessmentServices.GetForEdit( context.Profile.Id, skipNewCheck, ref status );
                    if ( context.Profile.Id == 0 )
                        ap.OwningAgentUid = context.Profile.OwningAgentUid;

                    data = ap;
                    break;
                case "LearningOpportunity":
                    PM.LearningOpportunityProfile lopp = LearningOpportunityServices.GetForEdit( context.Profile.Id, skipNewCheck, ref status );

                    if ( context.Profile.Id == 0 )
                        lopp.OwningAgentUid = context.Profile.OwningAgentUid;
                    data = lopp;
                    break;
                case "DurationProfile":
                case "DurationProfileExact":
                    //Temporary workaround
                    //data = context.Parent.Type == typeof( Credential ) ? 
                    //	CredentialServices.DurationProfile_GetTimeToEarn( context.Profile.Id ) :
                    data = CredentialServices.DurationProfile_Get( context.Profile.Id );
                    break;
                case "JurisdictionProfile_QA":
                case "JurisdictionProfile":
                    data = new JurisdictionServices().Get( context.Profile.RowId );
                    break;
                case "AgentRoleProfile_Recipient":
                case "AgentRoleProfile_Assets":
                case "AgentRoleProfile_OfferedBy":
                    data = OrganizationServices.GetEntityAgentRoles_AsEnumeration( context.Parent.RowId, context.Profile.RowId );
                    break;
                case "Agent_QAPerformed":
                    break;
                case "QualityAssuranceActionProfile_Recipient":
                    //data = CredentialServices.GetCredentialQARole( context.Parent.Id, context.Profile.Id );

                    data = OrganizationServices.QualityAssuranceAction_GetProfile( context.Parent.RowId, context.Profile.Id );
                    break;
                case "QualityAssuranceActionProfile_Actor":
                    break;
                //case "ConditionProfileOLD":

                //	if ( context.Parent.Type == typeof( Credential ) )
                //		data = CredentialServices.ConditionProfile_GetForEdit( context.Profile.Id, true );
                //	else
                //		data = ConditionProfileServices.ConditionProfile_GetForEdit( context.Profile.Id );
                //	break;
                case "ConditionProfile":

                    data = ConditionProfileServices.ConditionProfile_GetForEdit( context.Profile.Id );

                    break;
                case "ConditionManifest":

                    data = ConditionManifestServices.GetForEdit( context.Profile.Id );
                    break;
                case "CostManifest":

                    data = CostManifestServices.GetForEdit( context.Profile.Id );
                    break;
                case "RevocationProfile":
                    data = CredentialServices.RevocationProfile_GetForEdit( context.Profile.Id );
                    break;
                case "FinancialAlignmentObject":

                    data = ProfileServices.FinancialAlignmentProfile_Get( context.Profile.Id );
                    break;
                //case "TaskProfile":
                //	data = ConditionProfileServices.TaskProfile_Get( context.Profile.Id );
                //	break;
                case "CostProfile":
                    data = ProfileServices.CostProfile_Get( context.Profile.Id );
                    break;
                case "CostItemProfile":
                    data = ProfileServices.CostProfileItem_GetForEdit( context.Profile.Id );
                    break;
                //case "TextValueProfile": //N-A
                //	break;
                case "CredentialAlignmentObjectProfile":
                    //data = ProfileServices.CredentialAlignmentObject_Get( context.Profile.Id );
                    break;
                case "CredentialAlignmentObjectFrameworkProfileXXX": //old competencues
                    //data = ProfileServices.CredentialAlignmentObjectFrameworkProfile_Get( context.Profile.Id );
                    break;
                case "CredentialAlignmentObjectItemProfile": //OBSOLETE
					//data = ProfileServices.CredentialAlignmentObjectItemProfile_Get( context.Profile.Id );
                    break;
                case "AddressProfile":
                    data = ProfileServices.AddressProfile_Get( context.Profile.Id );
                    break;
                case "ContactPoint":
                    data = ProfileServices.ContactPoint_Get( context.Profile.Id );
                    break;
                case "ProcessProfile":
                    data = ProfileServices.ProcessProfile_Get( context.Profile.Id );
                    break;
                case "VerificationServiceProfile":
                    data = ProfileServices.VerificationServiceProfile_GetForEdit( context.Profile.Id );
                    break;
                case "VerificationStatus":
                    //data = ProfileServices.VerificationStatus_Get( context.Profile.Id );
                    data = OrganizationServices.VerificationStatus_Get( context.Profile.Id );
                    break;
                case "EarningsProfile":
                    break;
                case "EmploymentOutcomeProfile":
                    break;
                case "HoldersProfile":
                    break;
                default:
                    return new BaseProfile() { Id = context.Profile.Id, RowId = context.Profile.RowId, Name = context.Profile.Name };
            }

            ConvertToClientProfile( profile, data );
            return profile;
        }
        //

        /// <summary>
        /// Convert a DB entity to a Profile
        /// </summary>
        /// <param name="clientProfile"></param>
        /// <param name="serverProfile"></param>
        public static void ConvertToClientProfile( object clientProfile, object serverProfile )
        {
            //OtherList is to handle enumerations with an "other" text box
            var otherList = new Dictionary<string, string>();

            foreach ( var clientProperty in clientProfile.GetType().GetProperties() )
            {
                try
                {
                    //Handle other value
                    //Done before any other clientProperty matching takes place, since Other does not have an equivalently named clientProperty
                    //Pass-by-reference keeps this up to date
                    if ( clientProperty.Name == "Other" )
                    {
                        clientProperty.SetValue( clientProfile, otherList );
                    }
                    if ( clientProperty.Name == "HasCostManifest" )
                    {

                    }
                    //Get the attributes for this clientProperty, or set to defaults
                    var attributes = ( Property ) clientProperty.GetCustomAttributes( typeof( Property ), true ).FirstOrDefault() ?? new Property();

                    //Get convenience variables
                    //Value of the property
                    var clientPropertyValue = clientProperty.GetValue( clientProfile );
                    //Name of the property that matches its database name
                    var name = string.IsNullOrWhiteSpace( attributes.DBName ) ? clientProperty.Name : attributes.DBName;
                    //Server property itself
                    var serverProperty = serverProfile.GetType().GetProperties().FirstOrDefault( m => m.Name == name );
                    //Value of the server property
                    var serverPropertyValue = serverProperty.GetValue( serverProfile );

                    //If an explicit conversion method is set, use it
                    if ( !string.IsNullOrWhiteSpace( attributes.LoadMethod ) )
                    {
                        //Have to get method from a string since the models can't reference the service since that would create a circular reference
                        var method = typeof( EditorServices ).GetMethod( attributes.LoadMethod );
                        clientProperty.SetValue( clientProfile, method.Invoke( null, new object[] { serverPropertyValue, attributes } ) );
                    }

                    //If the two properties are simple types, just carry the value over
                    else if ( clientProperty.PropertyType == serverProperty.PropertyType )
                    {
                        clientProperty.SetValue( clientProfile, serverPropertyValue );
                    }

                    //If the clientProperty is a checkbox/dropdown list, handle it - and other values
                    else if ( clientProperty.PropertyType == typeof( int ) || clientProperty.PropertyType == typeof( List<int> ) )
                    {
                        var otherValue = "";
                        var list = LoadIdList( serverPropertyValue, attributes, ref otherValue );

                        if ( clientProperty.PropertyType == typeof( int ) )
                        {
                            clientProperty.SetValue( clientProfile, list.Count() > 0 ? list.First() : 0 );
                        }
                        else
                        {
                            clientProperty.SetValue( clientProfile, list );
                        }

                        if ( !string.IsNullOrWhiteSpace( otherValue ) )
                        {
                            otherList.Add( clientProperty.Name, otherValue );
                        }
                    }

                    //Duration clientProfile requires special handling because of differences between its class structure and database structure
                    else if ( clientProperty.PropertyType == typeof( DurationItem ) )
                    {
                        ConvertToClientProfile( clientPropertyValue, serverPropertyValue );
                    }

                    //TextValueProfile requires special handling
                    else if ( clientProperty.PropertyType == typeof( TextValueProfile ) )
                    {
                        ConvertToClientProfile( clientPropertyValue, serverPropertyValue );
                    }

                    //TextValueProfile requires special handling
                    else if ( clientProperty.PropertyType == typeof( List<TextValueProfile> ) )
                    {
                        var items = ( List<PM.TextValueProfile> ) serverPropertyValue;
                        var result = new List<TextValueProfile>();
                        foreach ( var item in items )
                        {
                            var newItem = new TextValueProfile();
                            ConvertToClientProfile( newItem, item );
                            result.Add( newItem );
                        }

                        clientProperty.SetValue( clientProfile, result );
                    }

                    //Otherwise, convert to a ProfileLink
                    else if ( clientProperty.PropertyType == typeof( ProfileLink ) )
                    {
                        clientProperty.SetValue( clientProfile, LoadProfileLink( serverPropertyValue, attributes ) );
                    }

                    //Or to a List<ProfileLink
                    else if ( clientProperty.PropertyType == typeof( List<ProfileLink> ) )
                    {
                        clientProperty.SetValue( clientProfile, LoadProfileLinkList( serverPropertyValue, attributes ) );
                    }

                    else
                    {
                    }
                }
                catch ( Exception ex )
                {
                    //apparantly ignoring - chg to trace to avoid confusion
                    LoggingHelper.DoTrace( 9, string.Format( "EditorServices.ConvertToClientProfile(), clientProperty: {0}  \\r\\n", clientProperty ) + ex.Message, false );
                }
            } //foreach
        }
        //

        /// <summary>
        /// Get a ProfileLink from an object
        /// </summary>
        /// <param name="serverPropertyValue"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static ProfileLink LoadProfileLink( object serverPropertyValue, Property attributes )
        {
            try
            {
                if ( attributes.DBType == typeof( Guid ) )
                {
                    return new ProfileLink() { RowId = ( Guid ) serverPropertyValue, Name = "Entity by GUID" };
                }
                else if ( attributes.DBType == typeof( int ) )
                {
                    return new ProfileLink() { Id = ( int ) serverPropertyValue, Name = "Entity by ID" };
                }
                else
                {
                    var data = serverPropertyValue as dynamic;

                    return new ProfileLink() { Id = data.Id, Name = GetName( serverPropertyValue ), RowId = data.RowId, Type = attributes.Type };
                }
            }
            catch { }

            return new ProfileLink();
        }
        //

        /// <summary>
        /// Get a List<ProfileLink> from an object that is internally a list
        /// </summary>
        /// <param name="serverPropertyValue"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public static List<ProfileLink> LoadProfileLinkList( object serverPropertyValue, Property attributes )
        {
            try
            {
                //Handle enumerations a little differently
                if ( attributes.DBType == typeof( Models.Common.Enumeration ) )
                {
                    var data = ( serverPropertyValue as Models.Common.Enumeration ).Items;
                    return data.ConvertAll( m => new ProfileLink()
                    {
                        Id = m.Id,
                        RowId = string.IsNullOrWhiteSpace( m.RowId ) ? new Guid() : Guid.Parse( m.RowId ),
                        Name = string.IsNullOrWhiteSpace( m.ItemSummary ) ? m.Name : m.ItemSummary,
                        Type = attributes.Type
                    } );
                }
                else
                {
                    //Special handling for role profiles
                    var isReceivedRole = attributes.DBType == typeof( AgentRoleProfile_Recipient );
                    var isActorRole = attributes.DBType == typeof( AgentRoleProfile_Actor );
                    var data = ( serverPropertyValue as IEnumerable<object> ).Cast<dynamic>().ToList();
                    return data.ConvertAll( m => new ProfileLink()
                    {
                        Id = m.Id,
                        RowId = isReceivedRole ? m.ActingAgentUid ?? new Guid() : isActorRole ? m.ActedUponEntityUid ?? new Guid() : m.RowId ?? new Guid(),
                        Name = GetName( m ),
                        Type = attributes.Type
                    } );
                }

            }
            catch { }
            return new List<ProfileLink>();
        }
        //

        //Try to determine a name
        public static string GetName( object data )
        {
            var properties = data.GetType().GetProperties();
            var name = "";
            foreach ( var property in properties )
            {
                if ( property.Name == "Name" || property.Name == "ProfileSummary" || property.Name == "ProfileName" )
                {
                    if ( string.IsNullOrWhiteSpace( name ) )
                    {
                        name = ( string ) property.GetValue( data );
                    }
                }
            }

            return name;
        }

        //Get a List<int> from an enumeration or code item list(?)
        public static List<int> LoadIdList( object serverPropertyValue, Property attributes, ref string otherValue )
        {
            try
            {
                //Try to process it as an enumeration
                var data = ( serverPropertyValue as Models.Common.Enumeration );
                otherValue = data.OtherValue;
                return data.Items.Select( m => m.Id ).ToList();
            }
            catch { }
            try
            {
                //Otherwise, try to get it dynamically
                var data = ( serverPropertyValue as IEnumerable<object> ).Cast<dynamic>().ToList();
                return data.Select<dynamic, int>( m => m.Id ).ToList();
            }
            catch { }
            return new List<int>();
        }
        //

        #endregion

        #region Save Methods
        /// <summary>
        /// Save a new Profile - use to create an immediate entity so that child components can be added without prompting the user to save.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="valid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static BaseProfile SaveNewProfile( ProfileContext context, ref bool valid, ref string status )
        {
            //BaseProfile profile = new BaseProfile();
            //Otherwise, get existing profile
            //NOT USED!!!
            //profile = ( BaseProfile ) Activator.CreateInstance( context.Profile.Type );
            object data = new { };

            //Get the user
            var user = AccountServices.GetUserFromSession();

            context.Profile.Id = 0;

            //TODO check for entity access 
            //How to return errors
            switch ( context.Profile.TypeName )
            {
                case "Credential":
                    break;
                case "QACredential":
                    break;
                case "Organization":
                    break;
                case "QAOrganization":
                    break;
                case "Assessment":
                    break;
                case "LearningOpportunity":
                    break;
                case "DurationProfile":
                case "DurationProfileExact":
                    break;
                case "JurisdictionProfile_QA":
                case "JurisdictionProfile":
                    break;

                case "ConditionProfile":
                {
                    //*** ENTER  PROFILE NAME ***
                    var entity = new PM.ConditionProfile() { ProfileName = "     " };
                    entity.IsStarterProfile = true;
                    entity.ConnectionProfileType = context.Profile.Property;
					if ( entity.ConnectionProfileType == "CredentialConnections" ||
						entity.ConnectionProfileType == "AssessmentConnections"  ||
						entity.ConnectionProfileType == "LearningOppConnections" )
					{
						entity.ConnectionProfileTypeId = 3;
					}
					else
					{
						//set the type based on the property
						entity.ConnectionProfileTypeId = ConditionProfileServices.GetConditionTypeId( entity.ConnectionProfileType );
					}
                    //this could be used?
                    entity.IsStarterProfile = true;
                    entity.ConditionSubTypeId = ConditionProfileServices.SetConditionSubTypeId( entity.ConnectionProfileType );


                    entity.AssertedByAgentUid = context.Profile.OwningAgentUid;

                    if ( new ConditionProfileServices().ConditionProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status ) )
                    {

                        context.Profile.Id = entity.Id;
                        context.Profile.RowId = entity.RowId;
                        context.Profile.Name = entity.ProfileName;
                    }
                    break;
                }
				case "ConditionProfileConnection":
				{
					//*** ENTER  PROFILE NAME ***
					var entity = new PM.ConditionProfile() { ProfileName = "     " };
					entity.IsStarterProfile = true;
					entity.ConnectionProfileType = context.Profile.Property;
					//set the type based on the property
					entity.ConnectionProfileTypeId = ConditionProfileServices.GetConditionTypeId( entity.ConnectionProfileType );

					//this could be used?
					entity.IsStarterProfile = true;
					entity.ConditionSubTypeId = ConditionProfileServices.SetConditionSubTypeId( entity.ConnectionProfileType );


					entity.AssertedByAgentUid = context.Profile.OwningAgentUid;

					if ( new ConditionProfileServices().ConditionProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status ) )
					{

						context.Profile.Id = entity.Id;
						context.Profile.RowId = entity.RowId;
						context.Profile.Name = entity.ProfileName;
					}
					break;
				}

				case "ConditionManifest": //force active save now
                    break;

                case "ConditionManifestXXX":
                    {
                        var entity = new MC.ConditionManifest() { ProfileName = "     " };

                        entity.IsStarterProfile = true;

                        entity.OwningAgentUid = context.Profile.OwningAgentUid;

                        if ( new ConditionManifestServices().Save( entity, context.Parent.RowId, "Initial", user, ref status ) )
                        {

                            context.Profile.Id = entity.Id;
                            context.Profile.RowId = entity.RowId;
                            context.Profile.Name = "new condition manifest";//entity.Name;
                        }
                        break;
                    }
                case "CostManifest": //force active save now
                    break;
                case "CostManifestXXX":
                    {
                        var entity = new MC.CostManifest() { ProfileName = "     " };

                        entity.IsStarterProfile = true;

                        entity.OwningAgentUid = context.Profile.OwningAgentUid;

                        if ( new CostManifestServices().Save( entity, context.Parent.RowId, "Initial", user, ref status ) )
                        {

                            context.Profile.Id = entity.Id;
                            context.Profile.RowId = entity.RowId;
                            context.Profile.Name = "new cost manifest";//entity.Name;
                        }
                        break;
                    }
                case "FinancialAlignmentObject":
                    break;
                case "RevocationProfile":
                    {
                        var entity = new PM.RevocationProfile() { ProfileName = " " };
                        entity.IsStarterProfile = true;

                        if ( new CredentialServices().RevocationProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status, true ) )
                        {
                            //data = CredentialServices.RevocationProfile_GetForEdit( entity.Id );
                            context.Profile.Id = entity.Id;
                            context.Profile.RowId = entity.RowId;
                            context.Profile.Name = entity.ProfileName;
                        }

                        break;
                    }
                case "CostProfile": //force active save now
                    break;
                case "CostProfileXXX":
                    {
                        //int newId = 0;
                        var entity = new PM.CostProfile() { ProfileName = " " };
                        entity.IsStarterProfile = true;
                        if ( new ProfileServices().CostProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status ) )
                        {
                            //data = ProfileServices.CostProfile_Get( newId );
                            context.Profile.Id = entity.Id;
                            context.Profile.RowId = entity.RowId;
                            context.Profile.Name = entity.ProfileName;
                        }

                    }
                    break;
                //case "CostItemProfile":  //	N/A
                //	break;
                //case "CredentialAlignmentObjectProfile":
                //	break;

				//obsolete
                case "CredentialAlignmentObjectFrameworkProfileXXX":
                    {
                        //var entity = new MC.CredentialAlignmentObjectFrameworkProfile();
                        //entity.EducationalFrameworkName = "";
                        //entity.AlignmentType = context.Profile.Property;
                        //entity.IsStarterProfile = true;
                        //if ( new ProfileServices().CredentialAlignmentObjectFrameworkProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status ) )
                        //{
                        //    context.Profile.Id = entity.Id;
                        //    context.Profile.RowId = entity.RowId;
                        //    context.Profile.Name = entity.EducationalFrameworkName;
                        //}
                        //else
                        //    valid = false;

                        break;
                    }
                case "CredentialAlignmentObjectItemProfile":
                    break;
                case "AddressProfile":
                    break;
                case "ContactPoint":
                    break;
                case "ProcessProfile1": //force active save now
                    break;
                case "ProcessProfile":
                    {
                        var entity = new PM.ProcessProfile();
                        entity.ProfileName = " ";
                        entity.IsStarterProfile = true;
                        //map the process type
                        entity.ProcessProfileType = context.Profile.Property;
                        entity.ProcessTypeId = ProfileServices.DetermineProcessProfileTypeId( entity.ProcessProfileType );
                        entity.ProcessingAgentUid = context.Profile.OwningAgentUid;
                        entity.IsStarterProfile = true;

                        if ( new ProfileServices().ProcessProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status, true ) )
                        {
                            context.Profile.Id = entity.Id;
                            context.Profile.RowId = entity.RowId;
                            context.Profile.Name = entity.ProfileName;
                        }

                        break;
                    }
                case "VerificationServiceProfile":
                    {
					var entity = new PM.VerificationServiceProfile();
					entity.ProfileName = "*** new profile ***";
					entity.Description = "please add a meaningful description";
					entity.IsStarterProfile = true;
					if ( new ProfileServices().VerificationServiceProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status, true ) )
					{
						context.Profile.Id = entity.Id;
						context.Profile.RowId = entity.RowId;
						context.Profile.Name = entity.ProfileName;
					}

					break;
                    }
                case "VerificationStatus":
                    break;
                case "EarningsProfile":
                    break;
                case "EmploymentOutcomeProfile":
                    break;
                case "HoldersProfile":
                    break;

                default:
                    return new BaseProfile() { Id = context.Profile.Id, RowId = context.Profile.RowId, Name = context.Profile.Name };
            }

            return GetProfile( context, true, ref valid, ref status );
        }


        /// <summary>
        /// Save a Profile (via add or update, as appropriate)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="clientProfile"></param>
        /// <param name="valid"></param>
        /// <param name="status"></param>
        /// <param name="isReferenceVersion">Initially should only for an add? Probably will need something for updates</param>
        /// <returns></returns>
        public static BaseProfile SaveProfile( ProfileContext context, BaseProfile clientProfile, ref bool valid, ref string status, bool isReferenceVersion )
        {

            //Get the user
            var user = AccountServices.GetUserFromSession();

            //Determine which type of server profile to initialize
            var profile = Activator.CreateInstance( context.Profile.Type );
            var attribute = ( Profile ) profile.GetType().GetCustomAttributes( typeof( Profile ), true ).FirstOrDefault() ?? new Profile();
            var serverType = attribute.DBType;

            //Convert from client profile to server profile
            var serverProfile = Activator.CreateInstance( serverType );
            ConvertToServerProfile( serverProfile, clientProfile );

            //Save the server profile
            //If new, call the Add method
            if ( context.Profile.Id == 0 )
            {
                return AddProfile( context, serverProfile, user, ref valid, ref status, isReferenceVersion );
            }
            //If existing, call the Update method
            else
            {
                return UpdateProfile( context, serverProfile, user, ref valid, ref status );
            }
        }
        //

        //Add a new profile
        private static BaseProfile AddProfile( ProfileContext context,
                object serverProfile,
                Models.AppUser user,
                ref bool valid,
                ref string status,
                bool isReferenceVersion,
                bool isEmpty = false )
        {
            var id = 0;
            var rowID = new Guid();
            string property = context.Profile.Property;

            switch ( context.Profile.TypeName )
            {
                case "Credential":
                    {
                        var profile = ( MC.Credential ) serverProfile;
                        profile.IsReferenceVersion = isReferenceVersion;
                        id = new CredentialServices().Add( profile, user, ref valid, ref status );
                        rowID = profile.RowId;
                        break;
                    }
                case "QACredential":
                    {
                        var profile = ( MC.Credential ) serverProfile;
                        profile.IsReferenceVersion = isReferenceVersion;
                        id = new CredentialServices().Add( profile, user, ref valid, ref status );
                        rowID = profile.RowId;
                        break;
                    }
                case "Organization":
                    {
                        //EnrichData( serverProfile, OrganizationServices.GetOrganization( context.Profile.Id ) );
                        var profile = ( MC.Organization ) serverProfile;
                        profile.IsReferenceVersion = isReferenceVersion;
                        id = new OrganizationServices().Add( profile, user, ref valid, ref status );
                        rowID = profile.RowId;
                    }
                    break;
                case "QAOrganization":
                    {
                        var profile = ( MC.QAOrganization ) serverProfile;
                        profile.IsReferenceVersion = isReferenceVersion;
                        id = new OrganizationServices().Add_QAOrg( profile, user, ref valid, ref status );
                        rowID = profile.RowId;
                    }
                    break;
                case "Assessment":
                    {
                        //EnrichData( serverProfile, AssessmentServices.Get( context.Profile.Id ) );
                        var profile = ( PM.AssessmentProfile ) serverProfile;
                        profile.IsReferenceVersion = isReferenceVersion;
                        id = new AssessmentServices().Add( profile, user, ref valid, ref status );
                        rowID = profile.RowId;
                    }
                    break;
                case "LearningOpportunity":
                    {
                        //EnrichData( serverProfile, LearningOpportunityServices.Get( context.Profile.Id ) );
                        var profile = ( PM.LearningOpportunityProfile ) serverProfile;
                        profile.IsReferenceVersion = isReferenceVersion;
                        id = new LearningOpportunityServices().Add( profile, user, ref valid, ref status );
                        rowID = profile.RowId;
                    }
                    break;
                case "DurationProfile":
                    {
                        var profile = ( PM.DurationProfile ) serverProfile;

                        //LoggingHelper.DoTrace( 2, "EditorServices. Add DurationProfile. userId: " + user.Id.ToString() );
                        valid = new CredentialServices().DurationProfile_Update( profile, context.Parent.RowId, context.Main.RowId, user, ref status, "Add" );
                        //}
                        id = profile.Id;
                        rowID = profile.RowId;
                    }
                    break;
                case "DurationProfileExact":
                    {
                        var profile = ( PM.DurationProfile ) serverProfile;
                        profile.DurationProfileTypeId = 3;
                        //LoggingHelper.DoTrace( 2, "EditorServices. Add DurationProfile. userId: " + user.Id.ToString() );
                        valid = new CredentialServices().DurationProfile_Update( profile, context.Parent.RowId, context.Main.RowId, user, ref status );
                        //}
                        id = profile.Id;
                        rowID = profile.RowId;
                    }
                    break;
                case "JurisdictionProfile_QA":
                case "JurisdictionProfile":
                    {
                        //EnrichData( serverProfile, new JurisdictionServices().Get( context.Profile.RowId ) );
                        var profile = ( MC.JurisdictionProfile ) serverProfile;
                        if ( new Factories.Entity_JurisdictionProfileManager().IsEmpty( profile ) )
                        {
                            profile.Description = "Auto-saved Jurisdiction";
                        }
                        int jprofilePurposeId = 1;
                        if ( context.Profile.Property == "Region"
                            || context.Profile.Property == "Residency" )
                            jprofilePurposeId = 2;
                        else if ( context.Profile.Property == "JurisdictionAssertions" )
                            jprofilePurposeId = 3;
                        else
                            jprofilePurposeId = 1;
                        valid = new JurisdictionServices().JurisdictionProfile_Add( profile, context.Parent.RowId, jprofilePurposeId, property, user, ref status );
                        id = profile.Id;
                        rowID = profile.RowId;
                    }
                    break;
                case "AgentRoleProfile_Recipient":
                    {
                        var profile = ( PM.OrganizationRoleProfile ) serverProfile;

                        //Assign acting agent
                        profile.ActingAgentId = profile.ActingAgent.Id;
                        profile.ActingAgentUid = profile.ActingAgent.RowId;
                        profile.ParentUid = context.Parent.RowId;

                        if ( context.Main.TypeName == "Organization" )
                            property = "OrganizationQARole";
                        //might be able to use for all - where allowing multiple roles
                        valid = new OrganizationServices().EntityAgentRole_Save( profile, profile.ParentUid, property, user, false, ref status );

                        //new CredentialServices().Credential_SaveOrgRole( profile, profile.TargetCredentialId, user, ref status );
                        //not applicable yet - a particular rowId is not partinent, as can be multiple roles
                        //want to just get the whole enumerated object
                        id = profile.ActingAgent.Id;
                        rowID = profile.ActingAgent.RowId;

                        //force the agent id into the context.Profile.Id for the GetProfile
                        context.Profile.Id = profile.ActingAgent.Id;
                        break;
                    }

                case "Agent_QAPerformed_Credential":
                case "Agent_QAPerformed_Organization":
                case "Agent_QAPerformed_Assessment":
                case "Agent_QAPerformed_Lopp":
                    {
                        var profile = ( PM.OrganizationAssertion ) serverProfile;

                        property = context.Profile.Property;

                        //might be able to use for all - where allowing multiple roles
                        valid = new OrganizationServices().EntityAssertionRole_Save( profile, context.Parent.RowId, property, user, false, ref status );

                        //new CredentialServices().Credential_SaveOrgRole( profile, profile.TargetCredentialId, user, ref status );
                        //not applicable yet - a particular rowId is not partinent, as can be multiple roles
                        //want to just get the whole enumerated object
                        id = profile.TargetEntityBaseId;
                        //rowID = assertion.TargetUid;

                        //force the agent id into the context.Profile.Id for the GetProfile
                        rowID = profile.TargetUid;
                        context.Profile.Id = profile.TargetEntityBaseId;
                        break;
                    }

                case "QualityAssuranceActionProfile_Recipient":
                    {
                        var profile = ( PM.QualityAssuranceActionProfile ) serverProfile;

                        //Assign acting agent = > may not be necessary, check
                        profile.ActingAgentId = profile.ActingAgent.Id;
                        profile.ActingAgentUid = profile.ActingAgent.RowId;
                        profile.IssuedCredentialId = profile.IssuedCredential.Id;

                        //Assign target
                        string parentType = context.Parent.TypeName;
                        switch ( context.Parent.TypeName )
                        {
                            case "Credential": profile.TargetCredentialId = context.Parent.Id; break;
                            case "Organization": profile.TargetOrganizationId = context.Parent.Id; break;
                            case "Assessment": profile.TargetAssessmentId = context.Parent.Id; break;
                            //case "LearningOpportunity": profile.TargetLearningOpportunityId = context.Parent.Id; break; //Apparently does not exist?
                            default: break;
                        }

                        new OrganizationServices().QualityAssuranceAction_SaveProfile( profile, context.Parent.RowId, user, ref status );

                        //new CredentialServices().Credential_SaveQAOrgRole( profile, profile.TargetCredentialId, user, ref status );

                        id = profile.Id;
                        rowID = profile.RowId;

                        break;
                    }
                case "Agent_QAPerformed":
                    {
                        var profile = ( PM.OrganizationRoleProfile ) serverProfile;

                        //Assign acting agent
                        profile.ActingAgentId = profile.ActingAgent.Id;
                        profile.ActingAgentUid = profile.ActingAgent.RowId;
                        profile.ParentUid = context.Parent.RowId;

                        //?????
                        if ( context.Main.TypeName == "Organization" )
                            property = "OrganizationQARole";

                        valid = new OrganizationServices().EntityAgentRole_Save( profile, profile.ParentUid, property, user, true, ref status );
                        //not sure of action on success
                        id = profile.ActingAgent.Id;
                        rowID = profile.ActingAgent.RowId;

                        //force the agent id into the context.Profile.Id for the GetProfile
                        context.Profile.Id = profile.ActingAgent.Id;
                        break;
                    }

                case "HasConditionProfile":
                case "ConditionProfile":
                    {
                        var profile = ( PM.ConditionProfile ) serverProfile;
                        profile.ConnectionProfileType = context.Profile.Property;
                        profile.ConditionSubTypeId = ConditionProfileServices.SetConditionSubTypeId( profile.ConnectionProfileType );

                        valid = new ConditionProfileServices().ConditionProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );

                        id = profile.Id;
                        rowID = profile.RowId;
                    }
                    break;

                case "ConditionManifest":
                    {
                        var entity = ( MC.ConditionManifest ) serverProfile;

                        //var entity = new MC.ConditionManifest() { ProfileName = "     " };

                        //this could be used?
                        //entity.IsStarterProfile = true;

                        entity.OwningAgentUid = context.Profile.OwningAgentUid;

                        if ( new ConditionManifestServices().Save( entity, context.Parent.RowId, "Initial", user, ref status ) )
                        {
                            id = entity.Id;
                            rowID = entity.RowId;
                        }
                        break;
                    }

                case "CostManifest":
                    {
                        var entity = ( MC.CostManifest ) serverProfile;
                        //var entity = new MC.CostManifest() { ProfileName = "     " };

                        //this could be used?
                        //entity.IsStarterProfile = true;

                        entity.OwningAgentUid = context.Profile.OwningAgentUid;

                        if ( new CostManifestServices().Save( entity, context.Parent.RowId, "Initial", user, ref status ) )
                        {
                            id = entity.Id;
                            rowID = entity.RowId;
                        }
                        break;
                    }
                case "FinancialAlignmentObject":
                    {
                        var profile = ( MC.FinancialAlignmentObject ) serverProfile;

                        valid = new ProfileServices().FinancialAlignmentProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );

                        id = profile.Id;
                        rowID = profile.RowId;
                        break;
                    }

                case "RevocationProfile":
                    {
                        var profile = ( PM.RevocationProfile ) serverProfile;
                        valid = new CredentialServices().RevocationProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
                        id = profile.Id;
                        rowID = profile.RowId;
                    }
                    break;

                //case "TaskProfile":
                //	{
                //		var profile = ( PM.TaskProfile ) serverProfile;
                //		valid = new ConditionProfileServices().TaskProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
                //		id = profile.Id;
                //		rowID = profile.RowId;
                //		break;
                //	}
                case "CostProfile":
                    {
                        var profile = ( PM.CostProfile ) serverProfile;
                        valid = new ProfileServices().CostProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
                        id = profile.Id;
                        rowID = profile.RowId;
                    }
                    break;
                case "CostItemProfile":
                    {
                        var profile = ( PM.CostProfileItem ) serverProfile;
                        valid = new ProfileServices().CostProfileItem_Save( profile, context.Parent.RowId, "Add", user, ref status );
                        id = profile.Id;
                        rowID = profile.RowId;
                    }
                    break;
                case "TextValueProfile": //		N/A
                    {

                        break;
                    }
                //case "CredentialAlignmentObjectProfile":
                //	{
                //		var profile = ( MC.CredentialAlignmentObjectProfile ) serverProfile;
                //		profile.AlignmentType = context.Profile.Property;
                //		valid = new ProfileServices().CredentialAlignmentObject_Save( profile, context.Parent.RowId, "Add", user, ref status );
                //		id = profile.Id;
                //		rowID = profile.RowId;
                //	}
                //	break;
                case "CredentialAlignmentObjectFrameworkProfileXXX":
                    //{
                    //    var profile = ( MC.CredentialAlignmentObjectFrameworkProfile ) serverProfile;
                    //    profile.ParentId = context.Parent.Id;
                    //    profile.AlignmentType = context.Profile.Property;

                    //    valid = new ProfileServices().CredentialAlignmentObjectFrameworkProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
                    //    id = profile.Id;
                    //    rowID = profile.RowId;
                    //}
                    break;
                case "CredentialAlignmentObjectItemProfile": //OBSOLETE
				{
                        //for competency
                        //var profile = ( MC.CredentialAlignmentObjectItemProfile ) serverProfile;
                        //profile.ParentId = context.Parent.Id;
                        //valid = new ProfileServices().CredentialAlignmentObjectItemProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
                        //id = profile.Id;
                        //rowID = profile.RowId;
                    }
                    break;
                case "AddressProfile":
                    {
                        //
                        var profile = ( MC.Address ) serverProfile;
                        valid = new ProfileServices().AddressProfile_Save( profile, context.Parent.RowId, "Add", context.Parent.TypeName, user, ref status );
                        id = profile.Id;
                        rowID = profile.RowId;
                    }
                    break;
                case "ContactPoint":
                    {
                        //
                        var profile = ( MC.ContactPoint ) serverProfile;
                        valid = new ProfileServices().ContactPoint_Save( profile, context.Parent.RowId, "Add", user, ref status );
                        id = profile.Id;
                        rowID = profile.RowId;
                    }
                    break;
                case "ProcessProfile":
                    {
                        var profile = ( PM.ProcessProfile ) serverProfile;
                        //map the process type
                        profile.ProcessProfileType = context.Profile.Property;
                        //???
                        profile.ProcessTypeId = 0;
                        valid = new ProfileServices().ProcessProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
                        id = profile.Id;
                        rowID = profile.RowId;
                        break;
                    }
                case "VerificationServiceProfile":
                    {
                        var profile = ( PM.VerificationServiceProfile ) serverProfile;
                        valid = new ProfileServices().VerificationServiceProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
                        id = profile.Id;
                        rowID = profile.RowId;
                        break;
                    }
                case "VerificationStatus":
                    {
                        var profile = ( PM.VerificationStatus ) serverProfile;
                        valid = new OrganizationServices().VerificationStatus_Save( profile, context.Parent.RowId, "Add", user, ref status );
                        id = profile.Id;
                        rowID = profile.RowId;
                        break; //
                    }
                case "EarningsProfile":
                    {

                        break;
                    }
                case "EmploymentOutcomeProfile":
                    {

                        break;
                    }
                case "HoldersProfile":
                    {

                        break;
                    }
                case "StarterProfile":
                    {
                        //Methods called below should initialize the relevant profile and auto-associate it as if it were selected via microsearch
                        //Cast to StarterProfile and override the context with the selected type
                        var profile = ( Models.Node.StarterProfile ) serverProfile;
                        context.Profile.TypeName = profile.ProfileType;

                        //Call the add method as if it were a normal add
                        var added = SaveProfile( context, ( BaseProfile ) profile, ref valid, ref status, isReferenceVersion );

                        //Make the association
                        //Note that Profile.Property is available to help determine the proper method to call
                        switch ( profile.SearchType )
                        {
                            case "CredentialSearch":

                                break;
                            case "OrganizationSearch":

                                break;
                            //case "AssessmentSearchOLD":
                            //	new CredentialServices().ConditionProfile_AddAsmt( context.Parent.Id, added.Id, user, ref valid, ref status );
                            //	break;
                            case "AssessmentSearch":
                                new ProfileServices().Assessment_Add( context.Parent.RowId, context.Main.RowId, added.Id, user, ref valid, ref status );
                                break;
                            //case "LearningOpportunitySearchOLD":
                            //	new CredentialServices().ConditionProfile_AddLearningOpportunity( context.Parent.Id, added.Id, user, ref valid, ref status );
                            //	break;
                            case "LearningOpportunitySearch":
                                new ProfileServices().LearningOpportunity_Add( context.Parent.RowId, context.Main.RowId, added.Id, user, ref valid, ref status );
                                break;
                            case "LearningOpportunityHasPartSearch":
                                //TODO - can we get rowId instead?
                                Guid rowId = context.Parent.RowId;
                                new LearningOpportunityServices().AddLearningOpportunity_AsPart( context.Parent.RowId, added.Id, user, ref valid, ref status );
                                break;
                            default: break;
                        }

                        //Return immediately
                        return new BaseProfile() { Id = added.Id, RowId = added.RowId, Name = added.Name };
                    }

                default:
                    {
                        valid = false;
                        status = "This profile is not handled yet, check back later";
                        return new BaseProfile() { Id = id, RowId = rowID, Name = context.Profile.Name };
                    }
            }

            //If ID > 0, get and return the profile
            if ( id > 0 )
            {
                context.Profile.Id = id;
                context.Profile.RowId = rowID;
                context.Main.Id = context.IsTopLevel ? id : context.Main.Id;
                return GetProfile( context, true, ref valid, ref status );
            }

            return null;
        }
        //

        /// <summary>
        /// Update an existing profile
        /// re: reference entities, shouldn't be updating these?
        /// If so, would have to use the starter profiles
        /// </summary>
        /// <param name="context"></param>
        /// <param name="serverProfile"></param>
        /// <param name="user"></param>
        /// <param name="valid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private static BaseProfile UpdateProfile( ProfileContext context, object serverProfile, Models.AppUser user, ref bool valid, ref string status )
        {
            string property = context.Profile.Property;

            switch ( context.Profile.TypeName )
            {
                case "Credential":
                    EnrichData( serverProfile, CredentialServices.GetBasicCredential( context.Profile.Id ) );
                    valid = new CredentialServices().Save( ( MC.Credential ) serverProfile, user, ref status );
                    break;
                case "QACredential":
                    EnrichData( serverProfile, CredentialServices.GetBasicCredential( context.Profile.Id ) );
                    valid = new CredentialServices().Save( ( MC.Credential ) serverProfile, user, ref status );
                    break;
                case "Organization":
                    EnrichData( serverProfile, OrganizationServices.GetForSummary( context.Profile.Id ) );
                    var org = ( MC.Organization ) serverProfile;
                    //org.IsNewVersion = true;
                    valid = new OrganizationServices().Update( org, user, ref status );
                    break;

                case "QAOrganization":
                    EnrichData( serverProfile, OrganizationServices.GetForSummary( context.Profile.Id ) );
                    var qaorg = ( MC.QAOrganization ) serverProfile;

                    valid = new OrganizationServices().Update_QAOrg( qaorg, user, ref status );
                    break;
                case "Assessment":
                    EnrichData( serverProfile, AssessmentServices.Get( context.Profile.Id ) );
                    var asmt = ( PM.AssessmentProfile ) serverProfile;
                    //asmt.IsNewVersion = true;
                    valid = new AssessmentServices().Update( ( PM.AssessmentProfile ) serverProfile, user, ref status );
                    break;
                case "LearningOpportunity":
                    EnrichData( serverProfile, LearningOpportunityServices.Get( context.Profile.Id ) );
                    var lopp = ( PM.LearningOpportunityProfile ) serverProfile;
                    //lopp.IsNewVersion = true;
                    valid = new LearningOpportunityServices().Update( ( PM.LearningOpportunityProfile ) serverProfile, "", user, ref status );
                    break;
                case "DurationProfile":
                    var originalData = CredentialServices.DurationProfile_Get( context.Profile.Id );
                    EnrichData( originalData, serverProfile );
                    valid = new CredentialServices().DurationProfile_Update( ( PM.DurationProfile ) serverProfile, context.Parent.RowId, context.Main.RowId, user, ref status );

                    break;
                case "DurationProfileExact":
                    {
                        var profile = CredentialServices.DurationProfile_Get( context.Profile.Id );
                        profile.DurationProfileTypeId = 3;
                        EnrichData( profile, serverProfile );
                        valid = new CredentialServices().DurationProfile_Update( ( PM.DurationProfile ) serverProfile, context.Parent.RowId, context.Main.RowId, user, ref status );
                    }
                    break;
                case "JurisdictionProfile_QA":
                case "JurisdictionProfile":
                    var js = new JurisdictionServices();
                    EnrichData( serverProfile, js.Get( context.Profile.Id ) );
                    valid = js.JurisdictionProfile_Update( ( MC.JurisdictionProfile ) serverProfile, context.Parent.RowId, property, AccountServices.GetUserFromSession(), ref status );
                    break;
                case "Agent_QAPerformed_Credential":
                case "Agent_QAPerformed_Organization":
                case "Agent_QAPerformed_Assessment":
                case "Agent_QAPerformed_Lopp":
                    {
                        var profile = ( PM.OrganizationAssertion ) serverProfile;
                        //Assign acting agent fields
                        //profile.ActingAgentId = profile.ActingAgent.Id;
                        //profile.ActingAgentUid = profile.ActingAgent.RowId;
                        profile.AgentUid = context.Parent.RowId;

                        property = context.Profile.Property;

                        //might be able to use for all - where allowing multiple roles
                        valid = new OrganizationServices().EntityAssertionRole_Save( profile, profile.AgentUid, property, user, false, ref status );

                        //necessary????
                        context.Profile.Id = profile.Id;
                        break;
                    }
                case "AgentRoleProfile_Recipient":
                    {
                        var profile = ( PM.OrganizationRoleProfile ) serverProfile;
                        //Assign acting agent fields
                        profile.ActingAgentId = profile.ActingAgent.Id;
                        profile.ActingAgentUid = profile.ActingAgent.RowId;
                        profile.ParentUid = context.Parent.RowId;

                        if ( context.Main.TypeName == "Organization" )
                            property = "OrganizationQARole";
                        //might be able to use for all - where allowing multiple roles
                        valid = new OrganizationServices().EntityAgentRole_Save( profile, profile.ParentUid, property, user, false, ref status );


                        //necessary????
                        context.Profile.Id = profile.ActingAgent.Id;
                        break;
                    }
                //case "AgentRoleProfile_Assets":
                //case "AgentRoleProfile_OfferedBy":
                //	{
                //		var profile = ( PM.OrganizationRoleProfile ) serverProfile;
                //		//Assign acting agent fields
                //		profile.ActingAgentId = profile.ActingAgent.Id;
                //		profile.ActingAgentUid = profile.ActingAgent.RowId;
                //		profile.ParentUid = context.Parent.RowId;


                //		//might be able to use for all - where allowing multiple roles
                //		valid = new OrganizationServices().CredentialAssets_EntityAgentRole_Save( profile, profile.ParentUid, property, user, ref status );


                //		//necessary????
                //		context.Profile.Id = profile.ActingAgent.Id;
                //		break;
                //	}
                case "QualityAssuranceActionProfile_Recipient":
                    {
                        var profile = ( PM.QualityAssuranceActionProfile ) serverProfile;

                        //Assign acting agent
                        profile.ParentUid = context.Parent.RowId;
                        profile.ActingAgentId = profile.ActingAgent.Id;
                        profile.ActingAgentUid = profile.ActingAgent.RowId;

                        profile.ActedUponEntityUid = profile.ActedUponEntity.EntityUid;
                        profile.ActedUponEntityId = profile.ActedUponEntity.Id;

                        profile.IssuedCredentialId = profile.IssuedCredential.Id;
                        profile.ParticipantAgentUid = profile.ParticipantAgent.RowId;

                        valid = new OrganizationServices().QualityAssuranceAction_SaveProfile( profile, profile.ParentUid, user, ref status );

                        //valid = new CredentialServices().Credential_SaveQAOrgRole( profile, profile.TargetCredentialId, user, ref status );

                        //force the agent id into the context.Profile.Id for the GetProfile
                        //context.Profile.Id = profile.ActingAgent.Id;
                        break;
                    }
                case "Agent_QAPerformed":
                    {
                        var profile = ( PM.OrganizationRoleProfile ) serverProfile;

                        //Assign acting agent
                        profile.ActingAgentId = profile.ActingAgent.Id;
                        profile.ActingAgentUid = profile.ActingAgent.RowId;
                        profile.ParentUid = context.Parent.RowId;

                        //?????
                        if ( context.Main.TypeName == "Organization" )
                            property = "OrganizationQARole";

                        valid = new OrganizationServices().EntityAgentRole_Save( profile, profile.ParentUid, property, user, true, ref status );

                        //force the agent id into the context.Profile.Id for the GetProfile
                        //TODO - determine if necessary
                        context.Profile.Id = profile.ActingAgent.Id;
                        break;
                    }
                case "QualityAssuranceActionProfile_Actor":
                    break;

                case "ConditionProfile":
                    {
                        var profile = ( PM.ConditionProfile ) serverProfile;
                        profile.ConnectionProfileType = context.Profile.Property;

                        valid = new ConditionProfileServices().ConditionProfile_Save( profile, context.Parent.RowId, "Update", user, ref status );
                    }
                    break;
                case "ConditionManifest":
                    {
                        var profile = ( MC.ConditionManifest ) serverProfile;

                        valid = new ConditionManifestServices().Save( profile, context.Parent.RowId, "Update", user, ref status );
                    }
                    break;
                case "CostManifest":
                    {
                        var profile = ( MC.CostManifest ) serverProfile;

                        valid = new CostManifestServices().Save( profile, context.Parent.RowId, "Update", user, ref status );
                    }
                    break;
                case "FinancialAlignmentObject":
                    {
                        var profile = ( MC.FinancialAlignmentObject ) serverProfile;

                        valid = new ProfileServices().FinancialAlignmentProfile_Save( profile, context.Parent.RowId, "Update", user, ref status );
                        break;
                    }
                case "RevocationProfile":
                    {
                        var profile = ( PM.RevocationProfile ) serverProfile;
                        valid = new CredentialServices().RevocationProfile_Save( profile, context.Parent.RowId, "Update", user, ref status );
                    }
                    break;

                //case "TaskProfile":
                //	{
                //		var profile = ( PM.TaskProfile ) serverProfile;
                //		valid = new ConditionProfileServices().TaskProfile_Save( profile, context.Parent.RowId, "Update", user, ref status );
                //	}
                //	break;
                case "CostProfile":
                    {
                        var profile = ( PM.CostProfile ) serverProfile;
                        valid = new ProfileServices().CostProfile_Save( profile, context.Parent.RowId, "Update", user, ref status );
                    }
                    break;
                case "CostItemProfile":
                    {
                        var profile = ( PM.CostProfileItem ) serverProfile;
                        valid = new ProfileServices().CostProfileItem_Save( profile, context.Parent.RowId, "Update", user, ref status );
                    }
                    break;
                //case "CredentialAlignmentObjectProfile":
                //	{
                //		var profile = ( MC.CredentialAlignmentObjectProfile ) serverProfile;
                //		profile.AlignmentType = context.Profile.Property;
                //		valid = new ProfileServices().CredentialAlignmentObject_Save( profile, context.Parent.RowId, "Update", user, ref status );
                //	}
                //	break;
                case "CredentialAlignmentObjectFrameworkProfileXXX":
                    //{
                    //    var profile = ( MC.CredentialAlignmentObjectFrameworkProfile ) serverProfile;
                    //    profile.AlignmentType = context.Profile.Property;
                    //    valid = new ProfileServices().CredentialAlignmentObjectFrameworkProfile_Save( profile, context.Parent.RowId, "Update", user, ref status );
                    //}
                    break;
                case "CredentialAlignmentObjectItemProfile": //OBSOLETE
				//{
    //                    var profile = ( MC.CredentialAlignmentObjectItemProfile ) serverProfile;
    //                    profile.ParentId = context.Parent.Id;
    //                    valid = new ProfileServices().CredentialAlignmentObjectItemProfile_Save( profile, context.Parent.RowId, "Update", user, ref status );
    //                }
                    break;
                case "AddressProfile":
                    {
                        var profile = ( MC.Address ) serverProfile;
                        valid = new ProfileServices().AddressProfile_Save( profile, context.Parent.RowId, "Update", context.Parent.TypeName, user, ref status );
                    }
                    break;
                case "ContactPoint":
                    {
                        var profile = ( MC.ContactPoint ) serverProfile;
                        valid = new ProfileServices().ContactPoint_Save( profile, context.Parent.RowId, "Update", user, ref status );
                    }
                    break;
                case "ProcessProfile":
                    {
                        var profile = ( PM.ProcessProfile ) serverProfile;
                        //map the process type
                        profile.ProcessProfileType = context.Profile.Property;

                        valid = new ProfileServices().ProcessProfile_Save( profile, context.Parent.RowId, "Update", user, ref status );
                    }
                    break;
                case "VerificationServiceProfile":
                    {
                        var profile = ( PM.VerificationServiceProfile ) serverProfile;
                        valid = new ProfileServices().VerificationServiceProfile_Save( profile, context.Parent.RowId, "Update", user, ref status );
                    }
                    break;
                case "VerificationStatus":
                    {
                        //
                        var profile = ( PM.VerificationStatus ) serverProfile;
                        valid = new OrganizationServices().VerificationStatus_Save( profile, context.Parent.RowId, "Update", user, ref status );
                        //var profile = ( PM.VerificationStatus ) serverProfile;
                        //valid = new ProfileServices().VerificationStatus_Save( profile, context.Parent.RowId, "Update", user, ref status );
                    }
                    break;
                case "EarningsProfile":
                    break;
                case "EmploymentOutcomeProfile":
                    break;
                case "HoldersProfile":
                    break;
                case "TextValueProfile":
                    break;

                default:
                    return new BaseProfile() { Id = context.Profile.Id, RowId = context.Profile.RowId, Name = context.Profile.Name };
            }

            if ( valid )
            {
                //TODO - do we need an indicator of a get after a save?
                //		- for example, this call may assume get for edit!!
                return GetProfile( context, true, ref valid, ref status );
            }
            return null;
        }
        //

        //Convert a client profile to a server profile
        public static void ConvertToServerProfile( object serverProfile, object clientProfile )
        {
            var clientProperties = clientProfile.GetType().GetProperties();
            foreach ( var clientProperty in clientProperties )
            {
                try
                {
                    if ( clientProperty.Name == "Recipient" )
                    {

                    }
                    //Get the attributes for this clientProperty, or set to defaults
                    var attributes = ( Property ) clientProperty.GetCustomAttributes( typeof( Property ), true ).FirstOrDefault() ?? new Property();

                    //Get convenience variables
                    //Value of the property
                    var clientPropertyValue = clientProperty.GetValue( clientProfile );
                    //Name of the property that matches its database name
                    var name = string.IsNullOrWhiteSpace( attributes.DBName ) ? clientProperty.Name : attributes.DBName;
                    //Server property itself
                    var serverProperty = serverProfile.GetType().GetProperties().FirstOrDefault( m => m.Name == name );
                    //Value of the server property
                    var serverPropertyValue = serverProperty.GetValue( serverProfile );

                    if ( name.ToLower().Contains( "status" ) )
                    {

                    }

                    //If an explicit conversion method is set, use it
                    if ( !string.IsNullOrWhiteSpace( attributes.SaveMethod ) )
                    {
                        //Have to get method from a string since the models can't reference the service since that would create a circular reference
                        var method = typeof( EditorServices ).GetMethod( attributes.SaveMethod );
                        serverProperty.SetValue( serverProfile, method.Invoke( null, new object[] { clientPropertyValue, attributes } ) );
                    }

                    //If the two properties are simple types, just carry the value over
                    else if ( serverProperty.PropertyType == clientProperty.PropertyType )
                    {
                        serverProperty.SetValue( serverProfile, clientPropertyValue );
                    }

                    //If the clientProperty is a checkbox/dropdown list, handle it - and other values
                    else if (
                        ( clientProperty.PropertyType == typeof( List<int> ) || clientProperty.PropertyType == typeof( int ) ) &&
                        attributes.DBType == typeof( MC.Enumeration ) )
                    {
                        //Get other value, if any
                        var otherValue = "";
                        try
                        {
                            var otherValues = ( Dictionary<string, string> ) clientProperties.FirstOrDefault( m => m.Name == "Other" ).GetValue( clientProfile );
                            otherValue = otherValues.FirstOrDefault( m => m.Key == clientProperty.Name ).Value;
                        }
                        catch { }

                        //Set value
                        serverProperty.SetValue( serverProfile, SaveIdList( clientPropertyValue, serverPropertyValue, clientProperty.PropertyType, otherValue ) );
                    }

                    //Duration clientProfile requires special handling because of differences between its class structure and database structure
                    else if ( clientProperty.PropertyType == typeof( DurationItem ) )
                    {
                        ConvertToServerProfile( serverPropertyValue, clientPropertyValue );
                    }

                    //TextValueProfile requires special handling
                    else if ( clientProperty.PropertyType == typeof( TextValueProfile ) )
                    {
                        ConvertToServerProfile( serverPropertyValue, clientPropertyValue );
                    }

                    //TextValueProfile requires special handling
                    else if ( clientProperty.PropertyType == typeof( List<TextValueProfile> ) )
                    {
                        var items = ( List<TextValueProfile> ) clientPropertyValue;
                        var result = new List<PM.TextValueProfile>();
                        foreach ( var item in items )
                        {
                            var newItem = new PM.TextValueProfile();
                            ConvertToServerProfile( newItem, item );
                            result.Add( newItem );
                        }

                        serverProperty.SetValue( serverProfile, result );
                    }

                    //If the value needs to be converted from a ProfileLink
                    else if ( clientProperty.PropertyType == typeof( ProfileLink ) )
                    {
                        SaveProfileLink( ( ProfileLink ) clientPropertyValue, attributes, serverProfile, serverProperty );
                    }

                }
                catch ( Exception ex )
                {

                }
            }
        }
        //

        //Convert a list of IDs or a single ID to an enumeration with Items where the items' IDs match the original input
        private static MC.Enumeration SaveIdList( object clientPropertyValue, object serverPropertyValue, Type clientPropertyType, string otherValue )
        {
            //Type casting
            var enumeration = ( MC.Enumeration ) serverPropertyValue; //Need this to get category ID
            var ids = clientPropertyType == typeof( int ) ? new List<int>() { ( int ) clientPropertyValue } : ( List<int> ) clientPropertyValue; //Ensure value is a list

            foreach ( var item in ids )
            {
                if ( enumeration.Items.FirstOrDefault( m => m.Id == item ) == null ) //Duplicate check
                {
                    enumeration.Items.Add( new MC.EnumeratedItem()
                    {
                        Id = item,
                        Selected = true
                    } );
                }
            }

            enumeration.OtherValue = otherValue;

            return enumeration;
        }
        //

        private static void SaveProfileLink( ProfileLink value, Property attributes, object serverProfile, System.Reflection.PropertyInfo serverProperty )
        {
            if ( attributes.DBType == typeof( Guid ) )
            {
                serverProperty.SetValue( serverProfile, value.RowId );
            }
            else if ( attributes.DBType == typeof( int ) )
            {
                serverProperty.SetValue( serverProfile, value.Id );
            }
            //Really want to avoid this! Avoiding recursion is the entire purpose of the ProfileLink system!
            else if ( attributes.SaveAsProfile == true )
            {
                var newItem = Activator.CreateInstance( serverProperty.PropertyType );
                var newProps = newItem.GetType().GetProperties();

                try { newProps.FirstOrDefault( m => m.Name == "Id" ).SetValue( newItem, value.Id ); } catch { }
                try { newProps.FirstOrDefault( m => m.Name == "RowId" ).SetValue( newItem, value.RowId ); } catch { }

                serverProperty.SetValue( serverProfile, newItem );
            }
        }
        //

        //Add missing database-related data that isn't carried in by the client object
        private static void EnrichData( object serverProfile, object source )
        {
            //Update Base Object data specifically - don't want to add other data
            var serverProfileProperties = serverProfile.GetType().GetProperties();
            var sourceProperties = source.GetType().GetProperties();

            foreach ( var basic in typeof( MC.BaseObject ).GetProperties() )
            {
                try
                {
                    if ( basic.Name == "DateEffective" ) //Skip this, since it's set in the metadata editor
                    {
                        continue;
                    }

                    //Match up the properties and copy the value over
                    var serverProfileProperty = serverProfileProperties.FirstOrDefault( m => m.Name == basic.Name );
                    var sourceProperty = sourceProperties.FirstOrDefault( m => m.Name == basic.Name );
                    serverProfileProperty.SetValue( serverProfile, sourceProperty.GetValue( source ) );
                }
                catch { }
            }

            //Update/fix missing IDs and Schemas from Enumerations
            foreach ( var property in serverProfileProperties.Where( m => m.PropertyType == typeof( MC.Enumeration ) ).ToList() )
            {
                try
                {
                    var matchedProperty = sourceProperties.FirstOrDefault( m => m.Name == property.Name );
                    var sourceEnumeration = ( MC.Enumeration ) matchedProperty.GetValue( source ); //Source enumeration
                    var serverEnumeration = ( MC.Enumeration ) property.GetValue( serverProfile ); //Server enumeration - with Items already filled out
                    var enumerationCodes = new EnumerationServices().GetEnumeration( sourceEnumeration.SchemaName ); //Enumeration codes for the current enumeration

                    //Copy enumeration data
                    serverEnumeration.Id = sourceEnumeration.Id;
                    serverEnumeration.SchemaName = sourceEnumeration.SchemaName;

                    //Attempt to preserve Item data
                    foreach ( var item in serverEnumeration.Items )
                    {
                        var matchedItem = enumerationCodes.Items.FirstOrDefault( m => m.Id == item.Id );
                        if ( matchedItem != null )
                        {
                            item.RowId = matchedItem.RowId;
                            item.RecordId = matchedItem.RecordId;
                            item.Created = matchedItem.Created;
                            item.CreatedById = matchedItem.CreatedById;
                            item.ParentId = matchedItem.ParentId;
                            item.Name = matchedItem.Name;
                            item.SchemaName = matchedItem.SchemaName;
                        }
                    }

                    //Set value
                    property.SetValue( serverProfile, serverEnumeration );
                }
                catch { }
            }

        }

        #endregion

        #region Delete Methods

        //Delete a profile
        public static void DeleteProfile( ProfileContext context, ref bool valid, ref string status )
        {
            //Get the user
            var user = AccountServices.GetUserFromSession();

            //Do the delete
            switch ( context.Profile.TypeName )
            {
                case "Credential":
                    valid = new CredentialServices().Delete( context.Profile.Id, user, ref status );
                    break;
                case "QACredential":
                    valid = new CredentialServices().Delete( context.Profile.Id, user, ref status );
                    break;
                case "Organization":
                    //called method checks for authorization
                    valid = new OrganizationServices().Organization_Delete( context.Profile.Id, user, ref status );
                    break;
                case "Assessment":
                    valid = new AssessmentServices().Delete( context.Profile.Id, user, ref status );
                    break;
                case "LearningOpportunity":
                    valid = new LearningOpportunityServices().Delete( context.Profile.Id, user, ref status );
                    break;
                case "DurationProfile":
                case "DurationProfileExact":
                    //valid = context.Parent.Type == typeof( Credential ) ?
                    //	new CredentialServices().DurationProfile_DeleteTimeToEarn( context.Profile.Id, ref status ) :
                    valid = new CredentialServices().DurationProfile_Delete( context.Profile.Id, ref status );
                    break;
                case "JurisdictionProfile_QA":
                case "JurisdictionProfile":
                    valid = new JurisdictionServices().JurisdictionProfile_Delete( context.Profile.Id, ref status );
                    break;
                case "AgentRoleProfile_Recipient":
                case "AgentRoleProfile_Assets":
                case "AgentRoleProfile_OfferedBy":

                    valid = new OrganizationServices().EntityAgent_DeleteAgentRoles( context.Main.RowId, context.Profile.RowId, user, ref status );
                    break;
                case "Agent_QAPerformed":
                    break;
                case "Agent_QAPerformed_Credential":
                case "Agent_QAPerformed_Organization":
                case "Agent_QAPerformed_Assessment":
                case "Agent_QAPerformed_Lopp":
                    valid = OrganizationServices.QAOrganization_Delete( context.Parent.RowId, context.Profile.RowId, user, ref status );
                    break;

                case "QualityAssuranceActionProfile_Recipient":
                    //valid = new CredentialServices().Credential_DeleteQAOrgRoles( context.Main.Id, context.Profile.Id, user, ref status );

                    valid = new OrganizationServices().QualityAssuranceAction_DeleteProfile( context.Main.Id, context.Profile.RowId, user, ref status );
                    break;
                case "QualityAssuranceActionProfile_Actor":
                    break;
                //case "ConditionProfileOLD":
                //	if ( context.Parent.Type == typeof( Credential ) )
                //		valid = new CredentialServices().ConditionProfile_Delete( context.Main.Id, context.Profile.Id, user, ref status );
                //	else
                //		valid = new ConditionProfileServices().ConditionProfile_Delete( context.Main.RowId, context.Profile.Id, user, ref status );
                //	break;
                case "ConditionProfile":
                    valid = new ConditionProfileServices().ConditionProfile_Delete( context.Main.RowId, context.Profile.Id, user, ref status );
                    break;
                case "ConditionManifest":
                    valid = new ConditionManifestServices().Delete( context.Main.RowId, context.Profile.Id, user, ref status );
                    break;
                case "CostManifest":
                    valid = new CostManifestServices().Delete( context.Main.RowId, context.Profile.Id, user, ref status );
                    break;
                case "RevocationProfile":
                    valid = new CredentialServices().RevocationProfile_Delete( context.Main.Id, context.Profile.Id, user, ref status );
                    break;
                case "FinancialAlignmentObject":
                    valid = new ProfileServices().FinancialAlignmentProfile_Delete( context.Main.RowId, context.Profile.Id, user, ref status );
                    break;
                //case "TaskProfile":
                //	valid = new ConditionProfileServices().TaskProfile_Delete( context.Main.RowId, context.Profile.Id, user, ref status );
                //	break;
                case "CostProfile":
                    valid = new ProfileServices().CostProfile_Delete( context.Profile.Id, user, ref status );
                    break;
                case "CostItemProfile":
                    valid = new ProfileServices().CostProfileItem_Delete( context.Profile.Id, user, ref status );
                    break;
                case "TextValueProfile":
                    valid = new ProfileServices().Entity_Reference_Delete( context.Parent.Id, context.Profile.Id, user, ref status );
                    break;
                //case "CredentialAlignmentObjectProfile":
                //	valid = new ProfileServices().CredentialAlignmentObject_Delete( context.Main.Id, context.Profile.Id, user, ref status );
                //	break;
                case "CredentialAlignmentObjectFrameworkProfileXXX": //old competencues
																  
					//NOTE - NEED TO USE context.Parent.RowId in this case, need to confirm other uses of context.Main.RowId
					//valid = new ProfileServices().CredentialAlignmentObjectFrameworkProfile_Delete( context.Parent.RowId, context.Profile.Id, user, ref status );
                    break;
                case "CredentialAlignmentObjectItemProfileXXX": //OBSOLETE
					//valid = new ProfileServices().Entity_Competency_Delete( context.Main.Id, context.Profile.Id, user, ref status );
                    break;
                case "AddressProfile":
                    valid = new ProfileServices().AddressProfile_Delete( context.Main.RowId, context.Profile.Id, context.Parent.TypeName, user, ref status );
                    break;
                case "ContactPoint":
                    valid = new ProfileServices().ContactPoint_Delete( context.Parent.RowId, context.Profile.Id, user, ref status );
                    break;
                case "ProcessProfile":
                    valid = new ProfileServices().ProcessProfile_Delete( context.Main.Id, context.Profile.Id, user, ref status );
                    break;
                case "VerificationServiceProfile":
                    valid = new ProfileServices().VerificationServiceProfile_Delete( context.Main.Id, context.Profile.Id, user, ref status );
                    break;
                case "VerificationStatus":
                    valid = new OrganizationServices().VerificationStatus_Delete( context.Parent.Id, context.Profile.Id, user, ref status );
                    //
                    break;
                case "EarningsProfile":
                    break;
                case "EmploymentOutcomeProfile":
                    break;
                case "HoldersProfile":
                    break;

                default:
                    valid = false;
                    status = "Unable to determine target profile";
                    break;
            }
        }
        //

        #endregion

        #region Publish Methods

        /// <summary>
        /// Register a profile
        /// </summary>
        /// <param name="context"></param>
        /// <param name="valid"></param>
        /// <param name="status"></param>
        public static void RegisterEntity( ProfileContext context, ref bool valid, ref string status )
        {
            //Get the user
            var user = AccountServices.GetUserFromSession();
            List<SiteActivity> list = new List<SiteActivity>();

            //Do the register
            switch ( context.Profile.TypeName )
            {
                case "Credential":
                case "CredentialProfile":
                    valid = new RegistryServices().PublishCredential( context.Profile.Id, user, ref status, ref list );
                    break;
                case "QACredential":
                    valid = new RegistryServices().PublishCredential( context.Profile.Id, user, ref status, ref list );
                    break;
                case "Organization":
                case "QAOrganization":
                    //called method checks for authorization
                    valid = new RegistryServices().PublishOrganization( context.Profile.Id, user, ref status, ref list );
                    break;
                case "AssessmentProfile":
                case "Assessment":
                    //called method checks for authorization
                    valid = new RegistryServices().PublishAssessment( context.Profile.Id, user, ref status, ref list );
                    break;
                case "LearningOpportunityProfile":
                case "LearningOpportunity":
                    //called method checks for authorization
                    valid = new RegistryServices().PublishLearningOpportunity( context.Profile.Id, user, ref status, ref list );
                    break;
                default:
                    valid = false;
                    status = "Profile not handled";
                    break;
            }
        }

        public static void UnregisterEntity( ProfileContext context, ref bool valid, ref string status )
        {
            //Get the user
            var user = AccountServices.GetUserFromSession();
            List<SiteActivity> list = new List<SiteActivity>();

            //Do the delete
            switch ( context.Profile.TypeName )
            {
                case "Credential":
                case "CredentialProfile":
                case "QACredential":
                    valid = new RegistryServices().Unregister_Credential( context.Profile.Id, user, ref status, ref list );
                    break;
                case "Organization":
                case "QAOrganization":
                    //called method checks for authorization
                    valid = new RegistryServices().Unregister_Organization( context.Profile.Id, user, ref status, ref list );
                    break;
                case "AssessmentProfile":
                case "Assessment":
                    valid = new RegistryServices().Unregister_Assessment( context.Profile.Id, user, ref status, ref list );
                    break;
                case "LearningOpportunityProfile":
                case "LearningOpportunity":
                    //called method checks for authorization
                    valid = new RegistryServices().Unregister_LearningOpportunity( context.Profile.Id, user, ref status, ref list );
                    break;
                case "ConditionManifest":
                    //called method checks for authorization
                    valid = new RegistryServices().Unregister_ConditionManifest( context.Profile.Id, user, ref status, ref list );
                    break;
                case "CostManifest":
                    //called method checks for authorization
                    valid = new RegistryServices().Unregister_CostManifest( context.Profile.Id, user, ref status, ref list );
                    break;
                default:
                    valid = false;
                    status = "Profile not handled";
                    break;
            }
        }
        #endregion

        #region approvals

        public static void ApproveEntity( ProfileContext context, ref bool valid, ref bool isPublished, ref string status, bool sendEmailOnSuccess )
        {
            //bool isPublished = false;
            //Get the user
            var user = AccountServices.GetUserFromSession();

            //Do we have rowId? 
            if ( ServiceHelper.IsValidGuid( context.Profile.RowId ) )
            {
                if ( new ProfileServices().Entity_Approval_Save( context.Profile.TypeName, context.Profile.RowId, user, ref isPublished, ref status, sendEmailOnSuccess ) )
                {

                }
            }
            else
            {
                if ( new ProfileServices().Entity_Approval_Save( context.Profile.TypeName, context.Profile.Id, user, ref isPublished, ref status, sendEmailOnSuccess ) )
                {

                }
            }

        } //

        public static void UnApproveEntity( ProfileContext context, ref bool valid, ref string status )
        {
            //Get the user
            var user = AccountServices.GetUserFromSession();

            //Do we have rowId?
            if ( ServiceHelper.IsValidGuid( context.Profile.RowId ) )
                new ProfileServices().Entity_Approval_Delete( context.Profile.RowId, user, ref status );
            else
                new ProfileServices().Entity_Approval_Delete( context.Profile.TypeName, context.Profile.Id, user, ref status );

        }
        #endregion
    }
    //
}
