using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Reflection;

namespace Models.Node
{
	public class Experimental
	{
		public class Conversion
		{
			public object GetValue( string propertyName, object source )
			{
				try
				{
					return source.GetType().GetProperties().FirstOrDefault( m => m.Name == propertyName ).GetValue( source );
				}
				catch 
				{
					return null;
				}
			}
			//
			public void SetValue( string propertyName, object source, object value )
			{
				try
				{
					source.GetType().GetProperties().FirstOrDefault( m => m.Name == propertyName ).SetValue( source, value );
				}
				catch { }
			}
			//

			public enum MappingRules { AUTO, DIRECT, ROLE, PROPERTY, PROFILE }

			public class PropertyMap
			{
				public PropertyMap() { }
				public PropertyMap( string name ) : this( name, name, name, MappingRules.AUTO ) { }
				public PropertyMap( string name, string target, string schema, MappingRules rules )
				{
					PropertyName = name;
					MappedPropertyName = target;
					SchemaName = schema;
					Rules = rules;
				}
				//

				public string PropertyName { get; set; }
				public string MappedPropertyName { get; set; }
				public string SchemaName { get; set; } //Do not include prefix (e.g., "ceterms:") for best results
				public MappingRules Rules { get; set; }
			}
			//

			public class Map
			{
				public Map()
				{
					Maps = new List<PropertyMap>();
				}
				public List<PropertyMap> Maps { get; set; }
			}
			//

			public class DatabaseMaps
			{
				public class Credential : Map
				{
					public Credential()
					{
						Maps = new List<PropertyMap>() {
							new PropertyMap( "OrganizationRole", "Roles", "", MappingRules.PROFILE )
						};
					}
				}
				//

			}

			public class RegistryMaps
			{
				public class Credential : Map
				{
					public Credential()
					{
						Maps = new List<PropertyMap>(){
							new PropertyMap( "creator", "Roles", "creator", MappingRules.PROPERTY ),
							new PropertyMap( "owner", "Roles", "owner", MappingRules.PROPERTY ),
							new PropertyMap( "updatedVersionBy", "Roles", "updatedVersionBy", MappingRules.PROPERTY ),
							new PropertyMap( "verifiedBy", "Roles", "verifiedBy", MappingRules.PROPERTY ),
							new PropertyMap( "assessedBy", "Roles", "assessedBy", MappingRules.PROPERTY ),
							new PropertyMap( "offeredBy", "Roles", "offeredBy", MappingRules.PROPERTY ),
							new PropertyMap( "accreditedBy", "Roles", "accreditedBy", MappingRules.PROPERTY ),
							new PropertyMap( "approvedBy", "Roles", "approvedBy", MappingRules.PROPERTY ),
							new PropertyMap( "conferredBy", "Roles", "conferredBy", MappingRules.PROPERTY ),
							new PropertyMap( "endorsedBy", "Roles", "endorsedBy", MappingRules.PROPERTY ),
							new PropertyMap( "recognizedBy", "Roles", "recognizedBy", MappingRules.PROPERTY ),
							new PropertyMap( "regulatedBy", "Roles", "regulatedBy", MappingRules.PROPERTY ),
							new PropertyMap( "revocationBy", "Roles", "revocationBy", MappingRules.PROPERTY ),
							new PropertyMap( "renewalBy", "Roles", "renewalBy", MappingRules.PROPERTY ),
							new PropertyMap( "validatedBy", "Roles", "validatedBy", MappingRules.PROPERTY ),
							new PropertyMap( "contributor", "Roles", "contributor", MappingRules.PROPERTY ),
						};
					}
				}
				//

			}

			public static void Convert( object input, object output, Map inputMap, Map outputMap, ref List<string> diagnostics )
			{
				var outputProperties = output.GetType().GetProperties();
				var inputProperties = input.GetType().GetProperties();
				foreach ( var currentProperty in outputProperties )
				{
					try
					{
						//Find the matching input property
						//Output Data <--> Output <--> Output Map <--> Input Map <--> Input Property <--> Input Data
						if ( currentProperty.Name == "owner" )
						{

						}
						var outputMapItem = outputMap.Maps.FirstOrDefault( m => m.PropertyName.ToLower() == currentProperty.Name.ToLower() ) ?? new PropertyMap( currentProperty.Name );
						var inputMapItem = inputMap.Maps.FirstOrDefault( m => m.MappedPropertyName.ToLower() == outputMapItem.MappedPropertyName.ToLower() ) ?? new PropertyMap( outputMapItem.MappedPropertyName );
						var inputProperty = input.GetType().GetProperties().FirstOrDefault( m => m.Name.ToLower() == inputMapItem.PropertyName.ToLower() );
						var inputData = inputProperty == null ? null : inputProperty.GetValue( input );
						if ( inputData == null )
						{
							diagnostics.Add( "Unable to find match for " + currentProperty.Name );
							continue;
						}

						//Direct conversion
						if ( 
							currentProperty.PropertyType == inputProperty.PropertyType || 
							( outputMapItem.Rules == MappingRules.DIRECT && inputMapItem.Rules == MappingRules.DIRECT ) 
						)
						{
							diagnostics.Add( "Setting value directly for property " + currentProperty.Name );
							currentProperty.SetValue( output, inputData );
						}

						//Role transformation into properties
						else if ( 
							( outputMapItem.Rules == MappingRules.PROPERTY && inputMapItem.Rules == MappingRules.ROLE ) || 
							( outputMapItem.Rules == MappingRules.AUTO && inputMapItem.Rules == MappingRules.AUTO && currentProperty.PropertyType == typeof( List<string> ) && inputProperty.PropertyType == typeof( List<Models.ProfileModels.OrganizationRoleProfile> ) ) 
						)
						{
							diagnostics.Add( "Converting a list of roles to a property, filtered by property name: Attempting to find '" + outputMapItem.SchemaName + "'. " );
							var matches = ( inputData as List<Models.ProfileModels.OrganizationRoleProfile> )
								.Where( m => m.AgentRole.Items
									.Where(n => n.SchemaName.ToLower().Contains( outputMapItem.SchemaName.ToLower() ) ).Count() > 0 )
								.ToList();
							diagnostics.Add( "Found " + matches.Count() + " matches. Setting values..." );
							//Set the value - may be better to append?
							if ( currentProperty.PropertyType == typeof( List<string> ) )
							{
								currentProperty.SetValue( output, matches.Select( m => m.ActingAgent.SubjectWebpage ).ToList() );
							}
							else if ( currentProperty.PropertyType == typeof( List<Models.Common.Organization> ) )
							{
								currentProperty.SetValue( output, matches.Select( m => m.ActingAgent ).ToList() );
							}
						}
						//TODO: property into role
						//TODO: profile to profile
						//TODO: multiple profiles into single profile list
						//TODO: test/proof of concept before going too far
						else if ( outputMapItem.Rules == MappingRules.AUTO && inputMapItem.Rules == MappingRules.AUTO )
						{

						}
					}
					catch ( Exception ex )
					{
						diagnostics.Add( "Error: " + ex.Message );
					}
				}
			}
			//

			public System.Reflection.PropertyInfo FindMappedInputProperty( System.Reflection.PropertyInfo property, object inputData, Map inputMap, Map outputMap )
			{
				//First determine whether the output object has been mapped to another property name
				var matchedOutputItem = outputMap.Maps.FirstOrDefault( m => m.PropertyName == property.Name );
				var outputTarget = matchedOutputItem == null ? property.Name : matchedOutputItem.MappedPropertyName;

				//Next determine whether the input object has been mapped to another property name
				var matchedInputItem = inputMap.Maps.FirstOrDefault( m => m.MappedPropertyName == outputTarget );
				var inputTarget = matchedInputItem == null ? outputTarget : matchedInputItem.PropertyName;

				//Attempt to return the data
				try
				{
					return inputData.GetType().GetProperties().FirstOrDefault( m => m.Name == inputTarget );
				}
				catch
				{
					return null;
				}
			}
			//

		}

		public class Normalization
		{
			public enum PropertyUsage { AUTO, ROLE, ROLE_LIST, PROFILE }
			public class NormalizedProperty
			{
				public NormalizedProperty() : this( null, null, PropertyUsage.AUTO ) { }
				public NormalizedProperty( string name, PropertyInfo property, PropertyUsage usage )
				{
					PropertyName = name;
					Property = property;
					Usage = usage;
				}
				public PropertyUsage Usage { get; set; }
				public PropertyInfo Property { get; set; }
				public string PropertyName { get; set; }
			}

			public class Map
			{
				public Map()
				{
					AssociatedType = typeof( string );
					Mapping = new List<NormalizedProperty>();
				}
				public Type AssociatedType { get; set; }
				public List<NormalizedProperty> Mapping { get; set; }
			}

			public static void Normalize( object input, Map map )
			{
				foreach ( var property in input.GetType().GetProperties() )
				{
					//Find the property in the map, or create a new one
					var mapItem = map.Mapping.FirstOrDefault( m => m.PropertyName.ToLower() == property.Name.ToLower() );
					if( mapItem == null )
					{
						map.Mapping.Add( new NormalizedProperty( property.Name, property, PropertyUsage.AUTO ) );
					}
					else
					{
						mapItem.Property = property;
					}
				}
			}

			public static void Normalize( object input, object output, Map inputMap, Map outputMap )
			{
				//Normalize the maps
				Normalize( input, inputMap );
				Normalize( output, outputMap );
			}

			public class DatabaseMaps
			{
				public class Credential : Map
				{
					public Credential()
					{
						AssociatedType = typeof( Models.Common.Credential );
						Mapping = new List<NormalizedProperty>()
						{
							new NormalizedProperty( "OrganizationRole", null, PropertyUsage.ROLE_LIST )
						};
					}
				}

				public void Convert( object input, object output, Map inputMap, Map outputMap )
				{
					Normalize( input, output, inputMap, outputMap );
					foreach ( var property in output.GetType().GetProperties() )
					{
						
					}
				}
			}

			public class CredentialMaps
			{
				public class Credential : Map
				{
					public Credential()
					{
						var roles = new List<string>() { "creator", "owner", "updatedVersionBy", "verifiedBy", "assessedBy", "offeredBy", "accreditedBy", "approvedBy", "conferredBy", "endoresedBy", "recognizedBy", "regulatedBy", "revocationBy", "renewalBy", "validatedBy", "contributor" };
						foreach ( var item in roles )
						{
							Mapping.Add( new NormalizedProperty( item, null, PropertyUsage.ROLE ) );
						}
					}
				}
			}

		}

	}
}

namespace Models.Normalized
{
	public class BaseProfile
	{
		public string Name { get; set; }
		public string Description { get; set; }
	}
	public class MainProfile : BaseProfile
	{
		public string Url { get; set; }
		public List<Role> OrganizationRole { get; set; }
	}
	public class Credential : MainProfile
	{

	}
	public class Organization : MainProfile
	{

	}
	public class Role {

	}
}

namespace Models.Neutral
{
	public enum ProfileType
	{
		CREDENTIAL, ORGANIZATION, ASSESSMENT, LEARNING_OPPORTUNITY, ROLE, PROFILE
	}
	public enum PropertyType
	{
		BASIC, ROLE_ITEM, ROLE_LIST, REFERENCE, PROFILE
	}
	public class Profile
	{
		public Profile()
		{

		}
		public Profile( object input, ProfileType type, Dictionary<string, PropertyType> map )
		{
			Type = type;
			foreach ( var property in input.GetType().GetProperties() ) 
			{
				try 
				{
					switch( map[ property.Name ] )
					{
						case PropertyType.BASIC: 
							{
								Basics.Add( property.Name, property.GetValue( input ) );
								break;
							} 
						case PropertyType.ROLE_ITEM:
							{
								//Construct a new role profile
								Roles.Add( new RoleProfile()
								{
									Agent = new Profile()
									{
										Basics = new Dictionary<string, object>()
										{
											{ "Url", property.GetValue( input ) }
										}
									},
									Role = new ReferenceItem()
									{
										SchemaName = property.Name
									}
								} );
								break;
							}
						case PropertyType.ROLE_LIST :
							{
								foreach(var item in ( property.GetValue( input ) as dynamic ).Cast<List<Models.ProfileModels.OrganizationRoleProfile>>() ) 
								{
									Roles.Add( new RoleProfile()
									{
										Agent = new Profile( item.ActingAgent, ProfileType.ORGANIZATION, null ),
										Role = new ReferenceItem() { CodeTableId = item.CodeId }
									} );
								}
								break;
							}
						default: break;
					}
				}
				catch 
				{
					Basics.Add( property.Name, property.GetValue( input ) );
				}
			}
		}
		public ProfileType Type { get; set; }
		public Dictionary<string, object> Basics { get; set; }
		public List<RoleProfile> Roles { get; set; }
		public List<Profile> References { get; set; }
		public List<Profile> Profiles { get; set; }
	}
	public class RoleProfile
	{
		public Profile Agent { get; set; }
		public ReferenceItem Role { get; set; }
	}
	public class ReferenceItem
	{
		public int DatabaseId { get; set; }
		public int CodeTableId { get; set; }
		public string ExternalCodeId { get; set; }
		public string SchemaName { get; set; }
		public string Label { get; set; }
	}
}

