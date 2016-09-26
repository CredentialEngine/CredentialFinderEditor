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
					data = CredentialServices.GetCredential( context.Profile.Id, true, true );
					break;
				case "Organization":
					data = OrganizationServices.GetOrganization( context.Profile.Id, true );
					break;
				case "Assessment":
					data = AssessmentServices.GetForEdit( context.Profile.Id );
					break;
				case "LearningOpportunity":
					data = LearningOpportunityServices.GetForEdit( context.Profile.Id );
					break;
				case "DurationProfile":
					//Temporary workaround
					//data = context.Parent.Type == typeof( Credential ) ? 
					//	CredentialServices.DurationProfile_GetTimeToEarn( context.Profile.Id ) :
						data = CredentialServices.DurationProfile_Get( context.Profile.Id );
					break;
				case "JurisdictionProfile":
					data = new JurisdictionServices().Get( context.Profile.RowId );
					break;
				case "AgentRoleProfile_Recipient":
					//data = CredentialServices.GetCredentialOrgRoles_AsEnumeration( context.Parent.Id, context.Profile.Id );

					data = OrganizationServices.GetEntityAgentRoles_AsEnumeration( context.Parent.RowId, context.Profile.RowId );
					break;
				case "AgentRoleProfile_Actor":
					break;
				case "QualityAssuranceActionProfile_Recipient":
					//data = CredentialServices.GetCredentialQARole( context.Parent.Id, context.Profile.Id );

					data = OrganizationServices.QualityAssuranceAction_GetProfile( context.Parent.RowId, context.Profile.Id );
					break;
				case "QualityAssuranceActionProfile_Actor":
					break;
				case "ConditionProfileOLD":
					
					if ( context.Parent.Type == typeof( Credential ) )
						data = CredentialServices.ConditionProfile_GetForEdit( context.Profile.Id, true );
					else
						data = ConditionProfileServices.ConditionProfile_GetForEdit( context.Profile.Id );
					break;
				case "ConditionProfile":

					data = ConditionProfileServices.ConditionProfile_GetForEdit( context.Profile.Id );
					break;
				case "RevocationProfile":
					data = CredentialServices.RevocationProfile_GetForEdit( context.Profile.Id );
					break;
				case "TaskProfileOLD":
					if ( context.Parent.Type == typeof( Credential ) )
						data = CredentialServices.ConditionProfile_GetTask( context.Profile.Id );
					else
						data = ConditionProfileServices.TaskProfile_Get( context.Profile.Id );
					break;
				case "TaskProfile":
					data = ConditionProfileServices.TaskProfile_Get( context.Profile.Id );
					break;
				case "CostProfile":
					data = ProfileServices.CostProfile_Get( context.Profile.Id );
					break;
				case "CostItemProfile":
					data = ProfileServices.CostProfileItem_Get( context.Profile.Id );
					break;
				//case "TextValueProfile": //N-A
				//	break;
				case "CredentialAlignmentObjectProfile":
					data = ProfileServices.CredentialAlignmentObject_Get( context.Profile.Id );
					break;
				case "CredentialAlignmentObjectFrameworkProfile":
					data = ProfileServices.CredentialAlignmentObjectFrameworkProfile_Get( context.Profile.Id );
					break;
				case "CredentialAlignmentObjectItemProfile":
					data = ProfileServices.CredentialAlignmentObjectItemProfile_Get( context.Profile.Id );
					break;
				case "AddressProfile":
					data = ProfileServices.AddressProfile_Get( context.Profile.Id, context.Parent.TypeName );
					break;
				case "ProcessProfile":
					break;
				case "VerificationServiceProfile":
					data = ProfileServices.AuthenticationProfile_Get( context.Profile.Id );
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
							clientProperty.SetValue(clientProfile, list.Count() > 0 ? list.First() : 0 );
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

					else { }
				}
				catch (Exception ex) 
				{ 
					//apparantly ignoring - chg to trace to avoid confusion
					LoggingHelper.DoTrace( 6,  string.Format( "EditorServices.ConvertToClientProfile(), clientProperty: {0}  \\r\\n", clientProperty ) + ex.Message, false );
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
			//Otherwise, get existing profile
			var profile = ( BaseProfile ) Activator.CreateInstance( context.Profile.Type );
			object data = new { };

			//Get the user
			var user = AccountServices.GetUserFromSession();
			var id = 0;
			var rowID = new Guid();
			context.Profile.Id = 0;

			//TODO check for entity access 
			//How to return errors
			switch ( context.Profile.TypeName )
			{
				case "Credential":
					break;
				case "Organization":
					break;
				case "Assessment":
					break;
				case "LearningOpportunity":
					break;
				case "DurationProfile":
					break;
				case "JurisdictionProfile":
					break;
				/*
				 * case "AgentRoleProfile_Recipient":	//		N/A
					break;
				case "AgentRoleProfile_Actor":	//		N/A
					break;
				case "QualityAssuranceActionProfile_Recipient":	//		N/A
					break;
				case "QualityAssuranceActionProfile_Actor":	//		N/A
					break;
				 */
				case "ConditionProfileOLD":
					{
						var entity = new PM.ConditionProfile(){ProfileName = "*** new profile ***"};
						entity.ConnectionProfileType = context.Profile.Property;
						if ( context.Parent.Type == typeof( Credential ) )
						{
							if ( new CredentialServices().ConditionProfile_Save( entity, context.Parent.RowId, context.Profile.Property, "Initial", user, ref status, true ) )
							{
								//data = CredentialServices.ConditionProfile_GetForEdit( entity.Id );
								context.Profile.Id = entity.Id;
								context.Profile.RowId = entity.RowId;
								context.Profile.Name = entity.ProfileName;
							}
						}
						else
						{
							if ( new ConditionProfileServices().ConditionProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status ) )
							{
								//data = CredentialServices.ConditionProfile_GetForEdit( entity.Id );
								context.Profile.Id = entity.Id;
								context.Profile.RowId = entity.RowId;
								context.Profile.Name = entity.ProfileName;
							}
						}
						
						
						break;
					}
				case "ConditionProfile":
					{
						var entity = new PM.ConditionProfile() { ProfileName = "*** new profile ***" };
						entity.ConnectionProfileType = context.Profile.Property;
						
						if ( new ConditionProfileServices().ConditionProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status ) )
						{
							//data = CredentialServices.ConditionProfile_GetForEdit( entity.Id );
							context.Profile.Id = entity.Id;
							context.Profile.RowId = entity.RowId;
							context.Profile.Name = entity.ProfileName;
						}
						break;
					}
				case "RevocationProfile":
					{
						var entity = new PM.RevocationProfile() { ProfileName = "*** new profile ***" };
						//entity.ProfileName = "*** new profile ***";
						if ( new CredentialServices().RevocationProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status, true ) )
						{
							//data = CredentialServices.RevocationProfile_GetForEdit( entity.Id );
							context.Profile.Id = entity.Id;
							context.Profile.RowId = entity.RowId;
							context.Profile.Name = entity.ProfileName;
						}
						
						break;
					}
				case "TaskProfileOLD":
					{
						var entity = new PM.TaskProfile();
						entity.ProfileName = "*** new profile ***";
						if ( new CredentialServices().TaskProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status, true ))
						{
							context.Profile.Id = entity.Id;
							context.Profile.RowId = entity.RowId;
							context.Profile.Name = entity.ProfileName;

							//return GetProfile( context, ref valid, ref status );

							//data = CredentialServices.ConditionProfile_GetTask( entity.Id );
						}
						
						break;
					}
				case "TaskProfile":
					{
						var entity = new PM.TaskProfile();
						entity.ProfileName = "*** new profile ***";
						if ( new ConditionProfileServices().TaskProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status ) )
						{
							context.Profile.Id = entity.Id;
							context.Profile.RowId = entity.RowId;
							context.Profile.Name = entity.ProfileName;
						}

						break;
					}
				case "CostProfile":
					{
						//int newId = 0;
						var entity = new PM.CostProfile() { ProfileName = "*** new profile ***" };
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
				case "CredentialAlignmentObjectProfile":
					break;
				case "CredentialAlignmentObjectFrameworkProfile":
					{
						var entity = new MC.CredentialAlignmentObjectFrameworkProfile();
						entity.EducationalFrameworkName = "*** new profile ***";
						if ( new ProfileServices().CredentialAlignmentObjectFrameworkProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status ) )
						{
							context.Profile.Id = entity.Id;
							context.Profile.RowId = entity.RowId;
							context.Profile.Name = entity.EducationalFrameworkName;
						}

						break;
					}
				case "CredentialAlignmentObjectItemProfile":
					break;
				case "AddressProfile":
					break;
				case "ProcessProfile":
					break;
				case "AuthenticationProfile":
					{
						var entity = new PM.AuthenticationProfile();
						entity.ProfileName = "*** new profile ***";
						if ( new ProfileServices().AuthenticationProfile_Save( entity, context.Parent.RowId, "Initial", user, ref status, true ) )
						{
							context.Profile.Id = entity.Id;
							context.Profile.RowId = entity.RowId;
							context.Profile.Name = entity.ProfileName;
						}

						break;
					}
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
		/// <returns></returns>
		public static BaseProfile SaveProfile( ProfileContext context, BaseProfile clientProfile, ref bool valid, ref string status )
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
				return AddProfile( context, serverProfile, user, ref valid, ref status );
			}
			//If existing, call the Update method
			else
			{
				return UpdateProfile( context, serverProfile, user, ref valid, ref status );
			}
		}
		//

		//Add a new profile
		private static BaseProfile AddProfile( ProfileContext context, object serverProfile, Models.AppUser user, ref bool valid, ref string status, bool isEmpty = false )
		{
			var id = 0;
			var rowID = new Guid();
			switch ( context.Profile.TypeName )
			{
				case "Credential":
					{
						//EnrichData( serverProfile, CredentialServices.GetCredential( context.Profile.Id ) );
						var profile = ( MC.Credential ) serverProfile;
						profile.IsNewVersion = true;
						id = new CredentialServices().Credential_Add( profile, user, ref valid, ref status );
						rowID = profile.RowId;
						break;
					}
				case "Organization":
					{
						//EnrichData( serverProfile, OrganizationServices.GetOrganization( context.Profile.Id ) );
						var profile = ( MC.Organization ) serverProfile;
						profile.IsNewVersion = true;
						id = new OrganizationServices().Organization_Add( profile, user, ref valid, ref status );
						rowID = profile.RowId;
					}
					break;
				case "Assessment":
					{
						//EnrichData( serverProfile, AssessmentServices.Get( context.Profile.Id ) );
						var profile = ( PM.AssessmentProfile ) serverProfile;
						profile.IsNewVersion = true;
						id = new AssessmentServices().Add( profile, user, ref status );
						rowID = profile.RowId;
					}
					break;
				case "LearningOpportunity":
					{
						//EnrichData( serverProfile, LearningOpportunityServices.Get( context.Profile.Id ) );
						var profile = ( PM.LearningOpportunityProfile ) serverProfile;
						profile.IsNewVersion = true;
						id = new LearningOpportunityServices().Add( profile, user, ref status );
						rowID = profile.RowId;
					}
					break;
				case "DurationProfile":
					{
						var profile = ( PM.DurationProfile ) serverProfile;
						//if ( context.Parent.Type == typeof( Credential ) )
						//{
						//	//var originalData = CredentialServices.DurationProfile_GetTimeToEarn( context.Profile.Id ) ;
						//	//EnrichData( originalData, serverProfile );
						//	valid = new CredentialServices().DurationProfile_UpdateTimeToEarn( profile, context.Main.RowId, user.Id, ref status );
						//}
						//else
						//{
							//var originalData = CredentialServices.DurationProfile_Get( context.Profile.Id );
							//EnrichData( originalData, serverProfile );
							valid = new CredentialServices().DurationProfile_Update( profile, context.Parent.RowId, context.Main.RowId, user.Id, ref status );
						//}
							id = profile.Id;
							rowID = profile.RowId;
						}
					break;
				case "JurisdictionProfile":
					{
						//EnrichData( serverProfile, new JurisdictionServices().Get( context.Profile.RowId ) );
						var profile = ( MC.JurisdictionProfile ) serverProfile;
						if ( new Factories.RegionsManager().IsEmpty( profile ) )
						{
							profile.Description = "Auto-saved Jurisdiction";
						}
						valid = new JurisdictionServices().JurisdictionProfile_Add( profile, context.Parent.RowId, context.Profile.Property == "Residency" ? 2 : 1, user.Id, ref status );
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

						//might be able to use for all - where allowing multiple roles
						valid = new OrganizationServices().EntityAgentRole_Save( profile, profile.ParentUid, user, ref status );

						//Assign target
						//switch ( context.Parent.TypeName )
						//{
						//	case "Credential": profile.TargetCredentialId = context.Parent.Id; break;
						//	case "Organization": 
						//		profile.TargetOrganizationId = context.Parent.Id;
						//		profile.ParentUid = context.Parent.RowId;
						//		break;
						//	case "Assessment": profile.TargetAssessmentId = context.Parent.Id; break;
						//	//case "LearningOpportunity": profile.TargetLearningOpportunityId = context.Parent.Id; break; //Apparently does not exist?
						//	default: break;
						//}

						//new CredentialServices().Credential_SaveOrgRole( profile, profile.TargetCredentialId, user, ref status );
						//not applicable yet - a particular rowId is not partinent, as can be multiple roles
						//want to just get the whole enumerated object
						id = profile.ActingAgent.Id;
						rowID = profile.ActingAgent.RowId;

						//force the agent id into the context.Profile.Id for the GetProfile
						context.Profile.Id = profile.ActingAgent.Id;
						break;
					}
				case "AgentRoleProfile_Actor":
					{

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
				case "QualityAssuranceActionProfile_Actor":
					{

						break;
					}
				case "ConditionProfileOLD":
					{
						var profile = ( PM.ConditionProfile ) serverProfile;
						profile.ConnectionProfileType = context.Profile.Property;
						if ( context.Parent.Type == typeof( Credential ) )
							valid = new CredentialServices().ConditionProfile_Save( profile, context.Parent.RowId, context.Profile.Property, "Add", user, ref status );
						else
							valid = new ConditionProfileServices().ConditionProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );

						id = profile.Id;
						rowID = profile.RowId;
					}
					break;
				case "ConditionProfile":
					{
						var profile = ( PM.ConditionProfile ) serverProfile;
						profile.ConnectionProfileType = context.Profile.Property;
						valid = new ConditionProfileServices().ConditionProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );

						id = profile.Id;
						rowID = profile.RowId;
					}
					break;
				case "RevocationProfile":
					{
						var profile = ( PM.RevocationProfile ) serverProfile;
						valid = new CredentialServices().RevocationProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
						id = profile.Id;
						rowID = profile.RowId;
					}
					break;
				case "TaskProfileOLD":
					{
						var profile = ( PM.TaskProfile ) serverProfile;
						valid = new CredentialServices().TaskProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
						id = profile.Id;
						rowID = profile.RowId;
						break;
					}
				case "TaskProfile":
					{
						var profile = ( PM.TaskProfile ) serverProfile;
						valid = new ConditionProfileServices().TaskProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
						id = profile.Id;
						rowID = profile.RowId;
						break;
					}
				case "CostProfile":
					{
						var profile = ( PM.CostProfile ) serverProfile;
						bool isValid = new ProfileServices().CostProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
						id = profile.Id;
						rowID = profile.RowId;
					}
					break;
				case "CostItemProfile":
					{
						var profile = ( PM.CostProfileItem ) serverProfile;
						bool isValid = new ProfileServices().CostProfileItem_Save( profile, context.Parent.RowId, "Add", user, ref status );
						id = profile.Id;
						rowID = profile.RowId;
					}
					break;
				case "TextValueProfile": //		N/A
					{

						break;
					}
				case "CredentialAlignmentObjectProfile":
					{
						var profile = ( MC.CredentialAlignmentObjectProfile ) serverProfile;
						profile.AlignmentType = context.Profile.Property;
						bool isValid = new ProfileServices().CredentialAlignmentObject_Save( profile, context.Parent.RowId, "Add", user, ref status );
						id = profile.Id;
						rowID = profile.RowId;
					}
					break;
				case "CredentialAlignmentObjectFrameworkProfile":
					{
						var profile = ( MC.CredentialAlignmentObjectFrameworkProfile ) serverProfile;
						profile.ParentId = context.Parent.Id;
						profile.AlignmentType = context.Profile.Property;

						bool isValid = new ProfileServices().CredentialAlignmentObjectFrameworkProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
						id = profile.Id;
						rowID = profile.RowId;
					}
					break;
				case "CredentialAlignmentObjectItemProfile":
					{
						var profile = ( MC.CredentialAlignmentObjectItemProfile ) serverProfile;
						profile.ParentId = context.Parent.Id;
						bool isValid = new ProfileServices().CredentialAlignmentObjectItemProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
						id = profile.Id;
						rowID = profile.RowId;
					}
					break;
				case "AddressProfile":
					{
						//
						var profile = ( MC.Address ) serverProfile;
						bool isValid = new ProfileServices().AddressProfile_Save( profile, context.Parent.RowId, "Add", context.Parent.TypeName, user, ref status );
						id = profile.Id;
						rowID = profile.RowId;
					}
					break;
				case "ProcessProfile":
					{

						break;
					}
				case "VerificationServiceProfile":
					{
						var profile = ( PM.AuthenticationProfile ) serverProfile;
						valid = new ProfileServices().AuthenticationProfile_Save( profile, context.Parent.RowId, "Add", user, ref status );
						id = profile.Id;
						rowID = profile.RowId;
						break;
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
						var profile = (Models.Node.StarterProfile) serverProfile;
						context.Profile.TypeName = profile.ProfileType;
						
						//Call the add method as if it were a normal add
						var added = SaveProfile( context, (BaseProfile) profile, ref valid, ref status );

						//Make the association
						//Note that Profile.Property is available to help determine the proper method to call
						switch(profile.SearchType){
							case "CredentialSearch":

								break;
							case "OrganizationSearch":
								
								break;
							case "AssessmentSearchOLD":
								new CredentialServices().ConditionProfile_AddAsmt( context.Parent.Id, added.Id, user, ref valid, ref status );
								break;
							case "AssessmentSearch":
								new ConditionProfileServices().Assessment_Add( context.Parent.Id, added.Id, user, ref valid, ref status );
								break;
							case "LearningOpportunitySearchOLD":
								new CredentialServices().ConditionProfile_AddLearningOpportunity( context.Parent.Id, added.Id, user, ref valid, ref status );
								break;
							case "LearningOpportunitySearch":
								new ConditionProfileServices().LearningOpportunity_Add( context.Parent.Id, added.Id, user, ref valid, ref status );
								break;
							case "LearningOpportunityHasPartSearch":
								new LearningOpportunityServices().AddLearningOpportunity_AsPart( context.Parent.Id, added.Id, user, ref valid, ref status );
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

		//Update an existing profile
		private static BaseProfile UpdateProfile( ProfileContext context, object serverProfile, Models.AppUser user, ref bool valid, ref string status )
		{
			switch ( context.Profile.TypeName )
			{
				case "Credential":
					EnrichData( serverProfile, CredentialServices.GetBasicCredential( context.Profile.Id ) );
					valid = new CredentialServices().Credential_Save( ( MC.Credential ) serverProfile, user, ref status );
					break;
				case "Organization":
					EnrichData( serverProfile, OrganizationServices.GetOrganization( context.Profile.Id ) );
					var org = ( MC.Organization ) serverProfile;
					org.IsNewVersion = true;
					valid = new OrganizationServices().Organization_Update( org, user, ref status );
					break;
				case "Assessment":
					EnrichData( serverProfile, AssessmentServices.Get( context.Profile.Id ) );
					var asmt = ( PM.AssessmentProfile ) serverProfile;
					asmt.IsNewVersion = true;
					valid = new AssessmentServices().Update( ( PM.AssessmentProfile ) serverProfile, user, ref status );
					break;
				case "LearningOpportunity":
					EnrichData( serverProfile, LearningOpportunityServices.Get( context.Profile.Id ) );
					var lopp = ( PM.LearningOpportunityProfile ) serverProfile;
					lopp.IsNewVersion = true;
					valid = new LearningOpportunityServices().Update( ( PM.LearningOpportunityProfile ) serverProfile, "", user, ref status );
					break;
				case "DurationProfile":
					//if ( context.Parent.Type == typeof( Credential ) )
					//{
					//	var originalData = CredentialServices.DurationProfile_GetTimeToEarn( context.Profile.Id ) ;
					//	EnrichData( originalData, serverProfile );
					//	valid = new CredentialServices().DurationProfile_UpdateTimeToEarn( ( PM.DurationProfile ) serverProfile, context.Main.RowId, user.Id, ref status );
					//}
					//else
					//{
						var originalData = CredentialServices.DurationProfile_Get( context.Profile.Id );
						EnrichData( originalData, serverProfile );
						valid = new CredentialServices().DurationProfile_Update( ( PM.DurationProfile ) serverProfile, context.Parent.RowId, context.Main.RowId, user.Id, ref status );
					//}
					
					break;
				case "JurisdictionProfile":
					var js = new JurisdictionServices();
					EnrichData( serverProfile, js.Get( context.Profile.Id ) );
					valid = js.JurisdictionProfile_Update( ( MC.JurisdictionProfile ) serverProfile, context.Parent.RowId, AccountServices.GetUserFromSession().Id, ref status );
					break;
				case "AgentRoleProfile_Recipient":
					{
						var profile = ( PM.OrganizationRoleProfile ) serverProfile;
						//Assign acting agent fields
						profile.ActingAgentId = profile.ActingAgent.Id;
						profile.ActingAgentUid = profile.ActingAgent.RowId;
						profile.ParentUid = context.Parent.RowId;

						//might be able to use for all - where allowing multiple roles
						valid = new OrganizationServices().EntityAgentRole_Save( profile, profile.ParentUid, user, ref status );

						
						//necessary????
						context.Profile.Id = profile.ActingAgent.Id;
						break;
					}
				case "AgentRoleProfile_Actor":
					break;
				case "QualityAssuranceActionProfile_Recipient":
					{
						var profile = ( PM.QualityAssuranceActionProfile ) serverProfile;

						//Assign acting agent
						profile.ParentUid = context.Parent.RowId;
						profile.ActingAgentId = profile.ActingAgent.Id;
						profile.ActingAgentUid = profile.ActingAgent.RowId;
						profile.IssuedCredentialId = profile.IssuedCredential.Id;

						valid = new OrganizationServices().QualityAssuranceAction_SaveProfile( profile, profile.ParentUid, user, ref status );

						//valid = new CredentialServices().Credential_SaveQAOrgRole( profile, profile.TargetCredentialId, user, ref status );

						//force the agent id into the context.Profile.Id for the GetProfile
						//context.Profile.Id = profile.ActingAgent.Id;
						break;
					}
				case "QualityAssuranceActionProfile_Actor":
					break;
				case "ConditionProfileOLD":
					{
						var profile = ( PM.ConditionProfile ) serverProfile;
						profile.ConnectionProfileType = context.Profile.Property;
						if ( context.Parent.Type == typeof( Credential ) )
							valid = new CredentialServices().ConditionProfile_Save( profile, context.Parent.RowId, context.Profile.Property, "Modify", user, ref status );
						else
							valid = new ConditionProfileServices().ConditionProfile_Save( profile, context.Parent.RowId, "Modify", user, ref status );
					}
					break;
				case "ConditionProfile":
					{
						var profile = ( PM.ConditionProfile ) serverProfile;
						profile.ConnectionProfileType = context.Profile.Property;
						valid = new ConditionProfileServices().ConditionProfile_Save( profile, context.Parent.RowId, "Modify", user, ref status );
					}
					break;
				case "RevocationProfile":
					{
						var profile = ( PM.RevocationProfile ) serverProfile;
						valid = new CredentialServices().RevocationProfile_Save( profile, context.Parent.RowId, "Modify", user, ref status );
					}
					break;
				case "TaskProfileOLD":
					{
						var profile = ( PM.TaskProfile ) serverProfile;
						valid = new CredentialServices().TaskProfile_Save( profile, context.Parent.RowId, "Modify", user, ref status );
					}
					break;
				case "TaskProfile":
					{
						var profile = ( PM.TaskProfile ) serverProfile;
						valid = new ConditionProfileServices().TaskProfile_Save( profile, context.Parent.RowId, "Modify", user, ref status );
					}
					break;
				case "CostProfile":
					{
						var profile = ( PM.CostProfile ) serverProfile;
						valid = new ProfileServices().CostProfile_Save( profile, context.Parent.RowId, "Modify", user, ref status );
					}
					break;
				case "CostItemProfile":
					{
						var profile = ( PM.CostProfileItem ) serverProfile;
						valid = new ProfileServices().CostProfileItem_Save( profile, context.Parent.RowId, "Modify", user, ref status );
					}
					break;
				case "CredentialAlignmentObjectProfile":
					{
						var profile = ( MC.CredentialAlignmentObjectProfile ) serverProfile;
						profile.AlignmentType = context.Profile.Property;
						valid = new ProfileServices().CredentialAlignmentObject_Save( profile, context.Parent.RowId, "Modify", user, ref status );
					}
					break;
				case "CredentialAlignmentObjectFrameworkProfile":
					{
						var profile = ( MC.CredentialAlignmentObjectFrameworkProfile ) serverProfile;
						profile.AlignmentType = context.Profile.Property;
						valid = new ProfileServices().CredentialAlignmentObjectFrameworkProfile_Save( profile, context.Parent.RowId, "Modify", user, ref status );
					}
					break;
				case "CredentialAlignmentObjectItemProfile":
					{
						var profile = ( MC.CredentialAlignmentObjectItemProfile ) serverProfile;
						profile.ParentId = context.Parent.Id;
						valid = new ProfileServices().CredentialAlignmentObjectItemProfile_Save( profile, context.Parent.RowId, "Modify", user, ref status );
					}
					break;
				case "AddressProfile":
					{
						var profile = ( MC.Address ) serverProfile;
						valid = new ProfileServices().AddressProfile_Save( profile, context.Parent.RowId, "Modify", context.Parent.TypeName, user, ref status );
					}
					break;
				case "ProcessProfile":
					break;
				case "VerificationServiceProfile":
					{
						var profile = ( PM.AuthenticationProfile ) serverProfile;
						valid = new ProfileServices().AuthenticationProfile_Save( profile, context.Parent.RowId, "Modify", user, ref status );
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
				return GetProfile( context, true, ref valid, ref status );
			}
			return null;
		}
		//

		//Convert a client profile to a server profile
		private static void ConvertToServerProfile( object serverProfile, object clientProfile )
		{
			var clientProperties = clientProfile.GetType().GetProperties();
			foreach ( var clientProperty in clientProperties )
			{
				try
				{
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

				try { newProps.FirstOrDefault( m => m.Name == "Id" ).SetValue( newItem, value.Id ); }	catch { }
				try { newProps.FirstOrDefault( m => m.Name == "RowId" ).SetValue( newItem, value.RowId ); }	catch { }

				serverProperty.SetValue( serverProfile, newItem );
			}
		}
		//

		//Add missing database-related data that isn't carried in by the client object
		private static void EnrichData(object serverProfile, object source){
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
					valid = new CredentialServices().Credential_Delete( context.Profile.Id, user, ref status );
					break;
				case "Organization":
					//called method checks for authorization
					valid = new OrganizationServices().Organization_Delete( context.Profile.Id, user, ref status );
					break;
				case "Assessment":
					valid = new AssessmentServices().Delete( context.Profile.Id, user.Id, ref status );
					break;
				case "LearningOpportunity":
					valid = new LearningOpportunityServices().Delete( context.Profile.Id, user.Id, ref status );
					break;
				case "DurationProfile":
					//valid = context.Parent.Type == typeof( Credential ) ?
					//	new CredentialServices().DurationProfile_DeleteTimeToEarn( context.Profile.Id, ref status ) :
						valid = new CredentialServices().DurationProfile_Delete( context.Profile.Id, ref status );
					break;
				case "JurisdictionProfile":
					valid = new JurisdictionServices().JurisdictionProfile_Delete( context.Profile.Id, ref status );
					break;
				case "AgentRoleProfile_Recipient":
					//valid = new CredentialServices().Credential_DeleteOrgRoles( context.Main.Id, context.Profile.RowId, user, ref status );

					valid = new OrganizationServices().Delete_EntityAgentRoles( context.Main.RowId, context.Profile.RowId, user, ref status );
					break;
				case "AgentRoleProfile_Actor":
					break;
				case "QualityAssuranceActionProfile_Recipient":
					//valid = new CredentialServices().Credential_DeleteQAOrgRoles( context.Main.Id, context.Profile.Id, user, ref status );

					valid = new OrganizationServices().QualityAssuranceAction_DeleteProfile( context.Main.Id, context.Profile.RowId, user, ref status );
					break;
				case "QualityAssuranceActionProfile_Actor":
					break;
				case "ConditionProfileOLD":
					if ( context.Parent.Type == typeof( Credential ) )
						valid = new CredentialServices().ConditionProfile_Delete( context.Main.Id, context.Profile.Id, user, ref status );
					else
						valid = new ConditionProfileServices().ConditionProfile_Delete( context.Main.RowId, context.Profile.Id, user, ref status );
					break;
				case "ConditionProfile":
					valid = new ConditionProfileServices().ConditionProfile_Delete( context.Main.RowId, context.Profile.Id, user, ref status );
					break;
				case "RevocationProfile":
					valid = new CredentialServices().RevocationProfile_Delete( context.Main.Id, context.Profile.Id, user, ref status );
					break;
				case "TaskProfileOLD":
					if ( context.Parent.Type == typeof( Credential ) )
						valid = new CredentialServices().ConditionProfile_DeleteTask( context.Main.Id, context.Profile.Id, user, ref status );
					else
						valid = new ConditionProfileServices().TaskProfile_Delete( context.Main.RowId, context.Profile.Id, user, ref status );
					break;
				case "TaskProfile":
					valid = new ConditionProfileServices().TaskProfile_Delete( context.Main.RowId, context.Profile.Id, user, ref status );
					break;
				case "CostProfile":
					valid = new ProfileServices().CostProfile_Delete( context.Profile.Id, user, ref status );
					break;
				case "CostItemProfile":
					valid = new ProfileServices().CostProfileItem_Delete( context.Profile.Id, user, ref status );
					break;
				case "TextValueProfile":
					valid = new ProfileServices().Entity_Reference_Delete( context.Parent.Id, context.Profile.Id, user, ref status );
					break;
				case "CredentialAlignmentObjectProfile":
					valid = new ProfileServices().CredentialAlignmentObject_Delete( context.Main.Id, context.Profile.Id, user, ref status );
					break;
				case "CredentialAlignmentObjectFrameworkProfile":
					valid = new ProfileServices().CredentialAlignmentObjectFrameworkProfile_Delete( context.Main.Id, context.Profile.Id, user, ref status );
					break;
				case "CredentialAlignmentObjectItemProfile":
					valid = new ProfileServices().CredentialAlignmentObjectItemProfile_Delete( context.Main.Id, context.Profile.Id, user, ref status );
					break;
				case "AddressProfile":
					valid = new ProfileServices().AddressProfile_Delete( context.Profile.Id, context.Parent.TypeName, user, ref status );
					break;
				case "ProcessProfile":
					break;
				case "VerificationServiceProfile":
					valid = new ProfileServices().AuthenticationProfile_Delete( context.Main.Id, context.Profile.Id, user, ref status );
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

			//Do the delete
			switch ( context.Profile.TypeName )
			{
				case "Credential":
					valid = new RegistryServices().MetadataRegistry_PublishCredential( context.Profile.Id, user, ref status, ref list );
					break;
				case "Organization":
					//called method checks for authorization
					valid = new RegistryServices().MetadataRegistry_PublishOrganization( context.Profile.Id, user, ref status, ref list );
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
					valid = new RegistryServices().UnregisterCredential( context.Profile.Id, user, ref status, ref list );
					break;
				case "Organization":
					//called method checks for authorization
					valid = new RegistryServices().UnregisterOrganization( context.Profile.Id, user, ref status, ref list );
					break;
				default:
					valid = false;
					status = "Profile not handled";
					break;
			}
		}
		#endregion 
	}
	//
}
