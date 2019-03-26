using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Models.Common;

namespace CTI.Directory.Models.EditorModels
{
	public class EditorListBase
	{
		public string MainLabel { get; set; }
		public Type MainClassType { get; set; }
		public List<EditorForm> EditorForms { get; set; }
		public void InjectConcepts( string propertyName, List<Concept> items )
		{
			foreach( var form in EditorForms )
			{
				form.InjectConcepts( propertyName, items );
			}
		}
	}
	//

	public class EditorForm
	{
		public EditorForm( string name, string label, List<EditorProperty> properties = null, Dictionary<string, List<Concept>> conceptsToInject = null )
		{
			EditorName = name;
			EditorLabel = label;
			Properties = properties ?? new List<EditorProperty>();
			InjectConcepts( conceptsToInject );
		}
		public string EditorName { get; set; }
		public string EditorLabel { get; set; }
		public List<EditorProperty> Properties { get; set; }
		public void InjectConcepts( Dictionary<string, List<Concept>> conceptsToInject )
		{
			if ( conceptsToInject != null )
			{
				foreach ( var item in conceptsToInject )
				{
					InjectConcepts( item.Key, item.Value );
				}
			}
		}
		public void InjectConcepts( string propertyName, List<Concept> items )
		{
			foreach ( var prop in Properties )
			{
				if ( prop.GetType() == typeof( EditorPropertyGroup ) )
				{
					foreach ( var member in (( EditorPropertyGroup ) prop).Properties )
					{
						TryInjectConcepts( member, propertyName, items );
					}
				}
				else
				{
					TryInjectConcepts( prop, propertyName, items );
				}
			}
		}
		public static void TryInjectConcepts( EditorProperty prop, string propertyName, List<Concept> items )
		{
			if ( prop.PropertyName == propertyName && prop.GetType() == typeof( ItemListProperty ) )
			{
				(( ItemListProperty ) prop).Items = items;
			}
		}

	}
	//

	public class EditorProperty
	{
		public EditorProperty() { }
		public EditorProperty( string name, string label, string dataType = "text", string interfaceType = "text", string helpText = "" )
		{
			PropertyName = name;
			PropertyLabel = label;
			DataType = dataType;
			InterfaceType = interfaceType;
			HelpText = helpText;
		}
		public string PropertyName { get; set; }
		public string PropertyLabel { get; set; }
		public string DataType { get; set; }
		public string InterfaceType { get; set; }
		public string HelpText { get; set; }
	}
	//

	public class ItemListProperty : EditorProperty
	{
		public ItemListProperty() { }
		public ItemListProperty( string name, string label, string dataType = "number", string interfaceType = "checkBoxList", List<Concept> items = null, int columnCount = 0, string helpText = "" ) : base( name, label, dataType, interfaceType, helpText )
		{
			Items = items ?? new List<Concept>();
			ColumnCount = columnCount;
		}
		public List<Concept> Items { get; set; }
		public int ColumnCount { get; set; }
	}
	public class Concept
	{
		public string Label { get; set; }
		public string Value { get; set; }
		public string Schema { get; set; }
		public string Group { get; set; }
	}
	//

	public class SubEditorContainerProperty : EditorProperty
	{
		public SubEditorContainerProperty( Type subEditorType, string name, string label, bool isInline = false, bool isMultiValue = true, string helpText = "" ) : base( name, label, subEditorType.Name, "subEditorContainer", helpText )
		{
			SubEditorType = subEditorType;
			IsInline = isInline;
			IsMultiValue = isMultiValue;
		}
		public Type SubEditorType { get; set; }
		public bool IsInline { get; set; }
		public bool IsMultiValue { get; set; }
	}
	//

	public class ReferenceProperty : EditorProperty
	{
		public ReferenceProperty( string referenceType, string name, string label, string helpText = "" ) : base( name, label, referenceType, "reference", helpText )
		{

		}
	}
	//

	public class EditorPropertyGroup : EditorProperty
	{
		public EditorPropertyGroup( string name, string label, List<EditorProperty> properties )
		{
			PropertyName = name;
			PropertyLabel = label;
			Properties = properties;
		}
		public List<EditorProperty> Properties { get; set; }
	}
	//

	public class CredentialEditorList : EditorListBase
	{
		public CredentialEditorList()
		{
			MainLabel = "Credential Editor";
			MainClassType = typeof( Credential );
			EditorForms = new List<EditorForm>()
			{
				new EditorForm( "basicinfo", "Basic Information", new List<EditorProperty>() {
					new EditorProperty( "Name", "Credential Name" ),
					new EditorProperty( "AlternateName", "Alternate Name" ),
					new EditorProperty( "Description", "Credential Description", "text", "textarea" ),
					new EditorProperty( "OwningOrganization", "Owning Organization", "organizationRole", "organizationRole" ),
					new ItemListProperty( "CredentialType", "Credential Type", "number", "radioButtonList", null, 2 ),
					new EditorProperty( "SubjectWebpage", "Subject Webpage", "url" ),
					new ItemListProperty( "CredentialStatusType", "Credential Status", "number", "radioButtonList" ),
					new EditorProperty( "ImageUrl", "Credential Image", "url" ),
					new ItemListProperty( "InLanguageId", "Language", "number", "select" ),
				} ),
				new EditorForm("requirements", "Conditions and Requirements", new List<EditorProperty>()
				{
					new EditorPropertyGroup("RequiresGroup", "Requirements", new List<EditorProperty>()
					{
						new SubEditorContainerProperty( typeof( ConditionProfileEditor ), "Requires", "" ),
					} ),
					new EditorPropertyGroup("RecommendationsGroup", "Recommendations", new List<EditorProperty>()
					{
						new SubEditorContainerProperty( typeof( ConditionProfileEditor ), "Recommends", "" ),
					} ),
					new EditorPropertyGroup("CorequisiteGroup", "Corequisite Conditions", new List<EditorProperty>()
					{
						new SubEditorContainerProperty( typeof( ConditionProfileEditor ), "Corequisite", "" ),
					} ),
				} ),
				new EditorForm( "additionalinfo", "Additional Information", new List<EditorProperty>()
				{
					new EditorPropertyGroup("LocationGroup", "Location Information", new List<EditorProperty>()
					{
						new EditorProperty( "AvailableOnlineAt", "Available Online At", "url" ),
						new EditorProperty( "AvailabilityListing", "Availability Listing", "url" ),
						new SubEditorContainerProperty( typeof( AddressEditor ), "Addresses", "Available at Addresses" )
					} ),
					new EditorPropertyGroup( "VersionGroup", "Version Tracking", new List<EditorProperty>()
					{
						new EditorProperty( "VersionIdentifier", "Version Identifier" ),
						new EditorProperty( "LatestVersionUrl", "Latest Version URL", "url" ),
						new EditorProperty( "PreviousVersion", "Previous Version URL", "url" )
					} ),
					new SubEditorContainerProperty( typeof( TextValueEditor ), "Keyword", "Keywords", true )
				} )
			};
		}
	}
	//

	public class TextValueEditor : EditorForm
	{
		public TextValueEditor( string name, string label ) : base( name, label )
		{
			Properties = new List<EditorProperty>()
			{
				new EditorProperty( "Value", "" )
			};
		}
	}
	//

	public class ConditionProfileEditor : EditorForm
	{
		public ConditionProfileEditor( string name, string label, Dictionary<string, List<Concept>> conceptsToInject = null ) : base( name, label )
		{
			Properties = new List<EditorProperty>()
			{
				new EditorProperty( "Name", "Conditions Name" ),
				new EditorProperty( "Description", "Conditions Description", "text", "textarea" ),
				new ReferenceProperty( "Competency", "TargetCompetency", "Applicable Competencies" ),
				new ReferenceProperty( "AssessmentProfile", "TargetAssessment", "Applicable Assessments" ),
				new ReferenceProperty( "LearningOpportunityProfile", "TargetLearningOpportunity", "Applicable Learning Opportunities" ),
				new ReferenceProperty( "Credential", "TargetCredential", "Applicable Credentials" ),
				new EditorPropertyGroup("AudienceGroup", "Audience", new List<EditorProperty>() {
					new ItemListProperty( "AudienceLevelType", "Audience Level", "number", "checkBoxList", null, 2 ),
					new ItemListProperty( "AudienceType", "Audience Type", "number", "checkBoxList", null, 3 )
				} )
			};
			InjectConcepts( conceptsToInject );
		}
	}
	//

	public class AddressEditor : EditorForm
	{
		public AddressEditor( string name, string label ) : base( name, label )
		{
			Properties = new List<EditorProperty>()
			{
				new EditorProperty( "Name", "Address Name" ),
				new EditorProperty( "Address1", "Street Address Line 1" ),
				new EditorProperty( "Address2", "Street Address Line 2" ),
				new EditorProperty( "PostOfficeBoxNumber", "Post Office Box Number" ),
				new EditorProperty( "City", "City" ),
				new EditorProperty( "Region", "State, Province, or Region" ),
				new EditorProperty( "PostalCode", "Postal Code" ),
				new EditorProperty( "Country", "Country" ),
				new SubEditorContainerProperty( typeof( ContactPointEditor ), "ContactPoint", "Contact Points" )
			};
		}
	}
	//

	public class ContactPointEditor : EditorForm
	{
		public ContactPointEditor( string name, string label ) : base(name, label )
		{
			Properties = new List<EditorProperty>()
			{
				new EditorProperty( "Name", "Contact Name" ),
				new EditorProperty( "ContactType", "Contact Type" ),
				
				new SubEditorContainerProperty( typeof(TextValueEditor), "PhoneNumbers", "Organization Phone/Fax Numbers", true ),
				new SubEditorContainerProperty( typeof(TextValueEditor), "Emails", "Organization Email Addresses", true ),
				new SubEditorContainerProperty( typeof(TextValueEditor), "SocialMediaPages", "Organization Social Media Pages", true ),
			};
		}
        //new EditorProperty( "ContactOption", "Contact Option" ),
    }
    //
}